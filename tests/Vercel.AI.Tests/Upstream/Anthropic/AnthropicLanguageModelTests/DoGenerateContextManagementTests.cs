// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; context management</c>.</summary>
public sealed class DoGenerateContextManagementTests
{
    private const string Id = Prefix + "doGenerate > context management::";

    private static string ContextManagement(string edits)
    {
        return """{"contextManagement":{"edits":""" + edits + "}}";
    }

    private static async Task<CaptureHandler> SendEdits(string edits)
    {
        var handler = JsonFixture("anthropic-text");

        await DoGenerate(handler, Options(ContextManagement(edits)));

        return handler;
    }

    private static JsonNode? ContextManagementMetadata(LanguageModelGenerateResult result)
    {
        return JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["anthropic"]![
            "contextManagement"
        ];
    }

    private static string ResponseWithContextManagement(string contextManagement)
    {
        return """
                {"id":"msg_123","type":"message","role":"assistant","content":[{"type":"text","text":"Hello"}],"model":"claude-3-haiku-20240307","stop_reason":"end_turn","stop_sequence":null,"usage":{"input_tokens":100,"output_tokens":50},"context_management":
                """
            + contextManagement
            + "}";
    }

    [Fact]
    [UpstreamTest(
        Id + "should send context_management in request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_context_management_in_request_body()
    {
        var handler = await SendEdits("""[{"type":"clear_tool_uses_20250919"}]""");

        AssertJsonContains(
            JsonNode.Parse(
                """{"context_management":{"edits":[{"type":"clear_tool_uses_20250919"}]}}"""
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should add context-management beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_context_management_beta_header()
    {
        var handler = await SendEdits("""[{"type":"clear_tool_uses_20250919"}]""");

        Assert.Contains("context-management-2025-06-27", Beta(handler));
    }

    [Fact]
    [UpstreamTest(
        Id + "should parse context_management from response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_context_management_from_response()
    {
        var result = await DoGenerate(
            JsonValue(
                ResponseWithContextManagement(
                    """{"applied_edits":[{"type":"clear_tool_uses_20250919","cleared_tool_uses":5,"cleared_input_tokens":10000}]}"""
                )
            ),
            Options()
        );

        AssertJson(
            """{"appliedEdits":[{"type":"clear_tool_uses_20250919","clearedToolUses":5,"clearedInputTokens":10000}]}""",
            ContextManagementMetadata(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map clear_tool_uses_20250919 with all options to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_clear_tool_uses_20250919_with_all_options_to_request_body()
    {
        var handler = await SendEdits(
            """[{"type":"clear_tool_uses_20250919","trigger":{"type":"input_tokens","value":50000},"keep":{"type":"tool_uses","value":5},"clearAtLeast":{"type":"input_tokens","value":10000},"clearToolInputs":true,"excludeTools":["important_tool"]}]"""
        );

        AssertJsonContains(
            JsonNode.Parse(
                """
                {"context_management":{"edits":[{
                  "type":"clear_tool_uses_20250919",
                  "trigger":{"type":"input_tokens","value":50000},
                  "keep":{"type":"tool_uses","value":5},
                  "clear_at_least":{"type":"input_tokens","value":10000},
                  "clear_tool_inputs":true,
                  "exclude_tools":["important_tool"]
                }]}}
                """
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map clear_thinking_20251015 with keep option to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_clear_thinking_20251015_with_keep_option_to_request_body()
    {
        var handler = await SendEdits(
            """[{"type":"clear_thinking_20251015","keep":{"type":"thinking_turns","value":3}}]"""
        );

        AssertJsonContains(
            JsonNode.Parse(
                """{"context_management":{"edits":[{"type":"clear_thinking_20251015","keep":{"type":"thinking_turns","value":3}}]}}"""
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map multiple context management edits to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_multiple_context_management_edits_to_request_body()
    {
        var handler = await SendEdits(
            """[{"type":"clear_tool_uses_20250919"},{"type":"clear_thinking_20251015"}]"""
        );

        AssertJsonContains(
            JsonNode.Parse(
                """{"context_management":{"edits":[{"type":"clear_tool_uses_20250919"},{"type":"clear_thinking_20251015"}]}}"""
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map compact_20260112 to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_compact_20260112_to_request_body()
    {
        var handler = await SendEdits(
            """[{"type":"compact_20260112","trigger":{"type":"input_tokens","value":50000}}]"""
        );

        AssertJsonContains(
            JsonNode.Parse(
                """{"context_management":{"edits":[{"type":"compact_20260112","trigger":{"type":"input_tokens","value":50000}}]}}"""
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map compact_20260112 with all options to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_compact_20260112_with_all_options_to_request_body()
    {
        var handler = await SendEdits(
            """[{"type":"compact_20260112","trigger":{"type":"input_tokens","value":50000},"pauseAfterCompaction":true,"instructions":"Summarize the conversation concisely."}]"""
        );

        AssertJsonContains(
            JsonNode.Parse(
                """
                {"context_management":{"edits":[{
                  "type":"compact_20260112",
                  "trigger":{"type":"input_tokens","value":50000},
                  "pause_after_compaction":true,
                  "instructions":"Summarize the conversation concisely."
                }]}}
                """
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should add compact beta header when using compact edit",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_compact_beta_header_when_using_compact_edit()
    {
        var handler = await SendEdits("""[{"type":"compact_20260112"}]""");

        Assert.Contains("compact-2026-01-12", Beta(handler));
        Assert.Contains("context-management-2025-06-27", Beta(handler));
    }

    [Fact]
    [UpstreamTest(
        Id + "should parse compaction response with iterations and compaction content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_compaction_response_with_iterations_and_compaction_content()
    {
        var result = await DoGenerate(JsonFixture("anthropic-compaction.1"), Options());

        var content = ContentJson(result.Content).AsArray();
        Assert.Equal(2, content.Count);
        Assert.Equal("text", (string?)content[0]!["type"]);
        Assert.Contains("## Summary of Conversation", (string)content[0]!["text"]!);
        AssertJson("""{"anthropic":{"type":"compaction"}}""", content[0]!["providerMetadata"]);
        Assert.Equal(["providerMetadata", "text", "type"], Keys(content[0]!));
        Assert.Equal("text", (string?)content[1]!["type"]);
        Assert.Contains(
            "Based on our conversation history, you had asked me",
            (string)content[1]!["text"]!
        );
        Assert.Equal(["text", "type"], Keys(content[1]!));

        AssertJson(
            """
            [
              {"type":"compaction","inputTokens":60385,"outputTokens":592},
              {"type":"message","inputTokens":682,"outputTokens":1320}
            ]
            """,
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["anthropic"]!["iterations"]
        );

        Assert.Equal(60385 + 682, result.Usage.InputTokens);
        Assert.Equal(592 + 1320, result.Usage.OutputTokens);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("null")]
    [UpstreamTest(
        Id + "should omit compaction response with %s content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_compaction_response_with_content(string compactionContent)
    {
        var result = await DoGenerate(
            JsonValue(
                """{"id":"msg_123","type":"message","role":"assistant","content":[{"type":"compaction","content":"""
                    + compactionContent
                    + """},{"type":"text","text":"Hello"}],"model":"claude-3-haiku-20240307","stop_reason":"end_turn","stop_sequence":null,"usage":{"input_tokens":100,"output_tokens":50}}"""
            ),
            Options()
        );

        AssertJson("""[{"type":"text","text":"Hello"}]""", ContentJson(result.Content));
    }

    [Fact]
    [UpstreamTest(
        Id + "should parse context_management with compact_20260112 from response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_context_management_with_compact_20260112_from_response()
    {
        var result = await DoGenerate(
            JsonValue(
                ResponseWithContextManagement("""{"applied_edits":[{"type":"compact_20260112"}]}""")
            ),
            Options()
        );

        AssertJson(
            """{"appliedEdits":[{"type":"compact_20260112"}]}""",
            ContextManagementMetadata(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should parse clear_tool_uses response with context_management",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_clear_tool_uses_response_with_context_management()
    {
        var result = await DoGenerate(JsonFixture("anthropic-clear-tool-uses.1"), Options());

        var content = ContentJson(result.Content).AsArray();
        Assert.Single(content);
        Assert.Equal("text", (string?)content[0]!["type"]);
        Assert.Contains(
            "Here's a comparison of the weather in both cities",
            (string)content[0]!["text"]!
        );
        Assert.Equal(["text", "type"], Keys(content[0]!));
        AssertJson("""{"appliedEdits":[]}""", ContextManagementMetadata(result));
        AssertJson(
            """
            {
              "inputTokens": {"cacheRead":0,"cacheWrite":0,"noCache":859,"total":859},
              "outputTokens": {"total":132},
              "raw": {
                "cache_creation": {"ephemeral_1h_input_tokens":0,"ephemeral_5m_input_tokens":0},
                "cache_creation_input_tokens": 0,
                "cache_read_input_tokens": 0,
                "inference_geo": "not_available",
                "input_tokens": 859,
                "output_tokens": 132,
                "service_tier": "standard"
              }
            }
            """,
            UsageJson(result.Usage)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should parse clear_thinking response with thinking and context_management",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_clear_thinking_response_with_thinking_and_context_management()
    {
        var result = await DoGenerate(JsonFixture("anthropic-clear-thinking.1"), Options());

        var content = ContentJson(result.Content).AsArray();
        Assert.Equal(2, content.Count);
        Assert.Equal("reasoning", (string?)content[0]!["type"]);
        Assert.Equal("925 divided by 5 = 185", (string?)content[0]!["text"]);
        Assert.Equal(
            System.Text.Json.JsonValueKind.String,
            content[0]!["providerMetadata"]!["anthropic"]!["signature"]!.GetValueKind()
        );
        Assert.Equal(["providerMetadata", "text", "type"], Keys(content[0]!));
        Assert.Equal(["signature"], Keys(content[0]!["providerMetadata"]!["anthropic"]!));
        AssertJson("""{"type":"text","text":"925 ÷ 5 = 185"}""", content[1]);
        AssertJson("""{"appliedEdits":[]}""", ContextManagementMetadata(result));
        AssertJson(
            """
            {
              "inputTokens": {"cacheRead":0,"cacheWrite":0,"noCache":69,"total":69},
              "outputTokens": {"total":33},
              "raw": {
                "cache_creation": {"ephemeral_1h_input_tokens":0,"ephemeral_5m_input_tokens":0},
                "cache_creation_input_tokens": 0,
                "cache_read_input_tokens": 0,
                "inference_geo": "not_available",
                "input_tokens": 69,
                "output_tokens": 33,
                "service_tier": "standard"
              }
            }
            """,
            UsageJson(result.Usage)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should parse combined clear_thinking and clear_tool_uses response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_combined_clear_thinking_and_clear_tool_uses_response()
    {
        var handler = JsonFixture("anthropic-combined-context-editing.1");

        var result = await DoGenerate(
            handler,
            Options(
                """
                {
                  "thinking":{"type":"enabled","budgetTokens":5000},
                  "contextManagement":{"edits":[
                    {"type":"clear_thinking_20251015","keep":{"type":"thinking_turns","value":1}},
                    {"type":"clear_tool_uses_20250919","trigger":{"type":"tool_uses","value":4},"keep":{"type":"tool_uses","value":1}}
                  ]}
                }
                """
            )
        );

        var content = ContentJson(result.Content).AsArray();
        Assert.Equal(2, content.Count);
        Assert.Equal("reasoning", (string?)content[0]!["type"]);
        Assert.Contains("25 * 37", (string)content[0]!["text"]!);
        Assert.Equal(["providerMetadata", "text", "type"], Keys(content[0]!));
        Assert.Equal(["signature"], Keys(content[0]!["providerMetadata"]!["anthropic"]!));
        Assert.Equal("text", (string?)content[1]!["type"]);
        Assert.Contains("925", (string)content[1]!["text"]!);
        Assert.Equal(["text", "type"], Keys(content[1]!));
        AssertJson("""{"appliedEdits":[]}""", ContextManagementMetadata(result));

        AssertJson(
            HelloBody(
                """
                {
                  "context_management": {"edits":[
                    {"keep":{"type":"thinking_turns","value":1},"type":"clear_thinking_20251015"},
                    {"keep":{"type":"tool_uses","value":1},"trigger":{"type":"tool_uses","value":4},"type":"clear_tool_uses_20250919"}
                  ]},
                  "thinking": {"budget_tokens":5000,"type":"enabled"}
                }
                """
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should return compaction stop reason as other",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_compaction_stop_reason_as_other()
    {
        var result = await DoGenerate(
            JsonValue(
                """{"id":"msg_123","type":"message","role":"assistant","content":[{"type":"compaction","content":"Compaction summary..."}],"model":"claude-3-haiku-20240307","stop_reason":"compaction","stop_sequence":null,"usage":{"input_tokens":100,"output_tokens":50}}"""
            ),
            Options()
        );

        AssertJson(
            """{"unified":"other","raw":"compaction"}""",
            FinishReasonJson(result.FinishReason, result.RawFinishReason)
        );
    }

    // toEqual fails on a property the expected object does not list.
    private static string[] Keys(JsonNode node)
    {
        return node.AsObject()
            .Select(pair => pair.Key)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();
    }
}
