module StatusLine.Tests.Segments.LinesChangedTests

open Xunit
open FsUnit.Xunit
open StatusLine.Types.Context
open StatusLine.Segments.LinesChanged

let private makeCost added removed = {
    TotalCostUsd = 0.0
    TotalDurationMs = 0
    TotalApiDurationMs = 0
    TotalLinesAdded = added
    TotalLinesRemoved = removed
}

[<Fact>]
let ``追加と削除を プラス スラッシュ マイナス の3片で返す`` () =
    let spans = format (makeCost 5 3) |> Option.get
    spans |> List.map _.Text |> should equal [ "+5"; "/"; "-3" ]

[<Fact>]
let ``追加片は緑色になる`` () =
    let span = format (makeCost 5 3) |> Option.get |> List.head
    int span.Color.Value.R |> should equal 144
    int span.Color.Value.G |> should equal 238
    int span.Color.Value.B |> should equal 144

[<Fact>]
let ``区切りのスラッシュは無色になる`` () =
    let span = format (makeCost 5 3) |> Option.get |> List.item 1
    span.Text |> should equal "/"
    span.Color |> should equal None

[<Fact>]
let ``削除片は赤色になる`` () =
    let span = format (makeCost 5 3) |> Option.get |> List.last
    int span.Color.Value.R |> should equal 240
    int span.Color.Value.G |> should equal 128
    int span.Color.Value.B |> should equal 128

[<Fact>]
let ``両方0のときプラス0スラッシュマイナス0を返す`` () =
    let spans = format (makeCost 0 0) |> Option.get
    spans |> List.map _.Text |> should equal [ "+0"; "/"; "-0" ]
