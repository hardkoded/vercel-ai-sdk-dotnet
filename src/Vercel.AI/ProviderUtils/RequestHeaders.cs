// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.ProviderUtils;

internal static class RequestHeaders
{
    /// <summary>
    /// Returns a copy of <paramref name="input"/> without the headers that must not reach an untrusted URL: host and virtual-host
    /// routing, proxy and origin spoofing, cloud metadata, cookies, and hop-by-hop transport headers (RFC 7230 section 6.1).
    /// <c>Authorization</c> and other credential-bearing caller headers are kept on purpose, because trusted provider requests may need them.
    /// Maps to <c>sanitizeRequestHeaders</c>.
    /// </summary>
    internal static HeaderCollection SanitizeRequestHeaders(HeaderCollection input)
    {
        var headers = new HeaderCollection();
        foreach (var header in input.Entries())
        {
            if (!IsBlockedRequestHeader(header.Key))
            {
                headers.Add(header.Key, header.Value);
            }
        }

        return headers;
    }

    private static bool IsBlockedRequestHeader(string name)
    {
        switch (name.ToLowerInvariant())
        {
            // Hop-by-hop / transport
            case "connection":
            case "keep-alive":
            case "te":
            case "trailer":
            case "transfer-encoding":
            case "upgrade":
            // Host / virtual-host routing
            case "host":
            // Proxy / origin spoofing
            case "forwarded":
            case "proxy-authorization":
            case "via":
            case "x-forwarded-for":
            case "x-forwarded-host":
            case "x-forwarded-proto":
            case "x-real-ip":
            // Cloud metadata (GCP, AWS IMDSv1/v2, Azure, Alibaba, DigitalOcean)
            case "metadata":
            case "metadata-flavor":
            case "x-aws-ec2-metadata-token":
            case "x-metadata-token":
            // Session / cookie
            case "cookie":
            case "set-cookie":
                return true;
            default:
                return false;
        }
    }
}
