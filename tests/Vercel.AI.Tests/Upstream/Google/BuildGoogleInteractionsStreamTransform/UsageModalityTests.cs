// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.BuildGoogleInteractionsStreamTransform;

/// <summary>Port of <c>build-google-interactions-stream-transform.test.ts</c> &gt; <c>buildGoogleInteractionsStreamTransform — usage modality</c>.</summary>
public sealed class UsageModalityTests
{
    [Fact]
    [UpstreamTest("packages/google/src/interactions/build-google-interactions-stream-transform.test.ts::buildGoogleInteractionsStreamTransform — usage modality::surfaces output_tokens_by_modality on the finish part providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public void Surfaces_output_tokens_by_modality_on_the_finish_part_providerMetadata()
    {
        var parts = GoogleInteractionsStreamSupport.Run(
            "{\"event_type\":\"interaction.created\",\"interaction\":{\"id\":\"v1_usage\",\"status\":\"in_progress\"}}",
            "{\"event_type\":\"interaction.completed\",\"interaction\":{\"id\":\"v1_usage\",\"status\":\"completed\",\"usage\":{\"total_output_tokens\":57939,\"output_tokens_by_modality\":[{\"modality\":\"video\",\"tokens\":57920},{\"modality\":\"text\",\"tokens\":19}]}}}");

        var finish = Assert.IsType<FinishStreamPart>(parts.Single(part => part is FinishStreamPart));

        GoogleUpstream.JsonEqual(finish.ProviderMetadata!.Value.GetProperty("google").GetProperty("outputTokensByModality"), "{\"video\":57920,\"text\":19}");
    }
}
