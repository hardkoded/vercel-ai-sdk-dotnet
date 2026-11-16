// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicServiceTier.AnthropicServiceTierSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicServiceTier;

/// <summary>Port of <c>anthropic-service-tier.test.ts</c> &gt; <c>Anthropic service-tier response metadata</c>.</summary>
public sealed class AnthropicServiceTierResponseMetadataTests
{
    private static string? Tier(JsonElement? metadata)
    {
        return metadata is { } value && value.GetProperty("anthropic").TryGetProperty("serviceTier", out var tier) ? tier.GetString() : null;
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/anthropic/src/anthropic-service-tier.test.ts::Anthropic service-tier response metadata::reads the applied tier from usage in %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_the_applied_tier_from_usage(string mode)
    {
        var response = Message();
        response["service_tier"] = "top-level";
        response["usage"]!["service_tier"] = "priority";

        var (_, metadata) = await Run(mode, Handler(mode, response), Options());

        Assert.Equal("priority", Tier(metadata));
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/anthropic/src/anthropic-service-tier.test.ts::Anthropic service-tier response metadata::retains top-level compatible-provider metadata in %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Retains_top_level_compatible_provider_metadata(string mode)
    {
        var response = Message();
        response["service_tier"] = "standard";

        var (_, metadata) = await Run(mode, Handler(mode, response), Options());

        Assert.Equal("standard", Tier(metadata));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-service-tier.test.ts::Anthropic service-tier response metadata::does not retain a previous message tier when the next message omits it", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_retain_a_previous_message_tier_when_the_next_message_omits_it()
    {
        var first = Message();
        first["usage"]!["service_tier"] = "standard";
        var second = Message();
        second["id"] = "msg_second";
        var handler = AnthropicParity.Ok(Events(first) + Events(second));
        handler.ContentType = "text/event-stream";

        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in await AnthropicParity.Client(handler).LanguageModel("claude-haiku-4-5").DoStreamAsync(Options(), CancellationToken.None))
        {
            parts.Add(part);
        }

        Assert.Equal(new string?[] { "standard", null }, parts.OfType<FinishStreamPart>().Select(part => Tier(part.ProviderMetadata)).ToArray());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-service-tier.test.ts::Anthropic service-tier response metadata::uses a final usage update when the service tier arrives in a delta", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_final_usage_update_when_the_service_tier_arrives_in_a_delta()
    {
        var handler = AnthropicParity.Ok(
            Sse(new JsonObject { ["type"] = "message_start", ["message"] = Message() })
            + Sse(JsonNode.Parse("{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\"},\"usage\":{\"output_tokens\":1,\"service_tier\":\"priority\"}}")!)
            + Sse(JsonNode.Parse("{\"type\":\"message_stop\"}")!));
        handler.ContentType = "text/event-stream";

        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in await AnthropicParity.Client(handler).LanguageModel("claude-haiku-4-5").DoStreamAsync(Options(), CancellationToken.None))
        {
            parts.Add(part);
        }

        Assert.Equal("priority", Tier(parts.OfType<FinishStreamPart>().First().ProviderMetadata));
    }
}
