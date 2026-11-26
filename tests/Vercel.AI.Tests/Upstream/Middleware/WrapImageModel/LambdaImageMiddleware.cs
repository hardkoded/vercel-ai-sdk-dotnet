// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapImageModel;

/// <summary>An image middleware object literal: each member is a delegate, like the upstream tests build them.</summary>
internal sealed class LambdaImageMiddleware : ImageModelMiddleware
{
    public Func<ImageModelCall, IImageCaller, ImageModelCall>? OnTransformParams { get; init; }

    public Func<ImageModelWrapContext, Task<ImageModelResult>>? OnWrapGenerate { get; init; }

    public Func<IImageCaller, string?>? OnOverrideProvider { get; init; }

    public Func<IImageCaller, string?>? OnOverrideModelId { get; init; }

    public Func<IImageCaller, int?>? OnOverrideMaxImagesPerCall { get; init; }

    public override Task<ImageModelCall> TransformParamsAsync(
        ImageModelCall call,
        IImageCaller model,
        CancellationToken cancellationToken
    ) =>
        OnTransformParams is null
            ? base.TransformParamsAsync(call, model, cancellationToken)
            : Task.FromResult(OnTransformParams(call, model));

    public override Task<ImageModelResult> WrapGenerateAsync(
        ImageModelWrapContext context,
        CancellationToken cancellationToken
    ) =>
        OnWrapGenerate is null
            ? base.WrapGenerateAsync(context, cancellationToken)
            : OnWrapGenerate(context);

    public override string? OverrideProvider(IImageCaller model) =>
        OnOverrideProvider?.Invoke(model);

    public override string? OverrideModelId(IImageCaller model) => OnOverrideModelId?.Invoke(model);

    public override int? OverrideMaxImagesPerCall(IImageCaller model) =>
        OnOverrideMaxImagesPerCall?.Invoke(model);
}
