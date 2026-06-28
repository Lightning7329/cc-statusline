namespace StatusLine.Tests.Segments.ContextWindowUsageTests

open Xunit
open FsUnit.Xunit
open StatusLine.Types.Context
open StatusLine.Segments.ContextWindowUsage

module FormatBar =

    [<Fact>]
    let ``0%のときバーが全て空白になる`` () =
        formatBar 0 |> should equal "          "

    [<Fact>]
    let ``100%のときバーが全て⣿になる`` () =
        formatBar 100 |> should equal "⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿"

    [<Fact>]
    let ``50%のとき前半5個が⣿で後半5個が空白になる`` () =
        formatBar 50 |> should equal "⣿⣿⣿⣿⣿     "

    [<Fact>]
    let ``25%のとき部分ブロックを含むバーを返す`` () =
        formatBar 25 |> should equal "⣿⣿⣦       "

    [<Fact>]
    let ``バーの長さが常に10文字になる`` () =
        for p in 0..100 do
            (formatBar p).Length |> should equal 10

    [<Fact>]
    let ``負の値は0にクランプされる`` () =
        formatBar -10 |> should equal (formatBar 0)

    [<Fact>]
    let ``100超の値は100にクランプされる`` () =
        formatBar 120 |> should equal (formatBar 100)

module Format =

    let private makeContextWindow usedPercentage = {
        TotalInputTokens = 0
        TotalOutputTokens = 0
        ContextWindowSize = 200000
        UsedPercentage = usedPercentage
        RemainingPercentage = None
        CurrentUsage = None
    }

    [<Fact>]
    let ``UsedPercentageがNoneのときNoneを返す`` () =
        format (makeContextWindow None) |> should equal None

    [<Fact>]
    let ``UsedPercentageがSomeのときセグメントを返す`` () =
        let seg = format (makeContextWindow (Some 50))
        seg.IsSome |> should be True
        seg.Value |> List.length |> should equal 2
        (seg.Value |> List.last).Text |> should equal "⣿⣿⣿⣿⣿      50%"

    [<Fact>]
    let ``先頭に無色のctxプレフィックスを付ける`` () =
        let head = (format (makeContextWindow (Some 50))).Value |> List.head
        head.Text |> should equal "ctx "
        head.Color |> should equal None

    [<Fact>]
    let ``0%のとき緑色を返す`` () =
        let span = (format (makeContextWindow (Some 0))).Value |> List.last
        int span.Color.Value.R |> should equal 0
        int span.Color.Value.G |> should equal 255
        int span.Color.Value.B |> should equal 0

    [<Fact>]
    let ``100%のとき赤色を返す`` () =
        let span = (format (makeContextWindow (Some 100))).Value |> List.last
        int span.Color.Value.R |> should equal 255
        int span.Color.Value.G |> should equal 0
        int span.Color.Value.B |> should equal 0
