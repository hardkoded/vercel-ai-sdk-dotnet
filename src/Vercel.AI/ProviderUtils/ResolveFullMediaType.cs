// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>A file part whose media type may be a top-level segment.</summary>
public sealed class MediaFilePart
{
    /// <summary>Creates a file part.</summary>
    public MediaFilePart(string mediaType, InlineFileData data)
    {
        MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <summary>Declared media type. May be a full type, a wildcard, or a top-level segment.</summary>
    public string MediaType { get; }

    /// <summary>File bytes or a URL.</summary>
    public InlineFileData Data { get; }
}

/// <summary>Resolves a full <c>type/subtype</c> media type. Maps to <c>resolveFullMediaType</c>.</summary>
public static class FullMediaTypes
{
    /// <summary>Returns a full media type, sniffing bytes when the declared type has no subtype.</summary>
    public static string ResolveFullMediaType(MediaFilePart part)
    {
        if (part is null)
        {
            throw new ArgumentNullException(nameof(part));
        }

        if (MediaTypes.IsFullMediaType(part.MediaType))
        {
            return part.MediaType;
        }

        if (part.Data.Type == "data")
        {
            var detected = MediaTypes.DetectMediaType(part.Data.Data!, MediaTypes.GetTopLevelMediaType(part.MediaType));
            if (detected != null)
            {
                return detected;
            }

            throw new UnsupportedFunctionalityError(
                "file of media type \"" + part.MediaType + "\" must specify subtype since it could not be auto-detected");
        }

        throw new UnsupportedFunctionalityError(
            "file of media type \"" + part.MediaType + "\" must specify subtype since it is not passed as inline bytes");
    }
}
