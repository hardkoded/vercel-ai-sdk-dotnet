// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; settings and options &gt; top-level reasoning</c>.</summary>
public sealed class DoGenerateSettingsAndOptionsTopLevelReasoningTests
{
    private const string Prefix = TestFile + " > doGenerate > settings and options > top-level reasoning::";

    [Fact]
    [UpstreamTest(Prefix + "should map top-level reasoning to reasoning effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_top_level_reasoning_to_reasoning_effort()
    {
        var capture = EmptyResponse("grok-4.3");
        var options = HelloPrompt();
        options.Reasoning = "high";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", (string?)Body(capture)["reasoning"]!["effort"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map top-level reasoning none to reasoning effort \"none\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_top_level_reasoning_none_to_reasoning_effort_none()
    {
        var capture = EmptyResponse("grok-4.3");
        var options = HelloPrompt();
        options.Reasoning = "none";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("none", (string?)Body(capture)["reasoning"]!["effort"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should coerce top-level reasoning xhigh to reasoning effort \"high\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_coerce_top_level_reasoning_xhigh_to_reasoning_effort_high()
    {
        var capture = EmptyResponse("grok-4.3");
        var options = HelloPrompt();
        options.Reasoning = "xhigh";

        await Model(capture, "grok-4.3").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", (string?)Body(capture)["reasoning"]!["effort"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map top-level reasoning xhigh to reasoning effort \"xhigh\" for grok-4.6", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_top_level_reasoning_xhigh_to_reasoning_effort_xhigh_for_grok_4_6()
    {
        var capture = EmptyResponse("grok-4.6");
        var options = HelloPrompt();
        options.Reasoning = "xhigh";

        await Model(capture, "grok-4.6").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("xhigh", (string?)Body(capture)["reasoning"]!["effort"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should coerce top-level reasoning max to reasoning effort \"xhigh\" for grok-4.6", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_coerce_top_level_reasoning_max_to_reasoning_effort_xhigh_for_grok_4_6()
    {
        var capture = EmptyResponse("grok-4.6");
        var options = HelloPrompt();
        options.Reasoning = "max";

        var result = await Model(capture, "grok-4.6").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("xhigh", (string?)Body(capture)["reasoning"]!["effort"]);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning \"max\" is not directly supported by this model. mapped to effort \"xhigh\".", warning.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer providerOptions reasoningEffort over top-level reasoning",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_prefer_providerOptions_reasoningEffort_over_top_level_reasoning()
    {
        var capture = EmptyResponse("grok-4.3");
        var options = HelloPrompt();
        options.Reasoning = "none";
        options.ProviderOptions = UpstreamChat.Bag("xai", """{"reasoningEffort":"high"}""");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", (string?)Body(capture)["reasoning"]!["effort"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit reasoning effort and warn for models that do not support it", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_reasoning_effort_and_warn_for_models_that_do_not_support_it()
    {
        var capture = EmptyResponse("grok-4.20-reasoning");
        var options = HelloPrompt();
        options.Reasoning = "none";

        var result = await Model(capture, "grok-4.20-reasoning").DoGenerateAsync(options, CancellationToken.None);

        Assert.False(Body(capture).ContainsKey("reasoning"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("reasoning \"none\" is not supported by this model.", warning.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should still pass providerOptions reasoningEffort for models that do not support top-level reasoning",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_still_pass_providerOptions_reasoningEffort_for_models_that_do_not_support_top_level_reasoning()
    {
        var capture = EmptyResponse("grok-4.20-reasoning");
        var options = HelloPrompt();
        options.ProviderOptions = UpstreamChat.Bag("xai", """{"reasoningEffort":"none"}""");

        await Model(capture, "grok-4.20-reasoning").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("none", (string?)Body(capture)["reasoning"]!["effort"]);
    }
}
