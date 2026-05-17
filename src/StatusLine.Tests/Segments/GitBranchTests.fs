module StatusLine.Tests.Segments.GitBranchTests

open Xunit
open FsUnit.Xunit
open StatusLine.Segments.GitBranch

let private icon = char 0xE0A0 |> string

[<Fact>]
let ``通常のブランチ名をフォーマットする`` () =
    formatBranch "main" |> should equal $"{icon} main"

[<Fact>]
let ``スラッシュ付きブランチ名をフォーマットする`` () =
    formatBranch "feature/add-git-segment"
    |> should equal $"{icon} feature/add-git-segment"

[<Fact>]
let ``短縮コミットハッシュをフォーマットする`` () =
    formatBranch "a1b2c3d" |> should equal $"{icon} a1b2c3d"

[<Fact>]
let ``formatWithRunnerがSomeのときフォーマットする`` () =
    let runner _ = Some "main"
    formatWithRunner runner "/some/dir" |> should equal (Some $"{icon} main")

[<Fact>]
let ``formatWithRunnerがNoneのときNoneを返す`` () =
    let runner _ = None
    formatWithRunner runner "/some/dir" |> should equal None
