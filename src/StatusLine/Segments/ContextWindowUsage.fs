module StatusLine.Segments.ContextWindowUsage

open StatusLine.Types.App
open StatusLine.Types.Context
open StatusLine.Utils.OptionBuilder

let private brailles = [| " "; "⣀"; "⣄"; "⣤"; "⣦"; "⣶"; "⣷"; "⣿" |]

let formatBar usedPercentage =
    let clamped = System.Math.Clamp(usedPercentage, 0, 100)

    if clamped = 100 then
        brailles[brailles.Length - 1] |> String.replicate 10
    else
        let fullCount = clamped / 10
        let remainder = clamped % 10
        let partialIndex = float (remainder * brailles.Length) / 10.0 |> int

        (brailles[brailles.Length - 1] |> String.replicate fullCount)
        + brailles[partialIndex]
        + (brailles[0] |> String.replicate (10 - 1 - fullCount))

let format (contextWindow: ContextWindow) : Segment option = option {
    let! usage = contextWindow.UsedPercentage
    let percentage = sprintf "%d%%" usage
    let bar = formatBar usage
    let color = StatusLine.Utils.Color.percentageToColor (float usage)

    return [
        { Text = "ctx "; Color = None }
        {
            Text = sprintf "%s %s" bar percentage
            Color = Some color
        }
    ]
}
