module StatusLine.Segments.PromptCache

open System
open System.Drawing
open StatusLine.Types.Context
open StatusLine.Types.App

let private warmLabel = "🔥 "
let private coldLabel = "🧊 "
/// rgb(120, 64, 64)
let private dimCoral = Color.FromArgb(120, 64, 64)
/// rgb(240, 128, 128)
let private brightCoral = Color.LightCoral

/// ヒット率が低いほど暗く、高いほど明るいコーラルにする。
let private coralByHitRatio (hitRatio: float) : Color =
    let mix (dim: byte) (bright: byte) =
        int (Math.Round(float dim + float (int bright - int dim) * hitRatio))

    Color.FromArgb(mix dimCoral.R brightCoral.R, mix dimCoral.G brightCoral.G, mix dimCoral.B brightCoral.B)

/// `/usage` の `Prompt cache (main)` 行と同じ値になるよう、0.5 は常に切り上げる。
let private hitRatioText (cache: PromptCache) : string =
    cache.HitRatio
    |> Option.map (fun hitRatio -> sprintf "%d%% " (int (Math.Round(hitRatio * 100.0, MidpointRounding.AwayFromZero))))
    |> Option.defaultValue ""

let private untilText (expiresAt: int64) : string =
    expiresAt
    |> StatusLine.Utils.DateTime.unixTimeToLocalDateTimeOffset
    |> _.ToString("HH:mm")
    |> sprintf " until %s"

let private formatWarm (cache: PromptCache) : Segment = [
    { Text = warmLabel; Color = None }
    {
        Text =
            hitRatioText cache
            + "warm"
            + (cache.ExpiresAt |> Option.map untilText |> Option.defaultValue "")
        Color =
            cache.HitRatio
            |> Option.map coralByHitRatio
            |> Option.defaultValue brightCoral
            |> Some
    }
]

/// ヒット率はセッション累計で次の要求の判断材料にならないため、ラベル以外を灰色にして沈める。
/// TTL 切れの cold でも過去の expires_at が届くが、until は表示しない。
let private formatCold (cache: PromptCache) : Segment = [
    { Text = coldLabel; Color = None }
    {
        Text = hitRatioText cache + "cold"
        Color = Some Color.Gray
    }
]

let format (promptCache: PromptCache option) : Segment option =
    promptCache
    |> Option.filter (fun cache -> cache.CachingObserved <> Some false)
    |> Option.bind (fun cache ->
        match cache.Warm with
        | Some true -> Some(formatWarm cache)
        | Some false -> Some(formatCold cache)
        | None -> None)
