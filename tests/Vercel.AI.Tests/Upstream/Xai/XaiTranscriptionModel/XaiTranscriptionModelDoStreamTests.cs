// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using static Vercel.AI.Tests.Upstream.Xai.XaiTranscriptionModel.XaiTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiTranscriptionModel;

/// <summary>Port of <c>xai-transcription-model.test.ts</c> &gt; <c>doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class XaiTranscriptionModelDoStreamTests
{
    private const string Prefix = "packages/xai/src/xai-transcription-model.test.ts::doStream::";

    [Fact]
    [UpstreamTest(Prefix + "should require channels when streaming multichannel audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_require_channels_when_streaming_multichannel_audio()
    {
        MockWebSocket.Reset();
        var model = StreamingModel();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            DoStream(model, providerOptions: """{"xai":{"multichannel":true}}"""));

        Assert.Equal("providerOptions", error.Parameter);
        Assert.Contains("providerOptions.xai.channels is required when providerOptions.xai.multichannel is true", error.Message);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream xAI STT over WebSocket", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_xAI_STT_over_WebSocket()
    {
        MockWebSocket.Reset();
        var testDate = new DateTimeOffset(DateTime.UnixEpoch, TimeSpan.Zero);
        var model = StreamingModel(clock: () => testDate);

        var result = await DoStream(
            model,
            providerOptions: """
                {"xai":{"language":"en","diarize":true,"keyterm":["AI SDK","Grok"],"streaming":{"interimResults":true,"endpointing":500,"smartTurn":0.7,"smartTurnTimeout":3000}}}
                """);

        var parts = Collect(result!);
        var socket = MockWebSocket.Instances[0];
        Assert.Equal(
            "wss://api.x.ai/v1/stt?sample_rate=16000&language=en&diarize=true&keyterm=AI+SDK&keyterm=Grok&encoding=pcm&interim_results=true&endpointing=500&smart_turn=0.7&smart_turn_timeout=3000",
            socket.Url.AbsoluteUri);
        Assert.Equal("Bearer test-api-key", socket.Headers!["Authorization"]);
        Assert.Empty(socket.Sent);

        socket.Message(Json("""{"type":"transcript.created"}"""));
        await Wait.UntilAsync(() => socket.Sent.Count >= 2);

        Assert.Equal(new byte[] { 1, 2, 3 }, (byte[])socket.Sent[0]);
        Assert.Equal("audio.done", (string?)Json((string)socket.Sent[1])["type"]);

        socket.Message(Json("""{"type":"transcript.partial","text":"Hel","is_final":false,"speech_final":false,"start":0,"duration":0.5}"""));
        socket.Message(Json("""{"type":"transcript.partial","text":"Hello","is_final":true,"speech_final":true,"start":0,"duration":1}"""));
        socket.Message(Json("""{"type":"transcript.done","text":"Hello","duration":1}"""));

        var all = await parts;
        Assert.Equal(new[] { "stream-start", "transcript-partial", "transcript-final", "finish" }, all.Select(part => part.Type));
        Assert.Empty(all[0].Warnings!);
        Assert.Null(all[1].Id);
        Assert.Equal("Hel", all[1].Text);
        Assert.Equal(0, all[1].StartSecond);
        Assert.Equal(0.5, all[1].DurationInSeconds);
        Assert.Null(all[1].ChannelIndex);
        Assert.Null(all[2].Id);
        Assert.Equal("Hello", all[2].Text);
        Assert.Equal(0, all[2].StartSecond);
        Assert.Equal(1, all[2].EndSecond);
        Assert.Null(all[2].ChannelIndex);
        Assert.Equal("Hello", all[3].Text);
        Assert.Empty(all[3].Segments!);
        Assert.Equal("en", all[3].Language);
        Assert.Equal(1, all[3].DurationInSeconds);
        Assert.Equal(testDate.UtcDateTime, result!.Response!.Timestamp);
        Assert.Equal(string.Empty, result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send the model id and vad_threshold as query params", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_the_model_id_and_vad_threshold_as_query_params()
    {
        MockWebSocket.Reset();
        var model = StreamingModel("grok-voice-transcribe-2.0");

        var result = await DoStream(model, providerOptions: """{"xai":{"vadThreshold":0.3}}""");

        result!.Stream.Cancel();
        Assert.Equal(
            "wss://api.x.ai/v1/stt?model=grok-voice-transcribe-2.0&sample_rate=16000&vad_threshold=0.3&encoding=pcm",
            MockWebSocket.Instances[0].Url.AbsoluteUri);
        Assert.Equal("grok-voice-transcribe-2.0", result.Response!.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send a model query param when no model id is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_send_a_model_query_param_when_no_model_id_is_set()
    {
        MockWebSocket.Reset();

        var result = await DoStream(StreamingModel());

        result!.Stream.Cancel();
        Assert.False(HasParameter(MockWebSocket.Instances[0].Url, "model"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should map audio/opus input to opus encoding without a warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_audio_opus_input_to_opus_encoding_without_a_warning()
    {
        MockWebSocket.Reset();

        var result = await DoStream(StreamingModel(), format: new AudioFormat("audio/opus", 48000));

        var parts = Collect(result!);
        var socket = MockWebSocket.Instances[0];
        Assert.Equal("opus", Values(socket.Url, "encoding").Single());
        Assert.Equal("48000", Values(socket.Url, "sample_rate").Single());

        socket.Message(Json("""{"type":"transcript.created"}"""));
        await Wait.FlushAsync();
        socket.Message(Json("""{"type":"transcript.done","text":"Hello"}"""));

        var all = await parts;
        Assert.Equal("stream-start", all[0].Type);
        Assert.Empty(all[0].Warnings!);
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept opus as an explicit audioFormat for streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_accept_opus_as_an_explicit_audioFormat_for_streaming()
    {
        MockWebSocket.Reset();

        var result = await DoStream(
            StreamingModel(),
            format: new AudioFormat("audio/pcm", 48000),
            providerOptions: """{"xai":{"audioFormat":"opus","sampleRate":16000}}""");

        result!.Stream.Cancel();
        var url = MockWebSocket.Instances[0].Url;
        Assert.Equal("opus", Values(url, "encoding").Single());
        Assert.Equal("16000", Values(url, "sample_rate").Single());
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward multichannel opus options to xAI", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_multichannel_opus_options_to_xAI()
    {
        MockWebSocket.Reset();

        var result = await DoStream(
            StreamingModel(),
            format: new AudioFormat("audio/opus", 48000),
            providerOptions: """{"xai":{"multichannel":true,"channels":2}}""");

        result!.Stream.Cancel();
        var url = MockWebSocket.Instances[0].Url;
        Assert.Equal("opus", Values(url, "encoding").Single());
        Assert.Equal("true", Values(url, "multichannel").Single());
        Assert.Equal("2", Values(url, "channels").Single());
        Assert.Equal("48000", Values(url, "sample_rate").Single());
    }

    [Fact]
    [UpstreamTest(Prefix + "should strip undefined header values before the WebSocket constructor", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_strip_undefined_header_values_before_the_WebSocket_constructor()
    {
        MockWebSocket.Reset();

        var result = await DoStream(
            StreamingModel(),
            headers: new Dictionary<string, string?> { ["Custom-Header"] = "custom-value", ["X-Unset"] = null });

        result!.Stream.Cancel();
        var headers = MockWebSocket.Instances[0].Headers!;
        Assert.DoesNotContain("X-Unset", headers.Keys);
        Assert.Equal("Bearer test-api-key", headers["Authorization"]);
        Assert.Equal("custom-value", headers["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the audio stream when the WebSocket constructor throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_cancel_the_audio_stream_when_the_WebSocket_constructor_throws()
    {
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var model = StreamingModel(configure: options => options.WebSocket = (_, _, _) => throw new InvalidOperationException("constructor failed"));

        var result = await DoStream(model, audio);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Collect(result!));
        Assert.Equal("constructor failed", error.Message);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the audio stream when an audio send throws mid-stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_cancel_the_audio_stream_when_an_audio_send_throws_mid_stream()
    {
        MockWebSocket.Reset();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        audio.Write(new byte[] { 1, 2, 3 });
        var model = StreamingModel();

        var result = await DoStream(model, audio);

        var parts = Collect(result!);
        var socket = MockWebSocket.Instances[0];
        socket.BinarySendHook = _ => throw new InvalidOperationException("send failed");

        socket.Message(Json("""{"type":"transcript.created"}"""));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => parts);
        Assert.Equal("send failed", error.Message);
        Assert.True(audioCancelled);
    }

    // Live API behavior: finals are re-sent (`speech_final` false then true) and `transcript.done` carries an empty `text`.
    [Fact]
    [UpstreamTest(Prefix + "should emit one transcript-final per utterance and reconstruct the finish text when transcript.done is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_emit_one_transcript_final_per_utterance_and_reconstruct_the_finish_text_when_transcript_done_is_empty()
    {
        var (socket, parts) = await Start();

        socket.Message(Json("""{"type":"transcript.partial","text":"Hello wor","is_final":false,"speech_final":false,"start":0,"duration":0.8}"""));
        socket.Message(Json("""{"type":"transcript.partial","text":"Hello world.","is_final":true,"speech_final":false,"start":0,"duration":1}"""));
        socket.Message(Json("""{"type":"transcript.partial","text":"Hello world.","is_final":true,"speech_final":true,"start":0,"duration":1}"""));
        socket.Message(Json("""{"type":"transcript.done","text":"","duration":1}"""));

        var all = await parts;
        var final = Assert.Single(all, part => part.Type == "transcript-final");
        Assert.Null(final.Id);
        Assert.Equal("Hello world.", final.Text);
        Assert.Equal(0, final.StartSecond);
        Assert.Equal(1, final.EndSecond);
        Assert.Null(final.ChannelIndex);

        // The speech_final:false re-send surfaces as a partial.
        Assert.Equal(2, all.Count(part => part.Type == "transcript-partial"));
        var finish = all[^1];
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hello world.", finish.Text);
        Assert.Empty(finish.Segments!);
        Assert.Null(finish.Language);
        Assert.Equal(1, finish.DurationInSeconds);
    }

    // Live API behavior: `is_final` fragments get merged and re-punctuated by the eventual `speech_final` event, so they are not stable finals.
    [Fact]
    [UpstreamTest(Prefix + "should treat is_final fragments as partials and use the speech_final text for finish", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_treat_is_final_fragments_as_partials_and_use_the_speech_final_text_for_finish()
    {
        var (socket, parts) = await Start();

        socket.Message(Json("""{"type":"transcript.partial","text":"No","is_final":true,"speech_final":false,"start":0,"duration":0.5}"""));
        socket.Message(Json("""{"type":"transcript.partial","text":", I'm not","is_final":true,"speech_final":false,"start":0.5,"duration":0.7}"""));
        socket.Message(Json("""{"type":"transcript.partial","text":"No, I'm not.","is_final":true,"speech_final":true,"start":0,"duration":1.2}"""));
        socket.Message(Json("""{"type":"transcript.done","text":"","duration":1.2}"""));

        var all = await parts;
        Assert.Equal(
            new[] { "stream-start", "transcript-partial", "transcript-partial", "transcript-final", "finish" },
            all.Select(part => part.Type));
        Assert.Equal("No, I'm not.", all[^1].Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should fall back to the latest pending text when no speech_final arrived before transcript.done", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_fall_back_to_the_latest_pending_text_when_no_speech_final_arrived_before_transcript_done()
    {
        var (socket, parts) = await Start();

        socket.Message(Json("""{"type":"transcript.partial","text":"Hello wor","is_final":false,"speech_final":false}"""));
        socket.Message(Json("""{"type":"transcript.partial","text":"Hello world","is_final":true,"speech_final":false}"""));
        socket.Message(Json("""{"type":"transcript.done","text":"","duration":1}"""));

        var finish = (await parts)[^1];
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hello world", finish.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should join finalized utterances per channel when transcript.done is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_join_finalized_utterances_per_channel_when_transcript_done_is_empty()
    {
        var (socket, parts) = await Start();

        socket.Message(Json("""{"type":"transcript.partial","text":"First utterance.","is_final":true,"speech_final":true,"start":0,"duration":1}"""));
        socket.Message(Json("""{"type":"transcript.partial","text":"Second utterance.","is_final":true,"speech_final":true,"start":1,"duration":1}"""));
        socket.Message(Json("""{"type":"transcript.done","text":"","duration":2}"""));

        var finish = (await parts)[^1];
        Assert.Equal("finish", finish.Type);
        Assert.Equal("First utterance. Second utterance.", finish.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should error the stream with the server message on error events", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_error_the_stream_with_the_server_message_on_error_events()
    {
        var (socket, parts) = await Start();

        socket.Message(Json("""{"type":"error","message":"invalid sample_rate"}"""));

        var error = await Assert.ThrowsAsync<AiSdkException>(() => parts);
        Assert.Equal("invalid sample_rate", error.Message);
        Assert.NotEmpty(socket.Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "should close the WebSocket and stop reading audio when the stream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_close_the_WebSocket_and_stop_reading_audio_when_the_stream_is_cancelled()
    {
        MockWebSocket.Reset();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);

        var result = await DoStream(StreamingModel(), audio);

        var socket = MockWebSocket.Instances[0];
        socket.Message(Json("""{"type":"transcript.created"}"""));
        await Wait.FlushAsync();

        result!.Stream.Cancel();
        await Wait.FlushAsync();

        Assert.NotEmpty(socket.Closes);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn on unrecognized inputAudioFormat types", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_on_unrecognized_inputAudioFormat_types()
    {
        MockWebSocket.Reset();

        var result = await DoStream(StreamingModel(), format: new AudioFormat("audio/wav"));

        var parts = Collect(result!);
        var socket = MockWebSocket.Instances[0];
        socket.Message(Json("""{"type":"transcript.created"}"""));
        await Wait.FlushAsync();
        socket.Message(Json("""{"type":"transcript.done","text":"Hello"}"""));

        var all = await parts;
        Assert.Equal("stream-start", all[0].Type);
        var warning = Assert.Single(all[0].Warnings!);
        Assert.Equal("other", warning.Type);
        Assert.Contains("Unrecognized inputAudioFormat.type \"audio/wav\"", warning.Message);
    }

    private static async Task<(MockWebSocket Socket, Task<List<ModelStreamPart>> Parts)> Start()
    {
        MockWebSocket.Reset();
        var result = await DoStream(StreamingModel());
        var parts = Collect(result!);
        var socket = MockWebSocket.Instances[0];
        socket.Message(Json("""{"type":"transcript.created"}"""));
        await Wait.FlushAsync();
        return (socket, parts);
    }
}
