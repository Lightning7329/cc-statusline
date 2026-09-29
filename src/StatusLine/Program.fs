open StatusLine

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
    let mutable input: string option = None

    /// エラーをログに記録し、ステータスラインに赤字で表示する。
    /// ログを書き込めたときだけ、表示にログの場所を添える。
    let reportError (summary: string) (heading: string) (detail: string) =
        let logPath =
            Utils.ErrorLog.resolvePath Utils.Settings.envReader (System.IO.Path.GetTempPath())

        let written =
            Utils.ErrorLog.formatEntry System.DateTimeOffset.Now heading detail input
            |> Utils.ErrorLog.append logPath

        (if written then $"{summary} (log: {logPath})" else summary)
        |> StatusLineBuilder.errorSegment
        |> ColoredOutput.render
        |> printfn "%s"

    try
        let raw = stdin.ReadToEnd()
        input <- Some raw

        match
            raw
            |> StatusLineBuilder.buildFromInput Segments.GitBranch.format (Utils.Settings.fromEnv ())
        with
        | Ok segment -> segment |> ColoredOutput.render |> printfn "%s"
        | Error error ->
            let (Types.App.InvalidJson message | Types.App.MissingOrInvalidField message) =
                error

            let summary = StatusLineBuilder.describeError error
            reportError summary summary message
    with ex ->
        eprintfn "statusline error: %s" ex.Message
        reportError "unexpected error" "exception" (string ex)
