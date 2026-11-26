// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ElevenLabsSpeechStream.SpeechStreamHarness;

namespace Vercel.AI.Tests.ElevenLabsSpeechStream;

public sealed class ElevenLabsSpeechModelDoStreamTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-speech-stream.test.ts::ElevenLabsSpeechModel.doStream::";

    private const string Commit = "109a4fe2d1a511cd05a608a11321b7e87bb20cd6";

    private readonly SpeechStreamHarness _harness = new SpeechStreamHarness();

    [Fact]
    [UpstreamTest(
        Prefix + "should stream text deltas and audio over the stream-input WebSocket",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_stream_text_deltas_and_audio_over_the_stream_input_WebSocket()
    {
        var result = await DoStream(
            _harness.CreateModel(),
            TextParts("Hello ", "", "wörld 👋 "),
            voice: "voice-1",
            outputFormat: new SpeechAudioFormat("audio/pcm", 24000),
            speed: 1.1,
            language: "en",
            providerOptions: """
            {"elevenlabs":{"voiceSettings":{"stability":0.4,"similarityBoost":0.8},"seed":7,
            "applyTextNormalization":"off",
            "streaming":{"autoMode":true,"chunkLengthSchedule":[50,120],"inactivityTimeout":60,"syncAlignment":true}}}
            """
        );

        var socket = _harness.LastSocket;
        Assert.Equal(
            "wss://api.elevenlabs.io/v1/text-to-speech/voice-1/stream-input?model_id=eleven_flash_v2_5&output_format=pcm_24000&language_code=en&inactivity_timeout=60&sync_alignment=true&auto_mode=true&apply_text_normalization=off&seed=7",
            socket.Url.ToString()
        );
        // Authenticated in the init message, so native WebSockets work too.
        Assert.DoesNotContain(socket.Headers ?? new Dictionary<string, string>(), header =>
            string.Equals(header.Key, "xi-api-key", StringComparison.OrdinalIgnoreCase)
        );
        Assert.DoesNotContain("test-api-key", result.RequestBody!.Value.GetRawText());

        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 4);

        AssertJson(
            """
            [
              {"text":" ","voice_settings":{"stability":0.4,"similarity_boost":0.8,"speed":1.1},
               "generation_config":{"chunk_length_schedule":[50,120]},"xi-api-key":"test-api-key"},
              {"text":"Hello "},
              {"text":"wörld 👋 "},
              {"text":""}
            ]
            """,
            SentMessages(socket)
        );

        socket.Message(
            System.Text.Json.Nodes.JsonNode.Parse(
                """{"audio":"AQI=","alignment":{"chars":["H"],"charStartTimesMs":[0],"charDurationsMs":[5]}}"""
            )!
        );
        socket.Message(System.Text.Json.Nodes.JsonNode.Parse("""{"audio":"AwQ="}""")!);
        socket.Message(System.Text.Json.Nodes.JsonNode.Parse("""{"audio":null,"isFinal":true}""")!);

        var parts = await ReadAllAsync(result.Stream);
        // 'Hello ' (6) + 'wörld 👋 ' (8 code points, 9 UTF-16 units)
        AssertJson(
            """
            [
              {"type":"stream-start","warnings":[]},
              {"type":"audio","audio":"AQI=","audioFormat":{"type":"audio/pcm","rate":24000},
               "providerMetadata":{"elevenlabs":{"alignment":{"chars":["H"],"charStartTimesMs":[0],"charDurationsMs":[5]}}}},
              {"type":"audio","audio":"AwQ=","audioFormat":{"type":"audio/pcm","rate":24000}},
              {"type":"finish","usage":{"inputCharacters":14}}
            ]
            """,
            parts.Select(Describe)
        );
        Assert.Contains(1000, socket.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should hold partial words until a word boundary and flush the rest at the end",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_hold_partial_words_until_a_word_boundary_and_flush_the_rest_at_the_end()
    {
        var result = await DoStream(
            _harness.CreateModel(),
            TextParts("Hel", "lo wor", "ld, how a", "re", " you")
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 6);

        AssertJson(
            """
            [{"text":"Hello "},{"text":"world, how "},{"text":"are "},{"text":"you"},{"text":""}]
            """,
            SentMessages(socket).Skip(1)
        );

        socket.Message(System.Text.Json.Nodes.JsonNode.Parse("""{"audio":"AQI="}""")!);
        socket.Message(System.Text.Json.Nodes.JsonNode.Parse("""{"isFinal":true}""")!);
        var parts = await ReadAllAsync(result.Stream);
        AssertJson("""{"type":"finish","usage":{"inputCharacters":24}}""", Describe(parts[^1]));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not hold text without spaces indefinitely",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_hold_text_without_spaces_indefinitely()
    {
        await DoStream(
            _harness.CreateModel(),
            TextParts("こんにちは", "世界。次", new string('x', 100))
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 4);

        AssertJson(
            "[{\"text\":\"こんにちは世界。\"},{\"text\":\"次"
                + new string('x', 100)
                + "\"},{\"text\":\"\"}]",
            SentMessages(socket).Skip(1)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should send keepalives while the text stream is idle, and none after the input ends",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The keepalive interval is shortened with ElevenLabsOptions.KeepaliveIntervalMs instead of fake timers."
    )]
    public async Task Should_send_keepalives_while_the_text_stream_is_idle_and_none_after_the_input_ends()
    {
        const int interval = 100;
        var text = new TrackedText([new SpeechTextPart("Thinking... ")], holdOpen: true);
        await DoStream(_harness.CreateModel(keepaliveIntervalMs: interval), text);
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 2);
        AssertJson("""[{"text":"Thinking... "}]""", SentMessages(socket).Skip(1));

        await Wait.UntilAsync(() => socket.Sent.Count >= 3);
        AssertJson(
            """[{"text":"Thinking... "},{"text":" "}]""",
            SentMessages(socket).Skip(1).Take(2)
        );

        text.Release();
        await Wait.UntilAsync(() => socket.Sent.Count >= 4);
        Assert.Equal("""{"text":""}""", SentMessages(socket).Last()!.ToJsonString());
        var sentAtEnd = socket.Sent.Count;
        await Task.Delay(interval * 4);
        Assert.Equal(sentAtEnd, socket.Sent.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not send keepalives when inactivityTimeout is set explicitly",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The keepalive interval is shortened with ElevenLabsOptions.KeepaliveIntervalMs instead of fake timers."
    )]
    public async Task Should_not_send_keepalives_when_inactivityTimeout_is_set_explicitly()
    {
        const int interval = 50;
        var text = new TrackedText([new SpeechTextPart("Thinking... ")], holdOpen: true);
        await DoStream(
            _harness.CreateModel(keepaliveIntervalMs: interval),
            text,
            providerOptions: """{"elevenlabs":{"streaming":{"inactivityTimeout":20}}}"""
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Task.Delay(interval * 12);
        AssertJson("""[{"text":"Thinking... "}]""", SentMessages(socket).Skip(1));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should error when a text part requests a different voice",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_error_when_a_text_part_requests_a_different_voice()
    {
        var result = await DoStream(
            _harness.CreateModel(),
            new TrackedText([
                new SpeechTextPart("Hello ", "voice-1"),
                new SpeechTextPart("there ", "voice-2"),
            ]),
            voice: "voice-1"
        );
        _harness.LastSocket.Open();
        await Assert.ThrowsAsync<InvalidArgumentError>(() => ReadAllAsync(result.Stream));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default the voice and output format",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_default_the_voice_and_output_format()
    {
        await DoStream(_harness.CreateModel(), TextParts("Hi"));

        Assert.Equal(
            "wss://api.elevenlabs.io/v1/text-to-speech/21m00Tcm4TlvDq8ikWAM/stream-input?model_id=eleven_flash_v2_5&output_format=mp3_44100_128",
            _harness.LastSocket.Url.ToString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include raw chunks when requested",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_include_raw_chunks_when_requested()
    {
        var result = await DoStream(
            _harness.CreateModel(),
            TextParts("Hi"),
            includeRawChunks: true
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 1);
        socket.Message(System.Text.Json.Nodes.JsonNode.Parse("""{"audio":"AQI="}""")!);
        socket.Message(System.Text.Json.Nodes.JsonNode.Parse("""{"isFinal":true}""")!);

        var parts = await ReadAllAsync(result.Stream);
        AssertJson(
            """[{"type":"raw","rawValue":{"audio":"AQI="}},{"type":"raw","rawValue":{"isFinal":true}}]""",
            parts.Where(part => part.Type == "raw").Select(Describe)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about unsupported options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_about_unsupported_options()
    {
        var result = await DoStream(
            _harness.CreateModel(),
            TextParts("Hi"),
            instructions: "whisper",
            providerOptions: """{"elevenlabs":{"previousText":"Before."}}"""
        );
        _harness.LastSocket.Open();
        await using var reader = result.Stream.GetAsyncEnumerator();

        Assert.True(await reader.MoveNextAsync());
        AssertJson(
            """
            {"type":"stream-start","warnings":[
              {"type":"unsupported","feature":"instructions",
               "details":"ElevenLabs speech models do not support instructions. Instructions parameter was ignored."},
              {"type":"unsupported","feature":"providerOptions.elevenlabs.previousText",
               "details":"ElevenLabs streaming speech does not support previousText."}
            ]}
            """,
            Describe(reader.Current)
        );
    }

    [Theory]
    [InlineData("single-voice streaming", "eleven_flash_v2_5", null)]
    [InlineData("streaming dialogue", "eleven_v4", """{"voices":["voice-a"]}""")]
    [UpstreamTest(
        Prefix + "should warn about HTTP-only generation options in $name",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_about_HTTP_only_generation_options_in_name(
        string name,
        string modelId,
        string? dialogue
    )
    {
        Assert.NotNull(name);
        var result = await DoStream(
            _harness.CreateModel(modelId),
            TextParts("Hi"),
            providerOptions: "{\"elevenlabs\":{"
                + (dialogue != null ? "\"dialogue\":" + dialogue + "," : string.Empty)
                + "\"withTimestamps\":true,\"inputs\":[{\"text\":\"Knock knock\",\"voiceId\":\"voice-a\"}],"
                + "\"musicLengthMs\":5000,\"durationSeconds\":2,\"loop\":true}}"
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await using var reader = result.Stream.GetAsyncEnumerator();

        Assert.True(await reader.MoveNextAsync());
        Assert.Equal("stream-start", reader.Current.Type);
        Assert.Equal(
            "providerOptions.elevenlabs.withTimestamps,providerOptions.elevenlabs.inputs,providerOptions.elevenlabs.musicLengthMs,providerOptions.elevenlabs.durationSeconds,providerOptions.elevenlabs.loop",
            string.Join(",", reader.Current.Warnings!.Select(warning => warning.Feature))
        );
        Assert.DoesNotContain("Knock", string.Join(",", socket.Sent.Select(sent => (string)sent)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should error on a server error message",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_error_on_a_server_error_message()
    {
        var result = await DoStream(_harness.CreateModel(), TextParts("Hi"));
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 1);
        socket.Message(
            System.Text.Json.Nodes.JsonNode.Parse(
                """{"message":"Invalid API key","error":"auth_error"}"""
            )!
        );

        var error = await Assert.ThrowsAsync<AiSdkException>(() => ReadAllAsync(result.Stream));
        Assert.Contains("ElevenLabs speech stream error: Invalid API key", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should error when the socket closes before isFinal",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_error_when_the_socket_closes_before_isFinal()
    {
        var result = await DoStream(_harness.CreateModel(), TextParts("Hi"));
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 1);
        socket.ServerClose(1008, "Have not received new text");

        var error = await Assert.ThrowsAsync<AiSdkException>(() => ReadAllAsync(result.Stream));
        Assert.Contains(
            "ElevenLabs speech stream closed before completion (code 1008: Have not received new text).",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should close the socket and cancel the text when the stream is cancelled",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_close_the_socket_and_cancel_the_text_when_the_stream_is_cancelled()
    {
        var text = new TrackedText([], holdOpen: true);
        var result = await DoStream(_harness.CreateModel(), text);
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 1);

        var reader = result.Stream.GetAsyncEnumerator();
        Assert.True(await reader.MoveNextAsync());
        await reader.DisposeAsync();

        Assert.NotEmpty(socket.Closes);
        await Wait.UntilAsync(() => text.Cancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should close the socket on abort",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit,
        Note = "SpeechModelStreamCall has no abort reason, so the stream fails with an OperationCanceledException instead of the abort reason."
    )]
    public async Task Should_close_the_socket_on_abort()
    {
        using var abort = new CancellationTokenSource();
        var result = await DoStream(
            _harness.CreateModel(),
            new TrackedText([], holdOpen: true),
            abortSignal: abort.Token
        );
        var socket = _harness.LastSocket;
        socket.Open();
        abort.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ReadAllAsync(result.Stream));
        Assert.NotEmpty(socket.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send versioned pronunciation dictionary locators",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_versioned_pronunciation_dictionary_locators()
    {
        await DoStream(
            _harness.CreateModel(),
            TextParts("Hi"),
            providerOptions: """
            {"elevenlabs":{"pronunciationDictionaryLocators":[{"pronunciationDictionaryId":"dict","versionId":"v1"}]}}
            """
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 1);

        AssertJson(
            """[{"pronunciation_dictionary_id":"dict","version_id":"v1"}]""",
            SentMessages(socket)[0]!["pronunciation_dictionary_locators"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject unversioned pronunciation dictionary locators before connecting",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_unversioned_pronunciation_dictionary_locators_before_connecting()
    {
        var text = new TrackedText([], holdOpen: true);
        var socketsBefore = _harness.SocketCount;

        await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            DoStream(
                _harness.CreateModel(),
                text,
                providerOptions: """
                {"elevenlabs":{"pronunciationDictionaryLocators":[{"pronunciationDictionaryId":"dict"}]}}
                """
            )
        );
        Assert.Equal(socketsBefore, _harness.SocketCount);
        Assert.True(text.Disposed);
    }

    [Theory]
    [InlineData("eleven_v3")]
    [InlineData("eleven_v3_conversational")]
    [InlineData("eleven_v4")]
    [UpstreamTest(
        Prefix
            + "should reject %s without dialogue options, since the TTS WebSocket does not serve it",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_modelId_without_dialogue_options_since_the_TTS_WebSocket_does_not_serve_it(
        string modelId
    )
    {
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            DoStream(_harness.CreateModel(modelId), TextParts("Hi"))
        );
    }
}
