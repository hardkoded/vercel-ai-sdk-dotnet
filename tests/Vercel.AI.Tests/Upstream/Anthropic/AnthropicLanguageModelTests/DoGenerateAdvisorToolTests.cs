// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; advisor tool</c>.</summary>
public sealed class DoGenerateAdvisorToolTests
{
    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > advisor tool::should expose stopReason for plaintext and redacted advisor results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_stopReason_for_plaintext_and_redacted_advisor_results()
    {
        var result = await DoGenerate(
            JsonFixture("anthropic-advisor-stop-reasons"),
            Options(
                tools:
                [
                    ProviderTool(
                        "anthropic.advisor_20260301",
                        "advisor",
                        """{"model":"claude-opus-4-7","maxTokens":2048}"""
                    ),
                ]
            ),
            "claude-sonnet-4-6"
        );

        AssertJson(
            """
            [
              {"result":{"stopReason":"max_tokens","text":"Partial plaintext advice.","type":"advisor_result"},"toolCallId":"srvtoolu_advisor_plaintext","toolName":"advisor","type":"tool-result"},
              {"result":{"encryptedContent":"opaque-encrypted-advice","stopReason":"end_turn","type":"advisor_redacted_result"},"toolCallId":"srvtoolu_advisor_redacted","toolName":"advisor","type":"tool-result"}
            ]
            """,
            ContentJson(result.Content.Where(part => part.Type == "tool-result"))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > advisor tool::should emit a tool-call for the advisor server_tool_use so it round-trips on follow-up turns",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_a_tool_call_for_the_advisor_server_tool_use_so_it_round_trips_on_follow_up_turns()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_advisor","model":"claude-sonnet-4-6","role":"assistant","content":[
              {"type":"server_tool_use","id":"srvtoolu_advisor_1","name":"advisor","input":{}},
              {"type":"advisor_tool_result","tool_use_id":"srvtoolu_advisor_1","content":{"type":"advisor_result","text":"Outline the design first; pick Submit semantics upfront."}},
              {"type":"text","text":"Here is the design outline..."}
            ],"stop_reason":"end_turn","usage":{"input_tokens":10,"output_tokens":20}}
            """
        );

        var result = await DoGenerate(
            handler,
            Options(
                tools:
                [
                    ProviderTool(
                        "anthropic.advisor_20260301",
                        "advisor",
                        """{"model":"claude-opus-4-7"}"""
                    ),
                ]
            ),
            "claude-sonnet-4-6"
        );

        AssertJson(
            """
            [
              {"input":"{}","providerExecuted":true,"toolCallId":"srvtoolu_advisor_1","toolName":"advisor","type":"tool-call"},
              {"result":{"text":"Outline the design first; pick Submit semantics upfront.","type":"advisor_result"},"toolCallId":"srvtoolu_advisor_1","toolName":"advisor","type":"tool-result"},
              {"text":"Here is the design outline...","type":"text"}
            ]
            """,
            ContentJson(result.Content)
        );
    }
}
