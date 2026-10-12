// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; multi-turn tool conversation</c>.</summary>
public sealed class DoGenerateMultiTurnToolConversationTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > multi-turn tool conversation::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body with user, assistant tool-call, and tool result", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body_with_user_assistant_tool_call_and_tool_result()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("What is the weather in Tokyo?"),
                new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_weather_123", "get_weather", """{"location":"Tokyo"}""") }, null),
                new ToolModelMessage("call_weather_123", "get_weather", """{"temperature":22,"condition":"sunny","humidity":65}""", false, "json"),
            },
            Tools = new[] { Tool("get_weather", "Get the current weather for a location", """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"]}""") },
        };

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {
              "input":[
                {"content":[{"text":"What is the weather in Tokyo?","type":"input_text"}],"role":"user","type":"message"},
                {"arguments":"{\"location\":\"Tokyo\"}","call_id":"call_weather_123","name":"get_weather","type":"function_call"},
                {"call_id":"call_weather_123","output":"{\"temperature\":22,\"condition\":\"sunny\",\"humidity\":65}","type":"function_call_output"}
              ],
              "model":"gemma-7b-it",
              "tools":[{"description":"Get the current weather for a location","name":"get_weather","parameters":{"properties":{"location":{"type":"string"}},"required":["location"],"type":"object"},"type":"function"}]
            }
            """);
    }
}
