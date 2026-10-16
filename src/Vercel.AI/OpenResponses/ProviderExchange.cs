// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI;

/// <summary>One HTTP round-trip, including response headers.</summary>
internal sealed class ProviderExchangeResult
{
    /// <summary>Creates a completed exchange.</summary>
    public ProviderExchangeResult(string body, byte[] bytes, IReadOnlyDictionary<string, string> headers)
    {
        Body = body ?? string.Empty;
        Bytes = bytes ?? Array.Empty<byte>();
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Response body decoded as UTF-8.</summary>
    public string Body { get; }

    /// <summary>Raw response bytes.</summary>
    public byte[] Bytes { get; }

    /// <summary>Response headers, including content headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Sends one provider request and keeps the response headers.</summary>
internal static class ProviderExchange
{
    private const long MaxErrorBodyBytes = 1024 * 1024;

    /// <summary>Sends <paramref name="content"/> and returns the body when the status is successful.</summary>
    public static async Task<ProviderExchangeResult> SendAsync(
        HttpClient httpClient,
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
            HttpResponseMessage response;
            try
            {
                response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
            {
                throw new ApiUserAbortException(exception, cancellationToken);
            }

            try
            {
                var bytes = response.Content == null
                    ? Array.Empty<byte>()
                    : await CancellableReads.ReadAsByteArrayAsync(response.Content, cancellationToken).ConfigureAwait(false);
                var body = Encoding.UTF8.GetString(bytes);
                if (!response.IsSuccessStatusCode)
                {
                    throw ProviderHttp.MapStatus((int)response.StatusCode, body);
                }

                return new ProviderExchangeResult(body, bytes, Copy(response));
            }
            finally
            {
                response.Dispose();
            }
        }
    }

    /// <summary>
    /// Gets a URL that came from response text. A URL off <paramref name="trustedOrigin"/> must pass <paramref name="validateUrl"/>
    /// and gets no <paramref name="trustedHeaders"/>. When the client follows a redirect, the final URL is checked too.
    /// The body is read in chunks and stops at the download size limit.
    /// </summary>
    public static async Task<ProviderExchangeResult> GetResponseUrlAsync(
        HttpClient httpClient,
        string url,
        string? trustedOrigin,
        Action<string> validateUrl,
        IReadOnlyDictionary<string, string?>? trustedHeaders,
        CancellationToken cancellationToken)
    {
        var trusted = trustedOrigin != null && ProviderValues.IsSameOrigin(url, trustedOrigin);
        if (!trusted)
        {
            validateUrl(url);
        }

        var uri = new Uri(url);
        using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
        {
            Apply(request, trusted ? trustedHeaders : null);
            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                var finalUri = response.RequestMessage?.RequestUri;
                if (finalUri != null
                    && finalUri != uri
                    && (trustedOrigin == null || !ProviderValues.IsSameOrigin(finalUri.AbsoluteUri, trustedOrigin)))
                {
                    validateUrl(finalUri.AbsoluteUri);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = string.Empty;
                    try
                    {
                        errorBody = Encoding.UTF8.GetString(await ResponseBodies.ReadResponseWithSizeLimitAsync(response, url, MaxErrorBodyBytes, cancellationToken).ConfigureAwait(false));
                    }
                    catch (DownloadError)
                    {
                        // An oversized error body is dropped. The status is the error.
                    }

                    throw ProviderHttp.MapStatus((int)response.StatusCode, errorBody);
                }

                var bytes = await ResponseBodies.ReadResponseWithSizeLimitAsync(response, url, null, cancellationToken).ConfigureAwait(false);
                return new ProviderExchangeResult(Encoding.UTF8.GetString(bytes), bytes, Copy(response));
            }
        }
    }

    /// <summary>JSON content whose media type is <c>application/json</c> without a charset parameter.</summary>
    public static StringContent Json(string json)
    {
        var content = new StringContent(json ?? string.Empty, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }

    /// <summary>Copies provider headers and lets the call override them.</summary>
    public static Dictionary<string, string?> Merge(
        IReadOnlyDictionary<string, string?>? providerHeaders,
        IEnumerable<KeyValuePair<string, string>>? requestHeaders)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (providerHeaders != null)
        {
            foreach (var pair in providerHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        if (requestHeaders != null)
        {
            foreach (var pair in requestHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    /// <summary>Reads <c>error.message</c> and <c>error.code</c>.</summary>
    public static bool TryParseError(string json, out string message, out int code)
    {
        message = string.Empty;
        code = 0;
        try
        {
            using (var document = System.Text.Json.JsonDocument.Parse(json))
            {
                if (!document.RootElement.TryGetProperty("error", out var error)
                    || error.ValueKind != System.Text.Json.JsonValueKind.Object
                    || !error.TryGetProperty("message", out var messageValue)
                    || messageValue.ValueKind != System.Text.Json.JsonValueKind.String
                    || !error.TryGetProperty("code", out var codeValue)
                    || codeValue.ValueKind != System.Text.Json.JsonValueKind.Number)
                {
                    return false;
                }

                message = messageValue.GetString() ?? string.Empty;
                code = codeValue.GetInt32();
                return true;
            }
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }

    /// <summary><c>ai-sdk/{provider}/{version}</c>.</summary>
    public static string UserAgent(string provider)
    {
        return "ai-sdk/" + provider + "/" + AiSdkVersion.Version;
    }

    /// <summary>Adds the SDK user agent when the caller did not set one.</summary>
    public static void AddUserAgent(IDictionary<string, string?> headers, string provider)
    {
        foreach (var key in headers.Keys)
        {
            if (string.Equals(key, "User-Agent", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        headers["User-Agent"] = UserAgent(provider);
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

    private static Dictionary<string, string> Copy(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }
}
