// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

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

        if (IsIPv4(hostname) && IsPrivateIPv4(hostname))
        {
            throw new DownloadException(text, "URL with IP address " + hostname + " is not allowed");
        }
    }

    private static bool IsIPv4(string hostname)
    {
        var parts = hostname.Split('.');
        if (parts.Length != 4)
        {
            return false;
        }

        foreach (var part in parts)
        {
            if (part.Length == 0 || (part.Length > 1 && part[0] == '0'))
            {
                return false;
            }

            if (!int.TryParse(part, out var number) || number < 0 || number > 255)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsPrivateIPv4(string ip)
    {
        var parts = ip.Split('.');
        var a = int.Parse(parts[0]);
        var b = int.Parse(parts[1]);
        var c = int.Parse(parts[2]);
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
}
