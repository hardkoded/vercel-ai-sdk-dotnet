// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>Removes credentials, paths, queries, and fragments of a request URL before telemetry. Maps to <c>telemetry/get-request-origin.ts</c>.</summary>
public static class RequestOrigin
{
    /// <summary>
    /// Returns the origin of an HTTP or HTTPS URL, or null for anything else.
    /// Maps to <c>getRequestOrigin</c>.
    /// </summary>
    public static string? GetRequestOrigin(string? url)
    {
        if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out var endpoint))
        {
            return null;
        }

        if (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        return endpoint.GetLeftPart(UriPartial.Authority).Replace(endpoint.UserInfo + "@", "");
    }

    /// <summary>
    /// Keeps the request metadata but reduces its URL to the origin, and drops an unusable URL.
    /// Maps to <c>sanitizeRequestMetadata</c>.
    /// </summary>
    public static LanguageModelStreamRequest? SanitizeRequestMetadata(
        LanguageModelStreamRequest? request
    )
    {
        if (request?.Url == null)
        {
            return request;
        }

        return new LanguageModelStreamRequest(request.Body)
        {
            Url = GetRequestOrigin(request.Url),
        };
    }
}
