// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock;

/// <summary>Encodes AWS event-stream frames, like <c>EventStreamCodec.encode</c> in upstream's tests.</summary>
internal static class EventStreamFrames
{
    public static byte[] Event(string eventType, string data)
    {
        return Encode(
            new[] { (":message-type", "event"), (":event-type", eventType) },
            data);
    }

    public static byte[] Exception(string exceptionType, string data)
    {
        return Encode(
            new[] { (":message-type", "exception"), (":exception-type", exceptionType), (":content-type", "application/json") },
            data);
    }

    /// <summary>One frame per line; the event type is the line's only top-level key and the payload is its value.</summary>
    public static IReadOnlyList<byte[]> FromChunkLines(IEnumerable<string> lines)
    {
        var frames = new List<byte[]>();
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                continue;
            }

            var node = JsonNode.Parse(line)!.AsObject();
            var property = node.Single();
            frames.Add(Event(property.Key, property.Value?.ToJsonString() ?? "null"));
        }

        return frames;
    }

    public static byte[] Encode((string Name, string Value)[] headers, string data)
    {
        var headerBytes = new List<byte>();
        foreach (var (name, value) in headers)
        {
            var nameBytes = Encoding.UTF8.GetBytes(name);
            var valueBytes = Encoding.UTF8.GetBytes(value);
            headerBytes.Add((byte)nameBytes.Length);
            headerBytes.AddRange(nameBytes);
            headerBytes.Add(7);
            headerBytes.Add((byte)(valueBytes.Length >> 8));
            headerBytes.Add((byte)valueBytes.Length);
            headerBytes.AddRange(valueBytes);
        }

        var payload = Encoding.UTF8.GetBytes(data);
        var total = 12 + headerBytes.Count + payload.Length + 4;
        var frame = new List<byte>();
        frame.AddRange(UInt32(total));
        frame.AddRange(UInt32(headerBytes.Count));
        frame.AddRange(UInt32(Vercel.AI.AmazonBedrock.AmazonBedrockEventStreamDecoder.Crc32(frame.ToArray(), 0, 8)));
        frame.AddRange(headerBytes);
        frame.AddRange(payload);
        frame.AddRange(UInt32(Vercel.AI.AmazonBedrock.AmazonBedrockEventStreamDecoder.Crc32(frame.ToArray(), 0, frame.Count)));
        return frame.ToArray();
    }

    private static byte[] UInt32(long value)
    {
        return new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
    }
}

/// <summary>Records one request and replies with event-stream frames, split across reads like a network stream.</summary>
internal sealed class UpstreamEventStreamHandler : HttpMessageHandler
{
    private readonly IReadOnlyList<byte[]> _chunks;
    private readonly IReadOnlyDictionary<string, string>? _headers;

    public UpstreamEventStreamHandler(IReadOnlyList<byte[]> chunks, IReadOnlyDictionary<string, string>? headers = null)
    {
        _chunks = chunks;
        _headers = headers;
    }

    public int Calls { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    public JsonElement BodyJson
    {
        get
        {
            using var document = JsonDocument.Parse(Body);
            return document.RootElement.Clone();
        }
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        Headers.Clear();
        foreach (var header in request.Headers)
        {
            Headers[header.Key] = string.Join(",", header.Value);
        }

        var stream = new MemoryStream();
        foreach (var chunk in _chunks)
        {
            stream.Write(chunk, 0, chunk.Length);
        }

        stream.Position = 0;
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnknownLengthContent(stream) };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.amazon.eventstream");
        response.Headers.TryAddWithoutValidation("cache-control", "no-cache");
        response.Headers.TryAddWithoutValidation("connection", "keep-alive");
        if (_headers != null)
        {
            foreach (var header in _headers)
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return response;
    }
}

internal static class BedrockStreamSupport
{
    public const string ModelId = "anthropic.claude-3-haiku-20240307-v1:0";

    public const string Start = "stream-start warnings=0";

    public const string Meta = "response-metadata id=undefined modelId=anthropic.claude-3-haiku-20240307-v1:0 timestamp=undefined";

