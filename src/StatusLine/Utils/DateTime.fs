module StatusLine.Utils.DateTime

open System

let UnixTimeToJstDateTimeOffset unixTime =
    unixTime
    |> DateTimeOffset.FromUnixTimeSeconds
    |> fun dt -> dt.ToOffset(TimeSpan.FromHours 9.0)
