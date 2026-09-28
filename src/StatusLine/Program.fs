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

    try
        let raw = stdin.ReadToEnd()
        input <- Some raw

        raw
        |> StatusLineBuilder.buildFromInput Segments.GitBranch.format (Utils.Settings.fromEnv ())
        |> ColoredOutput.render
        |> printfn "%s"
    with ex ->
        eprintfn "statusline error: %s" ex.Message

        Utils.ErrorLog.resolvePath Utils.Settings.envReader
        |> Option.iter (fun path ->
            Utils.ErrorLog.formatEntry System.DateTimeOffset.Now ex input
            |> Utils.ErrorLog.append path)

        printfn "statusline error: unexpected error"
