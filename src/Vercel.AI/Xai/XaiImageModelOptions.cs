// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Xai;

/// <summary>Provider options for xAI image generation, read from <c>providerOptions.xai</c>. Maps to <c>XaiImageModelOptions</c>.</summary>
public sealed class XaiImageModelOptions
{
    private static readonly string[] Resolutions = ["1k", "1.5k", "2k"];

    private static readonly string[] Qualities = ["low", "medium", "high", "auto"];

    /// <summary>Aspect ratio, used when the call sets no aspect ratio. Maps to <c>aspect_ratio</c>.</summary>
    public string? AspectRatio { get; set; }

    /// <summary>Output format such as <c>jpeg</c>. Maps to <c>output_format</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>Maps to <c>sync_mode</c>.</summary>
    public bool? SyncMode { get; set; }

    /// <summary><c>1k</c>, <c>1.5k</c>, or <c>2k</c>. Maps to <c>resolution</c>.</summary>
    public string? Resolution { get; set; }

    /// <summary><c>low</c>, <c>medium</c>, <c>high</c>, or <c>auto</c>. Maps to <c>quality</c>.</summary>
    public string? Quality { get; set; }

    /// <summary>A unique identifier representing the end user. Maps to <c>user</c>.</summary>
    public string? User { get; set; }

    /// <summary>Reads <c>providerOptions.xai</c>. Maps to <c>parseProviderOptions</c> with <c>xaiImageModelOptions</c>.</summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong shape.</exception>
    public static XaiImageModelOptions? Parse(JsonElement? providerOptions)
    {
        if (
            providerOptions is not { ValueKind: JsonValueKind.Object } options
            || !options.TryGetProperty(XaiProvider.ProviderId, out var raw)
            || raw.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return null;
        }

        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("expected an object");
        }

        return new XaiImageModelOptions
        {
            AspectRatio = Text(raw, "aspect_ratio"),
            OutputFormat = Text(raw, "output_format"),
            SyncMode = Flag(raw, "sync_mode"),
            Resolution = Text(raw, "resolution", Resolutions),
            Quality = Text(raw, "quality", Qualities),
            User = Text(raw, "user"),
        };
    }

    private static string? Text(JsonElement section, string name, string[]? allowed = null)
    {
        if (!Present(section, name, out var value))
        {
            return null;
        }

        if (
            value.ValueKind != JsonValueKind.String
            || (allowed != null && Array.IndexOf(allowed, value.GetString()) < 0)
        )
        {
            throw Invalid(
                name
                    + (
                        allowed == null
                            ? " must be a string"
                            : " must be one of " + string.Join(", ", allowed)
                    )
            );
        }

        return value.GetString();
    }

    private static bool? Flag(JsonElement section, string name)
    {
        if (!Present(section, name, out var value))
        {
            return null;
        }

        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw Invalid(name + " must be a boolean");
        }

        return value.GetBoolean();
    }

    private static bool Present(JsonElement section, string name, out JsonElement value)
    {
        return section.TryGetProperty(name, out value)
            && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined);
    }

    private static InvalidArgumentException Invalid(string reason)
    {
        return new InvalidArgumentException(
            "providerOptions",
            null,
            "invalid xai provider options: " + reason
        );
    }
}
