module StatusLine.Tests.Segments.SessionDurationTests

open Xunit
open FsUnit.Xunit
open StatusLine.Types.Context
open StatusLine.Segments.SessionDuration

let private makeCost durationMs = {
    TotalCostUsd = 0.0
    TotalDurationMs = durationMs
    TotalApiDurationMs = 0
    TotalLinesAdded = 0
    TotalLinesRemoved = 0
}

[<Fact>]
let ``砂時計片と経過時間片の2片で返す`` () =
    let spans = format (makeCost 4323000) |> Option.get
    spans |> List.map _.Text |> should equal [ "⏳ "; "1h 12m 3s" ]

[<Fact>]
let ``砂時計片は無色になる`` () =
    let span = format (makeCost 4323000) |> Option.get |> List.head
    span.Color |> should equal None

[<Fact>]
let ``経過時間片は水色になる`` () =
    let span = format (makeCost 4323000) |> Option.get |> List.last
    int span.Color.Value.R |> should equal 127
    int span.Color.Value.G |> should equal 179
    int span.Color.Value.B |> should equal 212
