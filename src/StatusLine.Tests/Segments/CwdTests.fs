module StatusLine.Tests.Segments.CwdTests

open Xunit
open FsUnit.Xunit
open StatusLine.Types.Context
open StatusLine.Types.App
open StatusLine.Segments.Cwd

let private workspace (currentDir: string) (projectDir: string) (addedDirs: string list) : Workspace = {
    CurrentDir = currentDir
    ProjectDir = projectDir
    AddedDirs = addedDirs
    GitWorktree = None
    Repo = None
}

let private segmentText seg =
    seg |> Option.get |> List.map _.Text |> String.concat ""

[<Fact>]
let ``current_dir が project_dir 配下のとき、project 名で短縮する`` () =
    workspace "/workspaces/proj/src" "/workspaces/proj" []
    |> format None
    |> segmentText
    |> should equal "proj/src"

[<Fact>]
let ``current_dir が added_dirs 配下のとき、added_dir 名で短縮する`` () =
    workspace "/workspaces/other/lib" "/workspaces/proj" [ "/workspaces/other" ]
    |> format None
    |> segmentText
    |> should equal "other/lib"

[<Fact>]
let ``current_dir が HOME 配下のとき、~ で短縮する`` () =
    workspace "/home/user/docs" "/workspaces/proj" []
    |> format (Some "/home/user")
    |> segmentText
    |> should equal "~/docs"

[<Fact>]
let ``どこにも該当しないとき、絶対パスをそのまま返す`` () =
    workspace "/var/tmp" "/workspaces/proj" []
    |> format None
    |> segmentText
    |> should equal "/var/tmp"
