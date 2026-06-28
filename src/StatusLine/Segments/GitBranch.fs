module StatusLine.Segments.GitBranch

open StatusLine.Types.App
open StatusLine.Utils

let private icon = char 0xE0A0 |> string

let formatBranch (branch: string) : Segment = $"{icon} {branch}" |> Segment.fromText

let formatWithRunner (runner: string -> string option) (cwd: string) : Segment option =
    runner cwd |> Option.map formatBranch

let private getBranch (cwd: string) : string option =
    Process.tryRun cwd "git" "--no-optional-locks symbolic-ref --short HEAD"
    |> Option.orElseWith (fun () -> Process.tryRun cwd "git" "--no-optional-locks rev-parse --short HEAD")

let format (cwd: string) : Segment option = formatWithRunner getBranch cwd
