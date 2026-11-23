// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

#pragma warning disable CS0618 // FormatOutputMessages is obsolete upstream too; its tests stay.
public sealed class FormatOutputMessagesTests
{
    private const string Test =
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::";

    [Fact]
    [UpstreamTest(Test + "should format text-only output", Coverage = UpstreamCoverage.Covered)]
    public void Should_format_text_only_output()
    {
        SemConvJson.AssertEqual(
            """[{"finish_reason":"stop","parts":[{"content":"Hello world","type":"text"}],"role":"assistant"}]""",
            GenAiConventions.FormatOutputMessages("stop", text: "Hello world")
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should format output with reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_output_with_reasoning()
    {
        SemConvJson.AssertEqual(
            """
            [{"finish_reason":"stop","parts":[{"content":"Let me think...","type":"reasoning"},{"content":"The answer is 42","type":"text"}],"role":"assistant"}]
            """,
            GenAiConventions.FormatOutputMessages(
                "stop",
                text: "The answer is 42",
                reasoning: ["Let me think..."]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should format output with tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_output_with_tool_calls()
    {
        SemConvJson.AssertEqual(
            """
            [{"finish_reason":"tool_call","parts":[{"arguments":{"city":"Paris"},"id":"call_abc","name":"get_weather","type":"tool_call"}],"role":"assistant"}]
            """,
            GenAiConventions.FormatOutputMessages(
                "tool-calls",
                toolCalls:
                [
                    new GeneratedToolCall("call_abc", "get_weather", """{"city":"Paris"}"""),
                ]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should format output with tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_output_with_tool_results()
    {
        SemConvJson.AssertEqual(
            """
            [{"finish_reason":"stop","parts":[{"id":"call_abc","response":{"temperature":21},"type":"tool_call_response"}],"role":"assistant"}]
            """,
            GenAiConventions.FormatOutputMessages(
                "stop",
                toolResults:
                [
                    new GeneratedToolResult("call_abc", "get_weather", """{"temperature":21}"""),
                ]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should format output with files",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port holds file data as bytes, so the part carries the base64 of the bytes instead of an already-encoded base64 string."
    )]
    public void Should_format_output_with_files()
    {
        SemConvJson.AssertEqual(
            """
            [{"finish_reason":"stop","parts":[{"content":"YWJj","mime_type":"image/png","modality":"image","type":"blob"}],"role":"assistant"}]
            """,
            GenAiConventions.FormatOutputMessages(
                "stop",
                files: [new GeneratedFile("abc"u8.ToArray(), "image/png")]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should combine reasoning, text, tool calls, and files",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port holds file data as bytes, so the part carries the base64 of the bytes instead of an already-encoded base64 string."
    )]
    public void Should_combine_reasoning_text_tool_calls_and_files()
    {
        SemConvJson.AssertEqual(
            """
            [{"finish_reason":"stop","parts":[
              {"content":"Thinking...","type":"reasoning"},
              {"content":"Here is the result","type":"text"},
              {"arguments":{"q":"test"},"id":"tc1","name":"search","type":"tool_call"},
              {"content":"ZGF0YQ==","mime_type":"image/jpeg","modality":"image","type":"blob"}
            ],"role":"assistant"}]
            """,
            GenAiConventions.FormatOutputMessages(
                "stop",
                text: "Here is the result",
                reasoning: ["Thinking..."],
                toolCalls: [new GeneratedToolCall("tc1", "search", """{"q":"test"}""")],
                files: [new GeneratedFile("data"u8.ToArray(), "image/jpeg")]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should map finish reasons correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_finish_reasons_correctly()
    {
        Assert.Equal("stop", GenAiConventions.FormatOutputMessages("stop")[0].FinishReason);
        Assert.Equal("length", GenAiConventions.FormatOutputMessages("length")[0].FinishReason);
        Assert.Equal(
            "tool_call",
            GenAiConventions.FormatOutputMessages("tool-calls")[0].FinishReason
        );
        Assert.Equal(
            "content_filter",
            GenAiConventions.FormatOutputMessages("content-filter")[0].FinishReason
        );
    }
}
