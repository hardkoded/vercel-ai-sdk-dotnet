// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenResponses;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; providerOptions reasoning</c>.</summary>
public sealed class DoGenerateProviderOptionsReasoningTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > providerOptions reasoning::";

    [Fact]
    [UpstreamTest(Prefix + "should send a provider-native reasoning effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_a_provider_native_reasoning_effort()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("open-responses", "{\"reasoningEffort\":\"max\"}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"max\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer providerOptions reasoning effort over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_prefer_providerOptions_reasoning_effort_over_top_level_reasoning()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "low";
        options.ProviderOptions = UpstreamChat.Bag("open-responses", "{\"reasoningEffort\":\"max\",\"reasoningSummary\":\"detailed\"}");

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"max\",\"summary\":\"detailed\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should combine top-level reasoning effort with providerOptions summary", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_combine_top_level_reasoning_effort_with_providerOptions_summary()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";
        options.ProviderOptions = UpstreamChat.Bag("open-responses", "{\"reasoningSummary\":\"auto\"}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"high\",\"summary\":\"auto\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    private static OpenResponsesLanguageModel Model(UpstreamCapture capture)
    {
        return (OpenResponsesLanguageModel)OpenResponsesProvider.Create(new OpenResponsesOptions { ApiKey = "test-key", BaseUrl = "https://example.test/v1" }, capture).LanguageModel("gemma-7b-it");
    }
}
