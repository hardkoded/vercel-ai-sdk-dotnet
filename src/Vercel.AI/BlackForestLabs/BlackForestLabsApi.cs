// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.BlackForestLabs;

/// <summary>Request, response, and trust helpers that the Black Forest Labs image and video models share.</summary>
internal static class BlackForestLabsApi
{
    internal static readonly string[] TerminalStatuses =
    {
        "Content Moderated", "Error", "Failed", "Request Moderated", "Task not found",
    };

    /// <summary>
    /// Black Forest Labs returns polling URLs on sibling cluster hosts of the API origin (for example <c>api.us1.bfl.ai</c>
    /// for a base URL on <c>api.bfl.ai</c>), so a strict same-origin check is not enough. Credentials may also go to
    /// any https host under the official <c>bfl.ai</c> domain.
    /// </summary>
    internal static bool IsTrustedUrl(string url, string baseUrl)
    {
        if (ProviderValues.IsSameOrigin(url, baseUrl))
        {
            return true;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            return false;
        }

        return parsed.Scheme == "https"
            && (parsed.Host == "bfl.ai" || parsed.Host.EndsWith(".bfl.ai", StringComparison.OrdinalIgnoreCase));
    }

    internal static ApiException Failed(ApiException exception, string url)
    {
        var body = exception.ResponseBody;
        string? message = null;
        string? status = null;
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                status = StringProperty(root, "status") ?? StringProperty(root, "state");
                if (root.TryGetProperty("detail", out var detail) && detail.ValueKind != JsonValueKind.Null)
                {
                    message = detail.ValueKind == JsonValueKind.String ? detail.GetString() : JsonNode.Parse(detail.GetRawText())!.ToJsonString();
                }
                else
                {
                    message = StringProperty(root, "message");
                }

                message ??= status != null ? "Black Forest Labs generation failed: " + status + "." : "Unknown Black Forest Labs error";
            }
        }
        catch (JsonException)
        {
            // Not a Black Forest Labs error body. The HTTP failure message stays.
        }

        var retryable = Array.IndexOf(TerminalStatuses, status ?? string.Empty) < 0
            && (exception.StatusCode == 408 || exception.StatusCode == 409 || exception.StatusCode == 429 || exception.StatusCode >= 500);
        return new ApiException(message ?? exception.Message, exception.StatusCode, body)
        {
            Url = url,
            IsRetryable = retryable,
            ResponseHeaders = exception.ResponseHeaders,
        };
    }

    internal static JsonDocument ParseSubmit(string text)
    {
        var document = Parse(text);
        var root = document.RootElement;
        var valid = root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
            && root.TryGetProperty("polling_url", out var pollingUrl) && IsUrl(pollingUrl)
            && NullableNumber(root, "cost") && NullableNumber(root, "input_mp") && NullableNumber(root, "output_mp");
        if (!valid)
        {
            document.Dispose();
            throw new InvalidResponseDataException(text, "Invalid Black Forest Labs submit response");
        }

        return document;
    }

    internal static JsonDocument Parse(string text)
    {
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(text) ? "{}" : text);
        }
        catch (JsonException exception)
        {
            throw new InvalidResponseDataException(text, "Invalid JSON response: " + exception.Message);
        }
    }

    internal static bool IsUrl(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String && Uri.TryCreate(value.GetString(), UriKind.Absolute, out _);
    }

    internal static bool NullableNumber(JsonElement root, string name)
    {
        return !root.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Number;
    }

    internal static string? StringProperty(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    internal static JsonElement? Number(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.Clone() : null;
    }

    internal static Dictionary<string, string?> UserAgentOnly(IReadOnlyDictionary<string, string?> headers)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (headers.TryGetValue("User-Agent", out var userAgent))
        {
            result["User-Agent"] = userAgent;
        }

        return result;
    }

    /// <summary>Adds <c>id=</c> to a polling URL that lacks one.</summary>
    internal static Uri WithRequestId(string pollUrl, string requestId)
    {
        var builder = new UriBuilder(pollUrl);
        var query = builder.Query.TrimStart('?');
        if (!query.Split('&').Any(part => part == "id" || part.StartsWith("id=", StringComparison.Ordinal)))
        {
            builder.Query = (query.Length == 0 ? string.Empty : query + "&") + "id=" + Uri.EscapeDataString(requestId);
        }

        return builder.Uri;
    }
}
