// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>
/// Normalizes decision state. Maps to <c>prepareDecisionState</c>.
/// Public state is a string, a JSON object, or a list of parts. Each part is a dictionary with
/// <c>type</c> <c>text</c> (<c>text</c>), <c>json</c> (<c>value</c>), or <c>file</c> (<c>mediaType</c>, <c>data</c>, optional <c>filename</c>).
/// File <c>data</c> is a <see cref="FilePartInput"/>, <c>byte[]</c>, <see cref="Uri"/>, or string.
/// </summary>
public static class DecisionState
{
    /// <summary>
    /// Turns public state into the list of parts the model receives.
    /// URL files are downloaded. File media types are resolved to a full type.
    /// </summary>
    public static async Task<IReadOnlyList<object?>> PrepareDecisionStateAsync(object? state, CancellationToken cancellationToken = default)
    {
        if (state is string text)
        {
            return new List<object?> { new Dictionary<string, object?> { ["type"] = "text", ["text"] = text } };
        }

        if (state is not IList<object?> parts)
        {
            return new List<object?> { new Dictionary<string, object?> { ["type"] = "json", ["value"] = state } };
        }

        var prepared = new List<object?>(parts.Count);
        foreach (var item in parts)
        {
            var part = (IDictionary<string, object?>)item!;
            prepared.Add(part["type"] as string == "file" ? await ConvertFileAsync(part, cancellationToken).ConfigureAwait(false) : part);
        }

        return prepared;
    }

    private static FilePartInput ToFilePartInput(object? data)
    {
        return data switch
        {
            FilePartInput input => input,
            byte[] bytes => FilePartInput.FromBytes(bytes),
            Uri url => FilePartInput.FromUrl(url),
            string value => FilePartInput.FromString(value),
            _ => throw new InvalidArgumentException("state", data, "file data must be bytes, a URL, a string, or a file part input"),
        };
    }

    private static async Task<object?> ConvertFileAsync(IDictionary<string, object?> part, CancellationToken cancellationToken)
    {
        var converted = FileParts.ConvertToLanguageModelV4FilePart(ToFilePartInput(part["data"]));
        var mediaType = converted.MediaType ?? (string)part["mediaType"]!;
        Dictionary<string, object?> data;
        switch (converted.DataType)
        {
            case "data":
                object inline = (object?)converted.Bytes ?? converted.Base64 ?? string.Empty;
                mediaType = MediaTypes.ResolveFullMediaType(mediaType, inline);
                data = new Dictionary<string, object?> { ["type"] = "data", ["data"] = inline };
                break;
            case "url":
                var downloaded = await Download.GetAsync(converted.Url!, abortSignal: cancellationToken).ConfigureAwait(false);
                var contentType = downloaded.MediaType?.Split(';')[0].Trim();
                if (!MediaTypes.IsFullMediaType(mediaType) && contentType != null && MediaTypes.IsFullMediaType(contentType) && MediaTypes.GetTopLevelMediaType(contentType) == mediaType)
                {
                    mediaType = contentType;
                }

                mediaType = MediaTypes.ResolveFullMediaType(mediaType, downloaded.Data);
                data = new Dictionary<string, object?> { ["type"] = "data", ["data"] = downloaded.Data };
                break;
            case "reference":
                data = new Dictionary<string, object?> { ["type"] = "reference", ["reference"] = converted.Reference };
                break;
            default:
                data = new Dictionary<string, object?> { ["type"] = "text", ["text"] = converted.Text };
                break;
        }

        var file = new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = mediaType, ["data"] = data };
        if (part.TryGetValue("filename", out var filename) && filename != null)
        {
            file["filename"] = filename;
        }

        return file;
    }
}
