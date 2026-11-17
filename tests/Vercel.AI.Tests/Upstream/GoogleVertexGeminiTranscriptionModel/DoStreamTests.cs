// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.GoogleVertexGeminiTranscriptionModel;

/// <summary>Port of <c>google-vertex-gemini-transcription-model.test.ts</c> &gt; <c>doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class DoStreamTests
{
    private const string Prefix =
        "packages/google-vertex/src/gemini-transcription/google-vertex-gemini-transcription-model.test.ts::doStream::";
    private static readonly AudioFormat Pcm16 = new AudioFormat("audio/pcm", 16000);

    [Fact]
    [UpstreamTest(
        Prefix + "streams transcription over the Vertex Live API WebSocket",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_transcription_over_the_Vertex_Live_API_WebSocket()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });
        audio.Complete();
        var result = await StreamAsync(
            CreateModel(),
            audio,
            Pcm16,
            "{\"googleVertex\":{\"customVocabulary\":[\"Gemini\"],\"mode\":\"SMART\"}}"
        );

        var parts = Collect(result);
        var ws = MockWebSocket.Instances[0];
        Assert.Equal(
            "wss://us-central1-aiplatform.googleapis.com/ws/google.cloud.aiplatform.v1.LlmBidiService/BidiGenerateContent",
            ws.Url.ToString()
        );
        Assert.Equal("Bearer test-oauth-token", ws.Headers!["Authorization"]);

        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count == 1);
        await Wait.FlushAsync();

        // Audio is gated on setupComplete: only the setup message was sent.
        Assert.Single(ws.Sent);
        JsonAssert.Equal(
            JsonNode.Parse((string)ws.Sent[0]),
            "{\"setup\":{\"model\":\"projects/test-project/locations/us-central1/publishers/google/models/gemini-3.5-transcribe-live\",\"inputAudioTranscription\":{\"customVocabulary\":[\"Gemini\"],\"mode\":\"SMART\"}}}"
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

        ws.Message(
            new JsonObject
            {
                ["serverContent"] = new JsonObject
                {
                    ["interimInputTranscription"] = new JsonObject { ["text"] = "hel" },
                },
            }
        );
        ws.Message(
            new JsonObject
            {
                ["serverContent"] = new JsonObject
                {
                    ["inputTranscription"] = new JsonObject
                    {
                        ["text"] = "hello world.",
                        ["finished"] = true,
                    },
                },
            }
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
                "transcript-final",
                "finish",
            },
            all.Select(part => part.Type).ToArray()
        );
        Assert.Empty(all[0].Warnings!);
        Assert.Equal("google-segment-0", all[1].Id);
        Assert.Equal("hel", all[1].Text);
        Assert.Equal("google-segment-0", all[2].Id);
        Assert.Equal("hello world.", all[2].Delta);
        Assert.Equal("google-segment-0", all[3].Id);
        Assert.Equal("hello world.", all[3].Text);
        var finish = all[4];
        Assert.Equal("hello world.", finish.Text);
        Assert.Empty(finish.Segments!);
        Assert.Null(finish.Language);
        Assert.Null(finish.DurationInSeconds);
        Assert.Equal(7, finish.Usage!.InputTokens);
        JsonAssert.Equal(
            finish.ProviderMetadata!.Value,
            "{\"google\":{\"usageMetadata\":{\"promptTokenCount\":7}}}"
        );
        Assert.Contains(1000, ws.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "falls back to the latest interim partial when no final segment arrives",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Falls_back_to_the_latest_interim_partial_when_no_final_segment_arrives()
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
            new JsonObject
            {
                ["serverContent"] = new JsonObject
                {
                    ["interimInputTranscription"] = new JsonObject { ["text"] = "hello world." },
                },
            }
        );
        ws.Message(
            new JsonObject { ["serverContent"] = new JsonObject { ["interactionStatus"] = "IDLE" } }
        );

        var finish = (await parts).Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal("hello world.", finish.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors when the socket closes before the audio ended",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Errors_when_the_socket_closes_before_the_audio_ended()
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
    [UpstreamTest(
        Prefix + "rejects streaming on unary model ids",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_streaming_on_unary_model_ids()
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
    public async Task Rejects_non_16kHz_PCM_input_audio()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1 });
        audio.Complete();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            StreamAsync(CreateModel(), audio, new AudioFormat("audio/pcm", 24000), "{}")
        );
        Assert.Contains("only supports 16kHz 16-bit PCM input audio", error.Message);
    }

    private static ITranscriptionCaller CreateModel(string modelId = "gemini-3.5-transcribe-live")
    {
        MockWebSocket.Reset();
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "test-project",
                Region = "us-central1",
                ApiKey = "test-oauth-token",
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
        string providerOptions
    )
    {
        var call = new TranscriptionStreamCall(
            audio,
            format,
            OperationJson.Parse(providerOptions),
            new Dictionary<string, string?>(),
            null,
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
