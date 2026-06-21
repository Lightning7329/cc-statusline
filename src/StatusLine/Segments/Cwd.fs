module StatusLine.Segments.Cwd

open StatusLine.Utils.WorkingDirectory

let format (workspaceRoot: string option) (cwd: string) : string = relativePath workspaceRoot cwd
