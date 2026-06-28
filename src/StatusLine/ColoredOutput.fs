module StatusLine.ColoredOutput

open Pastel
open StatusLine.Types.App

let applyColor (span: Span) : string =
    match span.Color with
    | None -> span.Text
    | Some c -> ConsoleExtensions.Pastel(span.Text, c)

let render (segment: Segment) : string =
    segment |> List.map applyColor |> String.concat ""
