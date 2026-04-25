module StatusLine.Tests.StatusLineBuilderTests

open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers
open StatusLine.Types
open StatusLine.StatusLineBuilder

let private unwrapOk result =
    result |> Result.defaultWith (sprintf "%A" >> failwith)

let private unwrapError result =
    match result with
    | Error e -> e
    | Ok v -> failwithf "Expected Error, but got Ok %A" v

// 全フィールドを含む最小限のベース JSON（sample.json 相当）
let private fullJson =
    """{
  "cwd": "/current/working/directory",
  "session_id": "abc123",
  "session_name": "my-session",
  "transcript_path": "/path/to/transcript.jsonl",
  "model": { "id": "claude-opus-4-6", "display_name": "Opus" },
  "workspace": {
    "current_dir": "/current/working/directory",
    "project_dir": "/original/project/directory",
    "added_dirs": []
  },
  "version": "2.1.90",
  "output_style": { "name": "default" },
  "cost": {
    "total_cost_usd": 0.01234,
    "total_duration_ms": 45000,
    "total_api_duration_ms": 2300,
    "total_lines_added": 156,
    "total_lines_removed": 23
  },
  "context_window": {
    "total_input_tokens": 15234,
    "total_output_tokens": 4521,
    "context_window_size": 200000,
    "used_percentage": 8,
    "remaining_percentage": 92,
    "current_usage": {
      "input_tokens": 8500,
      "output_tokens": 1200,
      "cache_creation_input_tokens": 5000,
      "cache_read_input_tokens": 2000
    }
  },
  "exceeds_200k_tokens": false,
  "rate_limits": {
    "five_hour": { "used_percentage": 23.5, "resets_at": 1738425600 },
    "seven_day": { "used_percentage": 41.2, "resets_at": 1738857600 }
  },
  "vim": { "mode": "NORMAL" },
  "agent": { "name": "security-reviewer" },
  "worktree": {
    "name": "my-feature",
    "path": "/path/to/.claude/worktrees/my-feature",
    "branch": "worktree-my-feature",
    "original_cwd": "/path/to/project",
    "original_branch": "main"
  }
}"""

// --- 全フィールドあり ---

[<Fact>]
let ``全フィールドありのとき session_name をパースできる`` () =
    let ctx = tryParseInput fullJson |> unwrapOk
    ctx.SessionName |> should equal (Some "my-session")

[<Fact>]
let ``全フィールドありのとき vim モードをパースできる`` () =
    let ctx = tryParseInput fullJson |> unwrapOk
    ctx.Vim |> should equal (Some { Mode = "NORMAL" })

[<Fact>]
let ``全フィールドありのとき agent をパースできる`` () =
    let ctx = tryParseInput fullJson |> unwrapOk
    ctx.Agent |> should equal (Some { Name = "security-reviewer" })

[<Fact>]
let ``全フィールドありのとき worktree をパースできる`` () =
    let ctx = tryParseInput fullJson |> unwrapOk

    ctx.Worktree
    |> should
        equal
        (Some {
            Name = "my-feature"
            Path = "/path/to/.claude/worktrees/my-feature"
            Branch = "worktree-my-feature"
            OriginalCwd = "/path/to/project"
            OriginalBranch = "main"
        })

[<Fact>]
let ``全フィールドありのとき rate_limits をパースできる`` () =
    let ctx = tryParseInput fullJson |> unwrapOk
    ctx.RateLimits |> Option.isSome |> should equal true
    ctx.RateLimits.Value.FiveHour.Value.UsedPercentage |> should equal 23.5
    ctx.RateLimits.Value.SevenDay.Value.UsedPercentage |> should equal 41.2

[<Fact>]
let ``全フィールドありのとき context_window.current_usage をパースできる`` () =
    let ctx = tryParseInput fullJson |> unwrapOk
    ctx.ContextWindow.CurrentUsage |> Option.isSome |> should equal true
    ctx.ContextWindow.CurrentUsage.Value.InputTokens |> should equal 8500

[<Fact>]
let ``全フィールドありのとき context_window の used_percentage をパースできる`` () =
    let ctx = tryParseInput fullJson |> unwrapOk
    ctx.ContextWindow.UsedPercentage |> should equal (Some 8)

// --- オプションフィールド不在 ---

[<Fact>]
let ``session_name がないとき None になる`` () =
    let ctx =
        tryParseInput (fullJson.Replace(""""session_name": "my-session",""", ""))
        |> unwrapOk

    ctx.SessionName |> should equal None

[<Fact>]
let ``vim がないとき None になる`` () =
    let ctx =
        tryParseInput (fullJson.Replace(""""vim": { "mode": "NORMAL" },""", ""))
        |> unwrapOk

    ctx.Vim |> should equal None

[<Fact>]
let ``agent がないとき None になる`` () =
    let ctx =
        tryParseInput (fullJson.Replace(""""agent": { "name": "security-reviewer" },""", ""))
        |> unwrapOk

    ctx.Agent |> should equal None

[<Fact>]
let ``worktree がないとき None になる`` () =
    let json =
        fullJson.Replace(
            """,
  "worktree": {
    "name": "my-feature",
    "path": "/path/to/.claude/worktrees/my-feature",
    "branch": "worktree-my-feature",
    "original_cwd": "/path/to/project",
    "original_branch": "main"
  }""",
            ""
        )

    let ctx = tryParseInput json |> unwrapOk
    ctx.Worktree |> should equal None

