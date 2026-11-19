// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;

namespace Vercel.AI.Xai;

/// <summary>The <c>xai</c> provider options of an image call. Maps to <c>xaiImageModelOptions</c>.</summary>
internal sealed class XaiImageModelOptions
{
    private static readonly string[] Resolutions = { "1k", "1.5k", "2k" };
    private static readonly string[] Qualities = { "low", "medium", "high", "auto" };

    public string? AspectRatio { get; private set; }

    public string? OutputFormat { get; private set; }

    public bool? SyncMode { get; private set; }

    public string? Resolution { get; private set; }

    public string? Quality { get; private set; }

    public string? User { get; private set; }

    /// <summary>Reads the <c>xai</c> entry of <paramref name="providerOptions"/>. Unknown keys are ignored.</summary>
    /// <exception cref="InvalidArgumentException">A known option has the wrong type or value.</exception>
    public static XaiImageModelOptions Parse(JsonElement? providerOptions)
    {
        var options = new XaiImageModelOptions();
        var xai = OpenAIJson.Child(providerOptions, "xai");
        if (xai is not { ValueKind: JsonValueKind.Object } values)
        {
            return options;
        }

        options.AspectRatio = ReadString(values, "aspect_ratio", null);
        options.OutputFormat = ReadString(values, "output_format", null);
        options.Resolution = ReadString(values, "resolution", Resolutions);
        options.Quality = ReadString(values, "quality", Qualities);
        options.User = ReadString(values, "user", null);
        if (
            values.TryGetProperty("sync_mode", out var syncMode)
            && syncMode.ValueKind != JsonValueKind.Null
        )
        {
            options.SyncMode = syncMode.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? syncMode.GetBoolean()
                : throw Invalid(values, "sync_mode must be a boolean");
        }

        return options;
    }

    private static string? ReadString(JsonElement values, string name, string[]? allowed)
    {
        if (!values.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw Invalid(values, name + " must be a string");
        }

        var text = value.GetString()!;
        if (allowed != null && Array.IndexOf(allowed, text) < 0)
        {
            throw Invalid(values, name + " must be one of " + string.Join(", ", allowed));
        }

        return text;
    }

    private static InvalidArgumentException Invalid(JsonElement values, string message)
    {
        return new InvalidArgumentException(
            "providerOptions",
            values,
            "invalid xai provider options: " + message
        );
    }
}
