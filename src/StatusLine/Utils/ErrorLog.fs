module StatusLine.Utils.ErrorLog

open System
open System.IO

/// ログファイルがこのサイズを超えたら、追記前に切り詰める
let private maxBytes = 1024L * 1024L

/// エラーログの出力先を決める。
/// `$XDG_STATE_HOME/cc-statusline/error.log`、未設定なら `$HOME/.local/state/cc-statusline/error.log`。
/// XDG Base Directory 仕様に従い、相対パスの `XDG_STATE_HOME` は無視する。
let resolvePath (getEnv: string -> string option) : string option =
    getEnv "XDG_STATE_HOME"
    |> Option.filter Path.IsPathRooted
    |> Option.orElseWith (fun () -> getEnv "HOME" |> Option.map (fun h -> Path.Combine(h, ".local", "state")))
    |> Option.map (fun stateDir -> Path.Combine(stateDir, "cc-statusline", "error.log"))

/// 1件分のログエントリを組み立てる。stdin を読む前に失敗した場合 input は None。
let formatEntry (now: DateTimeOffset) (ex: exn) (input: string option) : string =
    let inputText = input |> Option.defaultValue "(not read)"

    $"""===== {now.ToString "o"} =====
[exception]
{ex}
[input]
{inputText}

"""

/// ログファイルへ追記する。ログ出力自体の失敗は握りつぶす（status line の表示を優先するため）。
let append (path: string) (entry: string) : unit =
    try
        Path.GetDirectoryName path |> Directory.CreateDirectory |> ignore

        if File.Exists path && FileInfo(path).Length > maxBytes then
            File.WriteAllText(path, entry)
        else
            File.AppendAllText(path, entry)
    with _ ->
        ()
