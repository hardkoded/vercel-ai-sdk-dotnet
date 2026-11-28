// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Groq;

/// <summary>
/// The language model options of Groq. Maps to <c>GroqLanguageModelChatOptions</c> and the
/// <c>groqLanguageModelChatOptions</c> schema in <c>groq-chat-language-model-options.ts</c>.
/// </summary>
internal sealed class GroqLanguageModelChatOptions
{
    /// <summary><c>parsed</c>, <c>raw</c> or <c>hidden</c>, or null when not set.</summary>
    public string? ReasoningFormat { get; init; }

    /// <summary>Reasoning effort level: <c>none</c>, <c>default</c>, <c>low</c>, <c>medium</c> or <c>high</c>.</summary>
    public string? ReasoningEffort { get; init; }

    /// <summary>Whether to enable parallel function calling during tool use.</summary>
    public bool? ParallelToolCalls { get; init; }

    /// <summary>A unique identifier representing the end user.</summary>
    public string? User { get; init; }

    /// <summary>Whether to use structured outputs.</summary>
    public bool? StructuredOutputs { get; init; }

    /// <summary>Whether to use strict JSON schema validation.</summary>
    public bool? StrictJsonSchema { get; init; }

    /// <summary>Service tier: <c>on_demand</c>, <c>performance</c>, <c>flex</c> or <c>auto</c>.</summary>
    public string? ServiceTier { get; init; }
}

/// <summary>Groq chat options schema. Maps to <c>groq-chat-language-model-options.ts</c>.</summary>
internal static class GroqChatLanguageModelOptions
{
    /// <summary>
    /// Parses and validates <paramref name="providerOptions"/> under <paramref name="providerOptionsName"/> with the
    /// <c>groqLanguageModelChatOptions</c> schema. Unknown properties are dropped. Maps to <c>parseProviderOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentException">The options do not match the schema.</exception>
    public static GroqLanguageModelChatOptions? Parse(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName = "groq"
    )
    {
        if (
            providerOptions == null
            || !providerOptions.TryGetValue(providerOptionsName, out var bag)
            || bag.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return null;
        }

        var invalid = new InvalidArgumentException(
            "providerOptions",
            bag.GetRawText(),
            $"invalid {providerOptionsName} provider options"
        );
        if (bag.ValueKind != JsonValueKind.Object)
        {
            throw invalid;
        }

        string? reasoningFormat = null;
        string? reasoningEffort = null;
        bool? parallelToolCalls = null;
        string? user = null;
        bool? structuredOutputs = null;
        bool? strictJsonSchema = null;
        string? serviceTier = null;
        foreach (var property in bag.EnumerateObject())
        {
            var value = property.Value;
            if (value.ValueKind == JsonValueKind.Undefined)
            {
                continue;
            }

            switch (property.Name)
            {
                case "reasoningFormat":
                    reasoningFormat = AsEnum(value, invalid, "parsed", "raw", "hidden");
                    break;
                case "reasoningEffort":
                    reasoningEffort = AsEnum(
                        value,
                        invalid,
                        "none",
                        "default",
                        "low",
                        "medium",
                        "high"
                    );
                    break;
                case "parallelToolCalls":
                    parallelToolCalls = AsBool(value, invalid);
                    break;
                case "user":
                    user = AsString(value, invalid);
                    break;
                case "structuredOutputs":
                    structuredOutputs = AsBool(value, invalid);
                    break;
                case "strictJsonSchema":
                    strictJsonSchema = AsBool(value, invalid);
                    break;
                case "serviceTier":
                    serviceTier = AsEnum(
                        value,
                        invalid,
                        "on_demand",
                        "performance",
                        "flex",
                        "auto"
                    );
                    break;
            }
        }

        return new GroqLanguageModelChatOptions
        {
            ReasoningFormat = reasoningFormat,
            ReasoningEffort = reasoningEffort,
            ParallelToolCalls = parallelToolCalls,
            User = user,
            StructuredOutputs = structuredOutputs,
            StrictJsonSchema = strictJsonSchema,
            ServiceTier = serviceTier,
        };
    }

    private static bool AsBool(JsonElement value, InvalidArgumentException invalid)
    {
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw invalid,
        };
    }

    private static string AsString(JsonElement value, InvalidArgumentException invalid)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()!,
            _ => throw invalid,
        };
    }

    private static string AsEnum(
        JsonElement value,
        InvalidArgumentException invalid,
        params string[] allowed
    )
    {
        return
            value.ValueKind == JsonValueKind.String
            && Array.IndexOf(allowed, value.GetString()) >= 0
            ? value.GetString()!
            : throw invalid;
    }
}
