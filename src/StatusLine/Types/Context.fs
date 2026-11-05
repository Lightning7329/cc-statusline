module StatusLine.Types.Context

open System.Text.Json.Serialization

type Model = { Id: string; DisplayName: string }

type Repo = {
    Host: string
    /// For a gitlab.com project nested in subgroups, this is the full namespace path with slashes,
    /// such as "group/subgroup". Before v2.1.260, workspace.repo was absent for these projects.
    Owner: string
    Name: string
}

type Workspace = {
    /// Current working directory. Same value as cwd; this field is preferred for consistency
    /// with project_dir.
    CurrentDir: string
    /// Directory where Claude Code was launched, which may differ from cwd if the working directory
    /// changes during a session
    ProjectDir: string
    /// Additional directories added via /add-dir or --add-dir. Empty array if none have been added
    AddedDirs: string list
    /// Git worktree name when the current directory is inside a linked worktree created with
    /// git worktree add. Absent in the main working tree. Populated for any git worktree,
    /// unlike worktree.*, which is present only while the session is in a worktree session
    GitWorktree: string option
    /// Repository identity parsed from the origin remote, for example, "github.com", "anthropics",
    /// "claude-code". Absent outside a git repository or when no origin remote is configured.
    Repo: Repo option
}

type OutputStyle = {
    /// Name of the current output style
    Name: string
}

type Cost = {
    /// Estimated session cost in USD, computed client-side at list price unless a modelPricing table
    /// is in effect. May differ from your actual bill. Resets to $0 when /clear starts a new session
    TotalCostUsd: float
    /// Total wall-clock time the session has been running, in milliseconds. Accumulates across resumes
    /// and doesn't include time while the session isn't running
    TotalDurationMs: int
    /// Total time spent waiting for API responses in milliseconds
    TotalApiDurationMs: int
    /// Lines of code changed
    TotalLinesAdded: int
    /// Lines of code changed
    TotalLinesRemoved: int
}

type CurrentUsage = {
    /// Input tokens in current context
    InputTokens: int
    /// Output tokens generated
    OutputTokens: int
    /// Tokens written to cache
    CacheCreationInputTokens: int
    /// Tokens read from cache
    CacheReadInputTokens: int
}

type ContextWindow = {
    /// Token count currently in the context window, from the most recent API response. The sum of
    /// input_tokens, cache_creation_input_tokens, and cache_read_input_tokens.
    /// 0 before the first API response
    TotalInputTokens: int
    /// Output tokens from the most recent response. 0 before the first API response
    TotalOutputTokens: int
    /// Maximum context window size in tokens. 200000 by default, or 1000000 for models with
    /// extended context.
    ContextWindowSize: int
    /// Pre-calculated percentage of context window used. Calculated from input tokens only:
    /// input_tokens + cache_creation_input_tokens + cache_read_input_tokens.
    /// It does not include output_tokens. May be null early in the session
    UsedPercentage: int option
    /// Pre-calculated percentage of context window remaining. May be null early in the session
    RemainingPercentage: int option
    /// Token counts from the last API call, broken out by category. null before the first API call
    /// in a session, and again immediately after /compact until the next API call repopulates it
    CurrentUsage: CurrentUsage option
}

type RateLimitEntry = {
    /// Percentage of the 5-hour or 7-day rate limit consumed, from 0 to 100
    UsedPercentage: float
    /// Unix epoch seconds when the 5-hour or 7-day rate limit window resets
    ResetsAt: int64
}

