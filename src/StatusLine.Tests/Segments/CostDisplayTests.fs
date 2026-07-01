module StatusLine.Tests.Segments.CostDisplayTests

open System.Drawing
open Xunit
open FsUnit.Xunit
open StatusLine.Types.Context
open StatusLine.Types.App
open StatusLine.Segments.CostDisplay

let private makeCost usd = {
    TotalCostUsd = usd
    TotalDurationMs = 0
    TotalApiDurationMs = 0
    TotalLinesAdded = 0
    TotalLinesRemoved = 0
}

[<Fact>]
let ``コストを$0.0123形式でフォーマットする`` () =
    (format (makeCost 0.01234) |> Option.get |> List.exactlyOne).Text
    |> should equal "$0.0123"

[<Fact>]
let ``コストが0の場合は$0.0000を返す`` () =
    (format (makeCost 0.0) |> Option.get |> List.exactlyOne).Text
    |> should equal "$0.0000"

[<Fact>]
let ``コストが1.0の場合は$1.0000を返す`` () =
    (format (makeCost 1.0) |> Option.get |> List.exactlyOne).Text
    |> should equal "$1.0000"

[<Fact>]
let ``Colorはオレンジを返す`` () =
    (format (makeCost 0.5) |> Option.get |> List.exactlyOne).Color
    |> should equal (Some(Color.FromArgb(212, 162, 127)))
