// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI;

/// <summary>What <c>wrapEmbed</c> receives: the call after <c>transformParams</c>, the model, and the way to continue.</summary>
public sealed class EmbeddingModelWrapContext
{
    private readonly Func<CancellationToken, Task<EmbeddingModelResponse>> _doEmbed;

    /// <summary>Creates a context.</summary>
    public EmbeddingModelWrapContext(
        EmbeddingModelCall call,
        IEmbeddingCaller model,
        Func<CancellationToken, Task<EmbeddingModelResponse>> doEmbed
    )
    {
        Call = call ?? throw new ArgumentNullException(nameof(call));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _doEmbed = doEmbed ?? throw new ArgumentNullException(nameof(doEmbed));
    }

    /// <summary>The call after <c>transformParams</c>. Maps to <c>params</c>.</summary>
    public EmbeddingModelCall Call { get; }

    /// <summary>The wrapped model.</summary>
    public IEmbeddingCaller Model { get; }

    /// <summary>Calls <c>doEmbed</c> of the wrapped model with <see cref="Call"/>.</summary>
    public Task<EmbeddingModelResponse> DoEmbedAsync(CancellationToken cancellationToken) =>
        _doEmbed(cancellationToken);
}

/// <summary>Embedding-model middleware. Maps to <c>EmbeddingModelMiddleware</c>.</summary>
public interface IEmbeddingModelMiddleware
{
    /// <summary>Transforms the call before it runs. Maps to <c>transformParams</c>.</summary>
    Task<EmbeddingModelCall> TransformParamsAsync(
        EmbeddingModelCall call,
        IEmbeddingCaller model,
        CancellationToken cancellationToken
    );

    /// <summary>Wraps an embed call. Maps to <c>wrapEmbed</c>.</summary>
    Task<EmbeddingModelResponse> WrapEmbedAsync(
        EmbeddingModelWrapContext context,
        CancellationToken cancellationToken
    );

    /// <summary>Overrides the provider id. Null keeps the model's. Maps to <c>overrideProvider</c>.</summary>
    string? OverrideProvider(IEmbeddingCaller model);

    /// <summary>Overrides the model id. Null keeps the model's. Maps to <c>overrideModelId</c>.</summary>
    string? OverrideModelId(IEmbeddingCaller model);

    /// <summary>Overrides the maximum embeddings per call. Null keeps the model's. Maps to <c>overrideMaxEmbeddingsPerCall</c>.</summary>
    int? OverrideMaxEmbeddingsPerCall(IEmbeddingCaller model);

    /// <summary>Overrides whether calls may run in parallel. Null keeps the model's. Maps to <c>overrideSupportsParallelCalls</c>.</summary>
    bool? OverrideSupportsParallelCalls(IEmbeddingCaller model);
}

/// <summary>Passes every operation through. Override what the middleware changes.</summary>
public abstract class EmbeddingModelMiddleware : IEmbeddingModelMiddleware
{
    /// <inheritdoc />
    public virtual Task<EmbeddingModelCall> TransformParamsAsync(
        EmbeddingModelCall call,
        IEmbeddingCaller model,
        CancellationToken cancellationToken
    ) => Task.FromResult(call);

    /// <inheritdoc />
    public virtual Task<EmbeddingModelResponse> WrapEmbedAsync(
        EmbeddingModelWrapContext context,
        CancellationToken cancellationToken
    ) => context.DoEmbedAsync(cancellationToken);

    /// <inheritdoc />
    public virtual string? OverrideProvider(IEmbeddingCaller model) => null;

    /// <inheritdoc />
    public virtual string? OverrideModelId(IEmbeddingCaller model) => null;

    /// <inheritdoc />
    public virtual int? OverrideMaxEmbeddingsPerCall(IEmbeddingCaller model) => null;

    /// <inheritdoc />
    public virtual bool? OverrideSupportsParallelCalls(IEmbeddingCaller model) => null;
}

