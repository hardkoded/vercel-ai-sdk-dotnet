// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>assistant messages &gt; advisor 20260301 multi-turn round-trip</c>.</summary>
public sealed class Advisor20260301MultiTurnRoundTripTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages > advisor 20260301 multi-turn round-trip::";

    [Fact]
    [UpstreamTest(
        Prefix + "should convert advisor server_tool_use + advisor_result back to the API shape",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_advisor_server_tool_use_plus_advisor_result_back_to_the_API_shape()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{},"providerExecuted":true,"toolCallId":"srvtoolu_advisor_abc123","toolName":"advisor","type":"tool-call"},
              {"output":{"type":"json","value":{"type":"advisor_result","text":"Use a channel-based coordination pattern. Close the input channel first, then wait on a WaitGroup.","stopReason":"max_tokens"}},"toolCallId":"srvtoolu_advisor_abc123","toolName":"advisor","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_advisor_abc123","input":{},"name":"advisor","type":"server_tool_use"},
              {"content":{"stop_reason":"max_tokens","text":"Use a channel-based coordination pattern. Close the input channel first, then wait on a WaitGroup.","type":"advisor_result"},"tool_use_id":"srvtoolu_advisor_abc123","type":"advisor_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should round-trip advisor_redacted_result verbatim across turns",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_round_trip_advisor_redacted_result_verbatim_across_turns()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{},"providerExecuted":true,"toolCallId":"srvtoolu_advisor_redacted","toolName":"advisor","type":"tool-call"},
              {"output":{"type":"json","value":{"type":"advisor_redacted_result","encryptedContent":"opaque-encrypted-blob-xyz","stopReason":"end_turn"}},"toolCallId":"srvtoolu_advisor_redacted","toolName":"advisor","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        // The encrypted blob must round-trip verbatim under the snake_case
        // wire field name `encrypted_content` so the server can decrypt it.
        AssertJson(
            """
            {"content":{"encrypted_content":"opaque-encrypted-blob-xyz","stop_reason":"end_turn","type":"advisor_redacted_result"},"tool_use_id":"srvtoolu_advisor_redacted","type":"advisor_tool_result"}
            """,
            result.Messages[0]!["content"]![1]
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert advisor_tool_result_error back to the API shape",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_advisor_tool_result_error_back_to_the_API_shape()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{},"providerExecuted":true,"toolCallId":"srvtoolu_advisor_err","toolName":"advisor","type":"tool-call"},
              {"output":{"type":"json","value":{"type":"advisor_tool_result_error","errorCode":"max_uses_exceeded"}},"toolCallId":"srvtoolu_advisor_err","toolName":"advisor","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            {"content":{"error_code":"max_uses_exceeded","type":"advisor_tool_result_error"},"tool_use_id":"srvtoolu_advisor_err","type":"advisor_tool_result"}
            """,
            result.Messages[0]!["content"]![1]
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve multiple advisor turns interleaved with text",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_multiple_advisor_turns_interleaved_with_text()
    {
        var result = Run(
            """
            [{"role":"user","content":[{"type":"text","text":"Build a concurrent worker pool in Go with graceful shutdown."}]},
             {"role":"assistant","content":[
               {"type":"text","text":"Let me consult the advisor on this."},
               {"input":{},"providerExecuted":true,"toolCallId":"srvtoolu_advisor_1","toolName":"advisor","type":"tool-call"},
               {"output":{"type":"json","value":{"type":"advisor_result","text":"Use channels and a WaitGroup."}},"toolCallId":"srvtoolu_advisor_1","toolName":"advisor","type":"tool-result"},
               {"type":"text","text":"Here is the implementation."}]},
             {"role":"user","content":[{"type":"text","text":"Now add a max-in-flight limit of 10."}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[{"text":"Build a concurrent worker pool in Go with graceful shutdown.","type":"text"}],"role":"user"},
             {"content":[
               {"text":"Let me consult the advisor on this.","type":"text"},
               {"id":"srvtoolu_advisor_1","input":{},"name":"advisor","type":"server_tool_use"},
               {"content":{"text":"Use channels and a WaitGroup.","type":"advisor_result"},"tool_use_id":"srvtoolu_advisor_1","type":"advisor_tool_result"},
               {"text":"Here is the implementation.","type":"text"}],"role":"assistant"},
             {"content":[{"text":"Now add a max-in-flight limit of 10.","type":"text"}],"role":"user"}]
            """,
            result.Messages
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn (and not emit a result block) when output type is unsupported",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_warn_and_not_emit_a_result_block_when_output_type_is_unsupported()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{},"providerExecuted":true,"toolCallId":"srvtoolu_advisor_bad","toolName":"advisor","type":"tool-call"},
              {"output":{"type":"text","value":"should be json"},"toolCallId":"srvtoolu_advisor_bad","toolName":"advisor","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        // Only the server_tool_use block is emitted; the result is skipped.
        Assert.Single((JsonArray)result.Messages[0]!["content"]!);
        AssertWarnings(
            result,
            "provider executed tool result output type text for tool advisor is not supported"
        );
    }
}
