module StatusLine.Segments.GitBranch

open System.Drawing
open StatusLine.Types.App
open StatusLine.Utils

let private icon = char 0xE0A0 |> string

/// linked worktree のブランチの色
let private worktreeColor = Color.FromArgb(110, 200, 200) // rgb(110, 200, 200)

/// 通常はアイコンとブランチ名を色なしで返す。
/// linked worktree（`workspace.git_worktree` がある）なら、アイコンとブランチ名の間に `(wt)` を入れ、
/// `(wt) ブランチ名` の部分を水色にする（アイコンは色なし）。
let formatBranch (isWorktree: bool) (branch: string) : Segment =
    if isWorktree then
        [
            { Text = $"{icon} "; Color = None }
            {
                Text = $"(wt) {branch}"
                Color = Some worktreeColor
            }
        ]
    else
        $"{icon} {branch}" |> Segment.fromText

let formatWithRunner (runner: string -> string option) (cwd: string) (isWorktree: bool) : Segment option =
    runner cwd |> Option.map (formatBranch isWorktree)

let private getBranch (cwd: string) : string option =
    Process.tryRun cwd "git" "--no-optional-locks symbolic-ref --short HEAD"
    |> Option.orElseWith (fun () -> Process.tryRun cwd "git" "--no-optional-locks rev-parse --short HEAD")

let format (cwd: string) (isWorktree: bool) : Segment option =
    formatWithRunner getBranch cwd isWorktree
