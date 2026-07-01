module StatusLine.Segments.CostDisplay

open System.Drawing
open StatusLine.Types.Context
open StatusLine.Types.App

let private orange = Color.FromArgb(212, 162, 127)

let format (cost: Cost) : Segment option =
    Some [
        {
            Text = sprintf "$%.4f" cost.TotalCostUsd
            Color = Some orange
        }
    ]
