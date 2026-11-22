// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAICompatibleChatLanguageModelTests;

/// <summary>Port of <c>openai-compatible-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; response format</c>.</summary>
public sealed class DoGenerateResponseFormatTests
{
    private const string UpstreamCommit = "5006d83c4e72125ac13c54ccb4ac157766fc521d";

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
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
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
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
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
        "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should map top-level reasoning with mapReasoning",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_map_top_level_reasoning_with_mapReasoning()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "xhigh";

        var result = await UpstreamChat
            .Model(capture, "test-provider", "gpt-5", configure: settings => settings.MapReasoning =
                    (_, reasoning, warnings) =>
                    {
                        warnings.Add(new CallWarning("compatibility", "reasoning", null));
                        return "mapped-" + reasoning;
                    })
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            "mapped-xhigh",
            UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>()
        );
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "compatibility" && warning.Feature == "reasoning"
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should not apply mapReasoning to providerOptions reasoningEffort",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_not_apply_mapReasoning_to_providerOptions_reasoningEffort()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "medium";
        options.ProviderOptions = UpstreamChat.Bag(
            "test-provider",
            "{\"reasoningEffort\":\"max\"}"
        );
        var calls = 0;

        await UpstreamChat
            .Model(capture, "test-provider", "gpt-5", configure: settings => settings.MapReasoning =
                    (_, _, _) =>
                    {
                        calls++;
                        return "mapped";
                    })
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("max", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should omit reasoning_effort when no reasoning effort is set",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
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
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
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
