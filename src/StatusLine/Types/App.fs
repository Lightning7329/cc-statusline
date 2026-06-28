module StatusLine.Types.App

open System.Drawing

type Span = { Text: string; Color: Color option }

type Segment = Span list

module Segment =
    let fromText (text: string) : Segment = [ { Text = text; Color = None } ]

type ContextDeserializeError =
    | InvalidJson of message: string
    | MissingOrInvalidField of message: string
