module StatusLine.Tests.Utils.ProcessTests

open System.IO
open Xunit
open FsUnit.Xunit
open StatusLine.Utils.Process

let private withTempDir (f: string -> 'a) =
    let dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
    Directory.CreateDirectory(dir) |> ignore

    try
        f dir
    finally
        Directory.Delete(dir, true)

[<Fact>]
let ``正常終了するコマンドはSomeを返す`` () =
    withTempDir (fun dir -> tryRun dir "echo" "hello" |> should equal (Some "hello"))

[<Fact>]
let ``異常終了するコマンドはNoneを返す`` () =
    withTempDir (fun dir -> tryRun dir "git" "rev-parse --short HEAD" |> should equal None)

[<Fact>]
let ``存在しないコマンドはNoneを返す`` () =
    withTempDir (fun dir -> tryRun dir "nonexistent-command-xyz" "" |> should equal None)
