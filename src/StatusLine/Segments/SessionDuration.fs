module StatusLine.Segments.SessionDuration

open System.Drawing
open StatusLine.Types.Context
open StatusLine.Types.App
open StatusLine.Utils.DateTime

let private blue = Color.FromArgb(127, 179, 212)

let format (cost: Cost) : Segment option =
    Some [
        { Text = "⏳ "; Color = None }
        {
            Text = formatDurationMs cost.TotalDurationMs
            Color = Some blue
        }
    ]
