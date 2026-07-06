module StatusLine.Utils.DateTime

open System

let unixTimeToLocalDateTimeOffset unixTime =
    unixTime |> DateTimeOffset.FromUnixTimeSeconds |> _.ToLocalTime()

/// 経過時間を時・分・秒に分解した値。
/// 日単位は作らず、Hours は青天井（24 以上にもなり得る）。
type Duration = {
    Hours: int
    Minutes: int
    Seconds: int
}

/// ミリ秒を時・分・秒へ分解する。負値は 0 に丸める。
let msToDuration (totalMs: int) : Duration =
    let totalSeconds = max 0 (totalMs / 1000)

    {
        Hours = totalSeconds / 3600
        Minutes = totalSeconds % 3600 / 60
        Seconds = totalSeconds % 60
    }
