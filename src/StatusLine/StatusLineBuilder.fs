module StatusLine.StatusLineBuilder

open System.Drawing
open System.Text.Json
open System.Text.Json.Serialization
open Pastel
open StatusLine.ColoredOutput
open StatusLine.Segments
open StatusLine.Types.App
open StatusLine.Types.Context
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

let private concatRow (segments: string option list) =
    let parts = segments |> List.choose id

    match parts with
    | [] -> None
    | _ -> parts |> String.concat " | " |> Some

let build (c: Context) =
    let cwdText = Cwd.format c.Cwd |> Some
    let branchText = GitBranch.format c.Cwd
    let modelText = ModelName.format c.Model c.Effort |> Some
    let costText = CostDisplay.format c.Cost |> applyColor |> Some

    let contextUsage = option {
        let! bar = ContextWindowUsage.format c.ContextWindow |> Option.map applyColor
        return sprintf "ctx %s" bar
    }

    let fiveHourText = option {
        let! rateLimitEntry = c.RateLimits |> Option.bind _.FiveHour
        let fiveText = rateLimitEntry |> RateLimit.formatFiveHour |> applyColor
        return sprintf "5h %s" fiveText
    }

    let sevenDayText = option {
        let! rateLimitEntry = c.RateLimits |> Option.bind _.SevenDay
        let sevenText = rateLimitEntry |> RateLimit.formatSevenDay |> applyColor
        return sprintf "7d %s" sevenText
    }

    [
        [ cwdText; branchText; modelText; costText; contextUsage ]
        [ fiveHourText; sevenDayText ]
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
