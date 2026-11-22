// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Parallel;

/// <summary>Maps to <c>parallelErrorSchema</c> and the handler built from it: reads <c>error.message</c> of a failed Parallel response.</summary>
internal static class ParallelError
{
    internal static ApiCallError FailedResponse(
        string url,
        object requestBody,
        int status,
        string? statusText,
        IReadOnlyDictionary<string, string> headers,
        string text
    )
    {
        var message = statusText ?? string.Empty;
        object? data = null;
        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;
            if (
                root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("message", out var detail)
                && detail.ValueKind == JsonValueKind.String
            )
            {
                message = detail.GetString()!;
                data = root.Clone();
            }
        }
        catch (JsonException) { }

        return new ApiCallError(message, url, requestBody, status, headers, text, data: data);
    }
}
