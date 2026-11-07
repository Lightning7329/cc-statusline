namespace StatusLine.Tests.StatusLineBuilderTests

open System.Drawing
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers
open StatusLine.Segments
open StatusLine.Types.Context
open StatusLine.Types.App
open StatusLine.Utils.Settings
open StatusLine.StatusLineBuilder

module internal Fixture =
    let unwrapOk result =
        result |> Result.defaultWith (sprintf "%A" >> failwith)

    let unwrapError result =
        match result with
        | Error e -> e
        | Ok v -> failwithf "Expected Error, but got Ok %A" v

    /// Segment 全体を色情報なしのプレーンテキストに変換する
    let text (segment: Segment) =
        segment |> List.map _.Text |> String.concat ""

    /// Segment をプレーンテキスト化して行に分割する
    let lines (segment: Segment) =
        (text segment).Split('\n') |> Array.toList

    // 本番の副作用（git・環境変数）を置き換えるフェイク
    let withBranch (_: string) (isWorktree: bool) =
        Some(GitBranch.formatBranch isWorktree "main")

    let noBranch (_: string) (_: bool) : Segment option = None

    /// 受け取った isWorktree を "worktree" / "normal" の文字列で返すフェイク
    let worktreeProbe (_: string) (isWorktree: bool) =
        Some(Segment.fromText (if isWorktree then "worktree" else "normal"))

    let noHome: Settings = { Home = None }

    // 全フィールドを含む最小限のベース JSON（sample.json 相当）
    let fullJson =
        """{
  "cwd": "/current/working/directory",
  "session_id": "abc123",
  "session_name": "my-session",
  "prompt_id": "550e8400-e29b-41d4-a716-446655440000",
  "transcript_path": "/path/to/transcript.jsonl",
  "model": { "id": "claude-opus-4-6", "display_name": "Opus" },
  "workspace": {
    "current_dir": "/current/working/directory",
    "project_dir": "/original/project/directory",
    "added_dirs": [],
    "repo": { "host": "github.com", "owner": "anthropics", "name": "claude-code" }
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
  "prompt_cache": {
    "warm": true,
    "caching_observed": true,
    "ttl": "1h",
    "expires_at": 1738429200,
    "requests": 14,
    "misses": 2,
    "expected_rebuilds": 1,
    "hit_ratio": 0.91,
    "cache_write_tokens": 352000,
    "miss_recache_tokens": 310200,
    "last_miss_at": 1738425230,
    "last_miss_cause": {
      "causes": ["tools_changed"],
      "tools_added": 2,
      "tools_removed": 0
    },
    "miss_causes": {
      "tools_changed": 2
    },
    "recache_tokens_if_cold": 45000
  },
  "fast_mode": false,
  "effort": { "level": "high" },
  "thinking": { "enabled": true },
  "rate_limits": {
    "five_hour": { "used_percentage": 23.5, "resets_at": 1738425600 },
    "seven_day": { "used_percentage": 41.2, "resets_at": 1738857600 },
    "spend_limit": {
      "used_percentage": 62.8,
      "resets_at": 1740787200,
      "used_usd": 314.12,
      "limit_usd": 500,
      "period": "monthly"
    }
  },
  "vim": { "mode": "NORMAL" },
  "agent": { "name": "security-reviewer" },
  "pr": {
    "number": 1234,
    "url": "https://github.com/anthropics/claude-code/pull/1234",
    "review_state": "pending"
  },
  "worktree": {
    "name": "my-feature",
    "path": "/path/to/.claude/worktrees/my-feature",
    "branch": "worktree-my-feature",
    "original_cwd": "/path/to/project",
    "original_branch": "main"
  }
}"""

