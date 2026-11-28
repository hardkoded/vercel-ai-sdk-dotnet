// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GroqChatLanguageModelTests.GroqChatTestSupport;

namespace Vercel.AI.Tests.GroqChatLanguageModelTests;

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; top-level reasoning</c>.</summary>
public sealed class DoGenerateTopLevelReasoningTests
{
    private const string Prefix = TestFile + "::doGenerate > top-level reasoning::";

    [Fact]
    [UpstreamTest(
        Prefix + "should map top-level reasoning to reasoning_effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_top_level_reasoning_to_reasoning_effort()
    {
        var capture = JsonFixture("groq-text");
        var options = TestPrompt();
        options.Reasoning = "high";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit unsupported top-level reasoning none and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_unsupported_top_level_reasoning_none_and_warn()
    {
        var capture = JsonFixture("groq-text");
        var options = TestPrompt();
        options.Reasoning = "none";

        var result = await Model(capture, "llama-3.3-70b-versatile")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.False(UpstreamChat.Body(capture).ContainsKey("reasoning_effort"));
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported"
            && warning.Feature == "reasoning"
            && warning.Message == "reasoning \"none\" is not supported by this model."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer providerOptions reasoningEffort over top-level reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_providerOptions_reasoningEffort_over_top_level_reasoning()
    {
        var capture = JsonFixture("groq-text");
        var options = TestPrompt();
        options.Reasoning = "medium";
        options.ProviderOptions = UpstreamChat.Bag("groq", """{"reasoningEffort":"high"}""");

        var result = await Model(capture, "openai/gpt-oss-120b")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should coerce top-level reasoning max to high with a warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_coerce_top_level_reasoning_max_to_high_with_a_warning()
    {
        var capture = new UpstreamCapture();
        var model = (OpenAICompatibleLanguageModel)
            GroqProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture)
                .LanguageModel("llama-3.3-70b-versatile");
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "compatibility"
            && warning.Feature == "reasoning"
            && warning.Message
                == "reasoning \"max\" is not directly supported by this model. mapped to effort \"high\"."
        );
    }
}
