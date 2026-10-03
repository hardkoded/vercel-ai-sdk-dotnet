// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.AspNetCore;

/// <summary>Thrown when a UI message chunk does not match the protocol schema.</summary>
public sealed class UIMessageChunkException : Exception
{
    /// <summary>Creates an exception with <paramref name="message"/>.</summary>
    public UIMessageChunkException(string message)
        : base(message)
    {
    }
}

/// <summary>Reads AI SDK UI message chunks, including fields added by a newer server.</summary>
public static class UIMessageChunks
{
    private static readonly HashSet<string> KnownTypes = new(StringComparer.Ordinal)
    {
        "text-start",
        "text-delta",
        "text-end",
        "error",
        "tool-input-start",
        "tool-input-delta",
        "tool-input-available",
        "tool-input-error",
        "tool-approval-request",
        "tool-approval-response",
        "tool-output-available",
        "tool-output-error",
        "tool-output-denied",
        "reasoning-start",
        "reasoning-delta",
        "reasoning-end",
        "custom",
        "source-url",
        "source-document",
        "file",
        "reasoning-file",
        "start-step",
        "finish-step",
        "reset-step",
        "start",
        "finish",
        "abort",
        "message-metadata",
    };

    private static readonly HashSet<string> FinishReasons = new(StringComparer.Ordinal)
    {
        "stop",
        "length",
        "content-filter",
        "tool-calls",
        "error",
        "other",
    };

    /// <summary>Parses one chunk object. Extra properties on a known type are kept.</summary>
    /// <param name="value">JSON object for one chunk.</param>
    public static JsonObject Parse(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new UIMessageChunkException("UI message chunk must be a JSON object.");
        }

        if (!value.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
        {
            throw new UIMessageChunkException("UI message chunk is missing a type.");
        }

        var type = typeElement.GetString() ?? string.Empty;
        if (!IsKnown(type))
        {
            throw new UIMessageChunkException("Unknown UI message chunk type: " + type);
        }

        Validate(type, value);
        return JsonNode.Parse(value.GetRawText())!.AsObject();
    }

    /// <summary>Parses <c>data:</c> SSE frames into chunks. <c>data: [DONE]</c> ends the stream.</summary>
    /// <param name="sse">SSE payload.</param>
    public static IReadOnlyList<JsonObject> ParseSse(string sse)
    {
        if (sse is null)
        {
            throw new ArgumentNullException(nameof(sse));
        }

        var chunks = new List<JsonObject>();
        var events = sse.Split(new[] { "\n\n" }, StringSplitOptions.None);
        foreach (var evt in events)
        {
            if (evt.Length == 0)
            {
                continue;
            }

            string? data = null;
            foreach (var line in evt.Split('\n'))
            {
                if (!line.StartsWith("data:", StringComparison.Ordinal))
                {
                    continue;
                }

                var payload = line.Substring("data:".Length).TrimStart();
                if (payload == "[DONE]")
                {
                    return chunks;
                }

                data = data == null ? payload : data + "\n" + payload;
            }

            if (data == null)
            {
                continue;
            }

            using var document = JsonDocument.Parse(data);
            chunks.Add(Parse(document.RootElement));
        }

        return chunks;
    }

    private static bool IsKnown(string type)
    {
        return KnownTypes.Contains(type) || (type.StartsWith("data-", StringComparison.Ordinal) && type.Length > "data-".Length);
    }

    private static void Validate(string type, JsonElement value)
    {
        switch (type)
        {
            case "text-start":
            case "text-end":
            case "reasoning-start":
            case "reasoning-end":
                RequireString(value, "id");
                break;
            case "text-delta":
            case "reasoning-delta":
                RequireString(value, "id");
                RequireString(value, "delta");
                break;
            case "error":
            case "tool-output-error":
                RequireString(value, type == "error" ? "errorText" : "toolCallId");
                if (type == "tool-output-error")
                {
                    RequireString(value, "errorText");
                }

                break;
            case "tool-input-start":
            case "tool-input-available":
                RequireString(value, "toolCallId");
                RequireString(value, "toolName");
                if (type == "tool-input-available")
                {
                    RequireProperty(value, "input");
                }

                break;
            case "tool-input-delta":
                RequireString(value, "toolCallId");
                RequireString(value, "inputTextDelta");
                break;
            case "tool-input-error":
                RequireString(value, "toolCallId");
                RequireString(value, "toolName");
                RequireProperty(value, "input");
                RequireString(value, "errorText");
                break;
            case "tool-approval-request":
                RequireString(value, "approvalId");
                RequireString(value, "toolCallId");
                break;
            case "tool-approval-response":
                RequireString(value, "approvalId");
                RequireBoolean(value, "approved");
                break;
            case "tool-output-available":
                RequireString(value, "toolCallId");
                RequireProperty(value, "output");
                break;
            case "tool-output-denied":
                RequireString(value, "toolCallId");
                break;
            case "custom":
                RequireString(value, "kind");
                break;
            case "source-url":
                RequireString(value, "sourceId");
                RequireString(value, "url");
                break;
            case "source-document":
                RequireString(value, "sourceId");
                RequireString(value, "mediaType");
                RequireString(value, "title");
                break;
            case "file":
            case "reasoning-file":
                RequireString(value, "url");
                RequireString(value, "mediaType");
                break;
            case "message-metadata":
                RequireProperty(value, "messageMetadata");
                break;
            case "finish":
                if (value.TryGetProperty("finishReason", out var reason) && reason.ValueKind != JsonValueKind.Null)
                {
                    if (reason.ValueKind != JsonValueKind.String || !FinishReasons.Contains(reason.GetString() ?? string.Empty))
                    {
                        throw new UIMessageChunkException("UI message finish chunk has an unknown finishReason.");
                    }
                }

                break;
            default:
                if (type.StartsWith("data-", StringComparison.Ordinal))
                {
                    RequireProperty(value, "data");
                }

                break;
        }
    }

    private static void RequireProperty(JsonElement value, string name)
    {
        if (!value.TryGetProperty(name, out _))
        {
            throw new UIMessageChunkException("UI message chunk is missing " + name + ".");
        }
    }

    private static void RequireString(JsonElement value, string name)
    {
        if (!value.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new UIMessageChunkException("UI message chunk is missing " + name + ".");
        }
    }

    private static void RequireBoolean(JsonElement value, string name)
    {
        if (!value.TryGetProperty(name, out var property)
            || (property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False))
        {
            throw new UIMessageChunkException("UI message chunk is missing " + name + ".");
        }
    }
}
