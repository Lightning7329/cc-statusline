module StatusLine.Types.Context

open System.Text.Json.Serialization

type Model = { Id: string; DisplayName: string }

type Repo = {
    Host: string
    Owner: string
    Name: string
}

type Workspace = {
    CurrentDir: string
    ProjectDir: string
    AddedDirs: string list
    GitWorktree: string option
    Repo: Repo option
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
    UsedPercentage: int option
    RemainingPercentage: int option
    CurrentUsage: CurrentUsage option
}

type RateLimitEntry = {
    UsedPercentage: float
    ResetsAt: int64
}

type RateLimits = {
    FiveHour: RateLimitEntry option
    SevenDay: RateLimitEntry option
}

type Effort = { Level: string }

type Thinking = { Enabled: bool }

type VimMode = { Mode: string }

type Agent = { Name: string }

type Pr = {
    Number: int
    Url: string
    ReviewState: string option
}

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
    SessionName: string option
    PromptId: string option
    TranscriptPath: string
    Model: Model
    Workspace: Workspace
    Version: string
    OutputStyle: OutputStyle
    Cost: Cost
    ContextWindow: ContextWindow
    [<JsonPropertyName("exceeds_200k_tokens")>]
    Exceeds200kTokens: bool
    Effort: Effort option
    Thinking: Thinking
    RateLimits: RateLimits option
    Vim: VimMode option
    Agent: Agent option
    Pr: Pr option
    Worktree: Worktree option
}
