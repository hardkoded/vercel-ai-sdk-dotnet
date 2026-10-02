// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http;
using System.Text;

namespace Vercel.AI.Util;

/// <summary>Failure while downloading a URL.</summary>
public sealed class DownloadError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_DownloadError";

    /// <summary>Creates a download error.</summary>
    public DownloadError(string url, string message, int? statusCode = null, string? statusText = null, Exception? cause = null)
        : base("AI_DownloadError", message, cause)
    {
        Url = url;
        StatusCode = statusCode;
        StatusText = statusText;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>URL that failed.</summary>
    public string Url { get; }

    /// <summary>HTTP status, when the response was received.</summary>
    public int? StatusCode { get; }

    /// <summary>HTTP status text, when the response was received.</summary>
    public string? StatusText { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }

    /// <summary>Builds the message used for a non-success HTTP response.</summary>
    public static DownloadError FromStatus(string url, int statusCode, string statusText)
    {
        return new DownloadError(url, "Failed to download " + url + ": " + statusCode + " " + statusText, statusCode, statusText);
    }

    /// <summary>Builds the message used when the transport throws.</summary>
    public static DownloadError FromCause(string url, Exception cause)
    {
        return new DownloadError(url, "Failed to download " + url + ": " + cause, cause: cause);
    }
}

/// <summary>Bytes and media type returned by <c>Download.GetAsync</c>.</summary>
public sealed class DownloadResult
{
    /// <summary>Creates a download result.</summary>
    public DownloadResult(byte[]? data, string? mediaType)
    {
        Data = data ?? Array.Empty<byte>();
        MediaType = mediaType;
    }

    /// <summary>Response body.</summary>
    public byte[] Data { get; }

    /// <summary>Content-Type header, when present.</summary>
    public string? MediaType { get; }
}

/// <summary>Options passed to a download fetch implementation.</summary>
public sealed class DownloadRequest
{
    /// <summary>Creates the request.</summary>
    public DownloadRequest(HeaderCollection? headers, CancellationToken abortSignal, string redirect)
    {
        Headers = headers ?? new HeaderCollection();
        AbortSignal = abortSignal;
        Redirect = redirect;
    }

    /// <summary>Request headers.</summary>
    public HeaderCollection Headers { get; }

    /// <summary>Cancellation signal for the attempt.</summary>
    public CancellationToken AbortSignal { get; }

    /// <summary><c>manual</c> or <c>follow</c>.</summary>
    public string Redirect { get; }
}

/// <summary>One HTTP response seen by <see cref="Download"/>.</summary>
public sealed class DownloadResponse
{
    /// <summary>Creates a response.</summary>
    public DownloadResponse(int status, string? statusText, HeaderCollection? headers, byte[]? body, string? type = "basic")
    {
        Status = status;
        StatusText = statusText ?? string.Empty;
        Headers = headers ?? new HeaderCollection();
        Body = body;
        Type = type ?? "basic";
    }

    /// <summary>HTTP status.</summary>
    public int Status { get; }

    /// <summary>HTTP status text.</summary>
    public string StatusText { get; }

    /// <summary>Response headers.</summary>
    public HeaderCollection Headers { get; }

    /// <summary>Response body. May be null.</summary>
    public byte[]? Body { get; }

    /// <summary><c>basic</c> or <c>opaqueredirect</c>.</summary>
    public string Type { get; }

    /// <summary>Whether the status is 200-299.</summary>
    public bool Ok
    {
        get { return Status >= 200 && Status <= 299; }
    }

    /// <summary>Called when the body is cancelled.</summary>
    public Action? OnCancel { get; set; }

    /// <summary>Whether <see cref="Cancel"/> has run.</summary>
    public bool Cancelled { get; private set; }

