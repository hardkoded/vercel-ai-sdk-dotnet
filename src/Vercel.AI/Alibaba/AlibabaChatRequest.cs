// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Alibaba;

/// <summary>Builds the Alibaba chat request on top of the shared Chat Completions body. Maps to <c>AlibabaChatLanguageModel.getArgs</c>.</summary>
internal static class AlibabaChatRequest
{
    private const int MaxThinkingBudget = 16384;

    // Alibaba options that map to their own wire fields. The shared chat model would copy them as extra body fields.
    private static readonly string[] OptionNames = { "enableThinking", "thinkingBudget", "preserveThinking", "parallelToolCalls" };

    private static readonly string[] PreservedThinkingModelIds =
    {
        "kimi-k2.7-code",
        "qwen3.6-max-preview",
        "qwen3.6-plus",
        "qwen3.6-plus-2026-04-02",
        "qwen3.7-flash",
        "qwen3.7-flash-2026-07-15",
        "qwen3.7-max",
        "qwen3.7-max-2026-05-17",
        "qwen3.7-max-2026-05-20",
        "qwen3.7-max-2026-06-08",
        "qwen3.7-max-preview",
        "qwen3.7-plus",
        "qwen3.7-plus-2026-05-26",
        "qwen3.8-flash",
        "qwen3.8-max",
        "qwen3.8-max-0902",
    };

    private static readonly string[] JsonSchemaModelPrefixes =
    {
        "qwen3.7-plus",
        "qwen3.7-flash",
        "qwen3.7-max",
        "qwen3.8-max",
        "qwen3.8-flash",
    };

    public static bool SupportsPreservedThinking(string modelId)
    {
        return Array.IndexOf(PreservedThinkingModelIds, modelId) >= 0;
    }

