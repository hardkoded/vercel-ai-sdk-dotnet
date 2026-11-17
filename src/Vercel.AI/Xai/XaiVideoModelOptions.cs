// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Xai;

/// <summary>A mid-video image anchor. Upstream <c>XaiVideoKeyframe</c>.</summary>
public sealed class XaiVideoKeyframe
{
    /// <summary>Image URL.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Position of the anchor in seconds. Must be positive.</summary>
    public double TimestampSeconds { get; set; }
}

/// <summary>Store the generated video in the xAI Files API. Upstream <c>XaiVideoStorageOptions</c>.</summary>
public sealed class XaiVideoStorageOptions
{
    /// <summary>File name of the stored video.</summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>Seconds until the stored file expires. Positive, at most 2,592,000.</summary>
    public int? ExpiresAfter { get; set; }

    /// <summary>True to create a public URL with the default lifetime. Set <see cref="PublicUrlExpiresAfter"/> to choose the lifetime.</summary>
    public bool? PublicUrl { get; set; }

    /// <summary>Lifetime of the public URL in seconds, from 3,600 to 2,592,000. When set, a public URL is requested as an object.</summary>
    public int? PublicUrlExpiresAfter { get; set; }

    /// <summary>True when <c>publicUrl</c> was given as an object, even an empty one.</summary>
    internal bool PublicUrlIsObject { get; set; }
}

/// <summary>Provider options for xAI video generation, read from <c>providerOptions.xai</c>. Upstream <c>XaiVideoModelOptions</c> and <c>XaiParsedVideoModelOptions</c>.</summary>
/// <remarks>
/// Use <see cref="Mode"/> to select the operation:
/// <list type="bullet">
/// <item><description><c>edit-video</c> with <see cref="VideoUrl"/>: video editing (<c>POST /v1/videos/edits</c>).</description></item>
/// <item><description><c>extend-video</c> with <see cref="VideoUrl"/>: video extension (<c>POST /v1/videos/extensions</c>).</description></item>
/// <item><description><c>reference-to-video</c> with <see cref="ReferenceImageUrls"/>: R2V generation (<c>POST /v1/videos/generations</c>).</description></item>
/// <item><description>No mode: standard generation from text prompts or image input.</description></item>
/// </list>
/// Omitting the mode while providing <see cref="VideoUrl"/> behaves like <c>edit-video</c>. Omitting it while providing <see cref="ReferenceImageUrls"/> behaves like <c>reference-to-video</c>.
/// </remarks>
public sealed class XaiVideoModelOptions
{
    /// <summary>The <c>edit-video</c> mode.</summary>
    public const string EditVideo = "edit-video";

    /// <summary>The <c>extend-video</c> mode.</summary>
    public const string ExtendVideo = "extend-video";

    /// <summary>The <c>reference-to-video</c> mode.</summary>
    public const string ReferenceToVideo = "reference-to-video";

    private static readonly string[] Modes = [EditVideo, ExtendVideo, ReferenceToVideo];

    private static readonly string[] Resolutions = ["480p", "720p", "1080p"];

    /// <summary><c>edit-video</c>, <c>extend-video</c>, <c>reference-to-video</c>, or null.</summary>
    public string? Mode { get; set; }

    /// <summary>Source video URL to edit or extend.</summary>
    public string? VideoUrl { get; set; }

    /// <summary>Reference image URLs (1-7) for R2V generation.</summary>
    public IReadOnlyList<string>? ReferenceImageUrls { get; set; }

    /// <summary>Preset voice ids (up to 3) that give the subject a voice.</summary>
    public IReadOnlyList<string>? ReferenceVoiceIds { get; set; }

    /// <summary>Mid-video image anchors (up to 4).</summary>
    public IReadOnlyList<XaiVideoKeyframe>? Keyframes { get; set; }

    /// <summary>Store the generated video in the xAI Files API.</summary>
    public XaiVideoStorageOptions? StorageOptions { get; set; }

    /// <summary>A unique identifier representing the end user, for abuse monitoring.</summary>
    public string? User { get; set; }

