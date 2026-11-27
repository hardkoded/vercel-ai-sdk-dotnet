// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Alibaba;

/// <summary>One item of <see cref="AlibabaVideoModelOptions.Media"/>.</summary>
public sealed class AlibabaVideoModelOptionsMedia
{
    /// <summary>
    /// <c>reference_image</c>, <c>reference_video</c>, <c>reference_audio</c>, <c>first_frame</c>,
    /// <c>last_frame</c>, <c>file</c>, or <c>link</c>.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Public URL, or a <c>data:{mime};base64,{data}</c> URI for images.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>URL to an audio file used as voice reference for this media item.</summary>
    public string? ReferenceVoice { get; set; }
}

/// <summary>
/// Provider options for Alibaba video generation, read from <c>providerOptions.alibaba</c>.
/// Maps to <c>AlibabaVideoModelOptions</c> and <c>alibabaVideoModelOptionsSchema</c>.
/// </summary>
public sealed class AlibabaVideoModelOptions
{
    private static readonly string[] ShotTypes = ["single", "multi"];

    private static readonly string[] MediaTypes =
    [
        "reference_image",
        "reference_video",
        "reference_audio",
        "first_frame",
        "last_frame",
        "file",
        "link",
    ];

    private static readonly string[] Ratios = ["adaptive", "16:9", "9:16", "1:1", "4:3", "3:4"];

    /// <summary>Negative prompt to specify what to avoid (max 500 chars).</summary>
    public string? NegativePrompt { get; set; }

    /// <summary>URL to audio file for audio-video sync (WAV/MP3, 3-30s, max 15MB).</summary>
    public string? AudioUrl { get; set; }

    /// <summary>Enable prompt extension/rewriting for better generation. Defaults to true.</summary>
    public bool? PromptExtend { get; set; }

    /// <summary>
    /// Shot type: <c>single</c> for single-shot or <c>multi</c> for multi-shot narrative
    /// (wan2.6 and earlier; wan2.7 models describe shot structure in the prompt).
    /// </summary>
    public string? ShotType { get; set; }

    /// <summary>Whether to add watermark to generated video. Defaults to false.</summary>
    public bool? Watermark { get; set; }

    /// <summary>
    /// Enable audio generation (wan2.6 I2V/R2V and wan3 models; wan2.7 models always generate audio).
    /// Defaults to true on wan3.
    /// </summary>
    public bool? Audio { get; set; }

    /// <summary>
    /// Reference URLs for reference-to-video mode (wan2.6 models). Array of URLs to images (0-5) and/or
    /// videos (0-3), max 5 total. Use character identifiers (character1, character2) in prompts to reference them.
    /// </summary>
    public IReadOnlyList<string>? ReferenceUrls { get; set; }

    /// <summary>
    /// Explicit media array (wan2.7 and wan3 models). Overrides the automatic mapping from
    /// <c>inputReferences</c> and <c>frameImages</c>. Use <c>Image 1</c>, <c>Video 1</c>, etc. in prompts to
    /// reference media items (images, videos, and audio are counted separately, in array order).
    /// <c>reference_audio</c>, <c>file</c>, and <c>link</c> are wan3-only and have no top-level call option, so
    /// they can only be passed here. wan3 also rejects mixing <c>reference_*</c>/<c>file</c>/<c>link</c> with
    /// <c>first_frame</c>/<c>last_frame</c>.
    /// </summary>
    public IReadOnlyList<AlibabaVideoModelOptionsMedia>? Media { get; set; }

    /// <summary>
    /// Aspect ratio (wan2.7 text-to-video and reference-to-video models, and every wan3 generation).
    /// <c>adaptive</c> is wan3-only, and is its default.
    /// </summary>
    public string? Ratio { get; set; }

    /// <summary>Polling interval in milliseconds. Defaults to 5000 (5 seconds).</summary>
    public double? PollIntervalMs { get; set; }

    /// <summary>Maximum wait time in milliseconds for video generation. Defaults to 600000 (10 minutes).</summary>
    public double? PollTimeoutMs { get; set; }

