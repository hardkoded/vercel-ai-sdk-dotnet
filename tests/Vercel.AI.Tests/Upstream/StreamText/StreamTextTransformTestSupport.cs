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

    // Maps to the MockLanguageModelV4 of the upstream tests: each doStream call gets its own parts, request body and response headers.
    public sealed class CallsModel(
        Func<int, LanguageModelCallOptions, CallScript> script,
        string provider = "mock-provider",
        string modelId = "mock-model-id"
    ) : ILanguageModel
    {
        private int _calls;

        public int CallCount => Volatile.Read(ref _calls);

        public List<LanguageModelCallOptions> Prompts { get; } = [];

        public IReadOnlyDictionary<string, string> SupportedUrls =>
            Vercel.AI.ProviderUtils.UrlSupport.None;

        public string SpecificationVersion => "V4";

        public string Provider => provider;

        public string ModelId => modelId;

        public static CallsModel Single(
            IReadOnlyList<LanguageModelStreamPart> parts,
            string? requestBody = null,
            Dictionary<string, string>? headers = null
        ) => new((_, _) => new CallScript(parts, requestBody, headers));

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            var call = Interlocked.Increment(ref _calls);
            lock (Prompts)
            {
                Prompts.Add(options);
            }

            var scripted = script(call - 1, options);
            return Task.FromResult(
                new LanguageModelStreamResult(
                    SteeringTestModel.ToAsync(scripted.Parts, cancellationToken),
                    scripted.RequestBody == null
                        ? null
                        : new LanguageModelStreamRequest(
                            System.Text.Json.JsonSerializer.SerializeToElement(
                                scripted.RequestBody
                            )
                        ),
                    scripted.Headers == null
                        ? null
                        : new LanguageModelStreamResponse(scripted.Headers)
                )
            );
        }
    }

    public sealed record CallScript(
        IReadOnlyList<LanguageModelStreamPart> Parts,
        string? RequestBody = null,
        Dictionary<string, string>? Headers = null
    );

    public static readonly LanguageModelUsage TestUsage = SteeringTestSupport.Usage;

    public static FinishStreamPart Finish(
        FinishReason reason = FinishReason.Stop,
        string raw = "stop",
        System.Text.Json.JsonElement? providerMetadata = null
    ) => new(reason, TestUsage, raw, providerMetadata);

    public static async Task<List<TextStreamPart>> PartsAsync(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }
}
