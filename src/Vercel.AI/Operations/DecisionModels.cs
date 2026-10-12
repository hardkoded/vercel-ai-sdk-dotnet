// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>A provider or registry that can return decision models. Maps to a provider with a <c>decisionModel</c> factory.</summary>
public interface IDecisionModelProvider
{
    /// <summary>Returns the decision model for <paramref name="modelId"/>, or null when there is none.</summary>
    IDecisionCaller? DecisionModel(string modelId);
}

/// <summary>Resolves decision models, including string model ids. Maps to <c>resolveDecisionModel</c>.</summary>
public static class DecisionModels
{
    /// <summary>Message thrown when the default provider has no decision models.</summary>
    public const string MissingProviderMessage = "The default provider does not support decision models.";

    private static readonly Lazy<GatewayProvider> SharedGateway = new(() => GatewayProvider.Create());

    /// <summary>
    /// The global default provider (<c>AI_SDK_DEFAULT_PROVIDER</c>). A string model id is resolved through it. It must
    /// implement <see cref="IDecisionModelProvider"/>. When null, string ids go through Gateway.
    /// </summary>
    public static object? DefaultProvider { get; set; }

    /// <summary>Creates the provider used when <see cref="DefaultProvider"/> is null. Null uses a shared <see cref="GatewayProvider"/>.</summary>
    public static Func<IDecisionModelProvider>? GatewayFactory { get; set; }

    /// <summary>Resolves a decision model instance or a string model id.</summary>
    public static IDecisionCaller Resolve(object? model)
    {
        if (model is IDecisionCaller caller)
        {
            return EnsureVersion(caller);
        }

        if (model is string id)
        {
            var provider = DefaultProvider ?? (GatewayFactory != null ? GatewayFactory() : SharedGateway.Value);
            if (provider is not IDecisionModelProvider decisionProvider)
            {
                throw new InvalidOperationException(MissingProviderMessage);
            }

            var resolved = decisionProvider.DecisionModel(id) ?? throw new NoSuchModelError(id, "decisionModel");
            return EnsureVersion(resolved);
        }

        throw new InvalidArgumentException("model", model, "model is required");
    }

    internal static IDecisionCaller EnsureVersion(IDecisionCaller model)
    {
        if (model.SpecificationVersion != "v4")
        {
            throw new UnsupportedModelVersionException(model.SpecificationVersion, model.Provider, model.ModelId);
        }

        return model;
    }
}
