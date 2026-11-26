// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapEmbeddingModel;

/// <summary>An embedding middleware object literal: each member is a delegate, like the upstream tests build them.</summary>
internal sealed class LambdaEmbeddingMiddleware : EmbeddingModelMiddleware
{
    public Func<
        EmbeddingModelCall,
        IEmbeddingCaller,
        EmbeddingModelCall
    >? OnTransformParams { get; init; }

    public Func<EmbeddingModelWrapContext, Task<EmbeddingModelResponse>>? OnWrapEmbed { get; init; }

    public Func<IEmbeddingCaller, string?>? OnOverrideProvider { get; init; }

    public Func<IEmbeddingCaller, string?>? OnOverrideModelId { get; init; }

    public Func<IEmbeddingCaller, int?>? OnOverrideMaxEmbeddingsPerCall { get; init; }

    public Func<IEmbeddingCaller, bool?>? OnOverrideSupportsParallelCalls { get; init; }

    public override Task<EmbeddingModelCall> TransformParamsAsync(
        EmbeddingModelCall call,
        IEmbeddingCaller model,
        CancellationToken cancellationToken
    ) =>
        OnTransformParams is null
            ? base.TransformParamsAsync(call, model, cancellationToken)
            : Task.FromResult(OnTransformParams(call, model));

    public override Task<EmbeddingModelResponse> WrapEmbedAsync(
        EmbeddingModelWrapContext context,
        CancellationToken cancellationToken
    ) =>
        OnWrapEmbed is null
            ? base.WrapEmbedAsync(context, cancellationToken)
            : OnWrapEmbed(context);

    public override string? OverrideProvider(IEmbeddingCaller model) =>
        OnOverrideProvider?.Invoke(model);

    public override string? OverrideModelId(IEmbeddingCaller model) =>
        OnOverrideModelId?.Invoke(model);

    public override int? OverrideMaxEmbeddingsPerCall(IEmbeddingCaller model) =>
        OnOverrideMaxEmbeddingsPerCall?.Invoke(model);

    public override bool? OverrideSupportsParallelCalls(IEmbeddingCaller model) =>
        OnOverrideSupportsParallelCalls?.Invoke(model);
}
