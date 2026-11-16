// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicServiceTier.AnthropicServiceTierSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicServiceTier;

/// <summary>Port of <c>anthropic-service-tier.test.ts</c> &gt; <c>Anthropic service tiers</c>.</summary>
public sealed class AnthropicServiceTiersTests
{
    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/anthropic/src/anthropic-service-tier.test.ts::Anthropic service tiers::sends the tier when using %s", Coverage = UpstreamCoverage.Covered, Commit = "8291ccf2370b04e99a1d74ea27996a1c78a21492")]
    public async Task Sends_the_tier(string mode)
    {
        var handler = Handler(mode, Message());

        await Run(mode, handler, Options("{\"anthropic\":{\"serviceTier\":\"standard_only\"}}"));

        Assert.Equal("standard_only", (string?)JsonNode.Parse(handler.Body)!["service_tier"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-service-tier.test.ts::Anthropic service tiers::warns and omits a MiniMax-only tier", Coverage = UpstreamCoverage.Covered, Commit = "8291ccf2370b04e99a1d74ea27996a1c78a21492")]
    public async Task Warns_and_omits_a_MiniMax_only_tier()
    {
        var handler = Handler("generate", Message());

        var (warnings, _) = await Run("generate", handler, Options("{\"anthropic\":{\"serviceTier\":\"priority\"}}"));

        Assert.Null(JsonNode.Parse(handler.Body)!["service_tier"]);
        Assert.Contains(warnings, warning => warning is { Type: "unsupported", Feature: "serviceTier" });
    }
}
