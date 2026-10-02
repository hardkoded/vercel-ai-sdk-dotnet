// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace Vercel.AI.AspNetCore;

/// <summary>Turns plain text into UI message chunks.</summary>
public static class TextUiStream
{
    /// <summary>Decodes <paramref name="stream"/> as UTF-8 and passes each decoded chunk to <paramref name="onTextPart"/>.</summary>
    public static async Task ProcessAsync(IAsyncEnumerable<byte[]> stream, Func<string, Task> onTextPart, CancellationToken cancellationToken = default)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (onTextPart is null)
        {
            throw new ArgumentNullException(nameof(onTextPart));
        }

        var decoder = Encoding.UTF8.GetDecoder();
        await foreach (var chunk in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (chunk == null || chunk.Length == 0)
            {
                continue;
            }

            var count = decoder.GetCharCount(chunk, 0, chunk.Length, flush: false);
            if (count == 0)
            {
                continue;
            }

            var chars = new char[count];
            decoder.GetChars(chunk, 0, chunk.Length, chars, 0, flush: false);
            await onTextPart(new string(chars)).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Wraps each string from <paramref name="stream"/> as a <c>text-delta</c> between start, step, and finish chunks.
    /// The text part id is <c>text-1</c>.
    /// </summary>
    public static async IAsyncEnumerable<JsonObject> Transform(IAsyncEnumerable<string> stream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        yield return new JsonObject { ["type"] = "start" };
        yield return new JsonObject { ["type"] = "start-step" };
        yield return new JsonObject { ["type"] = "text-start", ["id"] = "text-1" };
        await foreach (var part in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return new JsonObject
            {
                ["type"] = "text-delta",
                ["id"] = "text-1",
                ["delta"] = part,
            };
        }

        yield return new JsonObject { ["type"] = "text-end", ["id"] = "text-1" };
        yield return new JsonObject { ["type"] = "finish-step" };
        yield return new JsonObject { ["type"] = "finish" };
    }
}
