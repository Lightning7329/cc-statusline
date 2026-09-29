open System
open System.IO
open StatusLine
open StatusLine.Types.App
open StatusLine.Utils

/// ステータスラインを表示できなかった原因
type AppError =
    | DeserializeFailed of ContextDeserializeError
    | Unexpected of exn

/// stdin から入力 JSON を読み込む
let tryReadInput () : Result<string, AppError> =
    try
        Ok(stdin.ReadToEnd())
    with ex ->
        Error(Unexpected ex)

/// 入力 JSON から、ANSI カラー付きのステータスラインを作る
let tryRender (input: string) : Result<string, AppError> =
    try
        input
        |> StatusLineBuilder.buildFromInput Segments.GitBranch.format (Settings.fromEnv ())
        |> Result.map ColoredOutput.render
        |> Result.mapError DeserializeFailed
    with ex ->
        Error(Unexpected ex)

/// ステータスラインに表示する、エラーの短い説明
let describe (error: AppError) : string =
    match error with
    | DeserializeFailed e -> StatusLineBuilder.describeError e
    | Unexpected _ -> "unexpected error"

/// エラーログに記録する 1 件分のエントリ。入力を読む前に失敗した場合 input は None
let formatLogEntry (input: string option) (error: AppError) : string =
    match error with
    | DeserializeFailed e -> ErrorLog.formatEntry DateTimeOffset.Now (describe error) e.Message input
    | Unexpected ex -> ErrorLog.formatEntry DateTimeOffset.Now "exception" (string ex) input

/// エラーをログに記録し、ステータスラインに赤字で表示する。
/// ログを書き込めたときだけ、表示にログの場所を添える。
let reportError (input: string option) (error: AppError) =
    match error with
    | Unexpected ex -> eprintfn "statusline error: %s" ex.Message
    | DeserializeFailed _ -> ()

    let logPath = ErrorLog.resolvePath Settings.envReader (Path.GetTempPath())
    let written = formatLogEntry input error |> ErrorLog.append logPath
    let summary = describe error

    (if written then $"{summary} (log: {logPath})" else summary)
    |> StatusLineBuilder.errorSegment
    |> ColoredOutput.render
    |> printfn "%s"

let args = System.Environment.GetCommandLineArgs()

if args |> Array.exists (fun a -> a = "--version" || a = "-v") then
    let version =
        System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes(typeof<System.Reflection.AssemblyInformationalVersionAttribute>, false)
        |> Array.tryHead
        |> Option.map (fun a -> (a :?> System.Reflection.AssemblyInformationalVersionAttribute).InformationalVersion)
        |> Option.defaultValue "unknown"

    printfn "%s" version
else
    // 出力は try の外で行う（エラー表示の出力に失敗したとき、それを予期しない例外として二重に記録しないため）
    match tryReadInput () with
    | Error error -> reportError None error
    | Ok input ->
        match tryRender input with
        | Ok statusLine -> printfn "%s" statusLine
        | Error error -> reportError (Some input) error
