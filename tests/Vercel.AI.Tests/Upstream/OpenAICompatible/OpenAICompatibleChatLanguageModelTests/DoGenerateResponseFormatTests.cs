// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.OpenAICompatibleChatLanguageModelTests;

/// <summary>Port of <c>openai-compatible-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; response format</c>.</summary>
public sealed class DoGenerateResponseFormatTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should pass top-level max reasoning as reasoning_effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_top_level_max_reasoning_as_reasoning_effort()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";

        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("max", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should send reasoningEffort null as reasoning_effort null",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_reasoningEffort_null_as_reasoning_effort_null()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("test-provider", "{\"reasoningEffort\":null}");

        await UpstreamChat
            .Model(capture, "test-provider", "gpt-5")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.True(body.ContainsKey("reasoning_effort"));
        Assert.Null(body["reasoning_effort"]);
        Assert.False(body.ContainsKey("reasoningEffort"));
    }

    [Fact]
    [UpstreamTest(
        "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should prefer providerOptions reasoningEffort null over top-level reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_providerOptions_reasoningEffort_null_over_top_level_reasoning()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "medium";
        options.ProviderOptions = UpstreamChat.Bag("test-provider", "{\"reasoningEffort\":null}");

        await UpstreamChat
            .Model(capture, "test-provider", "gpt-5")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.True(body.ContainsKey("reasoning_effort"));
        Assert.Null(body["reasoning_effort"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should omit reasoning_effort when no reasoning effort is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_reasoning_effort_when_no_reasoning_effort_is_set()
    {
        var capture = new UpstreamCapture();

        await UpstreamChat
            .Model(capture)
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.False(UpstreamChat.Body(capture).ContainsKey("reasoning_effort"));
    }

    [Fact]
    [UpstreamTest(
        "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should not overwrite a raw reasoning_effort provider option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_overwrite_a_raw_reasoning_effort_provider_option()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("test-provider", "{\"reasoning_effort\":null}");

        await UpstreamChat
            .Model(capture, "test-provider", "gpt-5")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.True(body.ContainsKey("reasoning_effort"));
        Assert.Null(body["reasoning_effort"]);
    }
}
