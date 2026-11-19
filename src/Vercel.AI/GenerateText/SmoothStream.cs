// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>
/// Detects the first chunk in a buffer. Maps to <c>ChunkDetector</c>.
/// Returns the first detected chunk, or <c>null</c> when no chunk was detected.
/// </summary>
public delegate string? ChunkDetector(string buffer);

/// <summary>
/// Splits text into segments, like <c>Intl.Segmenter</c>. .NET has no built-in word segmenter, so you provide one
/// (for example a wrapper over ICU) to chunk text with locale-aware word segmentation, which suits CJK languages.
/// </summary>
public interface ISegmenter
{
    /// <summary>Segments <paramref name="input"/>. Maps to <c>Intl.Segmenter.segment</c>. Only the first segment is read.</summary>
    IEnumerable<string> Segment(string input);
}

/// <summary>How <see cref="SmoothStream"/> chunks the text. Maps to the <c>'word' | 'line'</c> values of <c>chunking</c>.</summary>
public enum SmoothStreamChunking
{
    /// <summary>Stream word by word. This is the default.</summary>
    Word,

    /// <summary>Stream line by line.</summary>
    Line,
}

/// <summary>Options of <see cref="SmoothStream.Create"/>. Maps to the options of <c>smoothStream</c>.</summary>
public sealed class SmoothStreamOptions
{
    /// <summary>The delay in milliseconds between each chunk. Defaults to 10ms. Set to <c>null</c> to skip the delay. The delay is skipped while the document is hidden, which never happens in .NET.</summary>
    public int? DelayInMs { get; set; } = 10;

    /// <summary>Controls how the text is chunked for streaming. Defaults to <see cref="SmoothStreamChunking.Word"/>.</summary>
    public SmoothStreamChunking Chunking { get; set; } = SmoothStreamChunking.Word;

    /// <summary>A segmenter that chunks the text. Takes precedence over <see cref="ChunkDetector"/>, <see cref="ChunkingRegex"/> and <see cref="Chunking"/>.</summary>
    public ISegmenter? Segmenter { get; set; }

    /// <summary>A custom pattern that does not match the empty string. Takes precedence over <see cref="Chunking"/>.</summary>
    public Regex? ChunkingRegex { get; set; }

    /// <summary>A custom chunk detector. Takes precedence over <see cref="ChunkingRegex"/> and <see cref="Chunking"/>.</summary>
    public ChunkDetector? ChunkDetector { get; set; }

    /// <summary>Internal. For test use only. Maps to <c>_internal.delay</c>.</summary>
    internal Func<int?, CancellationToken, Task>? Delay { get; set; }

    /// <summary>
    /// Internal. For test use only. Replaces <see cref="SmoothStream"/>'s <c>isDocumentHidden</c>, which upstream reads
    /// from the <c>document</c> global that tests stub. .NET has no document, so the default is never hidden.
    /// </summary>
    internal Func<bool>? IsDocumentHidden { get; set; }
}

/// <summary>Smooths text and reasoning streaming output. Maps to <c>smoothStream</c>.</summary>
public static class SmoothStream
{
    private static readonly Regex s_word = new(
        @"\S+\s+",
        RegexOptions.Multiline,
        TimeSpan.FromSeconds(1)
    );
    private static readonly Regex s_line = new(
        @"\n+",
        RegexOptions.Multiline,
        TimeSpan.FromSeconds(1)
    );

    // Maps to isDocumentHidden. Browsers heavily throttle timers in hidden documents, which would stall the smoothing
    // delay and, through backpressure, the whole stream. .NET has no document, so it is never hidden.
    private static bool IsDocumentHidden() => false;

    /// <summary>
    /// Creates the transform. Call the returned function with the tools to get a transform of the full stream.
    /// </summary>
    public static Func<
        IReadOnlyList<Tool>?,
        Func<IAsyncEnumerable<TextStreamPart>, CancellationToken, IAsyncEnumerable<TextStreamPart>>
    > Create(SmoothStreamOptions? options = null)
    {
        options ??= new SmoothStreamOptions();
        var delayInMs = options.DelayInMs;
        var delay = options.Delay
            ?? ((ms, ct) => ms is int m ? Task.Delay(m, ct) : Task.CompletedTask);
        var isDocumentHidden = options.IsDocumentHidden ?? IsDocumentHidden;
        var detectChunk = CreateDetector(options);

        return _ =>
            (stream, cancellationToken) =>
                Transform(
                    stream,
                    detectChunk,
                    delayInMs,
                    delay,
                    isDocumentHidden,
                    cancellationToken
                );
    }

