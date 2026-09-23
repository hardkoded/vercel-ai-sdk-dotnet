// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Sdk.Provider;

namespace Vercel.AI.Sdk.Testing;

/// <summary>Language model that returns scripted generate and stream results.</summary>
public sealed class TestLanguageModel : ILanguageModel
{
    /// <summary>Creates a model with id <paramref name="modelId"/>.</summary>
    public TestLanguageModel(string modelId = "test")
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => "test";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Calls observed by generate and stream.</summary>
    public List<LanguageModelCallOptions> Calls { get; } = new();

    /// <summary>When set, generate returns this function's result.</summary>
    public Func<LanguageModelCallOptions, LanguageModelGenerateResult>? OnGenerate { get; set; }

    /// <summary>When set, stream yields these parts instead of a single text delta.</summary>
    public IReadOnlyList<LanguageModelStreamPart>? StreamParts { get; set; }

    /// <summary>A generate result whose only content is <paramref name="text"/>.</summary>
    public static LanguageModelGenerateResult Text(string text)
    {
        return new LanguageModelGenerateResult(
            new GeneratedContent[] { new GeneratedText(text) },
            FinishReason.Stop,
            new LanguageModelUsage(1, 1, 2),
            "stop");
    }

    /// <inheritdoc />
    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        Calls.Add(options);
        cancellationToken.ThrowIfCancellationRequested();
        var result = OnGenerate is null ? Text("ok") : OnGenerate(options);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Calls.Add(options);
        var parts = StreamParts ?? new LanguageModelStreamPart[]
        {
            new TextDeltaStreamPart("text", "ok"),
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop"),
        };
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return part;
            await Task.Yield();
        }
    }
}
