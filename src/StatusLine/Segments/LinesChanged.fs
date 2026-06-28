module StatusLine.Segments.LinesChanged

open System.Drawing
open StatusLine.Types.Context
open StatusLine.Types.App

let format (cost: Cost) : Segment = [
    {
        Text = sprintf "+%d" cost.TotalLinesAdded
        Color = Some Color.LightGreen
    }
    { Text = "/"; Color = None }
    {
        Text = sprintf "-%d" cost.TotalLinesRemoved
        Color = Some Color.LightCoral
    }
]