    /// <summary>Releases the body. Failures are ignored.</summary>
    public void Cancel()
    {
        try
        {
            Cancelled = true;
            var onCancel = OnCancel;
            if (onCancel != null)
            {
                onCancel();
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>A successful body response.</summary>
    public static DownloadResponse Bytes(byte[]? body, string? mediaType, int status = 200)
    {
        var headers = new HeaderCollection();
        if (mediaType != null)
        {
            headers.Add("content-type", mediaType);
        }

        return new DownloadResponse(status, "OK", headers, body);
    }

    /// <summary>A successful UTF-8 text response.</summary>
    public static DownloadResponse Text(string? text, string? mediaType = "text/plain")
    {
        return Bytes(Encoding.UTF8.GetBytes(text ?? string.Empty), mediaType);
    }
}

/// <summary>Fetches one URL for <see cref="Download"/>.</summary>
public delegate Task<DownloadResponse> DownloadFetch(string url, DownloadRequest request);

/// <summary>Downloads a URL with SSRF checks on every redirect hop.</summary>
public static class Download
{
    /// <summary>Default maximum download size: 2 GiB.</summary>
    public const long DefaultMaxBytes = 2L * 1024L * 1024L * 1024L;

    private const int MaxRedirects = 10;

    /// <summary>
    /// Fetch used when a call does not pass its own. Read on each download so a replacement is picked up immediately.
    /// </summary>
    public static DownloadFetch? Fetch { get; set; }

    /// <summary>
    /// When true, an opaque redirect is followed natively. Server runtimes leave this false and fail closed.
    /// </summary>
    public static bool IsBrowser { get; set; }

    /// <summary>User-Agent suffix sent on download requests.</summary>
    public static string? UserAgent { get; set; } = "ai-sdk/dotnet";

    /// <summary>Downloads <paramref name="url"/>.</summary>
    public static Task<DownloadResult> GetAsync(Uri url, long? maxBytes = null, CancellationToken abortSignal = default, DownloadFetch? fetch = null)
    {
        if (url == null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        var urlText = string.Equals(url.Scheme, "data", StringComparison.OrdinalIgnoreCase) ? url.OriginalString : url.AbsoluteUri;
        return GetAsync(urlText, maxBytes ?? DefaultMaxBytes, abortSignal, fetch);
    }

    /// <summary>Throws <see cref="DownloadError"/> when <paramref name="url"/> is not safe to request.</summary>
    public static void ValidateDownloadUrl(string url)
    {
        Uri parsed;
        try
        {
            parsed = new Uri(url, UriKind.Absolute);
        }
        catch (Exception)
        {
            throw new DownloadError(url, "Invalid URL: " + url);
        }

        if (string.Equals(parsed.Scheme, "data", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(parsed.Scheme, "http", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(parsed.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new DownloadError(url, "URL scheme must be http, https, or data, got " + parsed.Scheme + ":");
        }

        var hostname = (parsed.Host ?? string.Empty).ToLowerInvariant().TrimEnd('.');
        if (hostname.StartsWith("[", StringComparison.Ordinal) && hostname.EndsWith("]", StringComparison.Ordinal))
        {
            hostname = hostname.Substring(1, hostname.Length - 2);
        }

        if (hostname.Length == 0)
        {
            throw new DownloadError(url, "URL must have a hostname");
        }

        if (hostname == "localhost" || hostname.EndsWith(".local", StringComparison.Ordinal) || hostname.EndsWith(".localhost", StringComparison.Ordinal))
        {
            throw new DownloadError(url, "URL with hostname " + hostname + " is not allowed");
        }

        if (parsed.HostNameType == UriHostNameType.IPv6 || hostname.IndexOf(':') >= 0)
        {
            if (IsPrivateIPv6(hostname))
            {
                throw new DownloadError(url, "URL with IPv6 address " + parsed.Host + " is not allowed");
            }

            return;
        }

        if (IsIPv4(hostname) && IsPrivateIPv4(hostname))
        {
            throw new DownloadError(url, "URL with IP address " + hostname + " is not allowed");
        }
    }

    private static async Task<DownloadResult> GetAsync(string urlText, long maxBytes, CancellationToken abortSignal, DownloadFetch? fetch)
    {
        try
        {
            if (urlText.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                ValidateDownloadUrl(urlText);
                return DecodeDataUrl(urlText);
            }

            var response = await FetchWithValidatedRedirectsAsync(urlText, abortSignal, fetch).ConfigureAwait(false);
            if (!response.Ok)
            {
                response.Cancel();
                throw DownloadError.FromStatus(urlText, response.Status, response.StatusText);
            }

            var data = ReadWithLimit(response, urlText, maxBytes);
            var mediaType = response.Headers.Get("content-type");
            return new DownloadResult(data, mediaType);
        }
        catch (DownloadError)
        {
            throw;
        }
        catch (Exception error)
        {
            throw DownloadError.FromCause(urlText, error);
        }
    }

    private static async Task<DownloadResponse> FetchWithValidatedRedirectsAsync(string url, CancellationToken abortSignal, DownloadFetch? fetch)
    {
        var currentUrl = url;
        HeaderCollection headers = new HeaderCollection();
        headers.Add("user-agent", UserAgent ?? "ai-sdk/dotnet");
        for (var redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
        {
            ValidateDownloadUrl(currentUrl);
            var currentFetch = fetch ?? Fetch ?? DefaultFetchAsync;
            var response = await currentFetch(currentUrl, new DownloadRequest(headers, abortSignal, "manual")).ConfigureAwait(false);
            if (string.Equals(response.Type, "opaqueredirect", StringComparison.Ordinal))
            {
                if (!IsBrowser)
                {
                    throw new DownloadError(url, "Redirect from " + currentUrl + " could not be validated and was blocked");
                }

                return await currentFetch(currentUrl, new DownloadRequest(headers, abortSignal, "follow")).ConfigureAwait(false);
            }

            var location = response.Headers.Get("location");
            if (IsRedirectStatus(response.Status) && !string.IsNullOrEmpty(location))
            {
                response.Cancel();
                var next = new Uri(new Uri(currentUrl), location);
                if (!IsSameOrigin(next.AbsoluteUri, currentUrl))
                {
                    var userAgent = headers.Get("user-agent");
                    headers = new HeaderCollection();
                    if (userAgent != null)
                    {
                        headers.Add("user-agent", userAgent);
                    }
                }

                currentUrl = next.AbsoluteUri;
                continue;
            }

            return response;
        }

        throw new DownloadError(url, "Too many redirects (max " + MaxRedirects + ")");
    }

    private static byte[] ReadWithLimit(DownloadResponse response, string url, long maxBytes)
    {
        var contentLength = response.Headers.Get("content-length");
        if (!string.IsNullOrEmpty(contentLength))
        {
            long length;
            if (long.TryParse(contentLength, NumberStyles.Integer, CultureInfo.InvariantCulture, out length) && length > maxBytes)
            {
                response.Cancel();
                throw new DownloadError(url, "Download of " + url + " exceeded maximum size of " + maxBytes + " bytes (Content-Length: " + length + ").");
            }
        }

        var body = response.Body ?? Array.Empty<byte>();
        if (body.LongLength > maxBytes)
        {
            response.Cancel();
            throw new DownloadError(url, "Download of " + url + " exceeded maximum size of " + maxBytes + " bytes.");
        }

        return body;
    }

    private static DownloadResult DecodeDataUrl(string urlText)
    {
        var comma = urlText.IndexOf(',');
        var header = comma < 0 ? urlText : urlText.Substring(0, comma);
        var payload = comma < 0 ? string.Empty : urlText.Substring(comma + 1);
        var mediaType = header.Substring("data:".Length).Split(';')[0];
        byte[] data;
        if (header.IndexOf(";base64", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            data = Convert.FromBase64String(payload);
        }
        else
        {
            data = Encoding.UTF8.GetBytes(Uri.UnescapeDataString(payload));
        }

        return new DownloadResult(data, mediaType);
    }

    private static bool IsRedirectStatus(int status)
    {
        return status == 301 || status == 302 || status == 303 || status == 307 || status == 308;
    }

    private static bool IsSameOrigin(string left, string right)
    {
        var a = new Uri(left);
        var b = new Uri(right);
        return string.Equals(a.Scheme, b.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.Host, b.Host, StringComparison.OrdinalIgnoreCase)
            && a.Port == b.Port;
    }

    private static Task<DownloadResponse> DefaultFetchAsync(string url, DownloadRequest request)
    {
        return DefaultFetchCoreAsync(url, request);
    }

    private static async Task<DownloadResponse> DefaultFetchCoreAsync(string url, DownloadRequest request)
    {
        using (var handler = new HttpClientHandler { AllowAutoRedirect = false })
        using (var client = new HttpClient(handler))
        using (var message = new HttpRequestMessage(HttpMethod.Get, url))
        {
            var userAgent = request.Headers.Get("user-agent");
            if (!string.IsNullOrEmpty(userAgent))
            {
                message.Headers.TryAddWithoutValidation("User-Agent", userAgent);
            }

            using (var response = await client.SendAsync(message, request.AbortSignal).ConfigureAwait(false))
            {
                var headers = new HeaderCollection();
                foreach (var header in response.Headers)
                {
                    foreach (var value in header.Value)
                    {
                        headers.Add(header.Key, value);
                    }
                }

                foreach (var header in response.Content.Headers)
                {
                    foreach (var value in header.Value)
                    {
                        headers.Add(header.Key, value);
                    }
                }

                var body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return new DownloadResponse((int)response.StatusCode, response.ReasonPhrase, headers, body);
            }
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
            int number;
            if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out number) || number < 0 || number > 255 || number.ToString(CultureInfo.InvariantCulture) != parts[i])
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
        if (groups == null)
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

        if ((groups[0] & 0xffc0) == 0xfe80 || (groups[0] & 0xffc0) == 0xfec0 || (groups[0] & 0xff00) == 0xff00)
        {
            return true;
        }

        if (groups[0] == 0x2001 && groups[1] == 0x0db8)
        {
            return true;
        }

        if (groups[0] == 0x3fff && (groups[1] & 0xf000) == 0)
        {
            return true;
        }

        var embeds = TopZero(groups, 6)
            || (TopZero(groups, 5) && groups[5] == 0xffff)
            || (TopZero(groups, 4) && groups[4] == 0xffff && groups[5] == 0)
            || (groups[0] == 0x0064 && groups[1] == 0xff9b && groups[2] == 0 && groups[3] == 0 && groups[4] == 0 && groups[5] == 0)
            || (groups[0] == 0x0064 && groups[1] == 0xff9b && groups[2] == 0x0001);
        if (!embeds)
        {
            return false;
        }

        var a = (groups[6] >> 8) & 0xff;
        var b = groups[6] & 0xff;
        var c = (groups[7] >> 8) & 0xff;
        var d = groups[7] & 0xff;
        return IsPrivateIPv4(a + "." + b + "." + c + "." + d);
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
        if (head == null)
        {
            return null;
        }

        if (halves.Length == 2)
        {
            var tail = ParseIPv6Groups(halves[1]);
            if (tail == null)
            {
                return null;
            }

            var fill = 8 - head.Count - tail.Count;
            if (fill < 0)
            {
                return null;
            }

            var groups = new int[8];
            head.CopyTo(groups, 0);
            tail.CopyTo(groups, head.Count + fill);
            return groups;
        }

        return head.Count == 8 ? head.ToArray() : null;
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

/// <summary>Arguments passed to a single asset download.</summary>
public sealed class AssetDownloadCall
{
    /// <summary>Creates the call.</summary>
    public AssetDownloadCall(Uri url, bool isUrlSupportedByModel, CancellationToken abortSignal)
    {
        Url = url;
        IsUrlSupportedByModel = isUrlSupportedByModel;
        AbortSignal = abortSignal;
    }

    /// <summary>URL to download.</summary>
    public Uri Url { get; }

    /// <summary>Whether the model can read the URL itself.</summary>
    public bool IsUrlSupportedByModel { get; }

    /// <summary>Cancellation signal forwarded from the caller.</summary>
    public CancellationToken AbortSignal { get; }
}

/// <summary>Default download function: downloads URLs the model cannot fetch itself.</summary>
public static class DefaultDownloadFunction
{
    /// <summary>Creates a function that downloads each URL the model does not support.</summary>
    public static Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> Create(Func<AssetDownloadCall, Task<DownloadResult>>? download = null, CancellationToken abortSignal = default)
    {
        return async delegate(IReadOnlyList<DownloadRequestItem> requested)
        {
            var tasks = new Task<DownloadResult>[requested.Count];
            for (var i = 0; i < requested.Count; i++)
            {
                var item = requested[i];
                if (item.IsUrlSupportedByModel)
                {
                    tasks[i] = Task.FromResult<DownloadResult>(null!);
                    continue;
                }

                var call = new AssetDownloadCall(item.Url, item.IsUrlSupportedByModel, abortSignal);
                tasks[i] = download != null
                    ? download(call)
                    : Download.GetAsync(item.Url, null, abortSignal);
            }

            var completed = await Task.WhenAll(tasks).ConfigureAwait(false);
            return completed;
        };
    }
}

/// <summary>One URL passed to <see cref="DefaultDownloadFunction"/>.</summary>
public sealed class DownloadRequestItem
{
    /// <summary>Creates a request item.</summary>
    public DownloadRequestItem(Uri url, bool isUrlSupportedByModel)
    {
        Url = url;
        IsUrlSupportedByModel = isUrlSupportedByModel;
    }

    /// <summary>URL to download or pass through.</summary>
    public Uri Url { get; }

    /// <summary>When true, the model accepts the URL and the download function returns null.</summary>
    public bool IsUrlSupportedByModel { get; }
}
