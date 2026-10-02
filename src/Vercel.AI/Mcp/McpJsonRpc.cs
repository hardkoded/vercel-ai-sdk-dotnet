// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.Mcp;

/// <summary>Validates JSON-RPC 2.0 messages used by MCP transports.</summary>
public static class McpJsonRpc
{
    /// <summary>Parses <paramref name="text"/> and validates it as a JSON-RPC message.</summary>
    public static JsonObject Parse(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        return Validate(JsonNode.Parse(text));
    }

    /// <summary>
    /// Returns a clone of <paramref name="message"/> when it is a JSON-RPC request, notification, response, or error.
    /// </summary>
    public static JsonObject Validate(JsonNode? message)
    {
        if (message is not JsonObject obj || !IsVersion(obj))
        {
            throw new ArgumentException("Invalid JSON-RPC message.");
        }

        var keys = Keys(obj);
        if (IsRequest(obj, keys) || IsNotification(obj, keys) || IsResponse(obj, keys) || IsError(obj, keys))
        {
            return obj.DeepClone().AsObject();
        }

        throw new ArgumentException("Invalid JSON-RPC message.");
    }

    private static bool IsVersion(JsonObject obj)
    {
        return obj["jsonrpc"] is JsonValue version && version.TryGetValue<string>(out var text) && text == "2.0";
    }

    private static bool IsRequest(JsonObject obj, HashSet<string> keys)
    {
        return HasOnly(keys, "jsonrpc", "id", "method", "params")
            && keys.Contains("id")
            && keys.Contains("method")
            && IsId(obj["id"])
            && IsMethod(obj["method"])
            && IsOptionalObject(obj, keys, "params");
    }

    private static bool IsNotification(JsonObject obj, HashSet<string> keys)
    {
        return HasOnly(keys, "jsonrpc", "method", "params")
            && !keys.Contains("id")
            && keys.Contains("method")
            && IsMethod(obj["method"])
            && IsOptionalObject(obj, keys, "params");
    }

    private static bool IsResponse(JsonObject obj, HashSet<string> keys)
    {
        return HasOnly(keys, "jsonrpc", "id", "result")
            && keys.Contains("id")
            && keys.Contains("result")
            && IsId(obj["id"])
            && obj["result"] is JsonObject;
    }

    private static bool IsError(JsonObject obj, HashSet<string> keys)
    {
        if (!HasOnly(keys, "jsonrpc", "id", "error") || !keys.Contains("error"))
        {
            return false;
        }

        if (keys.Contains("id") && !IsId(obj["id"]))
        {
            return false;
        }

        if (obj["error"] is not JsonObject error)
        {
            return false;
        }

        var errorKeys = Keys(error);
        if (!HasOnly(errorKeys, "code", "message", "data") || !errorKeys.Contains("code") || !errorKeys.Contains("message"))
        {
            return false;
        }

        return IsInteger(error["code"]) && error["message"] is JsonValue message && message.TryGetValue<string>(out _);
    }

    private static bool IsOptionalObject(JsonObject obj, HashSet<string> keys, string name)
    {
        if (!keys.Contains(name))
        {
            return true;
        }

        return obj[name] is JsonObject;
    }

    private static bool IsMethod(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<string>(out _);
    }

    private static bool IsId(JsonNode? node)
    {
        if (node is not JsonValue value)
        {
            return false;
        }

        if (value.TryGetValue<string>(out _))
        {
            return true;
        }

        return IsInteger(value);
    }

    private static bool IsInteger(JsonNode? node)
    {
        if (node is not JsonValue value)
        {
            return false;
        }

        if (value.TryGetValue<int>(out _))
        {
            return true;
        }

        if (value.TryGetValue<long>(out _))
        {
            return true;
        }

        if (value.TryGetValue<double>(out var number) && (!double.IsNaN(number) && !double.IsInfinity(number)) && number == Math.Truncate(number))
        {
            return number >= int.MinValue && number <= int.MaxValue;
        }

        return false;
    }

    private static HashSet<string> Keys(JsonObject obj)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in obj)
        {
            keys.Add(pair.Key);
        }

        return keys;
    }

    private static bool HasOnly(HashSet<string> keys, params string[] allowed)
    {
        foreach (var key in keys)
        {
            var found = false;
            for (var index = 0; index < allowed.Length; index++)
            {
                if (key == allowed[index])
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return false;
            }
        }

        return true;
    }
}
