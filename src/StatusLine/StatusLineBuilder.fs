module StatusLine.StatusLineBuilder

open System.Drawing
open System.Text.Json
open System.Text.Json.Serialization
open StatusLine.Segments
open StatusLine.Types.App
open StatusLine.Types.Context
open StatusLine.Utils.Settings

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

let private separator: Segment = [ { Text = " | "; Color = None } ]
let private newline: Segment = [ { Text = "\n"; Color = None } ]

let private joinWith (sep: Segment) (segments: Segment list) : Segment =
    match segments with
    | [] -> []
    | _ -> segments |> List.reduce (fun acc segment -> acc @ sep @ segment)

let private concatRow (segments: Segment option list) : Segment option =
    match segments |> List.choose id with
    | [] -> None
    | parts -> parts |> joinWith separator |> Some

let build (formatBranch: string -> Segment option) (settings: Settings) (c: Context) : Segment =
    [
        [
            Cwd.format settings.Home c.Workspace
            formatBranch c.Cwd
            ModelName.format c.Model c.Effort
            ClaudeCodeVersion.format c.Version
        ]
        [
            ContextWindowUsage.format c.ContextWindow
            LinesChanged.format c.Cost
            CostDisplay.format c.Cost
            SessionDuration.format c.Cost
        ]
        [ RateLimit.formatFiveHour c.RateLimits; RateLimit.formatSevenDay c.RateLimits ]
    ]
    |> List.choose concatRow
    |> joinWith newline

let errorSegment (message: string) : Segment = [
    {
        Text = sprintf "statusline error: %s" message
        Color = Some Color.Red
    }
]

/// エラー表示に使う、パースエラーの種類の説明
let describeError (error: ContextDeserializeError) : string =
    match error with
    | InvalidJson _ -> "invalid JSON"
    | MissingOrInvalidField _ -> "missing or invalid field"

let buildFromInput
    (formatBranch: string -> Segment option)
    (settings: Settings)
    (input: string)
    : Result<Segment, ContextDeserializeError> =
    tryParseInput input |> Result.map (build formatBranch settings)
