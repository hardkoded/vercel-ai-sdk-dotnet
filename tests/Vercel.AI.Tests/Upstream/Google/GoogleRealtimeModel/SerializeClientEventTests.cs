// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeModel;

/// <summary>Port of <c>google-realtime-model.test.ts</c> &gt; <c>GoogleRealtimeModel &gt; serializeClientEvent</c>.</summary>
public sealed class SerializeClientEventTests
{
    private const string Id = "packages/google/src/realtime/google-realtime-model.test.ts::GoogleRealtimeModel > serializeClientEvent::";

    [Fact]
    [UpstreamTest(Id + "delegates to mapper", Coverage = UpstreamCoverage.Covered)]
    public void Delegates_to_mapper()
    {
        var model = GoogleRealtimeTestSupport.CreateModel("gemini-2.0-flash-live-001", GoogleRealtimeTestSupport.ApiKeyHeaders());

        var result = model.SerializeClientEvent(new RealtimeModelV4ClientEvent("input-audio-append") { Audio = "base64" });

        GoogleUpstream.JsonEqual(result, """{"realtimeInput":{"audio":{"data":"base64","mimeType":"audio/pcm;rate=16000"}}}""");
    }

    [Fact]
    [UpstreamTest(Id + "labels input audio with the configured capture rate", Coverage = UpstreamCoverage.Covered)]
    public void Labels_input_audio_with_the_configured_capture_rate()
    {
        var model = GoogleRealtimeTestSupport.CreateModel("gemini-2.0-flash-live-001", GoogleRealtimeTestSupport.ApiKeyHeaders());

        // The session-update carries the configured capture rate; later input
        // audio blobs must advertise that real rate, not a hardcoded 16000.
        model.SerializeClientEvent(new RealtimeModelV4ClientEvent("session-update") { Config = new RealtimeModelV4SessionConfig { InputAudioFormatType = "audio/pcm", InputAudioFormatRate = 24000 } });

        var result = model.SerializeClientEvent(new RealtimeModelV4ClientEvent("input-audio-append") { Audio = "base64" });

        GoogleUpstream.JsonEqual(result, """{"realtimeInput":{"audio":{"data":"base64","mimeType":"audio/pcm;rate=24000"}}}""");
    }
}
