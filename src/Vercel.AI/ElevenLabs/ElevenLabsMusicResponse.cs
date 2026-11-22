// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.ElevenLabs;

/// <summary>One lyric word with its start and end time in milliseconds.</summary>
public sealed class ElevenLabsMusicWordTimestamp
{
    /// <summary>Creates a word timestamp.</summary>
    public ElevenLabsMusicWordTimestamp(string word, double startMs, double endMs)
    {
        Word = word;
        StartMs = startMs;
        EndMs = endMs;
    }

    /// <summary>Lyric word.</summary>
    public string Word { get; }

    /// <summary>Start time in milliseconds.</summary>
    public double StartMs { get; }

    /// <summary>End time in milliseconds.</summary>
    public double EndMs { get; }
}

/// <summary>Audio and optional lyric word timings from <c>/v1/music/detailed</c>. Maps to <c>ElevenLabsMusicTimestampsResponse</c>.</summary>
public sealed class ElevenLabsMusicTimestampsResponse
{
    /// <summary>Creates a response.</summary>
    public ElevenLabsMusicTimestampsResponse(
        byte[] audio,
        IReadOnlyList<ElevenLabsMusicWordTimestamp>? wordTimestamps
    )
    {
        Audio = audio;
        WordTimestamps = wordTimestamps;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>Lyric word timings. Null when the response carries none.</summary>
    public IReadOnlyList<ElevenLabsMusicWordTimestamp>? WordTimestamps { get; }
}

/// <summary>Reads the <c>multipart/mixed</c> response of <c>/v1/music/detailed</c>. Maps to <c>elevenlabs-music-response.ts</c>.</summary>
public static class ElevenLabsMusicResponse
{
    private static readonly Regex BoundaryPattern = new(
        "(?:^|;)\\s*boundary=(?:\"([^\"]+)\"|([^;\\s]+))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    private static readonly Regex MultipartMixedPattern = new(
        "^multipart/mixed\\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    private static readonly Regex PartTypePattern = new(
        "^content-type:\\s*([^;\\r\\n]+)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    private static readonly byte[] HeaderSeparator = [13, 10, 13, 10];

    /// <summary>
    /// Creates a handler for the detailed music response: JSON metadata and binary audio as <c>multipart/mixed</c>.
    /// Maps to <c>createMusicTimestampsResponseHandler</c>.
    /// </summary>
    public static JsonResponseHandler<ElevenLabsMusicTimestampsResponse> CreateMusicTimestampsResponseHandler()
    {
        return async (response, url, requestBodyValues, cancellationToken) =>
        {
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var contentType = response.Content?.Headers.ContentType?.ToString() ?? string.Empty;
            var boundary = BoundaryPattern.Match(contentType);
            if (!MultipartMixedPattern.IsMatch(contentType) || !boundary.Success)
            {
                throw new InvalidResponseDataException(
                    contentType,
                    "ElevenLabs music response is missing a multipart boundary."
                );
            }

            var bytes = await ResponseBodies
                .ReadResponseWithSizeLimitAsync(response, url, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var delimiter = Encoding.UTF8.GetBytes(
                "--"
                    + (
                        boundary.Groups[1].Success
                            ? boundary.Groups[1].Value
                            : boundary.Groups[2].Value
                    )
            );
            byte[]? audio = null;
            JsonElement? metadata = null;
            IReadOnlyList<ElevenLabsMusicWordTimestamp>? wordTimestamps = null;
            var position = FindBoundary(bytes, delimiter, 0);
            var closed = false;

            while (position != -1)
            {
                var start = position + delimiter.Length;
                if (At(bytes, start) == 45 && At(bytes, start + 1) == 45)
                {
                    closed = true;
                    break;
                }

                var next = FindBoundary(bytes, delimiter, start + 2);
                if (next == -1)
                {
                    break;
                }

                var end = next - 2; // Remove only the CRLF framing this part.
                var headersEnd = FindBytes(bytes, HeaderSeparator, start + 2);
                if (headersEnd == -1 || headersEnd + HeaderSeparator.Length > end)
                {
                    throw new InvalidResponseDataException(
                        contentType,
                        "ElevenLabs music response contains malformed part headers."
                    );
                }

                var headers = Encoding.UTF8.GetString(bytes, start + 2, headersEnd - (start + 2));
                var partType = PartTypePattern.Match(headers) is { Success: true } typeMatch
                    ? typeMatch.Groups[1].Value.Trim().ToLowerInvariant()
                    : null;
                var bodyStart = headersEnd + HeaderSeparator.Length;
                if (partType == "application/json")
                {
                    if (metadata != null)
                    {
                        throw new InvalidResponseDataException(
                            contentType,
                            "ElevenLabs music response contains multiple metadata parts."
                        );
                    }

                    var text = Encoding.UTF8.GetString(bytes, bodyStart, end - bodyStart);
                    (metadata, wordTimestamps) = ParseMetadata(text);
                }
                else if (
                    partType != null
                    && partType.StartsWith("audio/", StringComparison.Ordinal)
                )
                {
                    if (audio != null)
                    {
                        throw new InvalidResponseDataException(
                            contentType,
                            "ElevenLabs music response contains multiple audio parts."
                        );
                    }

                    audio = new byte[end - bodyStart];
                    Array.Copy(bytes, bodyStart, audio, 0, audio.Length);
                }

                position = next;
            }

            if (!closed || audio == null || metadata == null)
            {
                throw new InvalidResponseDataException(
                    contentType,
                    "ElevenLabs music response must contain complete metadata and audio parts."
                );
            }

            return new JsonResponseHandlerResult<ElevenLabsMusicTimestampsResponse>(
                new ElevenLabsMusicTimestampsResponse(audio, wordTimestamps),
                metadata.Value,
                JsonStreams.ExtractResponseHeaders(response)
            );
        };
    }

    private static (
        JsonElement Raw,
        IReadOnlyList<ElevenLabsMusicWordTimestamp>? Words
    ) ParseMetadata(string text)
    {
        var parsed = JsonParsing.SafeParse(text);
        if (!parsed.Success || parsed.RawValue is not { } raw)
        {
            throw parsed.Error
                ?? new JsonParseException(text, new JsonException("Invalid JSON response"));
        }

        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw new TypeValidationException("Type validation failed: expected an object.", raw);
        }

        if (
            !raw.TryGetProperty("words_timestamps", out var list)
            || list.ValueKind == JsonValueKind.Null
        )
        {
            return (raw, null);
        }

        if (list.ValueKind != JsonValueKind.Array)
        {
            throw new TypeValidationException(
                "Type validation failed: words_timestamps must be an array.",
                raw
            );
        }

        var words = new List<ElevenLabsMusicWordTimestamp>();
        foreach (var item in list.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("word", out var word)
                || word.ValueKind != JsonValueKind.String
                || !item.TryGetProperty("start_ms", out var startMs)
                || startMs.ValueKind != JsonValueKind.Number
                || !item.TryGetProperty("end_ms", out var endMs)
                || endMs.ValueKind != JsonValueKind.Number
            )
            {
                throw new TypeValidationException(
                    "Type validation failed: each words_timestamps entry needs a string word and numeric start_ms and end_ms.",
                    raw
                );
            }

            words.Add(
                new ElevenLabsMusicWordTimestamp(
                    word.GetString()!,
                    startMs.GetDouble(),
                    endMs.GetDouble()
                )
            );
        }

        return (raw, words);
    }

    private static int At(byte[] bytes, int index) =>
        index >= 0 && index < bytes.Length ? bytes[index] : -1;

    private static int FindBytes(byte[] bytes, byte[] pattern, int from)
    {
        for (var i = Math.Max(from, 0); i <= bytes.Length - pattern.Length; i++)
        {
            var match = true;
            for (var j = 0; j < pattern.Length; j++)
            {
                if (bytes[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindBoundary(byte[] bytes, byte[] pattern, int from)
    {
        for (var i = FindBytes(bytes, pattern, from); i != -1; i = FindBytes(bytes, pattern, i + 1))
        {
            var end = i + pattern.Length;
            var lineStart = i == 0 || (At(bytes, i - 2) == 13 && At(bytes, i - 1) == 10);
            var lineEnd =
                (At(bytes, end) == 13 && At(bytes, end + 1) == 10)
                || (At(bytes, end) == 45 && At(bytes, end + 1) == 45);
            if (lineStart && lineEnd)
            {
                return i;
            }
        }

        return -1;
    }
}
