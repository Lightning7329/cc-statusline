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

module Format =
    open Fixture

    [<Fact>]
    let ``warmのときヒット率と期限の時刻を表示する`` () =
        format (Some warmCache)
        |> text
        |> should equal (sprintf "cache 92%% warm until %s" localTime)

    [<Fact>]
    let ``coldのときヒット率と状態を表示する`` () =
        format (Some { coldCache with ExpiresAt = None })
        |> text
        |> should equal "cache 92% cold"

    [<Fact>]
    let ``ExpiresAtが残っていてもcoldならuntilを表示しない`` () =
        // TTL 切れの cold では、過去の expires_at が届き続ける
        format (Some coldCache) |> text |> should equal "cache 92% cold"

    [<Fact>]
    let ``HitRatioがNoneのとき百分率を省く`` () =
        format (Some { warmCache with HitRatio = None })
        |> text
        |> should equal (sprintf "cache warm until %s" localTime)

    [<Fact>]
    let ``ExpiresAtがNoneのときuntilを省く`` () =
        format (Some { warmCache with ExpiresAt = None })
        |> text
        |> should equal "cache 92% warm"

    [<Fact>]
    let ``ヒット率は端数が0.5未満なら切り捨てる`` () =
        format (Some { coldCache with HitRatio = Some 0.004 })
        |> text
        |> should equal "cache 0% cold"

    [<Fact>]
    let ``ヒット率の端数が0.5で切り上げ先が奇数でも切り上げる`` () =
        // 銀行丸めなら 12% になる
        format (Some { coldCache with HitRatio = Some 0.125 })
        |> text
        |> should equal "cache 13% cold"

    [<Fact>]
    let ``ヒット率1のとき100%と表示する`` () =
        format (Some { coldCache with HitRatio = Some 1.0 })
        |> text
        |> should equal "cache 100% cold"

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
    let ``ラベルは無色にする`` () =
        (format (Some warmCache) |> spanOf "cache").Color |> should equal None

    [<Fact>]
    let ``ヒット率100%のとき百分率を緑色にする`` () =
        let span = format (Some { warmCache with HitRatio = Some 1.0 }) |> spanOf "%"

        int span.Color.Value.R |> should equal 0
        int span.Color.Value.G |> should equal 255
        int span.Color.Value.B |> should equal 0

    [<Fact>]
    let ``ヒット率0%のとき百分率を赤色にする`` () =
        let span = format (Some { warmCache with HitRatio = Some 0.0 }) |> spanOf "%"

        int span.Color.Value.R |> should equal 255
        int span.Color.Value.G |> should equal 0
        int span.Color.Value.B |> should equal 0

    [<Fact>]
    let ``warmの文字はLightGreenにする`` () =
        (format (Some warmCache) |> spanOf "warm").Color
        |> should equal (Some Color.LightGreen)

    [<Fact>]
    let ``coldの文字はLightCoralにする`` () =
        (format (Some coldCache) |> spanOf "cold").Color
        |> should equal (Some Color.LightCoral)
