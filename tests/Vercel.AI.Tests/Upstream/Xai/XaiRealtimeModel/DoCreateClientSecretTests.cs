// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Xai.XaiRealtimeModel;

/// <summary>Port of <c>xai-realtime-model.test.ts</c> &gt; <c>XaiRealtimeModel &gt; doCreateClientSecret</c>.</summary>
public sealed class DoCreateClientSecretTests
{
    private const string Id =
        "packages/xai/src/realtime/xai-realtime-model.test.ts::XaiRealtimeModel > doCreateClientSecret::";

    [Fact]
    [UpstreamTest(
        Id + "includes the model as a query param on the WebSocket URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Includes_the_model_as_a_query_param_on_the_WebSocket_URL()
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Json("{\"value\":\"secret\",\"expires_at\":123}")
        );
        using var client = new HttpClient(handler, disposeHandler: false);

        var result = await XaiRealtimeTestSupport
            .CreateModel(client)
            .CreateClientSecretAsync(null, CancellationToken.None);

        // xAI selects the voice model from the `model` query param; without it
        // the model choice is silently ignored.
        Assert.Equal("wss://api.x.ai/v1/realtime?model=grok-voice-latest", result.Url);
        Assert.Equal(
            "https://api.x.ai/v1/realtime/client_secrets",
            handler.Calls[0].Uri.AbsoluteUri
        );
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
    }
}
