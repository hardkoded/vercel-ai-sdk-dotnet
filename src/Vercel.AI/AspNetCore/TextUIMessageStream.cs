// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace Vercel.AI.AspNetCore;

/// <summary>Turns plain text into the UI message chunks a text-only client expects.</summary>
public static class TextUIMessageStream
{
    /// <summary>
    /// Wraps <paramref name="text"/> in start, one text block with id <c>text-1</c>, and finish chunks.
    /// An empty source still emits the surrounding chunks and no text delta.
    /// </summary>
    /// <param name="text">Text pieces, in order.</param>
    /// <param name="cancellationToken">Cancels reading <paramref name="text"/>.</param>
    public static async IAsyncEnumerable<JsonObject> TransformAsync(
        IAsyncEnumerable<string> text,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        yield return Chunk("start");
        yield return Chunk("start-step");
        yield return new JsonObject { ["type"] = "text-start", ["id"] = "text-1" };
        await foreach (var part in text.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return new JsonObject
            {
                ["type"] = "text-delta",
                ["id"] = "text-1",
                ["delta"] = part ?? string.Empty,
            };
        }

        yield return new JsonObject { ["type"] = "text-end", ["id"] = "text-1" };
        yield return Chunk("finish-step");
        yield return Chunk("finish");
    }

    /// <summary>Decodes UTF-8 chunks and passes each decoded piece to <paramref name="onTextPart"/>.</summary>
    /// <param name="chunks">Encoded text pieces. An empty sequence does not call <paramref name="onTextPart"/>.</param>
    /// <param name="onTextPart">Receives one decoded piece.</param>
    /// <param name="cancellationToken">Cancels reading <paramref name="chunks"/>.</param>
    public static async Task ProcessAsync(
        IAsyncEnumerable<byte[]> chunks,
        Func<string, Task> onTextPart,
        CancellationToken cancellationToken = default)
    {
        if (chunks is null)
        {
            throw new ArgumentNullException(nameof(chunks));
        }

        if (onTextPart is null)
        {
            throw new ArgumentNullException(nameof(onTextPart));
        }

        await foreach (var chunk in chunks.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (chunk == null || chunk.Length == 0)
            {
                continue;
            }

            var text = Encoding.UTF8.GetString(chunk);
            if (text.Length > 0)
            {
                await onTextPart(text).ConfigureAwait(false);
            }
        }
    }

    private static JsonObject Chunk(string type)
    {
        return new JsonObject { ["type"] = type };
    }
}
