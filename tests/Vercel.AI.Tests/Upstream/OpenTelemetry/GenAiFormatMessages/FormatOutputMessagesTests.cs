// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Formatter = Vercel.AI.OpenTelemetry.GenAiFormatMessages;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

/// <summary>Port of <c>gen-ai-format-messages.test.ts</c> &gt; <c>formatOutputMessages</c>.</summary>
public sealed class FormatOutputMessagesTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format text-only output",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_text_only_output()
    {
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"Hello world","type":"text"}],"role":"assistant"}]
            """,
            Formatter.FormatOutputMessages(
                text: "Hello world",
                reasoning: null,
                toolCalls: null,
                toolResults: null,
                files: null,
                finishReason: FinishReason.Stop
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format output with reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_output_with_reasoning()
    {
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"Let me think...","type":"reasoning"},{"content":"The answer is 42","type":"text"}],"role":"assistant"}]
            """,
            Formatter.FormatOutputMessages(
                text: "The answer is 42",
                reasoning: ["Let me think..."],
                toolCalls: null,
                toolResults: null,
                files: null,
                finishReason: FinishReason.Stop
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format output with tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_output_with_tool_calls()
    {
        OtelJson.Equal(
            """
            [{"finish_reason":"tool_call","parts":[{"arguments":{"city":"Paris"},"id":"call_abc","name":"get_weather","type":"tool_call"}],"role":"assistant"}]
            """,
            Formatter.FormatOutputMessages(
                text: null,
                reasoning: null,
                toolCalls:
                [
                    new GeneratedToolCall("call_abc", "get_weather", "{\"city\":\"Paris\"}"),
                ],
                toolResults: null,
                files: null,
                finishReason: FinishReason.ToolCalls
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format output with tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_output_with_tool_results()
    {
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"id":"call_abc","response":{"temperature":21},"type":"tool_call_response"}],"role":"assistant"}]
            """,
            Formatter.FormatOutputMessages(
                text: null,
                reasoning: null,
                toolCalls: null,
                toolResults:
                [
                    new GeneratedToolResult("call_abc", "get_weather", "{\"temperature\":21}"),
                ],
                files: null,
                finishReason: FinishReason.Stop
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format output with files",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_output_with_files()
    {
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"abc123","mime_type":"image/png","modality":"image","type":"blob"}],"role":"assistant"}]
            """,
            Formatter.FormatOutputMessages(
                text: null,
                reasoning: null,
                toolCalls: null,
                toolResults: null,
                files: [("image/png", "abc123")],
                finishReason: FinishReason.Stop
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should combine reasoning, text, tool calls, and files",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_combine_reasoning_text_tool_calls_and_files()
    {
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"Thinking...","type":"reasoning"},{"content":"Here is the result","type":"text"},{"arguments":{"q":"test"},"id":"tc1","name":"search","type":"tool_call"},{"content":"data","mime_type":"image/jpeg","modality":"image","type":"blob"}],"role":"assistant"}]
            """,
            Formatter.FormatOutputMessages(
                text: "Here is the result",
                reasoning: ["Thinking..."],
                toolCalls: [new GeneratedToolCall("tc1", "search", "{\"q\":\"test\"}")],
                toolResults: null,
                files: [("image/jpeg", "data")],
                finishReason: FinishReason.Stop
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should map finish reasons correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_finish_reasons_correctly()
    {
        string Reason(FinishReason finishReason) =>
            (string)
                Formatter.FormatOutputMessages(null, null, null, null, null, finishReason)[0][
                    "finish_reason"
                ]!;

        OtelJson.Equal(
            """
            {"contentFilter":"content_filter","length":"length","stop":"stop","toolCalls":"tool_call"}
            """,
            new Dictionary<string, string>
            {
                ["stop"] = Reason(FinishReason.Stop),
                ["length"] = Reason(FinishReason.Length),
                ["toolCalls"] = Reason(FinishReason.ToolCalls),
                ["contentFilter"] = Reason(FinishReason.ContentFilter),
            }
        );
    }
}
