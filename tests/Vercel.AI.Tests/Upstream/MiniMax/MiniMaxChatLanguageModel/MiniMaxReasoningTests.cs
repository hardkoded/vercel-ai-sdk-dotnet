// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MiniMaxChatLanguageModel;

/// <summary>Port of <c>minimax-chat-language-model.test.ts</c> &gt; <c>MiniMax reasoning</c>.</summary>
public sealed class MiniMaxReasoningTests
{
    private readonly MiniMaxServer _server = new MiniMaxServer();

    [Theory]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [InlineData("xhigh")]
    [InlineData("max")]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning::sends M3.1 Flash effort %s through the public MiniMax options", Coverage = UpstreamCoverage.Partial, Note = "The port has no typed MiniMax options schema, so the effort is passed as raw provider options and is not parsed by a schema first.")]
    public async Task Sends_M3_1_Flash_effort_through_the_public_MiniMax_options(string effort)
    {
        _server.SetJson("{\"id\":\"msg_minimax_effort\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello!\"}],\"model\":\"minimax-m3.1-flash\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":10}}");
        var options = MiniMaxParity.Hello();
        options.ProviderOptions = MiniMaxParity.Options("minimax", "{\"effort\":\"" + effort + "\"}");

        await MiniMaxParity.Provider(_server).Chat("minimax-m3.1-flash").DoGenerateAsync(options, CancellationToken.None);

        MiniMaxParity.AssertMatch(_server.Calls[0].Body, "{\"model\":\"minimax-m3.1-flash\",\"output_config\":{\"effort\":\"" + effort + "\"}}");
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning::sends thinking via the minimax provider option and returns a reasoning part", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_thinking_via_the_minimax_provider_option_and_returns_a_reasoning_part()
    {
        _server.SetJson("{\"id\":\"msg_minimax_reasoning\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"thinking\",\"thinking\":\"Counting the letters...\",\"signature\":\"sig_123\"},{\"type\":\"text\",\"text\":\"There are 3 \\\"r\\\"s.\"}],\"model\":\"minimax-m3\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}");
        var options = MiniMaxParity.Hello();
        options.ProviderOptions = MiniMaxParity.Options("minimax", "{\"thinking\":{\"type\":\"adaptive\"}}");

        var result = await MiniMaxParity.Provider(_server).Chat("minimax-m3").DoGenerateAsync(options, CancellationToken.None);

        // The thinking option flows to the request under the Anthropic contract.
        MiniMaxParity.AssertMatch(_server.Calls[0].Body, "{\"model\":\"minimax-m3\",\"thinking\":{\"type\":\"adaptive\"}}");

        // The thinking block surfaces as a reasoning part, ahead of the text part.
        Assert.Equal(2, result.Content.Count);
        Assert.Equal("reasoning", result.Content[0].Type);
        Assert.Equal("Counting the letters...", Assert.IsType<AnthropicReasoningContent>(result.Content[0]).Text);
        Assert.Equal("There are 3 \"r\"s.", Assert.IsType<GeneratedText>(result.Content[1]).Text);
    }
}