type SpendLimit = {
    /// How much of your spend limit you have used. Runs from 0 to 100, or above 100 once you exceed
    /// the limit. Comes with every response, so it is present whenever spend_limit is
    UsedPercentage: float
    /// Unix epoch seconds when the limit's period resets. Comes with every response, so it is present
    /// whenever spend_limit is
    ResetsAt: int64
    /// Your estimated spend so far in US dollars. It's an estimate and not a billed amount, and can be
    /// about five minutes older than used_percentage. Can be absent even when spend_limit is present.
    /// Requires v2.1.284 or later on both Claude Code and the gateway
    UsedUsd: float option
    /// Your limit in US dollars. Can be absent even when spend_limit is present.
    /// Requires v2.1.284 or later on both Claude Code and the gateway
    LimitUsd: float option
    /// The period the limit covers, one of daily, weekly, or monthly. Can be absent even when
    /// spend_limit is present. Requires v2.1.284 or later on both Claude Code and the gateway
    Period: string option
}

type RateLimits = {
    FiveHour: RateLimitEntry option
    SevenDay: RateLimitEntry option
    /// Behind a Claude apps gateway, the spend limit that applies to you. Appears after the session's
    /// first API response. Requires Claude Code v2.1.251 or later
    SpendLimit: SpendLimit option
}

type LastMissCause = {
    /// One or more cause names, such as tools_changed, system_prompt_changed, ttl_expired_5m,
    /// or likely_server_side
    Causes: string list option
    /// With tools_changed, how many tools were added to the request
    ToolsAdded: int option
    /// With tools_changed, how many tools were removed from the request
    ToolsRemoved: int option
    /// With system_prompt_changed, the change in the system prompt's length, in characters
    SystemCharDelta: int option
}

type PromptCache = {
    /// Whether the cached prefix is still within its TTL. false when the last response reported
    /// no cache tokens, even while caching_observed is true
    Warm: bool option
    /// Whether any response this session reported cache tokens. false means prompt caching is off,
    /// or your provider or gateway doesn't report it
    CachingObserved: bool option
    /// Cache lifetime of the current cached prefix: "5m" or "1h"
    Ttl: string option
    /// When the cached prefix leaves its TTL and goes cold, in epoch seconds. null when the last
    /// response reported no cache tokens
    ExpiresAt: int64 option
    /// API requests recorded for the main conversation this session
    Requests: int option
    /// Requests that re-processed content the cache already held: more than 5% and at least 2,000 tokens
    /// of what the request could have read from cache, with no compaction or tool-result clearing
    /// to explain the shortfall in cache reads
    Misses: int option
    /// Cache rebuilds that followed a compaction or a clearing of old tool results
    ExpectedRebuilds: int option
    /// Cache read tokens as a fraction of all input tokens this session, from 0 to 1. The denominator
    /// counts cache reads, cache writes, and uncached input. null while those counts are all zero
    HitRatio: float option
    /// All tokens written to the cache this session, the first request's initial write included
    CacheWriteTokens: int64 option
    /// Tokens written to the cache by the requests counted as misses
    MissRecacheTokens: int64 option
    /// When the last miss happened, in epoch seconds. null while the session has no misses
    LastMissAt: int64 option
    /// What Claude Code identified as the likely cause of the last miss. null until the session's
    /// first miss, and again whenever Claude Code couldn't identify a cause for the most recent miss.
    /// Requires Claude Code v2.1.260 or later
    LastMissCause: LastMissCause option
    /// How many of this session's diagnosed misses had each cause, keyed by the same cause names as
    /// last_miss_cause. Requires Claude Code v2.1.260 or later
    MissCauses: Map<string, int> option
    /// Tokens the next request re-caches if the cache has gone cold by then. null right after
    /// a compaction or a clearing of old tool results, until the next request records the rewritten
    /// conversation's size
    RecacheTokensIfCold: int64 option
}

type Effort = {
    /// Reasoning effort level set for the session (low, medium, high, xhigh, or max), not one that
    /// a skill, a subagent, or ultrathink set. If you set a level the model doesn't support, this shows
    /// the level Claude Code ran instead. A level you change shows here even before it starts.
    /// With a level in CLAUDE_CODE_EFFORT_LEVEL, this shows that level
    Level: string
}

type Thinking = {
    /// Whether extended thinking is enabled for the session
    Enabled: bool
}

