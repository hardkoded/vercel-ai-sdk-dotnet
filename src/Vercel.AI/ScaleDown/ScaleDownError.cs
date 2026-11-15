// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.ScaleDown;

/// <summary>Maps to <c>scaledownFailedResponseHandler</c>: reads the <c>detail</c> of a failed ScaleDown response.</summary>
internal static class ScaleDownError
{
    internal static ApiCallError FailedResponse(string url, object requestBody, int status, string? statusText, IReadOnlyDictionary<string, string> headers, string text)
    {
        var message = statusText ?? string.Empty;
        object? data = null;
        try
        {
            using var document = JsonDocument.Parse(text);
            if (TryReadDetail(document.RootElement, out var detail))
            {
                message = detail;
                data = document.RootElement.Clone();
            }
        }
        catch (JsonException)
        {
        }

        return new ApiCallError(message, url, requestBody, status, headers, text, data: data);
    }

    // The schema is { detail: string | { msg: string }[] }. A body that does not fit it falls back to the status text.
    private static bool TryReadDetail(JsonElement root, out string message)
    {
        message = string.Empty;
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("detail", out var detail))
        {
            return false;
        }

        if (detail.ValueKind == JsonValueKind.String)
        {
            message = detail.GetString()!;
            return true;
        }

        if (detail.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var messages = new List<string>();
        foreach (var item in detail.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("msg", out var msg) || msg.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            messages.Add(msg.GetString()!);
        }

        message = string.Join(", ", messages);
        return true;
    }
}
