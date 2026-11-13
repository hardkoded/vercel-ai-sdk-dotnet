// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput.ConvertToDeepSeekResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput;

/// <summary>Port of <c>convert-to-deepseek-responses-input.test.ts</c> &gt; <c>convertToDeepSeekResponsesInput &gt; multi-turn conversations</c>.</summary>
public sealed class MultiTurnConversationsTests
{
    private const string Prefix = TestFile + " > multi-turn conversations::";

    [Fact]
    [UpstreamTest(Prefix + "should preserve a full conversation with reasoning and a tool roundtrip", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_a_full_conversation_with_reasoning_and_a_tool_roundtrip()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(
            warnings,
            new SystemModelMessage("You are a helpful assistant."),
            User(new TextContentPart("What is the weather in Paris?")),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("I need the current weather.", Opts(("deepseek", """{"itemId":"rs_1"}"""))),
                new AssistantToolCallPart(new GeneratedToolCall("call_1", "weather", """{"location":"Paris"}""")),
            }),
            ToolText("call_1", "weather", """{"temperature":20}""", "json"),
            new AssistantModelMessage("It is 20 degrees in Paris.", null, null),
            User(new TextContentPart("And in London?")));

        JsonAssert.Equal(input, """
            [{"content":"You are a helpful assistant.","role":"system"},
             {"content":[{"text":"What is the weather in Paris?","type":"input_text"}],"role":"user"},
             {"content":[{"text":"I need the current weather.","type":"reasoning_text"}],"id":"rs_1","type":"reasoning"},
             {"arguments":"{\"location\":\"Paris\"}","call_id":"call_1","name":"weather","type":"function_call"},
             {"call_id":"call_1","output":"{\"temperature\":20}","type":"function_call_output"},
             {"content":"It is 20 degrees in Paris.","role":"assistant"},
             {"content":[{"text":"And in London?","type":"input_text"}],"role":"user"}]
            """);
        Assert.Empty(warnings);
    }
}
