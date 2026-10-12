// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; tool choice</c>.</summary>
public sealed class DoGenerateToolChoiceTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > tool choice::";

    [Fact]
    [UpstreamTest(Prefix + "should send tool_choice auto", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_tool_choice_auto()
    {
        var capture = await Send(ToolChoice.Auto);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it","tool_choice":"auto","tools":[{"description":"Get the current weather","name":"get_weather","parameters":{"properties":{"location":{"type":"string"}},"required":["location"],"type":"object"},"type":"function"}]}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send tool_choice none", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_tool_choice_none()
    {
        var capture = await Send(ToolChoice.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it","tool_choice":"none","tools":[{"description":"Get the current weather","name":"get_weather","parameters":{"properties":{"location":{"type":"string"}},"required":["location"],"type":"object"},"type":"function"}]}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send tool_choice required", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_tool_choice_required()
    {
        var capture = await Send(ToolChoice.Required);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it","tool_choice":"required","tools":[{"description":"Get the current weather","name":"get_weather","parameters":{"properties":{"location":{"type":"string"}},"required":["location"],"type":"object"},"type":"function"}]}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send tool_choice with specific tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_tool_choice_with_specific_tool()
    {
        var capture = await Send(ToolChoice.Tool("get_weather"));

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it","tool_choice":{"name":"get_weather","type":"function"},"tools":[{"description":"Get the current weather","name":"get_weather","parameters":{"properties":{"location":{"type":"string"}},"required":["location"],"type":"object"},"type":"function"}]}
            """);
    }

    private static async Task<UpstreamCapture> Send(ToolChoice toolChoice)
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.Tools = new[] { Tool("get_weather", "Get the current weather", """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"]}""") };
        options.ToolChoice = toolChoice;
        await Model(capture).DoGenerateAsync(options, CancellationToken.None);
        return capture;
    }
}
