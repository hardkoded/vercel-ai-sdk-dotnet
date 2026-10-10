// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; reasoning_effort</c>.</summary>
public sealed class DoGenerateReasoningEffortTests
{
    private const string Prefix = "packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning_effort::";

    [Fact]
    [UpstreamTest(Prefix + "should send reasoning_effort high for reasoning high", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_reasoning_effort_high_for_reasoning_high()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should send reasoning_effort high for reasoning medium", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_reasoning_effort_high_for_reasoning_medium()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "medium";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should send reasoning_effort high for reasoning minimal", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_reasoning_effort_high_for_reasoning_minimal()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "minimal";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should send reasoning_effort high with a warning for reasoning max", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_reasoning_effort_high_with_a_warning_for_reasoning_max()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning \"max\" is not directly supported by this model. mapped to effort \"high\".", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send reasoning_effort none for reasoning none", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_reasoning_effort_none_for_reasoning_none()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("none", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should allow provider option to override reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_allow_provider_option_to_override_reasoning()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";
        options.ProviderOptions = UpstreamChat.Bag("mistral", "{\"reasoningEffort\":\"none\"}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("none", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("magistral-large-latest")]
    [InlineData("mistral-large-5")]
    [InlineData("future-reasoning-model")]
    [UpstreamTest(Prefix + "forwards reasoning and provider overrides for future model %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_reasoning_and_provider_overrides_for_future_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var model = Model(capture, modelId);
        var first = UpstreamChat.Prompt();
        first.Reasoning = "high";

        var result = await model.DoGenerateAsync(first, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Empty(result.Warnings);

        var second = UpstreamChat.Prompt();
        second.Reasoning = "high";
        second.ProviderOptions = UpstreamChat.Bag("mistral", "{\"reasoningEffort\":\"none\"}");
        await model.DoGenerateAsync(second, CancellationToken.None);

        Assert.Equal("none", System.Text.Json.Nodes.JsonNode.Parse(capture.Requests[1].Body)!["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send reasoning_effort for non-supporting models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_send_reasoning_effort_for_non_supporting_models()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";

        await Model(capture, "mistral-large-latest").DoGenerateAsync(options, CancellationToken.None);

        Assert.False(UpstreamChat.Body(capture).ContainsKey("reasoning_effort"));
    }

    [Theory]
    [InlineData("mistral-large-4")]
    [InlineData("mistral-large-4-0")]
    [InlineData("mistral-medium-3-5")]
    [InlineData("mistral-medium-latest")]
    [InlineData("mistral-vibe-cli-fast")]
    [InlineData("zai-glm-5-2")]
    [InlineData("glm-5-2")]
    [InlineData("labs-leanstral-1-5")]
    [UpstreamTest(Prefix + "should send reasoning_effort for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_reasoning_effort_for_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";

        await Model(capture, modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("mistral-large-4")]
    [InlineData("mistral-large-4-0")]
    [UpstreamTest(Prefix + "should send provider reasoning_effort for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_provider_reasoning_effort_for_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("mistral", "{\"reasoningEffort\":\"high\"}");

        await Model(capture, modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, string modelId = "mistral-small-latest")
    {
        return (OpenAICompatibleLanguageModel)MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture).LanguageModel(modelId);
    }
}
