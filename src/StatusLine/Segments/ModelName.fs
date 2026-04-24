module StatusLine.Segments.ModelName

open StatusLine.Types

let format (model: Model) : string = model.Id.Replace("claude-", "")
