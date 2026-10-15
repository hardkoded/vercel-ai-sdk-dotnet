// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>Resolves decision models, including string model ids. Maps to <c>resolveDecisionModel</c>.</summary>
public static class DecisionModels
{
    /// <summary>Message thrown when the default provider has no decision models.</summary>
    public const string MissingProviderMessage = "The default provider does not support decision models.";

    private static readonly Lazy<GatewayProvider> SharedGateway = new(() => GatewayProvider.Create());

    /// <summary>
    /// The global default provider (<c>AI_SDK_DEFAULT_PROVIDER</c>). A string model id is resolved through it. It must
    /// implement <see cref="IDecisionProvider"/> or the deprecated <see cref="IEvaluationProvider"/>. When null, string ids go through Gateway.
    /// </summary>
    public static ProviderBase? DefaultProvider { get; set; }

    /// <summary>Creates the provider used when <see cref="DefaultProvider"/> is null. Null uses a shared <see cref="GatewayProvider"/>.</summary>
    public static Func<GatewayProvider>? GatewayFactory { get; set; }

    /// <summary>
    /// Resolves a decision model. <paramref name="model"/> wins when set. Otherwise <paramref name="modelId"/> goes through
    /// <see cref="DefaultProvider"/>, or Gateway when none is set.
    /// </summary>
    public static IDecisionCaller Resolve(IDecisionCaller? model, string? modelId)
    {
        if (model != null)
        {
            return Validate(model);
        }

        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new AiSdkException("Model or ModelId is required.");
        }

        var provider = DefaultProvider ?? (GatewayFactory != null ? GatewayFactory() : SharedGateway.Value);
#pragma warning disable CS0618 // IEvaluationProvider is the deprecated factory that string ids still resolve through.
        if (provider is not (IDecisionProvider or IEvaluationProvider))
#pragma warning restore CS0618
        {
            throw new InvalidOperationException(MissingProviderMessage);
        }

        return Validate(FromProvider(provider, modelId!) ?? throw new NoSuchModelError(modelId!, "decisionModel"));
    }

#pragma warning disable CS0618
    // Asks the decision factory, then the deprecated evaluation factory. Null when the provider has neither or returns no model.
    internal static IDecisionCaller? FromProvider(ProviderBase provider, string modelId)
    {
        if (provider is IDecisionProvider decision)
        {
            return decision.DecisionModel(modelId);
        }

        return provider is IEvaluationProvider evaluation && evaluation.EvaluationModel(modelId) is { } legacy ? DecisionCaller.FromLegacy(legacy) : null;
    }
#pragma warning restore CS0618

#pragma warning disable CS0618
    // Maps to asEvaluationModel: a model that already has DoEvaluateAsync is returned unchanged.
    internal static IEvaluationDecisionCaller AsEvaluationModel(IDecisionCaller model)
    {
        return model as IEvaluationDecisionCaller ?? new EvaluationDecisionCaller(model);
    }
#pragma warning restore CS0618

    // Maps to resolveDecisionModel for a model instance.
    internal static IDecisionCaller Validate(IDecisionCaller model)
    {
        return model.SpecificationVersion == "v4" ? model : throw new UnsupportedModelVersionException(model.SpecificationVersion, model.Provider, model.ModelId);
    }
}
