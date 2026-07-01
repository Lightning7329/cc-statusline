module StatusLine.Segments.ClaudeCodeVersion

open StatusLine.Types.App

let format (version: string) : Segment option =
    sprintf "v%s" version |> Segment.fromText |> Some
