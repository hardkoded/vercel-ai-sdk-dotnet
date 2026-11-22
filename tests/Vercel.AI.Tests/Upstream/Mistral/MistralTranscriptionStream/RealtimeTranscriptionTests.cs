// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralTranscriptionStream;

/// <summary>Port of <c>mistral-transcription-stream.test.ts</c> &gt; <c>realtime transcription</c>.</summary>
[Collection("MockWebSocket")]
public sealed class RealtimeTranscriptionTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-transcription-stream.test.ts::realtime transcription::";
    private const string RealtimeModel = "voxtral-mini-transcribe-realtime-3";

    private static readonly AudioFormat Pcm24 = new AudioFormat("audio/pcm", 24000);

    private static Vercel.AI.Mistral.MistralTranscriptionModel Model(
        string modelId = RealtimeModel,
        Action<MistralOptions>? configure = null
    )
    {
        var options = new MistralOptions { ApiKey = "test-key", WebSocket = MockWebSocket.Factory };
        configure?.Invoke(options);
        return new Vercel.AI.Mistral.MistralTranscriptionModel(
            MistralProvider.Create(options),
            modelId,
            () => DateTime.UnixEpoch
        );
    }

    private static AudioChunkStream Audio(params object[] chunks)
    {
        var stream = new AudioChunkStream();
        foreach (var chunk in chunks)
        {
            if (chunk is string base64)
            {
                stream.Write(base64);
            }
            else
            {
                stream.Write((byte[])chunk);
            }
        }

        stream.Complete();
        return stream;
    }

    private static TranscriptionStreamCall Call(
        AudioChunkStream audio,
        AudioFormat? format = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string?>? headers = null,
        bool? includeRawChunks = null,
        CancellationToken cancellationToken = default,
        Exception? abortReason = null
    )
    {
        using var options = JsonDocument.Parse(providerOptions);
        return new TranscriptionStreamCall(
            audio,
            format ?? Pcm24,
            options.RootElement.Clone(),
            headers ?? new Dictionary<string, string?>(),
            includeRawChunks,
            cancellationToken,
            abortReason
        );
    }

    private static Task<TranscriptionStreamStart?> DoStream(
        Vercel.AI.Mistral.MistralTranscriptionModel model,
        TranscriptionStreamCall call
    ) => model.DoStreamAsync(call, CancellationToken.None);

    private static async Task<(TranscriptionStreamStart Result, MockWebSocket Socket)> Start(
        TranscriptionStreamCall? call = null
    )
    {
        MockWebSocket.Reset();
        var result = await DoStream(Model(), call ?? Call(Audio()));
        var socket = MockWebSocket.Instances[0];
        socket.Message(Json("""{"type":"session.created"}"""));
        socket.Message(Json("""{"type":"session.updated"}"""));
        await Wait.FlushAsync();
        return (result!, socket);
    }

    private static async Task<List<ModelStreamPart>> Collect(TranscriptionStreamStart start)
    {
        var parts = new List<ModelStreamPart>();
        await foreach (var part in start.Stream.ReadAsync())
        {
            parts.Add(part);
        }

        return parts;
    }

    private static JsonNode Json(string text) => JsonNode.Parse(text)!;

    private static List<JsonNode> Sent(MockWebSocket socket) =>
        socket.Sent.Select(value => Json((string)value)).ToList();

    private static JsonNode SessionOf(MockWebSocket socket) => Sent(socket)[0]["session"]!;

    [Fact]
    [UpstreamTest(
        Prefix + "configures before sending bytes/base64 audio and waits for the final transcript",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Configures_before_sending_bytes_base64_audio_and_waits_for_the_final_transcript()
    {
        MockWebSocket.Reset();
        var result = await DoStream(
            Model(),
            Call(
                Audio(new byte[] { 1, 2 }, "AwQ="),
                providerOptions: """
                {"mistral":{"language":"en","contextBias":["AI SDK"],"targetStreamingDelayMs":480}}
                """
            )
        );
        var parts = Collect(result!);
        var socket = MockWebSocket.Instances[0];
        Assert.Equal(
            "wss://api.mistral.ai/v1/audio/transcriptions/realtime?model=voxtral-mini-transcribe-realtime-3",
            socket.Url.AbsoluteUri
        );
        Assert.Empty(socket.Sent);
        socket.Message(Json("""{"type":"session.created"}"""));
        await Wait.FlushAsync();
        var first = Sent(socket);
        Assert.Single(first);
        Assert.True(
            JsonNode.DeepEquals(
                Json(
                    """
                    {"type":"session.update","session":{"audio_format":{"encoding":"pcm_s16le","sample_rate":24000},"language":"en","context_bias":["AI SDK"],"target_streaming_delay_ms":480}}
                    """
                ),
                first[0]
            )
        );
        socket.Message(Json("""{"type":"session.updated"}"""));
        await Wait.UntilAsync(() => socket.Sent.Count >= 4);
        var audio = Sent(socket).Skip(1).ToList();
        Assert.Equal(
            new[] { "input_audio.append", "input_audio.append", "input_audio.end" },
            audio.Select(message => (string?)message["type"])
        );
        Assert.Equal("AQI=", (string?)audio[0]["audio"]);
        Assert.Equal("AwQ=", (string?)audio[1]["audio"]);
        Assert.Empty(socket.Closes);
        socket.Message(Json("""{"type":"transcription.text.delta","text":"Hello"}"""));
        socket.Message(Json("""{"type":"transcription.text.delta","text":" world."}"""));
        socket.Message(
            Json(
                """
                {"type":"transcription.done","text":"Hello world.","language":"en","model":"resolved-model","segments":[],"usage":{"prompt_audio_seconds":2,"total_tokens":5}}
                """
            )
        );
        var events = await parts;
        Assert.Equal(
            new[] { "Hello", " world." },
            events.Where(part => part.Type == "transcript-delta").Select(part => part.Delta)
        );
        var finish = events[^1];
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hello world.", finish.Text);
        Assert.Equal("en", finish.Language);
        Assert.Equal(2, finish.DurationInSeconds);
        Assert.Equal(2, finish.UsageValue!.Value.GetProperty("prompt_audio_seconds").GetDouble());
        Assert.Equal(5, finish.UsageValue!.Value.GetProperty("total_tokens").GetDouble());
        Assert.Equal("response-metadata", events[^2].Type);
        Assert.Equal("resolved-model", events[^2].ModelId);
        Assert.DoesNotContain(events, part => part.Type == "raw");
        Assert.Equal(DateTime.UnixEpoch, result!.Response!.Timestamp);
        Assert.Equal(RealtimeModel, result.Response.ModelId);
        Assert.Single(socket.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes provider and request headers, custom URLs, and future model IDs through",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Passes_provider_and_request_headers_custom_URLs_and_future_model_IDs_through()
    {
        MockWebSocket.Reset();
        var model = Model("future/model", options =>
        {
            options.BaseUrl = "http://proxy.test/custom/";
            options.Headers["X-Provider"] = "provider";
            options.Headers["X-Override"] = "old";
        });
        var result = await DoStream(
            model,
            Call(
                Audio(),
                new AudioFormat("audio/pcm"),
                headers: new Dictionary<string, string?>
                {
                    ["X-Request"] = "request",
                    ["X-Override"] = "new",
                    ["X-Undefined"] = null,
                }
            )
        );
        var socket = MockWebSocket.Instances[0];
        Assert.Equal(
            "ws://proxy.test/custom/audio/transcriptions/realtime?model=future%2Fmodel",
            socket.Url.AbsoluteUri
        );
        var headers = new Dictionary<string, string>(
            socket.Headers!,
            StringComparer.OrdinalIgnoreCase
        );
        Assert.Equal("Bearer test-key", headers["authorization"]);
        Assert.Equal("provider", headers["x-provider"]);
        Assert.Equal("request", headers["X-Request"]);
        Assert.Equal("new", headers["X-Override"]);
        Assert.DoesNotContain("X-Undefined", headers.Keys);
        Assert.Contains("ai-sdk/mistral/", headers["user-agent"]);
        socket.Message(Json("""{"type":"session.created"}"""));
        await Wait.FlushAsync();
        Assert.Equal(16000, (int)SessionOf(socket)["audio_format"]!["sample_rate"]!);
        result!.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "resolves async API keys for each realtime session",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7716e189ee4d50b24661b2fe4cfe25130643529b"
    )]
    public async Task Resolves_async_API_keys_for_each_realtime_session()
    {
        MockWebSocket.Reset();
        var keys = new Queue<string>(["first-key", "second-key"]);
        var calls = 0;
        var model = Model(configure: options =>
            options.ApiKey = Resolvable<string>.From(() =>
            {
                calls++;
                return Task.FromResult(keys.Dequeue());
            })
        );

        foreach (var key in new[] { "first-key", "second-key" })
        {
            var result = await DoStream(model, Call(Audio()));
            var headers = new Dictionary<string, string>(
                MockWebSocket.Instances[^1].Headers!,
                StringComparer.OrdinalIgnoreCase
            );
            Assert.Equal($"Bearer {key}", headers["authorization"]);
            result!.Stream.Cancel();
        }

        Assert.Equal(2, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves untimed speaker segments without fabricating timestamps",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Preserves_untimed_speaker_segments_without_fabricating_timestamps()
    {
        var (result, socket) = await Start(
            Call(
                Audio(),
                providerOptions: """{"mistral":{"diarize":true,"speakerReferences":{"Jane":"AQI="}}}"""
            )
        );
        var session = SessionOf(socket);
        Assert.True((bool)session["diarize"]!);
        Assert.Equal("AQI=", (string?)session["speaker_references"]!["Jane"]);
        socket.Message(
            Json(
                """{"type":"transcription.segment","text":" Hi.","start":null,"end":null,"speaker_id":"Jane"}"""
            )
        );
        socket.Message(
            Json(
                """{"type":"transcription.segment","text":" Hello.","start":1,"end":2,"speaker_id":"speaker_2"}"""
            )
        );
        socket.Message(
            Json(
                """
                {"type":"transcription.done","text":" Hi. Hello.","language":null,"segments":[{"text":" Hi.","start":null,"end":null,"speaker_id":"Jane"},{"text":" Hello.","start":1,"end":2,"speaker_id":"speaker_2"}]}
                """
            )
        );
        var events = await Collect(result);
        Assert.Equal("transcript-final", events[1].Type);
        Assert.Equal(" Hi.", events[1].Text);
        Assert.Null(events[1].StartSecond);
        Assert.Null(events[1].EndSecond);
        Assert.Equal(
            "Jane",
            events[1]
                .ProviderMetadata!.Value.GetProperty("mistral")
                .GetProperty("speakerId")
                .GetString()
        );
        var finish = events[^1];
        Assert.Equal("finish", finish.Type);
        var segment = Assert.Single(finish.Segments!);
        Assert.Equal(" Hello.", segment.Text);
        Assert.Equal(1, segment.StartSecond);
        Assert.Equal(2, segment.EndSecond);
        Assert.True(
            JsonNode.DeepEquals(
                Json(
                    """
                    {"mistral":{"segments":[{"text":" Hi.","speakerId":"Jane"},{"text":" Hello.","startSecond":1,"endSecond":2,"speakerId":"speaker_2"}]}}
                    """
                ),
                JsonNode.Parse(finish.ProviderMetadata!.Value.GetRawText())
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes turn and unknown events as raw chunks without finalizing early",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Exposes_turn_and_unknown_events_as_raw_chunks_without_finalizing_early()
    {
        var (result, socket) = await Start(
            Call(
                Audio(),
                includeRawChunks: true,
                providerOptions: """
                {"mistral":{"turnDetection":{"startThreshold":0.5,"eagerEndThreshold":0.4,"endThreshold":0.3,"minSilenceMs":300,"endTimeoutMs":2500}}}
                """
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                Json(
                    """{"start_threshold":0.5,"eager_end_threshold":0.4,"end_threshold":0.3,"min_silence_ms":300,"end_timeout_ms":2500}"""
                ),
                SessionOf(socket)["turn_detection"]
            )
        );
        var turns = new[] { "turn.start", "turn.eager_end", "turn.resume", "turn.end" }
            .Select(type =>
                Json("{\"type\":\"" + type + "\",\"time_s\":1,\"end_reason\":\"threshold\"}")
            )
            .ToList();
        foreach (var turn in turns)
        {
            socket.Message(turn);
        }

        socket.Message(Json("""{"type":"future.event","new_field":true}"""));
        await Wait.FlushAsync();
        Assert.Empty(socket.Closes);
        socket.Message(Json("""{"type":"transcription.text.delta","text":"Trailing words."}"""));
        socket.Message(
            Json(
                """{"type":"transcription.done","text":"Trailing words.","segments":null,"usage":null}"""
            )
        );
        var events = await Collect(result);
        var raws = events
            .Where(part => part.Type == "raw")
            .Select(part => JsonNode.Parse(part.Raw!.Value.GetRawText())!)
            .ToList();
        foreach (var turn in turns)
        {
            Assert.Contains(raws, raw => JsonNode.DeepEquals(raw, turn));
        }

        Assert.Contains(raws, raw =>
            JsonNode.DeepEquals(raw, Json("""{"type":"future.event","new_field":true}"""))
        );
        Assert.Contains(events, part =>
            part.Type == "transcript-delta" && part.Delta == "Trailing words."
        );
        var finish = events[^1];
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Trailing words.", finish.Text);
        Assert.Empty(finish.Segments!);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "splits large audio chunks and skips empty chunks",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Splits_large_audio_chunks_and_skips_empty_chunks()
    {
        var bytes = Enumerable.Repeat((byte)7, 300000).ToArray();
        var (result, socket) = await Start(Call(Audio(Array.Empty<byte>(), bytes)));
        await Wait.UntilAsync(() => socket.Sent.Count >= 7);
        var messages = Sent(socket);
        var chunks = messages
            .Where(message => (string?)message["type"] == "input_audio.append")
            .ToList();
        Assert.Equal(5, chunks.Count);
        Assert.All(chunks, chunk => Assert.True(((string)chunk["audio"]!).Length < 262144));
        Assert.Equal(
            bytes,
            chunks.SelectMany(chunk => Convert.FromBase64String((string)chunk["audio"]!)).ToArray()
        );
        Assert.Equal("input_audio.end", (string?)messages[^1]["type"]);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not start a second sender on duplicate session events",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Does_not_start_a_second_sender_on_duplicate_session_events()
    {
        var (result, socket) = await Start();
        await Wait.UntilAsync(() => socket.Sent.Count >= 2);
        socket.Message(Json("""{"type":"session.created"}"""));
        socket.Message(Json("""{"type":"session.updated"}"""));
        await Wait.FlushAsync();
        Assert.Equal(2, socket.Sent.Count);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns about offline-only options without sending them",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Warns_about_offline_only_options_without_sending_them()
    {
        var (result, socket) = await Start(
            Call(
                Audio(),
                providerOptions: """
                {"mistral":{"returnLanguage":true,"temperature":0.2,"timestampGranularities":["segment"]}}
                """
            )
        );
        var enumerator = result.Stream.ReadAsync().GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        var first = enumerator.Current;
        Assert.Equal("stream-start", first.Type);
        Assert.Equal(
            new[]
            {
                "providerOptions.mistral.returnLanguage",
                "providerOptions.mistral.temperature",
                "providerOptions.mistral.timestampGranularities",
            },
            first.Warnings!.Select(warning => warning.Feature)
        );
        Assert.True(
            JsonNode.DeepEquals(
                Json("""{"audio_format":{"encoding":"pcm_s16le","sample_rate":24000}}"""),
                SessionOf(socket)
            )
        );
        result.Stream.Cancel();
    }

    [Theory]
    [InlineData("audio/mpeg", 24000.0)]
    [InlineData("audio/pcm", 0.0)]
    [InlineData("audio/pcm", 1.5)]
    [UpstreamTest(
        Prefix + "rejects unsupported input format %j before connecting",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Rejects_unsupported_input_format_before_connecting(string type, double rate)
    {
        MockWebSocket.Reset();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            DoStream(Model(), Call(Audio(), new AudioFormat(type, rate)))
        );
        Assert.Contains("requires audio/pcm", error.Message);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Theory]
    [InlineData("79")]
    [InlineData("1201")]
    [InlineData("2399")]
    [InlineData("2401")]
    [InlineData("480.5")]
    [UpstreamTest(
        Prefix + "rejects invalid delay %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Rejects_invalid_delay(string targetStreamingDelayMs)
    {
        MockWebSocket.Reset();
        await Assert.ThrowsAnyAsync<Exception>(() =>
            DoStream(
                Model(),
                Call(
                    Audio(),
                    providerOptions: "{\"mistral\":{\"targetStreamingDelayMs\":"
                        + targetStreamingDelayMs
                        + "}}"
                )
            )
        );
        Assert.Empty(MockWebSocket.Instances);
    }

    [Theory]
    [InlineData(80)]
    [InlineData(1200)]
    [InlineData(2400)]
    [UpstreamTest(
        Prefix + "accepts delay %s and default turn detection",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Accepts_delay_and_default_turn_detection(int targetStreamingDelayMs)
    {
        var (result, socket) = await Start(
            Call(
                Audio(),
                providerOptions: "{\"mistral\":{\"targetStreamingDelayMs\":"
                    + targetStreamingDelayMs
                    + ",\"turnDetection\":{}}}"
            )
        );
        var session = SessionOf(socket);
        Assert.Equal(targetStreamingDelayMs, (int)session["target_streaming_delay_ms"]!);
        Assert.Empty(session["turn_detection"]!.AsObject());
        result.Stream.Cancel();
    }

    [Theory]
    [InlineData("abort")]
    [InlineData("cancel")]
    [InlineData("provider-error")]
    [InlineData("socket-error")]
    [InlineData("close")]
    [InlineData("invalid-json")]
    [InlineData("invalid-event")]
    [InlineData("audio-error")]
    [UpstreamTest(
        Prefix + "cleans up on %s during audio streaming",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Cleans_up_during_audio_streaming(string failure)
    {
        var cancels = 0;
        var audio = new AudioChunkStream(_ => Interlocked.Increment(ref cancels));
        using var abort = new CancellationTokenSource();
        var (result, socket) = await Start(
            Call(
                audio,
                cancellationToken: abort.Token,
                abortReason: new InvalidOperationException("cancelled")
            )
        );
        if (failure == "cancel")
        {
            result.Stream.Cancel();
        }
        else
        {
            var parts = Collect(result);
            switch (failure)
            {
                case "abort":
                    abort.Cancel();
                    break;
                case "provider-error":
                    socket.Message(
                        Json(
                            """{"type":"error","error":{"message":"Diarization is not compatible with turn detection.","code":1}}"""
                        )
                    );
                    break;
                case "socket-error":
                    socket.OnError?.Invoke(null);
                    break;
                case "close":
                    socket.ServerClose(1006, "lost connection");
                    break;
                case "invalid-json":
                    socket.OnMessage?.Invoke("{");
                    break;
                case "invalid-event":
                    socket.Message(Json("""{"type":"transcription.text.delta","text":123}"""));
                    break;
                case "audio-error":
                    audio.Fail(new InvalidOperationException("microphone failed"));
                    break;
            }

            if (failure != "audio-error")
            {
                await Assert.ThrowsAnyAsync<Exception>(() => parts);
            }
            else
            {
                await Task.WhenAny(parts, Task.Delay(500));
                result.Stream.Cancel();
            }
        }

        await Wait.FlushAsync();
        Assert.Single(socket.Closes);
        if (failure != "audio-error")
        {
            Assert.Equal(1, Volatile.Read(ref cancels));
        }

        var count = socket.Sent.Count;
        socket.Message(Json("""{"type":"session.updated"}"""));
        socket.Message(Json("""{"type":"transcription.done","text":"late"}"""));
        await Wait.FlushAsync();
        Assert.Equal(count, socket.Sent.Count);
    }

    [Theory]
    [InlineData("abort")]
    [InlineData("cancel")]
    [InlineData("constructor")]
    [UpstreamTest(
        Prefix + "cancels upstream on %s before the session starts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Cancels_upstream_before_the_session_starts(string failure)
    {
        MockWebSocket.Reset();
        var cancels = 0;
        var audio = new AudioChunkStream(_ => Interlocked.Increment(ref cancels));
        using var abort = new CancellationTokenSource();
        if (failure == "abort")
        {
            abort.Cancel();
        }

        var model = Model("future-model", options =>
        {
            if (failure == "constructor")
            {
                options.WebSocket = (_, _, _) =>
                    throw new InvalidOperationException("constructor failed");
            }
        });
        var result = await DoStream(
            model,
            Call(
                audio,
                cancellationToken: abort.Token,
                abortReason: new InvalidOperationException("already aborted")
            )
        );
        if (failure == "cancel")
        {
            result!.Stream.Cancel();
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(() => Collect(result!));
        }

        Assert.Equal(1, Volatile.Read(ref cancels));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "pauses audio reads when the socket buffer is full and aborts the wait",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Pauses_audio_reads_when_the_socket_buffer_is_full_and_aborts_the_wait()
    {
        MockWebSocket.Reset();
        var cancels = 0;
        var audio = new AudioChunkStream(_ => Interlocked.Increment(ref cancels));
        audio.Write(new byte[] { 1, 2 });
        audio.Write(new byte[] { 3, 4 });
        using var abort = new CancellationTokenSource();
        var result = await DoStream(
            Model(),
            Call(
                audio,
                cancellationToken: abort.Token,
                abortReason: new InvalidOperationException("stop")
            )
        );
        var parts = Collect(result!);
        var socket = MockWebSocket.Instances[0];
        socket.ReadyState = 1;
        socket.BufferedAmount = 2 * 1024 * 1024;
        socket.Message(Json("""{"type":"session.created"}"""));
        socket.Message(Json("""{"type":"session.updated"}"""));
        await Wait.FlushAsync();
        Assert.Equal(2, socket.Sent.Count);
        abort.Cancel();
        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Contains("stop", error.Message);
        await Wait.UntilAsync(() => Volatile.Read(ref cancels) == 1);
        Assert.Equal(2, socket.Sent.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "drains the final message before handling an immediate socket close",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Drains_the_final_message_before_handling_an_immediate_socket_close()
    {
        var (result, socket) = await Start();
        socket.OnMessage?.Invoke(
            System.Text.Encoding.UTF8.GetBytes(
                """{"type":"transcription.done","text":"Complete."}"""
            )
        );
        socket.ServerClose(1000);
        var events = await Collect(result);
        Assert.Equal("finish", events[^1].Type);
        Assert.Equal("Complete.", events[^1].Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "resumes sending when backpressure clears",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Resumes_sending_when_backpressure_clears()
    {
        MockWebSocket.Reset();
        var result = await DoStream(Model(), Call(Audio(new byte[] { 1, 2 }, new byte[] { 3, 4 })));
        var socket = MockWebSocket.Instances[0];
        socket.ReadyState = 1;
        socket.BufferedAmount = 2 * 1024 * 1024;
        socket.Message(Json("""{"type":"session.created"}"""));
        socket.Message(Json("""{"type":"session.updated"}"""));
        await Wait.FlushAsync();
        Assert.Equal(2, socket.Sent.Count);
        socket.BufferedAmount = 0;
        await Wait.UntilAsync(() => socket.Sent.Count == 4);
        Assert.Equal("input_audio.end", (string?)Sent(socket)[3]["type"]);
        result!.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves the provider error message when followed immediately by close",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b26902ea1a0d65e893e2f48d2bcbedcf5df70926"
    )]
    public async Task Preserves_the_provider_error_message_when_followed_immediately_by_close()
    {
        var (result, socket) = await Start();
        socket.Message(Json("""{"type":"error","error":{"message":"invalid model","code":400}}"""));
        socket.ServerClose(1000);
        var error = await Assert.ThrowsAnyAsync<Exception>(() => Collect(result));
        Assert.Contains("invalid model", error.Message);
    }
}
