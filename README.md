# cc-statusline

Claude Code のステータスライン用スクリプト。F# で実装し、各プラットフォーム向けのシングルバイナリとして配布します。

## ビルド

### Linux x64

```bash
dotnet publish -c Release -r linux-x64 --self-contained
```

### Linux ARM64

```bash
dotnet publish -c Release -r linux-arm64 --self-contained
```

### macOS Intel (x64)

```bash
dotnet publish -c Release -r osx-x64 --self-contained
```

### macOS Apple Silicon (ARM64)

```bash
dotnet publish -c Release -r osx-arm64 --self-contained
```

出力先: `src/StatusLine/bin/Release/net10.0/<RID>/publish/StatusLine`

## 開発

```bash
# 通常ビルド
dotnet build

# 実行
dotnet run --project src/StatusLine

# コードフォーマット
dotnet fantomas .
```
