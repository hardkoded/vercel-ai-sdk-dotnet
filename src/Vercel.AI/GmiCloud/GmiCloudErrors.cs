// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.GmiCloud;

/// <summary>Parsed GMI Cloud error object.</summary>
public sealed class GmiCloudErrorData
{
    /// <summary>Creates a parsed error.</summary>
    public GmiCloudErrorData(string message, string? details)
    {
        Message = message ?? string.Empty;
        Details = details;
    }

    /// <summary>Outer <c>error.message</c>.</summary>
    public string Message { get; }

    /// <summary>Nested <c>error.details</c> string, when present.</summary>
    public string? Details { get; }
}

/// <summary>
/// GMI Cloud nests the engine diagnostic inside <c>error.details</c> as a JSON string.
/// The message callers see is that inner message when it is present.
/// </summary>
public static class GmiCloudErrors
{
    /// <summary>True when the model asks the API to include usage on streamed chunks.</summary>
    public const bool IncludeUsage = true;

    /// <summary>Parses an error body. Plain text and objects without <c>error.message</c> return false.</summary>
    public static bool TryParse(string json, out GmiCloudErrorData? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using (var document = JsonDocument.Parse(json))
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("error", out var body) || body.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                if (!body.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                string? details = null;
                if (body.TryGetProperty("details", out var detailsElement) && detailsElement.ValueKind == JsonValueKind.String)
                {
                    details = detailsElement.GetString();
                }

                error = new GmiCloudErrorData(message.GetString() ?? string.Empty, details);
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>Returns the engine diagnostic inside <c>details</c>, or the outer message.</summary>
    public static string ToMessage(GmiCloudErrorData error)
    {
        if (error == null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        var unwrapped = UnwrapDetails(error.Details);
        return string.IsNullOrEmpty(unwrapped) ? error.Message : unwrapped!;
    }

    /// <summary>Rewrites <c>error.message</c> to the unwrapped diagnostic when <c>details</c> carries one.</summary>
    public static string Rewrite(string body)
    {
        if (string.IsNullOrWhiteSpace(body) || !TryParse(body, out var error) || error == null)
        {
            return body ?? string.Empty;
        }

        var message = ToMessage(error);
        if (message == error.Message)
        {
            return body;
        }

        try
        {
            var node = JsonNode.Parse(body) as JsonObject;
            if (node?["error"] is JsonObject errorNode)
            {
                errorNode["message"] = message;
                return node.ToJsonString();
            }
        }
        catch (JsonException)
        {
        }

        return body;
    }

    private static string? UnwrapDetails(string? details)
    {
        if (string.IsNullOrEmpty(details))
        {
            return null;
        }

        try
        {
            using (var document = JsonDocument.Parse(details))
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("error", out var error) || error.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                if (!error.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.String)
                {
                    return null;
                }

                var text = message.GetString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                return text;
            }
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>Replaces GMI error messages before the chat client reads the body.</summary>
internal sealed class GmiCloudErrorRewriter : HttpMessageHandler
{
    private readonly HttpClient _inner;

    /// <summary>Creates a rewriter that forwards to <paramref name="inner"/>.</summary>
    public GmiCloudErrorRewriter(HttpClient inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await _inner.SendAsync(await CloneAsync(request).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var body = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var rewritten = GmiCloudErrors.Rewrite(body);
        var next = new HttpResponseMessage(response.StatusCode)
        {
            Content = new StringContent(rewritten ?? string.Empty, System.Text.Encoding.UTF8, "application/json"),
            RequestMessage = response.RequestMessage,
        };
        response.Dispose();
        return next;
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content != null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = content;
        }

        return clone;
    }
}
