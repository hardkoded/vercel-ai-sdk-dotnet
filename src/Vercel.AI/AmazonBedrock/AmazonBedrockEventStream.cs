// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>One decoded AWS event-stream message.</summary>
internal sealed class DecodedEvent
{
    public DecodedEvent(string? messageType, string? eventType, string? exceptionType, string data)
    {
        MessageType = messageType;
        EventType = eventType;
        ExceptionType = exceptionType;
        Data = data;
    }

    public string? MessageType { get; }

    public string? EventType { get; }

    public string? ExceptionType { get; }

    public string Data { get; }
}

/// <summary>
/// Decodes the <c>application/vnd.amazon.eventstream</c> binary framing of a ConverseStream response.
/// Maps to <c>createAmazonBedrockEventStreamDecoder</c>.
/// </summary>
internal static class AmazonBedrockEventStreamDecoder
{
    private const int PreludeLength = 12;
    private const int MessageCrcLength = 4;

    // AWS event-stream messages are at most 16 MiB.
    private const uint MaxMessageLength = 16 * 1024 * 1024;

    /// <summary>
    /// Reads frames from <paramref name="body"/> and passes each to <paramref name="processEvent"/>,
    /// which enqueues zero or more outputs. A frame that does not decode, or bytes left at the end of
    /// the stream, throw.
    /// </summary>
    public static async IAsyncEnumerable<T> Create<T>(
        Stream body,
        Action<DecodedEvent, Action<T>> processEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = new byte[8192];
        var count = 0;
        var chunk = new byte[8192];
        var outputs = new List<T>();
        while (true)
        {
            var read = await body.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            if (count + read > buffer.Length)
            {
                Array.Resize(ref buffer, Math.Max(buffer.Length * 2, count + read));
            }

            Buffer.BlockCopy(chunk, 0, buffer, count, read);
            count += read;

            while (count >= 4)
            {
                var declaredLength = ReadUInt32(buffer, 0);
                if (declaredLength > MaxMessageLength)
                {
                    throw new AiSdkException(
                        "Amazon Bedrock event-stream frame length " + declaredLength + " exceeds the maximum of " + MaxMessageLength + " bytes.");
                }

                var totalLength = (int)declaredLength;
                if (count < totalLength)
                {
                    break;
                }

                var decoded = Decode(buffer, totalLength);
                Buffer.BlockCopy(buffer, totalLength, buffer, 0, count - totalLength);
                count -= totalLength;

                outputs.Clear();
                processEvent(decoded, outputs.Add);
                foreach (var output in outputs)
                {
                    yield return output;
                }
            }
        }

        if (count > 0)
        {
            throw new AiSdkException(
                "Incomplete Amazon Bedrock event-stream frame: " + count + " buffered bytes remain at end of stream.");
        }
    }

    internal static DecodedEvent Decode(byte[] frame, int totalLength)
    {
        if (totalLength < PreludeLength + MessageCrcLength)
        {
            throw new AiSdkException("Reached end of stream before the event-stream frame was complete.");
        }

        var headersLength = (int)ReadUInt32(frame, 4);
        var preludeCrc = ReadUInt32(frame, 8);
        var actualPreludeCrc = Crc32(frame, 0, 8);
        if (preludeCrc != actualPreludeCrc)
        {
            throw new AiSdkException(
                "The prelude checksum specified in the message (" + preludeCrc + ") does not match the calculated CRC32 checksum (" + actualPreludeCrc + ").");
        }

        var messageCrc = ReadUInt32(frame, totalLength - MessageCrcLength);
        var actualMessageCrc = Crc32(frame, 0, totalLength - MessageCrcLength);
        if (messageCrc != actualMessageCrc)
        {
            throw new AiSdkException(
                "The message checksum did not match the expected value of " + actualMessageCrc + ".");
        }

        var headers = ReadHeaders(frame, PreludeLength, headersLength);
        var payloadStart = PreludeLength + headersLength;
        var payloadLength = totalLength - payloadStart - MessageCrcLength;
        headers.TryGetValue(":message-type", out var messageType);
        headers.TryGetValue(":event-type", out var eventType);
        headers.TryGetValue(":exception-type", out var exceptionType);
        return new DecodedEvent(messageType, eventType, exceptionType, Encoding.UTF8.GetString(frame, payloadStart, payloadLength));
    }

    private static uint ReadUInt32(byte[] data, int offset)
    {
        return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
    }

    private static int ReadUInt16(byte[] data, int offset)
    {
        return (data[offset] << 8) | data[offset + 1];
    }

