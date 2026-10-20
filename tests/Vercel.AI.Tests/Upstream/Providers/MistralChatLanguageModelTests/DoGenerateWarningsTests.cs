// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; warnings</c>.</summary>
public sealed class DoGenerateWarningsTests
{
    private const string Prefix = "packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > warnings::";

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported reasoning for non-supporting models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_about_unsupported_reasoning_for_non_supporting_models()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "medium";

        var result = await Model(capture, "mistral-large-latest").DoGenerateAsync(options, CancellationToken.None);

        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported"
            && warning.Feature == "reasoning"
            && warning.Message == "This model does not support reasoning configuration.");
    }

    [Fact]
    [UpstreamTest(Prefix + "should emit compatibility warning for reasoning medium on supporting model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_emit_compatibility_warning_for_reasoning_medium_on_supporting_model()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "medium";

        var result = await Model(capture, "mistral-small-latest").DoGenerateAsync(options, CancellationToken.None);

        Assert.Contains(result.Warnings, warning => warning.Type == "compatibility" && warning.Feature == "reasoning");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not warn for reasoning high on supporting model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_warn_for_reasoning_high_on_supporting_model()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";

        var result = await Model(capture, "mistral-small-latest").DoGenerateAsync(options, CancellationToken.None);

        Assert.Empty(result.Warnings);
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
    [UpstreamTest(Prefix + "should not warn about reasoning for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_warn_about_reasoning_for_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";

        var result = await Model(capture, modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.DoesNotContain(result.Warnings, warning => warning.Type == "unsupported");
    }

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, string modelId)
    {
        return (OpenAICompatibleLanguageModel)MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture).LanguageModel(modelId);
    }
}
