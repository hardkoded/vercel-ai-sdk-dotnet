// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Vercel.AI.Util;

/// <summary>Bytes downloaded from a URL. Maps to the object returned by <c>download</c>.</summary>
public sealed class DownloadResult
{
    /// <summary>Creates a download result.</summary>
    public DownloadResult(byte[] data, string? mediaType)
    {
        Data = data;
        MediaType = mediaType;
    }

    /// <summary>Response body.</summary>
    public byte[] Data { get; }

    /// <summary>Content type without parameters, when the response sent one.</summary>
    public string? MediaType { get; }
}

/// <summary>A URL the default download function may fetch. Maps to one entry passed to a <c>DownloadFunction</c>.</summary>
public sealed class DownloadRequest
{
    /// <summary>Creates a request.</summary>
    public DownloadRequest(Uri url, bool isUrlSupportedByModel)
    {
        Url = url;
        IsUrlSupportedByModel = isUrlSupportedByModel;
    }

    /// <summary>The URL to download or to pass through.</summary>
    public Uri Url { get; }

    /// <summary>When true, the URL is returned as null and is not downloaded.</summary>
    public bool IsUrlSupportedByModel { get; }
}

/// <summary>A download failed. Maps to <c>DownloadError</c>.</summary>
public sealed class DownloadException : Exception
{
    /// <summary>Creates a download error.</summary>
    public DownloadException(string url, int? statusCode = null, string? statusText = null, string? message = null, Exception? cause = null)
        : base(message ?? BuildMessage(url, statusCode, statusText, cause), cause)
    {
        Url = url;
        StatusCode = statusCode;
        StatusText = statusText;
    }

    /// <summary>The URL that failed.</summary>
    public string Url { get; }

    /// <summary>HTTP status, when the response was received.</summary>
    public int? StatusCode { get; }

    /// <summary>HTTP reason phrase, when the response was received.</summary>
    public string? StatusText { get; }

    /// <summary>Returns whether <paramref name="error"/> is a <see cref="DownloadException"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is DownloadException;
    }

    private static string BuildMessage(string url, int? statusCode, string? statusText, Exception? cause)
    {
        if (cause is null)
        {
            return "Failed to download " + url + ": " + statusCode + " " + statusText;
        }

        return "Failed to download " + url + ": " + cause.Message;
    }
}

/// <summary>
/// Downloads a URL through a caller-supplied <see cref="HttpMessageHandler"/>.
/// Maps to <c>download</c> and <c>createDefaultDownloadFunction</c>. Data URLs are decoded locally.
/// </summary>
public static class FileDownload
{
    /// <summary>Default maximum download size: 2 GiB.</summary>
    public const long DefaultMaxDownloadSize = 2L * 1024L * 1024L * 1024L;

    private const int MaxRedirects = 10;

    private static readonly MethodInfo? SendAsyncMethod = typeof(HttpMessageHandler).GetMethod(
        "SendAsync",
        BindingFlags.Instance | BindingFlags.NonPublic,
        null,
        new[] { typeof(HttpRequestMessage), typeof(CancellationToken) },
        null);

