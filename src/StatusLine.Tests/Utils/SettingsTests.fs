namespace StatusLine.Tests.Utils.SettingsTests

open System
open Xunit
open FsUnit.Xunit
open StatusLine.Utils.Settings

module Load =

    [<Fact>]
    let ``getEnv の HOME を Home にマップする`` () =
        let getEnv =
            function
            | "HOME" -> Some "/home/user"
            | _ -> None

        (load getEnv).Home |> should equal (Some "/home/user")

module EnvReader =

    let private testKey = "STATUSLINE_TEST_ENVREADER"

    let private withEnv value f =
        let original = Environment.GetEnvironmentVariable testKey

        try
            Environment.SetEnvironmentVariable(testKey, value)
            f ()
        finally
            Environment.SetEnvironmentVariable(testKey, original)

    [<Fact>]
    let ``値が設定されている場合Someを返す`` () =
        withEnv "value" (fun () -> envReader testKey |> should equal (Some "value"))

    [<Fact>]
    let ``空文字の場合Noneに正規化する`` () =
        withEnv "" (fun () -> envReader testKey |> should equal None)

    [<Fact>]
    let ``未設定の場合Noneを返す`` () =
        withEnv null (fun () -> envReader testKey |> should equal None)
