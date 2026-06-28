module StatusLine.Segments.ModelName

open StatusLine.Types.App
open StatusLine.Types.Context

let format (model: Model) (effort: Effort option) : Segment option =
    let name = model.Id.Replace("claude-", "")

    match effort with
    | Some e -> sprintf "%s (%s)" name e.Level
    | None -> name
    |> Segment.fromText
    |> Some
