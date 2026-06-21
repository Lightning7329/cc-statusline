module StatusLine.Tests.Utils.WorkingDirectoryTests

open Xunit
open FsUnit.Xunit
open StatusLine.Utils.WorkingDirectory

[<Fact>]
let ``project_dir と一致する場合、project_dir のディレクトリ名を返す`` () =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = []
    let home = None
    let absolutePath = "/workspaces/my-project"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "my-project"

[<Fact>]
let ``project_dir 配下の場合、ディレクトリ名プラス相対パスを返す`` () =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = []
    let home = None
    let absolutePath = "/workspaces/my-project/foo/bar"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "my-project/foo/bar"

[<Fact>]
let ``project_dir 末尾にスラッシュがある場合でもディレクトリ名を正しく返す`` () =
    // Arrange
    let projectDir = "/workspaces/my-project/"
    let addedDirs = []
    let home = None
    let absolutePath = "/workspaces/my-project/foo/bar"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "my-project/foo/bar"

[<Fact>]
let ``project_dir 外で added_dirs のいずれかに一致する場合、その added_dir のディレクトリ名で短縮する`` () =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = [ "/workspaces/other-project" ]
    let home = None
    let absolutePath = "/workspaces/other-project/src"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "other-project/src"

[<Theory>]
[<InlineData(null)>]
[<InlineData("/workspaces/unrelated")>]
let ``project_dir 外かつ added_dirs 外で HOME 配下の場合、~ で短縮する`` (unrelatedAddedDir: string) =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = unrelatedAddedDir |> Option.ofObj |> Option.toList
    let home = Some "/home/user"
    let absolutePath = "/home/user/docs/note.md"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "~/docs/note.md"

[<Fact>]
let ``HOME と一致する場合、~ のみを返す`` () =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = []
    let home = Some "/home/user"
    let absolutePath = "/home/user"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "~"

[<Fact>]
let ``HOME 末尾にスラッシュがある場合でも ~ で短縮する`` () =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = []
    let home = Some "/home/user/"
    let absolutePath = "/home/user/docs"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "~/docs"

[<Theory>]
[<InlineData(null)>]
[<InlineData("/home/user")>]
let ``project_dir / added_dirs / HOME のいずれにも該当しない場合、絶対パスをそのまま返す`` (homeDir: string) =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = []
    let home = homeDir |> Option.ofObj
    let absolutePath = "/var/log/foo.log"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "/var/log/foo.log"

[<Fact>]
let ``project_dir と added_dirs の双方に該当しうる場合、project_dir を優先する`` () =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = [ "/workspaces" ]
    let home = None
    let absolutePath = "/workspaces/my-project/src"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "my-project/src"

[<Fact>]
let ``added_dirs に複数あり後者が一致する場合、後者で短縮する`` () =
    // Arrange
    let projectDir = "/workspaces/my-project"
    let addedDirs = [ "/a"; "/b/c" ]
    let home = None
    let absolutePath = "/b/c/file"

    // Act
    let result = shorten projectDir addedDirs home absolutePath

    // Assert
    result |> should equal "c/file"
