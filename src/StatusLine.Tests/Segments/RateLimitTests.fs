module StatusLine.Tests.Segments.RateLimitTests

open Xunit
open FsUnit.Xunit
open StatusLine.Types
open StatusLine.Segments.RateLimit

// 1738422000 = 2025-02-01 15:00:00 UTC = 2025-02-02 00:00:00 JST
let private testTimestamp = 1738422000L

[<Fact>]
let ``0%のとき緑色を返す`` () =
    let seg =
        formatEntry "HH:mm" {
            UsedPercentage = 0.0
            ResetsAt = testTimestamp
        }

    int seg.Color.Value.R |> should equal 0
    int seg.Color.Value.G |> should equal 255
    int seg.Color.Value.B |> should equal 0

[<Fact>]
let ``100%のとき赤色を返す`` () =
    let seg =
        formatEntry "HH:mm" {
            UsedPercentage = 100.0
            ResetsAt = testTimestamp
        }

    int seg.Color.Value.R |> should equal 255
    int seg.Color.Value.G |> should equal 0
    int seg.Color.Value.B |> should equal 0

[<Fact>]
let ``HH:mm形式でリセット時刻をフォーマットする`` () =
    let seg =
        formatEntry "HH:mm" {
            UsedPercentage = 50.0
            ResetsAt = testTimestamp
        }

    seg.Text |> should equal "50.0% (reset at 00:00)"

[<Fact>]
let ``MM/dd HH:mm形式でリセット時刻をフォーマットする`` () =
    let seg =
        formatEntry "MM/dd HH:mm" {
            UsedPercentage = 50.0
            ResetsAt = testTimestamp
        }

    seg.Text |> should equal "50.0% (reset at 02/02 00:00)"

[<Fact>]
let ``formatFiveHourはHH:mm形式を使う`` () =
    let seg =
        formatFiveHour (
            {
                UsedPercentage = 0.0
                ResetsAt = testTimestamp
            }
        )

    seg.Text |> should equal "0.0% (reset at 00:00)"

[<Fact>]
let ``formatSevenDayはMM/dd HH:mm形式を使う`` () =
    let seg =
        formatSevenDay (
            {
                UsedPercentage = 0.0
                ResetsAt = testTimestamp
            }
        )

    seg.Text |> should equal "0.0% (reset at 02/02 00:00)"
