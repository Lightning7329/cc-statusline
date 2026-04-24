module StatusLine.Segments.RateLimit

open StatusLine.Types

let formatEntry (dateFormat: string) (entry: RateLimitEntry) : Segment =
    let resetTime =
        entry.ResetsAt
        |> StatusLine.Utils.DateTime.UnixTimeToJstDateTimeOffset
        |> _.ToString(dateFormat)

    let text = sprintf "%1.1f%% (reset at %s)" entry.UsedPercentage resetTime
    let color = StatusLine.Color.percentageToColor entry.UsedPercentage
    { Text = text; Color = Some color }

let formatFiveHour (entry: RateLimitEntry) : Segment = formatEntry "HH:mm" entry

let formatSevenDay (entry: RateLimitEntry) : Segment = formatEntry "MM/dd HH:mm" entry
