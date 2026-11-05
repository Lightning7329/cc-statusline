namespace StatusLine.Tests.Segments.PromptCacheTests

open System
open System.Drawing
open Xunit
open FsUnit.Xunit
open StatusLine.Types.App
open StatusLine.Types.Context
open StatusLine.Segments.PromptCache

module internal Fixture =
    // 1738422000 = 2025-02-01 15:00:00 UTC
    let testTimestamp = 1738422000L

    let localTime =
        DateTimeOffset.FromUnixTimeSeconds(testTimestamp).ToLocalTime().ToString "HH:mm"

    let warmCache: PromptCache = {
        Warm = Some true
        CachingObserved = Some true
        Ttl = Some "5m"
        ExpiresAt = Some testTimestamp
        Requests = Some 14
        Misses = Some 2
        ExpectedRebuilds = Some 1
        HitRatio = Some 0.92
        CacheWriteTokens = Some 352000L
        MissRecacheTokens = Some 310200L
        LastMissAt = Some 1738425230L
        LastMissCause = None
        MissCauses = None
        RecacheTokensIfCold = Some 45000L
    }

    let coldCache = { warmCache with Warm = Some false }

    let text (segment: Segment option) =
        segment.Value |> List.map _.Text |> String.concat ""

    let spanOf (fragment: string) (segment: Segment option) =
        segment.Value |> List.find (fun span -> span.Text.Contains fragment)

    let rgb (span: Span) =
        let color = span.Color.Value
        int color.R, int color.G, int color.B

module Format =
    open Fixture

    [<Fact>]
    let ``warmのときヒット率と期限の時刻を表示する`` () =
        format (Some warmCache)
        |> text
        |> should equal (sprintf "🔥 92%% warm until %s" localTime)

    [<Fact>]
    let ``coldのときヒット率と状態を表示する`` () =
        format (Some { coldCache with ExpiresAt = None })
        |> text
        |> should equal "🧊 92% cold"

    [<Fact>]
    let ``ExpiresAtが残っていてもcoldならuntilを表示しない`` () =
        // TTL 切れの cold では、過去の expires_at が届き続ける
        format (Some coldCache) |> text |> should equal "🧊 92% cold"

    [<Fact>]
    let ``HitRatioがNoneのとき百分率を省く`` () =
        format (Some { warmCache with HitRatio = None })
        |> text
        |> should equal (sprintf "🔥 warm until %s" localTime)

    [<Fact>]
    let ``ExpiresAtがNoneのときuntilを省く`` () =
        format (Some { warmCache with ExpiresAt = None })
        |> text
        |> should equal "🔥 92% warm"

    [<Fact>]
    let ``ヒット率は端数が0.5未満なら切り捨てる`` () =
        format (Some { coldCache with HitRatio = Some 0.004 })
        |> text
        |> should equal "🧊 0% cold"

    [<Fact>]
    let ``ヒット率の端数が0.5で切り上げ先が奇数でも切り上げる`` () =
        // 銀行丸めなら 12% になる
        format (Some { coldCache with HitRatio = Some 0.125 })
        |> text
        |> should equal "🧊 13% cold"

    [<Fact>]
    let ``ヒット率1のとき100%と表示する`` () =
        format (Some { coldCache with HitRatio = Some 1.0 })
        |> text
        |> should equal "🧊 100% cold"

    [<Fact>]
    let ``PromptCacheがNoneのとき非表示にする`` () = format None |> should equal None

    [<Fact>]
    let ``CachingObservedがfalseのとき非表示にする`` () =
        format (
            Some {
                warmCache with
                    CachingObserved = Some false
            }
        )
        |> should equal None

    [<Fact>]
    let ``CachingObservedがNoneのとき表示する`` () =
        format (
            Some {
                warmCache with
                    CachingObserved = None
            }
        )
        |> text
        |> should haveSubstring "warm"

    [<Fact>]
    let ``WarmがNoneのとき非表示にする`` () =
        format (Some { warmCache with Warm = None }) |> should equal None

    [<Fact>]
    let ``warmのときラベルは無色にする`` () =
        (format (Some warmCache) |> spanOf "🔥").Color |> should equal None

    [<Fact>]
    let ``warmでヒット率100%のとき明るいコーラルにする`` () =
        format (Some { warmCache with HitRatio = Some 1.0 })
        |> spanOf "warm"
        |> rgb
        |> should equal (240, 128, 128)

    [<Fact>]
    let ``warmでヒット率0%のとき暗いコーラルにする`` () =
        format (Some { warmCache with HitRatio = Some 0.0 })
        |> spanOf "warm"
        |> rgb
        |> should equal (120, 64, 64)

    [<Fact>]
    let ``warmでヒット率50%のとき中間のコーラルにする`` () =
        format (Some { warmCache with HitRatio = Some 0.5 })
        |> spanOf "warm"
        |> rgb
        |> should equal (180, 96, 96)

    [<Fact>]
    let ``warmのとき百分率と状態と期限を同じ色にする`` () =
        let segment = format (Some warmCache)

        (segment |> spanOf "%").Color |> should equal (segment |> spanOf "until").Color
        (segment |> spanOf "%").Color |> should equal (segment |> spanOf "warm").Color

    [<Fact>]
    let ``warmでHitRatioがNoneのとき明るいコーラルにする`` () =
        format (Some { warmCache with HitRatio = None })
        |> spanOf "warm"
        |> rgb
        |> should equal (240, 128, 128)

    [<Fact>]
    let ``coldのときラベルは無色にする`` () =
        (format (Some coldCache) |> spanOf "🧊").Color |> should equal None

    [<Fact>]
    let ``coldのとき百分率と状態を灰色にする`` () =
        let segment = format (Some coldCache)

        (segment |> spanOf "%").Color |> should equal (Some Color.Gray)
        (segment |> spanOf "cold").Color |> should equal (Some Color.Gray)

    [<Fact>]
    let ``coldのときヒット率に応じて色を変えない`` () =
        let colorOf hitRatio =
            (format (
                Some {
                    coldCache with
                        HitRatio = Some hitRatio
                }
             )
             |> spanOf "cold")
                .Color

        colorOf 0.1 |> should equal (colorOf 0.9)
