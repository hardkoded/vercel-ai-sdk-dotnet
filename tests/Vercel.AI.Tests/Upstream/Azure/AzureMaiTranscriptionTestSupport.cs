// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Azure;

/// <summary>A started MAI stream with the socket it opened.</summary>
internal sealed class StartedMaiStream
{
    public StartedMaiStream(TranscriptionStreamStart start, Task<List<ModelStreamPart>> parts, MockWebSocket socket)
    {
        Start = start;
        Parts = parts;
        Socket = socket;
    }

    public TranscriptionStreamStart Start { get; }

    public Task<List<ModelStreamPart>> Parts { get; }

    public MockWebSocket Socket { get; }

    public IEnumerable<string?> Types => Socket.Sent.Select(sent => JsonNode.Parse((string)sent)!["type"]?.GetValue<string>());
}

/// <summary>Helpers of <c>azure-mai-transcription-model.test.ts</c>.</summary>
internal static class AzureMaiTestSupport
{
    public const string Transcription = "conversation.item.input_audio_transcription";

    /// <summary>One second at 24 kHz.</summary>
    public static readonly byte[] Pcm = new byte[48_000];

    public static AzureOpenAIProvider Setup(Action<AzureOpenAIOptions>? configure = null, bool customWebSocket = true)
    {
        var options = new AzureOpenAIOptions { ResourceName = "test-resource", ApiKey = "test-key" };
        if (customWebSocket)
        {
            options.WebSocket = MockWebSocket.Factory;
        }

        configure?.Invoke(options);
        return AzureOpenAIProvider.Create(options);
    }

    public static async Task<StartedMaiStream> StartStream(
        Action<AzureOpenAIOptions>? settings = null,
        string modelId = "mai-transcribe-2-streaming",
        IEnumerable<object>? audio = null,
        int rate = 24000,
        string? providerOptions = null,
        bool includeRawChunks = false,
        CancellationToken cancellationToken = default,
        bool customWebSocket = true)
    {
        MockWebSocket.Reset();
        var chunks = new AudioChunkStream();
        foreach (var chunk in audio ?? new object[] { Pcm })
        {
            if (chunk is string base64)
            {
                chunks.Write(base64);
            }
            else
            {
                chunks.Write((byte[])chunk);
            }
        }

        chunks.Complete();
        var model = (ITranscriptionCaller)Setup(settings, customWebSocket).Transcription(modelId);
        var start = await model.DoStreamAsync(
            new TranscriptionStreamCall(chunks, new AudioFormat("audio/pcm", rate), JsonSerializer.Deserialize<JsonElement>(providerOptions ?? "{}"), new Dictionary<string, string>(), includeRawChunks, cancellationToken),
            cancellationToken);
        return new StartedMaiStream(start!, Collect(start!), MockWebSocket.Instances[^1]);
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

    public static async Task ConfigureSession(MockWebSocket socket)
    {
        socket.Open();
        socket.Message(new JsonObject { ["type"] = "session.created", ["session"] = new JsonObject { ["id"] = "sess_1" } });
        await Wait.UntilAsync(() => socket.Sent.Count >= 1);
        socket.Message(new JsonObject { ["type"] = "session.updated", ["session"] = new JsonObject { ["id"] = "sess_1" } });
        await Wait.FlushAsync();
        await Wait.FlushAsync();
    }

    public static JsonObject Event(string type, string? itemId = null, string? delta = null, string? transcript = null, string? intermediate = null)
    {
        var value = new JsonObject { ["type"] = type };
        if (itemId != null)
        {
            value["item_id"] = itemId;
        }

        if (delta != null)
        {
            value["delta"] = delta;
        }

        if (transcript != null)
        {
            value["transcript"] = transcript;
        }

        if (intermediate != null)
        {
            value["intermediate"] = intermediate;
        }

        return value;
    }
}
