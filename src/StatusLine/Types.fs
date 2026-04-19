module StatusLine.Types

open System.Text.Json.Serialization

type Model = { Id: string; DisplayName: string }

type Workspace = {
    CurrentDir: string
    ProjectDir: string
    AddedDirs: string list
}

type OutputStyle = { Name: string }

type Cost = {
    TotalCostUsd: float
    TotalDurationMs: int
    TotalApiDurationMs: int
    TotalLinesAdded: int
    TotalLinesRemoved: int
}

type CurrentUsage = {
    InputTokens: int
    OutputTokens: int
    CacheCreationInputTokens: int
    CacheReadInputTokens: int
}

type ContextWindow = {
    TotalInputTokens: int
    TotalOutputTokens: int
    ContextWindowSize: int
    UsedPercentage: int
    RemainingPercentage: int
    CurrentUsage: CurrentUsage
}

type RateLimitEntry = {
    UsedPercentage: float
    ResetsAt: int64
}

type RateLimits = {
    FiveHour: RateLimitEntry
    SevenDay: RateLimitEntry
}

type VimMode = { Mode: string }

type Agent = { Name: string }

type Worktree = {
    Name: string
    Path: string
    Branch: string
    OriginalCwd: string
    OriginalBranch: string
}

type Context = {
    Cwd: string
    SessionId: string
    SessionName: string
    TranscriptPath: string
    Model: Model
    Workspace: Workspace
    Version: string
    OutputStyle: OutputStyle
    Cost: Cost
    ContextWindow: ContextWindow
    [<JsonPropertyName("exceeds_200k_tokens")>]
    Exceeds200kTokens: bool
    RateLimits: RateLimits
    Vim: VimMode option
    Agent: Agent option
    Worktree: Worktree option
}
