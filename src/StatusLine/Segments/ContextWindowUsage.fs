module StatusLine.Segments.ContextWindowUsage

open StatusLine.Types.App
open StatusLine.Types.Context

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

/// Formats a token count compactly: as is below 1,000, in whole `k` below 1,000,000,
/// and in `M` with one decimal place (trailing `.0` dropped) from 1,000,000.
/// Fractions are truncated so the display never overstates the count.
let formatTokens (tokens: int) =
    if tokens < 1_000 then
        string tokens
    elif tokens < 1_000_000 then
        sprintf "%dk" (tokens / 1_000)
    else
        let tenths = tokens / 100_000

        if tenths % 10 = 0 then
            sprintf "%dM" (tenths / 10)
        else
            sprintf "%d.%dM" (tenths / 10) (tenths % 10)

let format (contextWindow: ContextWindow) : Segment option =
    let usage = defaultArg contextWindow.UsedPercentage 0
    let percentage = sprintf "%d%%" usage
    let bar = formatBar usage
    let color = StatusLine.Utils.Color.percentageToColor (float usage)

    // TotalInputTokens is 0 before the first API response; omit the token counts until then.
    let tokens =
        if contextWindow.TotalInputTokens > 0 then
            sprintf
                " (%s/%s)"
                (formatTokens contextWindow.TotalInputTokens)
                (formatTokens contextWindow.ContextWindowSize)
        else
            ""

    [
        { Text = "ctx "; Color = None }
        {
            Text = sprintf "%s %s%s" bar percentage tokens
            Color = Some color
        }
    ]
    |> Some
