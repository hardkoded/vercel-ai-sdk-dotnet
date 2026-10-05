// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>Parsed Anthropic error body. Extra fields such as <c>details</c> are dropped.</summary>
public sealed class AnthropicErrorData
{
    /// <summary>Creates a parsed error.</summary>
    public AnthropicErrorData(bool success, string? type, string? errorType, string? message, string rawValue)
    {
        Success = success;
        Type = type;
        ErrorType = errorType;
        Message = message;
        RawValue = rawValue ?? string.Empty;
    }

    /// <summary>True when the body matches <c>{ type: error, error: { type, message } }</c>.</summary>
    public bool Success { get; }

    /// <summary>Top-level type.</summary>
    public string? Type { get; }

    /// <summary>Error type, such as <c>overloaded_error</c>.</summary>
    public string? ErrorType { get; }

    /// <summary>Error message.</summary>
    public string? Message { get; }

    /// <summary>Original JSON.</summary>
    public string RawValue { get; }

    /// <summary>Parses an error payload. A matching object succeeds even when extra properties are present.</summary>
    public static AnthropicErrorData Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || AnthropicJson.String(root, "type") != "error"
                || !root.TryGetProperty("error", out var error)
                || error.ValueKind != JsonValueKind.Object
                || AnthropicJson.String(error, "type") == null
                || AnthropicJson.String(error, "message") == null)
            {
                return new AnthropicErrorData(false, null, null, null, json);
            }

            return new AnthropicErrorData(true, "error", AnthropicJson.String(error, "type"), AnthropicJson.String(error, "message"), json);
        }
        catch (JsonException)
        {
            return new AnthropicErrorData(false, null, null, null, json);
        }
    }

    /// <summary>The validated value with only <c>type</c> and <c>error.type</c>/<c>error.message</c>.</summary>
    public JsonObject? Value()
    {
        if (!Success)
        {
            return null;
        }

        return new JsonObject
        {
            ["type"] = Type,
            ["error"] = new JsonObject
            {
                ["type"] = ErrorType,
                ["message"] = Message,
            },
        };
    }
}

/// <summary>HTTP status implied by an Anthropic stream error type.</summary>
public static class AnthropicStreamError
{
    /// <summary>Maps an error type onto a status code. Unknown types have no status.</summary>
    public static int? StatusCode(string? type)
    {
        switch (type)
        {
            case "api_error":
                return 500;
            case "overloaded_error":
                return 529;
            case "rate_limit_error":
                return 429;
            case "request_too_large":
                return 413;
            case "authentication_error":
                return 401;
            case "permission_error":
                return 403;
            case "not_found_error":
                return 404;
            case "billing_error":
            case "invalid_request_error":
                return 400;
            default:
                return null;
        }
    }

    /// <summary>True for api, overloaded, and rate-limit errors.</summary>
    public static bool IsRetryable(string? type)
    {
        return type == "api_error" || type == "overloaded_error" || type == "rate_limit_error";
    }
}
