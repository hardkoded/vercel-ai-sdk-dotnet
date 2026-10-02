// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using System.Text.Json.Nodes;

namespace Vercel.AI.Mcp;

/// <summary>Sentinel for a JSON-RPC value that was omitted the way JavaScript <c>undefined</c> is omitted.</summary>
internal static class McpUndefined
{
    internal static readonly object Value = new object();
}

/// <summary>Parses MCP <c>tools/call</c> results and converts them to model output.</summary>
public static class McpCallToolResult
{
    /// <summary>Parses <paramref name="value"/>. Throws when the result is not a call-tool result.</summary>
    public static JsonObject Parse(JsonNode? value)
    {
        if (!TryParse(value, out var result) || result is null)
        {
            throw new InvalidOperationException("Invalid call tool result.");
        }

        return result;
    }

    /// <summary>
    /// Parses an object graph. Cyclic <c>structuredContent</c> throws.
    /// Non-JSON structured content and a missing <c>toolResult</c> throw.
    /// </summary>
    public static JsonObject Parse(IDictionary<string, object?> value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.TryGetValue("structuredContent", out var structured))
        {
            EnsureAcyclic(structured);
            if (!IsJsonCompatible(structured))
            {
                throw new InvalidOperationException("structuredContent must be JSON.");
            }
        }

        if (!TryParse(value, out var result) || result is null)
        {
            throw new InvalidOperationException("Invalid call tool result.");
        }

