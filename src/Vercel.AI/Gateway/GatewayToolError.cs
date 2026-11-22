// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vercel.AI.Gateway;

/// <summary>
/// Error returned as the output of a Gateway provider-executed tool instead of a result. Maps to <c>GatewayToolError</c>.
/// </summary>
public sealed class GatewayToolError
{
    /// <summary>
    /// Error code: <c>api_error</c>, <c>configuration_error</c>, <c>execution_error</c>, <c>gateway_rate_limit</c>,
    /// <c>invalid_input</c>, <c>rate_limit</c>, <c>timeout</c>, <c>unknown</c> or <c>unknown_tool</c>.
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = "";

    /// <summary>Error message.</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    /// <summary>HTTP status code, when the error came from an HTTP response.</summary>
    [JsonPropertyName("statusCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? StatusCode { get; set; }

    /// <summary>Retry-After value of a rate limit response.</summary>
    [JsonPropertyName("retryAfter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RetryAfter { get; set; }

    /// <summary>HTTP status text.</summary>
    [JsonPropertyName("statusText")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StatusText { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

internal static class GatewayToolErrorShape
{
    private static readonly string[] Codes =
    [
        "api_error",
        "configuration_error",
        "execution_error",
        "gateway_rate_limit",
        "invalid_input",
        "rate_limit",
        "timeout",
        "unknown",
        "unknown_tool",
    ];

    /// <summary>True when the object has a known <c>error</c> code and a string <c>message</c>. Maps to <c>gatewayToolErrorSchema</c>.</summary>
    public static bool Matches(JsonElement root) =>
        root.TryGetProperty("error", out var code)
        && code.ValueKind == JsonValueKind.String
        && Array.IndexOf(Codes, code.GetString()) >= 0
        && root.TryGetProperty("message", out var message)
        && message.ValueKind == JsonValueKind.String;
}
