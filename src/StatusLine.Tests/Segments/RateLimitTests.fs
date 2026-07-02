namespace StatusLine.Tests.Segments.RateLimitTests

open System
open Xunit
open FsUnit.Xunit
open StatusLine.Types.Context
open StatusLine.Segments.RateLimit

module internal Fixture =
    // 1738422000 = 2025-02-01 15:00:00 UTC
    let testTimestamp = 1738422000L

    let localTimeString (format: string) =
        DateTimeOffset.FromUnixTimeSeconds(testTimestamp).ToLocalTime().ToString format

    let makeEntry usedPercentage = {
        UsedPercentage = usedPercentage
        ResetsAt = testTimestamp
    }

    let makeLimits fiveHour sevenDay = {
        FiveHour = fiveHour
        SevenDay = sevenDay
    }

module FormatEntry =
    open Fixture

    [<Fact>]
    let ``0%のとき緑色を返す`` () =
        let span = formatEntry "5h " "HH:mm" (makeEntry 0.0) |> List.last

        int span.Color.Value.R |> should equal 0
        int span.Color.Value.G |> should equal 255
        int span.Color.Value.B |> should equal 0

    [<Fact>]
    let ``100%のとき赤色を返す`` () =
        let span = formatEntry "5h " "HH:mm" (makeEntry 100.0) |> List.last

        int span.Color.Value.R |> should equal 255
        int span.Color.Value.G |> should equal 0
        int span.Color.Value.B |> should equal 0

    [<Fact>]
    let ``先頭に無色のラベルを付ける`` () =
        let head = formatEntry "5h " "HH:mm" (makeEntry 50.0) |> List.head

        head.Text |> should equal "5h "
        head.Color |> should equal None

    [<Fact>]
    let ``HH:mm形式でリセット時刻をフォーマットする`` () =
        let span = formatEntry "5h " "HH:mm" (makeEntry 50.0) |> List.last

        span.Text
        |> should equal (sprintf "50.0%% (reset at %s)" (localTimeString "HH:mm"))

    [<Fact>]
    let ``MM/dd HH:mm形式でリセット時刻をフォーマットする`` () =
        let span = formatEntry "7d " "MM/dd HH:mm" (makeEntry 50.0) |> List.last

        span.Text
        |> should equal (sprintf "50.0%% (reset at %s)" (localTimeString "MM/dd HH:mm"))

module FormatFiveHour =
    open Fixture

    [<Fact>]
    let ``5hラベルとHH:mm形式を使う`` () =
        let seg = formatFiveHour (Some(makeLimits (Some(makeEntry 0.0)) None)) |> Option.get

        (seg |> List.head).Text |> should equal "5h "

        (seg |> List.last).Text
        |> should equal (sprintf "0.0%% (reset at %s)" (localTimeString "HH:mm"))

    [<Fact>]
    let ``RateLimitsが不在のときNoneを返す`` () =
        formatFiveHour None |> should equal None

    [<Fact>]
    let ``FiveHourが不在のときNoneを返す`` () =
        formatFiveHour (Some(makeLimits None (Some(makeEntry 10.0))))
        |> should equal None

module FormatSevenDay =
    open Fixture

    [<Fact>]
    let ``7dラベルとMM/dd HH:mm形式を使う`` () =
        let seg = formatSevenDay (Some(makeLimits None (Some(makeEntry 0.0)))) |> Option.get

        (seg |> List.head).Text |> should equal "7d "

        (seg |> List.last).Text
        |> should equal (sprintf "0.0%% (reset at %s)" (localTimeString "MM/dd HH:mm"))

    [<Fact>]
    let ``RateLimitsが不在のときNoneを返す`` () =
        formatSevenDay None |> should equal None

    [<Fact>]
    let ``SevenDayが不在のときNoneを返す`` () =
        formatSevenDay (Some(makeLimits (Some(makeEntry 10.0)) None))
        |> should equal None
