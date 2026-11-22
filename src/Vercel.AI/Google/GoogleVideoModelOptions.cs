// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Google;

/// <summary>A reference image for Veo style or asset guidance. Upstream inline type of <c>referenceImages</c>.</summary>
public sealed class GoogleVideoModelReferenceImage
{
    /// <summary>Base64 image bytes.</summary>
    public string? BytesBase64Encoded { get; set; }

    /// <summary>Google Cloud Storage URI.</summary>
    public string? GcsUri { get; set; }

    /// <summary>The object as the user wrote it, sent as is when neither field is set.</summary>
    internal JsonElement Raw { get; set; }
}

/// <summary>Provider options for Google video generation, read from <c>providerOptions.google</c>. Upstream <c>GoogleVideoModelOptions</c>.</summary>
public sealed class GoogleVideoModelOptions
{
    private static readonly string[] PersonGenerations = ["dont_allow", "allow_adult", "allow_all"];

    /// <summary>Polling interval in milliseconds. Must be positive.</summary>
    public double? PollIntervalMs { get; set; }

    /// <summary>Polling timeout in milliseconds. Must be positive.</summary>
    public double? PollTimeoutMs { get; set; }

    /// <summary><c>dont_allow</c>, <c>allow_adult</c>, or <c>allow_all</c>.</summary>
    public string? PersonGeneration { get; set; }

    /// <summary>Negative prompt.</summary>
    public string? NegativePrompt { get; set; }

    /// <summary>Reference images for style or asset guidance.</summary>
    public List<GoogleVideoModelReferenceImage>? ReferenceImages { get; set; }

    /// <summary>Any other option. It is copied into the request <c>parameters</c>. Upstream <c>[key: string]: unknown</c>.</summary>
    public Dictionary<string, JsonElement> AdditionalProperties { get; } =
        new(StringComparer.Ordinal);

    /// <summary>Reads <c>providerOptions.google</c>. Returns null when it is absent. Maps to <c>parseProviderOptions</c> with <c>googleVideoModelOptionsSchema</c>.</summary>
    public static GoogleVideoModelOptions? Parse(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(GoogleProvider.ProviderName, out var raw)
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
                "providerOptions.google",
                null,
                "Invalid Google video provider options: " + exception.Message
            );
        }
    }

    private static GoogleVideoModelOptions Read(JsonElement raw)
    {
        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("expected an object.");
        }

        var options = new GoogleVideoModelOptions();
        foreach (var property in raw.EnumerateObject())
        {
            var value = property.Value;
            var isNull = value.ValueKind == JsonValueKind.Null;
            switch (property.Name)
            {
                case "pollIntervalMs":
                    options.PollIntervalMs = isNull ? null : Positive(property);
                    break;
                case "pollTimeoutMs":
                    options.PollTimeoutMs = isNull ? null : Positive(property);
                    break;
                case "personGeneration":
                    if (!isNull)
                    {
                        var text = Text(property);
                        if (Array.IndexOf(PersonGenerations, text) < 0)
                        {
                            throw new FormatException(
                                "personGeneration must be one of dont_allow, allow_adult, allow_all."
                            );
                        }

                        options.PersonGeneration = text;
                    }

                    break;
                case "negativePrompt":
                    options.NegativePrompt = isNull ? null : Text(property);
                    break;
                case "referenceImages":
                    if (!isNull)
                    {
                        if (value.ValueKind != JsonValueKind.Array)
                        {
                            throw new FormatException("referenceImages must be an array.");
                        }

                        options.ReferenceImages = [];
                        foreach (var item in value.EnumerateArray())
                        {
                            if (item.ValueKind != JsonValueKind.Object)
                            {
                                throw new FormatException("referenceImages must hold objects.");
                            }

                            var image = new GoogleVideoModelReferenceImage { Raw = item.Clone() };
                            foreach (var field in item.EnumerateObject())
                            {
                                if (field.Value.ValueKind == JsonValueKind.Null)
                                {
                                    continue;
                                }

                                if (field.Name == "bytesBase64Encoded")
                                {
                                    image.BytesBase64Encoded = Text(field);
                                }
                                else if (field.Name == "gcsUri")
                                {
                                    image.GcsUri = Text(field);
                                }
                            }

                            options.ReferenceImages.Add(image);
                        }
                    }

                    break;
                default:
                    options.AdditionalProperties[property.Name] = value.Clone();
                    break;
            }
        }

        return options;
    }

    private static double Positive(JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.Number || property.Value.GetDouble() <= 0)
        {
            throw new FormatException(property.Name + " must be a positive number.");
        }

        return property.Value.GetDouble();
    }

    private static string Text(JsonProperty property)
    {
        return property.Value.ValueKind == JsonValueKind.String
            ? property.Value.GetString()!
            : throw new FormatException(property.Name + " must be a string.");
    }
}
