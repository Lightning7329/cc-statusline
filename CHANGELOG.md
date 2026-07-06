# Changelog

This file is generated and maintained by
[release-please](https://github.com/googleapis/release-please) from
[Conventional Commits](https://www.conventionalcommits.org/). Do not edit it by hand.

New release entries are inserted below this preamble in reverse-chronological order.

## [0.4.1](https://github.com/Lightning7329/cc-statusline/compare/v0.4.0...v0.4.1) (2026-07-06)


### Features

* Context に prompt_id フィールドを追加する ([#80](https://github.com/Lightning7329/cc-statusline/issues/80)) ([8f6472d](https://github.com/Lightning7329/cc-statusline/commit/8f6472dc98a2b51b861782b242c18cd86c648b1c)), closes [#79](https://github.com/Lightning7329/cc-statusline/issues/79)
* セッション経過時間を表示するセグメントを追加する ([#82](https://github.com/Lightning7329/cc-statusline/issues/82)) ([e206c59](https://github.com/Lightning7329/cc-statusline/commit/e206c59eb9aed99d5c83b59b7583fca6605ea7d6))

## [0.4.0](https://github.com/Lightning7329/cc-statusline/compare/v0.3.0...v0.4.0) (2026-07-01)


### Features

* ClaudeCodeVersion セグメントを追加 ([#69](https://github.com/Lightning7329/cc-statusline/issues/69)) ([5c9c1b0](https://github.com/Lightning7329/cc-statusline/commit/5c9c1b04238f71a1cf329d83c13ae9880cec40a9))
* コスト系セグメントを2行目に移動し、CostDisplay の色をオレンジに変更 ([#71](https://github.com/Lightning7329/cc-statusline/issues/71)) ([3f766c2](https://github.com/Lightning7329/cc-statusline/commit/3f766c2752c2fd2dd24384b44622037da73001f0))

## [0.3.0](https://github.com/Lightning7329/cc-statusline/compare/v0.2.1...v0.3.0) (2026-06-28)


### Features

* 変更行数 (+追加/-削除) を表示する LinesChanged セグメントを追加 ([bbdb367](https://github.com/Lightning7329/cc-statusline/commit/bbdb367dcc7ce93514b1818e0a33d5f26296b82e))
* 変更行数 (+追加/-削除) を表示する LinesChanged セグメントを追加 ([7e624d1](https://github.com/Lightning7329/cc-statusline/commit/7e624d1b03e5eb092b36041b20d5b17e49b7a38d))


### Bug Fixes

* install.sh で json モジュール不在の python3-minimal に対応 ([#41](https://github.com/Lightning7329/cc-statusline/issues/41), [#49](https://github.com/Lightning7329/cc-statusline/issues/49)) ([#64](https://github.com/Lightning7329/cc-statusline/issues/64)) ([2aa1238](https://github.com/Lightning7329/cc-statusline/commit/2aa1238fce889548ae5692c39cd8f28b955b0d9c))
* install.sh の --dir を絶対パスに正規化 ([#62](https://github.com/Lightning7329/cc-statusline/issues/62)) ([02ff217](https://github.com/Lightning7329/cc-statusline/commit/02ff2179953e71fc0b2d68cbba2311c1ecde1b07)), closes [#61](https://github.com/Lightning7329/cc-statusline/issues/61)


### Documentation

* README のモデル表示例・フォント要件・セグメント一覧を修正 ([#66](https://github.com/Lightning7329/cc-statusline/issues/66)) ([38e8c4c](https://github.com/Lightning7329/cc-statusline/commit/38e8c4c3c1147428fa6db3a1673593ceb27781c4)), closes [#48](https://github.com/Lightning7329/cc-statusline/issues/48)

## [0.2.1](https://github.com/Lightning7329/cc-statusline/compare/v0.2.0...v0.2.1) (2026-06-21)


### Bug Fixes

* cwd 短縮を WORKSPACE_ROOT env から workspace コンテキスト由来に変更 ([#46](https://github.com/Lightning7329/cc-statusline/issues/46)) ([6c71586](https://github.com/Lightning7329/cc-statusline/commit/6c715868cc70d407f4c747a2319abe763c5c92d4))
* cwd 短縮を WORKSPACE_ROOT env から workspace コンテキスト由来に変更 ([#46](https://github.com/Lightning7329/cc-statusline/issues/46)) ([500b3aa](https://github.com/Lightning7329/cc-statusline/commit/500b3aa891b59d64361d064ef71dc7e8a4847802))
* release ワークフローを release-please に統合してトリガー不発を解消 ([#38](https://github.com/Lightning7329/cc-statusline/issues/38)) ([c6f4629](https://github.com/Lightning7329/cc-statusline/commit/c6f4629504681a57b96b926c611eb077505186a1)), closes [#37](https://github.com/Lightning7329/cc-statusline/issues/37)
* WORKSPACE_ROOT="" でのクラッシュを修正し例外を1行表示に収める ([#52](https://github.com/Lightning7329/cc-statusline/issues/52)) ([f924172](https://github.com/Lightning7329/cc-statusline/commit/f924172f859d50a6fbfda11a1b45224e06ce302b)), closes [#43](https://github.com/Lightning7329/cc-statusline/issues/43)
* レート制限のリセット時刻をローカルタイムゾーンで表示 ([#57](https://github.com/Lightning7329/cc-statusline/issues/57)) ([b31e687](https://github.com/Lightning7329/cc-statusline/commit/b31e6874d069e549cfa10cd74a483f4af054c8a9)), closes [#47](https://github.com/Lightning7329/cc-statusline/issues/47)

## [0.2.0](https://github.com/Lightning7329/cc-statusline/compare/v0.1.1...v0.2.0) (2026-05-31)


### ⚠ BREAKING CHANGES

* requires Claude Code v2.1.119 or later

### Features

* statusline スキーマ更新と effort level 表示の追加 ([#35](https://github.com/Lightning7329/cc-statusline/issues/35)) ([0b3d303](https://github.com/Lightning7329/cc-statusline/commit/0b3d3033898b08a08cddb8d5b600f619f8cd5474))
* テスト実行からHTMLカバレッジレポート作成までをスクリプト&タスク化 ([0f98a89](https://github.com/Lightning7329/cc-statusline/commit/0f98a896e9ae069f960333ffbd72c85f5b254685))

## [0.1.1](https://github.com/Lightning7329/cc-statusline/releases/tag/v0.1.1)

## [0.1.0](https://github.com/Lightning7329/cc-statusline/releases/tag/v0.1.0)

## [0.0.1](https://github.com/Lightning7329/cc-statusline/releases/tag/v0.0.1)
