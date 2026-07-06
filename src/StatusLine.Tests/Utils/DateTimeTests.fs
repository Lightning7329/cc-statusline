namespace StatusLine.Tests.Utils.DateTimeTests

open Xunit
open FsUnit.Xunit
open StatusLine.Utils.DateTime

module FormatDurationMs =

    [<Fact>]
    let ``0ミリ秒は0秒になる`` () = formatDurationMs 0 |> should equal "0s"

    [<Fact>]
    let ``1分未満は秒だけを返す`` () =
        formatDurationMs 45000 |> should equal "45s"

    [<Fact>]
    let ``59秒は秒だけを返す`` () =
        formatDurationMs 59000 |> should equal "59s"

    [<Fact>]
    let ``ちょうど60秒は1分0秒になる`` () =
        formatDurationMs 60000 |> should equal "1m 0s"

    [<Fact>]
    let ``1時間未満は分と秒を返す`` () =
        formatDurationMs 723000 |> should equal "12m 3s"

    [<Fact>]
    let ``ちょうど1時間は1時間0分0秒になる`` () =
        formatDurationMs 3600000 |> should equal "1h 0m 0s"

    [<Fact>]
    let ``1時間以上は時分秒すべてを返す`` () =
        formatDurationMs 4323000 |> should equal "1h 12m 3s"

    [<Fact>]
    let ``中間の分がゼロでも残す`` () =
        formatDurationMs 3605000 |> should equal "1h 0m 5s"

    [<Fact>]
    let ``24時間を超えても日単位を作らず時間に集約する`` () =
        formatDurationMs 90000000 |> should equal "25h 0m 0s"

    [<Fact>]
    let ``ゼロ埋めはしない`` () =
        formatDurationMs 3665000 |> should equal "1h 1m 5s"
