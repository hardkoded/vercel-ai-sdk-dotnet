// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Xai;

/// <summary>Options of <c>streaming</c> for streaming speech-to-text over WebSocket.</summary>
public sealed class XaiTranscriptionStreamingOptions
{
    /// <summary>Emit interim transcript results while speech is being processed.</summary>
    public bool? InterimResults { get; set; }

    /// <summary>Silence duration in milliseconds before an utterance-final event. Between 0 and 5000.</summary>
    public int? Endpointing { get; set; }

    /// <summary>End-of-turn detection threshold between 0 and 1. When set, enables Smart Turn.</summary>
    public double? SmartTurn { get; set; }

    /// <summary>Maximum silence duration in milliseconds before forcing <c>speech_final</c>. Between 1 and 5000.</summary>
    public int? SmartTurnTimeout { get; set; }
}

/// <summary>
/// xAI transcription options, read from <c>providerOptions.xai</c>. Maps to <c>XaiTranscriptionModelOptions</c> of
/// <c>xai-transcription-model-options.ts</c>.
/// </summary>
public sealed class XaiTranscriptionModelOptions
{
    private static readonly int[] SampleRates = { 8000, 16000, 22050, 24000, 44100, 48000 };

    /// <summary>
    /// Input audio encoding for raw, headerless audio: <c>pcm</c>, <c>mulaw</c>, or <c>alaw</c>. Container formats such as
    /// Ogg/Opus are auto-detected, so do not set this for them. <c>opus</c> is for streaming only, with one raw Opus packet per chunk.
    /// </summary>
    public string? AudioFormat { get; set; }

    /// <summary>Sample rate of the input audio in Hz.</summary>
    public int? SampleRate { get; set; }

    /// <summary>Language code used for inverse text normalization.</summary>
    public string? Language { get; set; }

    /// <summary>Enable inverse text normalization. Requires <see cref="Language"/>.</summary>
    public bool? Format { get; set; }

    /// <summary>Enable per-channel transcription for multichannel audio.</summary>
    public bool? Multichannel { get; set; }

    /// <summary>Number of interleaved audio channels. Between 2 and 8.</summary>
    public int? Channels { get; set; }

    /// <summary>Enable speaker diarization.</summary>
    public bool? Diarize { get; set; }

    /// <summary>Terms to bias transcription toward.</summary>
    public IReadOnlyList<string>? Keyterm { get; set; }

    /// <summary>Include filler words such as "uh" and "um" in the transcript.</summary>
    public bool? FillerWords { get; set; }

    /// <summary>Voice activity detection threshold between 0 and 1. Defaults to 0.5 for <c>transcribe</c> and 0.08 for streaming. 0 disables the filter.</summary>
    public double? VadThreshold { get; set; }

    /// <summary>Options for streaming speech-to-text over WebSocket.</summary>
    public XaiTranscriptionStreamingOptions? Streaming { get; set; }

