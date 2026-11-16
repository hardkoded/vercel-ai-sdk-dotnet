// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.MiniMaxChatLanguageModel;

/// <summary>Port of <c>minimax-chat-language-model.test.ts</c> &gt; <c>MiniMax service tiers</c>.</summary>
public sealed class MiniMaxServiceTiersTests
{
    private const string Message =
        "{\"id\":\"msg_tier\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"minimax-m3.1-flash\",\"content\":[],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";

    private static LanguageModelCallOptions Options(string providerOptions)
    {
        var options = MiniMaxParity.Hello();
        options.MaxOutputTokens = 16;
        options.ProviderOptions = MiniMaxParity.Options(providerOptions);
        return options;
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest(
        "packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax service tiers::sends priority admission when using %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8291ccf2370b04e99a1d74ea27996a1c78a21492"
    )]
    public async Task Sends_priority_admission(string mode)
    {
        var server = new MiniMaxServer();
        var model = MiniMaxParity.Provider(server).Chat("minimax-m3.1-flash");
        var options = Options("{\"minimax\":{\"serviceTier\":\"priority\"}}");
        if (mode == "generate")
        {
            server.SetJson(Message);
            await model.DoGenerateAsync(options, CancellationToken.None);
        }
        else
        {
            server.SetStream(
                "data: {\"type\":\"message_start\",\"message\":" + Message + "}\n\n",
                "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\"},\"usage\":{\"output_tokens\":1}}\n\n",
                "data: {\"type\":\"message_stop\"}\n\n"
            );
            await MiniMaxParity.Collect(await model.DoStreamAsync(options, CancellationToken.None));
        }

        Assert.Equal("priority", (string?)server.Calls[0].Body["service_tier"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax service tiers::uses the MiniMax tier over a canonical namespace value",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8291ccf2370b04e99a1d74ea27996a1c78a21492"
    )]
    public async Task Uses_the_MiniMax_tier_over_a_canonical_namespace_value()
    {
        var server = new MiniMaxServer();
        server.SetJson(Message);

        await MiniMaxParity
            .Provider(server)
            .Chat("minimax-m3.1-flash")
            .DoGenerateAsync(
                Options(
                    "{\"anthropic\":{\"serviceTier\":\"priority\"},\"minimax\":{\"serviceTier\":\"standard\"}}"
                ),
                CancellationToken.None
            );

        Assert.Equal("standard", (string?)server.Calls[0].Body["service_tier"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax service tiers::warns and omits an Anthropic-only tier",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8291ccf2370b04e99a1d74ea27996a1c78a21492"
    )]
    public async Task Warns_and_omits_an_Anthropic_only_tier()
    {
        var server = new MiniMaxServer();
        server.SetJson(Message);

        var result = await MiniMaxParity
            .Provider(server)
            .Chat("minimax-m3.1-flash")
            .DoGenerateAsync(
                Options("{\"anthropic\":{\"serviceTier\":\"auto\"}}"),
                CancellationToken.None
            );

        Assert.Null(server.Calls[0].Body["service_tier"]);
        Assert.Contains(result.Warnings, warning =>
            warning is { Type: "unsupported", Feature: "serviceTier" }
        );
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"invalid\"")]
    [UpstreamTest(
        "packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax service tiers::rejects an invalid tier %s before sending a request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8291ccf2370b04e99a1d74ea27996a1c78a21492"
    )]
    public async Task Rejects_an_invalid_tier_before_sending_a_request(string serviceTier)
    {
        var server = new MiniMaxServer();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            MiniMaxParity
                .Provider(server)
                .Chat("minimax-m3.1-flash")
                .DoGenerateAsync(
                    Options("{\"minimax\":{\"serviceTier\":" + serviceTier + "}}"),
                    CancellationToken.None
                )
        );

        Assert.Empty(server.Calls);
    }
}
