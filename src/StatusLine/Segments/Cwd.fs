module StatusLine.Segments.Cwd

open StatusLine.Types.Context
open StatusLine.Utils.WorkingDirectory

let format (home: string option) (workspace: Workspace) : string =
    shorten workspace.ProjectDir workspace.AddedDirs home workspace.CurrentDir
