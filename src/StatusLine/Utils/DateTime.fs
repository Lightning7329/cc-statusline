module StatusLine.Utils.DateTime

open System

let unixTimeToLocalDateTimeOffset unixTime =
    unixTime |> DateTimeOffset.FromUnixTimeSeconds |> _.ToLocalTime()
