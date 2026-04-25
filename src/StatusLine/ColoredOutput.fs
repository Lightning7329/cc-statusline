module StatusLine.ColoredOutput

open Pastel
open StatusLine.Types.App

let applyColor (seg: Segment) : string =
    match seg.Color with
    | None -> seg.Text
    | Some c -> ConsoleExtensions.Pastel(seg.Text, c)
