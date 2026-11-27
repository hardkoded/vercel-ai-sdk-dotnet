// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using static Vercel.AI.Tests.ElevenLabsTranscriptionModel.ElevenLabsTranscriptionTestSupport;

namespace Vercel.AI.Tests.ElevenLabsTranscriptionModel;

/// <summary>Port of <c>elevenlabs-transcription-model.test.ts</c> &gt; <c>doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class DoStreamTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-transcription-model.test.ts::doStream::";

    private const string TimestampsOptions =
        """{"elevenlabs":{"streaming":{"includeTimestamps":true}}}""";

    private static void AssertParts(string expected, IEnumerable<ModelStreamPart> parts) =>
        Assert.Equal(JsonNode.Parse(expected)!.ToJsonString(), Describe(parts).ToJsonString());

    [Fact]
    [UpstreamTest(
        Prefix + "streams Scribe v2 Realtime and maps partial, committed, and timestamped output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_Scribe_v2_Realtime_and_maps_partial_committed_and_timestamped_output()
    {
        MockWebSocket.Reset();
        var testDate = new DateTimeOffset(DateTime.UnixEpoch, TimeSpan.Zero);
        var model = Transcription(configure: options =>
        {
            options.Clock = () => testDate;
            options.Headers["Custom-Provider-Header"] = "provider-value";
        });

        var result = await DoStream(
            model,
            Audio(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }),
            new AudioFormat("audio/pcm", 16000),
            """
            {"elevenlabs":{"languageCode":"en","streaming":{"commitStrategy":"manual","enableLogging":false,"includeLanguageDetection":true,"includeTimestamps":true,"keyterms":["Vercel","AI SDK"],"minSilenceDurationMs":200,"minSpeechDurationMs":150,"noVerbatim":true,"previousText":"Earlier context","secondaryLanguages":["es","fr"],"vadSilenceThresholdSecs":1.2,"vadThreshold":0.5}}}
            """,
            new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-value" }
        );

        var parts = Collect(result!);
        var ws = Socket();
        Assert.Equal(
            "wss://api.elevenlabs.io/v1/speech-to-text/realtime",
            ws.Url.GetLeftPart(UriPartial.Path)
        );
        var query = Query(ws.Url);
        Assert.Equal("pcm_16000", query["audio_format"]);
        Assert.Equal("manual", query["commit_strategy"]);
        Assert.Equal("false", query["enable_logging"]);
        Assert.Equal("true", query["include_language_detection"]);
        Assert.Equal("true", query["include_timestamps"]);
        Assert.Equal("en", query["language_code"]);
        Assert.Equal("200", query["min_silence_duration_ms"]);
        Assert.Equal("150", query["min_speech_duration_ms"]);
        Assert.Equal("scribe_v2_realtime", query["model_id"]);
        Assert.Equal("true", query["no_verbatim"]);
        Assert.Equal("1.2", query["vad_silence_threshold_secs"]);
        Assert.Equal("0.5", query["vad_threshold"]);
        Assert.Equal(new[] { "Vercel", "AI SDK" }, QueryAll(ws.Url, "keyterms"));
        Assert.Equal(new[] { "es", "fr" }, QueryAll(ws.Url, "secondary_languages"));
        Assert.Equal("test-api-key", ws.Headers!["xi-api-key"]);
        Assert.Equal("provider-value", ws.Headers["Custom-Provider-Header"]);
        Assert.Equal("request-value", ws.Headers["Custom-Request-Header"]);

        await StartSessionAsync(ws, 3);

        Assert.Equal(
            new[]
            {
                """{"message_type":"input_audio_chunk","audio_base_64":"AQID","commit":false,"sample_rate":16000,"previous_text":"Earlier context"}""",
                """{"message_type":"input_audio_chunk","audio_base_64":"BAUG","commit":false,"sample_rate":16000}""",
                """{"message_type":"input_audio_chunk","audio_base_64":"","commit":true,"sample_rate":16000}""",
            },
            ws.Sent.Select(sent => JsonNode.Parse((string)sent)!.ToJsonString())
        );

        ws.Receive("""{"message_type":"partial_transcript","text":"Hello wor"}""");
        ws.Receive("""{"message_type":"committed_transcript","text":"Hello world."}""");
        ws.Receive(
            """{"message_type":"committed_transcript_with_timestamps","text":"Hello world.","language_code":"en","words":[{"text":"Hello","start":0,"end":0.4,"type":"word"},{"text":" ","start":0.4,"end":0.45,"type":"spacing"},{"text":"world.","start":0.45,"end":0.9,"type":"word"}]}"""
        );

        AssertParts(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"type":"transcript-partial","id":"session-1:0","text":"Hello wor"},
              {"type":"transcript-final","id":"session-1:0","text":"Hello world."},
              {"type":"finish","text":"Hello world.","segments":[{"text":"Hello","startSecond":0,"endSecond":0.4},{"text":" ","startSecond":0.4,"endSecond":0.45},{"text":"world.","startSecond":0.45,"endSecond":0.9}],"language":"en","durationInSeconds":0.9}
            ]
            """,
            await parts
        );
        Assert.Equal(testDate.UtcDateTime, result!.Response!.Timestamp);
        Assert.Equal("scribe_v2_realtime", result.Response.ModelId);
        Assert.Contains(1000, ws.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "detects language without returning timestamps unless requested",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Detects_language_without_returning_timestamps_unless_requested()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Transcription(),
            Audio(new byte[] { 1 }),
            providerOptions: """{"elevenlabs":{"streaming":{"includeLanguageDetection":true,"includeTimestamps":false}}}"""
        );

        var parts = Collect(result!);
        var ws = Socket();
        Assert.Equal("true", Query(ws.Url)["include_language_detection"]);
        Assert.Equal("true", Query(ws.Url)["include_timestamps"]);

        await StartSessionAsync(ws, 2);
        ws.Receive("""{"message_type":"committed_transcript","text":"Hola"}""");
        ws.Receive(
            """{"message_type":"committed_transcript_with_timestamps","text":"Hola","language_code":"es","words":[{"text":"Hola","start":0,"end":0.4,"type":"word"}]}"""
        );

        AssertParts(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"type":"transcript-final","id":"session-1:0","text":"Hola"},
              {"type":"finish","text":"Hola","segments":[],"language":"es"}
            ]
            """,
            await parts
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "finishes with committed text when the socket closes before timestamps arrive",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Finishes_with_committed_text_when_the_socket_closes_before_timestamps_arrive()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Transcription(),
            Audio(new byte[] { 1 }),
            providerOptions: TimestampsOptions
        );

        var parts = Collect(result!);
        var ws = Socket();
        await StartSessionAsync(ws, 2);
        ws.Receive("""{"message_type":"committed_transcript","text":"Hello"}""");
        ws.ServerClose();

        AssertParts(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"type":"transcript-final","id":"session-1:0","text":"Hello"},
              {"type":"finish","text":"Hello","segments":[]}
            ]
            """,
            await parts
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "finishes a timestamp-only response to the final commit",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Finishes_a_timestamp_only_response_to_the_final_commit()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Transcription(),
            Audio(new byte[] { 1 }),
            providerOptions: TimestampsOptions
        );

        var parts = Collect(result!);
        var ws = Socket();
        await StartSessionAsync(ws, 2);
        ws.Receive(
            """{"message_type":"committed_transcript_with_timestamps","text":"Hello","language_code":"en","words":[{"text":"Hello","start":0,"end":0.4,"type":"word"}]}"""
        );

        AssertParts(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"type":"transcript-final","id":"session-1:0","text":"Hello","startSecond":0,"endSecond":0.4},
              {"type":"finish","text":"Hello","segments":[{"text":"Hello","startSecond":0,"endSecond":0.4}],"language":"en","durationInSeconds":0.4}
            ]
            """,
            await parts
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "drains commits received after the input ends and correlates partial IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Drains_commits_received_after_the_input_ends_and_correlates_partial_IDs()
    {
        MockWebSocket.Reset();
        var result = await DoStream(Transcription(), Audio(new byte[] { 1 }));

        var parts = Collect(result!);
        var ws = Socket();
        await StartSessionAsync(ws, 2);

        ws.Receive("""{"message_type":"partial_transcript","text":"First"}""");
        ws.Receive("""{"message_type":"committed_transcript","text":" First  "}""");
        await Wait.FlushAsync();
        Assert.Empty(ws.Closes);

        // A server-initiated VAD/36-second commit can arrive just before the
        // explicit final commit. The first one must not finish the stream early.
        ws.Receive("""{"message_type":"partial_transcript","text":"Second"}""");
        ws.Receive("""{"message_type":"committed_transcript","text":" Second "}""");

        AssertParts(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"type":"transcript-partial","id":"session-1:0","text":"First"},
              {"type":"transcript-final","id":"session-1:0","text":"First"},
              {"type":"transcript-partial","id":"session-1:1","text":"Second"},
              {"type":"transcript-final","id":"session-1:1","text":"Second"},
              {"type":"finish","text":"First Second","segments":[]}
            ]
            """,
            await parts
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "handles legacy final transcript event variants",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Handles_legacy_final_transcript_event_variants()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Transcription(),
            Audio(new byte[] { 1 }),
            providerOptions: TimestampsOptions
        );

        var parts = Collect(result!);
        var ws = Socket();
        await StartSessionAsync(ws, 2);
        ws.Receive("""{"message_type":"final_transcript","text":"Legacy final"}""");
        ws.Receive(
            """{"message_type":"final_transcript_with_timestamps","text":"Legacy final","language_code":"en","words":[{"text":"Legacy final","start":0,"end":0.5,"type":"word"}]}"""
        );

        AssertParts(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"type":"transcript-final","id":"session-1:0","text":"Legacy final"},
              {"type":"finish","text":"Legacy final","segments":[{"text":"Legacy final","startSecond":0,"endSecond":0.5}],"language":"en","durationInSeconds":0.5}
            ]
            """,
            await parts
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "finishes normally when the final empty commit is rejected",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Finishes_normally_when_the_final_empty_commit_is_rejected()
    {
        MockWebSocket.Reset();
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1 });
        var result = await DoStream(Transcription(), audio);

        var parts = Collect(result!);
        var ws = Socket();
        await StartSessionAsync(ws, 1);
        ws.Receive("""{"message_type":"committed_transcript","text":"Already done"}""");
        audio.Complete();
        await Wait.UntilAsync(() => ws.Sent.Count >= 2);
        ws.Receive("""{"message_type":"insufficient_audio_activity","error":"not enough audio"}""");

        AssertParts(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"type":"transcript-final","id":"session-1:0","text":"Already done"},
              {"type":"finish","text":"Already done","segments":[]}
            ]
            """,
            await parts
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors when the upstream closes before the final commit",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Errors_when_the_upstream_closes_before_the_final_commit()
    {
        MockWebSocket.Reset();
        var result = await DoStream(Transcription(), new AudioChunkStream());

        var parts = Collect(result!);
        var ws = Socket();
        ws.Open();
        ws.Receive("""{"message_type":"session_started","session_id":"session-1"}""");
        ws.ServerClose();

        var error = await Assert.ThrowsAsync<AiSdkException>(() => parts);
        Assert.Contains(
            "ElevenLabs realtime transcription stream closed before completion",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "supports 8 kHz mu-law input", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_8_kHz_mu_law_input()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Transcription(),
            Audio(new byte[] { 1 }),
            new AudioFormat("audio/pcmu", 8000)
        );

        var parts = Collect(result!);
        var ws = Socket();
        Assert.Equal("ulaw_8000", Query(ws.Url)["audio_format"]);
        await StartSessionAsync(ws, 2);
        ws.Receive("""{"message_type":"committed_transcript","text":"Hello"}""");
        Assert.NotNull(await parts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "defaults PCM input without a rate to 16 kHz",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Defaults_PCM_input_without_a_rate_to_16_kHz()
    {
        MockWebSocket.Reset();
        var result = await DoStream(Transcription(), Audio(), new AudioFormat("audio/pcm"));

        var parts = Collect(result!);
        var ws = Socket();
        Assert.Equal("pcm_16000", Query(ws.Url)["audio_format"]);
        await StartSessionAsync(ws, 1);
        ws.Receive("""{"message_type":"committed_transcript","text":"Hello"}""");
        Assert.NotNull(await parts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects non-realtime models and unsupported input formats",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_non_realtime_models_and_unsupported_input_formats()
    {
        MockWebSocket.Reset();
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            DoStream(Transcription("scribe_v2"), Audio(), new AudioFormat("audio/pcm", 16000))
        );

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            DoStream(Transcription(), Audio(), new AudioFormat("audio/pcm", 32000))
        );
        Assert.Contains("ElevenLabs realtime transcription supports", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects background filtering together with timestamps",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_background_filtering_together_with_timestamps()
    {
        MockWebSocket.Reset();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            DoStream(
                Transcription(),
                Audio(),
                providerOptions: """{"elevenlabs":{"streaming":{"filterBackgroundAudio":true,"includeTimestamps":true}}}"""
            )
        );
        Assert.Contains("cannot be combined", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects background filtering together with language detection",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_background_filtering_together_with_language_detection()
    {
        MockWebSocket.Reset();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            DoStream(
                Transcription(),
                Audio(),
                providerOptions: """{"elevenlabs":{"streaming":{"filterBackgroundAudio":true,"includeLanguageDetection":true}}}"""
            )
        );
        Assert.Contains("cannot be combined", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts explicit null values in realtime provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Accepts_explicit_null_values_in_realtime_provider_options()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Transcription(),
            Audio(),
            providerOptions: """
            {"elevenlabs":{"streaming":{"commitStrategy":null,"enableLogging":null,"filterBackgroundAudio":null,"includeLanguageDetection":null,"includeTimestamps":null,"keyterms":null,"minSilenceDurationMs":null,"minSpeechDurationMs":null,"noVerbatim":null,"previousText":null,"secondaryLanguages":null,"vadSilenceThresholdSecs":null,"vadThreshold":null}}}
            """
        );

        var query = Query(Socket().Url);
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["model_id"] = "scribe_v2_realtime",
                ["audio_format"] = "pcm_16000",
            },
            query
        );
        result!.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns about batch-only provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Warns_about_batch_only_provider_options()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Transcription(),
            Audio(new byte[] { 1 }),
            providerOptions: """{"elevenlabs":{"diarize":true,"numSpeakers":2}}"""
        );

        var parts = Collect(result!);
        var ws = Socket();
        await StartSessionAsync(ws, 2);
        ws.Receive("""{"message_type":"committed_transcript","text":"Hello"}""");

        var all = await parts;
        AssertParts(
            """
            [
              {"type":"stream-start","warnings":[
                {"type":"unsupported","feature":"providerOptions.elevenlabs.diarize","details":"ElevenLabs realtime transcription does not support diarize."},
                {"type":"unsupported","feature":"providerOptions.elevenlabs.numSpeakers","details":"ElevenLabs realtime transcription does not support numSpeakers."}
              ]}
            ]
            """,
            all.Take(1)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors the stream with the provider error message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Errors_the_stream_with_the_provider_error_message()
    {
        MockWebSocket.Reset();
        var result = await DoStream(Transcription(), Audio(new byte[] { 1 }));

        var parts = Collect(result!);
        var ws = Socket();
        ws.Open();
        ws.Receive("""{"message_type":"session_started","session_id":"session-1"}""");
        ws.Receive("""{"message_type":"quota_exceeded","error":"quota exhausted"}""");

        var error = await Assert.ThrowsAsync<AiSdkException>(() => parts);
        Assert.Contains("quota exhausted", error.Message);
        Assert.NotEmpty(ws.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "explains the native WebSocket header requirement on socket errors",
        Coverage = UpstreamCoverage.Covered,
        Note = "The default transport is ClientWebSocket, which fails to connect to a closed local port."
    )]
    public async Task Explains_the_native_WebSocket_header_requirement_on_socket_errors()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Transcription(
                configure: options => options.BaseUrl = "http://127.0.0.1:1",
                mockWebSocket: false
            ),
            Audio()
        );

        var error = await Assert.ThrowsAsync<AiSdkException>(() => Collect(result!));
        Assert.Contains("Pass a header-capable WebSocket implementation", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "cancels the audio stream when the WebSocket constructor throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Cancels_the_audio_stream_when_the_WebSocket_constructor_throws()
    {
        MockWebSocket.Reset();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var model = Transcription(configure: options =>
            options.WebSocket = (_, _, _) =>
                throw new InvalidOperationException("constructor failed")
        );

        var result = await DoStream(model, audio);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Collect(result!));

        Assert.Equal("constructor failed", error.Message);
        Assert.True(audioCancelled);
    }
}
