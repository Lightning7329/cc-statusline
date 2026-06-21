module StatusLine.Utils.Settings

open System

/// アプリ全体が参照する環境変数を集約したレコード
type Settings = { WorkspaceRoot: string option }

/// getEnv を注入して Settings を構築する（テスト用にモック可能）
let load (getEnv: string -> string option) : Settings = {
    WorkspaceRoot = getEnv "WORKSPACE_ROOT"
}

/// 実環境変数を読む getEnv 実装。空文字は None に正規化する。
let envReader (key: string) : string option =
    Environment.GetEnvironmentVariable key
    |> Option.ofObj
    |> Option.filter (fun s -> s.Length > 0)

/// 実環境から Settings を構築する
let fromEnv () : Settings = load envReader
