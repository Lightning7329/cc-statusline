module StatusLine.Types.App

open System.Drawing

type Span = { Text: string; Color: Color option }

type Segment = Span list

type ContextDeserializeError =
    | InvalidJson of message: string
    | MissingOrInvalidField of message: string
