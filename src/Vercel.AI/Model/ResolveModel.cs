// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>
/// Maps to <c>resolve-model.ts</c>. A model instance is checked for its specification version. A model id goes through the global
/// default provider (<see cref="DecisionModels.DefaultProvider"/>), or Gateway when none is set. The port implements specification V4 only,
/// so there are no <c>asXModelV4</c> adapters.
/// </summary>
internal static class ResolveModel
{
    private static readonly Lazy<GatewayProvider> SharedGateway = new(
        () => GatewayProvider.Create(),
        LazyThreadSafetyMode.PublicationOnly
    );

    /// <summary>Maps to <c>resolveLanguageModel</c>.</summary>
    internal static ILanguageModel ResolveLanguageModel(ILanguageModel? model, string? modelId)
    {
        if (model is null)
        {
            return GetGlobalProvider().LanguageModel(RequireId(modelId));
        }

        CheckVersion(model.SpecificationVersion, model.Provider, model.ModelId);
        return model;
    }

    /// <summary>Maps to <c>resolveEmbeddingModel</c>.</summary>
    internal static IEmbeddingModel ResolveEmbeddingModel(IEmbeddingModel? model, string? modelId)
    {
        if (model is null)
        {
            return GetGlobalProvider().EmbeddingModel(RequireId(modelId));
        }

        CheckVersion(model.SpecificationVersion, model.Provider, model.ModelId);
        return model;
    }

    /// <summary>Maps to <c>resolveImageModel</c>.</summary>
    internal static IImageModel ResolveImageModel(IImageModel? model, string? modelId)
    {
        return model ?? GetGlobalProvider().ImageModel(RequireId(modelId));
    }

    /// <summary>Maps to <c>resolveSpeechModel</c>.</summary>
    internal static ISpeechModel ResolveSpeechModel(ISpeechModel? model, string? modelId)
    {
        return model ?? GetGlobalProvider().SpeechModel(RequireId(modelId));
    }

    /// <summary>Maps to <c>resolveTranscriptionModel</c>.</summary>
    internal static ITranscriptionModel ResolveTranscriptionModel(
        ITranscriptionModel? model,
        string? modelId
    )
    {
        return model ?? GetGlobalProvider().TranscriptionModel(RequireId(modelId));
    }

    /// <summary>Maps to <c>resolveVideoModel</c>.</summary>
    internal static IVideoModel ResolveVideoModel(IVideoModel? model, string? modelId)
    {
        if (model is null)
        {
            return GetGlobalProvider().VideoModel(RequireId(modelId));
        }

        CheckVersion(model.SpecificationVersion, model.Provider, model.ModelId);
        return model;
    }

    /// <summary>Maps to <c>resolveRerankingModel</c>.</summary>
    internal static IRerankingModel ResolveRerankingModel(IRerankingModel? model, string? modelId)
    {
        return model ?? GetGlobalProvider().RerankingModel(RequireId(modelId));
    }

    private static void CheckVersion(string version, string provider, string modelId)
    {
        if (!string.Equals(version, "v4", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedModelVersionException(version, provider, modelId);
        }
    }

    private static string RequireId(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new AiSdkException("Model or ModelId is required.");
        }

        return modelId!;
    }

    private static ProviderBase GetGlobalProvider()
    {
        return DecisionModels.DefaultProvider
            ?? (DecisionModels.GatewayFactory is { } factory ? factory() : SharedGateway.Value);
    }
}