    private static Dictionary<string, string?> ReadHeaders(byte[] data, int start, int length)
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);
        var position = start;
        var end = start + length;
        while (position < end)
        {
            var nameLength = data[position++];
            var name = Encoding.UTF8.GetString(data, position, nameLength);
            position += nameLength;
            var type = data[position++];
            string? value = null;
            switch (type)
            {
                case 0:
                case 1:
                    break;
                case 2:
                    position += 1;
                    break;
                case 3:
                    position += 2;
                    break;
                case 4:
                    position += 4;
                    break;
                case 5:
                case 8:
                    position += 8;
                    break;
                case 6:
                case 7:
                    var valueLength = ReadUInt16(data, position);
                    position += 2;
                    if (type == 7)
                    {
                        value = Encoding.UTF8.GetString(data, position, valueLength);
                    }

                    position += valueLength;
                    break;
                case 9:
                    position += 16;
                    break;
                default:
                    throw new AiSdkException("Unrecognized event-stream header type " + type + ".");
            }

            result[name] = value;
        }

        return result;
    }

    private static readonly uint[] CrcTable = BuildCrcTable();

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }

    internal static uint Crc32(byte[] data, int offset, int count)
    {
        var crc = 0xFFFFFFFFu;
        for (var i = offset; i < offset + count; i++)
        {
            crc = CrcTable[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
        }

        return crc ^ 0xFFFFFFFFu;
    }
}

/// <summary>A parsed stream chunk, or the reason it could not be parsed.</summary>
internal sealed class AmazonBedrockStreamChunk
{
    private AmazonBedrockStreamChunk(bool success, JsonElement value, JsonElement rawValue, string? error)
    {
        Success = success;
        Value = value;
        RawValue = rawValue;
        Error = error;
    }

    public bool Success { get; }

    /// <summary>The validated chunk, wrapped as <c>{ [eventType]: payload }</c>.</summary>
    public JsonElement Value { get; }

    /// <summary>The wrapped payload before validation.</summary>
    public JsonElement RawValue { get; }

    public string? Error { get; }

    public static AmazonBedrockStreamChunk Ok(JsonElement value, JsonElement rawValue) => new(true, value, rawValue, null);

    public static AmazonBedrockStreamChunk Fail(string error) => new(false, default, default, error);
}

/// <summary>
/// Turns a ConverseStream response body into wrapped, validated chunks.
/// Maps to <c>createAmazonBedrockEventStreamResponseHandler</c>.
/// </summary>
internal static class AmazonBedrockEventStreamResponseHandler
{
    /// <summary>
    /// Handles <paramref name="response"/>. <paramref name="validate"/> returns an error message for a chunk
    /// that does not match the stream schema, or null.
    /// </summary>
    public static async Task<(IReadOnlyDictionary<string, string> Headers, IAsyncEnumerable<AmazonBedrockStreamChunk> Stream)> HandleAsync(
        HttpResponseMessage response,
        Func<JsonElement, string?> validate,
        CancellationToken cancellationToken = default)
    {
        var headers = JsonStreams.ExtractResponseHeaders(response);
        if (response.Content is null || response.Content.Headers.ContentLength == 0)
        {
            throw new ApiException("Response body is empty", (int)response.StatusCode, null) { ResponseHeaders = headers };
        }

        var body = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var stream = AmazonBedrockEventStreamDecoder.Create<AmazonBedrockStreamChunk>(
            body,
            (evt, enqueue) => ProcessEvent(evt, enqueue, validate),
            cancellationToken);
        return (headers, stream);
    }

    private static void ProcessEvent(DecodedEvent evt, Action<AmazonBedrockStreamChunk> enqueue, Func<JsonElement, string?> validate)
    {
        var payloadType = evt.MessageType == "event"
            ? evt.EventType
            : evt.MessageType == "exception" ? evt.ExceptionType : null;
        if (payloadType is null)
        {
            return;
        }

        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(evt.Data);
        }
        catch (JsonException exception)
        {
            enqueue(AmazonBedrockStreamChunk.Fail("JSON parsing failed: Text: " + evt.Data + ".\nError message: " + exception.Message));
            return;
        }

        if (parsed is JsonObject payload)
        {
            payload.Remove("p");
        }

        var wrapped = new JsonObject { [payloadType] = parsed };
        using var document = JsonDocument.Parse(wrapped.ToJsonString());
        var element = document.RootElement.Clone();
        var error = validate(element);
        enqueue(error is null ? AmazonBedrockStreamChunk.Ok(element, element) : AmazonBedrockStreamChunk.Fail(error));
    }
}
