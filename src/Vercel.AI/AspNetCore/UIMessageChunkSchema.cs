// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.AspNetCore;

/// <summary>A UI message chunk rejected by <see cref="UIMessageChunkSchema"/>.</summary>
public sealed class UIMessageChunkValidationException : Exception
{
    /// <summary>Creates the exception.</summary>
    public UIMessageChunkValidationException(string message)
        : base(message)
    {
    }
}

/// <summary>One parsed server-sent UI message chunk.</summary>
public sealed class UIMessageChunkParseResult
{
    /// <summary>Creates a parse result.</summary>
    public UIMessageChunkParseResult(bool success, JsonObject? value, JsonObject? rawValue)
    {
        Success = success;
        Value = value;
        RawValue = rawValue;
    }

    /// <summary>True when <see cref="Value"/> matches the chunk schema.</summary>
    public bool Success { get; }

    /// <summary>Validated chunk, including fields added by a newer server.</summary>
    public JsonObject? Value { get; }

    /// <summary>JSON value before validation.</summary>
    public JsonObject? RawValue { get; }
}

/// <summary>Validates UI message chunks, including fields added by newer servers.</summary>
public static class UIMessageChunkSchema
{
    private static readonly HashSet<string> Known = new(StringComparer.Ordinal)
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

    /// <summary>Returns a clone of <paramref name="value"/> when it is a known UI message chunk.</summary>
    public static JsonObject Validate(JsonNode? value)
    {
        if (value is not JsonObject obj || obj["type"] is not JsonValue typeValue || !typeValue.TryGetValue<string>(out var type))
        {
            throw new UIMessageChunkValidationException("Invalid UI message chunk.");
        }

        if (!Known.Contains(type) && !type.StartsWith("data-", StringComparison.Ordinal))
        {
            throw new UIMessageChunkValidationException("Unknown UI message chunk type.");
        }

        Require(obj, type);
        return obj.DeepClone().AsObject();
    }

    /// <summary>Parses <c>data:</c> lines in <paramref name="sse"/> and validates each JSON payload.</summary>
    public static IReadOnlyList<UIMessageChunkParseResult> ParseJsonEventStream(string sse)
    {
        if (sse is null)
        {
            throw new ArgumentNullException(nameof(sse));
        }

        var results = new List<UIMessageChunkParseResult>();
        var lines = sse.Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (line.Length < 5 || !line.StartsWith("data:", StringComparison.Ordinal))
            {
                continue;
            }

            var payload = line.Substring(5).Trim();
            if (payload.Length == 0 || payload == "[DONE]")
            {
                continue;
            }

            var raw = JsonNode.Parse(payload) as JsonObject;
            var value = Validate(raw);
            results.Add(new UIMessageChunkParseResult(true, value, raw?.DeepClone().AsObject()));
        }

        return results;
    }

    private static void Require(JsonObject obj, string type)
    {
        switch (type)
        {
            case "text-start":
            case "text-end":
            case "reasoning-start":
            case "reasoning-end":
                Need(obj, "id");
                break;
            case "text-delta":
            case "reasoning-delta":
                Need(obj, "id");
                Need(obj, "delta");
                break;
            case "error":
                Need(obj, "errorText");
                break;
            case "tool-input-start":
            case "tool-input-available":
                Need(obj, "toolCallId");
                Need(obj, "toolName");
                if (type == "tool-input-available")
                {
                    Need(obj, "input");
                }

                break;
            case "tool-input-delta":
                Need(obj, "toolCallId");
                Need(obj, "inputTextDelta");
                break;
            case "tool-input-error":
                Need(obj, "toolCallId");
                Need(obj, "toolName");
                Need(obj, "input");
                Need(obj, "errorText");
                break;
            case "tool-approval-request":
                Need(obj, "approvalId");
                Need(obj, "toolCallId");
                break;
            case "tool-approval-response":
                Need(obj, "approvalId");
                Need(obj, "approved");
                break;
            case "tool-output-available":
                Need(obj, "toolCallId");
                Need(obj, "output");
                break;
            case "tool-output-error":
                Need(obj, "toolCallId");
                Need(obj, "errorText");
                break;
            case "tool-output-denied":
                Need(obj, "toolCallId");
                break;
            case "custom":
                Need(obj, "kind");
                break;
            case "source-url":
                Need(obj, "sourceId");
                Need(obj, "url");
                break;
            case "source-document":
                Need(obj, "sourceId");
                Need(obj, "mediaType");
                Need(obj, "title");
                break;
            case "file":
            case "reasoning-file":
                Need(obj, "url");
                Need(obj, "mediaType");
                break;
            case "finish":
                if (obj.ContainsKey("finishReason"))
                {
                    var reason = obj["finishReason"] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
                    if (reason == null || !FinishReasons.Contains(reason))
                    {
                        throw new UIMessageChunkValidationException("Invalid finish reason.");
                    }
                }

                break;
            case "message-metadata":
                Need(obj, "messageMetadata");
                break;
        }
    }

    private static void Need(JsonObject obj, string name)
    {
        if (!obj.ContainsKey(name))
        {
            throw new UIMessageChunkValidationException("Missing " + name + ".");
        }
    }
}
