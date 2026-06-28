module StatusLine.Segments.CostDisplay

open StatusLine.Types.Context
open StatusLine.Types.App

let format (cost: Cost) : Segment option =
    Some [
        {
            Text = sprintf "$%.4f" cost.TotalCostUsd
            Color = None
        }
    ]
