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

### レイヤー設計

出力ロジックを以下の2層に分離する：

1. **テキスト計算層**（テスト対象）: 表示するテキストの内容や、パーセンテージに応じた色（`System.Drawing.Color`）を計算する純粋なロジック。Pastelに依存しない。
2. **色付加層**（テスト対象外）: Pastelを使い、計算済みの色をテキストに適用してANSIエスケープコード付き文字列を生成する。

この設計により、テストプロジェクトへのPastel追加は不要。

## コードスタイル

- F# のフォーマットには Fantomas 7.0.0 を使用 (`dotnet fantomas .`)
- ブラケットスタイル: Stroustrup（開き括弧を同じ行に）、最大空行数 2（`.editorconfig` で強制）
