// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Prompt;

/// <summary>Media type and base64 payload from a data URL. Maps to <c>splitDataUrl</c>.</summary>
public sealed class DataUrlParts
{
    /// <summary>Creates parts.</summary>
    public DataUrlParts(string? mediaType, string? base64Content)
    {
        MediaType = mediaType;
        Base64Content = base64Content;
    }

    /// <summary>Media type, when the header contained one.</summary>
    public string? MediaType { get; }

    /// <summary>Base64 payload, when a comma was present.</summary>
    public string? Base64Content { get; }
}

/// <summary>Data URL parsing. Maps to <c>splitDataUrl</c>.</summary>
public static class DataUrls
{
    /// <summary>Splits a data URL. Returns empty parts when the value cannot be split.</summary>
    public static DataUrlParts SplitDataUrl(string dataUrl)
    {
        if (dataUrl is null)
        {
            return new DataUrlParts(null, null);
        }

        try
        {
            var comma = dataUrl.IndexOf(',');
            var header = comma < 0 ? dataUrl : dataUrl.Substring(0, comma);
            var payload = comma < 0 ? null : dataUrl.Substring(comma + 1);
            var semicolon = header.IndexOf(';');
            var typeAndScheme = semicolon < 0 ? header : header.Substring(0, semicolon);
            var colon = typeAndScheme.IndexOf(':');
            var mediaType = colon < 0 ? null : typeAndScheme.Substring(colon + 1);
            return new DataUrlParts(mediaType, payload);
        }
        catch (Exception)
        {
            return new DataUrlParts(null, null);
        }
    }
}
