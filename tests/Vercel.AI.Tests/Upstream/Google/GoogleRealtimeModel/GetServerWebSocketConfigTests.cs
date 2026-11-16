// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeModel;

/// <summary>Port of <c>google-realtime-model.test.ts</c> &gt; <c>GoogleRealtimeModel &gt; getServerWebSocketConfig</c>.</summary>
public sealed class GetServerWebSocketConfigTests
{
    private const string Id = "packages/google/src/realtime/google-realtime-model.test.ts::GoogleRealtimeModel > getServerWebSocketConfig::";

    [Fact]
    [UpstreamTest(Id + "connects to the unconstrained Bidi endpoint with the API key header", Coverage = UpstreamCoverage.Covered, Commit = GoogleRealtimeTestSupport.Commit)]
    public async Task Connects_to_the_unconstrained_Bidi_endpoint_with_the_API_key_header()
    {
        var model = GoogleRealtimeTestSupport.CreateModel("gemini-2.0-flash-live-001", GoogleRealtimeTestSupport.ApiKeyHeaders(("x-undefined", null)));

        var connection = await model.GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal("wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent", connection.Url);
        var header = Assert.Single(connection.Headers);
        Assert.Equal("x-goog-api-key", header.Key);
        Assert.Equal("test-key", header.Value);
        Assert.Null(connection.Protocols);
    }

    [Fact]
    [UpstreamTest(Id + "rejects unsupported endpoint intents", Coverage = UpstreamCoverage.Covered, Commit = GoogleRealtimeTestSupport.Commit)]
    public void Rejects_unsupported_endpoint_intents()
    {
        var model = GoogleRealtimeTestSupport.CreateModel("gemini-2.0-flash-live-001", GoogleRealtimeTestSupport.ApiKeyHeaders());

        var error = Assert.Throws<AiSdkException>(() => { _ = model.GetServerWebSocketConfigAsync(RealtimeModelV4SessionIntent.Transcription, CancellationToken.None); });
        Assert.Contains("does not support the 'transcription' session intent", error.Message, StringComparison.Ordinal);
    }
}