    /// <summary>Deliver directly to caller-managed storage using an HTTPS signed PUT URL. Upstream <c>output.uploadUrl</c>.</summary>
    public string? OutputUploadUrl { get; set; }

    /// <summary>Poll interval in milliseconds. Must be positive.</summary>
    public double? PollIntervalMs { get; set; }

    /// <summary>Poll timeout in milliseconds. Must be positive.</summary>
    public double? PollTimeoutMs { get; set; }

    /// <summary><c>480p</c>, <c>720p</c>, or <c>1080p</c>.</summary>
    public string? Resolution { get; set; }

    /// <summary>Options this class does not model. They are sent to the API as they are. Upstream <c>z.looseObject</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; set; } =
        new Dictionary<string, JsonElement>();

    /// <summary>
    /// Reads <c>providerOptions.xai</c>. Returns null when absent. An invalid value throws <see cref="InvalidArgumentException"/>.
    /// When <c>output</c> is present the error carries no details, so a signed upload URL never reaches a message.
    /// </summary>
    public static XaiVideoModelOptions? Parse(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(XaiProvider.ProviderId, out var raw)
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
            // Validation errors name the offending value. Signed upload URLs must not appear in diagnostics,
            // including errors about other options.
            if (raw.ValueKind == JsonValueKind.Object && raw.TryGetProperty("output", out _))
            {
                throw new InvalidArgumentException(
                    "providerOptions.xai",
                    null,
                    "Invalid xAI video provider options for upload delivery."
                );
            }

            throw new InvalidArgumentException(
                "providerOptions.xai",
                null,
                "Invalid xAI video provider options: " + exception.Message
            );
        }
    }

    private static XaiVideoModelOptions Read(JsonElement raw)
    {
        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("expected an object.");
        }

        var options = new XaiVideoModelOptions();
        var additional = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in raw.EnumerateObject())
        {
            var value = property.Value;
            switch (property.Name)
            {
                case "mode":
                    options.Mode = Enum(property, Modes);
                    break;
                case "videoUrl":
                    options.VideoUrl = NonEmptyString(property);
                    break;
                case "referenceImageUrls":
                    options.ReferenceImageUrls = Strings(property, 1, 7);
                    break;
                case "referenceVoiceIds":
                    options.ReferenceVoiceIds = Strings(property, 0, 3);
                    break;
                case "keyframes":
                    options.Keyframes = ReadKeyframes(property);
                    break;
                case "storageOptions":
                    options.StorageOptions = ReadStorage(value, property.Name);
                    break;
                case "user":
                    options.User =
                        value.ValueKind == JsonValueKind.String
                            ? value.GetString()
                            : throw Wrong(property.Name);
                    break;
                case "output":
                    options.OutputUploadUrl = Output(value);
                    break;
                case "pollIntervalMs":
                    options.PollIntervalMs = NullablePositive(property);
                    break;
                case "pollTimeoutMs":
                    options.PollTimeoutMs = NullablePositive(property);
                    break;
                case "resolution":
                    options.Resolution =
                        value.ValueKind == JsonValueKind.Null ? null : Enum(property, Resolutions);
                    break;
                default:
                    additional[property.Name] = value.Clone();
                    break;
            }
        }

        options.AdditionalProperties = additional;
        return options;
    }

    private static string Enum(JsonProperty property, string[] allowed)
    {
        return
            property.Value.ValueKind == JsonValueKind.String
            && Array.IndexOf(allowed, property.Value.GetString()) >= 0
            ? property.Value.GetString()!
            : throw new FormatException(
                property.Name + " must be one of " + string.Join(", ", allowed) + "."
            );
    }

    private static string NonEmptyString(JsonProperty property)
    {
        return
            property.Value.ValueKind == JsonValueKind.String
            && property.Value.GetString() is { Length: > 0 } text
            ? text
            : throw new FormatException(property.Name + " must be a non-empty string.");
    }

    private static List<string> Strings(JsonProperty property, int min, int max)
    {
        if (property.Value.ValueKind != JsonValueKind.Array)
        {
            throw Wrong(property.Name);
        }

        var items = new List<string>();
        foreach (var item in property.Value.EnumerateArray())
        {
            items.Add(
                item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } text
                    ? text
                    : throw new FormatException(
                        property.Name + " entries must be non-empty strings."
                    )
            );
        }

        return items.Count >= min && items.Count <= max
            ? items
            : throw new FormatException(
                property.Name + " must have between " + min + " and " + max + " entries."
            );
    }

    private static List<XaiVideoKeyframe> ReadKeyframes(JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.Array || property.Value.GetArrayLength() > 4)
        {
            throw new FormatException("keyframes must be an array of at most 4 entries.");
        }

        var keyframes = new List<XaiVideoKeyframe>();
        foreach (var item in property.Value.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("imageUrl", out var imageUrl)
                || imageUrl.ValueKind != JsonValueKind.String
                || imageUrl.GetString() is not { Length: > 0 }
                || !item.TryGetProperty("timestampSeconds", out var timestamp)
                || timestamp.ValueKind != JsonValueKind.Number
                || timestamp.GetDouble() <= 0
            )
            {
                throw new FormatException(
                    "keyframes entries need an imageUrl and a positive timestampSeconds."
                );
            }

            keyframes.Add(
                new XaiVideoKeyframe
                {
                    ImageUrl = imageUrl.GetString()!,
                    TimestampSeconds = timestamp.GetDouble(),
                }
            );
        }

        return keyframes;
    }

    private static XaiVideoStorageOptions ReadStorage(JsonElement raw, string name)
    {
        if (
            raw.ValueKind != JsonValueKind.Object
            || !raw.TryGetProperty("filename", out var filename)
            || filename.ValueKind != JsonValueKind.String
            || filename.GetString() is not { Length: > 0 }
        )
        {
            throw new FormatException(name + " needs a non-empty filename.");
        }

        var storage = new XaiVideoStorageOptions { Filename = filename.GetString()! };
        if (raw.TryGetProperty("expiresAfter", out var expiresAfter))
        {
            storage.ExpiresAfter = Integer(expiresAfter, 1, 2_592_000, name + ".expiresAfter");
        }

        if (raw.TryGetProperty("publicUrl", out var publicUrl))
        {
            switch (publicUrl.ValueKind)
            {
                case JsonValueKind.True:
                case JsonValueKind.False:
                    storage.PublicUrl = publicUrl.GetBoolean();
                    break;
                case JsonValueKind.Object:
                    storage.PublicUrlIsObject = true;
                    if (publicUrl.TryGetProperty("expiresAfter", out var publicExpiresAfter))
                    {
                        storage.PublicUrlExpiresAfter = Integer(
                            publicExpiresAfter,
                            3_600,
                            2_592_000,
                            name + ".publicUrl.expiresAfter"
                        );
                    }

                    break;
                default:
                    throw new FormatException(name + ".publicUrl must be a boolean or an object.");
            }
        }

        return storage;
    }

    private static int Integer(JsonElement value, int min, int max, string name)
    {
        return
            value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            && number >= min
            && number <= max
            ? number
            : throw new FormatException(
                name + " must be an integer from " + min + " to " + max + "."
            );
    }

    private static string Output(JsonElement value)
    {
        // An HTTPS URL without credentials or a fragment.
        if (
            value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("uploadUrl", out var uploadUrl)
            && uploadUrl.ValueKind == JsonValueKind.String
            && uploadUrl.GetString() is { Length: > 0 } text
            && Uri.TryCreate(text, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps
            && uri.UserInfo.Length == 0
            && uri.Fragment.Length == 0
        )
        {
            return text;
        }

        throw new FormatException(
            "output.uploadUrl must be an HTTPS URL without credentials or a fragment."
        );
    }

    private static double? NullablePositive(JsonProperty property)
    {
        if (property.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.Value.ValueKind == JsonValueKind.Number && property.Value.GetDouble() > 0
            ? property.Value.GetDouble()
            : throw new FormatException(property.Name + " must be a positive number.");
    }

    private static FormatException Wrong(string name)
    {
        return new FormatException(name + " has the wrong type.");
    }
}
