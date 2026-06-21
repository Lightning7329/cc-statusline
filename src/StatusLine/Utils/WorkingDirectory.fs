module StatusLine.Utils.WorkingDirectory

open System.IO

let private isUnder (baseDir: string) (path: string) =
    let trimmed = baseDir |> Path.TrimEndingDirectorySeparator
    path = trimmed || path.StartsWith(trimmed + string Path.DirectorySeparatorChar)

let private relativeWithBaseDirName (baseDir: string) (absolutePath: string) =
    let trimmed = baseDir |> Path.TrimEndingDirectorySeparator
    let baseDirName = trimmed |> Path.GetFileName
    let rel = Path.GetRelativePath(trimmed, absolutePath)
    if rel = "." then baseDirName else $"{baseDirName}/{rel}"

let private homeShortened (home: string) (absolutePath: string) =
    let trimmed = home |> Path.TrimEndingDirectorySeparator

    if absolutePath = trimmed then
        "~"
    else
        "~/" + Path.GetRelativePath(trimmed, absolutePath)

let shorten (projectDir: string) (addedDirs: string list) (home: string option) (absolutePath: string) : string =
    if isUnder projectDir absolutePath then
        relativeWithBaseDirName projectDir absolutePath
    else
        match addedDirs |> List.tryFind (fun d -> isUnder d absolutePath) with
        | Some d -> relativeWithBaseDirName d absolutePath
        | None ->
            match home with
            | Some h when isUnder h absolutePath -> homeShortened h absolutePath
            | _ -> absolutePath
