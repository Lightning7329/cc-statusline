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
