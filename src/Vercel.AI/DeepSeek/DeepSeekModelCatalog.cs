// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.DeepSeek;

/// <summary>What the serving endpoint supports. Maps to <c>DeepSeekEndpointOptions</c>.</summary>
internal sealed class DeepSeekEndpointOptions
{
    /// <summary>Maps to <c>supportsAssistantPrefixCompletion</c>.</summary>
    public bool? SupportsAssistantPrefixCompletion { get; init; }

    /// <summary>Maps to <c>supportsStrictToolCalls</c>.</summary>
    public bool? SupportsStrictToolCalls { get; init; }

    /// <summary>Maps to <c>supportsPenaltySampling</c>.</summary>
    public bool? SupportsPenaltySampling { get; init; }

    /// <summary>Maps to <c>supportsThinking</c>.</summary>
    public bool? SupportsThinking { get; init; }

    /// <summary>Maps to <c>supportsStructuredOutputs</c>.</summary>
    public bool? SupportsStructuredOutputs { get; init; }
}

/// <summary>
/// The capabilities of a DeepSeek language model on one endpoint. Maps to <c>DeepSeekLanguageModelEntry</c>.
/// Only the capabilities the chat model reads are ported.
/// </summary>
internal sealed class DeepSeekLanguageModelEntry
{
    /// <summary>Maps to <c>capabilities.language.tools.strict</c>. Null is <c>unknown</c>.</summary>
    public bool? ToolsStrict { get; init; }

    /// <summary>Maps to <c>capabilities.language.reasoning.configurable</c>.</summary>
    public bool ReasoningConfigurable { get; init; }

    /// <summary>Maps to <c>capabilities.language.reasoning.enabledByDefault</c>.</summary>
    public bool ReasoningEnabledByDefault { get; init; }

    /// <summary>Maps to <c>capabilities.language.reasoning.assistantHistory</c>.</summary>
    public bool ReasoningAssistantHistory { get; init; }

    /// <summary>Maps to <c>capabilities.language.sampling.penalties</c>.</summary>
    public bool SamplingPenalties { get; init; }

    /// <summary>Maps to <c>capabilities.language.structuredOutput.jsonSchema</c>.</summary>
    public bool StructuredOutputJsonSchema { get; init; }

    /// <summary>Maps to <c>capabilities.language.endpoints.assistantPrefixCompletion</c>.</summary>
    public bool EndpointsAssistantPrefixCompletion { get; init; }
}

/// <summary>DeepSeek model catalog. Maps to <c>deepseek-model-catalog.ts</c>. Only the capabilities the chat model reads are ported.</summary>
internal static class DeepSeekModelCatalog
{
    // Legacy aliases and pre-V4 IDs retain their previous protocol. Unknown IDs inherit the current protocol,
    // including assistant reasoning history.
    private static readonly Regex LegacyModelId = new(
        "(^|[/.])deepseek-(?:chat|reasoner|v[1-3](?:\\.\\d+)?)(?=[-/:.]|\\z)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    /// <summary>Returns the entry of a model on an endpoint. Maps to <c>getDeepSeekLanguageModelEntry</c>.</summary>
    public static DeepSeekLanguageModelEntry GetDeepSeekLanguageModelEntry(
        string modelId,
        DeepSeekEndpointOptions? endpoint = null
    )
    {
        endpoint ??= new DeepSeekEndpointOptions();
        var assistantHistory = !LegacyModelId.IsMatch(modelId);
        return new DeepSeekLanguageModelEntry
        {
            ToolsStrict = endpoint.SupportsStrictToolCalls,
            ReasoningConfigurable = endpoint.SupportsThinking != false,
            ReasoningEnabledByDefault = modelId == "deepseek-reasoner" || assistantHistory,
            ReasoningAssistantHistory = assistantHistory,
            SamplingPenalties = endpoint.SupportsPenaltySampling == true,
            StructuredOutputJsonSchema = endpoint.SupportsStructuredOutputs == true,
            EndpointsAssistantPrefixCompletion = endpoint.SupportsAssistantPrefixCompletion == true,
        };
    }
}
