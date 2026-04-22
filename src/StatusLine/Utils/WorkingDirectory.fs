module StatusLine.Utils.WorkingDirectory

open System
open System.IO

let relativePath (getEnv: string -> string option) absolutePath =
    match getEnv "WORKSPACE_ROOT" with
    | None -> absolutePath
    | Some baseDir ->
        let baseDirName = baseDir |> Path.TrimEndingDirectorySeparator |> Path.GetFileName
        let rel = Path.GetRelativePath(baseDir, absolutePath)
        if rel = "." then baseDirName else $"{baseDirName}/{rel}"

let relativePathFromEnv =
    relativePath (Environment.GetEnvironmentVariable >> Option.ofObj)
