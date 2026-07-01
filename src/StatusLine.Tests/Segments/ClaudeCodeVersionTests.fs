module StatusLine.Tests.Segments.ClaudeCodeVersionTests

open Xunit
open FsUnit.Xunit
open StatusLine.Segments.ClaudeCodeVersion

let private segmentText (seg: StatusLine.Types.App.Segment option) =
    seg |> Option.get |> List.map _.Text |> String.concat ""

[<Fact>]
let ``バージョン文字列に v プレフィックスを付ける`` () =
    format "2.1.90" |> segmentText |> should equal "v2.1.90"

[<Fact>]
let ``プレリリースバージョンもそのまま表示する`` () =
    format "1.0.0-beta.1" |> segmentText |> should equal "v1.0.0-beta.1"
