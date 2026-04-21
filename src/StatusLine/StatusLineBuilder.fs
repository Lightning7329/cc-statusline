module StatusLine.StatusLineBuilder

open System.Text.Json
open System.Text.Json.Serialization
open StatusLine.Utils
open StatusLine.Types

let private jsonOptions =
    let opts = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)
    opts.Converters.Add(JsonFSharpConverter())
    opts

/// <summary>
/// JSON文字列をContext型に変換する
/// </summary>
/// <param name="input">JSON文字列</param>
/// <returns>Context型のデータ</returns>
let parseInput (input: string) =
    JsonSerializer.Deserialize<Context>(input, jsonOptions)

/// <summary>
/// Context型からステータスラインの文字列を構築する
/// </summary>
/// <param name="c">Context型のデータ</param>
/// <returns>ステータスラインの文字列</returns>
let build (c: Context) =
    let elements = [
        c.Cwd
        c.Model.Id.Replace ("claude-", "")
        sprintf "$%.4f" c.Cost.TotalCostUsd
        sprintf "%1.1f%% (reset at %s)"
            c.RateLimits.FiveHour.UsedPercentage
            (c.RateLimits.FiveHour.ResetsAt |> DateTime.UnixTimeToJstDateTimeOffset |> _.ToString("HH:mm"))
        sprintf "%1.1f%% (reset at %s)"
            c.RateLimits.SevenDay.UsedPercentage
            (c.RateLimits.SevenDay.ResetsAt |> DateTime.UnixTimeToJstDateTimeOffset |> _.ToString("MM/dd HH:mm"))
    ]

    String.concat " | " elements

let buildFromInput = parseInput >> build
