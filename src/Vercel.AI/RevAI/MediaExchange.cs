// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;

namespace Vercel.AI.RevAI;

/// <summary>One HTTP response captured from a provider call.</summary>
internal sealed class MediaResponse
{
    /// <summary>Creates a response.</summary>
    public MediaResponse(int statusCode, byte[] body, Dictionary<string, string> headers)
    {
        StatusCode = statusCode;
        Body = body ?? Array.Empty<byte>();
        Headers = headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>HTTP status code.</summary>
    public int StatusCode { get; }

    /// <summary>Raw body bytes.</summary>
    public byte[] Body { get; }

    /// <summary>Response headers with lowercase names.</summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>Body decoded as UTF-8.</summary>
    public string Text
    {
        get { return Encoding.UTF8.GetString(Body); }
    }
}

/// <summary>Sends provider HTTP calls and keeps response headers.</summary>
internal static class MediaExchange
{
    /// <summary>Sends a request and returns the status, body, and headers.</summary>
    public static async Task<MediaResponse> SendAsync(
        HttpClient http,
        HttpMethod method,
        Uri uri,
        HttpContent? content,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        using (var request = new HttpRequestMessage(method, uri))
        {
            Apply(request, headers);
            request.Content = content;
            using (var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                var bytes = response.Content == null
                    ? Array.Empty<byte>()
                    : await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return new MediaResponse((int)response.StatusCode, bytes, Copy(response, bytes.Length));
            }
        }
    }

    /// <summary>JSON body whose content type is exactly <c>application/json</c>.</summary>
    public static HttpContent Json(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json ?? string.Empty);
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }

    private static void Apply(HttpRequestMessage request, IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var pair in headers)
        {
            if (string.IsNullOrEmpty(pair.Value))
            {
                continue;
            }

            if (pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var value = pair.Value!;
                var space = value.IndexOf(' ');
                if (space > 0)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        value.Substring(0, space),
                        value.Substring(space + 1));
                }
                else
                {
                    request.Headers.TryAddWithoutValidation("Authorization", value);
                }

                continue;
            }

            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }
    }

    private static Dictionary<string, string> Copy(HttpResponseMessage response, int bodyLength)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
            }
        }

        if (!headers.ContainsKey("content-length"))
        {
            headers["content-length"] = bodyLength.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return headers;
    }
}
