module StatusLine.Segments.GitBranch

open StatusLine.Types.App
open StatusLine.Utils

let private icon = char 0xE0A0 |> string

/// linked worktree（`workspace.git_worktree` がある）なら、ブランチ名の末尾に ` (wt)` を付ける
let formatBranch (isWorktree: bool) (branch: string) : Segment =
    let suffix = if isWorktree then " (wt)" else ""
    $"{icon} {branch}{suffix}" |> Segment.fromText

let formatWithRunner (runner: string -> string option) (cwd: string) (isWorktree: bool) : Segment option =
    runner cwd |> Option.map (formatBranch isWorktree)

let private getBranch (cwd: string) : string option =
    Process.tryRun cwd "git" "--no-optional-locks symbolic-ref --short HEAD"
    |> Option.orElseWith (fun () -> Process.tryRun cwd "git" "--no-optional-locks rev-parse --short HEAD")

let format (cwd: string) (isWorktree: bool) : Segment option =
    formatWithRunner getBranch cwd isWorktree
