// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c> &gt; <c>service tier</c>.
/// </summary>
public sealed class DoStreamServiceTierTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream > service tier::";

    private static LanguageModelCallOptions Options()
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.IncludeRawChunks = false;
        return options;
    }

    // The basic fixture with its interaction.completed event changed by the caller.
    private static List<JsonObject> BasicEvents(Action<JsonObject> changeInteraction)
    {
        var events = GoogleInteractionsStreamSupport.FixtureEvents("basic");
        var interaction = events.Single(e => (string?)e["event_type"] == "interaction.completed")["interaction"]!.AsObject();
        changeInteraction(interaction);
        return events;
    }

    private static async Task<string?> ServiceTier(List<JsonObject> events, string? header)
    {
        var handler = new SequencedHandler().Sse(GoogleInteractionsStreamSupport.Sse(events));
        if (header != null)
        {
            handler.ResponseHeaders["x-gemini-service-tier"] = header;
        }

        var parts = await GoogleInteractionsStreamSupport.Stream(GoogleInteractionsStreamSupport.Model(handler), Options());
        var finish = Assert.IsType<FinishStreamPart>(parts.Single(part => part is FinishStreamPart));
        var google = finish.ProviderMetadata!.Value.GetProperty("google");
        return google.TryGetProperty("serviceTier", out var tier) && tier.ValueKind == JsonValueKind.String ? tier.GetString() : null;
    }

    [Fact]
    [UpstreamTest(Prefix + "sends service_tier in the request body when providerOptions.google.serviceTier is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_service_tier_in_the_request_body_when_providerOptions_google_serviceTier_is_set()
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("basic");
        var options = Options();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"serviceTier\":\"priority\"}}");

        await GoogleInteractionsStreamSupport.Stream(GoogleInteractionsStreamSupport.Model(handler), options);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal("priority", (string?)body["service_tier"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "reads providerMetadata.google.serviceTier from the interaction.complete event body service_tier field", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_providerMetadata_google_serviceTier_from_the_interaction_complete_event_body_service_tier_field()
    {
        var events = BasicEvents(interaction => interaction["service_tier"] = "priority");

        Assert.Equal("priority", await ServiceTier(events, null));
    }

    [Fact]
    [UpstreamTest(Prefix + "falls back to the x-gemini-service-tier response header when service_tier is absent from the body", Coverage = UpstreamCoverage.Covered)]
    public async Task Falls_back_to_the_x_gemini_service_tier_response_header_when_service_tier_is_absent_from_the_body()
    {
        var events = BasicEvents(interaction => interaction.Remove("service_tier"));

        Assert.Equal("priority", await ServiceTier(events, "priority"));
    }

    [Fact]
    [UpstreamTest(Prefix + "omits serviceTier from finish providerMetadata when both response body and header are absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_serviceTier_from_finish_providerMetadata_when_both_response_body_and_header_are_absent()
    {
        var events = BasicEvents(interaction => interaction.Remove("service_tier"));

        Assert.Null(await ServiceTier(events, null));
    }
}
