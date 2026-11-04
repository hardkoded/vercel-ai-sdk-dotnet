// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;

namespace Vercel.AI.Tests.MiniMaxProvider;

/// <summary>Port of <c>minimax-provider.test.ts</c> &gt; <c>MiniMaxProvider &gt; chat</c>.</summary>
public sealed class ChatTests
{
    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > chat::should construct a chat model", Coverage = UpstreamCoverage.Covered)]
    public void Should_construct_a_chat_model()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create();
        var model = provider.Chat("minimax-m2.1");

        // Upstream asserts an AnthropicLanguageModel; the MiniMax chat model is the Anthropic-backed model in this port.
        Assert.IsType<Vercel.AI.MiniMax.MiniMaxChatLanguageModel>(model);
        Assert.Equal("minimax-m2.1", model.ModelId);
    }
}
