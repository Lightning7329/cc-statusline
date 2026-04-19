# CLAUDE.md

このファイルは、リポジトリのコードを操作する際に Claude Code (claude.ai/code) へのガイダンスを提供します。

## コマンド

```bash
# ビルド
dotnet build
dotnet build --configuration Release

# 実行
dotnet run --project src/StatusLine

# 自己完結型 Linux x64 バイナリとして発行
dotnet publish -c Release -r linux-x64 --self-contained

# コードフォーマット（Fantomas）
dotnet fantomas .
```

テストスイートは未設定です。

## アーキテクチャ

.NET 10 をターゲットとした F# コンソールアプリケーション。自己完結型 Linux x64 バイナリとして発行されます。エントリーポイントは `src/StatusLine/Program.fs` です。プロジェクトは初期スキャフォールディング段階であり、プレースホルダーの `printfn` 以外の実装はありません。

## コードスタイル

- F# のフォーマットには Fantomas 7.0.0 を使用 (`dotnet fantomas .`)
- ブラケットスタイル: Stroustrup（開き括弧を同じ行に）、最大空行数 2（`.editorconfig` で強制）
