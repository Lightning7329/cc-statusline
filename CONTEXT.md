# CONTEXT.md — cc-statusline ドメイン用語集

コードとレビューで使う用語の定義。アーキテクチャレビューはこの語彙を前提にする。

## 用語

- **Span** — 色付きテキストの最小単位。`{ Text: string; Color: Color option }`。`Color = None` は「端末のデフォルト色」を意味する。
- **Segment** — `Span list`。元々は「ステータスラインの 1 要素」だったが、区切り（`" | "`）・改行（`"\n"`）を無色 Span として含めることで**ステータスライン全体も 1 つの Segment として表現する**（2026-07 の設計決定。`build` の戻り値がこれ）。
- **行 (Row)** — `" | "` で結合されるセグメントの並び。行内の全セグメントが `None` の場合、その行は出力から除去される（空行除去）。
- **二層設計** — テキスト計算層（純粋関数、テスト対象、Pastel 非依存）と色適用層（`ColoredOutput` / Pastel）。色適用＝`render` は Program で最後に 1 回だけ行う。テキスト計算層に `open Pastel` が現れたら設計違反。
- **Settings** — アプリが参照する環境変数の集約レコード。`build` には引数で注入する。`Settings.fromEnv ()` を呼んでよいのは配線層（`buildFromInput` / Program）のみ。
- **配線 (wiring)** — 本番依存（実 git、実環境変数）を純粋関数に結びつける 1 行のコード。Program 内の `buildFromInput Segments.GitBranch.format (Utils.Settings.fromEnv ())` がその例で、本番配線は Program に集約する。StatusLineBuilder 以下は副作用を引数で受け取る純粋なパイプライン。
