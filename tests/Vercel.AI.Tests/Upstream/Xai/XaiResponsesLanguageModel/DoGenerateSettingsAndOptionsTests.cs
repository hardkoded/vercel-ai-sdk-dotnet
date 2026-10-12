// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; settings and options</c>.</summary>
public sealed class DoGenerateSettingsAndOptionsTests
{
    private const string Prefix = TestFile + " > doGenerate > settings and options::";

    [Fact]
    [UpstreamTest(Prefix + "should send model id and settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_model_id_and_settings()
    {
        var capture = EmptyResponse();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("you are helpful"), new UserModelMessage("hello") },
            Temperature = 0.5,
            TopP = 0.9,
            MaxOutputTokens = 100,
        };

        await Model(capture, "grok-4-fast-non-reasoning").DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(Body(capture), """
            {"input":[{"content":"you are helpful","role":"system"},{"content":[{"text":"hello","type":"input_text"}],"role":"user"}],
             "max_output_tokens":100,"model":"grok-4-fast-non-reasoning","temperature":0.5,"top_p":0.9}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported stopSequences", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_about_unsupported_stopSequences()
    {
        var options = HelloPrompt();
        options.StopSequences = new[] { "\n\n", "STOP" };

        var result = await Model(EmptyResponse()).DoGenerateAsync(options, CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("stopSequences", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported sampling settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_about_unsupported_sampling_settings()
    {
        var options = HelloPrompt();
        options.TopK = 10;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.5;

        var result = await Model(EmptyResponse()).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(new[] { "frequencyPenalty", "presencePenalty" }, result.Warnings.Select(warning => warning.Message));
        Assert.All(result.Warnings, warning => Assert.Equal("unsupported", warning.Type));
    }
}
