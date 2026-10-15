// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.AmazonBedrock;

/// <summary>
/// Turns the binary event stream of <c>invoke-with-response-stream</c> into Anthropic SSE data payloads.
/// Maps to <c>createAmazonBedrockAnthropicFetch</c>.
/// </summary>
internal static class AmazonBedrockAnthropicFetch
{
    /// <summary>The content type of a Bedrock binary event stream.</summary>
    public const string EventStreamContentType = "application/vnd.amazon.eventstream";

    /// <summary>
    /// Decodes <paramref name="body"/> frame by frame and yields the data of each SSE event, without the <c>data: </c> prefix.
    /// <c>[DONE]</c> marks <c>messageStop</c>.
    /// </summary>
    public static IAsyncEnumerable<string> TransformEventStreamToSse(Stream body, CancellationToken cancellationToken = default)
    {
        return AmazonBedrockEventStreamDecoder.Create<string>(body, ProcessEvent, cancellationToken);
    }

    private static void ProcessEvent(DecodedEvent evt, Action<string> enqueue)
    {
        if (evt.MessageType == "event")
        {
            if (evt.EventType == "chunk")
            {
                enqueue(ReadChunk(evt.Data));
            }
            else if (evt.EventType == "messageStop")
            {
                enqueue("[DONE]");
            }
        }
        else if (evt.MessageType == "exception")
        {
            enqueue(ReadException(evt));
        }
    }

    private static string ReadChunk(string data)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(data);
        }
        catch (JsonException)
        {
            return data;
        }

        if (parsed is JsonObject chunk && chunk["bytes"] is JsonValue value && value.TryGetValue<string>(out var bytes) && bytes.Length > 0)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(bytes));
        }

        return data;
    }

    private static string ReadException(DecodedEvent evt)
    {
        JsonNode? data;
        try
        {
            data = JsonNode.Parse(evt.Data);
        }
        catch (JsonException)
        {
            data = JsonValue.Create(evt.Data);
        }

        var message = data is JsonObject { } payload && payload["message"] is JsonValue text && text.TryGetValue<string>(out var value)
            ? value
            : evt.Data;
        var type = evt.EventType ?? evt.ExceptionType ?? "error";
        var error = new JsonObject { ["type"] = type, ["message"] = message };
        var (statusCode, isRetryable) = AmazonBedrockStreamError.Metadata(type);
        if (statusCode is { } status)
        {
            error["statusCode"] = status;
        }

        if (isRetryable is { } retryable)
        {
            error["isRetryable"] = retryable;
        }

        error["data"] = data;
        return new JsonObject { ["type"] = "error", ["error"] = error }.ToJsonString();
    }
}
