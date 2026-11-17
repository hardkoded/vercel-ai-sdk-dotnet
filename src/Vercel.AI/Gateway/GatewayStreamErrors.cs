// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Gateway;

/// <summary>Error mapping for the Gateway streaming WebSocket surfaces. Maps to <c>gateway-stream-errors.ts</c>.</summary>
internal static class GatewayStreamErrors
{
    /// <summary>Canonical status codes for server error-part types (no HTTP status exists on the WebSocket).</summary>
    private static readonly Dictionary<string, int> ServerErrorStatusCodes = new Dictionary<
        string,
        int
    >(StringComparer.Ordinal)
    {
        ["authentication_error"] = 401,
        ["failed_dependency"] = 424,
        ["forbidden"] = 403,
        ["internal_server_error"] = 500,
        ["invalid_request_error"] = 400,
        ["model_not_found"] = 404,
        ["rate_limit_exceeded"] = 429,
    };

    /// <summary>
    /// Fails <paramref name="controller"/> with <paramref name="error"/> mapped to a Gateway error. Cancellation
    /// errors pass through unchanged, as upstream's <c>asGatewayError</c> rethrows them. Maps to <c>errorControllerWithGatewayError</c>.
    /// </summary>
    public static void ErrorControllerWithGatewayError<TPart>(
        PartQueue<TPart> controller,
        Exception error,
        string? authMethod
    )
    {
        controller.Complete(
            IsCancellation(error) ? error : GatewayErrors.AsGatewayError(error, authMethod)
        );
    }

    /// <summary>
    /// Maps a server error-part payload (<c>{ message, type }</c>) to the public Gateway error class for its type;
    /// unknown shapes keep the generic message. Maps to <c>createErrorFromServerErrorPart</c>.
    /// </summary>
    public static Exception CreateErrorFromServerErrorPart(
        JsonElement? error,
        string? authMethod,
        string streamLabel = "transcription"
    )
    {
        if (
            error is { ValueKind: JsonValueKind.Object } payload
            && payload.TryGetProperty("message", out var message)
            && message.ValueKind == JsonValueKind.String
            && payload.TryGetProperty("type", out var type)
            && type.ValueKind == JsonValueKind.String
            && ServerErrorStatusCodes.TryGetValue(type.GetString()!, out var statusCode)
        )
        {
            var response = new JsonObject
            {
                ["error"] = new JsonObject
                {
                    ["message"] = message.GetString(),
                    ["type"] = type.GetString(),
                },
            };
            return GatewayErrors.Create(
                JsonSerializer.SerializeToElement(response),
                statusCode,
                authMethod
            );
        }

        return new AiSdkException(
            "AI Gateway " + streamLabel + " stream failed: " + GetServerErrorMessage(error)
        );
    }

    private static bool IsCancellation(Exception error)
    {
        return error is OperationCanceledException || error is TimeoutError;
    }

    // Extracts a human-readable message from an `error` stream part's payload for the terminal close error.
    private static string GetServerErrorMessage(JsonElement? error)
    {
        if (error is not { } payload)
        {
            return "unknown error";
        }

        if (
            payload.ValueKind == JsonValueKind.Object
            && payload.TryGetProperty("message", out var message)
            && message.ValueKind == JsonValueKind.String
        )
        {
            return message.GetString()!;
        }

        // JSON-stringifies object payloads (a plain ToString would not show the payload).
        return payload.ValueKind switch
        {
            JsonValueKind.String => payload.GetString()!,
            JsonValueKind.Null or JsonValueKind.Undefined => "unknown error",
            _ => JsonSerializer.Serialize(payload),
        };
    }
}
