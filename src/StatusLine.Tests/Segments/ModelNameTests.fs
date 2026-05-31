module StatusLine.Tests.Segments.ModelNameTests

open Xunit
open FsUnit.Xunit
open StatusLine.Types.Context
open StatusLine.Segments.ModelName

[<Fact>]
let ``claude-プレフィックスを除去する`` () =
    format
        {
            Id = "claude-opus-4-6"
            DisplayName = "Opus"
        }
        None
    |> should equal "opus-4-6"

[<Fact>]
let ``claude-プレフィックスがない場合はそのまま返す`` () =
    format { Id = "gpt-4"; DisplayName = "GPT-4" } None |> should equal "gpt-4"

[<Fact>]
let ``claude-haiku系モデルも正しく変換する`` () =
    format
        {
            Id = "claude-haiku-4-5"
            DisplayName = "Haiku"
        }
        None
    |> should equal "haiku-4-5"

[<Fact>]
let ``effort がある場合は括弧付きで level を併記する`` () =
    format
        {
            Id = "claude-opus-4-8"
            DisplayName = "Opus"
        }
        (Some { Level = "high" })
    |> should equal "opus-4-8 (high)"

[<Fact>]
let ``effort がない場合は level を付けない`` () =
    format
        {
            Id = "claude-opus-4-8"
            DisplayName = "Opus"
        }
        None
    |> should equal "opus-4-8"
