module StatusLine.StatusLineBuilder

open System.Text.Json
open System.Text.Json.Serialization
open StatusLine.Types
open StatusLine.Segments
open StatusLine.ColoredOutput

let private jsonOptions =
    let opts = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)
    opts.Converters.Add(JsonFSharpConverter())
    opts

let parseInput (input: string) =
    JsonSerializer.Deserialize<Context>(input, jsonOptions)

let build (c: Context) =
    let cwdText = Cwd.format c.Cwd
    let modelText = ModelName.format c.Model
    let costText = CostDisplay.format c.Cost |> applyColor
    let fiveText = RateLimit.formatFiveHour c.RateLimits.FiveHour |> applyColor
    let sevenText = RateLimit.formatSevenDay c.RateLimits.SevenDay |> applyColor
    [ cwdText; modelText; costText; fiveText; sevenText ] |> String.concat " | "

let buildFromInput = parseInput >> build
