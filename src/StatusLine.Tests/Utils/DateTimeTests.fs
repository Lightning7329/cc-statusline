namespace StatusLine.Tests.Utils.DateTimeTests

open Xunit
open FsUnit.Xunit
open StatusLine.Utils.DateTime

module MsToDuration =

    [<Fact>]
    let ``0ミリ秒はすべてゼロに分解する`` () =
        msToDuration 0 |> should equal { Hours = 0; Minutes = 0; Seconds = 0 }

    [<Fact>]
    let ``1分未満は秒だけに分解する`` () =
        msToDuration 45000 |> should equal { Hours = 0; Minutes = 0; Seconds = 45 }

    [<Fact>]
    let ``ちょうど60秒は1分に繰り上がる`` () =
        msToDuration 60000 |> should equal { Hours = 0; Minutes = 1; Seconds = 0 }

    [<Fact>]
    let ``1時間未満は分と秒に分解する`` () =
        msToDuration 723000 |> should equal { Hours = 0; Minutes = 12; Seconds = 3 }

    [<Fact>]
    let ``ちょうど1時間は1時間に繰り上がる`` () =
        msToDuration 3600000 |> should equal { Hours = 1; Minutes = 0; Seconds = 0 }

    [<Fact>]
    let ``時分秒すべてに分解する`` () =
        msToDuration 4323000 |> should equal { Hours = 1; Minutes = 12; Seconds = 3 }

    [<Fact>]
    let ``24時間を超えても日単位を作らず時間に集約する`` () =
        msToDuration 90000000 |> should equal { Hours = 25; Minutes = 0; Seconds = 0 }

    [<Fact>]
    let ``秒未満は切り捨てる`` () =
        msToDuration 999 |> should equal { Hours = 0; Minutes = 0; Seconds = 0 }

    [<Fact>]
    let ``負値は0に丸める`` () =
        msToDuration -5000 |> should equal { Hours = 0; Minutes = 0; Seconds = 0 }

module TryUnixTimeToLocalDateTimeOffset =

    // 9999-12-31 23:59:59 UTC
    let private maxUnixTime = 253402300799L
    // 0001-01-01 00:00:00 UTC
    let private minUnixTime = -62135596800L

    [<Fact>]
    let ``表せる範囲の値はSomeを返す`` () =
        tryUnixTimeToLocalDateTimeOffset 1738422000L
        |> Option.map _.ToUnixTimeSeconds()
        |> should equal (Some 1738422000L)

    [<Fact>]
    let ``上限の値はSomeを返す`` () =
        tryUnixTimeToLocalDateTimeOffset maxUnixTime
        |> Option.isSome
        |> should equal true

    [<Fact>]
    let ``下限の値はSomeを返す`` () =
        tryUnixTimeToLocalDateTimeOffset minUnixTime
        |> Option.isSome
        |> should equal true

    [<Fact>]
    let ``上限を超える値はNoneを返す`` () =
        tryUnixTimeToLocalDateTimeOffset (maxUnixTime + 1L) |> should equal None

    [<Fact>]
    let ``下限を下回る値はNoneを返す`` () =
        tryUnixTimeToLocalDateTimeOffset (minUnixTime - 1L) |> should equal None
