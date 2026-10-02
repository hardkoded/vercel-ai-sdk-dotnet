// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

internal sealed class ScriptedLanguageModel : ILanguageModel
{
    public ScriptedLanguageModel(string modelId = "test")
    {
        ModelId = modelId;
    }

    public string SpecificationVersion => "V4";

    public string Provider => "test";

    public string ModelId { get; }

    public List<LanguageModelCallOptions> Calls { get; } = new();

    public List<CancellationToken> Tokens { get; } = new();

    public Func<LanguageModelCallOptions, LanguageModelGenerateResult>? OnGenerate { get; set; }

    public Func<int, IReadOnlyList<LanguageModelStreamPart>>? OnStream { get; set; }

    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        Calls.Add(options);
        Tokens.Add(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var result = OnGenerate is null ? TestLanguageModel.Text("ok") : OnGenerate(options);
        return Task.FromResult(result);
    }

    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Calls.Add(options);
        Tokens.Add(cancellationToken);
        var parts = OnStream is null
            ? new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("text", "ok"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop"),
            }
            : OnStream(Calls.Count);
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return part;
            await Task.Yield();
        }
    }
}
