// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Fireworks;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.FireworksChatLanguageModelTests;

/// <summary>Port of <c>fireworks-chat-language-model.test.ts</c> &gt; <c>Fireworks chat reasoning</c>.</summary>
public sealed class FireworksChatReasoningTests
{
    private const string Prefix = "packages/fireworks/src/fireworks-chat-language-model.test.ts::Fireworks chat reasoning::";

    [Theory]
    [InlineData("minimal", "low")]
    [InlineData("xhigh", "high")]
    [InlineData("max", "high")]
    [UpstreamTest(Prefix + "should coerce \"%s\" to \"%s\" with a warning in doGenerate", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_coerce_reasoning_to_effort_with_a_warning_in_doGenerate(string reasoning, string effort)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = reasoning;

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(effort, UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + effort + "\".", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return the max compatibility warning in stream-start", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_the_max_compatibility_warning_in_stream_start()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = UpstreamChat.Sse("{\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\"},\"finish_reason\":\"stop\"}]}"),
            MediaType = "text/event-stream",
        };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";
        options.TopK = 1;

        var parts = await UpstreamChat.Read(Model(capture).DoStreamAsync(options, CancellationToken.None));

        var body = UpstreamChat.Body(capture);
        Assert.Equal("high", body["reasoning_effort"]!.GetValue<string>());
        Assert.True(body["stream"]!.GetValue<bool>());
        var start = Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Collection(
            start.Warnings,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("topK", warning.Message);
            },
            warning =>
            {
                Assert.Equal("compatibility", warning.Type);
                Assert.Equal("reasoning \"max\" is not directly supported by this model. mapped to effort \"high\".", warning.Message);
            });
    }

    [Theory]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [UpstreamTest(Prefix + "should pass through \"%s\" without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_reasoning_without_warnings(string reasoning)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = reasoning;

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(reasoning, UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should honor provider reasoning effort without warning about ignored portable max", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_honor_provider_reasoning_effort_without_warning_about_ignored_portable_max()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";
        options.ProviderOptions = UpstreamChat.Bag("fireworks", "{\"reasoningEffort\":\"medium\"}");

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("medium", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture)
    {
        return (OpenAICompatibleLanguageModel)FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, capture).LanguageModel("test-model");
    }
}
