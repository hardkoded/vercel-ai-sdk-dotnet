// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.AssemblyAI;

/// <summary>
/// Provider options for AssemblyAI speech generation, read from <c>providerOptions.assemblyai</c>.
/// Maps to <c>AssemblyAISpeechModelOptions</c> of <c>assemblyai-speech-model-options.ts</c>.
/// </summary>
public sealed class AssemblyAISpeechModelOptions
{
    private static readonly string[] Encodings = ["pcm_s16le", "pcm_mulaw", "pcm_alaw", "mp3"];

    private static readonly string[] Containers = ["raw", "wav"];

    private static readonly int[] SampleRates = [8000, 16000, 22050, 24000, 44100, 48000];

    private static readonly int[] BitRates = [32000, 48000, 64000, 96000, 128000];

    /// <summary>Audio encoding of the output (<c>pcm_s16le</c>, <c>pcm_mulaw</c>, <c>pcm_alaw</c>, or <c>mp3</c>). Overrides the encoding derived from the output format. Maps to <c>encoding</c>.</summary>
    public string? Encoding { get; set; }

    /// <summary><c>raw</c> returns the encoded samples alone; <c>wav</c> wraps them in a WAV file. Requires a <c>pcm_*</c> encoding. Maps to <c>container</c>.</summary>
    public string? Container { get; set; }

    /// <summary>Sample rate of the output audio in Hz (8000, 16000, 22050, 24000, 44100, 48000). Maps to <c>sampleRate</c>.</summary>
    public int? SampleRate { get; set; }

    /// <summary>MP3 bit rate in bits per second (32000, 48000, 64000, 96000, 128000). MP3 output only. Maps to <c>bitRate</c>.</summary>
    public int? BitRate { get; set; }

    /// <summary>Reads <c>providerOptions.assemblyai</c>. Maps to <c>parseProviderOptions</c> with <c>assemblyaiSpeechModelOptionsSchema</c>.</summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong shape or an unsupported value.</exception>
    public static AssemblyAISpeechModelOptions? Parse(
        IReadOnlyDictionary<string, JsonElement>? providerOptions
    )
    {
        if (
            providerOptions is null
            || !providerOptions.TryGetValue(AssemblyAIProvider.ProviderId, out var raw)
            || raw.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return null;
        }

        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentException(
                "providerOptions.assemblyai",
                raw.ToString(),
                "Expected an object."
            );
        }

        return new AssemblyAISpeechModelOptions
        {
            Encoding = OptionString(raw, "encoding", Encodings),
            Container = OptionString(raw, "container", Containers),
            SampleRate = OptionNumber(raw, "sampleRate", SampleRates),
            BitRate = OptionNumber(raw, "bitRate", BitRates),
        };
    }

    private static string? OptionString(JsonElement options, string name, string[] allowed)
    {
        if (options.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
            if (text is null || Array.IndexOf(allowed, text) < 0)
            {
                throw Invalid(name, value, string.Join(", ", allowed));
            }

            return text;
        }

        return null;
    }

    private static int? OptionNumber(JsonElement options, string name, int[] allowed)
    {
        if (options.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            if (
                value.ValueKind != JsonValueKind.Number
                || !value.TryGetInt32(out var number)
                || Array.IndexOf(allowed, number) < 0
            )
            {
                throw Invalid(name, value, string.Join(", ", allowed));
            }

            return number;
        }

        return null;
    }

    private static InvalidArgumentException Invalid(string name, JsonElement value, string allowed)
    {
        return new InvalidArgumentException(
            "providerOptions.assemblyai." + name,
            value.ToString(),
            "Expected one of: " + allowed + "."
        );
    }
}
