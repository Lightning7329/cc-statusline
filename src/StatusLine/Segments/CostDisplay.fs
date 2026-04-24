module StatusLine.Segments.CostDisplay

open StatusLine.Types

let format (cost: Cost) : Segment = {
    Text = sprintf "$%.4f" cost.TotalCostUsd
    Color = None
}
