// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Xai;

/// <summary>Provider options for xAI speech generation, read from <c>providerOptions.xai</c>. Maps to <c>XaiSpeechModelOptions</c>.</summary>
public sealed class XaiSpeechModelOptions
{
    private static readonly int[] SampleRates = [8000, 16000, 22050, 24000, 44100, 48000];

    private static readonly int[] BitRates = [32000, 64000, 96000, 128000, 192000];

    private static readonly int[] StreamingLatencies = [0, 1, 2];

    /// <summary>Sample rate of the generated audio in Hz. Maps to <c>sampleRate</c>.</summary>
    public int? SampleRate { get; set; }

    /// <summary>MP3 bit rate in bits per second. Only applies when the output format is mp3. Maps to <c>bitRate</c>.</summary>
    public int? BitRate { get; set; }

    /// <summary>Reduces time to first audio chunk, trading some quality for latency. Maps to <c>optimizeStreamingLatency</c>.</summary>
    public int? OptimizeStreamingLatency { get; set; }

    /// <summary>Normalizes written-form text into spoken-form text before synthesis. Maps to <c>textNormalization</c>.</summary>
    public bool? TextNormalization { get; set; }

    /// <summary>
    /// Returns character-level timing metadata alongside the audio, exposed through <c>providerMetadata.xai</c>. Maps to <c>withTimestamps</c>.
    /// </summary>
    public bool? WithTimestamps { get; set; }

    /// <summary>Map of phrases to spoken substitutions applied before synthesis. Maps to <c>replace</c>.</summary>
    public IReadOnlyDictionary<string, string>? Replace { get; set; }

    /// <summary>Reads <c>providerOptions.xai</c>. Maps to <c>parseProviderOptions</c> with <c>xaiSpeechModelOptionsSchema</c>.</summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong shape or an unsupported value.</exception>
    public static XaiSpeechModelOptions? Parse(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(XaiProvider.ProviderId, out var raw)
            || raw.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return null;
        }

        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("expected an object");
        }

        var options = new XaiSpeechModelOptions
        {
            SampleRate = OneOf(raw, "sampleRate", SampleRates),
            BitRate = OneOf(raw, "bitRate", BitRates),
            OptimizeStreamingLatency = OneOf(raw, "optimizeStreamingLatency", StreamingLatencies),
        };

        if (Present(raw, "textNormalization", out var normalization))
        {
            options.TextNormalization = Flag(normalization, "textNormalization");
        }

        if (Present(raw, "withTimestamps", out var timestamps))
        {
            options.WithTimestamps = Flag(timestamps, "withTimestamps");
        }

        if (Present(raw, "replace", out var replace))
        {
            if (replace.ValueKind != JsonValueKind.Object)
            {
                throw Invalid("replace must be an object of strings");
            }

            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in replace.EnumerateObject())
            {
                if (pair.Value.ValueKind != JsonValueKind.String)
                {
                    throw Invalid("replace must be an object of strings");
                }

                map[pair.Name] = pair.Value.GetString()!;
            }

            options.Replace = map;
        }

        return options;
    }

    private static int? OneOf(JsonElement section, string name, int[] allowed)
    {
        if (!Present(section, name, out var value))
        {
            return null;
        }

        if (
            value.ValueKind != JsonValueKind.Number
            || !value.TryGetInt32(out var number)
            || Array.IndexOf(allowed, number) < 0
        )
        {
            throw Invalid(name + " must be one of " + string.Join(", ", allowed));
        }

        return number;
    }

    private static bool Flag(JsonElement value, string name)
    {
        return value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : throw Invalid(name + " must be a boolean");
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
