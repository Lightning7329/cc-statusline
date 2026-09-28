namespace StatusLine.Tests.Utils.ErrorLogTests

open System
open System.IO
open Xunit
open FsUnit.Xunit
open StatusLine.Utils.ErrorLog

module ResolvePath =

    [<Fact>]
    let ``XDG_STATE_HOME が設定されていればその配下を返す`` () =
        let getEnv =
            function
            | "XDG_STATE_HOME" -> Some "/state"
            | "HOME" -> Some "/home/user"
            | _ -> None

        resolvePath getEnv |> should equal (Some "/state/cc-statusline/error.log")

    [<Fact>]
    let ``XDG_STATE_HOME が未設定なら HOME の .local/state 配下を返す`` () =
        let getEnv =
            function
            | "HOME" -> Some "/home/user"
            | _ -> None

        resolvePath getEnv
        |> should equal (Some "/home/user/.local/state/cc-statusline/error.log")

    [<Fact>]
    let ``XDG_STATE_HOME が相対パスなら無視して HOME にフォールバックする`` () =
        let getEnv =
            function
            | "XDG_STATE_HOME" -> Some ".state"
            | "HOME" -> Some "/home/user"
            | _ -> None

        resolvePath getEnv
        |> should equal (Some "/home/user/.local/state/cc-statusline/error.log")

    [<Fact>]
    let ``どちらも未設定なら None を返す`` () =
        resolvePath (fun _ -> None) |> should equal None

module FormatEntry =

    let private now = DateTimeOffset(2026, 9, 29, 12, 34, 56, TimeSpan.FromHours 9.0)

    [<Fact>]
    let ``タイムスタンプ・例外・入力を含む`` () =
        let entry =
            formatEntry now (InvalidOperationException "boom") (Some """{"foo":1}""")

        entry |> should haveSubstring "2026-09-29T12:34:56.0000000+09:00"
        entry |> should haveSubstring "System.InvalidOperationException: boom"
        entry |> should haveSubstring """{"foo":1}"""

    [<Fact>]
    let ``入力を読む前の失敗なら (not read) と記録する`` () =
        formatEntry now (Exception "boom") None |> should haveSubstring "(not read)"

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
            append path "first"
            File.ReadAllText path |> should equal "first")

    [<Fact>]
    let ``既存ファイルに追記する`` () =
        withTempDir (fun dir ->
            let path = Path.Combine(dir, "error.log")
            append path "first"
            append path "second"
            File.ReadAllText path |> should equal "firstsecond")

    [<Fact>]
    let ``サイズ上限を超えていたら切り詰めてから書き込む`` () =
        withTempDir (fun dir ->
            let path = Path.Combine(dir, "error.log")
            Directory.CreateDirectory dir |> ignore
            File.WriteAllText(path, String('x', 1024 * 1024 + 1))
            append path "new"
            File.ReadAllText path |> should equal "new")

    [<Fact>]
    let ``書き込めないパスでも例外を投げない`` () =
        withTempDir (fun dir ->
            Directory.CreateDirectory dir |> ignore
            let blocker = Path.Combine(dir, "file")
            File.WriteAllText(blocker, "")
            append (Path.Combine(blocker, "error.log")) "x")
