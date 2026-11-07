module StatusLine.Segments.ModelName

open StatusLine.Types.App
open StatusLine.Types.Context

let format (model: Model) (effort: Effort option) (fastMode: bool option) : Segment option =
    let name = model.Id.Replace("claude-", "")

    let tags = [
        yield! effort |> Option.map _.Level |> Option.toList
        if fastMode = Some true then
            "fast"
    ]

    match tags with
    | [] -> name
    | _ -> sprintf "%s (%s)" name (String.concat ", " tags)
    |> Segment.fromText
    |> Some
