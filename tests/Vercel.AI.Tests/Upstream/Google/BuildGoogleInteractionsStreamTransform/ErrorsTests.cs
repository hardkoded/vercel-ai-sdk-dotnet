// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.BuildGoogleInteractionsStreamTransform;

/// <summary>Port of <c>build-google-interactions-stream-transform.test.ts</c> &gt; <c>buildGoogleInteractionsStreamTransform — errors</c>.</summary>
public sealed class ErrorsTests
{
    [Fact]
    [UpstreamTest("packages/google/src/interactions/build-google-interactions-stream-transform.test.ts::buildGoogleInteractionsStreamTransform — errors::preserves the event type, code, and raw error event", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_event_type_code_and_raw_error_event()
    {
        var eventJson = "{\"event_type\":\"error\",\"event_id\":\"event-error\",\"error\":{\"code\":\"429\",\"message\":\"Rate limit reached\"}}";

        var parts = GoogleInteractionsStreamSupport.Run(eventJson);

        var errorPart = Assert.IsType<ErrorStreamPart>(parts.Single(part => part is ErrorStreamPart));
        var error = Assert.IsType<ProviderStreamError>(errorPart.Error);
        Assert.Equal("Rate limit reached", error.Message);
        Assert.Equal("error", error.Type);
        Assert.Equal("429", error.Code);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(eventJson), Assert.IsAssignableFrom<JsonNode>(error.Data)));
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
    }
}
