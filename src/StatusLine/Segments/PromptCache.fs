module StatusLine.Segments.PromptCache

open System
open System.Drawing
open StatusLine.Types.Context
open StatusLine.Types.App

let private warmLabel = "🔥 "
let private coldLabel = "🧊 "
let private dimCoral = Color.FromArgb(120, 64, 64) // rgb(120, 64, 64)
let private brightCoral = Color.LightCoral // rgb(240, 128, 128)

/// ヒット率が低いほど暗く、高いほど明るいコーラルにする。
let private coralByHitRatio (hitRatio: float) : Color =
    let mix (dim: byte) (bright: byte) =
        int (Math.Round(float dim + (float bright - float dim) * hitRatio))

    Color.FromArgb(mix dimCoral.R brightCoral.R, mix dimCoral.G brightCoral.G, mix dimCoral.B brightCoral.B)

/// `/usage` の `Prompt cache (main)` 行と同じ値になるよう、0.5 は常に切り上げる。
let private hitRatioText (cache: PromptCache) : string =
    cache.HitRatio
    |> Option.map (fun hitRatio -> sprintf "%d%% " (int (Math.Round(hitRatio * 100.0, MidpointRounding.AwayFromZero))))
    |> Option.defaultValue ""

/// 期限は補足情報なので、日時に変換できない値でもステータスライン全体を落とさず省く。
let private untilText (cache: PromptCache) : string =
    cache.ExpiresAt
    |> Option.bind StatusLine.Utils.DateTime.tryUnixTimeToLocalDateTimeOffset
    |> Option.map (fun expiresAt -> sprintf " until %s" (expiresAt.ToString "HH:mm"))
    |> Option.defaultValue ""

let private formatWarm (cache: PromptCache) : Segment = [
    { Text = warmLabel; Color = None }
    {
        Text = hitRatioText cache + "warm" + untilText cache
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

/// 範囲外のヒット率は `Color.FromArgb` の例外や 100% を超える表示につながるため、0 から 1 に収める。
let private clampHitRatio (cache: PromptCache) : PromptCache = {
    cache with
        HitRatio = cache.HitRatio |> Option.map (fun hitRatio -> Math.Clamp(hitRatio, 0.0, 1.0))
}

let format (promptCache: PromptCache option) : Segment option =
    promptCache
    |> Option.filter (fun cache -> cache.CachingObserved <> Some false)
    |> Option.map clampHitRatio
    |> Option.bind (fun cache ->
        cache.Warm
        |> Option.map (fun warm -> if warm then formatWarm cache else formatCold cache))
