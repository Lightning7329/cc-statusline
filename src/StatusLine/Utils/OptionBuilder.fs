module StatusLine.Utils.OptionBuilder

type OptionBuilder() =
    member _.Bind(m, f) =
        match m with
        | Some x -> f x
        | None -> None

    member _.Return x = Some x
    member _.ReturnFrom m = m
    member _.Zero() = None

let option = OptionBuilder()
