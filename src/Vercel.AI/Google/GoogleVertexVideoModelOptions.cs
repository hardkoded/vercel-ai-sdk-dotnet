// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Google;

/// <summary>
/// Port of <c>GoogleVertexVideoModelOptions</c> (<c>google-vertex-video-model-options.ts</c>). Read from
/// <c>providerOptions.googleVertex</c>, or the legacy <c>providerOptions.vertex</c>.
/// </summary>
public sealed class GoogleVertexVideoModelOptions
{
    private static readonly string[] PersonGenerationValues =
    [
        "dont_allow",
        "allow_adult",
        "allow_all",
    ];

    /// <summary>Polling interval in milliseconds. Positive.</summary>
    public double? PollIntervalMs { get; set; }

    /// <summary>Polling timeout in milliseconds. Positive.</summary>
    public double? PollTimeoutMs { get; set; }

    /// <summary><c>dont_allow</c>, <c>allow_adult</c>, or <c>allow_all</c>.</summary>
    public string? PersonGeneration { get; set; }

    /// <summary>Negative prompt.</summary>
    public string? NegativePrompt { get; set; }

    /// <summary>Whether to generate audio.</summary>
    public bool? GenerateAudio { get; set; }

    /// <summary>Output directory in Google Cloud Storage.</summary>
    public string? GcsOutputDirectory { get; set; }

    /// <summary>Reference images, each with <c>bytesBase64Encoded</c> or <c>gcsUri</c>.</summary>
    public JsonElement? ReferenceImages { get; set; }

    /// <summary>Options this class does not model, sent to the API as they are. Upstream <c>z.looseObject</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; set; } =
        new Dictionary<string, JsonElement>();

    /// <summary>
    /// Maps <c>googleVertexVideoModelOptionsSchema</c>. Reads <c>providerOptions.googleVertex</c> first, then
    /// <c>providerOptions.vertex</c>. Returns null when neither is present. An invalid value throws <see cref="InvalidArgumentException"/>.
    /// </summary>
    public static GoogleVertexVideoModelOptions? Parse(JsonElement providerOptions)
    {
        foreach (var provider in new[] { "googleVertex", "vertex" })
        {
            if (
                providerOptions.ValueKind == JsonValueKind.Object
                && providerOptions.TryGetProperty(provider, out var raw)
                && raw.ValueKind != JsonValueKind.Null
            )
            {
                return Read(provider, raw);
            }
        }

        return null;
    }

    private static GoogleVertexVideoModelOptions Read(string provider, JsonElement raw)
    {
        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(provider, "expected an object.");
        }

        var options = new GoogleVertexVideoModelOptions();
        var additional = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in raw.EnumerateObject())
        {
            var value = property.Value;
            var isNull = value.ValueKind == JsonValueKind.Null;
            switch (property.Name)
            {
                case "pollIntervalMs":
                    options.PollIntervalMs = isNull ? null : Positive(provider, property);
                    break;
                case "pollTimeoutMs":
                    options.PollTimeoutMs = isNull ? null : Positive(provider, property);
                    break;
                case "personGeneration":
                    options.PersonGeneration = isNull ? null : Text(provider, property);
                    if (
                        options.PersonGeneration != null
                        && Array.IndexOf(PersonGenerationValues, options.PersonGeneration) < 0
                    )
                    {
                        throw Invalid(
                            provider,
                            "personGeneration must be dont_allow, allow_adult, or allow_all."
                        );
                    }

                    break;
                case "negativePrompt":
                    options.NegativePrompt = isNull ? null : Text(provider, property);
                    break;
                case "generateAudio":
                    options.GenerateAudio = isNull ? null : Flag(provider, property);
                    break;
                case "gcsOutputDirectory":
                    options.GcsOutputDirectory = isNull ? null : Text(provider, property);
                    break;
                case "referenceImages":
                    options.ReferenceImages = isNull ? null : ReferenceImageList(provider, value);
                    break;
                default:
                    additional[property.Name] = value.Clone();
                    break;
            }
        }

        options.AdditionalProperties = additional;
        return options;
    }

    private static double Positive(string provider, JsonProperty property)
    {
        if (
            property.Value.ValueKind != JsonValueKind.Number
            || !property.Value.TryGetDouble(out var number)
            || number <= 0
        )
        {
            throw Invalid(provider, property.Name + " must be a positive number.");
        }

        return number;
    }

    private static string Text(string provider, JsonProperty property)
    {
        return property.Value.ValueKind == JsonValueKind.String
            ? property.Value.GetString()!
            : throw Invalid(provider, property.Name + " must be a string.");
    }

    private static bool Flag(string provider, JsonProperty property)
    {
        return property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.Value.GetBoolean()
            : throw Invalid(provider, property.Name + " must be a boolean.");
    }

    private static JsonElement ReferenceImageList(string provider, JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw Invalid(provider, "referenceImages must be an array.");
        }

        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw Invalid(provider, "referenceImages must contain objects.");
            }

            foreach (var name in new[] { "bytesBase64Encoded", "gcsUri" })
            {
                if (
                    item.TryGetProperty(name, out var field)
                    && field.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)
                )
                {
                    throw Invalid(provider, "referenceImages." + name + " must be a string.");
                }
            }
        }

        return value.Clone();
    }

    private static InvalidArgumentException Invalid(string provider, string message)
    {
        return new InvalidArgumentException(
            "providerOptions." + provider,
            null,
            "Invalid Google Vertex video provider options: " + message
        );
    }
}
