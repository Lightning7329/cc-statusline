module StatusLine.Segments.PromptCache

open System
open System.Drawing
open StatusLine.Types.Context
open StatusLine.Types.App

/// `/usage` の `Prompt cache (main)` 行と同じ値になるよう、0.5 は常に切り上げる。
let private formatHitRatio (hitRatio: float) : Span = {
    Text = sprintf "%d%% " (int (Math.Round(hitRatio * 100.0, MidpointRounding.AwayFromZero)))
    Color = Some(StatusLine.Utils.Color.percentageToColor ((1.0 - hitRatio) * 100.0))
}

let private formatUntil (expiresAt: int64) : string =
    expiresAt
    |> StatusLine.Utils.DateTime.unixTimeToLocalDateTimeOffset
    |> _.ToString("HH:mm")
    |> sprintf " until %s"

/// TTL 切れの cold でも過去の expires_at が届くため、until は warm のときだけ表示する。
let private formatState (cache: PromptCache) : Span option =
    match cache.Warm with
    | Some true ->
        Some {
            Text = "warm" + (cache.ExpiresAt |> Option.map formatUntil |> Option.defaultValue "")
            Color = Some Color.LightGreen
        }
    | Some false ->
        Some {
            Text = "cold"
            Color = Some Color.LightCoral
        }
    | None -> None

let format (promptCache: PromptCache option) : Segment option =
    promptCache
    |> Option.filter (fun cache -> cache.CachingObserved <> Some false)
    |> Option.bind (fun cache ->
        formatState cache
        |> Option.map (fun state -> [
            { Text = "cache "; Color = None }
            yield! cache.HitRatio |> Option.map formatHitRatio |> Option.toList
            state
        ]))
