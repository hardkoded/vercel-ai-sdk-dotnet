// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>A generated file that still points at a URL.</summary>
public sealed class GeneratedFileUrl : GeneratedContent
{
    /// <summary>Creates a URL-backed file.</summary>
    public GeneratedFileUrl(Uri url, string mediaType)
        : base("file")
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        MediaType = mediaType ?? string.Empty;
    }

    /// <summary>File URL.</summary>
    public Uri Url { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }
}

/// <summary>A download failed or the URL is not safe to fetch. Maps to <c>DownloadError</c>.</summary>
public sealed class DownloadException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public DownloadException(string url, string message, int? statusCode = null)
        : base(message)
    {
        Url = url ?? string.Empty;
        StatusCode = statusCode;
    }

    /// <summary>Stable error name.</summary>
    public string Name { get; } = "AI_DownloadError";

    /// <summary>URL that was rejected or that failed.</summary>
    public string Url { get; }

    /// <summary>HTTP status, when the failure came from a response.</summary>
    public int? StatusCode { get; }
}

/// <summary>Rejects unsafe file URLs before a download starts. Maps to <c>validateDownloadUrl</c>.</summary>
public static class DownloadUrls
{
    /// <summary>Throws <see cref="DownloadException"/> when <paramref name="url"/> is not safe to download.</summary>
    public static void ValidateDownloadUrl(string url)
    {
        if (url is null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            throw new DownloadException(url, "Invalid URL: " + url);
        }

        ValidateDownloadUrl(parsed);
    }

    /// <summary>Throws <see cref="DownloadException"/> when <paramref name="url"/> is not safe to download.</summary>
    public static void ValidateDownloadUrl(Uri url)
    {
        if (url is null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        var text = url.AbsoluteUri;
        if (string.Equals(url.Scheme, "data", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(url.Scheme, "http", StringComparison.OrdinalIgnoreCase) && !string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new DownloadException(text, "URL scheme must be http, https, or data, got " + url.Scheme + ":");
        }

        var hostname = url.Host.TrimEnd('.').ToLowerInvariant();
        if (hostname.Length == 0)
        {
            throw new DownloadException(text, "URL must have a hostname");
        }

        if (string.Equals(hostname, "localhost", StringComparison.Ordinal) || hostname.EndsWith(".local", StringComparison.Ordinal) || hostname.EndsWith(".localhost", StringComparison.Ordinal))
        {
            throw new DownloadException(text, "URL with hostname " + hostname + " is not allowed");
        }

        if (hostname.StartsWith("[", StringComparison.Ordinal) && hostname.EndsWith("]", StringComparison.Ordinal))
        {
            if (Download.IsPrivateIPv6(hostname.Substring(1, hostname.Length - 2)))
            {
                throw new DownloadException(text, "URL with IPv6 address " + hostname + " is not allowed");
            }

            return;
        }

        if (Download.IsIPv4(hostname) && Download.IsPrivateIPv4(hostname))
        {
            throw new DownloadException(text, "URL with IP address " + hostname + " is not allowed");
        }
    }
}