/// <summary>Applies default settings for an embedding model. Maps to <c>defaultEmbeddingSettingsMiddleware</c>.</summary>
public sealed class DefaultEmbeddingSettingsMiddleware : EmbeddingModelMiddleware
{
    /// <summary>Creates middleware. <paramref name="headers"/> and <paramref name="providerOptions"/> are defaults the call overrides.</summary>
    public DefaultEmbeddingSettingsMiddleware(
        IReadOnlyDictionary<string, string>? headers = null,
        JsonElement? providerOptions = null
    )
    {
        Headers = headers;
        ProviderOptions = providerOptions;
    }

    /// <summary>Default headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Default provider options.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <inheritdoc />
    public override Task<EmbeddingModelCall> TransformParamsAsync(
        EmbeddingModelCall call,
        IEmbeddingCaller model,
        CancellationToken cancellationToken
    )
    {
        var headers = DefaultSettingsMiddleware.MergeMaps(Headers, call.Headers)!;
        JsonElement? providerOptions = ProviderOptions is { } defaults && call.ProviderOptions is { } given
            ? DefaultSettingsMiddleware.MergeObjects(defaults, given)
            : call.ProviderOptions ?? ProviderOptions;
        return Task.FromResult(
            new EmbeddingModelCall(
                call.Values,
                headers,
                providerOptions,
                call.CancellationToken,
                call.Dimensions
            )
        );
    }
}

/// <summary>Wraps an embedding model with middleware. Maps to <c>wrapEmbeddingModel</c>.</summary>
public static class EmbeddingModelMiddlewareExtensions
{
    /// <summary>
    /// Wraps <paramref name="model"/>. The first middleware transforms the input first, and the last is wrapped directly around the model.
    /// <paramref name="modelId"/> and <paramref name="providerId"/> override the ids of every wrapper.
    /// </summary>
    public static IEmbeddingCaller WrapEmbeddingModel(
        this IEmbeddingCaller model,
        IEnumerable<IEmbeddingModelMiddleware> middleware,
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
            current = new MiddlewareEmbeddingModel(current, item, modelId, providerId);
        }

        return current;
    }

    /// <summary>Wraps <paramref name="model"/> with one middleware.</summary>
    public static IEmbeddingCaller WrapEmbeddingModel(
        this IEmbeddingCaller model,
        IEmbeddingModelMiddleware middleware,
        string? modelId = null,
        string? providerId = null
    ) => model.WrapEmbeddingModel(new[] { middleware }, modelId, providerId);
}

internal sealed class MiddlewareEmbeddingModel(
    IEmbeddingCaller inner,
    IEmbeddingModelMiddleware middleware,
    string? modelId,
    string? providerId
) : IEmbeddingCaller
{
    public string SpecificationVersion => inner.SpecificationVersion;

    public string Provider => providerId ?? middleware.OverrideProvider(inner) ?? inner.Provider;

    public string ModelId => modelId ?? middleware.OverrideModelId(inner) ?? inner.ModelId;

    public int? MaxEmbeddingsPerCall =>
        middleware.OverrideMaxEmbeddingsPerCall(inner) ?? inner.MaxEmbeddingsPerCall;

    public double? MaxInputBytesPerCall => inner.MaxInputBytesPerCall;

    public bool SupportsParallelCalls =>
        middleware.OverrideSupportsParallelCalls(inner) ?? inner.SupportsParallelCalls;

    public Task<JsonElement?> TransformProviderOptionsAsync(
        JsonElement? providerOptions,
        IReadOnlyList<string> values,
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken
    ) =>
        inner.TransformProviderOptionsAsync(
            providerOptions,
            values,
            startIndex,
            endIndex,
            cancellationToken
        );

    public async Task<EmbeddingModelResponse> DoEmbedAsync(
        EmbeddingModelCall call,
        CancellationToken cancellationToken
    )
    {
        var transformed = await middleware
            .TransformParamsAsync(call, inner, cancellationToken)
            .ConfigureAwait(false);
        return await middleware
            .WrapEmbedAsync(
                new EmbeddingModelWrapContext(
                    transformed,
                    inner,
                    token => inner.DoEmbedAsync(transformed, token)
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
