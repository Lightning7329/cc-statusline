namespace StatusLine.Tests.Utils.OptionBuilderTests

open Xunit
open FsUnit.Xunit
open StatusLine.Utils.OptionBuilder

module Bind =

    [<Fact>]
    let ``Someをletバインドすると後続が評価される`` () =
        let result = option {
            let! x = Some 1
            let! y = Some 2
            return x + y
        }

        result |> should equal (Some 3)

    [<Fact>]
    let ``Noneをletバインドすると短絡して後続を評価しない`` () =
        let mutable evaluated = false

        let result = option {
            let! _ = None
            evaluated <- true
            return 1
        }

        result |> should equal None
        evaluated |> should be False

module Return =

    [<Fact>]
    let ``値をSomeで包む`` () =
        let result = option { return 42 }
        result |> should equal (Some 42)

module ReturnFrom =

    [<Fact>]
    let ``Someをそのまま返す`` () =
        let result = option { return! Some 5 }
        result |> should equal (Some 5)

    [<Fact>]
    let ``Noneをそのまま返す`` () =
        let result = option { return! None }
        result |> should equal None

module Zero =

    [<Fact>]
    let ``return省略時はNoneになる`` () =
        let result = option {
            if false then
                return 1
        }

        result |> should equal None
