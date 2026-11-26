// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.OpenTelemetry;

/// <summary>Maps to <c>server-attributes.ts</c>.</summary>
internal static class ServerAttributes
{
    /// <summary>
    /// Only record the known transport destination, never infer a provider backend. Maps to <c>getServerAttributes</c>.
    /// </summary>
    public static Dictionary<string, object?> GetServerAttributes(string? url)
    {
        var attributes = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (
            url is null
            || !Uri.TryCreate(url, UriKind.Absolute, out var endpoint)
            || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)
        )
        {
            return attributes;
        }

        attributes["server.address"] = endpoint.Host.Trim('[', ']');
        attributes["server.port"] = endpoint.Port;
        return attributes;
    }
}
