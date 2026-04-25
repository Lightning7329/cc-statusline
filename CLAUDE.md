# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## コマンド

```bash
# ビルド
dotnet build
dotnet build --configuration Release

# テスト
dotnet test
dotnet test --filter "テスト名の一部"

# 実行（stdin から JSON を受け取る）
cat sample.json | dotnet run --project src/StatusLine

# 自己完結型バイナリとして発行
dotnet publish -c Release -r linux-x64 --self-contained

# コードフォーマット（Fantomas）
dotnet fantomas .
```

## アーキテクチャ

.NET 10 をターゲットとした F# コンソールアプリケーション。Claude Code のステータスライン表示を担う。stdin から JSON（Context）を受け取り、ANSIカラー付きのステータスライン文字列を stdout に出力する。

エントリーポイントは `src/StatusLine/Program.fs`（1行）。

### データフロー

```
stdin JSON → tryParseInput (Result<Context, ContextDeserializeError>)
           → build (各セグメントをフォーマット・色付け)
           → " | " で結合して stdout へ
```

### レイヤー設計

出力ロジックを以下の2層に分離する：

1. **テキスト計算層**（テスト対象）: 表示するテキストの内容や、パーセンテージに応じた色（`System.Drawing.Color`）を計算する純粋なロジック。Pastel に依存しない。
2. **色付加層**（テスト対象外）: `ColoredOutput.applyColor` が Pastel を使い、計算済みの色をテキストに適用して ANSI エスケープコード付き文字列を生成する。

この設計により、テストプロジェクトへの Pastel 追加は不要。

### モジュール構成

- **Types.fs** — 全レコード型・判別共用体（`Context`, `Segment`, `ContextDeserializeError` 等）
- **Color.fs** — `percentageToColor: float → Color`（0%=緑 → 100%=赤のグラデーション、OKLCH 色空間で補間。Wacton.Unicolour 使用）
- **Segments/** — 各セグメントの純粋なフォーマッタ（`Cwd`, `ModelName`, `CostDisplay`, `RateLimit`）
- **Utils/** — ヘルパー（`DateTime`：JST変換、`WorkingDirectory`：パス正規化）
- **StatusLineBuilder.fs** — JSON パース（`tryParseInput`）とセグメント組み立て（`build`）のオーケストレーション

### JSON デシリアライズ

FSharp.SystemTextJson を使用。snake_case ↔ PascalCase の自動変換。`option` 型フィールドは JSON の不在・null いずれも `None` にマッピング（`SkippableOptionFields.Always`）。パース失敗は `Result` 型の `ContextDeserializeError`（`InvalidJson` / `MissingOrInvalidField`）で返す。

## コードスタイル

- F# のフォーマットには Fantomas 7.0.0 を使用 (`dotnet fantomas .`)
- ブラケットスタイル: Stroustrup（開き括弧を同じ行に）、最大空行数 2（`.editorconfig` で強制）

## テスト

XUnit + FsUnit.Xunit。DU ケースのアサーションには `FsUnit.CustomMatchers.ofCase` を使用。テスト名は日本語。
