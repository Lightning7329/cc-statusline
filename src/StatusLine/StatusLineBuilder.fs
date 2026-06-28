module StatusLine.StatusLineBuilder

open System.Drawing
open System.Text.Json
open System.Text.Json.Serialization
open Pastel
open StatusLine.ColoredOutput
open StatusLine.Segments
open StatusLine.Types.App
open StatusLine.Types.Context
open StatusLine.Utils
open StatusLine.Utils.OptionBuilder

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

let private concatRow (segments: Segment option list) =
    let parts = segments |> List.choose id |> List.map render

    match parts with
    | [] -> None
    | _ -> parts |> String.concat " | " |> Some

let build (c: Context) =
    let settings = Settings.fromEnv ()

    let fiveHour = option {
        let! rateLimitEntry = c.RateLimits |> Option.bind _.FiveHour
        return! RateLimit.formatFiveHour rateLimitEntry
    }

    let sevenDay = option {
        let! rateLimitEntry = c.RateLimits |> Option.bind _.SevenDay
        return! RateLimit.formatSevenDay rateLimitEntry
    }

    [
        [
            Cwd.format settings.Home c.Workspace
            GitBranch.format c.Cwd
            ModelName.format c.Model c.Effort
            CostDisplay.format c.Cost
            LinesChanged.format c.Cost
            ContextWindowUsage.format c.ContextWindow
        ]
        [ fiveHour; sevenDay ]
    ]
    |> List.choose concatRow
    |> String.concat "\n"

let buildFromInput input =
    match tryParseInput input with
    | Ok ctx ->
        try
            build ctx
        with ex ->
            eprintfn "statusline error: %s" ex.Message
            "statusline error: unexpected error".Pastel Color.Red
    | Error(InvalidJson _) -> "statusline error: invalid JSON".Pastel Color.Red
    | Error(MissingOrInvalidField _) -> "statusline error: missing or invalid field".Pastel Color.Red
