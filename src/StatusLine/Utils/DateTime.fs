module StatusLine.Utils.DateTime

open System

let unixTimeToLocalDateTimeOffset unixTime =
    unixTime |> DateTimeOffset.FromUnixTimeSeconds |> _.ToLocalTime()

/// ミリ秒を `1h 12m 3s` 形式の経過時間文字列にする。
/// 上位のゼロ単位は省略し、下位はゼロでも残す（`< 1h` → `12m 3s`、`< 1m` → `45s`）。
/// 日単位は作らず時間は青天井、ゼロ埋めはしない。
let formatDurationMs (totalMs: int) : string =
    let totalSeconds = max 0 (totalMs / 1000)
    let hours = totalSeconds / 3600
    let minutes = (totalSeconds % 3600) / 60
    let seconds = totalSeconds % 60

    if hours > 0 then
        sprintf "%dh %dm %ds" hours minutes seconds
    elif minutes > 0 then
        sprintf "%dm %ds" minutes seconds
    else
        sprintf "%ds" seconds
