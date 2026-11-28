// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.GoogleTranscriptionModel;

/// <summary>Port of <c>google-transcription-model.test.ts</c> &gt; <c>doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class DoStreamTests
{
    private const string Prefix =
        "packages/google/src/transcription/google-transcription-model.test.ts::doStream::";
    private static readonly AudioFormat Pcm16 = new AudioFormat("audio/pcm", 16000);

    [Fact]
    [UpstreamTest(
        Prefix + "streams transcription over the Gemini Live API WebSocket",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task streams_transcription_over_the_Gemini_Live_API_WebSocket()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });
        audio.Complete();
        var result = await StreamAsync(
            CreateModel(),
            audio,
            Pcm16,
            "{\"google\":{\"customVocabulary\":[\"Gemini\"],\"languageCodes\":[\"en-US\"]}}"
        );

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        Assert.Equal(
            "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent?key=test-api-key",
            ws.Url.ToString()
        );

        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count == 1);
        await Wait.FlushAsync();

        // Audio is gated on setupComplete: only the setup message was sent.
        Assert.Single(ws.Sent);
        JsonAssert.Equal(
            JsonNode.Parse((string)ws.Sent[0]),
            "{\"setup\":{\"model\":\"models/gemini-3.5-transcribe-live\",\"inputAudioTranscription\":{\"languageCodes\":[\"en-US\"],\"customVocabulary\":[\"Gemini\"]}}}"
        );

        ws.Message(new JsonObject { ["setupComplete"] = new JsonObject() });
        await Wait.UntilAsync(() => ws.Sent.Count == 3);

        // Audio chunk + audioStreamEnd.
        JsonAssert.Equal(
            JsonNode.Parse((string)ws.Sent[1]),
            "{\"realtimeInput\":{\"audio\":{\"data\":\"AQID\",\"mimeType\":\"audio/pcm;rate=16000\"}}}"
        );
        JsonAssert.Equal(
            JsonNode.Parse((string)ws.Sent[2]),
            "{\"realtimeInput\":{\"audioStreamEnd\":true}}"
        );

        ws.Message(Content("interimInputTranscription", new JsonObject { ["text"] = "hel" }));
        ws.Message(
            Content(
                "inputTranscription",
                new JsonObject { ["text"] = "hello ", ["languageCode"] = "en-US" }
            )
        );
        ws.Message(
            Content(
                "inputTranscription",
                new JsonObject { ["text"] = "world.", ["finished"] = true }
            )
        );
        ws.Message(
            new JsonObject
            {
                ["usageMetadata"] = new JsonObject { ["promptTokenCount"] = 7 },
                ["serverContent"] = new JsonObject
                {
                    ["turnComplete"] = true,
                    ["interactionStatus"] = "IDLE",
                },
            }
        );

        var all = await parts;
        Assert.Equal(
            new[]
            {
                "stream-start",
                "transcript-partial",
                "transcript-delta",
                "transcript-delta",
                "transcript-final",
                "finish",
            },
            all.Select(part => part.Type).ToArray()
        );
        Assert.Empty(all[0].Warnings!);
        Assert.Equal("google-segment-0", all[1].Id);
        Assert.Equal("hel", all[1].Text);
        Assert.Equal("google-segment-0", all[2].Id);
        Assert.Equal("hello ", all[2].Delta);
        Assert.Equal("google-segment-0", all[3].Id);
        Assert.Equal("world.", all[3].Delta);
        Assert.Equal("google-segment-0", all[4].Id);
        Assert.Equal("hello world.", all[4].Text);
        var finish = all[5];
        Assert.Equal("hello world.", finish.Text);
        Assert.Empty(finish.Segments!);
        Assert.Equal("en-US", finish.Language);
        Assert.Null(finish.DurationInSeconds);
        JsonAssert.Equal(finish.UsageValue!.Value, "{\"promptTokenCount\":7}");
        JsonAssert.Equal(
            finish.ProviderMetadata!.Value,
            "{\"google\":{\"usageMetadata\":{\"promptTokenCount\":7}}}"
        );
        Assert.Contains(1000, ws.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes the SMART transcription mode into the live setup",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task passes_the_SMART_transcription_mode_into_the_live_setup()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2 });
        audio.Complete();
        var result = await StreamAsync(
            CreateModel(),
            audio,
            Pcm16,
            "{\"google\":{\"mode\":\"SMART\"}}"
        );

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count == 1);

        JsonAssert.Equal(
            JsonNode.Parse((string)ws.Sent[0]),
            "{\"setup\":{\"model\":\"models/gemini-3.5-transcribe-live\",\"inputAudioTranscription\":{\"mode\":\"SMART\"}}}"
        );

        ws.Message(new JsonObject { ["setupComplete"] = new JsonObject() });
        await Wait.UntilAsync(() => ws.Sent.Count == 3);
        ws.Message(
            Content("inputTranscription", new JsonObject { ["text"] = "hi", ["finished"] = true })
        );
        ws.Message(Content("interactionStatus", "IDLE"));
        Assert.NotNull(await parts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts the pre-launch REQUIRES_ACTION interaction status as idle",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task accepts_the_pre_launch_REQUIRES_ACTION_interaction_status_as_idle()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2 });
        audio.Complete();
        var result = await StreamAsync(CreateModel(), audio, Pcm16, "{}");

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        ws.Message(new JsonObject { ["setupComplete"] = new JsonObject() });
        await Wait.UntilAsync(() => ws.Sent.Count == 3);

        ws.Message(
            Content("inputTranscription", new JsonObject { ["text"] = "hi", ["finished"] = true })
        );
        ws.Message(Content("interactionStatus", "REQUIRES_ACTION"));

        var last = (await parts).Last();
        Assert.Equal("finish", last.Type);
        Assert.Equal("hi", last.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "falls back to the latest interim partial when no final segment arrives",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task falls_back_to_the_latest_interim_partial_when_no_final_segment_arrives()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2 });
        audio.Complete();
        var result = await StreamAsync(CreateModel(), audio, Pcm16, "{}");

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        ws.Message(new JsonObject { ["setupComplete"] = new JsonObject() });
        await Wait.UntilAsync(() => ws.Sent.Count == 3);

        ws.Message(Content("interimInputTranscription", new JsonObject { ["text"] = "hello wor" }));
        ws.Message(
            Content("interimInputTranscription", new JsonObject { ["text"] = "hello world." })
        );
        ws.Message(Content("interactionStatus", "IDLE"));

        var all = await parts;
        var final = all[^2];
        Assert.Equal("transcript-final", final.Type);
        Assert.Equal("google-segment-0", final.Id);
        Assert.Equal("hello world.", final.Text);
        Assert.Equal("finish", all[^1].Type);
        Assert.Equal("hello world.", all[^1].Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "finishes with accumulated text when the server closes after audio ended",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task finishes_with_accumulated_text_when_the_server_closes_after_audio_ended()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2 });
        audio.Complete();
        var result = await StreamAsync(CreateModel(), audio, Pcm16, "{}");

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        ws.Message(new JsonObject { ["setupComplete"] = new JsonObject() });
        await Wait.UntilAsync(() => ws.Sent.Count == 3);

        ws.Message(Content("inputTranscription", new JsonObject { ["text"] = "partial words" }));
        ws.ServerClose(1000);

        var last = (await parts).Last();
        Assert.Equal("finish", last.Type);
        Assert.Equal("partial words", last.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors when the socket closes before the audio ended",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task errors_when_the_socket_closes_before_the_audio_ended()
    {
        var result = await StreamAsync(CreateModel(), new AudioChunkStream(), Pcm16, "{}");

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        ws.Message(new JsonObject { ["setupComplete"] = new JsonObject() });
        await Wait.FlushAsync();

        ws.ServerClose(1011, "internal error");
        var error = await Assert.ThrowsAsync<AiSdkException>(() => parts);
        Assert.Contains(
            "closed unexpectedly before finishing (code 1011, reason: internal error)",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "surfaces server error messages", Coverage = UpstreamCoverage.Covered)]
    public async Task surfaces_server_error_messages()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1 });
        audio.Complete();
        var result = await StreamAsync(CreateModel(), audio, Pcm16, "{}");

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        ws.Message(new JsonObject { ["setupComplete"] = new JsonObject() });
        await Wait.FlushAsync();
        ws.Message(
            new JsonObject { ["error"] = new JsonObject { ["message"] = "quota exceeded" } }
        );

        var error = await Assert.ThrowsAsync<AiSdkException>(() => parts);
        Assert.Contains("quota exceeded", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects streaming on unary model ids",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task rejects_streaming_on_unary_model_ids()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1 });
        audio.Complete();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            StreamAsync(CreateModel("gemini-3.5-transcribe"), audio, Pcm16, "{}")
        );
        Assert.Contains("does not support streaming transcription", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects non-16kHz PCM input audio",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task rejects_non_16kHz_PCM_input_audio()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1 });
        audio.Complete();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            StreamAsync(CreateModel(), audio, new AudioFormat("audio/pcm", 24000), "{}")
        );
        Assert.Contains("only supports 16kHz 16-bit PCM input audio", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits raw chunks when includeRawChunks is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task emits_raw_chunks_when_includeRawChunks_is_set()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1 });
        audio.Complete();
        var result = await StreamAsync(CreateModel(), audio, Pcm16, "{}", includeRawChunks: true);

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        ws.Message(new JsonObject { ["setupComplete"] = new JsonObject() });
        await Wait.UntilAsync(() => ws.Sent.Count == 3);
        ws.Message(
            Content("inputTranscription", new JsonObject { ["text"] = "ok", ["finished"] = true })
        );
        ws.Message(Content("interactionStatus", "IDLE"));

        Assert.True((await parts).Count(part => part.Type == "raw") > 0);
    }

    private static JsonObject Content(string key, JsonNode value)
    {
        return new JsonObject { ["serverContent"] = new JsonObject { [key] = value } };
    }

    private static ITranscriptionCaller CreateModel(string modelId = "gemini-3.5-transcribe-live")
    {
        MockWebSocket.Reset();
        var provider = GoogleProvider.Create(
            new GoogleOptions
            {
                ApiKey = "test-api-key",
                WebSocket = MockWebSocket.Factory,
                FinishGraceMs = 5000,
            }
        );
        return (ITranscriptionCaller)provider.TranscriptionModel(modelId);
    }

    private static async Task<TranscriptionStreamStart> StreamAsync(
        ITranscriptionCaller model,
        AudioChunkStream audio,
        AudioFormat format,
        string providerOptions,
        bool? includeRawChunks = null
    )
    {
        var call = new TranscriptionStreamCall(
            audio,
            format,
            OperationJson.Parse(providerOptions),
            new Dictionary<string, string?>(),
            includeRawChunks,
            CancellationToken.None,
            null
        );
        return (await model.DoStreamAsync(call, CancellationToken.None))!;
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
}
