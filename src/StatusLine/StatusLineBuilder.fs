module StatusLine.StatusLineBuilder

open System.Text.Json
open System.Text.Json.Serialization
open StatusLine.Types
open StatusLine.Segments
open StatusLine.ColoredOutput

let private jsonOptions =
    let opts =
        JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)

    let fsOpt =
        JsonFSharpOptions
            .Default()
            .WithSkippableOptionFields(SkippableOptionFields.Always, deserializeNullAsNone = true)

    opts.Converters.Add(JsonFSharpConverter fsOpt)

    opts

let parseInput (input: string) =
    JsonSerializer.Deserialize<Context>(input, jsonOptions)

let build (c: Context) =
    let cwdText = Cwd.format c.Cwd
    let modelText = ModelName.format c.Model
    let costText = CostDisplay.format c.Cost |> applyColor

    let fiveHourEntry = c.RateLimits |> Option.bind _.FiveHour
    let sevenDayEntry = c.RateLimits |> Option.bind _.SevenDay

    let fiveText = RateLimit.formatFiveHour fiveHourEntry |> Option.map applyColor
    let sevenText = RateLimit.formatSevenDay sevenDayEntry |> Option.map applyColor

    [ Some cwdText; Some modelText; Some costText; fiveText; sevenText ]
    |> List.choose id
    |> String.concat " | "

let buildFromInput = parseInput >> build
