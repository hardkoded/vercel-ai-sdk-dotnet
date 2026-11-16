// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Cartesia;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Cartesia.CartesiaTranscriptionModel;

/// <summary>Port of <c>cartesia-transcription-model.test.ts</c> &gt; <c>doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class DoStreamTests
{
    private const string Prefix = "packages/cartesia/src/cartesia-transcription-model.test.ts::doStream::";
    private static readonly AudioFormat Pcm16 = new AudioFormat("audio/pcm", 16000);
    private static readonly AudioFormat Pcm24 = new AudioFormat("audio/pcm", 24000);

    [Theory]
    [InlineData("ink-3")]
    [InlineData("new-streaming-model")]
    [UpstreamTest(Prefix + "routes %s through streaming transcription instead of the batch endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Routes_the_model_through_streaming_transcription_instead_of_the_batch_endpoint(string modelId)
    {
        MockWebSocket.Reset();
        var handler = Handler();
        var model = Model(modelId, handler);
        var call = new TranscriptionModelCall(Array.Empty<byte>(), "audio/wav", OperationJson.Parse("{}"), new Dictionary<string, string>(), CancellationToken.None);
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => ((ITranscriptionCaller)model).DoGenerateAsync(call, CancellationToken.None));
        Assert.Empty(handler.Calls);

        var result = await Stream(model, Audio(), Pcm16);
        Assert.Equal("https://api.cartesia.ai/access-token", handler.Calls[0].Uri.ToString());
        var socket = MockWebSocket.Instances[0];
        Assert.Equal("/stt/turns/websocket", socket.Url.AbsolutePath);
        Assert.Equal(modelId, Query(socket.Url)["model"][0]);
        socket.Open();
        result.Stream.Cancel();
        Assert.NotEmpty(socket.Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "streams Ink 2 turn-detected transcription over WebSocket", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_Ink_2_turn_detected_transcription_over_WebSocket()
    {
        MockWebSocket.Reset();
        var handler = Handler();
        var testDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var provider = CartesiaProvider.Create(new CartesiaOptions { ApiKey = "test-api-key", Version = "2026-03-01", WebSocket = MockWebSocket.Factory }, handler);
        var model = new Vercel.AI.Cartesia.CartesiaTranscriptionModel(provider, "ink-2", () => testDate);
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });
        audio.Complete();

        var result = await Stream(model, audio, Pcm16, providerOptions: @"{""cartesia"":{""language"":""en""}}");

        Assert.Equal("https://api.cartesia.ai/access-token", handler.Calls[0].Uri.ToString());
        JsonAssert.Equal(OperationJson.Parse(handler.Calls[0].Text), @"{""grants"":{""stt"":true}}");
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("2026-03-01", handler.Calls[0].Header("cartesia-version"));

        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        Assert.Equal(
            "wss://api.cartesia.ai/stt/turns/websocket?model=ink-2&encoding=pcm_s16le&sample_rate=16000&cartesia_version=2026-03-01&access_token=test-access-token",
            socket.Url.AbsoluteUri);

        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 2);
        Assert.Equal(new byte[] { 1, 2, 3 }, (byte[])socket.Sent[0]);
        Assert.Equal(@"{""type"":""close""}", socket.Sent[1]);

        socket.Message(new JsonObject { ["type"] = "turn.update", ["request_id"] = "turn-1", ["transcript"] = "Hello" });
        socket.Message(new JsonObject { ["type"] = "turn.end", ["request_id"] = "turn-1", ["transcript"] = "Hello world" });
        socket.Message(new JsonObject { ["type"] = "turn.end", ["request_id"] = "turn-1", ["transcript"] = "How are you?" });
        await Wait.FlushAsync();
        socket.ServerClose();

        var values = await parts;
        Assert.Equal(new[] { "stream-start", "transcript-partial", "transcript-final", "transcript-final", "finish" }, values.Select(part => part.Type));
        Assert.Empty(values[0].Warnings!);
        Assert.Equal(("turn-1", "Hello"), (values[1].Id, values[1].Text));
        Assert.Equal(("turn-1", "Hello world"), (values[2].Id, values[2].Text));
        Assert.Equal(("turn-1", "How are you?"), (values[3].Id, values[3].Text));
        Assert.Equal("Hello world How are you?", values[4].Text);
        Assert.Empty(values[4].Segments!);
        Assert.Equal("en", values[4].Language);
        Assert.Null(values[4].DurationInSeconds);
        Assert.Equal(
            "wss://api.cartesia.ai/stt/turns/websocket?model=ink-2&encoding=pcm_s16le&sample_rate=16000&cartesia_version=2026-03-01",
            result.Request!.Body);
        Assert.Equal(testDate, result.Response!.Timestamp);
        Assert.Equal("ink-2", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "supports manual finalization when turn detection is disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_manual_finalization_when_turn_detection_is_disabled()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model("ink-2"), Audio(), new AudioFormat("audio/pcmu", 8000), providerOptions: @"{""cartesia"":{""language"":""en"",""streaming"":{""turnDetection"":false}}}");
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        Assert.Contains("wss://api.cartesia.ai/stt/websocket?model=ink-2&encoding=pcm_mulaw&sample_rate=8000", socket.Url.AbsoluteUri);
        Assert.Contains("&language=en", socket.Url.AbsoluteUri);

        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 2);
        Assert.Equal("finalize", socket.Sent[1]);

        socket.Message(new JsonObject { ["type"] = "transcript", ["request_id"] = "request-1", ["text"] = "Hello ", ["is_final"] = true, ["duration"] = 0.5 });
        socket.Message(new JsonObject { ["type"] = "transcript", ["request_id"] = "request-1", ["text"] = "world", ["is_final"] = true, ["duration"] = 0.4 });
        socket.Message(new JsonObject { ["type"] = "flush_done", ["request_id"] = "request-1" });
        await Wait.UntilAsync(() => socket.Sent.Count >= 3);
        Assert.Equal("close", socket.Sent[2]);
        socket.Message(new JsonObject { ["type"] = "done", ["request_id"] = "request-1" });

        var finish = (await parts).Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hello world", finish.Text);
        Assert.Empty(finish.Segments!);
        Assert.Equal("en", finish.Language);
        Assert.Equal(0.9, finish.DurationInSeconds!.Value, 9);
    }

    [Theory]
    [InlineData("more than 100 keyterms")]
    [InlineData("a fractional turn timeout")]
    [UpstreamTest(Prefix + "rejects %s before creating a connection", Coverage = UpstreamCoverage.Covered, Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023")]
    public async Task Rejects_invalid_streaming_options_before_creating_a_connection(string description)
    {
        MockWebSocket.Reset();
        var handler = Handler();
        var streaming = description == "more than 100 keyterms"
            ? "{\"keyterms\":[" + string.Join(",", Enumerable.Range(0, 101).Select(i => "\"term" + i + "\"")) + "]}"
            : "{\"turnEndTimeoutMs\":800.5}";
        await Assert.ThrowsAsync<InvalidArgumentException>(() => Stream(Model("ink-2", handler), Audio(), Pcm24, providerOptions: @"{""cartesia"":{""streaming"":" + streaming + "}}"));
        Assert.Empty(handler.Calls);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends keyterms as repeated query parameters", Coverage = UpstreamCoverage.Covered, Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023")]
    public async Task Sends_keyterms_as_repeated_query_parameters()
    {
        MockWebSocket.Reset();
        await Stream(Model("ink-2"), Audio(), Pcm24, providerOptions: @"{""cartesia"":{""streaming"":{""keyterms"":[""Cartesia"",""Sonic 3""]}}}");
        var url = MockWebSocket.Instances[0].Url;
        Assert.Equal("/stt/turns/websocket", url.AbsolutePath);
        Assert.Equal(new[] { "Cartesia", "Sonic 3" }, Query(url)["keyterm"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends keyterms when turn detection is disabled", Coverage = UpstreamCoverage.Covered, Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023")]
    public async Task Sends_keyterms_when_turn_detection_is_disabled()
    {
        MockWebSocket.Reset();
        await Stream(Model("ink-2"), Audio(), Pcm24, providerOptions: @"{""cartesia"":{""streaming"":{""turnDetection"":false,""keyterms"":[""Cartesia""]}}}");
        var url = MockWebSocket.Instances[0].Url;
        Assert.Equal("/stt/websocket", url.AbsolutePath);
        Assert.Equal(new[] { "Cartesia" }, Query(url)["keyterm"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends turn thresholds with turn detection enabled", Coverage = UpstreamCoverage.Covered, Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023")]
    public async Task Sends_turn_thresholds_with_turn_detection_enabled()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model("ink-2"), Audio(), Pcm24, providerOptions: @"{""cartesia"":{""streaming"":{""turnStartThreshold"":0.5,""turnEagerEndThreshold"":0.4,""turnEndThreshold"":0.3,""turnEndTimeoutMs"":800}}}");
        var socket = MockWebSocket.Instances[0];
        var query = Query(socket.Url);
        Assert.Equal("0.5", query["turn_start_threshold"][0]);
        Assert.Equal("0.4", query["turn_eager_end_threshold"][0]);
        Assert.Equal("0.3", query["turn_end_threshold"][0]);
        Assert.Equal("800", query["turn_end_timeout_ms"][0]);

        socket.Open();
        var first = await First(result);
        Assert.Equal("stream-start", first.Type);
        Assert.Empty(first.Warnings!);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "omits turn thresholds and warns when turn detection is disabled", Coverage = UpstreamCoverage.Covered, Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023")]
    public async Task Omits_turn_thresholds_and_warns_when_turn_detection_is_disabled()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model("ink-2"), Audio(), Pcm24, providerOptions: @"{""cartesia"":{""streaming"":{""turnDetection"":false,""turnStartThreshold"":0.5,""turnEagerEndThreshold"":0.4,""turnEndThreshold"":0.3,""turnEndTimeoutMs"":800}}}");
        var socket = MockWebSocket.Instances[0];
        var query = Query(socket.Url);
        Assert.False(query.ContainsKey("turn_start_threshold"));
        Assert.False(query.ContainsKey("turn_eager_end_threshold"));
        Assert.False(query.ContainsKey("turn_end_threshold"));
        Assert.False(query.ContainsKey("turn_end_timeout_ms"));

        socket.Open();
        var first = await First(result);
        Assert.Equal("stream-start", first.Type);
        Assert.Equal(
            new[] { "turnStartThreshold", "turnEagerEndThreshold", "turnEndThreshold", "turnEndTimeoutMs" }.Select(name => "providerOptions.cartesia.streaming." + name),
            first.Warnings!.Select(warning => warning.Feature!));
        Assert.All(first.Warnings!, warning =>
        {
            Assert.Equal("unsupported", warning.Type);
            Assert.Equal("Turn thresholds require turnDetection to be enabled.", warning.Details);
        });
        result.Stream.Cancel();
    }

    [Theory]
    [InlineData("pcm_f16le")]
    [InlineData("pcm_f32le")]
    [InlineData("pcm_s32le")]
    [UpstreamTest(Prefix + "supports the %s streaming PCM encoding without a warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_the_streaming_PCM_encoding_without_a_warning(string encoding)
    {
        MockWebSocket.Reset();
        var result = await Stream(Model("ink-2"), Audio(), new AudioFormat("audio/pcm", 48000), providerOptions: @"{""cartesia"":{""streaming"":{""encoding"":""" + encoding + @"""}}}");
        var socket = MockWebSocket.Instances[0];
        Assert.Equal(encoding, Query(socket.Url)["encoding"][0]);

        // Widening generic audio/pcm is the intended use, so no warning.
        socket.Open();
        var first = await First(result);
        Assert.Equal("stream-start", first.Type);
        Assert.Empty(first.Warnings!);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "still validates inputAudioFormat.type when streaming.encoding is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Still_validates_inputAudioFormat_type_when_streaming_encoding_is_set()
    {
        MockWebSocket.Reset();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Stream(Model("ink-2"), Audio(), new AudioFormat("audio/mpeg", 44100), providerOptions: @"{""cartesia"":{""streaming"":{""encoding"":""pcm_s16le""}}}"));
        Assert.Contains("Unsupported Cartesia streaming audio format: audio/mpeg", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns when streaming.encoding contradicts the declared audio format", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_streaming_encoding_contradicts_the_declared_audio_format()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model("ink-2"), Audio(), new AudioFormat("audio/pcmu", 8000), providerOptions: @"{""cartesia"":{""streaming"":{""encoding"":""pcm_f32le""}}}");
        // The override wins on the wire, but the contradiction is surfaced.
        var socket = MockWebSocket.Instances[0];
        Assert.Equal("pcm_f32le", Query(socket.Url)["encoding"][0]);
        socket.Open();
        var first = await First(result);
        var warning = Assert.Single(first.Warnings!);
        Assert.Equal("other", warning.Type);
        Assert.Equal("providerOptions.cartesia.streaming.encoding 'pcm_f32le' contradicts inputAudioFormat.type 'audio/pcmu' (inferred 'pcm_mulaw'); sending 'pcm_f32le'.", warning.Message);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "warns when generic audio/pcm is overridden to a G.711 encoding", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_generic_audio_pcm_is_overridden_to_a_G711_encoding()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model("ink-2"), Audio(), new AudioFormat("audio/pcm", 8000), providerOptions: @"{""cartesia"":{""streaming"":{""encoding"":""pcm_mulaw""}}}");
        // Only linear PCM widths widen audio/pcm silently. G.711 contradicts it.
        var socket = MockWebSocket.Instances[0];
        Assert.Equal("pcm_mulaw", Query(socket.Url)["encoding"][0]);
        socket.Open();
        var first = await First(result);
        var warning = Assert.Single(first.Warnings!);
        Assert.Equal("other", warning.Type);
        Assert.Equal("providerOptions.cartesia.streaming.encoding 'pcm_mulaw' contradicts inputAudioFormat.type 'audio/pcm' (inferred 'pcm_s16le'); sending 'pcm_mulaw'.", warning.Message);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported model operations and Ink 2 languages", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_model_operations_and_Ink_2_languages()
    {
        MockWebSocket.Reset();
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Stream(Model("ink-whisper"), Audio(), Pcm16));

        var call = new TranscriptionModelCall(Array.Empty<byte>(), "audio/wav", OperationJson.Parse("{}"), new Dictionary<string, string>(), CancellationToken.None);
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => ((ITranscriptionCaller)Model("ink-2")).DoGenerateAsync(call, CancellationToken.None));

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Stream(Model("ink-2"), Audio(), Pcm16, providerOptions: @"{""cartesia"":{""language"":""es""}}"));
        Assert.Contains("currently supports English only", error.Message);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Fact]
    [UpstreamTest(Prefix + "closes the WebSocket and stops reading audio when cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Closes_the_WebSocket_and_stops_reading_audio_when_cancelled()
    {
        MockWebSocket.Reset();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var result = await Stream(Model("ink-2"), audio, Pcm16);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.FlushAsync();

        result.Stream.Cancel();
        await Wait.UntilAsync(() => audioCancelled);

        Assert.NotEmpty(socket.Closes);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "cancels input when the WebSocket constructor fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_input_when_the_WebSocket_constructor_fails()
    {
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var provider = CartesiaProvider.Create(
            new CartesiaOptions { ApiKey = "test-api-key", WebSocket = (_, _, _) => throw new InvalidOperationException("connection failed") },
            Handler());
        var result = await Stream(provider.TranscriptionModel("ink-2"), audio, Pcm16);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Collect(result));
        Assert.Equal("connection failed", error.Message);
        await Wait.UntilAsync(() => audioCancelled);
    }

    private static ParityHandler Handler()
    {
        return new ParityHandler(_ => ParityHandler.Json(@"{""token"":""test-access-token""}"));
    }

    private static ITranscriptionModel Model(string modelId, ParityHandler? handler = null)
    {
        var provider = CartesiaProvider.Create(new CartesiaOptions { ApiKey = "test-api-key", WebSocket = MockWebSocket.Factory }, handler ?? Handler());
        return provider.TranscriptionModel(modelId);
    }

    private static async Task<TranscriptionStreamStart> Stream(ITranscriptionModel model, AudioChunkStream audio, AudioFormat format, string? providerOptions = null)
    {
        var call = new TranscriptionStreamCall(audio, format, OperationJson.Parse(providerOptions ?? "{}"), new Dictionary<string, string?>(), null, CancellationToken.None);
        return (await ((ITranscriptionCaller)model).DoStreamAsync(call, CancellationToken.None))!;
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

    private static async Task<ModelStreamPart> First(TranscriptionStreamStart start)
    {
        await foreach (var part in start.Stream.ReadAsync())
        {
            return part;
        }

        throw new InvalidOperationException("The stream produced no part.");
    }

    private static Dictionary<string, List<string>> Query(Uri url)
    {
        var query = new Dictionary<string, List<string>>();
        foreach (var pair in url.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var index = pair.IndexOf('=');
            var key = Uri.UnescapeDataString(pair.Substring(0, index).Replace('+', ' '));
            if (!query.TryGetValue(key, out var list))
            {
                query[key] = list = new List<string>();
            }

            list.Add(Uri.UnescapeDataString(pair.Substring(index + 1).Replace('+', ' ')));
        }

        return query;
    }

    private static AudioChunkStream Audio()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });
        audio.Complete();
        return audio;
    }
}
