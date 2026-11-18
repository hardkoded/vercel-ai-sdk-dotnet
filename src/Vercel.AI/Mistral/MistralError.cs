// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Maps failed Mistral responses to errors. Maps to <c>mistralFailedResponseHandler</c>.</summary>
internal static class MistralError
{
    /// <summary>Builds the error for a failed response. The message is the Mistral <c>message</c> field when the body has one.</summary>
    public static ApiException FailedResponse(
        int statusCode,
        string body,
        Uri url,
        object? requestBodyValues
    )
    {
        var message = Message(body) ?? JsonValues.ExtractErrorMessage(body, statusCode);
        return ProviderHttp.MapStatusWithHeaders(
            statusCode,
            message,
            body,
            null,
            url.AbsoluteUri,
            requestBodyValues
        );
    }

    private static string? Message(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            return
                root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("object", out var kind)
                && kind.ValueKind == JsonValueKind.String
                && kind.GetString() == "error"
                && root.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String
                ? message.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
