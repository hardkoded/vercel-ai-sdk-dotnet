// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using RealtimeModel = Vercel.AI.OpenAI.OpenAIRealtimeModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeModel;

/// <summary>Port of <c>openai-realtime-model.test.ts</c> &gt; <c>OpenAIRealtimeModel</c> &gt; <c>getServerWebSocketConfig</c>.</summary>
public sealed class GetServerWebSocketConfigTests
{
    private const string Prefix =
        "packages/openai/src/realtime/openai-realtime-model.test.ts::OpenAIRealtimeModel > getServerWebSocketConfig::";

    private const string Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782";

    private static RealtimeModel CreateModel(string? baseUrl = null)
    {
        var provider = OpenAIUpstream.Provider(new OpenAICapture(), options =>
        {
            options.Headers["x-custom"] = "custom-header";
            options.Headers["x-undefined"] = null!;
            if (baseUrl != null)
            {
                options.BaseUrl = baseUrl;
            }
        });
        return new RealtimeModel(provider, "gpt-realtime");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "builds the conversation URL and forwards defined headers",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Builds_the_conversation_URL_and_forwards_defined_headers()
    {
        var config = await CreateModel()
            .GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal("wss://api.openai.com/v1/realtime?model=gpt-realtime", config.Url);
        // Undefined header values are dropped. The .NET provider also sends its user-agent.
        Assert.Equal(
            new[] { "authorization", "user-agent", "x-custom" },
            config
                .Headers.Keys.Select(key => key.ToLowerInvariant())
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray()
        );
        Assert.Equal("Bearer test-api-key", config.Headers["authorization"]);
        Assert.Equal("custom-header", config.Headers["x-custom"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "builds the transcription URL (model goes in session.update)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Builds_the_transcription_URL_model_goes_in_session_update()
    {
        var config = await CreateModel()
            .GetServerWebSocketConfigAsync(
                RealtimeModelV4SessionIntent.Transcription,
                CancellationToken.None
            );

        Assert.Equal("wss://api.openai.com/v1/realtime?intent=transcription", config.Url);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects translation until the normalized codec supports it",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_translation_until_the_normalized_codec_supports_it()
    {
        // Upstream passes the unsupported intent 'translation' (`as never`).
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateModel()
                .GetServerWebSocketConfigAsync(
                    (RealtimeModelV4SessionIntent)2,
                    CancellationToken.None
                )
        );

        Assert.Matches("is not supported", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses ws:// for an http base URL (local/proxy)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Uses_ws_for_an_http_base_URL_local_proxy()
    {
        var config = await CreateModel("http://localhost:8787/v1")
            .GetServerWebSocketConfigAsync(null, CancellationToken.None);

        Assert.Equal("ws://localhost:8787/v1/realtime?model=gpt-realtime", config.Url);
    }
}
