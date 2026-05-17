module StatusLine.Tests.Segments.GitBranchTests

open Xunit
open FsUnit.Xunit
open StatusLine.Segments.GitBranch

let private icon = ""

[<Fact>]
let ``通常のブランチ名をそのまま返す`` () =
    formatBranch "main" |> should equal $"{icon} main"

[<Fact>]
let ``スラッシュ付きブランチ名をそのまま返す`` () =
    formatBranch "feature/add-git-segment"
    |> should equal $"{icon} feature/add-git-segment"

[<Fact>]
let ``HEADのときdetachedを返す`` () =
    formatBranch "HEAD" |> should equal $"{icon} detached"

[<Fact>]
let ``formatWithRunnerがSomeのときブランチ名をフォーマットする`` () =
    let runner _ = Some "main"
    formatWithRunner runner "/some/dir" |> should equal (Some $"{icon} main")

[<Fact>]
let ``formatWithRunnerがNoneのときNoneを返す`` () =
    let runner _ = None
    formatWithRunner runner "/some/dir" |> should equal None

[<Fact>]
let ``formatWithRunnerがHEADを返すときdetachedにフォーマットする`` () =
    let runner _ = Some "HEAD"
    formatWithRunner runner "/some/dir" |> should equal (Some $"{icon} detached")
