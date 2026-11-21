// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Parallel;

/// <summary>Known options accepted under <c>providerOptions.parallel</c>. Maps to <c>parallelProviderOptions</c>.</summary>
public static class ParallelProviderOptions
{
    /// <summary>Decision options, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Decision =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["includeExcerpts"] = new("Include evidence excerpts in citations. Defaults to true."),
        };
}

/// <summary>Parallel model catalog. Maps to <c>parallel-model-catalog.ts</c>.</summary>
public static class ParallelModelCatalog
{
    /// <summary>Known decision model ids. Maps to <c>parallelDecisionModelIds</c>.</summary>
    public static IReadOnlyList<string> DecisionModelIds { get; } = [ParallelDecisionModelId.Fast];

    /// <summary>Question types every Parallel decision model supports. Maps to <c>parallelSupportedQuestionTypes</c>.</summary>
    public static IReadOnlyList<string> SupportedQuestionTypes { get; } =
    ["boolean", "choice", "score"];

    /// <summary>Entries of every known model id. Maps to <c>parallelModelCatalog</c>.</summary>
    public static IReadOnlyDictionary<string, ModelEntry<object>> Entries { get; } =
        DecisionModelIds.ToDictionary(
            id => id,
            GetParallelDecisionModelEntry,
            StringComparer.Ordinal
        );

    /// <summary>Returns the entry of a decision model. An unknown model id has unknown option support. Maps to <c>getParallelDecisionModelEntry</c>.</summary>
    public static ModelEntry<object> GetParallelDecisionModelEntry(string modelId)
    {
        var known = DecisionModelIds.Contains(modelId);
        return new ModelEntry<object>
        {
            Capabilities = new ModelCapabilities
            {
                Decision = new DecisionCapabilities
                {
                    QuestionTypes = new DecisionQuestionTypeCapabilities
                    {
                        Boolean = ModelCapability.Supported,
                        Choice = ModelCapability.Supported,
                        Score = ModelCapability.Supported,
                        Box = ModelCapability.Unsupported,
                    },
                    Inputs = new DecisionInputCapabilities { Image = ModelCapability.Unsupported },
                    Reasoning = ModelCapability.Unsupported,
                    MaxOutputTokens = ModelCapability.Unsupported,
                },
            },
            ProviderOptions = new ModelProviderOptions
            {
                Decision = new Dictionary<string, ModelCapability>
                {
                    ["includeExcerpts"] = known
                        ? ModelCapability.Supported
                        : ModelCapability.Unknown,
                },
            },
        };
    }
}
