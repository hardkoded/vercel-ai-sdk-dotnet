// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; thinking options</c>.</summary>
public sealed class DoGenerateThinkingOptionsTests
{
    private const string Prefix =
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > thinking options::";

    [Fact]
    [UpstreamTest(
        Prefix + "should map generic reasoning levels to reasoning_effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_generic_reasoning_levels_to_reasoning_effort()
    {
        var capture = new UpstreamCapture();
        var provider = Provider(capture);
        var expected = new[] { ("low", "low"), ("xhigh", "high"), ("max", "max") };

        foreach (var (reasoning, _) in expected)
        {
            var options = UpstreamChat.Prompt();
            options.Reasoning = reasoning;
            await Model(provider, "kimi-k3").DoGenerateAsync(options, CancellationToken.None);
        }

        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Equal(
                expected[index].Item2,
                Body(capture, index)["reasoning_effort"]!.GetValue<string>()
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map reasoning none to the lowest reasoning_effort with a warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_map_reasoning_none_to_the_lowest_reasoning_effort_with_a_warning()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        var result = await Model(Provider(capture), "kimi-k3")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("low", Body(capture, 0)["reasoning_effort"]!.GetValue<string>());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal(
            "reasoning \"none\" is not directly supported by this model. mapped to effort \"low\".",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map reasoning none to thinking enabled for Kimi K2.7 with a warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_map_reasoning_none_to_thinking_enabled_for_Kimi_K2_7_with_a_warning()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        var result = await Model(Provider(capture), "kimi-k2.7-code")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.False(Body(capture, 0).ContainsKey("thinking"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal(
            "reasoning \"none\" is not supported by Kimi K2.7; thinking cannot be disabled. Using thinking enabled instead.",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map reasoning none to disabled thinking for unknown models",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_map_reasoning_none_to_disabled_thinking_for_unknown_models()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        var result = await Model(Provider(capture), "custom-model")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture, 0);
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"disabled\"}"), body["thinking"])
        );
        Assert.False(body.ContainsKey("reasoning_effort"));
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("kimi-k3.1-pro")]
    [InlineData("kimi-k3.1-pro-highspeed")]
    [UpstreamTest(
        Prefix + "should map generic reasoning to reasoning_effort for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_map_generic_reasoning_to_reasoning_effort_for_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "medium";

        await Model(Provider(capture), modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("low", Body(capture, 0)["reasoning_effort"]!.GetValue<string>());
    }

    private static MoonshotProvider Provider(UpstreamCapture capture) =>
        MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);

    private static OpenAICompatibleLanguageModel Model(MoonshotProvider provider, string modelId) =>
        (OpenAICompatibleLanguageModel)provider.LanguageModel(modelId);

    private static JsonObject Body(UpstreamCapture capture, int index) =>
        (JsonObject)JsonNode.Parse(capture.Requests[index].Body)!;
}
