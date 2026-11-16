// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel.GatewayRealtimeModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

/// <summary>Port of <c>gateway-realtime-model.test.ts</c> &gt; <c>GatewayRealtimeModel</c> &gt; <c>getServerWebSocketConfig</c>.</summary>
public sealed class GetServerWebSocketConfigTests
{
    private const string Prefix = "packages/gateway/src/gateway-realtime-model.test.ts::GatewayRealtimeModel > getServerWebSocketConfig::";

    [Fact]
    [UpstreamTest(Prefix + "returns a header-authenticated Gateway WebSocket connection", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Returns_a_header_authenticated_Gateway_WebSocket_connection()
    {
        var connection = await CreateTestModel().GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal("wss://ai-gateway.vercel.sh/v4/ai/realtime-model?ai-model-id=openai%2Fgpt-realtime", connection.Url);
        Assert.Equal(new Dictionary<string, string> { ["Authorization"] = "Bearer vcst_test-secret" }, connection.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "includes team scoping as a header for server-side connections", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Includes_team_scoping_as_a_header_for_server_side_connections()
    {
        var connection = await CreateTestModel("openai/gpt-realtime", "https://ai-gateway.vercel.sh/v4/ai", "vck_test-token", "my-team")
            .GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer vck_test-token",
                ["x-vercel-ai-gateway-team"] = "my-team",
            },
            connection.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported endpoint intents", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Rejects_unsupported_endpoint_intents()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateTestModel().GetServerWebSocketConfigAsync(RealtimeModelV4SessionIntent.Transcription, CancellationToken.None));
        Assert.Contains("does not support the 'transcription' session intent", error.Message, StringComparison.Ordinal);
    }
}
