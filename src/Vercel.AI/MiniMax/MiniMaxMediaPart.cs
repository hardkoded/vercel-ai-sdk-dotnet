// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.MiniMax;

/// <summary>Converts image and video file parts to MiniMax Messages blocks. Maps to <c>convertToMiniMaxMediaPart</c>.</summary>
internal static class MiniMaxMediaPart
{
    public static JsonObject? Convert(JsonElement part)
    {
        var mediaType = AnthropicJson.String(part, "mediaType") ?? string.Empty;
        var type = MediaTypes.GetTopLevelMediaType(mediaType);
        if (type != "image" && type != "video")
        {
            return null;
        }

        var options = Options(part);
        var mediaOptions = new JsonObject();
        if (type == "image")
        {
            if (options != null && AnthropicJson.Property(options.Value, "detail") is { } detail)
            {
                var value = detail.ValueKind == JsonValueKind.String ? detail.GetString() : null;
                if (value != "low" && value != "default" && value != "high")
                {
                    throw new InvalidArgumentException("providerOptions", detail.GetRawText(), "MiniMax image detail must be 'low', 'default', or 'high'.");
                }

                mediaOptions["detail"] = value;
            }
        }
        else if (options != null && AnthropicJson.Property(options.Value, "fps") is { } fps)
        {
            if (fps.ValueKind != JsonValueKind.Number || fps.GetDouble() < 0.2 || fps.GetDouble() > 5)
            {
                throw new InvalidArgumentException("providerOptions", fps.GetRawText(), "MiniMax video fps must be a number between 0.2 and 5.");
            }

            mediaOptions["fps"] = fps.GetDouble();
        }

        var data = part.TryGetProperty("data", out var dataValue) ? dataValue : default;
        var dataType = data.ValueKind == JsonValueKind.Object ? AnthropicJson.String(data, "type") : null;
        var source = new JsonObject();
        switch (dataType)
        {
            case "data":
                var base64 = AnthropicJson.String(data, "data") ?? string.Empty;
                source["type"] = "base64";
                source["media_type"] = MediaTypes.IsFullMediaType(mediaType) ? mediaType : MediaTypes.ResolveFullMediaType(mediaType, System.Convert.FromBase64String(base64));
                source["data"] = base64;
                break;
            case "url":
                source["type"] = "url";
                source["url"] = AnthropicJson.String(data, "url");
                break;
            case "reference":
                if (type != "video")
                {
                    throw new UnsupportedFunctionalityException("MiniMax image file references", "Unsupported functionality: MiniMax image file references");
                }

                var reference = new Dictionary<string, string>();
                if (data.TryGetProperty("reference", out var referenceValue) && referenceValue.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in referenceValue.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            reference[property.Name] = property.Value.GetString()!;
                        }
                    }
                }

                var fileId = ProviderReferences.ResolveProviderReference(reference, "minimax");
                source["type"] = "url";
                source["url"] = fileId.StartsWith("mm_file://", StringComparison.Ordinal) ? fileId : "mm_file://" + fileId;
                break;
            default:
                var functionality = "MiniMax " + type + " input with " + dataType + " data";
                throw new UnsupportedFunctionalityException(functionality, "Unsupported functionality: " + functionality);
        }

        foreach (var pair in mediaOptions)
        {
            source[pair.Key] = pair.Value?.DeepClone();
        }

        return new JsonObject { ["type"] = type, ["source"] = source };
    }

    private static JsonElement? Options(JsonElement part)
    {
        if (!part.TryGetProperty("providerOptions", out var all) || all.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return all.TryGetProperty("minimax", out var minimax) && minimax.ValueKind == JsonValueKind.Object ? minimax : (JsonElement?)null;
    }
}
