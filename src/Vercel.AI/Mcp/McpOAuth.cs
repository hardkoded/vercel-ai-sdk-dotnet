// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;

namespace Vercel.AI.Mcp;

/// <summary>Compares OAuth resource URIs the way the MCP authorization spec describes.</summary>
public static class McpOAuth
{
    /// <summary>
    /// Returns the resource URI for <paramref name="serverUrl"/> with the fragment removed.
    /// The result uses the form produced by the WHATWG URL <c>href</c>, including a slash on pathless URLs.
    /// </summary>
    public static string ResourceUrlFromServerUrl(string serverUrl)
    {
        if (serverUrl is null)
        {
            throw new ArgumentNullException(nameof(serverUrl));
        }

        return FormatHref(new Uri(serverUrl));
    }

    /// <summary>
    /// Returns true when <paramref name="requestedResource"/> has the same origin as
    /// <paramref name="configuredResource"/> and its path is the configured path or a sub-path.
    /// </summary>
    public static bool CheckResourceAllowed(string requestedResource, string configuredResource)
    {
        if (requestedResource is null)
        {
            throw new ArgumentNullException(nameof(requestedResource));
        }

        if (configuredResource is null)
        {
            throw new ArgumentNullException(nameof(configuredResource));
        }

        var requested = new Uri(requestedResource);
        var configured = new Uri(configuredResource);
        if (!string.Equals(Origin(requested), Origin(configured), StringComparison.Ordinal))
        {
            return false;
        }

        if (requested.AbsolutePath.Length < configured.AbsolutePath.Length)
        {
            return false;
        }

        var requestedPath = WithTrailingSlash(requested.AbsolutePath);
        var configuredPath = WithTrailingSlash(configured.AbsolutePath);
        return requestedPath.StartsWith(configuredPath, StringComparison.Ordinal);
    }

    private static string WithTrailingSlash(string path)
    {
        if (path.EndsWith("/", StringComparison.Ordinal))
        {
            return path;
        }

        return path + "/";
    }

    private static string Origin(Uri uri)
    {
        var builder = new StringBuilder();
        builder.Append(uri.Scheme.ToLowerInvariant());
        builder.Append("://");
        builder.Append(uri.Host.ToLowerInvariant());
        if (!uri.IsDefaultPort)
        {
            builder.Append(':');
            builder.Append(uri.Port.ToString(CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private static string FormatHref(Uri uri)
    {
        var builder = new StringBuilder();
        builder.Append(Origin(uri));
        var path = uri.AbsolutePath;
        if (path.Length == 0)
        {
            path = "/";
        }

        builder.Append(path);
        builder.Append(uri.Query);
        return builder.ToString();
    }
}