    private static Func<string, string?> CreateDetector(SmoothStreamOptions options)
    {
        if (options.Segmenter is { } segmenter)
        {
            return buffer =>
            {
                if (buffer.Length == 0)
                {
                    return null;
                }

                using var enumerator = segmenter.Segment(buffer).GetEnumerator();
                var first = enumerator.MoveNext() ? enumerator.Current : null;
                return string.IsNullOrEmpty(first) ? null : first;
            };
        }

        if (options.ChunkDetector is { } chunking)
        {
            return buffer =>
            {
                var match = chunking(buffer);
                if (match is null)
                {
                    return null;
                }

                if (match.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Chunking function must return a non-empty string."
                    );
                }

                if (!buffer.StartsWith(match, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Chunking function must return a match that is a prefix of the buffer. Received: \"{match}\" which is not a prefix of buffer \"{buffer}\""
                    );
                }

                return match;
            };
        }

        var regex = options.ChunkingRegex
            ?? (options.Chunking == SmoothStreamChunking.Line ? s_line : s_word);
        return buffer =>
        {
            var match = regex.Match(buffer);
            if (!match.Success)
            {
                return null;
            }

            if (match.Length == 0)
            {
                throw new InvalidOperationException(
                    "Chunking RegExp must not match an empty string."
                );
            }

            return buffer.Substring(0, match.Index) + match.Value;
        };
    }

    private static async IAsyncEnumerable<TextStreamPart> Transform(
        IAsyncEnumerable<TextStreamPart> stream,
        Func<string, string?> detectChunk,
        int? delayInMs,
        Func<int?, CancellationToken, Task> delay,
        Func<bool> isDocumentHidden,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var buffer = string.Empty;
        var id = string.Empty;
        string? type = null;
        JsonElement? providerMetadata = null;

        TextStreamPart? Flush()
        {
            if (type is not null && (buffer.Length > 0 || providerMetadata is not null))
            {
                var part = CreateDelta(type, id, buffer, providerMetadata);
                buffer = string.Empty;
                providerMetadata = null;
                return part;
            }

            return null;
        }

        await foreach (
            var chunk in stream.WithCancellation(cancellationToken).ConfigureAwait(false)
        )
        {
            if (chunk is PingPart)
            {
                yield return chunk;
                continue;
            }

            string chunkType;
            string chunkId;
            string chunkText;
            JsonElement? chunkMetadata;
            switch (chunk)
            {
                case TextDeltaPart t:
                    (chunkType, chunkId, chunkText, chunkMetadata) = (
                        "text-delta",
                        t.Id,
                        t.Text,
                        t.ProviderMetadata
                    );
                    break;
                case ReasoningDeltaPart r:
                    (chunkType, chunkId, chunkText, chunkMetadata) = (
                        "reasoning-delta",
                        r.Id,
                        r.Text,
                        r.ProviderMetadata
                    );
                    break;
                default:
                    if (Flush() is { } flushed)
                    {
                        yield return flushed;
                    }

                    yield return chunk;
                    continue;
            }

            if (chunkText.Length == 0 && chunkMetadata is not null)
            {
                if (Flush() is { } flushed)
                {
                    yield return flushed;
                }

                yield return chunk;
                continue;
            }

            // Flush at metadata boundaries because one output part cannot preserve
            // metadata from multiple input deltas.
            if (
                buffer.Length > 0
                && (
                    chunkType != type
                    || chunkId != id
                    || providerMetadata is not null
                    || chunkMetadata is not null
                )
            )
            {
                if (Flush() is { } flushed)
                {
                    yield return flushed;
                }
            }

            buffer += chunkText;
            id = chunkId;
            type = chunkType;
            providerMetadata = chunkMetadata;

            string? match;
            while ((match = detectChunk(buffer)) is not null)
            {
                yield return CreateDelta(type, id, match, providerMetadata);
                buffer = buffer.Substring(match.Length);

                await delay(isDocumentHidden() ? null : delayInMs, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (buffer.Length == 0)
            {
                providerMetadata = null;
            }
        }
    }

    private static TextStreamPart CreateDelta(
        string type,
        string id,
        string text,
        JsonElement? providerMetadata
    ) =>
        type == "text-delta"
            ? new TextDeltaPart(id, text, providerMetadata)
            : new ReasoningDeltaPart(id, text, providerMetadata);
}