    /// <summary>
    /// Downloads <paramref name="url"/>. Private addresses and localhost are rejected.
    /// Redirects are validated before the next request. <paramref name="handler"/> performs HTTP
    /// and is not disposed.
    /// </summary>
    public static async Task<DownloadResult> DownloadAsync(
        Uri url,
        HttpMessageHandler? handler = null,
        long? maxBytes = null,
        CancellationToken cancellationToken = default)
    {
        if (url is null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        var urlText = url.OriginalString;
        var limit = maxBytes ?? DefaultMaxDownloadSize;
        try
        {
            Validate(urlText);
            if (string.Equals(url.Scheme, "data", StringComparison.OrdinalIgnoreCase))
            {
                return ReadDataUrl(url);
            }

            if (handler is null)
            {
                throw new DownloadException(urlText, message: "An HttpMessageHandler is required to download " + urlText + ".");
            }

            var current = url;
            for (var redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
            {
                Validate(current.OriginalString);
                var response = await SendAsync(handler, current, cancellationToken).ConfigureAwait(false);
                Uri? next = null;
                string? location = null;
                if (response.Headers.Location is Uri locationUri)
                {
                    location = locationUri.OriginalString;
                }
                else if (response.Headers.TryGetValues("Location", out var values))
                {
                    foreach (var value in values)
                    {
                        location = value;
                        break;
                    }
                }

                var status = (int)response.StatusCode;
                if (location is not null && IsRedirectStatus(status))
                {
                    CancelBody(response);
                    next = new Uri(current, location);
                    current = next;
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var statusText = response.ReasonPhrase;
                    CancelBody(response);
                    throw new DownloadException(urlText, status, statusText);
                }

                var data = await ReadBodyAsync(response, urlText, limit, cancellationToken).ConfigureAwait(false);
                var mediaType = response.Content is null ? null : response.Content.Headers.ContentType?.MediaType;
                response.Dispose();
                return new DownloadResult(data, mediaType);
            }

            throw new DownloadException(urlText, message: "Too many redirects (max " + MaxRedirects.ToString(CultureInfo.InvariantCulture) + ")");
        }
        catch (Exception ex)
        {
            if (ex is DownloadException)
            {
                throw;
            }

            throw new DownloadException(urlText, cause: ex);
        }
    }

    /// <summary>
    /// Downloads each request the model cannot fetch itself and passes the abort token through.
    /// Supported URLs produce a null entry.
    /// </summary>
    public static Func<IReadOnlyList<DownloadRequest>, Task<DownloadResult?[]>> CreateDefaultDownloadFunction(
        Func<Uri, bool, CancellationToken, Task<DownloadResult>> download,
        CancellationToken cancellationToken = default)
    {
        if (download is null)
        {
            throw new ArgumentNullException(nameof(download));
        }

        return async requests =>
        {
            var results = new DownloadResult?[requests.Count];
            for (var i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                if (request.IsUrlSupportedByModel)
                {
                    results[i] = null;
                }
                else
                {
                    results[i] = await download(request.Url, false, cancellationToken).ConfigureAwait(false);
                }
            }

            return results;
        };
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpMessageHandler handler, Uri url, CancellationToken cancellationToken)
    {
        if (SendAsyncMethod is null)
        {
            throw new InvalidOperationException("HttpMessageHandler.SendAsync is not available.");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent", "ai-sdk/dotnet");
        try
        {
            var invoked = SendAsyncMethod.Invoke(handler, new object[] { request, cancellationToken });
            if (invoked is not Task<HttpResponseMessage> task)
            {
                throw new InvalidOperationException("HttpMessageHandler.SendAsync did not return a task.");
            }

            return await task.ConfigureAwait(false);
        }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }

            throw;
        }
    }

    private static async Task<byte[]> ReadBodyAsync(HttpResponseMessage response, string url, long maxBytes, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return new byte[0];
        }

        var announced = response.Content.Headers.ContentLength;
        if (announced is long length && length > maxBytes)
        {
            CancelBody(response);
            throw new DownloadException(
                url,
                message: "Download of " + url + " exceeded maximum size of " + maxBytes.ToString(CultureInfo.InvariantCulture) + " bytes (Content-Length: " + length.ToString(CultureInfo.InvariantCulture) + ").");
        }

        using (var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
        using (var output = new MemoryStream())
        {
            var buffer = new byte[8192];
            while (true)
            {
                var read = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                if (output.Length + read > maxBytes)
                {
                    throw new DownloadException(
                        url,
                        message: "Download of " + url + " exceeded maximum size of " + maxBytes.ToString(CultureInfo.InvariantCulture) + " bytes.");
                }

                output.Write(buffer, 0, read);
            }

            return output.ToArray();
        }
    }

    private static void CancelBody(HttpResponseMessage response)
    {
        try
        {
            if (response.Content is not null)
            {
                response.Content.Dispose();
            }
        }
        catch (Exception)
        {
        }

        try
        {
            response.Dispose();
        }
        catch (Exception)
        {
        }
    }

    private static bool IsRedirectStatus(int status)
    {
        return status == 301 || status == 302 || status == 303 || status == 307 || status == 308;
    }

    private static DownloadResult ReadDataUrl(Uri url)
    {
        var text = url.OriginalString;
        var comma = text.IndexOf(',');
        if (comma < 0 || !text.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            throw new DownloadException(text, message: "Invalid data URL: " + text);
        }

        var header = text.Substring("data:".Length, comma - "data:".Length);
        var payload = text.Substring(comma + 1);
        var isBase64 = header.IndexOf("base64", StringComparison.OrdinalIgnoreCase) >= 0;
        var mediaType = header;
        var semi = header.IndexOf(';');
        if (semi >= 0)
        {
            mediaType = header.Substring(0, semi);
        }

        if (mediaType.Length == 0)
        {
            mediaType = "text/plain";
        }

        byte[] data;
        try
        {
            data = isBase64
                ? Convert.FromBase64String(payload)
                : System.Text.Encoding.UTF8.GetBytes(Uri.UnescapeDataString(payload));
        }
        catch (FormatException ex)
        {
            throw new DownloadException(text, message: "Error decoding data URL", cause: ex);
        }

        return new DownloadResult(data, mediaType);
    }

    private static void Validate(string url)
    {
        Uri parsed;
        try
        {
            parsed = new Uri(url, UriKind.Absolute);
        }
        catch (Exception)
        {
            throw new DownloadException(url, message: "Invalid URL: " + url);
        }

        if (string.Equals(parsed.Scheme, "data", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(parsed.Scheme, "http", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(parsed.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new DownloadException(url, message: "URL scheme must be http, https, or data, got " + parsed.Scheme + ":");
        }

        var hostname = parsed.Host.ToLowerInvariant().TrimEnd('.');
        if (hostname.Length == 0)
        {
            throw new DownloadException(url, message: "URL must have a hostname");
        }

        if (hostname == "localhost" || hostname.EndsWith(".local", StringComparison.Ordinal) || hostname.EndsWith(".localhost", StringComparison.Ordinal))
        {
            throw new DownloadException(url, message: "URL with hostname " + hostname + " is not allowed");
        }

        if (hostname.StartsWith("[", StringComparison.Ordinal) && hostname.EndsWith("]", StringComparison.Ordinal))
        {
            var ipv6 = hostname.Substring(1, hostname.Length - 2);
            if (IsPrivateIPv6(ipv6))
            {
                throw new DownloadException(url, message: "URL with IPv6 address " + hostname + " is not allowed");
            }

            return;
        }

        if (IsIPv4(hostname))
        {
            if (IsPrivateIPv4(hostname))
            {
                throw new DownloadException(url, message: "URL with IP address " + hostname + " is not allowed");
            }

            return;
        }

        if (hostname.IndexOf(':') >= 0 && IsPrivateIPv6(hostname))
        {
            throw new DownloadException(url, message: "URL with IPv6 address " + hostname + " is not allowed");
        }
    }

    private static bool IsIPv4(string hostname)
    {
        var parts = hostname.Split('.');
        if (parts.Length != 4)
        {
            return false;
        }

        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Length == 0)
            {
                return false;
            }

            int number;
            if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out number) || number < 0 || number > 255)
            {
                return false;
            }

            if (number.ToString(CultureInfo.InvariantCulture) != part)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsPrivateIPv4(string ip)
    {
        var parts = ip.Split('.');
        var a = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var b = int.Parse(parts[1], CultureInfo.InvariantCulture);
        var c = int.Parse(parts[2], CultureInfo.InvariantCulture);
        if (a == 0 || a == 10 || a == 127)
        {
            return true;
        }

        if (a == 100 && b >= 64 && b <= 127)
        {
            return true;
        }

        if (a == 169 && b == 254)
        {
            return true;
        }

        if (a == 172 && b >= 16 && b <= 31)
        {
            return true;
        }

        if (a == 192 && b == 0 && (c == 0 || c == 2))
        {
            return true;
        }

        if (a == 192 && b == 168)
        {
            return true;
        }

        if (a == 198 && (b == 18 || b == 19))
        {
            return true;
        }

        if (a == 198 && b == 51 && c == 100)
        {
            return true;
        }

        if (a == 203 && b == 0 && c == 113)
        {
            return true;
        }

        return a >= 224;
    }

    private static bool IsPrivateIPv6(string ip)
    {
        var groups = ParseIPv6(ip);
        if (groups is null)
        {
            return true;
        }

        if (TopZero(groups, 7) && (groups[7] == 0 || groups[7] == 1))
        {
            return true;
        }

        if ((groups[0] & 0xfe00) == 0xfc00)
        {
            return true;
        }

        if ((groups[0] & 0xffc0) == 0xfe80)
        {
            return true;
        }

        if ((groups[0] & 0xffc0) == 0xfec0)
        {
            return true;
        }

        if ((groups[0] & 0xff00) == 0xff00)
        {
            return true;
        }

        if (groups[0] == 0x2001 && groups[1] == 0x0db8)
        {
            return true;
        }

        if (groups[0] == 0x3fff && (groups[1] & 0xf000) == 0x0000)
        {
            return true;
        }

        var embedsIPv4 = TopZero(groups, 6)
            || (TopZero(groups, 5) && groups[5] == 0xffff)
            || (TopZero(groups, 4) && groups[4] == 0xffff && groups[5] == 0)
            || (groups[0] == 0x0064 && groups[1] == 0xff9b && groups[2] == 0 && groups[3] == 0 && groups[4] == 0 && groups[5] == 0)
            || (groups[0] == 0x0064 && groups[1] == 0xff9b && groups[2] == 0x0001);
        if (!embedsIPv4)
        {
            return false;
        }

        var first = (groups[6] >> 8) & 0xff;
        var second = groups[6] & 0xff;
        var third = (groups[7] >> 8) & 0xff;
        var fourth = groups[7] & 0xff;
        return IsPrivateIPv4(first + "." + second + "." + third + "." + fourth);
    }

    private static bool TopZero(int[] groups, int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (groups[i] != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static int[]? ParseIPv6(string ip)
    {
        var address = ip.ToLowerInvariant();
        var zone = address.IndexOf('%');
        if (zone >= 0)
        {
            address = address.Substring(0, zone);
        }

        var halves = address.Split(new[] { "::" }, StringSplitOptions.None);
        if (halves.Length > 2)
        {
            return null;
        }

        var head = ParseIPv6Groups(halves[0]);
        if (head is null)
        {
            return null;
        }

        if (halves.Length == 2)
        {
            var tail = ParseIPv6Groups(halves[1]);
            if (tail is null)
            {
                return null;
            }

            var fill = 8 - head.Count - tail.Count;
            if (fill < 0)
            {
                return null;
            }

            var groups = new int[8];
            for (var i = 0; i < head.Count; i++)
            {
                groups[i] = head[i];
            }

            for (var i = 0; i < tail.Count; i++)
            {
                groups[head.Count + fill + i] = tail[i];
            }

            return groups;
        }

        if (head.Count != 8)
        {
            return null;
        }

        return head.ToArray();
    }

    private static List<int>? ParseIPv6Groups(string segment)
    {
        var groups = new List<int>();
        if (segment.Length == 0)
        {
            return groups;
        }

        var parts = segment.Split(':');
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.IndexOf('.') >= 0)
            {
                if (i != parts.Length - 1 || !IsIPv4(part))
                {
                    return null;
                }

                var bytes = part.Split('.');
                var a = int.Parse(bytes[0], CultureInfo.InvariantCulture);
                var b = int.Parse(bytes[1], CultureInfo.InvariantCulture);
                var c = int.Parse(bytes[2], CultureInfo.InvariantCulture);
                var d = int.Parse(bytes[3], CultureInfo.InvariantCulture);
                groups.Add((a << 8) | b);
                groups.Add((c << 8) | d);
                continue;
            }

            if (part.Length == 0 || part.Length > 4)
            {
                return null;
            }

            int value;
            if (!int.TryParse(part, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            {
                return null;
            }

            groups.Add(value);
        }

        return groups;
    }
}