type VimMode = {
    /// Current vim mode (NORMAL, INSERT, VISUAL, or VISUAL LINE) when vim mode is enabled
    Mode: string
}

type Agent = {
    /// Agent name when running with the --agent flag or agent settings configured
    Name: string
}

type Pr = {
    Number: int
    Url: string
    /// Review status of the open PR: approved, pending, changes_requested, or draft.
    /// May be independently absent even when pr is present
    ReviewState: string option
    /// mr when pr describes a GitLab merge request. Absent for GitHub pull requests.
    /// For a merge request, Claude Code sets review_state to approved when GitLab reports it mergeable,
    /// pending for any other open state, and draft for a draft. Requires Claude Code v2.1.234 or later
    Kind: string option
}

type Worktree = {
    /// Name of the active worktree. Present only while the session is in a worktree session
    Name: string
    /// Absolute path to the worktree directory
    Path: string
    /// Git branch name for the worktree (for example, "worktree-my-feature").
    /// Absent for hook-based worktrees
    Branch: string option
    /// The directory Claude was in before entering the worktree
    OriginalCwd: string
    /// Git branch checked out before entering the worktree. Absent for hook-based worktrees
    OriginalBranch: string option
}

type Context = {
    /// Current working directory. Same value as workspace.current_dir, which is preferred for
    /// consistency with workspace.project_dir.
    Cwd: string
    /// Unique session identifier
    SessionId: string
    /// Session name. Uses the custom name set with the --name flag or /rename when one exists,
    /// otherwise the AI-generated session title. The default display name, such as my-app-3f,
    /// doesn't populate this field. Absent when the session has neither a custom name nor
    /// an AI-generated title
    SessionName: string option
    /// UUID identifying the user prompt currently being processed. Matches the prompt.id attribute on
    /// OpenTelemetry events. Absent until the first user input
    PromptId: string option
    /// Path to conversation transcript file
    TranscriptPath: string
    /// Current model identifier and display name
    Model: Model
    Workspace: Workspace
    /// Claude Code version
    Version: string
    OutputStyle: OutputStyle
    Cost: Cost
    /// The live context window from the most recent API response
    ContextWindow: ContextWindow
    /// Whether the total token count (input, cache, and output tokens combined) from the most recent
    /// API response exceeds 200k. This is a fixed threshold regardless of actual context window size.
    [<JsonPropertyName("exceeds_200k_tokens")>]
    Exceeds200kTokens: bool
    /// The session's prompt cache statistics for the main conversation: hit ratio, misses, and whether
    /// the cache is warm. Absent until the main conversation's first API response.
    /// Requires Claude Code v2.1.251 or later
    PromptCache: PromptCache option
    /// Whether fast mode is enabled for the session
    FastMode: bool option
    /// Absent when the current model does not support the effort parameter
    Effort: Effort option
    Thinking: Thinking
    /// Appears only for claude.ai Pro, Max, and Ultra subscribers, or behind a Claude apps gateway that
    /// sets a spend limit for you. Even then, it appears only after the first API response in the
    /// session. Each window (five_hour, seven_day, spend_limit) may be independently absent, and
    /// Claude Code drops a window once its resets_at time passes.
    RateLimits: RateLimits option
    /// Appears only when vim mode is enabled
    Vim: VimMode option
    /// Appears only when running with the --agent flag or agent settings configured
    Agent: Agent option
    /// Open pull request for the current branch. Mirrors the PR badge in the footer. In a repository with
    /// a GitLab remote, Claude Code fills these fields from the branch's open merge request instead,
    /// so number is the merge request number. Merge request data requires Claude Code v2.1.234 or later.
    /// Absent when not in a git repository, until a pull request or merge request is found, or once
    /// it merges or closes
    Pr: Pr option
    /// Appears only while the session is in a worktree session. When present, branch and original_branch
    /// may also be absent for hook-based worktrees
    Worktree: Worktree option
}
