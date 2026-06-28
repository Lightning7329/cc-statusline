module StatusLine.Tests.Segments.RateLimitTests

open System
open Xunit
open FsUnit.Xunit
open StatusLine.Types.Context
open StatusLine.Types.App
open StatusLine.Segments.RateLimit

// 1738422000 = 2025-02-01 15:00:00 UTC
let private testTimestamp = 1738422000L

let private localTimeString (format: string) =
    DateTimeOffset.FromUnixTimeSeconds(testTimestamp).ToLocalTime().ToString format

[<Fact>]
let ``0%のとき緑色を返す`` () =
    let span =
        formatEntry "5h " "HH:mm" {
            UsedPercentage = 0.0
            ResetsAt = testTimestamp
        }
        |> List.last

    int span.Color.Value.R |> should equal 0
    int span.Color.Value.G |> should equal 255
    int span.Color.Value.B |> should equal 0

[<Fact>]
let ``100%のとき赤色を返す`` () =
    let span =
        formatEntry "5h " "HH:mm" {
            UsedPercentage = 100.0
            ResetsAt = testTimestamp
        }
        |> List.last

    int span.Color.Value.R |> should equal 255
    int span.Color.Value.G |> should equal 0
    int span.Color.Value.B |> should equal 0

[<Fact>]
let ``先頭に無色のラベルを付ける`` () =
    let head =
        formatEntry "5h " "HH:mm" {
            UsedPercentage = 50.0
            ResetsAt = testTimestamp
        }
        |> List.head

    head.Text |> should equal "5h "
    head.Color |> should equal None

[<Fact>]
let ``HH:mm形式でリセット時刻をフォーマットする`` () =
    let span =
        formatEntry "5h " "HH:mm" {
            UsedPercentage = 50.0
            ResetsAt = testTimestamp
        }
        |> List.last

    span.Text
    |> should equal (sprintf "50.0%% (reset at %s)" (localTimeString "HH:mm"))

[<Fact>]
let ``MM/dd HH:mm形式でリセット時刻をフォーマットする`` () =
    let span =
        formatEntry "7d " "MM/dd HH:mm" {
            UsedPercentage = 50.0
            ResetsAt = testTimestamp
        }
        |> List.last

    span.Text
    |> should equal (sprintf "50.0%% (reset at %s)" (localTimeString "MM/dd HH:mm"))

[<Fact>]
let ``formatFiveHourは5hラベルとHH:mm形式を使う`` () =
    let seg =
        formatFiveHour {
            UsedPercentage = 0.0
            ResetsAt = testTimestamp
        }
        |> Option.get

    (seg |> List.head).Text |> should equal "5h "

    (seg |> List.last).Text
    |> should equal (sprintf "0.0%% (reset at %s)" (localTimeString "HH:mm"))

[<Fact>]
let ``formatSevenDayは7dラベルとMM/dd HH:mm形式を使う`` () =
    let seg =
        formatSevenDay {
            UsedPercentage = 0.0
            ResetsAt = testTimestamp
        }
        |> Option.get

    (seg |> List.head).Text |> should equal "7d "

    (seg |> List.last).Text
    |> should equal (sprintf "0.0%% (reset at %s)" (localTimeString "MM/dd HH:mm"))