    public const string NoUsage = "in(undefined,undefined,undefined,undefined) out(undefined,undefined,undefined) raw=undefined";

    public const string JsonToolMeta = " meta={\"amazonBedrock\":{\"isJsonResponseFromTool\":true,\"stopSequence\":null},\"bedrock\":{\"isJsonResponseFromTool\":true,\"stopSequence\":null}}";

    public static readonly ModelMessage[] TestPrompt =
    {
        new SystemModelMessage("System Prompt"),
        new UserModelMessage("Hello"),
    };

    public static UpstreamEventStreamHandler Chunks(params string[] lines)
    {
        return new UpstreamEventStreamHandler(EventStreamFrames.FromChunkLines(lines));
    }

    public static UpstreamEventStreamHandler Fixture(string name, IReadOnlyDictionary<string, string>? headers = null)
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "amazon-bedrock-" + name + ".chunks.txt"));
        return new UpstreamEventStreamHandler(EventStreamFrames.FromChunkLines(lines), headers);
    }

    public static ILanguageModel CreateModel(HttpMessageHandler handler, string modelId = ModelId)
    {
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "test-access-key",
            SecretAccessKey = "test-secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        return provider.LanguageModel(modelId);
    }

    public static async Task<List<LanguageModelStreamPart>> Stream(HttpMessageHandler handler, LanguageModelCallOptions? options = null, string modelId = ModelId)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in CreateModel(handler, modelId).DoStreamAsync(options ?? Call(), CancellationToken.None))
        {
            parts.Add(part);
        }

        return parts;
    }

    public static LanguageModelCallOptions Call(string? providerOptions = null)
    {
        return new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = providerOptions is null ? null : Options(providerOptions),
        };
    }

    public static Dictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        var result = new Dictionary<string, JsonElement>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            result[property.Name] = property.Value.Clone();
        }

        return result;
    }

    public static string[] Render(IEnumerable<LanguageModelStreamPart> parts) => parts.Select(Render).ToArray();

    /// <summary>One compact line per part, standing in for vitest's inline snapshots.</summary>
    public static string Render(LanguageModelStreamPart part)
    {
        switch (part)
        {
            case StreamStartStreamPart start:
                return "stream-start warnings=" + start.Warnings.Count;
            case ResponseMetadataStreamPart metadata:
                return "response-metadata id=" + (metadata.Id ?? "undefined") + " modelId=" + metadata.ModelId + " timestamp=" + (metadata.Timestamp?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'") ?? "undefined");
            case TextStartStreamPart textStart:
                return "text-start " + textStart.Id;
            case TextDeltaStreamPart textDelta:
                return "text-delta " + textDelta.Id + " " + Quote(textDelta.Delta);
            case TextEndStreamPart textEnd:
                return "text-end " + textEnd.Id;
            case ReasoningStartStreamPart reasoningStart:
                return "reasoning-start " + reasoningStart.Id;
            case ReasoningDeltaStreamPart reasoningDelta:
                return "reasoning-delta " + reasoningDelta.Id + " " + Quote(reasoningDelta.Delta) + MetaSuffix(reasoningDelta.ProviderMetadata);
            case ReasoningEndStreamPart reasoningEnd:
                return "reasoning-end " + reasoningEnd.Id + MetaSuffix(reasoningEnd.ProviderMetadata);
            case ToolInputStartStreamPart inputStart:
                return "tool-input-start " + inputStart.Id + " " + inputStart.ToolName;
            case ToolInputDeltaStreamPart inputDelta:
                return "tool-input-delta " + inputDelta.Id + " " + Quote(inputDelta.Delta);
            case ToolInputEndStreamPart inputEnd:
                return "tool-input-end " + inputEnd.Id;
            case ToolCallStreamPart call:
                return "tool-call " + call.ToolCallId + " " + call.ToolName + " " + Quote(call.ArgumentsJson);
            case RawStreamPart raw:
                return "raw " + raw.RawJson;
            case ErrorStreamPart error:
                return "error " + Quote(error.Message);
            case FinishStreamPart finish:
                return "finish " + FinishReasonName(finish.FinishReason) + " raw=" + (finish.RawFinishReason ?? "undefined")
                    + " usage=" + Usage(finish.Usage) + MetaSuffix(finish.ProviderMetadata);
            default:
                return part.Type;
        }
    }

    public static string FinishReasonName(FinishReason reason)
    {
        return reason switch
        {
            FinishReason.Stop => "stop",
            FinishReason.Length => "length",
            FinishReason.ContentFilter => "content-filter",
            FinishReason.ToolCalls => "tool-calls",
            FinishReason.Error => "error",
            _ => "other",
        };
    }

    private static string Usage(LanguageModelUsage usage)
    {
        string N(int? value) => value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "undefined";
        return "in(" + N(usage.InputTokens) + "," + N(usage.NoCacheInputTokens) + "," + N(usage.CacheReadTokens) + "," + N(usage.CacheWriteTokens)
            + ") out(" + N(usage.OutputTokens) + "," + N(usage.TextTokens) + "," + N(usage.ReasoningTokens) + ") raw=" + (usage.Raw?.GetRawText() ?? "undefined");
    }

    private static string MetaSuffix(JsonElement? metadata) => metadata is { } value ? " meta=" + Canonical(value) : string.Empty;

    public static string Canonical(JsonElement value) => Canonical(JsonNode.Parse(value.GetRawText()));

    private static string Canonical(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                return "{" + string.Join(",", obj.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => JsonSerializer.Serialize(p.Key) + ":" + Canonical(p.Value))) + "}";
            case JsonArray array:
                return "[" + string.Join(",", array.Select(Canonical)) + "]";
            case JsonValue value when value.TryGetValue<string>(out var text):
                return Quote(text);
            default:
                return node?.ToJsonString() ?? "null";
        }
    }

    public static string Quote(string value) => JsonSerializer.Serialize(value, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

    public static bool JsonEquals(string expectedJson, JsonElement actual)
    {
        return JsonNode.DeepEquals(JsonNode.Parse(expectedJson), JsonNode.Parse(actual.GetRawText()));
    }

    public static bool JsonMatches(string expectedJson, JsonElement actual) => Matches(JsonNode.Parse(expectedJson), JsonNode.Parse(actual.GetRawText()));

    // Mirrors vitest's toMatchObject: every expected property must match, extra actual properties are fine.
    private static bool Matches(JsonNode? expected, JsonNode? actual)
    {
        if (expected is JsonObject expectedObject)
        {
            if (actual is not JsonObject actualObject)
            {
                return false;
            }

            foreach (var property in expectedObject)
            {
                if (!actualObject.TryGetPropertyValue(property.Key, out var value) || !Matches(property.Value, value))
                {
                    return false;
                }
            }

            return true;
        }

        return JsonNode.DeepEquals(expected, actual);
    }
}

/// <summary>Streams a body whose length is not known up front, like a chunked network response.</summary>
internal sealed class UnknownLengthContent : HttpContent
{
    private readonly Stream _stream;

    public UnknownLengthContent(Stream stream)
    {
        _stream = stream;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => _stream.CopyToAsync(stream);

    protected override Task<Stream> CreateContentReadStreamAsync() => Task.FromResult(_stream);

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }
}

/// <summary>A read-only stream that hands back one chunk per read, like a network body.</summary>
internal sealed class ChunkedStream : Stream
{
    private readonly Queue<byte[]> _chunks;
    private byte[]? _current;
    private int _offset;

    public ChunkedStream(params byte[][] chunks)
    {
        _chunks = new Queue<byte[]>(chunks);
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_current is null || _offset >= _current.Length)
        {
            if (_chunks.Count == 0)
            {
                return 0;
            }

            _current = _chunks.Dequeue();
            _offset = 0;
        }

        var length = Math.Min(count, _current.Length - _offset);
        Buffer.BlockCopy(_current, _offset, buffer, offset, length);
        _offset += length;
        return length;
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
