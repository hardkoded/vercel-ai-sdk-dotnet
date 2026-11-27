// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel.CartesiaRealtimeModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel;

/// <summary>Port of <c>cartesia-realtime-model.test.ts</c> &gt; <c>CartesiaRealtimeModel &gt; doCreateClientSecret</c>.</summary>
public sealed class DoCreateClientSecretTests
{
    private const string Id =
        "packages/cartesia/src/cartesia-realtime-model.test.ts::CartesiaRealtimeModel > doCreateClientSecret::";

    [Fact]
    [UpstreamTest(
        Id + "creates an Ink 2 auto-finalize client secret",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_an_Ink_2_auto_finalize_client_secret()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"token\":\"access-token\"}"));
        using var client = new HttpClient(handler, disposeHandler: false);

        var result = await CreateModel(client)
            .CreateClientSecretAsync(
                new RealtimeModelV4SessionConfig
                {
                    InputAudioFormatType = "audio/pcm",
                    InputAudioFormatRate = 16000,
                    InputAudioTranscription = new RealtimeModelV4AudioTranscription
                    {
                        Language = "en",
                    },
                    TurnDetection = new RealtimeModelV4TurnDetection { Type = "server-vad" },
                },
                60,
                CancellationToken.None
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal("https://api.cartesia.ai/access-token", call.Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal("Bearer test-key", call.Header("Authorization"));
        Assert.Equal("2026-03-01", call.Header("Cartesia-Version"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("{\"grants\":{\"stt\":true},\"expires_in\":60}", call.Text);
        Assert.Equal("access-token", result.Token);
        Assert.Equal(
            "wss://api.cartesia.ai/stt/turns/websocket?model=ink-2&encoding=pcm_s16le&sample_rate=16000&cartesia_version=2026-03-01",
            result.Url
        );
        Assert.Equal(1783512060L, result.ExpiresAt);
    }

    [Fact]
    [UpstreamTest(
        Id + "uses the manual-finalize endpoint when turn detection is disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_the_manual_finalize_endpoint_when_turn_detection_is_disabled()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"token\":\"access-token\"}"));
        using var client = new HttpClient(handler, disposeHandler: false);

        var result = await CreateModel(client)
            .CreateClientSecretAsync(
                new RealtimeModelV4SessionConfig
                {
                    InputAudioFormatType = "audio/pcmu",
                    InputAudioFormatRate = 8000,
                    InputAudioTranscription = new RealtimeModelV4AudioTranscription
                    {
                        Language = "en",
                    },
                    TurnDetectionDisabled = true,
                },
                null,
                CancellationToken.None
            );

        Assert.Equal(
            "wss://api.cartesia.ai/stt/websocket?model=ink-2&encoding=pcm_mulaw&sample_rate=8000&cartesia_version=2026-03-01&language=en",
            result.Url
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "rejects unsupported languages and token lifetimes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_unsupported_languages_and_token_lifetimes()
    {
        var model = CreateModel();

        var language = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.CreateClientSecretAsync(
                new RealtimeModelV4SessionConfig
                {
                    InputAudioTranscription = new RealtimeModelV4AudioTranscription
                    {
                        Language = "es",
                    },
                },
                null,
                CancellationToken.None
            )
        );
        Assert.Contains("currently supports English only", language.Message);

        var lifetime = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.CreateClientSecretAsync(null, 3601, CancellationToken.None)
        );
        Assert.Contains("between 1 and 3600 seconds", lifetime.Message);
    }
}
