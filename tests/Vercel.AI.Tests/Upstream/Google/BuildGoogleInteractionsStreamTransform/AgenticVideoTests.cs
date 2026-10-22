// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.BuildGoogleInteractionsStreamTransform;

/// <summary>Port of <c>build-google-interactions-stream-transform.test.ts</c> &gt; <c>buildGoogleInteractionsStreamTransform — agentic video</c>.</summary>
public sealed class AgenticVideoTests
{
    [Fact]
    [UpstreamTest("packages/google/src/interactions/build-google-interactions-stream-transform.test.ts::buildGoogleInteractionsStreamTransform — agentic video::emits processing steps as custom parts", Coverage = UpstreamCoverage.Covered)]
    public void Emits_processing_steps_as_custom_parts()
    {
        var parts = GoogleInteractionsStreamSupport.Run(
            "{\"event_type\":\"interaction.created\",\"interaction\":{\"id\":\"interaction-1\",\"status\":\"in_progress\"}}",
            "{\"event_type\":\"step.start\",\"index\":0,\"step\":{\"type\":\"processing_call\",\"id\":\"processing-1\",\"signature\":\"call-signature\"}}",
            "{\"event_type\":\"step.stop\",\"index\":0}",
            "{\"event_type\":\"step.start\",\"index\":1,\"step\":{\"type\":\"processing_result\",\"call_id\":\"processing-1\",\"signature\":\"result-signature\"}}",
            "{\"event_type\":\"step.stop\",\"index\":1}",
            "{\"event_type\":\"interaction.completed\",\"interaction\":{\"id\":\"interaction-1\",\"status\":\"completed\"}}");

        var custom = GoogleInteractionsStreamSupport.Snapshot(parts.Where(part => part is CustomStreamPart));

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("[{\"type\":\"custom\",\"kind\":\"google.processing_call\",\"providerMetadata\":{\"google\":{\"signature\":\"call-signature\",\"interactionId\":\"interaction-1\",\"processingId\":\"processing-1\"}}},{\"type\":\"custom\",\"kind\":\"google.processing_result\",\"providerMetadata\":{\"google\":{\"signature\":\"result-signature\",\"interactionId\":\"interaction-1\",\"processingCallId\":\"processing-1\"}}}]"),
                custom),
            custom.ToJsonString());
    }
}
