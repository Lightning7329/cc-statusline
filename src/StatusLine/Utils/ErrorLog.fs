module StatusLine.Utils.ErrorLog

open System
open System.IO

/// ログファイルがこのサイズを超えたら、追記前に切り詰める
let private maxBytes = 1024L * 1024L

/// エラーログの出力先を決める。優先順は
/// `$XDG_STATE_HOME/cc-statusline/error.log` → `$HOME/.local/state/cc-statusline/error.log`
/// → `{tempDir}/cc-statusline/error.log`。
/// 相対パスの環境変数は無視する（起動元ディレクトリにログが作られるのを防ぐため。XDG Base Directory 仕様にも沿う）。
let resolvePath (getEnv: string -> string option) (tempDir: string) : string =
    let rooted key =
        getEnv key |> Option.filter Path.IsPathRooted

    rooted "XDG_STATE_HOME"
    |> Option.orElseWith (fun () -> rooted "HOME" |> Option.map (fun h -> Path.Combine(h, ".local", "state")))
    |> Option.defaultValue tempDir
    |> fun baseDir -> Path.Combine(baseDir, "cc-statusline", "error.log")

/// 1件分のログエントリを組み立てる。stdin を読む前に失敗した場合 input は None。
let formatEntry (now: DateTimeOffset) (ex: exn) (input: string option) : string =
    let inputText = input |> Option.defaultValue "(not read)"

    $"""===== {now.ToString "o"} =====
[exception]
{ex}
[input]
{inputText}

"""

/// ログには入力 JSON（プロジェクトのパス等）が含まれるため、ディレクトリは所有者のみアクセス可能にする。
/// 一時ディレクトリにフォールバックした場合に他ユーザーから読まれないようにするのが主目的。
let private ownerOnly =
    UnixFileMode.UserRead ||| UnixFileMode.UserWrite ||| UnixFileMode.UserExecute

/// ログファイルへ追記する。ログ出力自体の失敗は握りつぶす（status line の表示を優先するため）。
let append (path: string) (entry: string) : unit =
    try
        Directory.CreateDirectory(Path.GetDirectoryName path, ownerOnly) |> ignore

        if File.Exists path && FileInfo(path).Length > maxBytes then
            File.WriteAllText(path, entry)
        else
            File.AppendAllText(path, entry)
    with _ ->
        ()
