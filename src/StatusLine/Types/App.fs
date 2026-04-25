module StatusLine.Types.App

open System.Drawing

type Segment = { Text: string; Color: Color option }

type ContextDeserializeError =
    | InvalidJson of message: string
    | MissingOrInvalidField of message: string
