// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; providerOptions reasoning</c>.</summary>
public sealed class DoGenerateProviderOptionsReasoningTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > providerOptions reasoning::";

    [Fact]
    [UpstreamTest(Prefix + "should send a provider-native reasoning effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_a_provider_native_reasoning_effort()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("lmstudio", "{\"reasoningEffort\":\"max\"}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"max\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer providerOptions reasoning effort over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_prefer_providerOptions_reasoning_effort_over_top_level_reasoning()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.Reasoning = "low";
        options.ProviderOptions = UpstreamChat.Bag("lmstudio", "{\"reasoningEffort\":\"max\",\"reasoningSummary\":\"detailed\"}");

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"max\",\"summary\":\"detailed\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send reasoning.summary via providerOptions", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_reasoning_summary_via_providerOptions()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("lmstudio", "{\"reasoningSummary\":\"detailed\"}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it","reasoning":{"summary":"detailed"}}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should combine top-level reasoning effort with providerOptions summary", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_combine_top_level_reasoning_effort_with_providerOptions_summary()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";
        options.ProviderOptions = UpstreamChat.Bag("lmstudio", "{\"reasoningSummary\":\"auto\"}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it","reasoning":{"effort":"high","summary":"auto"}}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send reasoning.summary concise via providerOptions", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_reasoning_summary_concise_via_providerOptions()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("lmstudio", "{\"reasoningSummary\":\"concise\"}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it","reasoning":{"summary":"concise"}}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not set reasoning when providerOptions has no reasoning fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_set_reasoning_when_providerOptions_has_no_reasoning_fields()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("lmstudio", "{}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it"}
            """);
    }
}
