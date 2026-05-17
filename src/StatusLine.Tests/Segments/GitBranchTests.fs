module StatusLine.Tests.Segments.GitBranchTests

open Xunit
open FsUnit.Xunit
open StatusLine.Segments.GitBranch

let private icon = char 0xE0A0 |> string

module FormatBranch =
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

module FormatWithRunner =
    [<Fact>]
    let ``Someのときフォーマットする`` () =
        let runner _ = Some "main"
        formatWithRunner runner "/some/dir" |> should equal (Some $"{icon} main")

    [<Fact>]
    let ``Noneのときはそのまま返す`` () =
        let runner _ = None
        formatWithRunner runner "/some/dir" |> should equal None
