// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using TranscriptionModel = Vercel.AI.OpenAI.OpenAITranscriptionModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAITranscriptionModel;

/// <summary>Helpers of the <c>doStream</c> tests of <c>openai-transcription-model.test.ts</c>.</summary>
internal static class OpenAITranscriptionTestSupport
{
    public const string Transcription = "conversation.item.input_audio_transcription";

    /// <summary>A model configured like the upstream tests: <c>https://api.openai.com/v1</c> and a mock WebSocket.</summary>
    public static TranscriptionModel Model(
        string modelId,
        Func<Dictionary<string, string?>>? headers = null,
        WebSocketFactory? webSocket = null,
        Func<DateTimeOffset>? clock = null)
    {
        return new TranscriptionModel(
            modelId,
            "test-provider",
            new ProviderHttp(new HttpClient(new OpenAICapture())),
            () => new Uri("https://api.openai.com/v1/audio/transcriptions"),
            () => new Uri("https://api.openai.com/v1/realtime?intent=transcription"),
            headers ?? (() => new Dictionary<string, string?> { ["Authorization"] = "Bearer test-api-key" }),
            webSocket ?? MockWebSocket.Factory,
            clock);
    }

    /// <summary>One chunk and then the end of the audio, like <c>convertArrayToReadableStream([new Uint8Array([1, 2, 3])])</c>.</summary>
    public static AudioChunkStream Audio()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });
        audio.Complete();
        return audio;
    }

    public static async Task<(TranscriptionStreamStart Start, Task<List<ModelStreamPart>> Parts, MockWebSocket Socket)> StartAsync(
        TranscriptionModel model,
        AudioChunkStream? audio = null,
        string? providerOptions = null,
        IReadOnlyDictionary<string, string?>? headers = null)
    {
        var call = new TranscriptionStreamCall(
            audio ?? Audio(),
            new AudioFormat("audio/pcm", 24000),
            JsonSerializer.Deserialize<JsonElement>(providerOptions ?? "{}"),
            headers ?? new Dictionary<string, string?>(),
            null,
            CancellationToken.None);
        var start = await model.DoStreamAsync(call, CancellationToken.None);
        return (start!, Collect(start!), MockWebSocket.Instances[^1]);
    }

    public static async Task<List<ModelStreamPart>> Collect(TranscriptionStreamStart start)
    {
        var parts = new List<ModelStreamPart>();
        await foreach (var part in start.Stream.ReadAsync())
        {
            parts.Add(part);
        }

        return parts;
    }

    public static JsonObject Completed(string transcript)
    {
        return new JsonObject { ["type"] = Transcription + ".completed", ["item_id"] = "item-1", ["transcript"] = transcript };
    }

    public static JsonNode? Sent(MockWebSocket socket, int index)
    {
        return JsonNode.Parse((string)socket.Sent[index]);
    }
}
