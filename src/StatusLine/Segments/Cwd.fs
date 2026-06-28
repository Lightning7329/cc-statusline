module StatusLine.Segments.Cwd

open StatusLine.Types.App
open StatusLine.Types.Context
open StatusLine.Utils.WorkingDirectory

let format (home: string option) (workspace: Workspace) : Segment option =
    shorten workspace.ProjectDir workspace.AddedDirs home workspace.CurrentDir
    |> Segment.fromText
    |> Some