        return result;
    }

    /// <summary>Returns true when <paramref name="value"/> is a call-tool result and writes the normalized object.</summary>
    public static bool TryParse(JsonNode? value, out JsonObject? result)
    {
        result = null;
        if (value is not JsonObject obj)
        {
            return false;
        }

        var hasContent = obj.ContainsKey("content");
        var hasStructured = obj.ContainsKey("structuredContent");
        var hasToolResult = obj.ContainsKey("toolResult");
        if (hasContent)
        {
            if (obj["content"] is not JsonArray content || !ValidContent(content))
            {
                return false;
            }

            result = obj.DeepClone().AsObject();
            if (!result.ContainsKey("isError"))
            {
                result["isError"] = false;
            }

            return true;
        }

        if (hasStructured)
        {
            var structured = obj["structuredContent"];
            var text = structured == null ? "null" : structured.ToJsonString();
            result = obj.DeepClone().AsObject();
            if (structured == null)
            {
                result["structuredContent"] = JsonNode.Parse("null")!;
            }

            result["content"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = text,
                },
            };
            if (!result.ContainsKey("isError"))
            {
                result["isError"] = false;
            }

            return true;
        }

        if (hasToolResult)
        {
            result = obj.DeepClone().AsObject();
            return true;
        }

        return false;
    }

    /// <summary>Returns true when <paramref name="value"/> is a call-tool result and writes the normalized object.</summary>
    public static bool TryParse(IDictionary<string, object?> value, out JsonObject? result)
    {
        result = null;
        if (value is null)
        {
            return false;
        }

        if (value.TryGetValue("structuredContent", out var structured) && !HasContent(value))
        {
            if (!IsJsonCompatible(structured))
            {
                return false;
            }

            try
            {
                EnsureAcyclic(structured);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        if (value.TryGetValue("toolResult", out var toolResult) && ReferenceEquals(toolResult, McpUndefined.Value))
        {
            return false;
        }

        return TryParse(ToNode(value), out result);
    }

    /// <summary>
    /// Converts an MCP tool result into AI SDK model output.
    /// Text and images become content parts. Any other shape becomes JSON.
    /// </summary>
    public static JsonObject ToModelOutput(JsonNode? output)
    {
        if (output is not JsonObject result || result["content"] is not JsonArray content)
        {
            return new JsonObject
            {
                ["type"] = "json",
                ["value"] = output?.DeepClone(),
            };
        }

        var converted = new JsonArray();
        foreach (var partNode in content)
        {
            if (partNode is not JsonObject part)
            {
                converted.Add(new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = partNode?.ToJsonString() ?? "null",
                });
                continue;
            }

            var type = part["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var typeText) ? typeText : null;
            if (type == "text" && part.ContainsKey("text"))
            {
                converted.Add(new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = part["text"]?.DeepClone(),
                });
                continue;
            }

            if (type == "image" && part.ContainsKey("data") && part.ContainsKey("mimeType"))
            {
                converted.Add(new JsonObject
                {
                    ["type"] = "file",
                    ["mediaType"] = part["mimeType"]?.DeepClone(),
                    ["data"] = new JsonObject
                    {
                        ["type"] = "data",
                        ["data"] = part["data"]?.DeepClone(),
                    },
                });
                continue;
            }

            converted.Add(new JsonObject
            {
                ["type"] = "text",
                ["text"] = part.ToJsonString(),
            });
        }

        return new JsonObject
        {
            ["type"] = "content",
            ["value"] = converted,
        };
    }

    private static bool HasContent(IDictionary<string, object?> value)
    {
        return value.TryGetValue("content", out var content) && !ReferenceEquals(content, McpUndefined.Value);
    }

    private static bool IsJsonCompatible(object? value)
    {
        if (value is null || ReferenceEquals(value, McpUndefined.Value) || value is BigInteger)
        {
            return false;
        }

        if (value is double number)
        {
            return (!double.IsNaN(number) && !double.IsInfinity(number));
        }

        if (value is float single)
        {
            return (!float.IsNaN(single) && !float.IsInfinity(single));
        }

        return true;
    }

    private static void EnsureAcyclic(object? value)
    {
        var seen = new HashSet<object>();
        Walk(value, seen);
    }

    private static void Walk(object? value, HashSet<object> seen)
    {
        if (value is null || value is string || value is ValueType)
        {
            if (value is double number && (double.IsNaN(number) || double.IsInfinity(number)))
            {
                throw new InvalidOperationException("structuredContent must be JSON.");
            }

            return;
        }

        if (!seen.Add(value))
        {
            throw new InvalidOperationException("Cyclic structured content.");
        }

        if (value is IDictionary<string, object?> dictionary)
        {
            foreach (var pair in dictionary)
            {
                Walk(pair.Value, seen);
            }

            return;
        }

        if (value is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                Walk(item, seen);
            }
        }
    }

    private static JsonNode? ToNode(object? value)
    {
        switch (value)
        {
            case null:
                return null;
            case JsonNode node:
                return node.DeepClone();
            case string text:
                return JsonValue.Create(text);
            case bool flag:
                return JsonValue.Create(flag);
            case int number:
                return JsonValue.Create(number);
            case long number:
                return JsonValue.Create(number);
            case double number when (!double.IsNaN(number) && !double.IsInfinity(number)):
                return JsonValue.Create(number);
            case IDictionary<string, object?> dictionary:
                var obj = new JsonObject();
                foreach (var pair in dictionary)
                {
                    if (ReferenceEquals(pair.Value, McpUndefined.Value))
                    {
                        continue;
                    }

                    obj[pair.Key] = ToNode(pair.Value);
                }

                return obj;
            case System.Collections.IEnumerable enumerable:
                var array = new JsonArray();
                foreach (var item in enumerable)
                {
                    array.Add(ToNode(item));
                }

                return array;
            default:
                return null;
        }
    }

    private static bool ValidContent(JsonArray content)
    {
        foreach (var item in content)
        {
            if (!ValidContentItem(item))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidContentItem(JsonNode? node)
    {
        if (node is not JsonObject part || part["type"] is not JsonValue typeValue || !typeValue.TryGetValue<string>(out var type))
        {
            return false;
        }

        switch (type)
        {
            case "text":
                return part["text"] is JsonValue text && text.TryGetValue<string>(out _);
            case "image":
                return part["mimeType"] is JsonValue mime && mime.TryGetValue<string>(out _)
                    && part["data"] is JsonValue data && data.TryGetValue<string>(out var encoded) && IsBase64(encoded);
            case "resource":
                return ValidResource(part["resource"]);
            case "resource_link":
                return part["uri"] is JsonValue uri && uri.TryGetValue<string>(out _)
                    && part["name"] is JsonValue name && name.TryGetValue<string>(out _);
            default:
                return true;
        }
    }

    private static bool ValidResource(JsonNode? node)
    {
        if (node is not JsonObject resource || resource["uri"] is not JsonValue uri || !uri.TryGetValue<string>(out _))
        {
            return false;
        }

        var hasText = resource["text"] is JsonValue text && text.TryGetValue<string>(out _);
        var hasBlob = resource["blob"] is JsonValue blob && blob.TryGetValue<string>(out var encoded) && IsBase64(encoded);
        return hasText || hasBlob;
    }

    private static bool IsBase64(string value)
    {
        if (value.Length == 0 || value.Length % 4 != 0)
        {
            return false;
        }

        var padding = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            var digit = (character >= 'A' && character <= 'Z')
                || (character >= 'a' && character <= 'z')
                || (character >= '0' && character <= '9')
                || character == '+'
                || character == '/';
            if (digit)
            {
                if (padding > 0)
                {
                    return false;
                }

                continue;
            }

            if (character == '=')
            {
                padding++;
                if (padding > 2)
                {
                    return false;
                }

                continue;
            }

            return false;
        }

        return true;
    }
}
