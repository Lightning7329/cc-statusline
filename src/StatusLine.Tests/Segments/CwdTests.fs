module StatusLine.Tests.Segments.CwdTests

open Xunit
open FsUnit.Xunit
open StatusLine.Segments.Cwd

[<Fact>]
let ``WORKSPACE_ROOTが設定されている場合、相対パスを返す`` () =
    format (Some "/workspaces/proj") "/workspaces/proj/src"
    |> should equal "proj/src"

[<Fact>]
let ``WORKSPACE_ROOTが未設定の場合、絶対パスをそのまま返す`` () =
    format None "/some/absolute/path" |> should equal "/some/absolute/path"
