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

module FormatTokens =

    [<Theory>]
    [<InlineData(0, "0")>]
    [<InlineData(999, "999")>]
    let ``1000未満はそのまま表示する`` (tokens: int, expected: string) =
        formatTokens tokens |> should equal expected

    [<Theory>]
    [<InlineData(1_000, "1k")>]
    [<InlineData(120_456, "120k")>]
    [<InlineData(200_000, "200k")>]
    [<InlineData(999_999, "999k")>]
    let ``1000以上はkの単位で整数に切り捨てて表示する`` (tokens: int, expected: string) =
        formatTokens tokens |> should equal expected

    [<Theory>]
    [<InlineData(1_250_000, "1.2M")>]
    [<InlineData(1_099_999, "1M")>]
    let ``1000000以上はMの単位で小数1桁に切り捨てて表示する`` (tokens: int, expected: string) =
        formatTokens tokens |> should equal expected

    [<Theory>]
    [<InlineData(1_000_000, "1M")>]
    [<InlineData(2_000_000, "2M")>]
    let ``Mの小数部が0のときは整数で表示する`` (tokens: int, expected: string) =
        formatTokens tokens |> should equal expected

module Format =

    let private makeContextWindowWith totalInputTokens contextWindowSize usedPercentage = {
        TotalInputTokens = totalInputTokens
        TotalOutputTokens = 0
        ContextWindowSize = contextWindowSize
        UsedPercentage = usedPercentage
        RemainingPercentage = None
        CurrentUsage = None
    }

    let private makeContextWindow usedPercentage =
        makeContextWindowWith 0 200000 usedPercentage

    [<Fact>]
    let ``Noneのとき0%を表示する`` () =
        let seg = format (makeContextWindow None)
        seg.IsSome |> should be True
        (seg.Value |> List.last).Text |> should equal "           0%"

    [<Fact>]
    let ``NoneとSome0が同一の結果を返す`` () =
        format (makeContextWindow None)
        |> should equal (format (makeContextWindow (Some 0)))

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

    [<Fact>]
    let ``使用量と上限のトークン数を括弧内に表示する`` () =
        let seg = format (makeContextWindowWith 120_000 1_000_000 (Some 12))
        (seg.Value |> List.last).Text |> should equal "⣿⣀         12% (120k/1M)"

    [<Fact>]
    let ``トークン数は%と同じ色のスパンに含まれる`` () =
        let seg = format (makeContextWindowWith 120_000 1_000_000 (Some 12))
        seg.Value |> List.length |> should equal 2

        (seg.Value |> List.last).Color
        |> should equal (Some(StatusLine.Utils.Color.percentageToColor 12.0))

    [<Fact>]
    let ``TotalInputTokensが0のとき括弧ごと省く`` () =
        let seg = format (makeContextWindowWith 0 200_000 (Some 0))
        (seg.Value |> List.last).Text |> should equal "           0%"

    [<Fact>]
    let ``ContextWindowSizeが0のとき括弧ごと省く`` () =
        let seg = format (makeContextWindowWith 5_000 0 (Some 0))
        (seg.Value |> List.last).Text |> should equal "           0%"
