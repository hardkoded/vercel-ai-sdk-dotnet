// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenResponsesInputTests.ConvertToOpenResponsesInputHelpers;

namespace Vercel.AI.Tests.ConvertToOpenResponsesInputTests;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; assistant messages with tool calls</c>.</summary>
public sealed class AssistantMessagesWithToolCallsTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > assistant messages with tool calls::";

    [Fact]
    [UpstreamTest(Prefix + "should convert assistant message with a single tool-call", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_assistant_message_with_a_single_tool_call()
    {
        var result = Convert(Assistant(Call("call_123", "get_weather", "{\"location\":\"San Francisco\"}")));

        AssertInput(
            "[{\"arguments\":\"{\\\"location\\\":\\\"San Francisco\\\"}\",\"call_id\":\"call_123\",\"name\":\"get_weather\",\"type\":\"function_call\"}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through tool-call string input", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_tool_call_string_input()
    {
        var result = Convert(Assistant(Call("call_124", "get_weather", "{\"location\":\"Berlin\"}")));

        AssertInput(
            "[{\"arguments\":\"{\\\"location\\\":\\\"Berlin\\\"}\",\"call_id\":\"call_124\",\"name\":\"get_weather\",\"type\":\"function_call\"}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert assistant message with text and tool-call", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_assistant_message_with_text_and_tool_call()
    {
        var result = Convert(Assistant(
            new AssistantTextPart("Let me check the weather for you."),
            Call("call_456", "get_weather", "{\"location\":\"New York\"}")));

        AssertInput(
            "[{\"content\":[{\"text\":\"Let me check the weather for you.\",\"type\":\"output_text\"}],\"role\":\"assistant\",\"type\":\"message\"},"
            + "{\"arguments\":\"{\\\"location\\\":\\\"New York\\\"}\",\"call_id\":\"call_456\",\"name\":\"get_weather\",\"type\":\"function_call\"}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert assistant message with multiple tool-calls", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_assistant_message_with_multiple_tool_calls()
    {
        var result = Convert(Assistant(
            Call("call_001", "get_weather", "{\"location\":\"Paris\"}"),
            Call("call_002", "get_time", "{\"timezone\":\"Europe/Paris\"}")));

        AssertInput(
            "[{\"arguments\":\"{\\\"location\\\":\\\"Paris\\\"}\",\"call_id\":\"call_001\",\"name\":\"get_weather\",\"type\":\"function_call\"},"
            + "{\"arguments\":\"{\\\"timezone\\\":\\\"Europe/Paris\\\"}\",\"call_id\":\"call_002\",\"name\":\"get_time\",\"type\":\"function_call\"}]",
            result);
    }
}
