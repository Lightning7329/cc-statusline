module StatusLine.Tests.Utils.SettingsTests

open System
open Xunit
open FsUnit.Xunit
open StatusLine.Utils.Settings

[<Fact>]
let ``loadはgetEnvのWORKSPACE_ROOTをWorkspaceRootにマップする`` () =
    let getEnv =
        function
        | "WORKSPACE_ROOT" -> Some "/workspaces/proj"
        | _ -> None

    (load getEnv).WorkspaceRoot |> should equal (Some "/workspaces/proj")

[<Fact>]
let ``envReaderは値が設定されている場合Someを返す`` () =
    let key = "STATUSLINE_TEST_ENVREADER"
    let original = Environment.GetEnvironmentVariable key

    try
        Environment.SetEnvironmentVariable(key, "value")
        envReader key |> should equal (Some "value")
    finally
        Environment.SetEnvironmentVariable(key, original)

[<Fact>]
let ``envReaderは空文字の場合Noneに正規化する`` () =
    let key = "STATUSLINE_TEST_ENVREADER"
    let original = Environment.GetEnvironmentVariable key

    try
        Environment.SetEnvironmentVariable(key, "")
        envReader key |> should equal None
    finally
        Environment.SetEnvironmentVariable(key, original)

[<Fact>]
let ``envReaderは未設定の場合Noneを返す`` () =
    let key = "STATUSLINE_TEST_ENVREADER_UNSET"
    let original = Environment.GetEnvironmentVariable key

    try
        Environment.SetEnvironmentVariable(key, null)
        envReader key |> should equal None
    finally
        Environment.SetEnvironmentVariable(key, original)
