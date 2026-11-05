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

type SpendLimit = {
    UsedPercentage: float
    ResetsAt: int64
    UsedUsd: float option
    LimitUsd: float option
    Period: string option
}

type RateLimits = {
    FiveHour: RateLimitEntry option
    SevenDay: RateLimitEntry option
    SpendLimit: SpendLimit option
}

type LastMissCause = {
    Causes: string list option
    ToolsAdded: int option
    ToolsRemoved: int option
    SystemCharDelta: int option
}

type PromptCache = {
    Warm: bool option
    CachingObserved: bool option
    Ttl: string option
    ExpiresAt: int64 option
    Requests: int option
    Misses: int option
    ExpectedRebuilds: int option
    HitRatio: float option
    CacheWriteTokens: int64 option
    MissRecacheTokens: int64 option
    LastMissAt: int64 option
    LastMissCause: LastMissCause option
    MissCauses: Map<string, int> option
    RecacheTokensIfCold: int64 option
}

type Effort = { Level: string }

type Thinking = { Enabled: bool }

type VimMode = { Mode: string }

type Agent = { Name: string }

type Pr = {
    Number: int
    Url: string
    ReviewState: string option
    Kind: string option
}

type Worktree = {
    Name: string
    Path: string
    Branch: string option
    OriginalCwd: string
    OriginalBranch: string option
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
    PromptCache: PromptCache option
    FastMode: bool option
    Effort: Effort option
    Thinking: Thinking
    RateLimits: RateLimits option
    Vim: VimMode option
    Agent: Agent option
    Pr: Pr option
    Worktree: Worktree option
}
