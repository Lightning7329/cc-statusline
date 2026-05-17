module StatusLine.Utils.Process

open System.Diagnostics

let tryRun (workingDirectory: string) (fileName: string) (arguments: string) : string option =
    try
        let psi =
            ProcessStartInfo(
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            )

        use proc = Process.Start(psi)
        let output = proc.StandardOutput.ReadToEnd()
        proc.WaitForExit()

        if proc.ExitCode = 0 then Some(output.TrimEnd()) else None
    with _ ->
        None
