module StatusLine.Segments.GitBranch

open System.Drawing
open StatusLine.Types.App
open StatusLine.Utils

let private icon = char 0xE0A0 |> string

/// 通常の作業ツリーのブランチ名の色
let private normalColor = Color.FromArgb(140, 200, 140)

/// linked worktree のブランチ名の色
let private worktreeColor = Color.FromArgb(110, 200, 200)

/// ブランチ名に色を付ける（アイコンは色なし）。
/// linked worktree（`workspace.git_worktree` がある）なら水色、それ以外はグリーンにする。
let formatBranch (isWorktree: bool) (branch: string) : Segment = [
    { Text = $"{icon} "; Color = None }
    {
        Text = branch
        Color = Some(if isWorktree then worktreeColor else normalColor)
    }
]

let formatWithRunner (runner: string -> string option) (cwd: string) (isWorktree: bool) : Segment option =
    runner cwd |> Option.map (formatBranch isWorktree)

let private getBranch (cwd: string) : string option =
    Process.tryRun cwd "git" "--no-optional-locks symbolic-ref --short HEAD"
    |> Option.orElseWith (fun () -> Process.tryRun cwd "git" "--no-optional-locks rev-parse --short HEAD")

let format (cwd: string) (isWorktree: bool) : Segment option =
    formatWithRunner getBranch cwd isWorktree
