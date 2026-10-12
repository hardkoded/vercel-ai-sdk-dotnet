// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenResponses;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; top-level reasoning</c>.</summary>
public sealed class DoGenerateTopLevelReasoningTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > top-level reasoning::";

    [Fact]
    [UpstreamTest(Prefix + "should map top-level reasoning to reasoning effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_top_level_reasoning_to_reasoning_effort()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"high\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should coerce top-level reasoning minimal to low", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_coerce_top_level_reasoning_minimal_to_low()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "minimal";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"low\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should map top-level reasoning none to none", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_top_level_reasoning_none_to_none()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"none\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass xhigh directly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_xhigh_directly()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "xhigh";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"xhigh\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass max directly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_max_directly()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"effort\":\"max\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should not set reasoning when not specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_set_reasoning_when_not_specified()
    {
        var capture = new UpstreamCapture { ResponseBody = OpenResponsesTestSupport.Fixture("lmstudio-basic.1") };
        var options = UpstreamChat.Prompt();

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.False(UpstreamChat.Body(capture).ContainsKey("reasoning"));
    }

    private static OpenResponsesLanguageModel Model(UpstreamCapture capture)
    {
        return (OpenResponsesLanguageModel)OpenResponsesProvider.Create(new OpenResponsesOptions { ApiKey = "test-key", BaseUrl = "https://example.test/v1" }, capture).LanguageModel("gemma-7b-it");
    }
}
