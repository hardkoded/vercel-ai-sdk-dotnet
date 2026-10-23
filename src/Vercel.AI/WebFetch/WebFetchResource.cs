// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Sockets;
using Vercel.AI.Util;

namespace Vercel.AI.WebFetch;

internal sealed class WebFetchResourceResult
{
    private WebFetchResourceResult()
    {
    }

    public bool IsSuccess { get; private init; }

    public string FinalUrl { get; private init; } = string.Empty;

    public int StatusCode { get; private init; }

    public string? ContentType { get; private init; }

    public byte[] Body { get; private init; } = Array.Empty<byte>();

    public string ErrorCode { get; private init; } = string.Empty;

    public string Message { get; private init; } = string.Empty;

    public int? ErrorStatusCode { get; private init; }

    public static WebFetchResourceResult Success(string finalUrl, int statusCode, string? contentType, byte[] body)
    {
        return new WebFetchResourceResult { IsSuccess = true, FinalUrl = finalUrl, StatusCode = statusCode, ContentType = contentType, Body = body };
    }

    public static WebFetchResourceResult Error(string errorCode, string message, int? statusCode = null)
    {
        return new WebFetchResourceResult { ErrorCode = errorCode, Message = message, ErrorStatusCode = statusCode };
    }
}

/// <summary>Fetches an untrusted URL with a GET request. Maps to <c>fetch-web-resource.ts</c>.</summary>
internal static class WebFetchResource
{
    private const int MaxRedirects = 5;

    // Error messages never include URLs. The model already knows the URL it requested, and repeating model-provided input
    // in results lets the model place arbitrary text into the conversation.

    /// <summary>
    /// Redirects are followed manually so that every hop is checked against the domain filter and the private-address guard
    /// before it is requested. A custom fetch must provide equivalent validation of resolved addresses.
    /// </summary>
    public static async Task<WebFetchResourceResult> FetchAsync(
        string url,
        HeaderCollection headers,
        CancellationToken abortSignal,
        long maxBytes,
        DownloadFetch? fetch,
        Func<Uri, bool> isDomainAllowed)
    {
        var currentUrl = url;

        for (var redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
        {
            if (!isDomainAllowed(new Uri(currentUrl)))
            {
                return WebFetchResourceResult.Error(
                    "url_not_allowed",
                    redirectCount == 0 ? "The domain of the URL is not allowed." : "The URL redirected to a domain that is not allowed.");
            }

            try
            {
                Download.ValidateDownloadUrl(currentUrl);
            }
            catch (DownloadError)
            {
                return WebFetchResourceResult.Error(
                    "url_not_allowed",
                    redirectCount == 0
                        ? "The URL points to a local, private, or reserved address, which is not allowed."
                        : "The URL redirected to a local, private, or reserved address, which is not allowed.");
            }

            DownloadResponse response;
            try
            {
                response = await Download.FetchWithValidatedEndpointAsync(
                    currentUrl,
                    new DownloadRequest(headers, abortSignal, "manual") { MaxBytes = maxBytes },
                    fetch,
                    redirect: "manual").ConfigureAwait(false);
            }
            catch (Exception error) when (!abortSignal.IsCancellationRequested)
            {
                // The fetch rejects hostnames that resolve to private addresses with a DownloadError.
                if (FindCause(error, DownloadError.IsInstance) != null)
                {
                    return WebFetchResourceResult.Error("url_not_allowed", "The URL resolves to a private or reserved address, which is not allowed.");
                }

                return WebFetchResourceResult.Error("network_error", "The request failed" + FormatErrorCode(error) + ".");
            }

            if (response.Type == "opaqueredirect")
            {
                response.Cancel();
                return WebFetchResourceResult.Error("network_error", "A redirect could not be validated and was blocked.");
            }

            var location = response.Headers.Get("location");
            if (IsRedirectStatus(response.Status) && !string.IsNullOrEmpty(location))
            {
                response.Cancel();
                currentUrl = new Uri(new Uri(currentUrl), location).AbsoluteUri;
                continue;
            }

            if (!response.Ok)
            {
                response.Cancel();

                var challenged = response.Headers.Get("cf-mitigated") == "challenge";
                return WebFetchResourceResult.Error(
                    "http_error",
                    challenged
                        ? "The request was blocked by bot protection (HTTP " + response.Status + ")."
                        : "The request failed with HTTP " + response.Status + (string.IsNullOrEmpty(response.StatusText) ? string.Empty : " " + response.StatusText) + ".",
                    response.Status);
            }

            var body = response.Body ?? Array.Empty<byte>();
            long contentLength;
            var declared = response.Headers.Get("content-length");
            if ((declared != null && long.TryParse(declared, NumberStyles.Integer, CultureInfo.InvariantCulture, out contentLength) && contentLength > maxBytes)
                || body.LongLength > maxBytes)
            {
                response.Cancel();
                return WebFetchResourceResult.Error("too_large", "The response exceeds the maximum size of " + maxBytes.ToString(CultureInfo.InvariantCulture) + " bytes.");
            }

            return WebFetchResourceResult.Success(currentUrl, response.Status, response.Headers.Get("content-type"), body);
        }

        return WebFetchResourceResult.Error("too_many_redirects", "The URL exceeded the maximum of " + MaxRedirects + " redirects.");
    }

    private static bool IsRedirectStatus(int status)
    {
        // Redirect status codes per the fetch spec (https://fetch.spec.whatwg.org/#redirect-status).
        return status == 301 || status == 302 || status == 303 || status == 307 || status == 308;
    }

    // Returns the error or the first of its causes that matches the predicate. Fetch implementations wrap the underlying error.
    private static object? FindCause(Exception? error, Func<object?, bool> predicate)
    {
        var current = error;
        for (var depth = 0; current != null && depth < 5; current = current.InnerException, depth++)
        {
            if (predicate(current))
            {
                return current;
            }
        }

        return null;
    }

    // Formats a socket error such as `HostNotFound`, which explains a network failure without repeating the URL.
    private static string FormatErrorCode(Exception error)
    {
        var cause = FindCause(error, candidate => candidate is SocketException) as SocketException;
        return cause == null ? string.Empty : " (" + cause.SocketErrorCode + ")";
    }
}
