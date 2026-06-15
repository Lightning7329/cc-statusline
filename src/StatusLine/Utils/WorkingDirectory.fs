module StatusLine.Utils.WorkingDirectory

open System.IO

let relativePath (workspaceRoot: string option) absolutePath =
    match workspaceRoot with
    | None -> absolutePath
    | Some baseDir ->
        let baseDirName = baseDir |> Path.TrimEndingDirectorySeparator |> Path.GetFileName
        let rel = Path.GetRelativePath(baseDir, absolutePath)
        if rel = "." then baseDirName else $"{baseDirName}/{rel}"
