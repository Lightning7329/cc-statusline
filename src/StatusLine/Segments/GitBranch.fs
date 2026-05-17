module StatusLine.Segments.GitBranch

open StatusLine.Utils.Process

let private icon = char 0xE0A0 |> string

let formatBranch (branch: string) : string = $"{icon} {branch}"

let formatWithRunner (runner: string -> string option) (cwd: string) : string option =
    runner cwd |> Option.map formatBranch

let private getBranch (cwd: string) : string option =
    tryRun cwd "git" "--no-optional-locks symbolic-ref --short HEAD"
    |> Option.orElseWith (fun () -> tryRun cwd "git" "--no-optional-locks rev-parse --short HEAD")

let format (cwd: string) : string option = formatWithRunner getBranch cwd
