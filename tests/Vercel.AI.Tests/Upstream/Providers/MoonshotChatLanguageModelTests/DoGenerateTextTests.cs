// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; text</c> (sampling options).</summary>
public sealed class DoGenerateTextTests
{
    private const string Prefix =
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > text::";

    [Theory]
    [InlineData("kimi-k2.5")]
    [InlineData("kimi-k2.6")]
    [InlineData("kimi-k2.7-code")]
    [InlineData("kimi-k2.7-code-highspeed")]
    [InlineData("kimi-k3")]
    [InlineData("kimi-k3.1-pro")]
    [InlineData("kimi-k3.1-pro-highspeed")]
    [UpstreamTest(
        Prefix + "should omit fixed sampling options and warn for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_fixed_sampling_options_and_warn_for_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;

        var result = await Model(capture, modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    $"{{\"model\":\"{modelId}\",\"messages\":[{{\"role\":\"user\",\"content\":\"Hello\"}}]}}"
                ),
                UpstreamChat.Body(capture)
            )
        );
        Assert.Collection(
            result.Warnings,
            w => AssertFixed(w, "temperature", modelId),
            w => AssertFixed(w, "topP", modelId),
            w => AssertFixed(w, "frequencyPenalty", modelId),
            w => AssertFixed(w, "presencePenalty", modelId)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve sampling options for custom model IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_sampling_options_for_custom_model_IDs()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;

        await Model(capture, "custom-model").DoGenerateAsync(options, CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.Equal("custom-model", body["model"]!.GetValue<string>());
        Assert.Equal(0.2, body["temperature"]!.GetValue<double>());
        Assert.Equal(0.4, body["top_p"]!.GetValue<double>());
        Assert.Equal(0.5, body["frequency_penalty"]!.GetValue<double>());
        Assert.Equal(0.6, body["presence_penalty"]!.GetValue<double>());
    }

    private static void AssertFixed(
        Vercel.AI.Provider.CallWarning warning,
        string feature,
        string modelId
    )
    {
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal(feature, warning.Feature);
        Assert.Equal(
            $"{feature} is fixed by model \"{modelId}\" and has been omitted.",
            warning.Message
        );
    }

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, string modelId) =>
        (OpenAICompatibleLanguageModel)
            MoonshotProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture)
                .LanguageModel(modelId);
}
