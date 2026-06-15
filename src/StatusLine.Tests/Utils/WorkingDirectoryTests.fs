module StatusLine.Tests.Utils.WorkingDirectoryTests

open Xunit
open FsUnit.Xunit
open StatusLine.Utils.WorkingDirectory

[<Fact>]
let ``absolutePathとWORKSPACE_ROOTが一致する場合、WORKSPACE_ROOTのディレクトリ名を返す`` () =
    relativePath (Some "/workspaces/my-project") "/workspaces/my-project"
    |> should equal "my-project"

[<Fact>]
let ``absolutePathがWORKSPACE_ROOT配下の場合、ディレクトリ名プラス相対パスを返す`` () =
    relativePath (Some "/workspaces/my-project") "/workspaces/my-project/foo/bar"
    |> should equal "my-project/foo/bar"

[<Fact>]
let ``absolutePathがWORKSPACE_ROOTの兄弟ディレクトリの場合、my-project/../other-projectを返す`` () =
    relativePath (Some "/workspaces/my-project") "/workspaces/other-project"
    |> should equal "my-project/../other-project"

[<Fact>]
let ``WORKSPACE_ROOTが未設定の場合、absolutePathをそのまま返す`` () =
    relativePath None "/some/absolute/path" |> should equal "/some/absolute/path"

[<Fact>]
let ``absolutePathが全く別ツリーの場合、相対表記で返す`` () =
    relativePath (Some "/workspaces/my-project") "/home/user/docs"
    |> should equal "my-project/../../home/user/docs"

[<Fact>]
let ``WORKSPACE_ROOTの末尾にスラッシュがある場合でもディレクトリ名を正しく返す`` () =
    relativePath (Some "/workspaces/my-project/") "/workspaces/my-project/foo/bar"
    |> should equal "my-project/foo/bar"
