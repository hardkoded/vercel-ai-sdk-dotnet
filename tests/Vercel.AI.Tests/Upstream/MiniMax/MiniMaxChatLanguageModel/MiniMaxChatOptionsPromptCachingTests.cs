// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.MiniMaxChatLanguageModel;

/// <summary>Port of <c>minimax-chat-language-model.test.ts</c> &gt; <c>MiniMax chat options (%s)</c> &gt; <c>prompt caching</c>.</summary>
public sealed class MiniMaxChatOptionsPromptCachingTests
{
    private const string CacheOptions = "{\"cacheControl\":{\"type\":\"ephemeral\"}}";

    public static IEnumerable<object?[]> Plain() => MiniMaxChatOptionsCall.Modes();

    public static IEnumerable<object?[]> Keys() => MiniMaxChatOptionsCall.Modes(new object?[] { "cacheControl" }, new object?[] { "cache_control" });

    public static IEnumerable<object?[]> OlderModels() => MiniMaxChatOptionsCall.Modes(new object?[] { "minimax-m3" }, new object?[] { "MiniMax-M2.7" });

    [Theory]
    [MemberData(nameof(Plain))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > prompt caching::forwards cache control on system messages and individual text blocks", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_cache_control_on_system_messages_and_individual_text_blocks(string mode)
    {
        var userContent = new UserContentPart[]
        {
            MiniMaxChatOptionsCall.Text("Stable reference.", "{\"minimax\":" + CacheOptions + "}"),
            MiniMaxChatOptionsCall.Text("Changing question."),
        };
        var prompt = new ModelMessage[]
        {
            new SystemModelMessage("Stable instructions.", MiniMaxParity.Options("minimax", CacheOptions)),
            new UserModelMessage(userContent),
        };
        var originalTexts = userContent.Select(part => ((TextContentPart)part).Text).ToArray();

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, new LanguageModelCallOptions { Prompt = prompt });

        JsonAssert.Equal(body["system"], "[{\"type\":\"text\",\"text\":\"Stable instructions.\",\"cache_control\":{\"type\":\"ephemeral\"}}]");
        JsonAssert.Equal(
            body["messages"]![0]!["content"],
            "[{\"type\":\"text\",\"text\":\"Stable reference.\",\"cache_control\":{\"type\":\"ephemeral\"}},{\"type\":\"text\",\"text\":\"Changing question.\"}]");
        Assert.Empty(warnings);
        Assert.Equal(originalTexts, userContent.Select(part => ((TextContentPart)part).Text));
        Assert.Equal(2, prompt.Length);
    }

    [Theory]
    [MemberData(nameof(Plain))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > prompt caching::applies message cache control to the final block", Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_message_cache_control_to_the_final_block(string mode)
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { new TextContentPart("First."), new TextContentPart("Last.") })
            {
                ProviderOptions = MiniMaxParity.Options("minimax", CacheOptions),
            },
        };

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, new LanguageModelCallOptions { Prompt = prompt });

        JsonAssert.Equal(
            body["messages"]![0]!["content"],
            "[{\"type\":\"text\",\"text\":\"First.\"},{\"type\":\"text\",\"text\":\"Last.\",\"cache_control\":{\"type\":\"ephemeral\"}}]");
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Keys))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > prompt caching::prefers minimax.%s over the anthropic namespace", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_minimax_over_the_anthropic_namespace(string mode, string key)
    {
        var options = MiniMaxChatOptionsCall.WithText(
            "Reference.",
            "{\"minimax\":{\"" + key + "\":{\"type\":\"ephemeral\"}},\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\",\"ttl\":\"1h\"}}}");

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, options);

        JsonAssert.Equal(body["messages"]![0]!["content"]![0]!["cache_control"], "{\"type\":\"ephemeral\"}");
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Keys))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > prompt caching::falls back to anthropic.%s when minimax has no cache setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Falls_back_to_anthropic_when_minimax_has_no_cache_setting(string mode, string key)
    {
        var options = MiniMaxChatOptionsCall.WithText(
            "Reference.",
            "{\"minimax\":{},\"anthropic\":{\"" + key + "\":{\"type\":\"ephemeral\"}}}");

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, options);

        JsonAssert.Equal(body["messages"]![0]!["content"]![0]!["cache_control"], "{\"type\":\"ephemeral\"}");
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Plain))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > prompt caching::counts one breakpoint per block across both namespaces", Coverage = UpstreamCoverage.Covered)]
    public async Task Counts_one_breakpoint_per_block_across_both_namespaces(string mode)
    {
        var content = Enumerable.Range(0, 5)
            .Select(index => (UserContentPart)MiniMaxChatOptionsCall.Text("Reference " + index + ".", "{\"minimax\":" + CacheOptions + ",\"anthropic\":" + CacheOptions + "}"))
            .ToArray();

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage(content) } });

        var caches = body["messages"]![0]!["content"]!.AsArray().Select(part => part!["cache_control"]?.ToJsonString()).ToArray();
        Assert.Equal(new string?[] { "{\"type\":\"ephemeral\"}", "{\"type\":\"ephemeral\"}", "{\"type\":\"ephemeral\"}", "{\"type\":\"ephemeral\"}", null }, caches);
        var warning = Assert.Single(warnings);
        Assert.Equal("cacheControl breakpoint limit", warning.Feature);
    }

    [Theory]
    [MemberData(nameof(Plain))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > prompt caching::does not add cache control when no marker is supplied", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_add_cache_control_when_no_marker_is_supplied(string mode)
    {
        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode);

        Assert.False(body["messages"]![0]!["content"]![0]!.AsObject().ContainsKey("cache_control"));
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(OlderModels))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > prompt caching::keeps the existing cache namespace behavior for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_the_existing_cache_namespace_behavior(string mode, string modelId)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    MiniMaxChatOptionsCall.Text("MiniMax marker.", "{\"minimax\":" + CacheOptions + "}"),
                    MiniMaxChatOptionsCall.Text("Anthropic marker.", "{\"anthropic\":" + CacheOptions + "}"),
                }),
            },
        };

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, options, modelId);

        JsonAssert.Equal(
            body["messages"]![0]!["content"],
            "[{\"type\":\"text\",\"text\":\"MiniMax marker.\"},{\"type\":\"text\",\"text\":\"Anthropic marker.\",\"cache_control\":{\"type\":\"ephemeral\"}}]");
        Assert.Empty(warnings);
    }
}
