namespace StatusLine.Tests.Utils.ErrorLogTests

open System
open System.IO
open Xunit
open FsUnit.Xunit
open StatusLine.Utils.ErrorLog

module ResolvePath =

    let private tempDir = "/tmp"

    [<Fact>]
    let ``XDG_STATE_HOME が設定されていればその配下を返す`` () =
        let getEnv =
            function
            | "XDG_STATE_HOME" -> Some "/state"
            | "HOME" -> Some "/home/user"
            | _ -> None

        resolvePath getEnv tempDir |> should equal "/state/cc-statusline/error.log"

    [<Fact>]
    let ``XDG_STATE_HOME が未設定なら HOME の .local/state 配下を返す`` () =
        let getEnv =
            function
            | "HOME" -> Some "/home/user"
            | _ -> None

        resolvePath getEnv tempDir
        |> should equal "/home/user/.local/state/cc-statusline/error.log"

    [<Fact>]
    let ``XDG_STATE_HOME が相対パスなら無視して HOME にフォールバックする`` () =
        let getEnv =
            function
            | "XDG_STATE_HOME" -> Some ".state"
            | "HOME" -> Some "/home/user"
            | _ -> None

        resolvePath getEnv tempDir
        |> should equal "/home/user/.local/state/cc-statusline/error.log"

    [<Fact>]
    let ``HOME が相対パスなら無視して一時ディレクトリにフォールバックする`` () =
        let getEnv =
            function
            | "HOME" -> Some "home"
            | _ -> None

        resolvePath getEnv tempDir |> should equal "/tmp/cc-statusline/error.log"

    [<Fact>]
    let ``どちらも未設定なら一時ディレクトリ配下を返す`` () =
        resolvePath (fun _ -> None) tempDir
        |> should equal "/tmp/cc-statusline/error.log"

module FormatEntry =

    let private now = DateTimeOffset(2026, 9, 29, 12, 34, 56, TimeSpan.FromHours 9.0)

    [<Fact>]
    let ``タイムスタンプ・見出し・詳細・入力を含む`` () =
        // Arrange
        let heading = "missing or invalid field"
        let detail = "Path: $.cwd"
        let input = Some """{"foo":1}"""

        // Act
        let entry = formatEntry now heading detail input

        // Assert
        entry |> should haveSubstring "2026-09-29T12:34:56.0000000+09:00"
        entry |> should haveSubstring "[missing or invalid field]\nPath: $.cwd"
        entry |> should haveSubstring """{"foo":1}"""

    [<Fact>]
    let ``入力を読む前の失敗なら (not read) と記録する`` () =
        // Arrange
        let heading = "exception"
        let detail = "boom"
        let input = None

        // Act
        let entry = formatEntry now heading detail input

        // Assert
        entry |> should haveSubstring "(not read)"

    [<Fact>]
    let ``入力が空文字列なら (empty) と記録する`` () =
        // Arrange
        let heading = "invalid JSON"
        let detail = "boom"
        let input = Some ""

        // Act
        let entry = formatEntry now heading detail input

        // Assert
        entry |> should haveSubstring "(empty)"

module Append =

    let private withTempDir (f: string -> unit) =
        let dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())

        try
            f dir
        finally
            if Directory.Exists dir then
                Directory.Delete(dir, true)

    [<Fact>]
    let ``ディレクトリがなければ作成して書き込む`` () =
        withTempDir (fun dir ->
            let path = Path.Combine(dir, "nested", "error.log")
            append path "first" |> should equal true
            File.ReadAllText path |> should equal "first")

    [<Fact>]
    let ``作成したディレクトリは所有者のみアクセス可能`` () =
        withTempDir (fun dir ->
            let path = Path.Combine(dir, "nested", "error.log")
            append path "first" |> ignore

            File.GetUnixFileMode(Path.GetDirectoryName path)
            |> should equal (UnixFileMode.UserRead ||| UnixFileMode.UserWrite ||| UnixFileMode.UserExecute))

    [<Fact>]
    let ``既存ファイルに追記する`` () =
        withTempDir (fun dir ->
            let path = Path.Combine(dir, "error.log")
            append path "first" |> ignore
            append path "second" |> ignore
            File.ReadAllText path |> should equal "firstsecond")

    [<Fact>]
    let ``サイズ上限を超えていたら切り詰めてから書き込む`` () =
        withTempDir (fun dir ->
            let path = Path.Combine(dir, "error.log")
            Directory.CreateDirectory dir |> ignore
            File.WriteAllText(path, String('x', 1024 * 1024 + 1))
            append path "new" |> ignore
            File.ReadAllText path |> should equal "new")

    [<Fact>]
    let ``書き込めないパスでも例外を投げず false を返す`` () =
        withTempDir (fun dir ->
            Directory.CreateDirectory dir |> ignore
            let blocker = Path.Combine(dir, "file")
            File.WriteAllText(blocker, "")
            append (Path.Combine(blocker, "error.log")) "x" |> should equal false)
