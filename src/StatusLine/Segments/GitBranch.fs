module StatusLine.Segments.GitBranch

open StatusLine.Utils.Process

let private icon = char 0xE0A0 |> string

let formatBranch (branch: string) : string =
    let name = if branch = "HEAD" then "detached" else branch
    $"{icon} {name}"

let formatWithRunner (runner: string -> string option) (cwd: string) : string option =
    runner cwd |> Option.map formatBranch

let format (cwd: string) : string option =
    let runner dir =
        tryRun dir "git" "rev-parse --abbrev-ref HEAD"

    formatWithRunner runner cwd
