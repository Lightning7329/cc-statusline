---
paths:
  - "src/StatusLine.Tests/**/*.fs"
---

# テストの書き方

## フレームワーク

- XUnit + FsUnit.Xunit を使用する。
- テスト名は日本語で記述する。
- 判別共用体のケース判定には `FsUnit.CustomMatchers.ofCase` を使う。

## モジュール分割（どの関数をテストしているか明示する）

1つのテストファイルで複数の関数を検証する場合、**関数ごとにモジュールへ分割**して、
どの関数のテストかをひと目で分かるようにする。

- ファイル先頭は `module` ではなく `namespace` で宣言する。
- 検証対象の関数名と同名（PascalCase）のサブモジュールを作り、その中に `[<Fact>]` を並べる。
- テスト名には関数名を繰り返さない（モジュール名で表現済みのため）。
- そのモジュール内だけで使うヘルパー（`makeXxx` などのファクトリ）は、当該モジュール内に
  `let private` で置く。

参考実装: `src/StatusLine.Tests/Segments/ContextWindowUsageTests.fs`

```fsharp
namespace StatusLine.Tests.Utils.SettingsTests

open Xunit
open FsUnit.Xunit
open StatusLine.Utils.Settings

module Load =

    [<Fact>]
    let ``getEnvのWORKSPACE_ROOTをWorkspaceRootにマップする`` () = ...

module EnvReader =

    [<Fact>]
    let ``空文字の場合Noneに正規化する`` () = ...
```

### 単一関数のみのテスト

検証対象が実質1関数だけのファイルでは、無理にモジュール分割せず
`module Xxx.Tests` のフラットな構成でよい。分割の目的は「複数関数が混在したときの
見通し」なので、混在しないなら適用しない。
