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
    try
        stdin.ReadToEnd()
        |> StatusLineBuilder.buildFromInput Segments.GitBranch.format (Utils.Settings.fromEnv ())
        |> ColoredOutput.render
        |> printfn "%s"
    with ex ->
        eprintfn "statusline error: %s" ex.Message
        printfn "statusline error: unexpected error"
