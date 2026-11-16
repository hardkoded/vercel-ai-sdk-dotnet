// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Mistral;

/// <summary>Reasoning support of a model. Maps to the catalog's <c>ReasoningCapabilities</c>.</summary>
internal sealed class MistralReasoningCapabilities
{
    /// <summary>Creates reasoning capabilities.</summary>
    public MistralReasoningCapabilities(IReadOnlyList<string> efforts)
    {
        Efforts = efforts;
        Support = efforts.Count > 0;
    }

    /// <summary>Whether the model accepts reasoning configuration.</summary>
    public bool Support { get; }

    /// <summary>The accepted reasoning efforts.</summary>
    public IReadOnlyList<string> Efforts { get; }
}

/// <summary>Language model entry of the Mistral catalog. Maps to <c>MistralLanguageModelEntry</c>.</summary>
internal sealed class MistralLanguageModelEntry
{
    /// <summary>Creates an entry.</summary>
    public MistralLanguageModelEntry(MistralReasoningCapabilities reasoning, MistralReasoningCapabilities conversationReasoning)
    {
        Reasoning = reasoning;
        ConversationReasoning = conversationReasoning;
    }

    /// <summary>Reasoning support of the Chat API. Maps to <c>capabilities.language.reasoning</c>.</summary>
    public MistralReasoningCapabilities Reasoning { get; }

    /// <summary>Reasoning support of the Conversations API. Maps to <c>capabilities.language.endpoints.conversation.reasoning</c>.</summary>
    public MistralReasoningCapabilities ConversationReasoning { get; }
}

/// <summary>Mistral model catalog. Maps to <c>mistral-model-catalog.ts</c>. Only the reasoning capabilities are ported.</summary>
internal static class MistralModelCatalog
{
    // Known non-reasoning models from https://api.mistral.ai/v1/models (2026-10-06).
    // Unknown IDs inherit current reasoning configuration support.
    private static readonly HashSet<string> ModelsWithoutReasoningEffort = new(StringComparer.Ordinal)
    {
        "codestral-2508",
        "codestral-latest",
        "ministral-14b-2512",
        "ministral-14b-latest",
        "ministral-3b-2512",
        "ministral-3b-latest",
        "ministral-8b-2512",
        "ministral-8b-latest",
        "mistral-code-fim-latest",
        "mistral-code-latest",
        "mistral-large-latest",
        "mistral-large-2512",
        "voxtral-small-2507",
        "voxtral-small-latest",
    };

    // GLM-5.3 models accept only low, high, and max reasoning effort and cannot disable reasoning.
    // Omitting reasoning_effort uses max. https://docs.mistral.ai/studio/conversations/reasoning (2026-10-26)
    private static readonly HashSet<string> ModelsWithLowHighMaxReasoningEffort = new(StringComparer.Ordinal)
    {
        "zai-glm-5",
        "zai-glm-5-3",
        "zai-glm-5-3-flash",
        "zai-glm-latest",
    };

    // Mistral Large 4 Chat models accept low, medium, and high reasoning effort and cannot disable reasoning (see #24350).
    private static readonly HashSet<string> ModelsWithLowMediumHighReasoningEffort = new(StringComparer.Ordinal)
    {
        "mistral-large-4",
        "mistral-large-4-0",
    };

    // https://api.mistral.ai/v1/models (2026-10-06)
    private static readonly HashSet<string> ReasoningEffortModelIds = new(StringComparer.Ordinal)
    {
        "glm-5-2",
        "labs-leanstral-1-5",
        "labs-leanstral-1-5-1",
        "magistral-medium-latest",
        "magistral-small-latest",
        "mistral-large-4",
        "mistral-large-4-0",
        "mistral-medium",
        "mistral-medium-2604",
        "mistral-medium-3",
        "mistral-medium-3-5",
        "mistral-medium-3.5",
        "mistral-medium-latest",
        "mistral-small-2603",
        "mistral-small-latest",
        "mistral-vibe-cli-fast",
        "mistral-vibe-cli-latest",
        "mistral-vibe-cli-with-tools",
        "zai-glm-5-2",
    };

    /// <summary>Returns the catalog entry of a language model. Maps to <c>getMistralLanguageModelEntry</c>.</summary>
    public static MistralLanguageModelEntry GetMistralLanguageModelEntry(string modelId)
    {
        var reasoning = new MistralReasoningCapabilities(
            ModelsWithoutReasoningEffort.Contains(modelId)
                ? Array.Empty<string>()
                : ModelsWithLowHighMaxReasoningEffort.Contains(modelId)
                    ? new[] { "low", "high", "max" }
                    : ModelsWithLowMediumHighReasoningEffort.Contains(modelId)
                        ? new[] { "low", "medium", "high" }
                        : new[] { "none", "high" });
        var conversationReasoning = new MistralReasoningCapabilities(
            ReasoningEffortModelIds.Contains(modelId) ? new[] { "none", "high" } : Array.Empty<string>());
        return new MistralLanguageModelEntry(reasoning, conversationReasoning);
    }
}
