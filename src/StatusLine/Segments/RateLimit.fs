module StatusLine.Segments.RateLimit

open StatusLine.Types.Context
open StatusLine.Types.App

let formatEntry (label: string) (dateFormat: string) (entry: RateLimitEntry) : Segment =
    let resetTime =
        entry.ResetsAt
        |> StatusLine.Utils.DateTime.unixTimeToLocalDateTimeOffset
        |> _.ToString(dateFormat)

    let text = sprintf "%1.1f%% (reset at %s)" entry.UsedPercentage resetTime
    let color = StatusLine.Color.percentageToColor entry.UsedPercentage

    [ { Text = label; Color = None }; { Text = text; Color = Some color } ]

let formatFiveHour (entry: RateLimitEntry) : Segment = entry |> formatEntry "5h " "HH:mm"

let formatSevenDay (entry: RateLimitEntry) : Segment =
    entry |> formatEntry "7d " "MM/dd HH:mm"
