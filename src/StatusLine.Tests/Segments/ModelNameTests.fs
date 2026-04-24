module StatusLine.Tests.Segments.ModelNameTests

open Xunit
open FsUnit.Xunit
open StatusLine.Types
open StatusLine.Segments.ModelName

[<Fact>]
let ``claude-プレフィックスを除去する`` () =
    format {
        Id = "claude-opus-4-6"
        DisplayName = "Opus"
    }
    |> should equal "opus-4-6"

[<Fact>]
let ``claude-プレフィックスがない場合はそのまま返す`` () =
    format { Id = "gpt-4"; DisplayName = "GPT-4" } |> should equal "gpt-4"

[<Fact>]
let ``claude-haiku系モデルも正しく変換する`` () =
    format {
        Id = "claude-haiku-4-5"
        DisplayName = "Haiku"
    }
    |> should equal "haiku-4-5"
