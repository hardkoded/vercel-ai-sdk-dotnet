// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.Xai;

/// <summary>Model capability checks for xAI models.</summary>
internal static class XaiReasoningEffort
{
    // grok-4.20 reasoning and non-reasoning models, including dated variants, reject the effort parameter for every value.
    private static readonly Regex ModelsWithoutReasoningEffort = new Regex(
        @"^grok-4\.20(-\d{4})?-(non-)?reasoning$",
        RegexOptions.CultureInvariant);

    /// <summary>False for models that reject the reasoning effort parameter. Maps to <c>supportsReasoningEffort</c>.</summary>
    public static bool Supports(string modelId)
    {
        return !ModelsWithoutReasoningEffort.IsMatch(modelId);
    }
}

/// <summary>Converts xAI Responses usage and finish reasons.</summary>
internal static class XaiResponsesResults
{
    /// <summary>Usage reported when the response has none.</summary>
    public static LanguageModelUsage MissingUsage()
    {
        return new LanguageModelUsage(0, 0, 0, cacheReadTokens: 0, cacheWriteTokens: 0, reasoningTokens: 0, noCacheInputTokens: 0, textTokens: 0);
    }

    /// <summary>Converts a Responses <c>usage</c> object. Maps to <c>convertXaiResponsesUsage</c>.</summary>
    /// <exception cref="InvalidOperationException">A known usage field has the wrong type.</exception>
    public static LanguageModelUsage ConvertUsage(JsonElement usage)
    {
        if (usage.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("The xAI usage must be an object.");
        }

        var inputTokens = RequiredInt(usage, "input_tokens");
        var outputTokens = RequiredInt(usage, "output_tokens");
        OptionalInt(usage, "total_tokens");
        var cacheRead = NestedInt(usage, "input_tokens_details", "cached_tokens") ?? 0;
        var reasoning = NestedInt(usage, "output_tokens_details", "reasoning_tokens") ?? 0;

        // Some responses report cached tokens as an addition to input tokens instead of a part of them.
        var inputIncludesCached = cacheRead <= inputTokens;
        return new LanguageModelUsage(
            inputIncludesCached ? inputTokens : inputTokens + cacheRead,
            outputTokens,
            null,
            cacheReadTokens: cacheRead,
            reasoningTokens: reasoning,
            raw: usage.Clone(),
            noCacheInputTokens: inputIncludesCached ? inputTokens - cacheRead : inputTokens,
            textTokens: outputTokens - reasoning);
    }

    /// <summary>Maps a Responses status or incomplete reason. Maps to <c>mapXaiResponsesFinishReason</c>.</summary>
    public static FinishReason MapFinishReason(string? finishReason)
    {
        switch (finishReason)
        {
            case "stop":
            case "completed":
                return FinishReason.Stop;
            case "length":
            case "max_output_tokens":
                return FinishReason.Length;
            case "tool_calls":
            case "function_call":
                return FinishReason.ToolCalls;
            case "content_filter":
                return FinishReason.ContentFilter;
            default:
                return FinishReason.Other;
        }
    }

    private static int RequiredInt(JsonElement element, string name)
    {
        return OptionalInt(element, name) ?? throw new InvalidOperationException("The xAI usage is missing " + name + ".");
    }

    private static int? OptionalInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
        {
            throw new InvalidOperationException("The xAI usage field " + name + " must be an integer.");
        }

        return number;
    }

    private static int? NestedInt(JsonElement element, string objectName, string name)
    {
        if (!element.TryGetProperty(objectName, out var child) || child.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return OptionalInt(child, name);
    }
}
