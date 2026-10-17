// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Perplexity;

/// <summary>Maps decision state parts to the Perplexity Decisions <c>state</c> field. Maps to <c>convertToPerplexityDecisionState</c>.</summary>
internal static class PerplexityDecisionState
{
    private const int MaxImages = 8;

    private static readonly string[] ImageMediaTypes = { "image/png", "image/jpeg", "image/webp" };

    private static readonly Regex ImageDataUrl = new Regex("^data:image/(png|jpeg|webp);base64,", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    internal static JsonNode? Convert(IReadOnlyList<DecisionModelStatePart> state)
    {
        var parts = new List<JsonNode?>();
        foreach (var part in state)
        {
            switch (part)
            {
                case DecisionModelTextPart text:
                    parts.Add(JsonValue.Create(text.Text));
                    break;
                case DecisionModelJsonPart json:
                    parts.Add(JsonNode.Parse(json.Value.GetRawText()));
                    break;
                case DecisionModelFilePart file:
                    parts.Add(Image(file));
                    break;
            }
        }

        var images = 0;
        foreach (var part in parts)
        {
            images += CountImages(part);
        }

        if (images > MaxImages)
        {
            throw new InvalidArgumentException("state", state, "Perplexity Decisions supports at most 8 images in the shared state.");
        }

        // Preserve a single string/object/array as native state. Other JSON values
        // must remain inside an array because the API rejects them at the top level.
        if (parts.Count == 1 && (parts[0] is JsonObject or JsonArray || (parts[0] is JsonValue value && value.GetValueKind() == JsonValueKind.String)))
        {
            return parts[0];
        }

        var array = new JsonArray();
        foreach (var part in parts)
        {
            array.Add(part);
        }

        return array;
    }

    private static JsonObject Image(DecisionModelFilePart file)
    {
        var dataType = file.Data.DataType;
        if (dataType != "data")
        {
            throw new UnsupportedFunctionalityException("Perplexity decision file input", "'Perplexity decision file input: " + dataType + "' functionality not supported.");
        }

        var bytes = file.Data.Bytes;
        var base64Text = file.Data.Base64 ?? string.Empty;
        var mediaType = MediaTypes.IsFullMediaType(file.MediaType)
            ? file.MediaType
            : bytes != null ? MediaTypes.DetectMediaType(bytes, "image") : MediaTypes.DetectMediaType(base64Text, "image");
        if (mediaType == null || Array.IndexOf(ImageMediaTypes, mediaType) < 0)
        {
            throw new UnsupportedFunctionalityException("Perplexity decision image media type", "'Perplexity decision image media type: " + file.MediaType + "' functionality not supported.");
        }

        var base64 = bytes != null ? System.Convert.ToBase64String(bytes) : base64Text;
        return new JsonObject
        {
            ["type"] = "image_url",
            ["image_url"] = new JsonObject { ["url"] = "data:" + mediaType + ";base64," + base64 },
        };
    }

    private static int CountImages(JsonNode? node)
    {
        switch (node)
        {
            case JsonArray array:
                return array.Sum(CountImages);
            case JsonObject obj:
                if (obj["type"] is JsonValue type && type.TryGetValue<string>(out var typeName) && typeName == "image_url")
                {
                    if (obj["image_url"] is not JsonObject image
                        || image["url"] is not JsonValue url
                        || !url.TryGetValue<string>(out var text)
                        || !ImageDataUrl.IsMatch(text))
                    {
                        throw new UnsupportedFunctionalityException("Perplexity decision images", "Perplexity decision images require PNG, JPEG, or WebP base64 data URLs");
                    }

                    return 1;
                }

                return obj.Sum(pair => CountImages(pair.Value));
            default:
                return 0;
        }
    }
}
