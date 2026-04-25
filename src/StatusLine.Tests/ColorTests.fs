module StatusLine.Tests.ColorTests

open Xunit
open FsUnit.Xunit
open StatusLine.Color

[<Fact>]
let ``0.0は緑(0, 255, 0)を返す`` () =
    let c = percentageToColor 0.0
    int c.R |> should equal 0
    int c.G |> should equal 255
    int c.B |> should equal 0

[<Fact>]
let ``100.0は赤(255, 0, 0)を返す`` () =
    let c = percentageToColor 100.0
    int c.R |> should equal 255
    int c.G |> should equal 0
    int c.B |> should equal 0

[<Fact>]
let ``50.0はOKLCH補間による中間色を返す`` () =
    let c = percentageToColor 50.0
    int c.R |> should equal 221
    int c.G |> should equal 162
    int c.B |> should equal 0

[<Fact>]
let ``0未満はクランプして緑を返す`` () =
    let c = percentageToColor -10.0
    int c.R |> should equal 0
    int c.G |> should equal 255
    int c.B |> should equal 0

[<Fact>]
let ``100超はクランプして赤を返す`` () =
    let c = percentageToColor 110.0
    int c.R |> should equal 255
    int c.G |> should equal 0
    int c.B |> should equal 0
