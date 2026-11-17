// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; top-level reasoning</c>.</summary>
public sealed class DoGenerateTopLevelReasoningTests
{
    private const string Prefix = TestFile + " > doGenerate > top-level reasoning::";

    private static void AssertNoReasoningWarning(
        Vercel.AI.Provider.LanguageModelGenerateResult result
    )
    {
        Assert.DoesNotContain(result.Warnings, warning =>
            warning.Type == "compatibility" && warning.Feature == "reasoning"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map top-level reasoning to thinking enabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_top_level_reasoning_to_thinking_enabled()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "high";

        await Model(capture, "deepseek-reasoner").DoGenerateAsync(options, CancellationToken.None);

        var requestBody = RequestBody(capture);
        AssertJson(requestBody["thinking"], """{"type":"enabled"}""");
        Assert.Equal("high", requestBody["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map top-level reasoning none to thinking disabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_top_level_reasoning_none_to_thinking_disabled()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "none";

        await Model(capture, "deepseek-reasoner").DoGenerateAsync(options, CancellationToken.None);

        var requestBody = RequestBody(capture);
        AssertJson(requestBody["thinking"], """{"type":"disabled"}""");
        Assert.Null(requestBody["reasoning_effort"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map top-level reasoning xhigh to reasoning_effort high",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_top_level_reasoning_xhigh_to_reasoning_effort_high()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "xhigh";

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", RequestBody(capture)["reasoning_effort"]!.GetValue<string>());
        AssertContainsWarning(
            result,
            "compatibility",
            "reasoning",
            "reasoning \"xhigh\" is not directly supported by this model. mapped to effort \"high\"."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map top-level reasoning max directly to reasoning_effort max",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_top_level_reasoning_max_directly_to_reasoning_effort_max()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "max";

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("max", RequestBody(capture)["reasoning_effort"]!.GetValue<string>());
        AssertNoReasoningWarning(result);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should map top-level reasoning low to reasoning_effort low without a compatibility warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_top_level_reasoning_low_to_reasoning_effort_low_without_a_compatibility_warning()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "low";

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("low", RequestBody(capture)["reasoning_effort"]!.GetValue<string>());
        AssertNoReasoningWarning(result);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should map top-level reasoning medium to reasoning_effort low with a compatibility warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_top_level_reasoning_medium_to_reasoning_effort_low_with_a_compatibility_warning()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "medium";

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("low", RequestBody(capture)["reasoning_effort"]!.GetValue<string>());
        AssertContainsWarning(
            result,
            "compatibility",
            "reasoning",
            "reasoning \"medium\" is not directly supported by this model. mapped to effort \"low\"."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should map top-level reasoning minimal to reasoning_effort low with compatibility warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_top_level_reasoning_minimal_to_reasoning_effort_low_with_compatibility_warning()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "minimal";

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("low", RequestBody(capture)["reasoning_effort"]!.GetValue<string>());
        AssertContainsWarning(
            result,
            "compatibility",
            "reasoning",
            "reasoning \"minimal\" is not directly supported by this model. mapped to effort \"low\"."
        );
    }

    [Theory]
    [InlineData("low", "low", false)]
    [InlineData("medium", "high", true)]
    [InlineData("xhigh", "max", true)]
    [UpstreamTest(
        Prefix + "should map providerOptions reasoningEffort $input to $output",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_providerOptions_reasoningEffort_input_to_output(
        string input,
        string output,
        bool warning
    )
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.ProviderOptions = DeepSeekOptions("{\"reasoningEffort\":\"" + input + "\"}");

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(output, RequestBody(capture)["reasoning_effort"]!.GetValue<string>());
        if (warning)
        {
            AssertWarnings(
                result,
                (
                    "compatibility",
                    "reasoningEffort",
                    "reasoningEffort \""
                        + input
                        + "\" is not a canonical DeepSeek value. mapped to \""
                        + output
                        + "\"."
                )
            );
        }
        else
        {
            AssertWarnings(result);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should map legacy providerOptions thinking.type=adaptive to enabled with a compatibility warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_legacy_providerOptions_thinking_type_adaptive_to_enabled_with_a_compatibility_warning()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.ProviderOptions = DeepSeekOptions("""{"thinking":{"type":"adaptive"}}""");

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(options, CancellationToken.None);

        AssertJson(RequestBody(capture)["thinking"], """{"type":"enabled"}""");
        AssertContainsWarning(
            result,
            "compatibility",
            "thinking.type",
            "thinking.type \"adaptive\" is not a canonical DeepSeek value. mapped to \"enabled\"."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass providerOptions reasoningEffort",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_providerOptions_reasoningEffort()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.ProviderOptions = DeepSeekOptions("""{"reasoningEffort":"max"}""");

        await Model(capture, "deepseek-reasoner").DoGenerateAsync(options, CancellationToken.None);

        var requestBody = RequestBody(capture);
        Assert.Equal("max", requestBody["reasoning_effort"]!.GetValue<string>());
        // When only reasoningEffort is set without thinking, thinking should be
        // undefined and rely on the API default (enabled).
        Assert.Null(requestBody["thinking"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer providerOptions thinking over top-level reasoning",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_prefer_providerOptions_thinking_over_top_level_reasoning()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "none";
        options.ProviderOptions = DeepSeekOptions("""{"thinking":{"type":"enabled"}}""");

        await Model(capture, "deepseek-reasoner").DoGenerateAsync(options, CancellationToken.None);

        AssertJson(RequestBody(capture)["thinking"], """{"type":"enabled"}""");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer providerOptions reasoningEffort over top-level reasoning",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_prefer_providerOptions_reasoningEffort_over_top_level_reasoning()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Reasoning = "high";
        options.ProviderOptions = DeepSeekOptions("""{"reasoningEffort":"max"}""");

        await Model(capture, "deepseek-reasoner").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("max", RequestBody(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not set thinking when reasoning is not specified",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_set_thinking_when_reasoning_is_not_specified()
    {
        var capture = JsonFixture("deepseek-text");

        await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Null(RequestBody(capture)["thinking"]);
    }
}