[<Fact>]
let ``rate_limits がないとき None になる`` () =
    let json =
        fullJson.Replace(
            """,
  "rate_limits": {
    "five_hour": { "used_percentage": 23.5, "resets_at": 1738425600 },
    "seven_day": { "used_percentage": 41.2, "resets_at": 1738857600 }
  }""",
            ""
        )

    let ctx = tryParseInput json |> unwrapOk
    ctx.RateLimits |> should equal None

// --- ネストされた option の部分不在 ---

[<Fact>]
let ``rate_limits.five_hour だけないとき FiveHour が None になる`` () =
    let json =
        fullJson.Replace(""""five_hour": { "used_percentage": 23.5, "resets_at": 1738425600 },""", "")

    let ctx = tryParseInput json |> unwrapOk
    ctx.RateLimits |> Option.isSome |> should equal true
    ctx.RateLimits.Value.FiveHour |> should equal None
    ctx.RateLimits.Value.SevenDay |> Option.isSome |> should equal true

[<Fact>]
let ``rate_limits.seven_day だけないとき SevenDay が None になる`` () =
    let json =
        fullJson.Replace(
            """,
    "seven_day": { "used_percentage": 41.2, "resets_at": 1738857600 }""",
            ""
        )

    let ctx = tryParseInput json |> unwrapOk
    ctx.RateLimits |> Option.isSome |> should equal true
    ctx.RateLimits.Value.SevenDay |> should equal None
    ctx.RateLimits.Value.FiveHour |> Option.isSome |> should equal true

// --- null フィールド ---

[<Fact>]
let ``context_window.current_usage が null のとき None になる`` () =
    let json =
        fullJson.Replace(
            """"current_usage": {
      "input_tokens": 8500,
      "output_tokens": 1200,
      "cache_creation_input_tokens": 5000,
      "cache_read_input_tokens": 2000
    }""",
            """"current_usage": null"""
        )

    let ctx = tryParseInput json |> unwrapOk
    ctx.ContextWindow.CurrentUsage |> should equal None

[<Fact>]
let ``context_window.used_percentage が null のとき None になる`` () =
    let json =
        fullJson.Replace(""""used_percentage": 8,""", """"used_percentage": null,""")

    let ctx = tryParseInput json |> unwrapOk
    ctx.ContextWindow.UsedPercentage |> should equal None

[<Fact>]
let ``context_window.remaining_percentage が null のとき None になる`` () =
    let json =
        fullJson.Replace(""""remaining_percentage": 92,""", """"remaining_percentage": null,""")

    let ctx = tryParseInput json |> unwrapOk
    ctx.ContextWindow.RemainingPercentage |> should equal None

// --- workspace.git_worktree ---

[<Fact>]
let ``workspace.git_worktree があるとき Some になる`` () =
    let replacement =
        "\"added_dirs\": [], \"git_worktree\": \"/path/to/linked/worktree\""

    let json = fullJson.Replace("\"added_dirs\": []", replacement)
    let ctx = tryParseInput json |> unwrapOk
    ctx.Workspace.GitWorktree |> should equal (Some "/path/to/linked/worktree")

[<Fact>]
let ``workspace.git_worktree がないとき None になる`` () =
    let ctx = tryParseInput fullJson |> unwrapOk
    ctx.Workspace.GitWorktree |> should equal None

// --- 異常系: InvalidJson ---

[<Fact>]
let ``空文字列のとき InvalidJson を返す`` () =
    tryParseInput "" |> unwrapError |> should be (ofCase <@ InvalidJson "" @>)

[<Fact>]
let ``不正なJSONのとき InvalidJson を返す`` () =
    tryParseInput "{ invalid json }"
    |> unwrapError
    |> should be (ofCase <@ InvalidJson "" @>)

// --- 異常系: MissingOrInvalidField ---

[<Fact>]
let ``空オブジェクトのとき MissingOrInvalidField を返す`` () =
    tryParseInput "{}"
    |> unwrapError
    |> should be (ofCase <@ MissingOrInvalidField "" @>)

[<Fact>]
let ``必須フィールド model が欠損しているとき MissingOrInvalidField を返す`` () =
    let json =
        fullJson.Replace(""""model": { "id": "claude-opus-4-6", "display_name": "Opus" },""", "")

    tryParseInput json
    |> unwrapError
    |> should be (ofCase <@ MissingOrInvalidField "" @>)

[<Fact>]
let ``必須フィールド cwd が欠損しているとき MissingOrInvalidField を返す`` () =
    let json = fullJson.Replace(""""cwd": "/current/working/directory",""", "")

    tryParseInput json
    |> unwrapError
    |> should be (ofCase <@ MissingOrInvalidField "" @>)

[<Fact>]
let ``必須フィールド cost が欠損しているとき MissingOrInvalidField を返す`` () =
    let json =
        fullJson.Replace(
            """,
  "cost": {
    "total_cost_usd": 0.01234,
    "total_duration_ms": 45000,
    "total_api_duration_ms": 2300,
    "total_lines_added": 156,
    "total_lines_removed": 23
  }""",
            ""
        )

    tryParseInput json
    |> unwrapError
    |> should be (ofCase <@ MissingOrInvalidField "" @>)

[<Fact>]
let ``フィールドの型が違うとき MissingOrInvalidField を返す`` () =
    let json =
        fullJson.Replace(""""total_cost_usd": 0.01234""", """"total_cost_usd": "not_a_number" """)

    tryParseInput json
    |> unwrapError
    |> should be (ofCase <@ MissingOrInvalidField "" @>)
