// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.ElevenLabsSpeechStream;

/// <summary>
/// Maps to <c>textParts</c>, <c>createModel</c>, <c>lastSocket</c> and the module-level <c>MockWebSocket</c> of
/// <c>elevenlabs-speech-stream.test.ts</c>. Each test owns its sockets, so tests can run in parallel.
/// </summary>
internal sealed class SpeechStreamHarness
{
    private readonly List<MockWebSocket> _sockets = new List<MockWebSocket>();

    public int SocketCount
    {
        get
        {
            lock (_sockets)
            {
                return _sockets.Count;
            }
        }
    }

    public MockWebSocket LastSocket
    {
        get
        {
            lock (_sockets)
            {
                return _sockets.Count > 0
                    ? _sockets[_sockets.Count - 1]
                    : throw new InvalidOperationException("no socket");
            }
        }
    }

    public ElevenLabsProvider.ElevenLabsSpeechModel CreateModel(
        string modelId = "eleven_flash_v2_5",
        int keepaliveIntervalMs = 15_000
    )
    {
        var provider = ElevenLabsProvider.Create(
            new ElevenLabsOptions
            {
                ApiKey = "test-api-key",
                KeepaliveIntervalMs = keepaliveIntervalMs,
                WebSocket = (url, protocols, headers) =>
                {
                    var socket = new MockWebSocket(url, protocols, headers);
                    lock (_sockets)
                    {
                        _sockets.Add(socket);
                    }

                    return socket;
                },
            }
        );
        return (ElevenLabsProvider.ElevenLabsSpeechModel)provider.SpeechModel(modelId);
    }

    public static TrackedText TextParts(params string[] texts) =>
        new TrackedText(texts.Select(text => new SpeechTextPart(text)));

    public static Task<SpeechModelStreamResult> DoStream(
        ElevenLabsProvider.ElevenLabsSpeechModel model,
        TrackedText text,
        string? voice = null,
        SpeechAudioFormat? outputFormat = null,
        string? instructions = null,
        double? speed = null,
        string? language = null,
        string? providerOptions = null,
        bool? includeRawChunks = null,
        CancellationToken abortSignal = default
    )
    {
        return model.DoStreamAsync(
            new SpeechModelStreamCall(
                new SpeechTextStream(text),
                voice,
                outputFormat,
                instructions,
                speed,
                language,
                JsonDocument.Parse(providerOptions ?? "{}").RootElement.Clone(),
                new Dictionary<string, string>(),
                includeRawChunks,
                abortSignal
            ),
            abortSignal
        );
    }

    public static async Task<List<SpeechModelStreamPart>> ReadAllAsync(
        IAsyncEnumerable<SpeechModelStreamPart> stream
    )
    {
        var parts = new List<SpeechModelStreamPart>();
        await foreach (var part in stream)
        {
            parts.Add(part);
        }

        return parts;
    }

    /// <summary>Maps to <c>sentMessages()</c>.</summary>
    public static List<JsonNode?> SentMessages(MockWebSocket socket) =>
        socket.Sent.Select(sent => JsonNode.Parse((string)sent)).ToList();

    public static JsonObject Describe(SpeechModelStreamPart part)
    {
        var result = new JsonObject { ["type"] = part.Type };
        if (part.Warnings != null)
        {
            result["warnings"] = new JsonArray(
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

        if (part.AudioBase64 != null)
        {
            result["audio"] = part.AudioBase64;
        }

        if (part.AudioFormat != null)
        {
            var format = new JsonObject { ["type"] = part.AudioFormat.Type };
            if (part.AudioFormat.Rate != null)
            {
                format["rate"] = part.AudioFormat.Rate;
            }

            result["audioFormat"] = format;
        }

        if (part.ProviderMetadata != null)
        {
            result["providerMetadata"] = JsonNode.Parse(part.ProviderMetadata.Value.GetRawText());
        }

        if (part.Usage != null)
        {
            result["usage"] = new JsonObject { ["inputCharacters"] = part.Usage.InputCharacters };
        }

        if (part.RawValue != null)
        {
            result["rawValue"] = JsonNode.Parse(part.RawValue.Value.GetRawText());
        }

        return result;
    }

    public static void AssertJson(string expected, IEnumerable<JsonNode?> actual)
    {
        var actualArray = new JsonArray(actual.Select(node => node?.DeepClone()).ToArray());
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), actualArray),
            "Expected " + expected + " but was " + actualArray.ToJsonString()
        );
    }

    public static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), actual),
            "Expected " + expected + " but was " + actual?.ToJsonString()
        );
}

/// <summary>
/// A text stream the test can watch. It yields its parts, then stays open until <see cref="Release"/> (or forever when
/// <c>holdOpen</c> is set) and ends when its reader cancels. Maps to <c>convertArrayToReadableStream</c> and
/// <c>new ReadableStream({ cancel })</c>.
/// </summary>
internal sealed class TrackedText : IAsyncEnumerable<SpeechTextPart>
{
    private readonly List<SpeechTextPart> _parts;
    private readonly TaskCompletionSource<bool>? _hold;

    public TrackedText(IEnumerable<SpeechTextPart> parts, bool holdOpen = false)
    {
        _parts = parts.ToList();
        _hold = holdOpen ? new TaskCompletionSource<bool>() : null;
    }

    public bool Disposed { get; private set; }

    public bool Cancelled { get; private set; }

    public void Release() => _hold?.TrySetResult(true);

    public IAsyncEnumerator<SpeechTextPart> GetAsyncEnumerator(
        CancellationToken cancellationToken = default
    ) => new Enumerator(this, cancellationToken);

    private sealed class Enumerator(TrackedText owner, CancellationToken cancellationToken)
        : IAsyncEnumerator<SpeechTextPart>
    {
        private int _index = -1;

        public SpeechTextPart Current { get; private set; } = null!;

        public async ValueTask<bool> MoveNextAsync()
        {
            _index++;
            if (_index < owner._parts.Count)
            {
                Current = owner._parts[_index];
                return true;
            }

            if (owner._hold != null)
            {
                var cancelled = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
                {
                    var finished = await Task.WhenAny(owner._hold.Task, cancelled.Task);
                    if (finished == cancelled.Task)
                    {
                        owner.Cancelled = true;
                        return false;
                    }
                }
            }

            return false;
        }

        public ValueTask DisposeAsync()
        {
            owner.Disposed = true;
            return default;
        }
    }
}