    public static bool SupportsJsonSchemaOutput(string modelId)
    {
        foreach (var prefix in JsonSchemaModelPrefixes)
        {
            if (modelId == prefix || modelId.StartsWith(prefix + "-", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static JsonObject Transform(string modelId, LanguageModelCallOptions options, JsonObject body, IList<CallWarning> warnings)
    {
        var alibaba = options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue(AlibabaProvider.ProviderId, out var bag)
            && bag.ValueKind == JsonValueKind.Object
                ? bag
                : (JsonElement?)null;
        foreach (var name in OptionNames)
        {
            body.Remove(name);
        }

        // Alibaba accepts top_k and has no frequency penalty. The shared request does the opposite.
        for (var i = warnings.Count - 1; i >= 0; i--)
        {
            if (warnings[i].Type == "unsupported" && warnings[i].Feature == "topK")
            {
                warnings.RemoveAt(i);
            }
        }

        body.Remove("frequency_penalty");
        if (options.FrequencyPenalty != null)
        {
            warnings.Insert(0, new CallWarning("unsupported", "frequencyPenalty", null));
        }

        if (options.TopK is { } topK)
        {
            body["top_k"] = topK;
        }

        // Preserved thinking defaults to on for models that support it. An explicit option always passes through.
        // Unsupported models without an explicit option omit the wire field.
        var preserveThinking = ReadBool(alibaba, "preserveThinking") ?? (SupportsPreservedThinking(modelId) ? true : (bool?)null);

        var hasSchema = options.JsonSchema is { ValueKind: not (JsonValueKind.Undefined or JsonValueKind.Null) };
        var json = body.ContainsKey("response_format");
        var useJsonSchema = json && hasSchema && SupportsJsonSchemaOutput(modelId);
        var useJsonObject = json && !useJsonSchema;
        for (var i = warnings.Count - 1; i >= 0; i--)
        {
            if (warnings[i].Type == "unsupported" && warnings[i].Feature == "responseFormat")
            {
                warnings.RemoveAt(i);
            }
        }

        if (useJsonSchema)
        {
            var schema = new JsonObject
            {
                ["schema"] = JsonNode.Parse(options.JsonSchema!.Value.GetRawText()),
                ["name"] = options.JsonSchemaName ?? "response",
            };
            if (options.JsonSchemaDescription != null)
            {
                schema["description"] = options.JsonSchemaDescription;
            }

            body["response_format"] = new JsonObject { ["type"] = "json_schema", ["json_schema"] = schema };
        }
        else if (useJsonObject)
        {
            body["response_format"] = new JsonObject { ["type"] = "json_object" };
            if (hasSchema)
            {
                warnings.Add(new CallWarning(
                    "compatibility",
                    "responseFormat JSON schema",
                    "Alibaba does not support JSON Schema output for model " + modelId + ". "
                    + "JSON Object mode is used instead. The schema was injected into the system message and will only be validated locally."));
            }
        }

        var prompt = useJsonObject ? InjectJsonInstruction(options.Prompt, hasSchema ? options.JsonSchema : null) : options.Prompt;
        var cacheControlValidator = new AlibabaCacheControlValidator();
        body["messages"] = AlibabaChatMessages.Convert(prompt, preserveThinking ?? false, cacheControlValidator);
        foreach (var warning in cacheControlValidator.Warnings)
        {
            warnings.Add(warning);
        }

        var thinking = ResolveThinking(options.Reasoning, alibaba, warnings);
        foreach (var pair in thinking)
        {
            body[pair.Key] = pair.Value;
        }

        if (preserveThinking is { } preserve)
        {
            body["preserve_thinking"] = preserve;
        }

        var prepared = AlibabaPrepareTools.Prepare(options.Tools, options.ToolChoice);
        if (prepared.Tools != null)
        {
            body["tools"] = prepared.Tools;
        }

        if (prepared.ToolChoice != null)
        {
            body["tool_choice"] = prepared.ToolChoice;
        }

        if (prepared.Tools != null && ReadBool(alibaba, "parallelToolCalls") is { } parallel)
        {
            body["parallel_tool_calls"] = parallel;
        }

        foreach (var warning in prepared.ToolWarnings)
        {
            warnings.Add(warning);
        }

        return body;
    }

    public static LanguageModelUsage ConvertUsage(JsonElement usage)
    {
        var prompt = ReadNumber(usage, "prompt_tokens") ?? 0;
        var completion = ReadNumber(usage, "completion_tokens") ?? 0;
        var cacheRead = ReadNestedNumber(usage, "prompt_tokens_details", "cached_tokens") ?? 0;
        var cacheWrite = ReadNestedNumber(usage, "prompt_tokens_details", "cache_creation_input_tokens") ?? 0;
        var reasoning = ReadNestedNumber(usage, "completion_tokens_details", "reasoning_tokens") ?? 0;
        return new LanguageModelUsage(
            prompt,
            completion,
            ReadNumber(usage, "total_tokens"),
            cacheReadTokens: cacheRead,
            cacheWriteTokens: cacheWrite,
            reasoningTokens: reasoning,
            raw: usage.Clone(),
            // Alibaba counts both cache reads and cache writes inside prompt_tokens.
            noCacheInputTokens: prompt - cacheRead - cacheWrite,
            textTokens: Math.Max(0, completion - reasoning));
    }

    private static IReadOnlyList<ModelMessage> InjectJsonInstruction(IReadOnlyList<ModelMessage> prompt, JsonElement? schema)
    {
        var first = prompt.Count > 0 ? prompt[0] as SystemModelMessage : null;
        var instruction = Vercel.AI.GenerateText.JsonInstructions.InjectJsonInstruction(first?.Content, schema);
        var messages = new List<ModelMessage>(prompt.Count + 1) { new SystemModelMessage(instruction) };
        for (var i = first == null ? 0 : 1; i < prompt.Count; i++)
        {
            messages.Add(prompt[i]);
        }

        return messages;
    }

    private static List<KeyValuePair<string, JsonNode?>> ResolveThinking(string? reasoning, JsonElement? alibaba, IList<CallWarning> warnings)
    {
        var result = new List<KeyValuePair<string, JsonNode?>>();
        var enableThinking = ReadBool(alibaba, "enableThinking");
        var budget = alibaba is { } options && options.TryGetProperty("thinkingBudget", out var budgetElement) && budgetElement.ValueKind == JsonValueKind.Number
            ? JsonNode.Parse(budgetElement.GetRawText())
            : null;
        if (enableThinking != null || budget != null)
        {
            if (enableThinking != null)
            {
                result.Add(new KeyValuePair<string, JsonNode?>("enable_thinking", enableThinking));
            }

            if (budget != null)
            {
                result.Add(new KeyValuePair<string, JsonNode?>("thinking_budget", budget));
            }

            return result;
        }

        if (!ReasoningMap.IsCustomReasoning(reasoning))
        {
            return result;
        }

        if (reasoning == "none")
        {
            result.Add(new KeyValuePair<string, JsonNode?>("enable_thinking", false));
            return result;
        }

        var modelWarnings = new List<ModelWarning>();
        var thinkingBudget = ReasoningMap.MapReasoningToProviderBudget(reasoning!, MaxThinkingBudget, MaxThinkingBudget, modelWarnings);
        foreach (var warning in modelWarnings)
        {
            if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
            {
                warnings.Add(callWarning);
            }
        }

        result.Add(new KeyValuePair<string, JsonNode?>("enable_thinking", true));
        if (thinkingBudget is { } value)
        {
            result.Add(new KeyValuePair<string, JsonNode?>("thinking_budget", value));
        }

        return result;
    }

    private static bool? ReadBool(JsonElement? bag, string name)
    {
        return bag is { } options
            && options.TryGetProperty(name, out var value)
            && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
                ? value.GetBoolean()
                : null;
    }

    private static int? ReadNumber(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
                ? number
                : null;
    }

    private static int? ReadNestedNumber(JsonElement element, string outer, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(outer, out var inner)
            ? ReadNumber(inner, name)
            : null;
    }
}
