// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Encodes provider-returned identifiers for URL paths.</summary>
public static class PathSegments
{
    /// <summary>
    /// Encodes a raw identifier as one URL path segment, including standalone dot segments that URL parsers
    /// would otherwise normalize. Pass unencoded values; for resource paths with intentional slashes, encode
    /// each segment separately.
    /// </summary>
    public static string Encode(string value)
    {
        var encoded = Uri.EscapeDataString(value ?? string.Empty);

        // URL parsing normalizes both literal and percent-encoded dot segments.
        return encoded == "."
            ? "%252E"
            : encoded == ".."
                ? "%252E%252E"
                : encoded;
    }
}
