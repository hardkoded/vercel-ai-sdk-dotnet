// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Middleware;

/// <summary>Maps to <c>MockEmbeddingModelV4</c>.</summary>
internal sealed class MockEmbeddingModel : IEmbeddingCaller
{
    public string Provider { get; init; } = "mock-provider";

    public string ModelId { get; init; } = "mock-model-id";

    public string SpecificationVersion => "v4";

    public int? MaxEmbeddingsPerCall { get; init; } = 1;

    public double? MaxInputBytesPerCall { get; init; }

    public bool SupportsParallelCalls { get; init; } = true;

    public Func<EmbeddingModelCall, Task<EmbeddingModelResponse>>? OnDoEmbed { get; init; }

    public Func<
        JsonElement?,
        IReadOnlyList<string>,
        int,
        int,
        JsonElement?
    >? OnTransformProviderOptions { get; init; }

    public List<EmbeddingModelCall> DoEmbedCalls { get; } = [];

    public Task<JsonElement?> TransformProviderOptionsAsync(
        JsonElement? providerOptions,
        IReadOnlyList<string> values,
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            OnTransformProviderOptions?.Invoke(providerOptions, values, startIndex, endIndex)
        );

    public Task<EmbeddingModelResponse> DoEmbedAsync(
        EmbeddingModelCall call,
        CancellationToken cancellationToken
    )
    {
        DoEmbedCalls.Add(call);
        return OnDoEmbed is null
            ? Task.FromResult(new EmbeddingModelResponse([]))
            : OnDoEmbed(call);
    }
}
