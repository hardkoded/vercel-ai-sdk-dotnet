// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>service tier</c>.
/// </summary>
public sealed class DoGenerateServiceTierTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > service tier::";

    private static LanguageModelCallOptions Options(string? providerOptions = null)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        if (providerOptions != null)
        {
            options.ProviderOptions = GoogleUpstream.ProviderOptions(providerOptions);
        }

        return options;
    }

    // The basic fixture with its body changed by the caller.
    private static string Basic(Action<JsonObject> change)
    {
        var fixture = JsonNode.Parse(GoogleInteractionsStreamSupport.JsonFixture("basic"))!.AsObject();
        change(fixture);
        return fixture.ToJsonString();
    }

    private static string? ServiceTier(LanguageModelGenerateResult result)
    {
        var google = result.ProviderMetadata!.Value.GetProperty("google");
        return google.TryGetProperty("serviceTier", out var tier) && tier.ValueKind == JsonValueKind.String ? tier.GetString() : null;
    }

    [Fact]
    [UpstreamTest(Prefix + "sends service_tier in the request body when providerOptions.google.serviceTier is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_service_tier_in_the_request_body_when_providerOptions_google_serviceTier_is_set()
    {
        var handler = new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options("{\"google\":{\"serviceTier\":\"priority\"}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal("priority", (string?)body["service_tier"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits service_tier from the request body when providerOptions.google.serviceTier is not set", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_service_tier_from_the_request_body_when_providerOptions_google_serviceTier_is_not_set()
    {
        var handler = new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False(body.ContainsKey("service_tier"));
    }

    [Fact]
    [UpstreamTest(Prefix + "reads providerMetadata.google.serviceTier from the response body service_tier field", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_providerMetadata_google_serviceTier_from_the_response_body_service_tier_field()
    {
        var handler = new SequencedHandler().Json(Basic(fixture => fixture["service_tier"] = "priority"));

        var result = await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Equal("priority", ServiceTier(result));
    }

    [Fact]
    [UpstreamTest(Prefix + "falls back to the x-gemini-service-tier response header when service_tier is absent from the body", Coverage = UpstreamCoverage.Covered)]
    public async Task Falls_back_to_the_x_gemini_service_tier_response_header_when_service_tier_is_absent_from_the_body()
    {
        var handler = new SequencedHandler().Json(Basic(fixture => fixture.Remove("service_tier")));
        handler.ResponseHeaders["x-gemini-service-tier"] = "priority";

        var result = await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Equal("priority", ServiceTier(result));
    }

    [Fact]
    [UpstreamTest(Prefix + "omits serviceTier from providerMetadata when both response body and header are absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_serviceTier_from_providerMetadata_when_both_response_body_and_header_are_absent()
    {
        var handler = new SequencedHandler().Json(Basic(fixture => fixture.Remove("service_tier")));

        var result = await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Null(ServiceTier(result));
    }
}
