// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenResponsesInputTests.ConvertToOpenResponsesInputHelpers;

namespace Vercel.AI.Tests.ConvertToOpenResponsesInputTests;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; tool messages</c>.</summary>
public sealed class ToolMessagesTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > tool messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert tool message with json output", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_message_with_json_output()
    {
        var result = Convert(new ToolModelMessage("call_123", "get_weather", "{\"temperature\":72,\"condition\":\"sunny\"}", false, "json"));

        AssertInput(
            "[{\"call_id\":\"call_123\",\"output\":\"{\\\"temperature\\\":72,\\\"condition\\\":\\\"sunny\\\"}\",\"type\":\"function_call_output\"}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert tool message with text output", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_message_with_text_output()
    {
        var result = Convert(new ToolModelMessage("call_456", "search", "Search results: Found 5 items", false, "text"));

        AssertInput("[{\"call_id\":\"call_456\",\"output\":\"Search results: Found 5 items\",\"type\":\"function_call_output\"}]", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert tool message with error-text output", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_message_with_error_text_output()
    {
        var result = Convert(new ToolModelMessage("call_789", "api_call", "API request failed: timeout", true, "error-text"));

        AssertInput("[{\"call_id\":\"call_789\",\"output\":\"API request failed: timeout\",\"type\":\"function_call_output\"}]", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert tool message with execution-denied output", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_message_with_execution_denied_output()
    {
        var result = Convert(new ToolModelMessage("call_denied", "dangerous_action", "User declined the action", true, "execution-denied"));

        AssertInput("[{\"call_id\":\"call_denied\",\"output\":\"User declined the action\",\"type\":\"function_call_output\"}]", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert tool message with multiple tool results", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_message_with_multiple_tool_results()
    {
        // The C# prompt model holds one tool result per ToolModelMessage, so the two results are two consecutive messages.
        var result = Convert(
            new ToolModelMessage("call_001", "get_weather", "{\"temp\":72}", false, "json"),
            new ToolModelMessage("call_002", "get_time", "3:00 PM", false, "text"));

        AssertInput(
            "[{\"call_id\":\"call_001\",\"output\":\"{\\\"temp\\\":72}\",\"type\":\"function_call_output\"},"
            + "{\"call_id\":\"call_002\",\"output\":\"3:00 PM\",\"type\":\"function_call_output\"}]",
            result);
    }
}
