module StatusLine.Segments.RateLimit

open StatusLine.Types.Context
open StatusLine.Types.App

let formatEntry (label: string) (dateFormat: string) (entry: RateLimitEntry) : Segment =
    let resetTime =
        entry.ResetsAt
        |> StatusLine.Utils.DateTime.unixTimeToLocalDateTimeOffset
        |> _.ToString(dateFormat)

    let text = sprintf "%1.1f%% (reset at %s)" entry.UsedPercentage resetTime
    let color = StatusLine.Utils.Color.percentageToColor entry.UsedPercentage

    [ { Text = label; Color = None }; { Text = text; Color = Some color } ]

let formatFiveHour (limits: RateLimits option) : Segment option =
    limits |> Option.bind _.FiveHour |> Option.map (formatEntry "5h " "HH:mm")

let formatSevenDay (limits: RateLimits option) : Segment option =
    limits |> Option.bind _.SevenDay |> Option.map (formatEntry "7d " "MM/dd HH:mm")
