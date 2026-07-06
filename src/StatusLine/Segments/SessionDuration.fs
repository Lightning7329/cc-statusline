module StatusLine.Segments.SessionDuration

open System.Drawing
open StatusLine.Types.Context
open StatusLine.Types.App
open StatusLine.Utils

let private blue = Color.FromArgb(127, 179, 212)

/// Duration を `1h 12m 3s` 形式にする。
/// 上位のゼロ単位は省略し、下位はゼロでも残す（`< 1h` → `12m 3s`、`< 1m` → `45s`）。
/// ゼロ埋めはしない。
let private formatDuration (d: DateTime.Duration) : string =
    if d.Hours > 0 then
        sprintf "%dh %dm %ds" d.Hours d.Minutes d.Seconds
    elif d.Minutes > 0 then
        sprintf "%dm %ds" d.Minutes d.Seconds
    else
        sprintf "%ds" d.Seconds

let format (cost: Cost) : Segment option =
    Some [
        { Text = "⏳ "; Color = None }
        {
            Text = cost.TotalDurationMs |> DateTime.msToDuration |> formatDuration
            Color = Some blue
        }
    ]
