# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Test (all)
dotnet test

# Test (single / filtered)
dotnet test --filter "full/partial name of test"

# Run (reads JSON from stdin)
cat tmp/sample.json | dotnet run --project src/StatusLine

# Publish as self-contained binary
dotnet publish -c Release -r linux-x64 --self-contained

# Code formatting (Fantomas)
dotnet fantomas .
```

## Architecture

F# console application targeting .NET 10. Reads a JSON `Context` from stdin, outputs ANSI-colored status line text to stdout. Entry point: `src/StatusLine/Program.fs` (supports `--version` flag).

### Data flow

```
stdin JSON → tryParseInput (Result<Context, ContextDeserializeError>)
           → build (format each segment with color)
           → rows joined by "\n", segments within a row joined by " | "
           → stdout
```

### Two-layer output design

1. **Text computation layer** (tested): Segment formatters return `Segment` records containing plain text and a `System.Drawing.Color` value. No dependency on Pastel.
2. **Color application layer** (not tested): `ColoredOutput.applyColor` wraps text with ANSI escape codes via Pastel.

This separation means the test project does not need a Pastel reference.

### Module compilation order (as in .fsproj)

`Utils/` (OptionBuilder, DateTime, WorkingDirectory, Process) → `Types/` (Context, App) → `Color` → `Segments/` (ContextWindowUsage, Cwd, GitBranch, ModelName, CostDisplay, RateLimit) → `ColoredOutput` → `StatusLineBuilder` → `Program`

### JSON deserialization

FSharp.SystemTextJson with `JsonNamingPolicy.SnakeCaseLower` for automatic snake_case ↔ PascalCase conversion. Option fields: JSON absence and `null` both map to `None` (`SkippableOptionFields.Always` with `deserializeNullAsNone = true`). Parse failures return `Result` with `ContextDeserializeError` (`InvalidJson` | `MissingOrInvalidField`).

### Color gradient

`Color.percentageToColor: float → Color` interpolates from green (0%) to red (100%) in OKLCH color space using Wacton.Unicolour.

## Code style

- Format F# with Fantomas 7.0.0 (`dotnet fantomas .`)
- Bracket style: Stroustrup; max 2 consecutive blank lines (`.editorconfig`)

## Testing

XUnit + FsUnit.Xunit. Test names are in Japanese. Use `FsUnit.CustomMatchers.ofCase` for discriminated union case assertions.
