// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>thinking options</c>.
/// </summary>
public sealed class DoGenerateThinkingOptionsTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "thinking options::";

    private static LanguageModelCallOptions With(string? reasoning = null, string? options = null)
    {
        var call = TestPrompt();
        call.Reasoning = reasoning;
        if (options != null)
        {
            call.ProviderOptions = Opts("{\"moonshotai\":" + options + "}");
        }

        return call;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit unsupported budget tokens and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_unsupported_budget_tokens_and_warn()
    {
        var (model, handler) = Model("kimi-k2.6", "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(
            With(options: "{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":2048}}"),
            CancellationToken.None
        );

        Strict(Body(handler)["thinking"], "{\"type\":\"enabled\"}");
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("deprecated", warning.Type);
        Assert.Equal("providerOptions.moonshotai.thinking.budgetTokens", warning.Setting);
        Assert.Equal(
            "Moonshot Chat Completions does not support budget_tokens. Remove budgetTokens; the option has been omitted.",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map reasoningHistory preserved to thinking.keep all",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_reasoningHistory_preserved_to_thinking_keep_all()
    {
        var (model, handler) = Model("kimi-k2.6", "moonshotai-reasoning");

        await model.DoGenerateAsync(
            With(
                options: "{\"thinking\":{\"type\":\"enabled\"},\"reasoningHistory\":\"preserved\"}"
            ),
            CancellationToken.None
        );

        var body = Body(handler);
        Strict(body["thinking"], "{\"type\":\"enabled\",\"keep\":\"all\"}");
        Assert.False(body.ContainsKey("reasoning_history"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not send reasoning_history for other reasoningHistory values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_send_reasoning_history_for_other_reasoningHistory_values()
    {
        var (model, handler) = Model("kimi-k2.6", "moonshotai-reasoning");

        await model.DoGenerateAsync(
            With(options: "{\"reasoningHistory\":\"interleaved\"}"),
            CancellationToken.None
        );

        var body = Body(handler);
        Assert.False(body.ContainsKey("reasoning_history"));
        Assert.False(body.ContainsKey("thinking"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through reasoning_effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_reasoning_effort()
    {
        var (model, handler) = Model("kimi-k3", "moonshotai-reasoning");

        await model.DoGenerateAsync(
            With(options: "{\"reasoningEffort\":\"max\"}"),
            CancellationToken.None
        );

        Assert.Equal("max", (string?)Body(handler)["reasoning_effort"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map generic reasoning levels to reasoning_effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_generic_reasoning_levels_to_reasoning_effort()
    {
        var (model, handler) = Model("kimi-k3", "moonshotai-reasoning");

        await model.DoGenerateAsync(With("low"), CancellationToken.None);
        Assert.Equal("low", (string?)Body(handler, 0)["reasoning_effort"]);

        await model.DoGenerateAsync(With("xhigh"), CancellationToken.None);
        Assert.Equal("high", (string?)Body(handler, 1)["reasoning_effort"]);

        await model.DoGenerateAsync(With("max"), CancellationToken.None);
        Assert.Equal("max", (string?)Body(handler, 2)["reasoning_effort"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer explicit reasoningEffort over generic reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_explicit_reasoningEffort_over_generic_reasoning()
    {
        var (model, handler) = Model("kimi-k3", "moonshotai-reasoning");

        await model.DoGenerateAsync(
            With("low", "{\"reasoningEffort\":\"high\"}"),
            CancellationToken.None
        );

        Assert.Equal("high", (string?)Body(handler)["reasoning_effort"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and omit reasoning_effort for reasoning none",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_omit_reasoning_effort_for_reasoning_none()
    {
        var (model, handler) = Model("kimi-k3", "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(With("none"), CancellationToken.None);

        Assert.Equal("low", (string?)Body(handler)["reasoning_effort"]);
        AssertWarnings(
            result.Warnings,
            (
                "compatibility",
                "reasoning",
                "reasoning \"none\" is not directly supported by this model. mapped to effort \"low\"."
            )
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit thinking for Kimi K3", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_thinking_for_Kimi_K3()
    {
        var (model, handler) = Model("kimi-k3", "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(
            With(
                options: "{\"thinking\":{\"type\":\"disabled\"},\"reasoningHistory\":\"preserved\"}"
            ),
            CancellationToken.None
        );

        Assert.False(Body(handler).ContainsKey("thinking"));
        AssertWarnings(
            result.Warnings,
            (
                "unsupported",
                "thinking",
                "Kimi K3 always reasons and does not accept the thinking field. The option has been omitted."
            )
        );
    }

    [Theory]
    [InlineData("kimi-k2.7-code")]
    [InlineData("kimi-k2.7-code-highspeed")]
    [UpstreamTest(
        Prefix + "should not disable thinking for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_disable_thinking_for(string modelId)
    {
        var (model, handler) = Model(modelId, "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(
            With(options: "{\"thinking\":{\"type\":\"disabled\"}}"),
            CancellationToken.None
        );

        Assert.False(Body(handler).ContainsKey("thinking"));
        AssertWarnings(
            result.Warnings,
            ("unsupported", "thinking.type \"disabled\"", "Kimi K2.7 thinking cannot be disabled.")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should rely on K2.7 preserved-thinking defaults",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_rely_on_K2_7_preserved_thinking_defaults()
    {
        var (model, handler) = Model("kimi-k2.7-code", "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(
            With(options: "{\"reasoningHistory\":\"preserved\"}"),
            CancellationToken.None
        );

        Assert.False(Body(handler).ContainsKey("thinking"));
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("kimi-k2.5")]
    [InlineData("kimi-k2.6")]
    [UpstreamTest(
        Prefix + "should map generic reasoning to thinking for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_generic_reasoning_to_thinking_for(string modelId)
    {
        var (model, handler) = Model(modelId, "moonshotai-reasoning");

        await model.DoGenerateAsync(With("low"), CancellationToken.None);
        Strict(Body(handler, 0)["thinking"], "{\"type\":\"enabled\"}");

        await model.DoGenerateAsync(With("none"), CancellationToken.None);
        Strict(Body(handler, 1)["thinking"], "{\"type\":\"disabled\"}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit reasoning effort for K2 models and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_reasoning_effort_for_K2_models_and_warn()
    {
        var (model, handler) = Model("kimi-k2.6", "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(
            With(options: "{\"reasoningEffort\":\"high\"}"),
            CancellationToken.None
        );

        Assert.False(Body(handler).ContainsKey("reasoning_effort"));
        AssertWarnings(
            result.Warnings,
            (
                "unsupported",
                "reasoningEffort",
                "reasoningEffort is only supported by Kimi K3 and has been omitted for model \"kimi-k2.6\"."
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit thinking and reasoning for Moonshot V1",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_thinking_and_reasoning_for_Moonshot_V1()
    {
        var (model, handler) = Model("moonshot-v1-8k", "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(
            With(
                "high",
                "{\"reasoningEffort\":\"high\",\"thinking\":{\"type\":\"enabled\"},\"reasoningHistory\":\"preserved\"}"
            ),
            CancellationToken.None
        );

        var body = Body(handler);
        Assert.False(body.ContainsKey("reasoning_effort"));
        Assert.False(body.ContainsKey("thinking"));
        AssertWarnings(
            result.Warnings,
            (
                "unsupported",
                "reasoningEffort",
                "reasoningEffort is only supported by Kimi K3 and has been omitted for model \"moonshot-v1-8k\"."
            ),
            (
                "unsupported",
                "thinking",
                "thinking is not supported by model \"moonshot-v1-8k\" and has been omitted."
            ),
            ("unsupported", "reasoning", "reasoning is not supported by model \"moonshot-v1-8k\"."),
            (
                "unsupported",
                "reasoningHistory 'preserved' is not supported by model \"moonshot-v1-8k\"",
                null
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit thinking.keep and warn on models without keep support",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_thinking_keep_and_warn_on_models_without_keep_support()
    {
        var (model, handler) = Model("kimi-k2.5", "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(
            With(options: "{\"reasoningHistory\":\"preserved\"}"),
            CancellationToken.None
        );

        var body = Body(handler);
        Assert.False(body.ContainsKey("thinking"));
        Assert.False(body.ContainsKey("reasoning_history"));
        AssertWarnings(
            result.Warnings,
            (
                "unsupported",
                "reasoningHistory 'preserved' is not supported by model \"kimi-k2.5\"",
                null
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract reasoning content and remap usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_reasoning_content_and_remap_usage()
    {
        var (model, _) = Model("kimi-k2.6", "moonshotai-reasoning");

        var result = await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(2, result.Content.Count);
        Assert.Equal(
            "Let me count the letters carefully.",
            Assert.IsType<GeneratedReasoning>(result.Content[0]).Text
        );
        Assert.Equal("There are three.", Assert.IsType<GeneratedText>(result.Content[1]).Text);
        Assert.Equal(20, result.Usage.InputTokens);
        Assert.Equal(10, result.Usage.NoCacheInputTokens);
        Assert.Equal(10, result.Usage.CacheReadTokens);
        Assert.Equal(30, result.Usage.OutputTokens);
        Assert.Equal(8, result.Usage.TextTokens);
        Assert.Equal(22, result.Usage.ReasoningTokens);
    }
}
