// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Mistral language model provider options. Maps to <c>MistralLanguageModelChatOptions</c>.</summary>
public sealed class MistralLanguageModelChatOptions
{
    /// <summary>Whether to inject a safety prompt before all conversations. Defaults to <c>false</c>.</summary>
    public bool? SafePrompt { get; init; }

    /// <summary>Maximum number of document images.</summary>
    public double? DocumentImageLimit { get; init; }

    /// <summary>Maximum number of document pages.</summary>
    public double? DocumentPageLimit { get; init; }

    /// <summary>Whether to use structured outputs. Defaults to <c>true</c>.</summary>
    public bool? StructuredOutputs { get; init; }

    /// <summary>Whether to use strict JSON schema validation. Defaults to <c>false</c>.</summary>
    public bool? StrictJsonSchema { get; init; }

    /// <summary>
    /// Whether to enable parallel function calling during tool use. When set to <c>false</c>, the model uses at most one
    /// tool per response. Defaults to <c>true</c>.
    /// </summary>
    public bool? ParallelToolCalls { get; init; }

    /// <summary>A stable identifier used to route requests with shared prompt prefixes to the same cache.</summary>
    public string? PromptCacheKey { get; init; }

    /// <summary>
    /// Reasoning effort for models that support adjustable reasoning: <c>high</c>, <c>none</c>, <c>low</c>, <c>medium</c>,
    /// or <c>max</c>. Which values a model accepts depends on the model.
    /// </summary>
    public string? ReasoningEffort { get; init; }

    internal static Dictionary<string, OptionValidator> Shape()
    {
        return new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
        {
            ["safePrompt"] = ProviderOptionShape.Boolean(),
            ["documentImageLimit"] = ProviderOptionShape.Number(),
            ["documentPageLimit"] = ProviderOptionShape.Number(),
            ["structuredOutputs"] = ProviderOptionShape.Boolean(),
            ["strictJsonSchema"] = ProviderOptionShape.Boolean(),
            ["parallelToolCalls"] = ProviderOptionShape.Boolean(),
            ["promptCacheKey"] = ProviderOptionShape.String(),
            ["reasoningEffort"] = ProviderOptionShape.Enum("high", "none", "low", "medium", "max"),
        };
    }

    internal static MistralLanguageModelChatOptions FromJson(JsonElement value)
    {
        return new MistralLanguageModelChatOptions
        {
            SafePrompt = Bool(value, "safePrompt"),
            DocumentImageLimit = Number(value, "documentImageLimit"),
            DocumentPageLimit = Number(value, "documentPageLimit"),
            StructuredOutputs = Bool(value, "structuredOutputs"),
            StrictJsonSchema = Bool(value, "strictJsonSchema"),
            ParallelToolCalls = Bool(value, "parallelToolCalls"),
            PromptCacheKey = value.TryGetProperty("promptCacheKey", out var key) && key.ValueKind == JsonValueKind.String ? key.GetString() : null,
            ReasoningEffort = value.TryGetProperty("reasoningEffort", out var effort) && effort.ValueKind == JsonValueKind.String ? effort.GetString() : null,
        };
    }

    private static bool? Bool(JsonElement value, string name)
    {
        return value.TryGetProperty(name, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : null;
    }

    private static double? Number(JsonElement value, string name)
    {
        return value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number ? property.GetDouble() : null;
    }
}
