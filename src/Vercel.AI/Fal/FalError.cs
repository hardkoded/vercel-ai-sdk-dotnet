// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Fal;

/// <summary>Fal error bodies, like upstream's <c>fal-error.ts</c>: <c>{error: {message, code}}</c>.</summary>
internal static class FalError
{
    /// <summary>Port of <c>falFailedResponseHandler</c>.</summary>
    public static ApiException FailedResponseHandler(int statusCode, string body)
    {
        return ProviderHttp.MapStatus(statusCode, ErrorToMessage(statusCode, body), body);
    }

    private static string ErrorToMessage(int statusCode, string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (
                document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String
                && error.TryGetProperty("code", out var code)
                && code.ValueKind == JsonValueKind.Number
            )
            {
                return message.GetString()!;
            }
        }
        catch (JsonException)
        {
            // Falls through to the shared extraction.
        }

        return JsonValues.ExtractErrorMessage(body, statusCode);
    }
}
