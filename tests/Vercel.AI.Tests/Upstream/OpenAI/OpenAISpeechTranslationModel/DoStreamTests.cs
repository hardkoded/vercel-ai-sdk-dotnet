// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using TranslationModel = Vercel.AI.OpenAI.OpenAISpeechTranslationModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAISpeechTranslationModel;

/// <summary>Port of <c>openai-speech-translation-model.test.ts</c> &gt; <c>doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class DoStreamTests
{
    private const string Prefix =
        "packages/openai/src/speech-translation/openai-speech-translation-model.test.ts::doStream::";

    private const string SessionUpdate =
        "{\"type\":\"session.update\",\"session\":{\"audio\":{\"input\":{\"transcription\":{\"model\":\"gpt-realtime-whisper\"},\"noise_reduction\":null},\"output\":{\"language\":\"es\"}}}}";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should stream gpt-realtime-translate using the OpenAI realtime translations WebSocket",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_gpt_realtime_translate_using_the_OpenAI_realtime_translations_WebSocket()
    {
        MockWebSocket.Reset();
        var model = CreateModel(clock: () => DateTimeOffset.UnixEpoch);

        var (start, parts, ws) = await StartAsync(model);

        Assert.Equal(
            "wss://api.openai.com/v1/realtime/translations?model=gpt-realtime-translate",
            ws.Url.AbsoluteUri
        );
        Assert.Equal(new[] { "realtime", "openai-insecure-api-key.test-api-key" }, ws.Protocols);

        // OpenAI rejects handshakes that send both auth channels:
        Assert.DoesNotContain(ws.Headers!.Keys, key =>
            key.Equals("authorization", StringComparison.OrdinalIgnoreCase)
        );

        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(SessionUpdate), Sent(ws, 0)));
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"type\":\"session.input_audio_buffer.append\",\"audio\":\"AQID\"}"
                ),
                Sent(ws, 1)
            )
        );
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"session.close\"}"), Sent(ws, 2))
        );

        ws.Message(Event("session.input_transcript.delta", "event-1", "Hello"));
        ws.Message(Event("session.output_transcript.delta", "event-2", "Hola"));
        ws.Message(Event("session.output_audio.delta", "event-3", "BAUG"));
        ws.Message(Event("session.closed", "event-4"));

        var result = await parts;
        Assert.Equal(
            new[]
            {
                "stream-start",
                "source-transcript-delta",
                "output-text-delta",
                "audio",
                "source-transcript-final",
                "output-text-final",
                "finish",
            },
            result.Select(part => part.Type).ToArray()
        );
        Assert.Empty(result[0].Warnings!);
        Assert.Equal("Hello", result[1].Delta);
        Assert.Equal("Hola", result[2].Delta);
        Assert.Equal(Convert.FromBase64String("BAUG"), result[3].Audio);
        Assert.Equal("Hello", result[4].Text);
        Assert.Equal("Hola", result[5].Text);
        Assert.Equal("Hello", result[6].SourceText);
        Assert.Equal("Hola", result[6].OutputText);
        Assert.Null(result[6].Usage);
        Assert.Equal(DateTime.UnixEpoch, start.Response!.Timestamp);
        Assert.Equal("gpt-realtime-translate", start.Response.ModelId);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(start.Request!.Body!), Sent(ws, 0)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accumulate transcript deltas until the session closes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accumulate_transcript_deltas_until_the_session_closes()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Event("session.input_transcript.delta", "event-1", "Hello "));
        ws.Message(Event("session.input_transcript.delta", "event-2", "world"));
        ws.Message(Event("session.output_transcript.delta", "event-3", "Hola "));
        ws.Message(Event("session.output_transcript.delta", "event-4", "mundo"));
        ws.Message(Event("session.closed", "event-5"));

        var last = (await parts)[^1];
        Assert.Equal("finish", last.Type);
        Assert.Equal("Hello world", last.SourceText);
        Assert.Equal("Hola mundo", last.OutputText);
        Assert.Null(last.Usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should parse a real OpenAI Realtime translation response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_a_real_OpenAI_Realtime_translation_response()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        var fixture = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "openai-realtime-speech-translation.chunks.txt"
        );
        foreach (var line in File.ReadAllLines(fixture).Where(line => line.Trim().Length > 0))
        {
            ws.Message(JsonNode.Parse(line)!);
        }

        var result = await parts;
        var audio = Assert.Single(result, part => part.Type == "audio");
        Assert.Equal(Convert.FromBase64String("AQID"), audio.Audio);
        Assert.Equal("source-transcript-final", result[^3].Type);
        Assert.Equal(" The quick brown fox jumps over the lazy", result[^3].Text);
        Assert.Equal("output-text-final", result[^2].Type);
        Assert.Equal("La rápida zorra marrón salta sobre el perro perezoso.", result[^2].Text);
        Assert.Equal("finish", result[^1].Type);
        Assert.Equal(" The quick brown fox jumps over the lazy", result[^1].SourceText);
        Assert.Equal(
            "La rápida zorra marrón salta sobre el perro perezoso.",
            result[^1].OutputText
        );
        Assert.Null(result[^1].Usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about unsupported sourceLanguage and outputAudioFormat",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_about_unsupported_sourceLanguage_and_outputAudioFormat()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(
            model,
            sourceLanguage: "en",
            outputAudioFormat: "audio/pcm"
        );
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(SessionUpdate), Sent(ws, 0)));

        ws.Message(Event("session.output_transcript.delta", "event-1", "Hola"));
        ws.Message(Event("session.closed", "event-2"));

        var result = await parts;
        Assert.Equal("stream-start", result[0].Type);
        Assert.Collection(
            result[0].Warnings!,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("sourceLanguage", warning.Feature);
                Assert.Equal(
                    "The OpenAI Realtime translation API auto-detects the source language and does not accept a source language.",
                    warning.Details
                );
            },
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("outputAudioFormat", warning.Feature);
                Assert.Equal(
                    "The OpenAI Realtime translation API always outputs 24kHz 16-bit PCM audio and does not accept an output audio format.",
                    warning.Details
                );
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include raw provider chunks when includeRawChunks is enabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_raw_provider_chunks_when_includeRawChunks_is_enabled()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model, includeRawChunks: true);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(new JsonObject { ["type"] = "session.updated" });
        ws.Message(Event("session.output_transcript.delta", "event-1", "Hola"));
        ws.Message(Event("session.closed", "event-2"));

        var raw = (await parts).Where(part => part.Type == "raw").ToList();
        Assert.Equal(3, raw.Count);
        Assert.Equal("{\"type\":\"session.updated\"}", raw[0].Raw!.Value.GetRawText());
        Assert.True(
            JsonNode.DeepEquals(
                Event("session.output_transcript.delta", "event-1", "Hola"),
                JsonNode.Parse(raw[1].Raw!.Value.GetRawText())
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                Event("session.closed", "event-2"),
                JsonNode.Parse(raw[2].Raw!.Value.GetRawText())
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should strip only the Authorization header and pass other headers to the WebSocket constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_only_the_Authorization_header_and_pass_other_headers_to_the_WebSocket_constructor()
    {
        MockWebSocket.Reset();
        var model = CreateModel(() =>
            new Dictionary<string, string?>
            {
                ["Authorization"] = "Bearer test-api-key",
                ["OpenAI-Organization"] = "test-organization",
                ["Custom-Header"] = "custom-value",
            }
        );

        var (_, parts, ws) = await StartAsync(model);

        Assert.Equal(new[] { "realtime", "openai-insecure-api-key.test-api-key" }, ws.Protocols);
        Assert.Equal("test-organization", ws.Headers!["OpenAI-Organization"]);
        Assert.Equal("custom-value", ws.Headers["Custom-Header"]);
        Assert.DoesNotContain(ws.Headers.Keys, key =>
            key.Equals("authorization", StringComparison.OrdinalIgnoreCase)
        );

        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);
        ws.Message(Event("session.output_transcript.delta", "event-1", "Hola"));
        ws.Message(Event("session.closed", "event-2"));
        Assert.NotNull(await parts);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should use only the realtime protocol and pass headers unchanged when there is no Authorization header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_only_the_realtime_protocol_and_pass_headers_unchanged_when_there_is_no_Authorization_header()
    {
        MockWebSocket.Reset();
        var model = CreateModel(() =>
            new Dictionary<string, string?> { ["Custom-Header"] = "custom-value" }
        );

        var (start, _, ws) = await StartAsync(model);

        start.Stream.Cancel();
        Assert.Equal(new[] { "realtime" }, ws.Protocols);
        Assert.Equal("custom-value", ws.Headers!["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel the audio stream when the WebSocket constructor throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cancel_the_audio_stream_when_the_WebSocket_constructor_throws()
    {
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var model = CreateModel(
            webSocket: (_, _, _) => throw new InvalidOperationException("constructor failed")
        );

        var (_, parts, _) = await StartAsync(model, audio, expectSocket: false);

        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Equal("constructor failed", error.Message);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit recoverable server errors and continue streaming",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_recoverable_server_errors_and_continue_streaming()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(
            new JsonObject
            {
                ["type"] = "error",
                ["error"] = new JsonObject { ["message"] = "invalid target language" },
            }
        );
        ws.Message(Event("session.output_transcript.delta", "event-1", "Hola"));
        ws.Message(Event("session.closed", "event-2"));

        var result = await parts;
        var error = Assert.Single(result, part => part.Type == "error");
        Assert.Contains("invalid target language", error.Error!.Message);
        var last = result[^1];
        Assert.Equal("finish", last.Type);
        Assert.Equal(string.Empty, last.SourceText);
        Assert.Equal("Hola", last.OutputText);
        Assert.Null(last.Usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should close the WebSocket and stop reading audio when the stream is cancelled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_close_the_WebSocket_and_stop_reading_audio_when_the_stream_is_cancelled()
    {
        MockWebSocket.Reset();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var model = CreateModel();

        var (start, _, ws) = await StartAsync(model, audio);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 1);

        start.Stream.Cancel();
        await Wait.FlushAsync();

        Assert.NotEmpty(ws.Closes);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be created by the provider translation factory with realtime auth",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_created_by_the_provider_translation_factory_with_realtime_auth()
    {
        MockWebSocket.Reset();
        var provider = OpenAIProvider.Create(
            new OpenAIOptions { ApiKey = "test-api-key", WebSocket = MockWebSocket.Factory }
        );

        var model = provider.SpeechTranslationModel("gpt-realtime-translate");
        Assert.Equal("openai.speech-translation", model.Provider);
        Assert.Equal("gpt-realtime-translate", model.ModelId);

        var (start, _, ws) = await StartAsync((TranslationModel)model);

        start.Stream.Cancel();
        Assert.Equal(new[] { "realtime", "openai-insecure-api-key.test-api-key" }, ws.Protocols);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject unsupported input audio types",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_unsupported_input_audio_types()
    {
        var model = CreateModel();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            StartAsync(
                model,
                inputAudioFormat: new AudioFormat("audio/pcmu", 24000),
                expectSocket: false
            )
        );
        Assert.Contains(
            "The OpenAI Realtime translation API only supports 24kHz 16-bit PCM input audio.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject unsupported input audio sample rates",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_unsupported_input_audio_sample_rates()
    {
        var model = CreateModel();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            StartAsync(
                model,
                inputAudioFormat: new AudioFormat("audio/pcm", 16000),
                expectSocket: false
            )
        );
        Assert.Contains(
            "The OpenAI Realtime translation API only supports 24kHz 16-bit PCM input audio.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should skip empty output audio deltas",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_skip_empty_output_audio_deltas()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Event("session.output_audio.delta", "event-1", string.Empty));
        ws.Message(Event("session.output_audio.delta", "event-2", "BAUG"));
        ws.Message(Event("session.output_transcript.delta", "event-3", "Hola"));
        ws.Message(Event("session.closed", "event-4"));

        var audio = Assert.Single((await parts), part => part.Type == "audio");
        Assert.Equal(Convert.FromBase64String("BAUG"), audio.Audio);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should error the stream with close diagnostics when the socket closes before finishing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_error_the_stream_with_close_diagnostics_when_the_socket_closes_before_finishing()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Event("session.output_transcript.delta", "event-1", "Ho"));
        ws.ServerClose(1011, "internal server error");

        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Contains(
            "OpenAI realtime translation WebSocket closed unexpectedly before finishing (code 1011, reason: internal server error).",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should error the stream when the abort signal fires mid-stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_error_the_stream_when_the_abort_signal_fires_mid_stream()
    {
        MockWebSocket.Reset();
        var model = CreateModel();
        using var abort = new CancellationTokenSource();

        var (_, parts, ws) = await StartAsync(
            model,
            cancellationToken: abort.Token,
            abortReason: new InvalidOperationException("user aborted")
        );
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Event("session.output_transcript.delta", "event-1", "Ho"));
        await Wait.FlushAsync();

        abort.Cancel();

        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Equal("user aborted", error.Message);
        Assert.NotEmpty(ws.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore server messages after the stream finished",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_ignore_server_messages_after_the_stream_finished()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Event("session.output_transcript.delta", "event-1", "Hola"));
        ws.Message(Event("session.closed", "event-2"));

        var result = await parts;
        Assert.Equal("finish", result[^1].Type);

        // post-finish messages are a no-op (no throw, no new parts):
        ws.Message(Event("session.output_transcript.delta", "event-3", "late"));
        ws.Message(Event("session.closed", "event-4"));
        await Wait.FlushAsync();
        Assert.Equal(4, result.Count);
    }

    private static TranslationModel CreateModel(
        Func<Dictionary<string, string?>>? headers = null,
        WebSocketFactory? webSocket = null,
        Func<DateTimeOffset>? clock = null
    )
    {
        return new TranslationModel(
            "gpt-realtime-translate",
            "test-provider",
            new ProviderHttp(new HttpClient(new OpenAICapture())),
            path => new Uri("https://api.openai.com/v1/" + path),
            headers
                ?? (
                    () =>
                        new Dictionary<string, string?>
                        {
                            ["Authorization"] = "Bearer test-api-key",
                        }
                ),
            webSocket ?? MockWebSocket.Factory,
            clock
        );
    }

    /// <summary>One chunk and then the end of the audio, like <c>convertArrayToReadableStream([new Uint8Array([1, 2, 3])])</c>.</summary>
    private static AudioChunkStream Audio()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });
        audio.Complete();
        return audio;
    }

    private static JsonObject Event(string type, string eventId, string? delta = null)
    {
        var value = new JsonObject { ["type"] = type, ["event_id"] = eventId };
        if (delta != null)
        {
            value["delta"] = delta;
        }

        return value;
    }

    private static JsonNode? Sent(MockWebSocket socket, int index)
    {
        return JsonNode.Parse((string)socket.Sent[index]);
    }

    private static async Task<(
        TranslationStreamStart Start,
        Task<List<ModelStreamPart>> Parts,
        MockWebSocket Socket
    )> StartAsync(
        TranslationModel model,
        AudioChunkStream? audio = null,
        string? sourceLanguage = null,
        AudioFormat? outputAudioFormat = null,
        bool? includeRawChunks = null,
        AudioFormat? inputAudioFormat = null,
        CancellationToken cancellationToken = default,
        Exception? abortReason = null,
        bool expectSocket = true
    )
    {
        var call = new TranslationStreamCall(
            audio ?? Audio(),
            inputAudioFormat ?? new AudioFormat("audio/pcm", 24000),
            "es",
            sourceLanguage,
            outputAudioFormat,
            JsonSerializer.Deserialize<JsonElement>("{}"),
            new Dictionary<string, string>(),
            includeRawChunks,
            cancellationToken,
            abortReason
        );
        var start = await model.DoStreamAsync(call, CancellationToken.None);
        return (start, Collect(start), expectSocket ? MockWebSocket.Instances[^1] : null!);
    }

    private static async Task<List<ModelStreamPart>> Collect(TranslationStreamStart start)
    {
        var parts = new List<ModelStreamPart>();
        await foreach (var part in start.Stream.ReadAsync())
        {
            parts.Add(part);
        }

        return parts;
    }
}
