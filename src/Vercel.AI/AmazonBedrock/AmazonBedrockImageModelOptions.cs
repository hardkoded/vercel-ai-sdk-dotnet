// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;

namespace Vercel.AI.AmazonBedrock;

/// <summary>The <c>amazonBedrock</c> provider options of an image call. Maps to <c>amazonBedrockImageModelOptionsSchema</c>.</summary>
internal sealed class AmazonBedrockImageModelOptions
{
    private static readonly string[] Qualities = { "standard", "premium" };

    private static readonly string[] Styles =
    {
        "3D_ANIMATED_FAMILY_FILM",
        "DESIGN_SKETCH",
        "FLAT_VECTOR_ILLUSTRATION",
        "GRAPHIC_NOVEL_ILLUSTRATION",
        "MAXIMALISM",
        "MIDCENTURY_RETRO",
        "PHOTOREALISM",
        "SOFT_DIGITAL_PAINTING",
    };

    private static readonly string[] TaskTypes =
    {
        "TEXT_IMAGE",
        "IMAGE_VARIATION",
        "INPAINTING",
        "OUTPAINTING",
        "BACKGROUND_REMOVAL",
    };

    private static readonly string[] OutPaintingModes = { "DEFAULT", "PRECISE" };

    public string? Quality { get; private set; }

    public string? NegativeText { get; private set; }

    public double? CfgScale { get; private set; }

    public string? Style { get; private set; }

    public string? TaskType { get; private set; }

    public string? MaskPrompt { get; private set; }

    public string? OutPaintingMode { get; private set; }

    public double? SimilarityStrength { get; private set; }

    /// <summary>
    /// Reads the <c>amazonBedrock</c> entry of <paramref name="providerOptions"/>, falling back to the legacy
    /// <c>bedrock</c> entry. Unknown keys are ignored.
    /// </summary>
    /// <exception cref="InvalidArgumentException">A known option has the wrong type or value.</exception>
    public static AmazonBedrockImageModelOptions Parse(JsonElement? providerOptions)
    {
        var options = new AmazonBedrockImageModelOptions();
        foreach (var key in new[] { "amazonBedrock", "bedrock" })
        {
            if (OpenAIJson.Child(providerOptions, key) is not { } child)
            {
                continue;
            }

            if (child.ValueKind != JsonValueKind.Object)
            {
                throw Invalid(key, child, "expected an object");
            }

            options.Quality = ReadString(key, child, "quality", Qualities);
            options.NegativeText = ReadString(key, child, "negativeText", null);
            options.CfgScale = ReadNumber(key, child, "cfgScale");
            options.Style = ReadString(key, child, "style", Styles);
            options.TaskType = ReadString(key, child, "taskType", TaskTypes);
            options.MaskPrompt = ReadString(key, child, "maskPrompt", null);
            options.OutPaintingMode = ReadString(key, child, "outPaintingMode", OutPaintingModes);
            options.SimilarityStrength = ReadNumber(key, child, "similarityStrength");
            return options;
        }

        return options;
    }

    private static string? ReadString(
        string key,
        JsonElement values,
        string name,
        string[]? allowed
    )
    {
        if (!values.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw Invalid(key, values, name + " must be a string");
        }

        var text = value.GetString()!;
        if (allowed != null && Array.IndexOf(allowed, text) < 0)
        {
            throw Invalid(key, values, name + " must be one of " + string.Join(", ", allowed));
        }

        return text;
    }

    private static double? ReadNumber(string key, JsonElement values, string name)
    {
        if (!values.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : throw Invalid(key, values, name + " must be a number");
    }

    private static InvalidArgumentException Invalid(string key, JsonElement values, string message)
    {
        return new InvalidArgumentException(
            "providerOptions",
            values,
            "invalid " + key + " provider options: " + message
        );
    }
}
