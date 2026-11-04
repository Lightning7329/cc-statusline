# Development

## Build

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

Output: `src/StatusLine/bin/Release/net10.0/<RID>/publish/statusline`

### Binary size optimization

The project file enables the following settings to reduce the self-contained binary size:

| Setting                         | Effect                                                                 |
| ------------------------------- | ---------------------------------------------------------------------- |
| `InvariantGlobalization`        | Excludes ICU (internationalization) data (~30 MB reduction)            |
| `EnableCompressionInSingleFile` | Gzip-compresses IL assemblies inside the SingleFile (~10 MB reduction) |
| `DebugType=none`                | Excludes debug symbols                                                 |
| `StripSymbols`                  | Strips native symbols                                                  |

## Commands

```bash
# Build
dotnet build

# Run
dotnet run --project src/StatusLine

# Test
dotnet test

# Code formatting
dotnet fantomas .
```

## Testing locally

Place a sample JSON file at `tmp/sample.json`. The full JSON schema is available at [Available data](https://code.claude.com/docs/en/statusline#available-data).

Then:

```bash
# Build and run
cat tmp/sample.json | dotnet run --project src/StatusLine

# Run a published binary directly
cat tmp/sample.json | src/StatusLine/bin/Release/net10.0/linux-arm64/publish/statusline
```

## Watching the statusline JSON schema

`.github/workflows/check-statusline-schema.yml` runs daily and compares the key paths in the official [Full JSON schema](https://code.claude.com/docs/en/statusline#available-data) example with `schema/statusline-keys.txt`. When keys are added or removed, it opens (or updates) a single PR from `bot/statusline-schema` that updates the snapshot. Merging the PR marks the diff as acknowledged; decide whether to support new fields in separate issues.

The check can also be run locally (requires `curl` and `jq`):

```bash
# Updates schema/statusline-keys.txt and prints the PR body if there is a diff
scripts/check-statusline-schema.sh

# Use a local copy of the docs instead of fetching it
DOCS_FILE=path/to/statusline.md scripts/check-statusline-schema.sh
```
