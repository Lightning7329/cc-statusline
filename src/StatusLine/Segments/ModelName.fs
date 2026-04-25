module StatusLine.Segments.ModelName

open StatusLine.Types.Context

let format (model: Model) : string = model.Id.Replace("claude-", "")
