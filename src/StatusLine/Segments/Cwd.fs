module StatusLine.Segments.Cwd

open StatusLine.Utils.WorkingDirectory

let formatCwd (getEnv: string -> string option) (cwd: string) : string = relativePath getEnv cwd

let format (cwd: string) : string = relativePathFromEnv cwd
