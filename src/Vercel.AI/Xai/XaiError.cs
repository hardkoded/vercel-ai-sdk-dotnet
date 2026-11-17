// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>xAI error bodies, like upstream's <c>xai-error.ts</c>: <c>{error: {message}}</c>, <c>{code, error}</c> and <c>{error: string}</c>.</summary>
internal static class XaiError
{
    /// <summary>Port of <c>xaiFailedResponseHandler</c>.</summary>
    public static ApiException FailedResponseHandler(int statusCode, string body)
    {
        return ProviderHttp.MapStatus(statusCode, ErrorToMessage(body, statusCode), body);
    }

    private static string ErrorToMessage(string body, int statusCode)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (
                root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.String
                && error.GetString() is { } text
            )
            {
                return
                    root.TryGetProperty("code", out var code)
                    && code.ValueKind == JsonValueKind.String
                    ? code.GetString() + ": " + text
                    : text;
            }
        }
        catch (JsonException)
        {
            // Falls through to the shared extraction.
        }

        return JsonValues.ExtractErrorMessage(body, statusCode);
    }
}
