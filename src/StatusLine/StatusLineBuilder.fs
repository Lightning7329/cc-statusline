module StatusLine.StatusLineBuilder

open System.Drawing
open System.Text.Json
open System.Text.Json.Serialization
open Pastel
open StatusLine.Types.Context
open StatusLine.Types.App
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

let tryParseInput (input: string) =
    try
        Ok(JsonSerializer.Deserialize<Context>(input, jsonOptions))
    with
    | :? JsonException as ex ->
        let isValidJson =
            try
                (JsonDocument.Parse input).Dispose()
                true
            with :? JsonException ->
                false

        if isValidJson then
            Error(MissingOrInvalidField ex.Message)
        else
            Error(InvalidJson ex.Message)
    | ex -> Error(InvalidJson ex.Message)

let build (c: Context) =
    let cwdText = Cwd.format c.Cwd |> Some
    let modelText = ModelName.format c.Model |> Some
    let costText = CostDisplay.format c.Cost |> applyColor |> Some

    let contextUsage =
        ContextWindowUsage.format c.ContextWindow |> Option.map applyColor

    let fiveHourEntry = c.RateLimits |> Option.bind _.FiveHour
    let sevenDayEntry = c.RateLimits |> Option.bind _.SevenDay

    let fiveText = RateLimit.formatFiveHour fiveHourEntry |> Option.map applyColor
    let sevenText = RateLimit.formatSevenDay sevenDayEntry |> Option.map applyColor

    [ cwdText; modelText; costText; contextUsage; fiveText; sevenText ]
    |> List.choose id
    |> String.concat " | "

let buildFromInput input =
    match tryParseInput input with
    | Ok ctx -> build ctx
    | Error(InvalidJson _) -> "statusline error: invalid JSON".Pastel Color.Red
    | Error(MissingOrInvalidField _) -> "statusline error: missing or invalid field".Pastel Color.Red
