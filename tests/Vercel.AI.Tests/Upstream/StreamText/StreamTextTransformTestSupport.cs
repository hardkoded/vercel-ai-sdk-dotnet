// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.StreamText;

internal static class StreamTextTransformTestSupport
{
    // Maps to createTestModel(): "Hello", ", ", "world!" text deltas.
    public static SteeringTestModel CreateTestModel() =>
        SteeringTestModel.Stream(() =>
            new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello"),
                new TextDeltaStreamPart("1", ", "),
                new TextDeltaStreamPart("1", "world!"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, SteeringTestSupport.Usage, "stop"),
            }
        );

    // Maps to a TransformStream whose transform maps each chunk to zero or more chunks.
    public static Func<
        IAsyncEnumerable<TextStreamPart>,
        CancellationToken,
        IAsyncEnumerable<TextStreamPart>
    > Map(Func<TextStreamPart, IEnumerable<TextStreamPart>> transform) =>
        (stream, cancellationToken) => Run(stream, transform, cancellationToken);

    private static async IAsyncEnumerable<TextStreamPart> Run(
        IAsyncEnumerable<TextStreamPart> stream,
        Func<TextStreamPart, IEnumerable<TextStreamPart>> transform,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var part in stream.WithCancellation(cancellationToken))
        {
            foreach (var mapped in transform(part))
            {
                yield return mapped;
            }
        }
    }

    public static async Task<List<string>> TextStreamAsync(StreamTextResult result)
    {
        var chunks = new List<string>();
        await foreach (var chunk in result.TextStream())
        {
            chunks.Add(chunk);
        }

        return chunks;
    }
}
