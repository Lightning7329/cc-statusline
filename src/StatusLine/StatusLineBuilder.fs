module StatusLine.StatusLineBuilder

open System.Text.Json
open System.Text.Json.Serialization
open StatusLine.Types

let private jsonOptions =
    let opts = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)
    opts.Converters.Add(JsonFSharpConverter())
    opts

let parseInput (input: string) =
    JsonSerializer.Deserialize<Context>(input, jsonOptions)

let build (c: Context) = c.ToString()

let buildFromInput = parseInput >> build
