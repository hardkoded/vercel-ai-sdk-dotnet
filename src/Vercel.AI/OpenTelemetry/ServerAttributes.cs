// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.OpenTelemetry;

/// <summary>Maps to <c>server-attributes.ts</c>.</summary>
internal static class ServerAttributes
{
    /// <summary>
    /// Maps to <c>getServerAttributes</c>. Only records the known transport destination, never infers a provider backend.
    /// </summary>
    public static Dictionary<string, object?> Get(string? url)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (
            url is null
            || !Uri.TryCreate(url, UriKind.Absolute, out var endpoint)
            || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)
        )
        {
            return result;
        }

        result["server.address"] = endpoint.Host.Trim('[', ']');
        result["server.port"] = endpoint.Port;
        return result;
    }
}
