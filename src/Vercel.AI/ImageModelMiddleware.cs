// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using ProviderGeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI;

/// <summary>What <c>wrapGenerate</c> receives for an image model: the call after <c>transformParams</c>, the model, and the way to continue.</summary>
public sealed class ImageModelWrapContext
{
    private readonly Func<CancellationToken, Task<ImageModelResult>> _doGenerate;

    /// <summary>Creates a context.</summary>
    public ImageModelWrapContext(
        ImageModelCall call,
        IImageCaller model,
        Func<CancellationToken, Task<ImageModelResult>> doGenerate
    )
    {
        Call = call ?? throw new ArgumentNullException(nameof(call));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _doGenerate = doGenerate ?? throw new ArgumentNullException(nameof(doGenerate));
    }

    /// <summary>The call after <c>transformParams</c>. Maps to <c>params</c>.</summary>
    public ImageModelCall Call { get; }

    /// <summary>The wrapped model.</summary>
    public IImageCaller Model { get; }

    /// <summary>Calls <c>doGenerate</c> of the wrapped model with <see cref="Call"/>.</summary>
    public Task<ImageModelResult> DoGenerateAsync(CancellationToken cancellationToken) =>
        _doGenerate(cancellationToken);
}

/// <summary>Image-model middleware. Maps to <c>ImageModelMiddleware</c>.</summary>
public interface IImageModelMiddleware
{
    /// <summary>Transforms the call before it runs. Maps to <c>transformParams</c>.</summary>
    Task<ImageModelCall> TransformParamsAsync(
        ImageModelCall call,
        IImageCaller model,
        CancellationToken cancellationToken
    );

    /// <summary>Wraps a generate call. Maps to <c>wrapGenerate</c>.</summary>
    Task<ImageModelResult> WrapGenerateAsync(
        ImageModelWrapContext context,
        CancellationToken cancellationToken
    );

    /// <summary>Overrides the provider id. Null keeps the model's. Maps to <c>overrideProvider</c>.</summary>
    string? OverrideProvider(IImageCaller model);

    /// <summary>Overrides the model id. Null keeps the model's. Maps to <c>overrideModelId</c>.</summary>
    string? OverrideModelId(IImageCaller model);

    /// <summary>Overrides the maximum images per call. Null keeps the model's. Maps to <c>overrideMaxImagesPerCall</c>.</summary>
    int? OverrideMaxImagesPerCall(IImageCaller model);
}

/// <summary>Passes every operation through. Override what the middleware changes.</summary>
public abstract class ImageModelMiddleware : IImageModelMiddleware
{
    /// <inheritdoc />
    public virtual Task<ImageModelCall> TransformParamsAsync(
        ImageModelCall call,
        IImageCaller model,
        CancellationToken cancellationToken
    ) => Task.FromResult(call);

    /// <inheritdoc />
    public virtual Task<ImageModelResult> WrapGenerateAsync(
        ImageModelWrapContext context,
        CancellationToken cancellationToken
    ) => context.DoGenerateAsync(cancellationToken);

    /// <inheritdoc />
    public virtual string? OverrideProvider(IImageCaller model) => null;

    /// <inheritdoc />
    public virtual string? OverrideModelId(IImageCaller model) => null;

    /// <inheritdoc />
    public virtual int? OverrideMaxImagesPerCall(IImageCaller model) => null;
}

/// <summary>Wraps an image model with middleware. Maps to <c>wrapImageModel</c>.</summary>
public static class ImageModelMiddlewareExtensions
{
    /// <summary>
    /// Wraps <paramref name="model"/>. The first middleware transforms the input first, and the last is wrapped directly around the model.
    /// <paramref name="modelId"/> and <paramref name="providerId"/> override the ids of every wrapper.
    /// </summary>
    public static IImageCaller WrapImageModel(
        this IImageCaller model,
        IEnumerable<IImageModelMiddleware> middleware,
        string? modelId = null,
        string? providerId = null
    )
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        var current = model;
        foreach (var item in (middleware ?? throw new ArgumentNullException(nameof(middleware))).Reverse())
        {
            current = new MiddlewareImageModel(current, item, modelId, providerId);
        }

        return current;
    }

    /// <summary>Wraps <paramref name="model"/> with one middleware.</summary>
    public static IImageCaller WrapImageModel(
        this IImageCaller model,
        IImageModelMiddleware middleware,
        string? modelId = null,
        string? providerId = null
    ) => model.WrapImageModel(new[] { middleware }, modelId, providerId);
}

internal sealed class MiddlewareImageModel(
    IImageCaller inner,
    IImageModelMiddleware middleware,
    string? modelId,
    string? providerId
) : IImageCaller, IImageModel
{
    public string Provider => providerId ?? middleware.OverrideProvider(inner) ?? inner.Provider;

    public string ModelId => modelId ?? middleware.OverrideModelId(inner) ?? inner.ModelId;

    public int? MaxImagesPerCall =>
        middleware.OverrideMaxImagesPerCall(inner) ?? inner.MaxImagesPerCall;

    public async Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken) =>
        middleware.OverrideMaxImagesPerCall(inner)
        ?? await inner.ResolveMaxImagesPerCallAsync(cancellationToken).ConfigureAwait(false);

    public async Task<ImageModelResult> DoGenerateAsync(
        ImageModelCall call,
        CancellationToken cancellationToken
    )
    {
        var transformed = await middleware
            .TransformParamsAsync(call, inner, cancellationToken)
            .ConfigureAwait(false);
        return await middleware
            .WrapGenerateAsync(
                new ImageModelWrapContext(
                    transformed,
                    inner,
                    token => inner.DoGenerateAsync(transformed, token)
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public async Task<ImageGenerationResult> DoGenerateAsync(
        ImageCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await DoGenerateAsync(options.ToModelCall(cancellationToken), cancellationToken)
            .ConfigureAwait(false);
        var images = new List<ProviderGeneratedImage>();
        foreach (var image in result.Images)
        {
            var data = image is string base64 ? Convert.FromBase64String(base64) : (byte[])image!;
            images.Add(
                new ProviderGeneratedImage(
                    Vercel.AI.ProviderUtils.MediaTypes.DetectMediaType(data, "image") ?? "image/png",
                    data,
                    null
                )
            );
        }

        return new ImageGenerationResult(
            images,
            result.Warnings.Select(warning => warning.ToCallWarning()).ToList()
        );
    }
}