    /// <summary>
    /// Reads <c>providerOptions.alibaba</c>. Returns null when absent. An invalid value throws
    /// <see cref="InvalidArgumentException"/>. Unknown keys are allowed, like <c>z.looseObject</c>.
    /// </summary>
    public static AlibabaVideoModelOptions? Parse(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(AlibabaProvider.ProviderId, out var raw)
            || raw.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        try
        {
            return Read(raw);
        }
        catch (FormatException exception)
        {
            throw new InvalidArgumentException(
                "providerOptions.alibaba",
                null,
                "Invalid Alibaba video provider options: " + exception.Message
            );
        }
    }

    private static AlibabaVideoModelOptions Read(JsonElement raw)
    {
        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("expected an object.");
        }

        var options = new AlibabaVideoModelOptions();
        foreach (var property in raw.EnumerateObject())
        {
            switch (property.Name)
            {
                case "negativePrompt":
                    options.NegativePrompt = String(property);
                    break;
                case "audioUrl":
                    options.AudioUrl = String(property);
                    break;
                case "promptExtend":
                    options.PromptExtend = Boolean(property);
                    break;
                case "shotType":
                    options.ShotType = Enum(property, ShotTypes);
                    break;
                case "watermark":
                    options.Watermark = Boolean(property);
                    break;
                case "audio":
                    options.Audio = Boolean(property);
                    break;
                case "referenceUrls":
                    options.ReferenceUrls = Strings(property);
                    break;
                case "media":
                    options.Media = ReadMedia(property);
                    break;
                case "ratio":
                    options.Ratio = Enum(property, Ratios);
                    break;
                case "pollIntervalMs":
                    options.PollIntervalMs = Positive(property);
                    break;
                case "pollTimeoutMs":
                    options.PollTimeoutMs = Positive(property);
                    break;
            }
        }

        return options;
    }

    private static string? String(JsonProperty property)
    {
        return property.Value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => property.Value.GetString(),
            _ => throw new FormatException(property.Name + " must be a string."),
        };
    }

    private static bool? Boolean(JsonProperty property)
    {
        return property.Value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new FormatException(property.Name + " must be a boolean."),
        };
    }

    private static double? Positive(JsonProperty property)
    {
        return property.Value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Number when property.Value.GetDouble() > 0 => property.Value.GetDouble(),
            _ => throw new FormatException(property.Name + " must be a positive number."),
        };
    }

    private static string? Enum(JsonProperty property, string[] allowed)
    {
        if (property.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return
            property.Value.ValueKind == JsonValueKind.String
            && Array.IndexOf(allowed, property.Value.GetString()) >= 0
            ? property.Value.GetString()
            : throw new FormatException(
                property.Name + " must be one of " + string.Join(", ", allowed) + "."
            );
    }

    private static List<string>? Strings(JsonProperty property)
    {
        if (property.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.Value.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException(property.Name + " must be an array of strings.");
        }

        var items = new List<string>();
        foreach (var item in property.Value.EnumerateArray())
        {
            items.Add(
                item.ValueKind == JsonValueKind.String
                    ? item.GetString()!
                    : throw new FormatException(property.Name + " entries must be strings.")
            );
        }

        return items;
    }

    private static List<AlibabaVideoModelOptionsMedia>? ReadMedia(JsonProperty property)
    {
        if (property.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.Value.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("media must be an array.");
        }

        var media = new List<AlibabaVideoModelOptionsMedia>();
        foreach (var item in property.Value.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String
                || Array.IndexOf(MediaTypes, type.GetString()) < 0
                || !item.TryGetProperty("url", out var url)
                || url.ValueKind != JsonValueKind.String
            )
            {
                throw new FormatException(
                    "media entries need a type ("
                        + string.Join(", ", MediaTypes)
                        + ") and a string url."
                );
            }

            string? referenceVoice = null;
            if (item.TryGetProperty("referenceVoice", out var voice))
            {
                referenceVoice = voice.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.String => voice.GetString(),
                    _ => throw new FormatException("media referenceVoice must be a string."),
                };
            }

            media.Add(
                new AlibabaVideoModelOptionsMedia
                {
                    Type = type.GetString()!,
                    Url = url.GetString()!,
                    ReferenceVoice = referenceVoice,
                }
            );
        }

        return media;
    }
}
