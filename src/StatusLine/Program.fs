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

    // try 内では表示する内容を決めるだけにし、出力は try の外で 1 回だけ行う
    // （エラー表示の出力が失敗したときに、それを予期しない例外として二重に記録しないため）
    let outcome =
        try
            let raw = stdin.ReadToEnd()
            input <- Some raw

            match
                raw
                |> StatusLineBuilder.buildFromInput Segments.GitBranch.format (Utils.Settings.fromEnv ())
            with
            | Ok segment -> Ok(ColoredOutput.render segment)
            | Error error ->
                let (Types.App.InvalidJson message | Types.App.MissingOrInvalidField message) =
                    error

                let summary = StatusLineBuilder.describeError error
                Error(summary, summary, message)
        with ex ->
            eprintfn "statusline error: %s" ex.Message
            Error("unexpected error", "exception", string ex)

    match outcome with
    | Ok text -> printfn "%s" text
    | Error(summary, heading, detail) -> reportError summary heading detail
