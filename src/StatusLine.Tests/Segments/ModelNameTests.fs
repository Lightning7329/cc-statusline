module StatusLine.Tests.Segments.ModelNameTests

open Xunit
open FsUnit.Xunit
open StatusLine.Types.App
open StatusLine.Types.Context
open StatusLine.Segments.ModelName

let private segmentText seg =
    seg |> Option.get |> List.map _.Text |> String.concat ""

[<Fact>]
let ``claude-プレフィックスを除去する`` () =
    format
        {
            Id = "claude-opus-4-6"
            DisplayName = "Opus"
        }
        None
        None
    |> segmentText
    |> should equal "opus-4-6"

[<Fact>]
let ``claude-プレフィックスがない場合はそのまま返す`` () =
    format { Id = "gpt-4"; DisplayName = "GPT-4" } None None
    |> segmentText
    |> should equal "gpt-4"

[<Fact>]
let ``claude-haiku系モデルも正しく変換する`` () =
    format
        {
            Id = "claude-haiku-4-5"
            DisplayName = "Haiku"
        }
        None
        None
    |> segmentText
    |> should equal "haiku-4-5"

[<Fact>]
let ``effort がある場合は括弧付きで level を併記する`` () =
    format
        {
            Id = "claude-opus-4-8"
            DisplayName = "Opus"
        }
        (Some { Level = "high" })
        None
    |> segmentText
    |> should equal "opus-4-8 (high)"

[<Fact>]
let ``effort がない場合は level を付けない`` () =
    format
        {
            Id = "claude-opus-4-8"
            DisplayName = "Opus"
        }
        None
        None
    |> segmentText
    |> should equal "opus-4-8"


[<Fact>]
let ``effort と fast mode が両方あるときはカンマ区切りで併記する`` () =
    format
        {
            Id = "claude-opus-5-5"
            DisplayName = "Opus"
        }
        (Some { Level = "high" })
        (Some true)
    |> segmentText
    |> should equal "opus-5-5 (high, fast)"


[<Fact>]
let ``effort がなく fast mode が有効なときは fast だけを括弧付きで表示する`` () =
    format
        {
            Id = "claude-opus-5-5"
            DisplayName = "Opus"
        }
        None
        (Some true)
    |> segmentText
    |> should equal "opus-5-5 (fast)"


[<Fact>]
let ``fast mode が無効なときは fast を付けない`` () =
    format
        {
            Id = "claude-opus-5-5"
            DisplayName = "Opus"
        }
        (Some { Level = "high" })
        (Some false)
    |> segmentText
    |> should equal "opus-5-5 (high)"


[<Fact>]
let ``effort がなく fast mode が無効なときはモデル名だけを表示する`` () =
    format
        {
            Id = "claude-opus-5-5"
            DisplayName = "Opus"
        }
        None
        (Some false)
    |> segmentText
    |> should equal "opus-5-5"
