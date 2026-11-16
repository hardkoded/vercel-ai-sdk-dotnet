// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Cerebras;

/// <summary>Known options accepted under <c>providerOptions.cerebras</c>. Upstream <c>cerebrasProviderOptions</c>.</summary>
public static class CerebrasProviderOptions
{
    /// <summary>Language options (upstream <c>cerebrasProviderOptions.language</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Language =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["user"] = new("End-user identifier for abuse monitoring."),
            ["strictJsonSchema"] = new("Use strict JSON schema validation."),
            ["parallelToolCalls"] = new("Allow parallel function calls."),
            ["logprobs"] = new("Return token log probabilities."),
            ["topLogprobs"] = new("Number of token log probabilities to return."),
            ["logitBias"] = new("Bias token selection by token ID."),
            ["serviceTier"] = new("Request processing tier."),
            ["queueThreshold"] = new("Maximum acceptable flex queue time."),
            ["reasoningEffort"] = new("Reasoning effort for configurable models."),
            ["reasoningFormat"] = new("Format of the returned reasoning."),
            ["prediction"] = new("Predicted output content."),
            ["promptCacheKey"] = new("Cache affinity key for shared prompts."),
        };
}

/// <summary>Structured output support of a Cerebras language model. Upstream <c>LanguageCapabilities.structuredOutput</c>.</summary>
public sealed class CerebrasStructuredOutputCapabilities
{
    /// <summary>Whether JSON schemas are forwarded.</summary>
    public bool JsonSchema { get; init; }
}

/// <summary>
/// The language capabilities of a Cerebras model: the shared <see cref="LanguageCapabilities"/> plus
/// <see cref="StructuredOutput"/>. Upstream <c>LanguageCapabilities</c> of <c>cerebras-model-catalog.ts</c>.
/// </summary>
public sealed class CerebrasLanguageCapabilities
{
    /// <summary>Upstream <c>inputs</c>.</summary>
    public LanguageInputCapabilities Inputs { get; init; } = new();

    /// <summary>Upstream <c>objectGeneration</c>.</summary>
    public ModelCapability ObjectGeneration { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>tools</c>.</summary>
    public LanguageToolCapabilities Tools { get; init; } = new();

    /// <summary>Upstream <c>reasoning</c>.</summary>
    public LanguageReasoningCapabilities Reasoning { get; init; } = new();

    /// <summary>Upstream <c>sampling</c>.</summary>
    public LanguageSamplingCapabilities Sampling { get; init; } = new();

    /// <summary>Upstream <c>structuredOutput</c>.</summary>
    public CerebrasStructuredOutputCapabilities StructuredOutput { get; init; } = new();
}

/// <summary>A catalog entry of a Cerebras language model. Upstream <c>CerebrasLanguageModelEntry</c>.</summary>
public sealed class CerebrasLanguageModelEntry
{
    /// <summary>Upstream <c>capabilities.language</c>.</summary>
    public CerebrasLanguageCapabilities Language { get; init; } = new();

    /// <summary>Upstream <c>providerOptions.language</c>: support of each option in <see cref="CerebrasProviderOptions.Language"/>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; } =
        new Dictionary<string, ModelCapability>();
}

/// <summary>Cerebras model catalog. Upstream <c>cerebras-model-catalog.ts</c>.</summary>
public static class CerebrasModelCatalog
{
    // Preserve the provider page's model set and image-input exception. Upstream languageDefinitions.
    private static readonly (string ModelId, bool Image)[] LanguageDefinitions =
    [
        ("gpt-oss-120b", false),
        ("gemma-4-31b", true),
    ];

    /// <summary>Known language model ids. Upstream <c>CerebrasModelIds['language']</c>.</summary>
    public static IReadOnlyList<string> LanguageModelIds { get; } =
    [.. LanguageDefinitions.Select(d => d.ModelId)];

    /// <summary>Entries of every known model id. Upstream <c>cerebrasModelCatalog</c>; renamed because C# does not allow a member named like its class.</summary>
    public static IReadOnlyDictionary<string, CerebrasLanguageModelEntry> Entries { get; } =
        LanguageDefinitions.ToDictionary(
            d => d.ModelId,
            d => CreateLanguageModelEntry(d.Image),
            StringComparer.Ordinal
        );

    /// <summary>Returns the entry of a language model. Any other model id resolves to an entry with unknown support. Upstream <c>getCerebrasLanguageModelEntry</c>.</summary>
    public static CerebrasLanguageModelEntry GetCerebrasLanguageModelEntry(string modelId) =>
        Entries.TryGetValue(modelId, out var entry) ? entry : CreateLanguageModelEntry(null);

    private static CerebrasLanguageModelEntry CreateLanguageModelEntry(bool? image)
    {
        var support = image is not null ? ModelCapability.Supported : ModelCapability.Unknown;
        var language = new CerebrasLanguageCapabilities
        {
            Inputs = new LanguageInputCapabilities
            {
                Image = image is { } hasImage
                    ? ModelCapability.From(hasImage)
                    : ModelCapability.Unknown,
                Audio = ModelCapability.Unsupported,
            },
            ObjectGeneration = support,
            Tools = new LanguageToolCapabilities { Usage = support, Streaming = support },
            Reasoning = new LanguageReasoningCapabilities { Support = support },
            Sampling = new LanguageSamplingCapabilities
            {
                Temperature = ModelCapability.Unknown,
                TopP = ModelCapability.Unknown,
                Logprobs = support,
            },

            // Keep forwarding JSON schemas for custom IDs as the existing provider does.
            StructuredOutput = new CerebrasStructuredOutputCapabilities { JsonSchema = true },
        };

        return new CerebrasLanguageModelEntry
        {
            Language = language,
            ProviderOptions = DeclareProviderOptions(
                CerebrasProviderOptions.Language,
                support,
                new Dictionary<string, ModelCapability>
                {
                    ["strictJsonSchema"] = ModelCapability.When(
                        "Requires a JSON schema response format."
                    ),
                    ["serviceTier"] = ModelCapability.When(
                        "Requires a preview-enabled dedicated endpoint."
                    ),
                    ["queueThreshold"] = ModelCapability.When(
                        "Requires flex/auto on a preview-enabled dedicated endpoint."
                    ),
                    ["reasoningEffort"] = image is not null
                        ? ModelCapability.When("Supported values depend on the reasoning model.")
                        : ModelCapability.Unknown,
                    ["reasoningFormat"] = image is not null
                        ? ModelCapability.When("Available formats depend on the reasoning model.")
                        : ModelCapability.Unknown,
                }
            ),
        };
    }

    private static Dictionary<string, ModelCapability> DeclareProviderOptions(
        IReadOnlyDictionary<string, ProviderOptionDefinition> definitions,
        ModelCapability support,
        IReadOnlyDictionary<string, ModelCapability> exceptions
    )
    {
        var result = new Dictionary<string, ModelCapability>(StringComparer.Ordinal);
        foreach (var option in definitions.Keys)
        {
            result[option] = exceptions.TryGetValue(option, out var exception)
                ? exception
                : support;
        }

        return result;
    }
}
