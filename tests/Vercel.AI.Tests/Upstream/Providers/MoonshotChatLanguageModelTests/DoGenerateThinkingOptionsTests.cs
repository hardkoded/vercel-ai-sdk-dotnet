// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; thinking options</c>.</summary>
public sealed class DoGenerateThinkingOptionsTests
{
    private const string Prefix =
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > thinking options::";

    [Fact]
    [UpstreamTest(
        Prefix + "should map generic reasoning levels to reasoning_effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_generic_reasoning_levels_to_reasoning_effort()
    {
        var capture = Reasoning();
        var provider = Provider(capture);
        var expected = new[] { ("low", "low"), ("xhigh", "high"), ("max", "max") };

        foreach (var (reasoning, _) in expected)
        {
            var options = UpstreamChat.Prompt();
            options.Reasoning = reasoning;
            await Model(provider, "kimi-k3").DoGenerateAsync(options, CancellationToken.None);
        }

        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Equal(
                expected[index].Item2,
                Body(capture, index)["reasoning_effort"]!.GetValue<string>()
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map reasoning none to the lowest reasoning_effort with a warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_map_reasoning_none_to_the_lowest_reasoning_effort_with_a_warning()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        var result = await Model(Provider(capture), "kimi-k3")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("low", Body(capture, 0)["reasoning_effort"]!.GetValue<string>());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal(
            "reasoning \"none\" is not directly supported by this model. mapped to effort \"low\".",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map reasoning none to thinking enabled for Kimi K2.7 with a warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_map_reasoning_none_to_thinking_enabled_for_Kimi_K2_7_with_a_warning()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        var result = await Model(Provider(capture), "kimi-k2.7-code")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.False(Body(capture, 0).ContainsKey("thinking"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal(
            "reasoning \"none\" is not supported by Kimi K2.7; thinking cannot be disabled. Using thinking enabled instead.",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map reasoning none to disabled thinking for unknown models",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_map_reasoning_none_to_disabled_thinking_for_unknown_models()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        var result = await Model(Provider(capture), "custom-model")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture, 0);
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"disabled\"}"), body["thinking"])
        );
        Assert.False(body.ContainsKey("reasoning_effort"));
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("kimi-k3.1-pro")]
    [InlineData("kimi-k3.1-pro-highspeed")]
    [UpstreamTest(
        Prefix + "should map generic reasoning to reasoning_effort for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_map_generic_reasoning_to_reasoning_effort_for_model(string modelId)
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "medium";

        await Model(Provider(capture), modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("low", Body(capture, 0)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit unsupported budget tokens and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_unsupported_budget_tokens_and_warn()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":2048}}"
        );

        var result = await Model(Provider(capture), "kimi-k2.6")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"type\":\"enabled\"}"),
                Body(capture, 0)["thinking"]
            )
        );
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
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"thinking\":{\"type\":\"enabled\"},\"reasoningHistory\":\"preserved\"}"
        );

        await Model(Provider(capture), "kimi-k2.6")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture, 0);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"type\":\"enabled\",\"keep\":\"all\"}"),
                body["thinking"]
            )
        );
        Assert.False(body.ContainsKey("reasoning_history"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not send reasoning_history for other reasoningHistory values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_send_reasoning_history_for_other_reasoningHistory_values()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"reasoningHistory\":\"interleaved\"}"
        );

        await Model(Provider(capture), "kimi-k2.6")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture, 0);
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
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("moonshotai", "{\"reasoningEffort\":\"max\"}");

        await Model(Provider(capture), "kimi-k3").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("max", Body(capture, 0)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer explicit reasoningEffort over generic reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_explicit_reasoningEffort_over_generic_reasoning()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "low";
        options.ProviderOptions = UpstreamChat.Bag("moonshotai", "{\"reasoningEffort\":\"high\"}");

        await Model(Provider(capture), "kimi-k3").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", Body(capture, 0)["reasoning_effort"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("kimi-k3")]
    [InlineData("kimi-k3.1-pro")]
    [InlineData("kimi-k3.1-pro-highspeed")]
    [UpstreamTest(
        Prefix + "should omit thinking for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_omit_thinking_for_model(string modelId)
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"thinking\":{\"type\":\"disabled\"},\"reasoningHistory\":\"preserved\"}"
        );

        var result = await Model(Provider(capture), modelId)
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.False(Body(capture, 0).ContainsKey("thinking"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("thinking", warning.Feature);
        Assert.Equal(
            "Kimi K3 always reasons and does not accept the thinking field. The option has been omitted.",
            warning.Message
        );
    }

    [Theory]
    [InlineData("kimi-k2.7-code")]
    [InlineData("kimi-k2.7-code-highspeed")]
    [UpstreamTest(
        Prefix + "should not disable thinking for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_disable_thinking_for_model(string modelId)
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"thinking\":{\"type\":\"disabled\"}}"
        );

        var result = await Model(Provider(capture), modelId)
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.False(Body(capture, 0).ContainsKey("thinking"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("thinking.type \"disabled\"", warning.Feature);
        Assert.Equal("Kimi K2.7 thinking cannot be disabled.", warning.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should rely on K2.7 preserved-thinking defaults",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_rely_on_K2_7_preserved_thinking_defaults()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"reasoningHistory\":\"preserved\"}"
        );

        var result = await Model(Provider(capture), "kimi-k2.7-code")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.False(Body(capture, 0).ContainsKey("thinking"));
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("kimi-k2.5")]
    [InlineData("kimi-k2.6")]
    [UpstreamTest(
        Prefix + "should map generic reasoning to thinking for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_generic_reasoning_to_thinking_for_model(string modelId)
    {
        var capture = Reasoning();
        var provider = Provider(capture);

        var low = UpstreamChat.Prompt();
        low.Reasoning = "low";
        await Model(provider, modelId).DoGenerateAsync(low, CancellationToken.None);
        var none = UpstreamChat.Prompt();
        none.Reasoning = "none";
        await Model(provider, modelId).DoGenerateAsync(none, CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"type\":\"enabled\"}"),
                Body(capture, 0)["thinking"]
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"type\":\"disabled\"}"),
                Body(capture, 1)["thinking"]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit reasoning effort for K2 models and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_reasoning_effort_for_K2_models_and_warn()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("moonshotai", "{\"reasoningEffort\":\"high\"}");

        var result = await Model(Provider(capture), "kimi-k2.6")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.False(Body(capture, 0).ContainsKey("reasoning_effort"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("reasoningEffort", warning.Feature);
        Assert.Equal(
            "reasoningEffort is only supported by Kimi K3 and has been omitted for model \"kimi-k2.6\".",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit thinking and reasoning for Moonshot V1",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_thinking_and_reasoning_for_Moonshot_V1()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"reasoningEffort\":\"high\",\"thinking\":{\"type\":\"enabled\"},\"reasoningHistory\":\"preserved\"}"
        );

        var result = await Model(Provider(capture), "moonshot-v1-8k")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture, 0);
        Assert.False(body.ContainsKey("reasoning_effort"));
        Assert.False(body.ContainsKey("thinking"));
        Assert.Collection(
            result.Warnings,
            w =>
            {
                Assert.Equal("unsupported", w.Type);
                Assert.Equal("reasoningEffort", w.Feature);
                Assert.Equal(
                    "reasoningEffort is only supported by Kimi K3 and has been omitted for model \"moonshot-v1-8k\".",
                    w.Message
                );
            },
            w =>
            {
                Assert.Equal("unsupported", w.Type);
                Assert.Equal("thinking", w.Feature);
                Assert.Equal(
                    "thinking is not supported by model \"moonshot-v1-8k\" and has been omitted.",
                    w.Message
                );
            },
            w =>
            {
                Assert.Equal("unsupported", w.Type);
                Assert.Equal("reasoning", w.Feature);
                Assert.Equal("reasoning is not supported by model \"moonshot-v1-8k\".", w.Message);
            },
            w =>
            {
                Assert.Equal("unsupported", w.Type);
                Assert.Equal(
                    "reasoningHistory 'preserved' is not supported by model \"moonshot-v1-8k\"",
                    w.Feature
                );
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit thinking.keep and warn on models without keep support",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_thinking_keep_and_warn_on_models_without_keep_support()
    {
        var capture = Reasoning();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"reasoningHistory\":\"preserved\"}"
        );

        var result = await Model(Provider(capture), "kimi-k2.5")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture, 0);
        Assert.False(body.ContainsKey("thinking"));
        Assert.False(body.ContainsKey("reasoning_history"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal(
            "reasoningHistory 'preserved' is not supported by model \"kimi-k2.5\"",
            warning.Feature
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract reasoning content and remap usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_reasoning_content_and_remap_usage()
    {
        var capture = Reasoning();

        var result = await Model(Provider(capture), "kimi-k2.6")
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.Collection(
            result.Content,
            c =>
                Assert.Equal(
                    "Let me count the letters carefully.",
                    Assert.IsType<GeneratedReasoning>(c).Text
                ),
            c => Assert.Equal("There are three.", Assert.IsType<GeneratedText>(c).Text)
        );
        Assert.Equal(20, result.Usage.InputTokens);
        Assert.Equal(10, result.Usage.NoCacheInputTokens);
        Assert.Equal(10, result.Usage.CacheReadTokens);
        Assert.Equal(30, result.Usage.OutputTokens);
        Assert.Equal(8, result.Usage.TextTokens);
        Assert.Equal(22, result.Usage.ReasoningTokens);
    }

    /// <summary>Capture that answers with the <c>moonshotai-reasoning</c> fixture.</summary>
    private static UpstreamCapture Reasoning() =>
        new()
        {
            ResponseBody = """
                {"id":"chatcmpl-test-reasoning","object":"chat.completion","created":1785880001,"model":"kimi-k2.6","choices":[{"index":0,"message":{"role":"assistant","content":"There are three.","reasoning_content":"Let me count the letters carefully."},"finish_reason":"stop"}],"usage":{"prompt_tokens":20,"completion_tokens":30,"total_tokens":50,"cached_tokens":10,"completion_tokens_details":{"reasoning_tokens":22}}}
                """,
        };

    private static MoonshotProvider Provider(UpstreamCapture capture) =>
        MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);

    private static OpenAICompatibleLanguageModel Model(MoonshotProvider provider, string modelId) =>
        (OpenAICompatibleLanguageModel)provider.LanguageModel(modelId);

    private static JsonObject Body(UpstreamCapture capture, int index) =>
        (JsonObject)JsonNode.Parse(capture.Requests[index].Body)!;
}
