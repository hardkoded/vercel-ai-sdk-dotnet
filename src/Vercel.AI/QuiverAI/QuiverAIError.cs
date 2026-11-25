// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenResponses;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.QuiverAI;

/// <summary>QuiverAI error bodies, like upstream's <c>quiverai-error.ts</c>: <c>{status, code, message, request_id}</c>.</summary>
internal static class QuiverAIError
{
    /// <summary>Maps to <c>isRetryable</c> of <c>quiveraiFailedResponseHandler</c>: only 429 and 5xx.</summary>
    internal static bool IsRetryable(int statusCode)
    {
        return statusCode == 429 || statusCode >= 500;
    }

    /// <summary>The message of a QuiverAI error body. Maps to the <c>errorToMessage</c> of <c>quiveraiFailedResponseHandler</c>.</summary>
    internal static string ErrorToMessage(int statusCode, string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (
                root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("status", out var status)
                && status.ValueKind == JsonValueKind.Number
                && status.TryGetInt32(out _)
                && Text(root, "code") is not null
                && Text(root, "request_id") is not null
                && Text(root, "message") is { } message
            )
            {
                return message;
            }
        }
        catch (JsonException)
        {
            // Falls through to the shared extraction.
        }

        return JsonValues.ExtractErrorMessage(body, statusCode);
    }

    /// <summary>Maps to <c>getQuiverAIResponseErrorMetadata</c>.</summary>
    internal static OpenResponsesErrorMetadata? GetQuiverAIResponseErrorMetadata(JsonElement error)
    {
        int? statusCode = null;
        if (
            error.ValueKind == JsonValueKind.Object
            && error.TryGetProperty("status_code", out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var code)
            && code >= 400
            && code <= 599
        )
        {
            statusCode = code;
        }

        return new OpenResponsesErrorMetadata { StatusCode = statusCode };
    }

    private static string? Text(JsonElement root, string name)
    {
        return
            root.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            && value.GetString() is { Length: > 0 } text
            ? text
            : null;
    }
}