module TryParseInput =
    open Fixture

    // --- 全フィールドあり ---

    [<Fact>]
    let ``全フィールドありのとき session_name をパースできる`` () =
        let ctx = tryParseInput fullJson |> unwrapOk
        ctx.SessionName |> should equal (Some "my-session")

    [<Fact>]
    let ``全フィールドありのとき prompt_id をパースできる`` () =
        let ctx = tryParseInput fullJson |> unwrapOk
        ctx.PromptId |> should equal (Some "550e8400-e29b-41d4-a716-446655440000")

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
                Branch = Some "worktree-my-feature"
                OriginalCwd = "/path/to/project"
                OriginalBranch = Some "main"
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

    [<Fact>]
    let ``全フィールドありのとき rate_limits.spend_limit をパースできる`` () =
        let ctx = tryParseInput fullJson |> unwrapOk

        ctx.RateLimits.Value.SpendLimit
        |> should
            equal
            (Some {
                UsedPercentage = 62.8
                ResetsAt = 1740787200L
                UsedUsd = Some 314.12
                LimitUsd = Some 500.0
                Period = Some "monthly"
            })

    [<Fact>]
    let ``全フィールドありのとき prompt_cache をパースできる`` () =
        let ctx = tryParseInput fullJson |> unwrapOk

        ctx.PromptCache
        |> should
            equal
            (Some {
                Warm = Some true
                CachingObserved = Some true
                Ttl = Some "1h"
                ExpiresAt = Some 1738429200L
                Requests = Some 14
                Misses = Some 2
                ExpectedRebuilds = Some 1
                HitRatio = Some 0.91
                CacheWriteTokens = Some 352000L
                MissRecacheTokens = Some 310200L
                LastMissAt = Some 1738425230L
                LastMissCause =
                    Some {
                        Causes = Some [ "tools_changed" ]
                        ToolsAdded = Some 2
                        ToolsRemoved = Some 0
                        SystemCharDelta = None
                    }
                MissCauses = Some(Map [ "tools_changed", 2 ])
                RecacheTokensIfCold = Some 45000L
            })

    [<Fact>]
    let ``全フィールドありのとき fast_mode をパースできる`` () =
        let ctx = tryParseInput fullJson |> unwrapOk
        ctx.FastMode |> should equal (Some false)

    // --- オプションフィールド不在 ---

    [<Fact>]
    let ``fast_mode がないとき None になる`` () =
        let ctx =
            tryParseInput (fullJson.Replace(""""fast_mode": false,""", "")) |> unwrapOk

        ctx.FastMode |> should equal None

    [<Fact>]
    let ``pr.kind があるとき Some になる`` () =
        let json =
            fullJson.Replace("\"review_state\": \"pending\"", "\"review_state\": \"pending\", \"kind\": \"mr\"")

        let ctx = tryParseInput json |> unwrapOk
        ctx.Pr.Value.Kind |> should equal (Some "mr")

    [<Fact>]
    let ``pr.kind がないとき None になる`` () =
        let ctx = tryParseInput fullJson |> unwrapOk
        ctx.Pr |> Option.isSome |> should equal true
        ctx.Pr.Value.Kind |> should equal None

    [<Fact>]
    let ``session_name がないとき None になる`` () =
        let ctx =
            tryParseInput (fullJson.Replace(""""session_name": "my-session",""", ""))
            |> unwrapOk

        ctx.SessionName |> should equal None

    [<Fact>]
    let ``prompt_id がないとき None になる`` () =
        let ctx =
            tryParseInput (fullJson.Replace(""""prompt_id": "550e8400-e29b-41d4-a716-446655440000",""", ""))
            |> unwrapOk

        ctx.PromptId |> should equal None

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
    "seven_day": { "used_percentage": 41.2, "resets_at": 1738857600 },
    "spend_limit": {
      "used_percentage": 62.8,
      "resets_at": 1740787200,
      "used_usd": 314.12,
      "limit_usd": 500,
      "period": "monthly"
    }
  }""",
                ""
            )

        let ctx = tryParseInput json |> unwrapOk
        ctx.RateLimits |> should equal None

    // --- ネストされた option の部分不在 ---

    [<Fact>]
    let ``worktree.branch と worktree.original_branch がないとき None になる`` () =
        let json =
            fullJson
                .Replace(""""branch": "worktree-my-feature",""", "")
                .Replace(""""original_cwd": "/path/to/project",""", "\"original_cwd\": \"/path/to/project\"")
                .Replace("\"original_branch\": \"main\"", "")

        let ctx = tryParseInput json |> unwrapOk
        ctx.Worktree |> Option.isSome |> should equal true
        ctx.Worktree.Value.Branch |> should equal None
        ctx.Worktree.Value.OriginalBranch |> should equal None

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
            fullJson.Replace(""""seven_day": { "used_percentage": 41.2, "resets_at": 1738857600 },""", "")

        let ctx = tryParseInput json |> unwrapOk
        ctx.RateLimits |> Option.isSome |> should equal true
        ctx.RateLimits.Value.SevenDay |> should equal None
        ctx.RateLimits.Value.FiveHour |> Option.isSome |> should equal true

    [<Fact>]
    let ``rate_limits.spend_limit だけないとき SpendLimit が None になる`` () =
        let json =
            fullJson.Replace(
                """,
    "spend_limit": {
      "used_percentage": 62.8,
      "resets_at": 1740787200,
      "used_usd": 314.12,
      "limit_usd": 500,
      "period": "monthly"
    }""",
                ""
            )

        let ctx = tryParseInput json |> unwrapOk
        ctx.RateLimits.Value.SpendLimit |> should equal None
        ctx.RateLimits.Value.FiveHour |> Option.isSome |> should equal true

    [<Fact>]
    let ``spend_limit の金額と期間がまだ届いていないとき None になる`` () =
        let json =
            fullJson.Replace(
                """"resets_at": 1740787200,
      "used_usd": 314.12,
      "limit_usd": 500,
      "period": "monthly"
    }""",
                """"resets_at": 1740787200
    }"""
            )

        let ctx = tryParseInput json |> unwrapOk

        ctx.RateLimits.Value.SpendLimit
        |> should
            equal
            (Some {
                UsedPercentage = 62.8
                ResetsAt = 1740787200L
                UsedUsd = None
                LimitUsd = None
                Period = None
            })

    [<Fact>]
    let ``spend_limit.used_percentage が 100 を超えていてもパースできる`` () =
        let json =
            fullJson.Replace(""""used_percentage": 62.8,""", """"used_percentage": 104.5,""")

        let ctx = tryParseInput json |> unwrapOk
        ctx.RateLimits.Value.SpendLimit.Value.UsedPercentage |> should equal 104.5

    [<Fact>]
    let ``prompt_cache がないとき None になる`` () =
        // キー名を変えて、prompt_cache が届いていない状態にする（未知のキーは無視される）
        let json = fullJson.Replace("\"prompt_cache\":", "\"not_prompt_cache\":")

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache |> should equal None

    [<Fact>]
    let ``prompt_cache の中身が空でもパースできる`` () =
        let json =
            fullJson.Replace("\"prompt_cache\":", "\"prompt_cache\": {}, \"not_prompt_cache\":")

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache.Value.Warm |> should equal None
        ctx.PromptCache.Value.MissCauses |> should equal None

    [<Fact>]
    let ``last_miss_cause と miss_causes がないとき None になる`` () =
        let json =
            fullJson.Replace(
                """"last_miss_cause": {
      "causes": ["tools_changed"],
      "tools_added": 2,
      "tools_removed": 0
    },
    "miss_causes": {
      "tools_changed": 2
    },""",
                ""
            )

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache.Value.LastMissCause |> should equal None
        ctx.PromptCache.Value.MissCauses |> should equal None

    // --- prompt_cache の値の範囲 ---

    [<Fact>]
    let ``last_miss_cause.system_char_delta が負の値でもパースできる`` () =
        let json =
            fullJson.Replace(
                """"causes": ["tools_changed"],
      "tools_added": 2,
      "tools_removed": 0""",
                """"causes": ["system_prompt_changed"],
      "system_char_delta": -120"""
            )

        let ctx = tryParseInput json |> unwrapOk

        ctx.PromptCache.Value.LastMissCause
        |> should
            equal
            (Some {
                Causes = Some [ "system_prompt_changed" ]
                ToolsAdded = None
                ToolsRemoved = None
                SystemCharDelta = Some -120
            })

    [<Fact>]
    let ``miss_causes は cause 名をそのままキーにして複数受け取れる`` () =
        let json =
            fullJson.Replace(
                """"miss_causes": {
      "tools_changed": 2
    },""",
                """"miss_causes": {
      "tools_changed": 2,
      "ttl_expired_5m": 1
    },"""
            )

        let ctx = tryParseInput json |> unwrapOk

        ctx.PromptCache.Value.MissCauses
        |> should equal (Some(Map [ "tools_changed", 2; "ttl_expired_5m", 1 ]))

    [<Fact>]
    let ``累計トークン数が int の範囲を超えてもパースできる`` () =
        let json =
            fullJson
                .Replace(""""cache_write_tokens": 352000,""", """"cache_write_tokens": 3000000000,""")
                .Replace(""""miss_recache_tokens": 310200,""", """"miss_recache_tokens": 3000000001,""")
                .Replace(""""recache_tokens_if_cold": 45000""", """"recache_tokens_if_cold": 3000000002""")

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache.Value.CacheWriteTokens |> should equal (Some 3000000000L)
        ctx.PromptCache.Value.MissRecacheTokens |> should equal (Some 3000000001L)
        ctx.PromptCache.Value.RecacheTokensIfCold |> should equal (Some 3000000002L)

    // --- null フィールド ---

    [<Fact>]
    let ``prompt_cache の expires_at が null のとき None になる`` () =
        let json =
            fullJson.Replace(""""expires_at": 1738429200,""", """"expires_at": null,""")

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache.Value.ExpiresAt |> should equal None

    [<Fact>]
    let ``prompt_cache の hit_ratio が null のとき None になる`` () =
        let json = fullJson.Replace(""""hit_ratio": 0.91,""", """"hit_ratio": null,""")

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache.Value.HitRatio |> should equal None

    [<Fact>]
    let ``prompt_cache の last_miss_at が null のとき None になる`` () =
        let json =
            fullJson.Replace(""""last_miss_at": 1738425230,""", """"last_miss_at": null,""")

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache.Value.LastMissAt |> should equal None

    [<Fact>]
    let ``prompt_cache の last_miss_cause が null のとき None になる`` () =
        let json =
            fullJson.Replace(
                """"last_miss_cause": {
      "causes": ["tools_changed"],
      "tools_added": 2,
      "tools_removed": 0
    },""",
                """"last_miss_cause": null,"""
            )

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache.Value.LastMissCause |> should equal None

    [<Fact>]
    let ``prompt_cache の recache_tokens_if_cold が null のとき None になる`` () =
        let json =
            fullJson.Replace(""""recache_tokens_if_cold": 45000""", """"recache_tokens_if_cold": null""")

        let ctx = tryParseInput json |> unwrapOk
        ctx.PromptCache.Value.RecacheTokensIfCold |> should equal None

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

module Build =
    open Fixture

    // 合成（行の並び・区切り・空行除去）だけを検証する。
    // 各セグメントの内容・色・書式の詳細は各セグメント単体テストの管轄。

    let private fullContext = tryParseInput fullJson |> unwrapOk

    [<Fact>]
    let ``全セグメントが揃うと3行を返す`` () =
        build withBranch noHome fullContext |> lines |> List.length |> should equal 3

    [<Fact>]
    let ``行はcwd行・コンテキスト行・レート制限行の順に並ぶ`` () =
        let rows = build withBranch noHome fullContext |> lines

        rows[0] |> should haveSubstring "v2.1.90"
        rows[1] |> should startWith "ctx "
        rows[2] |> should startWith "5h "
        rows[2] |> should haveSubstring "7d "

    [<Fact>]
    let ``コンテキスト行の末尾に経過時間セグメントが並ぶ`` () =
        let rows = build withBranch noHome fullContext |> lines

        // 2 行目 = ctx / lines / cost / duration。末尾は砂時計セグメント。
        rows[1].Split(" | ") |> Array.last |> should startWith "⏳ "

    [<Fact>]
    let ``行内のセグメントを「 | 」で結合する`` () =
        let firstRow = (build withBranch noHome fullContext |> lines)[0]

        // cwd・ブランチ・モデル・バージョンの 4 セグメント = 区切り 3 つ
        firstRow.Split(" | ") |> Array.length |> should equal 4

    [<Fact>]
    let ``ブランチが取れないとき1行目のセグメントが1つ減る`` () =
        let firstRow = (build noBranch noHome fullContext |> lines)[0]

        firstRow.Split(" | ") |> Array.length |> should equal 3

    [<Fact>]
    let ``workspace.git_worktree があるとき formatBranch に worktree であることを渡す`` () =
        let c = {
            fullContext with
                Workspace = {
                    fullContext.Workspace with
                        GitWorktree = Some "my-feature"
                }
        }

        let firstRow = (build worktreeProbe noHome c |> lines)[0]

        firstRow.Split(" | ") |> Array.item 1 |> should equal "worktree"

    [<Fact>]
    let ``workspace.git_worktree がないとき formatBranch に worktree でないことを渡す`` () =
        let firstRow = (build worktreeProbe noHome fullContext |> lines)[0]

        firstRow.Split(" | ") |> Array.item 1 |> should equal "normal"

    [<Fact>]
    let ``fast_mode が有効なときモデル名のセグメントに渡す`` () =
        let c = {
            fullContext with
                FastMode = Some true
        }

        let firstRow = (build withBranch noHome c |> lines)[0]

        firstRow.Split(" | ") |> Array.item 2 |> should equal "opus-4-6 (high, fast)"

    [<Fact>]
    let ``RateLimitsとPromptCacheが不在のとき3行目を出力しない`` () =
        let c = {
            fullContext with
                RateLimits = None
                PromptCache = None
        }

        build withBranch noHome c |> lines |> List.length |> should equal 2

    [<Fact>]
    let ``3行目の末尾にプロンプトキャッシュのセグメントが並ぶ`` () =
        let rows = build withBranch noHome fullContext |> lines

        // 3 行目 = 5h / 7d / cache。
        rows[2].Split(" | ") |> Array.last |> should startWith "🔥 "

    [<Fact>]
    let ``RateLimitsが不在でもPromptCacheがあれば3行目を出力する`` () =
        let c = { fullContext with RateLimits = None }

        let rows = build withBranch noHome c |> lines

        rows |> List.length |> should equal 3
        rows[2] |> should startWith "🔥 "

    [<Fact>]
    let ``HOME配下のcwdはチルダ表記で始まる`` () =
        // Arrange
        let formatBranch = withBranch
        let settings: Settings = { Home = Some "/current" }
        let c = fullContext // current_dir = /current/working/directory

        // Act
        let firstRow = (build formatBranch settings c |> lines)[0]

        // Assert
        firstRow |> should startWith "~/working/directory"

    [<Fact>]
    let ``区切りと改行のSpanは無色になる`` () =
        build withBranch noHome fullContext
        |> List.filter (fun span -> span.Text = " | " || span.Text = "\n")
        |> List.forall (fun span -> span.Color = None)
        |> should equal true

module BuildFromInput =
    open Fixture

    [<Fact>]
    let ``正常なJSONのときステータスラインのSegmentを返す`` () =
        let seg = buildFromInput withBranch noHome fullJson |> unwrapOk

        seg |> lines |> List.length |> should equal 3
        (seg |> lines)[0] |> should haveSubstring "v2.1.90"

    [<Fact>]
    let ``不正なJSONのとき InvalidJson を返す`` () =
        buildFromInput withBranch noHome "{ invalid json }"
        |> unwrapError
        |> should be (ofCase <@ InvalidJson "" @>)

    [<Fact>]
    let ``フィールド欠損のとき原因のパスを含む MissingOrInvalidField を返す`` () =
        let json = fullJson.Replace(""""cwd": "/current/working/directory",""", "")

        let error = buildFromInput withBranch noHome json |> unwrapError

        error |> should be (ofCase <@ MissingOrInvalidField "" @>)
        error.Message |> should haveSubstring "cwd"

module DescribeError =

    [<Fact>]
    let ``InvalidJson は invalid JSON と表す`` () =
        describeError (InvalidJson "detail") |> should equal "invalid JSON"

    [<Fact>]
    let ``MissingOrInvalidField は missing or invalid field と表す`` () =
        describeError (MissingOrInvalidField "detail")
        |> should equal "missing or invalid field"

module ErrorSegment =

    [<Fact>]
    let ``statusline error: を前置した赤のSegmentを返す`` () =
        let seg = errorSegment "invalid JSON"

        Fixture.text seg |> should equal "statusline error: invalid JSON"

        seg
        |> List.forall (fun span -> span.Color = Some Color.Red)
        |> should equal true
