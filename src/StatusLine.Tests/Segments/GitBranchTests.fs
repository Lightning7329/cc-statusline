module StatusLine.Tests.Segments.GitBranchTests

open System.IO
open Xunit
open FsUnit.Xunit
open StatusLine.Segments.GitBranch
open StatusLine.Types.App
open StatusLine.Utils.Process

let private icon = char 0xE0A0 |> string

let private segmentText (seg: Segment option) =
    seg |> Option.get |> List.map _.Text |> String.concat ""

let private withTempDir (f: string -> (string -> unit) -> 'a) =
    let dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
    Directory.CreateDirectory dir |> ignore
    let git args = tryRun dir "git" args |> ignore

    try
        f dir git
    finally
        Directory.Delete(dir, true)

module Format =
    [<Fact>]
    let ``gitリポジトリ内でブランチ名を返す`` () =
        withTempDir (fun dir git ->
            git "init -b test-branch"
            format dir |> segmentText |> should equal $"{icon} test-branch")

    [<Fact>]
    let ``detached HEAD時に短縮コミットハッシュを返す`` () =
        withTempDir (fun dir git ->
            git "init -b main"
            git "-c user.name=test -c user.email=test@test commit --allow-empty -m init"
            let hash = tryRun dir "git" "rev-parse --short HEAD" |> Option.get
            git "checkout --detach"
            format dir |> segmentText |> should equal $"{icon} {hash}")

    [<Fact>]
    let ``gitリポジトリ外ではNoneを返す`` () =
        withTempDir (fun dir _ -> format dir |> should equal None)
