// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeModel;

/// <summary>Port of <c>google-realtime-model.test.ts</c> &gt; <c>GoogleRealtimeModel &gt; getWebSocketConfig</c>.</summary>
public sealed class GetWebSocketConfigTests
{
    private const string Id = "packages/google/src/realtime/google-realtime-model.test.ts::GoogleRealtimeModel > getWebSocketConfig::";

    [Fact]
    [UpstreamTest(Id + "returns URL with access_token query param", Coverage = UpstreamCoverage.Covered)]
    public void Returns_URL_with_access_token_query_param()
    {
        var model = GoogleRealtimeTestSupport.CreateModel("gemini-2.0-flash-live-001", GoogleRealtimeTestSupport.ApiKeyHeaders());

        var url = model.GetWebSocketUrl("my-token", "wss://example.com/ws");

        Assert.Equal("wss://example.com/ws?access_token=my-token", url);
    }
}
