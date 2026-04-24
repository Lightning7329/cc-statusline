module StatusLine.Tests.Segments.CwdTests

open Xunit
open FsUnit.Xunit
open StatusLine.Segments.Cwd

[<Fact>]
let ``環境変数が設定されている場合、WORKSPACE_ROOT相対パスを返す`` () =
    formatCwd (fun _ -> Some "/workspaces/proj") "/workspaces/proj/src"
    |> should equal "proj/src"

[<Fact>]
let ``環境変数が未設定の場合、絶対パスをそのまま返す`` () =
    formatCwd (fun _ -> None) "/some/absolute/path"
    |> should equal "/some/absolute/path"
