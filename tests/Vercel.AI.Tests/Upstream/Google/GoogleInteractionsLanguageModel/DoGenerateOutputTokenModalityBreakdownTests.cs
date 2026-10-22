// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>output token modality breakdown</c>.
/// </summary>
public sealed class DoGenerateOutputTokenModalityBreakdownTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > output token modality breakdown::";

    [Fact]
    [UpstreamTest(Prefix + "surfaces output_tokens_by_modality on providerMetadata.google.outputTokensByModality", Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_output_tokens_by_modality_on_providerMetadata_google_outputTokensByModality()
    {
        var fixture = JsonNode.Parse(GoogleInteractionsStreamSupport.JsonFixture("basic"))!.AsObject();
        fixture["usage"]!["output_tokens_by_modality"] = JsonNode.Parse("[{\"modality\":\"video\",\"tokens\":57920},{\"modality\":\"text\",\"tokens\":19}]");
        var handler = new SequencedHandler().Json(fixture.ToJsonString());

        var result = await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(
            GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?")),
            CancellationToken.None);

        var google = result.ProviderMetadata!.Value.GetProperty("google");
        GoogleUpstream.JsonEqual(google.GetProperty("outputTokensByModality"), "{\"video\":57920,\"text\":19}");
    }

    [Fact]
    [UpstreamTest(Prefix + "omits outputTokensByModality when the response has no breakdown", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_outputTokensByModality_when_the_response_has_no_breakdown()
    {
        var handler = new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));

        var result = await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(
            GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?")),
            CancellationToken.None);

        var google = result.ProviderMetadata!.Value.GetProperty("google");
        Assert.False(google.TryGetProperty("outputTokensByModality", out _));
    }
}
