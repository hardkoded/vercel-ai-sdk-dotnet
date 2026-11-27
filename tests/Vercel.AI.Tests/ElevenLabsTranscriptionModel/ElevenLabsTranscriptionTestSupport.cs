// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.ElevenLabsTranscriptionModel;

/// <summary>Helpers of <c>elevenlabs-transcription-model.test.ts</c>: the module-level <c>MockWebSocket</c>, <c>flush</c> and the <c>doStream</c> calls.</summary>
internal static class ElevenLabsTranscriptionTestSupport
{
    /// <summary>Maps to <c>createElevenLabs({ apiKey, webSocket: MockWebSocket })</c>.</summary>
    public static ElevenLabsProvider CreateProvider(
        Action<ElevenLabsOptions>? configure = null,
        bool mockWebSocket = true
    )
    {
        var options = new ElevenLabsOptions { ApiKey = "test-api-key" };
        if (mockWebSocket)
        {
            options.WebSocket = MockWebSocket.Factory;
        }

        configure?.Invoke(options);
        return ElevenLabsProvider.Create(options);
    }

    public static ITranscriptionCaller Transcription(
        string modelId = "scribe_v2_realtime",
        Action<ElevenLabsOptions>? configure = null,
        bool mockWebSocket = true
    ) => (ITranscriptionCaller)CreateProvider(configure, mockWebSocket).TranscriptionModel(modelId);

    public static AudioChunkStream Audio(params byte[][] chunks)
    {
        var stream = new AudioChunkStream();
        foreach (var chunk in chunks)
        {
            stream.Write(chunk);
        }

        stream.Complete();
        return stream;
    }

    public static Task<TranscriptionStreamStart?> DoStream(
        ITranscriptionCaller model,
        AudioChunkStream audio,
        AudioFormat? format = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string?>? headers = null
    )
    {
        using var options = JsonDocument.Parse(providerOptions);
        return model.DoStreamAsync(
            new TranscriptionStreamCall(
                audio,
                format ?? new AudioFormat("audio/pcm", 16000),
                options.RootElement.Clone(),
                headers ?? new Dictionary<string, string?>(),
                null,
                CancellationToken.None
            ),
            CancellationToken.None
        );
    }

    /// <summary>Maps to <c>convertReadableStreamToArray</c>, started before the socket is driven.</summary>
    public static Task<List<ModelStreamPart>> Collect(TranscriptionStreamStart start) =>
        Task.Run(async () =>
        {
            var parts = new List<ModelStreamPart>();
            await foreach (var part in start.Stream.ReadAsync())
            {
                parts.Add(part);
            }

            return parts;
        });

    public static MockWebSocket Socket(int index = 0) => MockWebSocket.Instances[index];

    /// <summary>Maps to <c>ws.message(value)</c>.</summary>
    public static void Receive(this MockWebSocket socket, string json) =>
        socket.Message(JsonNode.Parse(json)!);

    /// <summary>Opens the socket and sends <c>session_started</c>, then waits for the audio and the final commit to be sent. Maps to <c>ws.open(); ws.message(...); await flush()</c>.</summary>
    public static async Task StartSessionAsync(MockWebSocket socket, int expectedSends)
    {
        socket.Open();
        socket.Receive("""{"message_type":"session_started","session_id":"session-1"}""");
        await Wait.UntilAsync(() => socket.Sent.Count >= expectedSends);
    }

    /// <summary>The query parameters of a socket URL, last value wins like <c>Object.fromEntries(url.searchParams)</c>.</summary>
    public static Dictionary<string, string> Query(Uri url)
    {
        var result = new Dictionary<string, string>();
        foreach (var (name, value) in Pairs(url))
        {
            result[name] = value;
        }

        return result;
    }

    public static List<string> QueryAll(Uri url, string name) =>
        Pairs(url).Where(pair => pair.Name == name).Select(pair => pair.Value).ToList();

    /// <summary>The parts in the shape of the upstream <c>toEqual</c> objects. Unset members are omitted, like <c>undefined</c>.</summary>
    public static JsonArray Describe(IEnumerable<ModelStreamPart> parts)
    {
        var array = new JsonArray();
        foreach (var part in parts)
        {
            var node = new JsonObject { ["type"] = part.Type };
            if (part.Warnings is not null)
            {
                node["warnings"] = new JsonArray(
                    part.Warnings.Select(warning =>
                            (JsonNode?)
                                new JsonObject
                                {
                                    ["type"] = warning.Type,
                                    ["feature"] = warning.Feature,
                                    ["details"] = warning.Details,
                                }
                        )
                        .ToArray()
                );
            }

            Set(node, "id", part.Id);
            Set(node, "text", part.Text);
            if (part.StartSecond is { } start)
            {
                node["startSecond"] = start;
            }

            if (part.EndSecond is { } end)
            {
                node["endSecond"] = end;
            }

            if (part.Segments is not null)
            {
                node["segments"] = new JsonArray(
                    part.Segments.Select(segment =>
                            (JsonNode?)
                                new JsonObject
                                {
                                    ["text"] = segment.Text,
                                    ["startSecond"] = segment.StartSecond,
                                    ["endSecond"] = segment.EndSecond,
                                }
                        )
                        .ToArray()
                );
            }

            Set(node, "language", part.Language);
            if (part.DurationInSeconds is { } duration)
            {
                node["durationInSeconds"] = duration;
            }

            array.Add(node);
        }

        return array;
    }

    private static void Set(JsonObject node, string name, string? value)
    {
        if (value is not null)
        {
            node[name] = value;
        }
    }

    private static IEnumerable<(string Name, string Value)> Pairs(Uri url)
    {
        foreach (
            var pair in url
                .Query.TrimStart('?')
                .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
        )
        {
            var parts = pair.Split(new[] { '=' }, 2);
            yield return (
                Uri.UnescapeDataString(parts[0].Replace('+', ' ')),
                Uri.UnescapeDataString(parts[1].Replace('+', ' '))
            );
        }
    }
}
