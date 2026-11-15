// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>Parsed Anthropic error body. Unknown fields are dropped. <c>details</c> keeps <c>error_code</c> and <c>unsupported</c>.</summary>
public sealed class AnthropicErrorData
{
    /// <summary>Creates a parsed error.</summary>
    public AnthropicErrorData(bool success, string? type, string? errorType, string? message, string rawValue, JsonObject? details = null, bool hasDetails = false)
    {
        Details = details;
        HasDetails = hasDetails;
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

    /// <summary>
    /// The <c>error.details</c> object with only <c>error_code</c> and <c>unsupported</c>, for example
    /// <c>thread_not_found</c>. <see langword="null"/> when the body has no details or they have an unknown shape.
    /// </summary>
    public JsonObject? Details { get; }

    /// <summary>True when <c>error.details</c> is present as an object or as <c>null</c>.</summary>
    public bool HasDetails { get; }

    /// <summary>The <c>error.details.error_code</c> value, such as <c>thread_not_found</c>.</summary>
    public string? ErrorCode
    {
        get { return Details != null && Details["error_code"] is JsonValue value && value.TryGetValue<string>(out var code) ? code : null; }
    }

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

            JsonObject? details = null;
            var hasDetails = false;
            if (error.TryGetProperty("details", out var detailsElement))
            {
                if (detailsElement.ValueKind == JsonValueKind.Null)
                {
                    hasDetails = true;
                }
                else if (detailsElement.ValueKind == JsonValueKind.Object)
                {
                    hasDetails = true;
                    details = new JsonObject();
                    if (detailsElement.TryGetProperty("error_code", out var code))
                    {
                        if (code.ValueKind == JsonValueKind.String)
                        {
                            details["error_code"] = code.GetString();
                        }
                        else if (code.ValueKind == JsonValueKind.Null)
                        {
                            details["error_code"] = null;
                        }
                    }

                    if (detailsElement.TryGetProperty("unsupported", out var unsupported))
                    {
                        if (unsupported.ValueKind == JsonValueKind.Null)
                        {
                            details["unsupported"] = null;
                        }
                        else if (unsupported.ValueKind == JsonValueKind.Array && unsupported.EnumerateArray().All(item => item.ValueKind == JsonValueKind.String))
                        {
                            var items = new JsonArray();
                            foreach (var item in unsupported.EnumerateArray())
                            {
                                items.Add(item.GetString());
                            }

                            details["unsupported"] = items;
                        }
                    }
                }
            }

            return new AnthropicErrorData(true, "error", AnthropicJson.String(error, "type"), AnthropicJson.String(error, "message"), json, details, hasDetails);
        }
        catch (JsonException)
        {
            return new AnthropicErrorData(false, null, null, null, json);
        }
    }

    /// <summary>The validated value with <c>type</c>, <c>error.type</c>, <c>error.message</c>, and <c>error.details</c> when present.</summary>
    public JsonObject? Value()
    {
        if (!Success)
        {
            return null;
        }

        var error = new JsonObject
        {
            ["type"] = ErrorType,
            ["message"] = Message,
        };
        if (HasDetails)
        {
            error["details"] = Details?.DeepClone();
        }

        return new JsonObject
        {
            ["type"] = Type,
            ["error"] = error,
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
