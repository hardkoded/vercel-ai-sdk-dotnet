// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using TranslationModel = Vercel.AI.Google.GoogleSpeechTranslationModel;

namespace Vercel.AI.Tests.Upstream.Google.GoogleSpeechTranslationModel;

/// <summary>Port of <c>google-speech-translation-model.test.ts</c> &gt; <c>doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class DoStreamTests
{
    private const string Prefix =
        "packages/google/src/speech-translation/google-speech-translation-model.test.ts::doStream::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream translation over the Gemini Live API WebSocket",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_stream_translation_over_the_Gemini_Live_API_WebSocket()
    {
        MockWebSocket.Reset();
        var model = CreateModel(currentDate: () => DateTimeOffset.UnixEpoch);

        var (start, parts, ws) = await StartAsync(
            model,
            providerOptions: "{\"google\":{\"echoTargetLanguage\":true}}"
        );
        Assert.Equal(
            "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent?key=test-api-key",
            ws.Url.AbsoluteUri
        );
        Assert.Empty(ws.Headers!);

        ws.Open();
        await Wait.FlushAsync();

        // Audio is gated on setupComplete: only the setup message was sent.
        Assert.Single(ws.Sent);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"setup\":{\"model\":\"models/gemini-3.5-live-translate-preview\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"translationConfig\":{\"targetLanguageCode\":\"es\",\"echoTargetLanguage\":true}},\"inputAudioTranscription\":{},\"outputAudioTranscription\":{}}}"
                ),
                Sent(ws, 0)
            )
        );

        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"realtimeInput\":{\"audio\":{\"data\":\"AQID\",\"mimeType\":\"audio/pcm;rate=16000\"}}}"
                ),
                Sent(ws, 1)
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"realtimeInput\":{\"audioStreamEnd\":true}}"),
                Sent(ws, 2)
            )
        );

        ws.Message(Json("{\"serverContent\":{\"inputTranscription\":{\"text\":\"Hello\"}}}"));
        ws.Message(
            Json(
                "{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"BAUG\"}}]},\"outputTranscription\":{\"text\":\"Hola\"}}}"
            )
        );
        ws.Message(
            Json(
                "{\"usageMetadata\":{\"promptTokensDetails\":[{\"modality\":\"AUDIO\",\"tokenCount\":10},{\"modality\":\"TEXT\",\"tokenCount\":2}],\"responseTokensDetails\":[{\"modality\":\"AUDIO\",\"tokenCount\":20},{\"modality\":\"TEXT\",\"tokenCount\":3}]}}"
            )
        );
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));

        var result = await parts;
        Assert.Equal(
            new[]
            {
                "stream-start",
                "source-transcript-delta:google-item-0:Hello",
                "audio:google-item-0:BAUG",
                "output-text-delta:google-item-0:Hola",
                "source-transcript-final:google-item-0:Hello",
                "output-text-final:google-item-0:Hola",
                "finish:Hello:Hola",
            },
            Shape(result)
        );
        Assert.Empty(result[0].Warnings!);
        Assert.Equal(10, result[^1].Usage!.InputAudioTokens);
        Assert.Equal(20, result[^1].Usage!.OutputAudioTokens);
        Assert.Equal(DateTime.UnixEpoch, start.Response!.Timestamp);
        Assert.Equal("gemini-3.5-live-translate-preview", start.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about unsupported sourceLanguage and outputAudioFormat",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_warn_about_unsupported_sourceLanguage_and_outputAudioFormat()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(
            model,
            sourceLanguage: "en",
            outputAudioFormat: new AudioFormat("audio/pcm", 24000)
        );
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Json("{\"serverContent\":{\"outputTranscription\":{\"text\":\"Hola\"}}}"));
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));

        var result = await parts;
        Assert.Equal("stream-start", result[0].Type);
        Assert.Collection(
            result[0].Warnings!,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("sourceLanguage", warning.Feature);
                Assert.Equal(
                    "The Gemini Live translation API auto-detects the source language and does not accept a source language.",
                    warning.Details
                );
            },
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("outputAudioFormat", warning.Feature);
                Assert.Equal(
                    "The Gemini Live API always outputs 24kHz 16-bit PCM audio and does not accept an output audio format.",
                    warning.Details
                );
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject non-PCM input audio formats",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_reject_non_PCM_input_audio_formats()
    {
        var model = CreateModel();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            StartAsync(
                model,
                inputAudioFormat: new AudioFormat("audio/pcmu", 8000),
                expectSocket: false
            )
        );
        Assert.Contains(
            "The Gemini Live translation API only supports 16kHz 16-bit PCM input audio.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject unsupported input audio sample rates",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_reject_unsupported_input_audio_sample_rates()
    {
        var model = CreateModel();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            StartAsync(
                model,
                inputAudioFormat: new AudioFormat("audio/pcm", 24000),
                expectSocket: false
            )
        );
        Assert.Contains(
            "The Gemini Live translation API only supports 16kHz 16-bit PCM input audio.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accumulate multiple turns before the audio ends",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_accumulate_multiple_turns_before_the_audio_ends()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        // An audio stream that stays open until we complete it.
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });

        var (_, parts, ws) = await StartAsync(model, audio);
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 2);

        // The first turn completes while audio is still streaming: no finish yet.
        ws.Message(
            Json(
                "{\"serverContent\":{\"inputTranscription\":{\"text\":\"Hello \"},\"outputTranscription\":{\"text\":\"Hola \"}}}"
            )
        );
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));
        await Wait.FlushAsync();
        Assert.False(parts.IsCompleted);

        // Audio ends; the second turn completes and finishes the stream.
        audio.Complete();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);
        ws.Message(
            Json(
                "{\"serverContent\":{\"inputTranscription\":{\"text\":\"world\"},\"outputTranscription\":{\"text\":\"mundo\"}}}"
            )
        );
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));

        Assert.Equal(
            new[]
            {
                "stream-start",
                "source-transcript-delta:google-item-0:Hello ",
                "output-text-delta:google-item-0:Hola ",
                "source-transcript-final:google-item-0:Hello ",
                "output-text-final:google-item-0:Hola ",
                "source-transcript-delta:google-item-1:world",
                "output-text-delta:google-item-1:mundo",
                "source-transcript-final:google-item-1:world",
                "output-text-final:google-item-1:mundo",
                "finish:Hello world:Hola mundo",
            },
            Shape(await parts)
        );
        Assert.Null((await parts)[^1].Usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include all turns that arrive after the audio ended early",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_include_all_turns_that_arrive_after_the_audio_ended_early()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        // The shipped example sends all audio immediately: audioStreamEnd is set before any server turn arrives.
        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        // audioStreamEnd has been sent; two turns follow.
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"realtimeInput\":{\"audioStreamEnd\":true}}"),
                Sent(ws, 2)
            )
        );

        ws.Message(
            Json(
                "{\"serverContent\":{\"inputTranscription\":{\"text\":\"Hello \"},\"outputTranscription\":{\"text\":\"Hola \"}}}"
            )
        );
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));

        // Second-turn activity cancels the pending finish: no truncation.
        ws.Message(
            Json(
                "{\"serverContent\":{\"inputTranscription\":{\"text\":\"world\"},\"outputTranscription\":{\"text\":\"mundo\"}}}"
            )
        );
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));

        var result = await parts;
        var last = result[^1];
        Assert.Equal("finish", last.Type);
        Assert.Equal("Hello world", last.SourceText);
        Assert.Equal("Hola mundo", last.OutputText);
        Assert.Null(last.Usage);
        Assert.Equal(
            new[]
            {
                "output-text-final:google-item-0:Hola ",
                "output-text-final:google-item-1:mundo",
            },
            Shape(result).Where(part => part.StartsWith("output-text-final")).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should finish without further server messages when the turn completed before the audio ended",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_finish_without_further_server_messages_when_the_turn_completed_before_the_audio_ended()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });

        var (_, parts, ws) = await StartAsync(model, audio);
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 2);

        // The turn completes while the input audio stream is still open.
        ws.Message(
            Json(
                "{\"serverContent\":{\"inputTranscription\":{\"text\":\"Hello\"},\"outputTranscription\":{\"text\":\"Hola\"}}}"
            )
        );
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));
        await Wait.FlushAsync();

        // The audio then ends; the already-received turnComplete satisfies the finish condition
        // without any further server message (no hang).
        audio.Complete();

        var last = (await parts)[^1];
        Assert.Equal("finish", last.Type);
        Assert.Equal("Hello", last.SourceText);
        Assert.Equal("Hola", last.OutputText);
        Assert.Null(last.Usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should finish continuous translation after trailing output silence",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_finish_continuous_translation_after_trailing_output_silence()
    {
        MockWebSocket.Reset();
        var model = CreateModel(finishGraceMs: 500);

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(
            Json(
                "{\"serverContent\":{\"inputTranscription\":{\"text\":\"Hello\"},\"outputTranscription\":{\"text\":\"Hola\"}}}"
            )
        );
        ws.Message(
            Json(
                "{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\""
                    + Convert.ToBase64String(new byte[] { 1, 0 })
                    + "\"}}]}}}"
            )
        );

        var silence = Convert.ToBase64String(new byte[12_000]);
        var silenceMessage =
            "{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\""
            + silence
            + "\"}}]}}}";
        ws.Message(Json(silenceMessage));
        await Wait.FlushAsync();
        Assert.Empty(ws.Closes);

        ws.Message(Json(silenceMessage));

        var result = await parts;
        Assert.Equal("source-transcript-final:google-item-0:Hello", Shape(result)[^3]);
        Assert.Equal("output-text-final:google-item-0:Hola", Shape(result)[^2]);
        Assert.Equal("finish:Hello:Hola", Shape(result)[^1]);
        Assert.Null(result[^1].Usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should parse a real Google Live translation response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_parse_a_real_Google_Live_translation_response()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        var messages = File.ReadAllLines(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Fixtures",
                    "google-live-speech-translation.chunks.txt"
                )
            )
            .Where(line => line.Trim().Length > 0)
            .ToList();
        ws.Open();
        ws.Message(Json(messages[0]));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        foreach (var message in messages.Skip(1))
        {
            ws.Message(Json(message));
        }

        var result = await parts;
        Assert.Equal(
            new[] { "audio:google-item-0:AQIDBAUGBwg=", "audio:google-item-0:AAAAAAAAAAAAAAAA" },
            Shape(result).Where(part => part.StartsWith("audio:")).ToArray()
        );
        Assert.Equal(
            "source-transcript-final:google-item-0:The quick brown fox jumps over the lazy dog.",
            Shape(result)[^3]
        );
        Assert.Equal(
            "output-text-final:google-item-0:El zorro marrón rápido salta sobre el perro perezoso.",
            Shape(result)[^2]
        );
        var last = result[^1];
        Assert.Equal("finish", last.Type);
        Assert.Equal("The quick brown fox jumps over the lazy dog.", last.SourceText);
        Assert.Equal("El zorro marrón rápido salta sobre el perro perezoso.", last.OutputText);
        Assert.Equal(150, last.Usage!.InputAudioTokens);
        Assert.Equal(150, last.Usage.OutputAudioTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should finish when the server closes while a finish is pending",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_finish_when_the_server_closes_while_a_finish_is_pending()
    {
        MockWebSocket.Reset();

        // Grace period > 0 so the pending finish is still scheduled at close time.
        var graceModel = CreateModel(finishGraceMs: 10_000);

        var (_, parts, ws) = await StartAsync(graceModel);
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Json("{\"serverContent\":{\"outputTranscription\":{\"text\":\"Hola\"}}}"));
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));
        await Wait.FlushAsync();

        // A close while the grace-period finish is pending confirms completion.
        ws.ServerClose(1000, string.Empty);

        var last = (await parts)[^1];
        Assert.Equal("finish", last.Type);
        Assert.Equal(string.Empty, last.SourceText);
        Assert.Equal("Hola", last.OutputText);
        Assert.Null(last.Usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should error the stream with close diagnostics when the socket closes before finishing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_error_the_stream_with_close_diagnostics_when_the_socket_closes_before_finishing()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Json("{\"serverContent\":{\"outputTranscription\":{\"text\":\"Ho\"}}}"));
        ws.ServerClose(1011, "internal server error");

        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Contains(
            "Google Live translation WebSocket closed unexpectedly before finishing (code 1011, reason: internal server error).",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should error the stream when the abort signal fires mid-stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_error_the_stream_when_the_abort_signal_fires_mid_stream()
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
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Json("{\"serverContent\":{\"outputTranscription\":{\"text\":\"Ho\"}}}"));
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
    public async Task should_ignore_server_messages_after_the_stream_finished()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Json("{\"serverContent\":{\"outputTranscription\":{\"text\":\"Hola\"}}}"));
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));

        var result = await parts;
        Assert.Equal("finish", result[^1].Type);

        // Post-finish messages are a no-op (no throw, no new parts).
        ws.Message(Json("{\"serverContent\":{\"outputTranscription\":{\"text\":\"late\"}}}"));
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));
        await Wait.FlushAsync();
        Assert.Equal(4, result.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include raw provider chunks when includeRawChunks is enabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_include_raw_provider_chunks_when_includeRawChunks_is_enabled()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model, includeRawChunks: true);
        ws.Open();
        ws.Message(Json("{\"setupComplete\":{}}"));
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(Json("{\"serverContent\":{\"outputTranscription\":{\"text\":\"Hola\"}}}"));
        ws.Message(Json("{\"serverContent\":{\"turnComplete\":true}}"));

        var raw = (await parts).Where(part => part.Type == "raw").ToList();
        Assert.Equal(3, raw.Count);
        Assert.Equal("{\"setupComplete\":{}}", raw[0].Raw!.Value.GetRawText());
        Assert.Equal(
            "{\"serverContent\":{\"outputTranscription\":{\"text\":\"Hola\"}}}",
            raw[1].Raw!.Value.GetRawText()
        );
        Assert.Equal("{\"serverContent\":{\"turnComplete\":true}}", raw[2].Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when no API key is available",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_throw_when_no_API_key_is_available()
    {
        var model = CreateModel(headers: () => new Dictionary<string, string?>());

        var error = await Assert.ThrowsAsync<AiSdkException>(() =>
            StartAsync(model, expectSocket: false)
        );
        Assert.Contains(
            "Google Generative AI API key is required for streaming translation.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should find the API key with case-insensitive header lookup",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_find_the_API_key_with_case_insensitive_header_lookup()
    {
        MockWebSocket.Reset();
        var model = CreateModel(headers: () =>
            new Dictionary<string, string?> { ["X-Goog-Api-Key"] = "case-insensitive-key" }
        );

        var (start, _, ws) = await StartAsync(model);

        start.Stream.Cancel();
        Assert.Contains("key=case-insensitive-key", ws.Url.AbsoluteUri);
        Assert.Empty(ws.Headers!);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass non-credential headers to the WebSocket constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_pass_non_credential_headers_to_the_WebSocket_constructor()
    {
        MockWebSocket.Reset();
        var model = CreateModel(headers: () =>
            new Dictionary<string, string?>
            {
                ["x-goog-api-key"] = "test-api-key",
                ["Custom-Header"] = "custom-value",
            }
        );

        var (start, _, ws) = await StartAsync(model);

        start.Stream.Cancel();
        var header = Assert.Single(ws.Headers!);
        Assert.Equal("Custom-Header", header.Key);
        Assert.Equal("custom-value", header.Value);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should error the stream on server error messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_error_the_stream_on_server_error_messages()
    {
        MockWebSocket.Reset();
        var model = CreateModel();

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(Json("{\"error\":{\"message\":\"invalid target language\"}}"));

        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Contains("invalid target language", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel the audio stream when the WebSocket constructor throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_cancel_the_audio_stream_when_the_WebSocket_constructor_throws()
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
        Prefix + "should close the WebSocket and stop reading audio when the stream is cancelled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_close_the_WebSocket_and_stop_reading_audio_when_the_stream_is_cancelled()
    {
        MockWebSocket.Reset();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var model = CreateModel();

        var (start, _, ws) = await StartAsync(model, audio);
        ws.Open();
        await Wait.FlushAsync();

        start.Stream.Cancel();
        await Wait.FlushAsync();

        Assert.NotEmpty(ws.Closes);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be created by the provider translation factory",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task should_be_created_by_the_provider_translation_factory()
    {
        MockWebSocket.Reset();
        var provider = GoogleProvider.Create(
            new GoogleOptions { ApiKey = "test-api-key", WebSocket = MockWebSocket.Factory }
        );

        var model = provider.SpeechTranslationModel("gemini-3.5-live-translate-preview");
        Assert.Equal("google.generative-ai.speech-translation", model.Provider);
        Assert.Equal("gemini-3.5-live-translate-preview", model.ModelId);

        var (start, _, ws) = await StartAsync((TranslationModel)model);

        start.Stream.Cancel();
        Assert.Contains("key=test-api-key", ws.Url.AbsoluteUri);
    }

    private static TranslationModel CreateModel(
        Func<Dictionary<string, string?>>? headers = null,
        WebSocketFactory? webSocket = null,
        Func<DateTimeOffset>? currentDate = null,
        int finishGraceMs = 0
    )
    {
        return new TranslationModel(
            "gemini-3.5-live-translate-preview",
            new GoogleSpeechTranslationModelConfig(
                "test-provider",
                "https://generativelanguage.googleapis.com/v1beta",
                _ =>
                    Task.FromResult<IReadOnlyDictionary<string, string?>>(
                        headers?.Invoke()
                            ?? new Dictionary<string, string?>
                            {
                                ["x-goog-api-key"] = "test-api-key",
                            }
                    )
            )
            {
                WebSocket = webSocket ?? MockWebSocket.Factory,
                CurrentDate = currentDate,
                FinishGraceMs = finishGraceMs,
            }
        );
    }

    private static JsonNode Json(string text)
    {
        return JsonNode.Parse(text)!;
    }

    private static JsonNode? Sent(MockWebSocket socket, int index)
    {
        return JsonNode.Parse((string)socket.Sent[index]);
    }

    private static string[] Shape(List<ModelStreamPart> parts)
    {
        return parts
            .Select(part =>
                part.Type switch
                {
                    "source-transcript-delta" or "output-text-delta" => part.Type
                        + ":"
                        + part.Id
                        + ":"
                        + part.Delta,
                    "source-transcript-final" or "output-text-final" => part.Type
                        + ":"
                        + part.Id
                        + ":"
                        + part.Text,
                    "audio" => part.Type
                        + ":"
                        + part.Id
                        + ":"
                        + Convert.ToBase64String(part.Audio!),
                    "finish" => part.Type + ":" + part.SourceText + ":" + part.OutputText,
                    _ => part.Type,
                }
            )
            .ToArray();
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
        string providerOptions = "{}",
        bool expectSocket = true
    )
    {
        if (audio == null)
        {
            audio = new AudioChunkStream();
            audio.Write(new byte[] { 1, 2, 3 });
            audio.Complete();
        }

        var call = new TranslationStreamCall(
            audio,
            inputAudioFormat ?? new AudioFormat("audio/pcm", 16000),
            "es",
            sourceLanguage,
            outputAudioFormat,
            JsonSerializer.Deserialize<JsonElement>(providerOptions),
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