    /// <summary>
    /// Reads and validates <c>providerOptions.xai</c>. Returns null when the namespace is missing. Maps to
    /// <c>parseProviderOptions</c> with <c>xaiTranscriptionModelOptionsSchema</c>.
    /// </summary>
    internal static XaiTranscriptionModelOptions? Parse(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty("xai", out var xai)
            || xai.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (xai.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(xai, "expected an object");
        }

        var options = new XaiTranscriptionModelOptions();
        foreach (var property in xai.EnumerateObject())
        {
            var value = property.Value;
            if (
                value.ValueKind == JsonValueKind.Null
                && property.Name != "vadThreshold"
                && property.Name != "streaming"
            )
            {
                continue;
            }

            switch (property.Name)
            {
                case "audioFormat":
                    options.AudioFormat =
                        value.ValueKind == JsonValueKind.String
                        && value.GetString() is "pcm" or "mulaw" or "alaw" or "opus"
                            ? value.GetString()
                            : throw Invalid(
                                xai,
                                "audioFormat must be one of pcm, mulaw, alaw, opus"
                            );
                    break;
                case "sampleRate":
                    options.SampleRate =
                        value.ValueKind == JsonValueKind.Number
                        && value.TryGetInt32(out var rate)
                        && Array.IndexOf(SampleRates, rate) >= 0
                            ? rate
                            : throw Invalid(
                                xai,
                                "sampleRate must be one of " + string.Join(", ", SampleRates)
                            );
                    break;
                case "language":
                    options.Language = String(xai, value, "language");
                    break;
                case "format":
                    options.Format = Bool(xai, value, "format");
                    break;
                case "multichannel":
                    options.Multichannel = Bool(xai, value, "multichannel");
                    break;
                case "channels":
                    options.Channels =
                        value.ValueKind == JsonValueKind.Number
                        && value.TryGetInt32(out var channels)
                        && channels >= 2
                        && channels <= 8
                            ? channels
                            : throw Invalid(xai, "channels must be an integer between 2 and 8");
                    break;
                case "diarize":
                    options.Diarize = Bool(xai, value, "diarize");
                    break;
                case "keyterm":
                    options.Keyterm =
                        value.ValueKind == JsonValueKind.String ? new[] { value.GetString()! }
                        : value.ValueKind == JsonValueKind.Array
                        && value
                            .EnumerateArray()
                            .All(item => item.ValueKind == JsonValueKind.String)
                            ? value.EnumerateArray().Select(item => item.GetString()!).ToArray()
                        : throw Invalid(xai, "keyterm must be a string or an array of strings");
                    break;
                case "fillerWords":
                    options.FillerWords = Bool(xai, value, "fillerWords");
                    break;
                case "vadThreshold":
                    options.VadThreshold =
                        value.ValueKind == JsonValueKind.Number
                        && value.GetDouble() >= 0
                        && value.GetDouble() <= 1
                            ? value.GetDouble()
                        : value.ValueKind == JsonValueKind.Null ? null
                        : throw Invalid(xai, "vadThreshold must be a number between 0 and 1");
                    break;
                case "streaming":
                    options.Streaming =
                        value.ValueKind == JsonValueKind.Null ? null : ParseStreaming(xai, value);
                    break;
            }
        }

        return options;
    }

    private static XaiTranscriptionStreamingOptions ParseStreaming(
        JsonElement xai,
        JsonElement streaming
    )
    {
        if (streaming.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(xai, "streaming must be an object");
        }

        var options = new XaiTranscriptionStreamingOptions();
        foreach (var property in streaming.EnumerateObject())
        {
            var value = property.Value;
            switch (property.Name)
            {
                case "interimResults":
                    options.InterimResults = Bool(xai, value, "streaming.interimResults");
                    break;
                case "endpointing":
                    options.Endpointing = Integer(xai, value, "streaming.endpointing", 0, 5000);
                    break;
                case "smartTurn":
                    options.SmartTurn =
                        value.ValueKind == JsonValueKind.Number
                        && value.GetDouble() >= 0
                        && value.GetDouble() <= 1
                            ? value.GetDouble()
                            : throw Invalid(
                                xai,
                                "streaming.smartTurn must be a number between 0 and 1"
                            );
                    break;
                case "smartTurnTimeout":
                    options.SmartTurnTimeout = Integer(
                        xai,
                        value,
                        "streaming.smartTurnTimeout",
                        1,
                        5000
                    );
                    break;
            }
        }

        return options;
    }

    private static int Integer(JsonElement xai, JsonElement value, string name, int min, int max)
    {
        return
            value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            && number >= min
            && number <= max
            ? number
            : throw Invalid(xai, name + " must be an integer between " + min + " and " + max);
    }

    private static bool Bool(JsonElement xai, JsonElement value, string name)
    {
        return value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : throw Invalid(xai, name + " must be a boolean");
    }

    private static string String(JsonElement xai, JsonElement value, string name)
    {
        return value.ValueKind == JsonValueKind.String
            ? value.GetString()!
            : throw Invalid(xai, name + " must be a string");
    }

    private static InvalidArgumentException Invalid(JsonElement xai, string reason)
    {
        return new InvalidArgumentException(
            "providerOptions",
            xai.GetRawText(),
            "Invalid xai provider options: " + reason + "."
        );
    }
}
