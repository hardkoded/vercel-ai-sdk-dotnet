// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput.ConvertToOpenResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; message chains</c>.</summary>
public sealed class MessageChainsTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > message chains::";

    [Fact]
    [UpstreamTest(Prefix + "should convert user - assistant - user message chain", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_user_assistant_user_message_chain()
    {
        var result = Convert(
            new UserModelMessage("What is the capital of France?"),
            AssistantText("The capital of France is Paris."),
            new UserModelMessage("And what about Germany?"));

        JsonAssert.Equal(result.Input, """
            [
              {"content":[{"text":"What is the capital of France?","type":"input_text"}],"role":"user","type":"message"},
              {"content":[{"text":"The capital of France is Paris.","type":"output_text"}],"role":"assistant","type":"message"},
              {"content":[{"text":"And what about Germany?","type":"input_text"}],"role":"user","type":"message"}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert user - assistant (tool call) - tool message chain", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_user_assistant_tool_call_tool_message_chain()
    {
        var result = Convert(
            new UserModelMessage("What is the weather in Tokyo?"),
            AssistantToolCalls(null, new GeneratedToolCall("call_weather", "get_weather", """{"location":"Tokyo"}""")),
            new ToolModelMessage("call_weather", "get_weather", """{"temperature":25,"condition":"cloudy"}""", false, "json"));

        JsonAssert.Equal(result.Input, """
            [
              {"content":[{"text":"What is the weather in Tokyo?","type":"input_text"}],"role":"user","type":"message"},
              {"arguments":"{\"location\":\"Tokyo\"}","call_id":"call_weather","name":"get_weather","type":"function_call"},
              {"call_id":"call_weather","output":"{\"temperature\":25,\"condition\":\"cloudy\"}","type":"function_call_output"}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert a tool roundtrip with follow-up assistant message", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_a_tool_roundtrip_with_follow_up_assistant_message()
    {
        var result = Convert(
            new UserModelMessage("What is the weather in Tokyo?"),
            AssistantToolCalls(null, new GeneratedToolCall("call_weather", "get_weather", """{"location":"Tokyo"}""")),
            new ToolModelMessage("call_weather", "get_weather", """{"temperature":25,"condition":"cloudy"}""", false, "json"),
            AssistantText("It is 25 C and cloudy in Tokyo."));

        JsonAssert.Equal(result.Input, """
            [
              {"content":[{"text":"What is the weather in Tokyo?","type":"input_text"}],"role":"user","type":"message"},
              {"arguments":"{\"location\":\"Tokyo\"}","call_id":"call_weather","name":"get_weather","type":"function_call"},
              {"call_id":"call_weather","output":"{\"temperature\":25,\"condition\":\"cloudy\"}","type":"function_call_output"},
              {"content":[{"text":"It is 25 C and cloudy in Tokyo.","type":"output_text"}],"role":"assistant","type":"message"}
            ]
            """);
    }
}
