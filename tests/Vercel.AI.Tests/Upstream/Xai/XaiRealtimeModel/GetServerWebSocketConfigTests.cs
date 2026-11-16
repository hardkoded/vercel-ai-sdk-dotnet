// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.XaiRealtimeModel;

/// <summary>Port of <c>xai-realtime-model.test.ts</c> &gt; <c>XaiRealtimeModel &gt; getServerWebSocketConfig</c>.</summary>
public sealed class GetServerWebSocketConfigTests
{
    private const string Id =
        "packages/xai/src/realtime/xai-realtime-model.test.ts::XaiRealtimeModel > getServerWebSocketConfig::";
    private const string Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782";

    [Fact]
    [UpstreamTest(
        Id + "builds the voice URL with the model query param and forwards headers",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Builds_the_voice_URL_with_the_model_query_param_and_forwards_headers()
    {
        var connection = await XaiRealtimeTestSupport
            .CreateModel()
            .GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal("wss://api.x.ai/v1/realtime?model=grok-voice-latest", connection.Url);
        var header = Assert.Single(connection.Headers);
        Assert.Equal("authorization", header.Key);
        Assert.Equal("Bearer test-key", header.Value);
        Assert.Null(connection.Protocols);
    }

    [Fact]
    [UpstreamTest(
        Id + "uses ws:// for an http base URL (local/proxy)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Uses_ws_for_an_http_base_URL()
    {
        IReadOnlyDictionary<string, string?> headers = new Dictionary<string, string?>
        {
            ["authorization"] = "Bearer test-key",
        };
        var localModel = new global::Vercel.AI.Xai.XaiRealtimeModel(
            "grok-voice-latest",
            new XaiRealtimeModelConfig("xai.realtime", "http://localhost:8787/v1", _ =>
                Task.FromResult(headers)
            )
        );

        var connection = await localModel.GetServerWebSocketConfigAsync(
            null,
            CancellationToken.None
        );

        Assert.Equal("ws://localhost:8787/v1/realtime?model=grok-voice-latest", connection.Url);
    }

    [Fact]
    [UpstreamTest(
        Id + "rejects unsupported endpoint intents",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Rejects_unsupported_endpoint_intents()
    {
        var error = Assert.Throws<AiSdkException>(() =>
        {
            _ = XaiRealtimeTestSupport
                .CreateModel()
                .GetServerWebSocketConfigAsync(
                    RealtimeModelV4SessionIntent.Transcription,
                    CancellationToken.None
                );
        });

        Assert.Contains(
            "does not support the 'transcription' session intent",
            error.Message,
            StringComparison.Ordinal
        );
    }
}
