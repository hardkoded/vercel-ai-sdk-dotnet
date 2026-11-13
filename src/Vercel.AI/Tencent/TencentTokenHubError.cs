// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Tencent;

/// <summary>TokenHub error handling. Upstream <c>tencent-tokenhub-error.ts</c>.</summary>
internal static class TencentTokenHubError
{
    /// <summary>
    /// Message of a failed TokenHub response: <c>{ code, message }</c> or <c>{ error: { message } }</c>. Null when the body is neither.
    /// Upstream <c>tencentTokenHubErrorDataSchema</c> with <c>errorToMessage</c>.
    /// </summary>
    internal static string? ReadErrorMessage(string? body)
    {
        try
        {
            using var document = JsonDocument.Parse(body ?? string.Empty);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
            {
                return error.TryGetProperty("message", out var nested) && nested.ValueKind == JsonValueKind.String ? nested.GetString() : null;
            }

            return root.TryGetProperty("code", out var code) && code.ValueKind is JsonValueKind.Number or JsonValueKind.String
                && root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String
                    ? message.GetString()
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// TokenHub's Kling endpoints follow the Kling envelope, where a non-zero <c>code</c> marks a failed request even when the HTTP status is 2xx.
    /// Throws when it does. Upstream <c>createTencentTokenHubResponseHandler</c>.
    /// </summary>
    internal static JsonElement ReadResponse(ProviderExchangeResult response, string url)
    {
        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, response.Body) { Url = url, ResponseHeaders = response.Headers };
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new ApiException("Invalid JSON response", 200, response.Body) { Url = url, ResponseHeaders = response.Headers };
        }

        if (root.TryGetProperty("code", out var code) && code.ValueKind != JsonValueKind.Null)
        {
            var failed = code.ValueKind switch
            {
                JsonValueKind.Number => code.GetDouble() != 0,
                JsonValueKind.String => code.GetString() != "0",
                _ => throw new ApiException("Invalid JSON response", 200, response.Body) { Url = url, ResponseHeaders = response.Headers },
            };
            if (failed)
            {
                var message = root.TryGetProperty("message", out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
                throw new ApiException(message ?? "TokenHub request failed with code " + code.ToString(), 200, response.Body)
                {
                    Url = url,
                    ResponseHeaders = response.Headers,
                    IsRetryable = false,
                    ErrorData = root,
                };
            }
        }

        return root;
    }
}
