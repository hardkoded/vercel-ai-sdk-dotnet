// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Moonshot;

/// <summary>
/// The message provider options of Moonshot AI. Maps to <c>moonshotaiAllMessageProviderOptions</c> in
/// <c>moonshotai-chat-options.ts</c>.
/// </summary>
internal sealed class MoonshotAIMessageProviderOptions
{
    /// <summary>The name of the participant represented by the message.</summary>
    public string? Name { get; init; }

    /// <summary>Whether the assistant message is a partial response that Moonshot continues.</summary>
    public bool? Partial { get; init; }

    /// <summary>Function tools to load at this point in a Kimi K3 conversation, or null when not set.</summary>
    public IReadOnlyList<LanguageModelTool>? Tools { get; init; }
}

/// <summary>The thinking configuration of <see cref="MoonshotAILanguageModelOptions"/>.</summary>
internal sealed class MoonshotAIThinkingOptions
{
    /// <summary><c>enabled</c> or <c>disabled</c>, or null when not set.</summary>
    public string? Type { get; init; }

    /// <summary>Deprecated thinking budget. Accepted for backwards compatibility, then omitted with a warning.</summary>
    public int? BudgetTokens { get; init; }
}

/// <summary>The context caching options of <see cref="MoonshotAILanguageModelOptions"/>.</summary>
internal sealed class MoonshotAIPromptCacheOptions
{
    /// <summary>The cache mode. Only <c>implicit</c> is accepted.</summary>
    public string? Mode { get; init; }

    /// <summary>The cache write tier, <c>5m</c> or <c>1h</c>, or null when not set.</summary>
    public string? Ttl { get; init; }
}

/// <summary>
/// The language model options of Moonshot AI. Maps to <c>MoonshotAILanguageModelOptions</c> and the
/// <c>moonshotaiLanguageModelOptions</c> schema in <c>moonshotai-chat-options.ts</c>.
/// </summary>
internal sealed class MoonshotAILanguageModelOptions
{
    /// <summary>Whether to use strict JSON schema validation for structured outputs.</summary>
    public bool? StrictJsonSchema { get; init; }

    /// <summary>Whether to return log probabilities for generated tokens.</summary>
    public bool? Logprobs { get; init; }

    /// <summary>Number of most likely tokens to return at each token position, 0 to 20.</summary>
    public int? TopLogprobs { get; init; }

    /// <summary>Reasoning effort for Kimi K3: <c>low</c>, <c>high</c> or <c>max</c>.</summary>
    public string? ReasoningEffort { get; init; }

    /// <summary>Static predicted content, as the wire object <c>{ type: "content", content }</c>.</summary>
    public JsonObject? Prediction { get; init; }

    /// <summary>Thinking configuration for Kimi K2.x models.</summary>
    public MoonshotAIThinkingOptions? Thinking { get; init; }

    /// <summary>Preserved reasoning behavior: <c>disabled</c>, <c>interleaved</c> or <c>preserved</c>.</summary>
    public string? ReasoningHistory { get; init; }

    /// <summary>Key used to cache responses for similar requests.</summary>
    public string? PromptCacheKey { get; init; }

    /// <summary>Context caching options for Kimi K3 models.</summary>
    public MoonshotAIPromptCacheOptions? PromptCacheOptions { get; init; }

    /// <summary>A stable identifier used to help Moonshot detect users violating usage policies.</summary>
    public string? SafetyIdentifier { get; init; }
}

/// <summary>Moonshot AI chat options. Maps to <c>moonshotai-chat-options.ts</c>.</summary>
internal static class MoonshotChatOptions
{
    /// <summary>
    /// Parses and validates <paramref name="providerOptions"/> under <paramref name="providerOptionsName"/> with the
    /// <c>moonshotaiLanguageModelOptions</c> schema. Unknown properties are dropped. Maps to <c>parseProviderOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentException">The options do not match the schema.</exception>
    public static MoonshotAILanguageModelOptions? ParseLanguageModelOptions(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
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

        bool? strictJsonSchema = null;
        bool? logprobs = null;
        int? topLogprobs = null;
        string? reasoningEffort = null;
        JsonObject? prediction = null;
        MoonshotAIThinkingOptions? thinking = null;
        string? reasoningHistory = null;
        string? promptCacheKey = null;
        MoonshotAIPromptCacheOptions? promptCacheOptions = null;
        string? safetyIdentifier = null;
        foreach (var property in bag.EnumerateObject())
        {
            var value = property.Value;
            switch (property.Name)
            {
                case "strictJsonSchema":
                    strictJsonSchema = AsBool(value, invalid);
                    break;
                case "logprobs":
                    logprobs = AsBool(value, invalid);
                    break;
                case "topLogprobs":
                    topLogprobs = AsInt(value, 0, 20, invalid);
                    break;
                case "reasoningEffort":
                    reasoningEffort = AsEnum(value, invalid, "low", "high", "max");
                    break;
                case "prediction":
                    prediction = ParsePrediction(value, invalid);
                    break;
                case "thinking":
                    thinking = ParseThinking(value, invalid);
                    break;
                case "reasoningHistory":
                    reasoningHistory = AsEnum(
                        value,
                        invalid,
                        "disabled",
                        "interleaved",
                        "preserved"
                    );
                    break;
                case "promptCacheKey":
                    promptCacheKey = AsString(value, invalid);
                    break;
                case "promptCacheOptions":
                    promptCacheOptions = ParsePromptCacheOptions(value, invalid);
                    break;
                case "safetyIdentifier":
                    safetyIdentifier = AsString(value, invalid);
                    break;
            }
        }

        return new MoonshotAILanguageModelOptions
        {
            StrictJsonSchema = strictJsonSchema,
            Logprobs = logprobs,
            TopLogprobs = topLogprobs,
            ReasoningEffort = reasoningEffort,
            Prediction = prediction,
            Thinking = thinking,
            ReasoningHistory = reasoningHistory,
            PromptCacheKey = promptCacheKey,
            PromptCacheOptions = promptCacheOptions,
            SafetyIdentifier = safetyIdentifier,
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
        return value.ValueKind == JsonValueKind.String ? value.GetString()! : throw invalid;
    }

    private static string AsEnum(
        JsonElement value,
        InvalidArgumentException invalid,
        params string[] allowed
    )
    {
        var text = AsString(value, invalid);
        return Array.IndexOf(allowed, text) >= 0 ? text : throw invalid;
    }

    private static int AsInt(JsonElement value, int min, int max, InvalidArgumentException invalid)
    {
        if (
            value.ValueKind != JsonValueKind.Number
            || !value.TryGetDouble(out var number)
            || number != Math.Floor(number)
            || number < min
            || number > max
        )
        {
            throw invalid;
        }

        return (int)number;
    }

    private static JsonObject ParsePrediction(JsonElement value, InvalidArgumentException invalid)
    {
        if (
            value.ValueKind != JsonValueKind.Object
            || !value.TryGetProperty("type", out var type)
            || type.ValueKind != JsonValueKind.String
            || type.GetString() != "content"
            || !value.TryGetProperty("content", out var content)
        )
        {
            throw invalid;
        }

        JsonNode contentNode;
        if (content.ValueKind == JsonValueKind.String)
        {
            contentNode = content.GetString()!;
        }
        else if (content.ValueKind == JsonValueKind.Array)
        {
            var parts = new JsonArray();
            foreach (var part in content.EnumerateArray())
            {
                if (
                    part.ValueKind != JsonValueKind.Object
                    || !part.TryGetProperty("type", out var partType)
                    || partType.ValueKind != JsonValueKind.String
                    || partType.GetString() != "text"
                    || !part.TryGetProperty("text", out var text)
                    || text.ValueKind != JsonValueKind.String
                )
                {
                    throw invalid;
                }

                parts.Add(new JsonObject { ["type"] = "text", ["text"] = text.GetString() });
            }

            contentNode = parts;
        }
        else
        {
            throw invalid;
        }

        return new JsonObject { ["type"] = "content", ["content"] = contentNode };
    }

    private static MoonshotAIThinkingOptions ParseThinking(
        JsonElement value,
        InvalidArgumentException invalid
    )
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw invalid;
        }

        string? type = null;
        int? budgetTokens = null;
        foreach (var property in value.EnumerateObject())
        {
            switch (property.Name)
            {
                case "type":
                    type = AsEnum(property.Value, invalid, "enabled", "disabled");
                    break;
                case "budgetTokens":
                    budgetTokens = AsInt(property.Value, 1024, int.MaxValue, invalid);
                    break;
            }
        }

        return new MoonshotAIThinkingOptions { Type = type, BudgetTokens = budgetTokens };
    }

    private static MoonshotAIPromptCacheOptions ParsePromptCacheOptions(
        JsonElement value,
        InvalidArgumentException invalid
    )
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw invalid;
        }

        string? mode = null;
        string? ttl = null;
        foreach (var property in value.EnumerateObject())
        {
            switch (property.Name)
            {
                case "mode":
                    mode = AsEnum(property.Value, invalid, "implicit");
                    break;
                case "ttl":
                    ttl = AsEnum(property.Value, invalid, "5m", "1h");
                    break;
            }
        }

        return new MoonshotAIPromptCacheOptions { Mode = mode, Ttl = ttl };
    }

    /// <summary>
    /// Parses and validates <paramref name="providerOptions"/> under <paramref name="providerOptionsName"/> with the
    /// <c>moonshotaiAllMessageProviderOptions</c> schema. Maps to <c>parseProviderOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentException">The options do not match the schema.</exception>
    public static MoonshotAIMessageProviderOptions? ParseMessageProviderOptions(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
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

        string? name = null;
        bool? partial = null;
        List<LanguageModelTool>? tools = null;
        foreach (var property in bag.EnumerateObject())
        {
            switch (property.Name)
            {
                case "name":
                    name =
                        property.Value.ValueKind == JsonValueKind.String
                            ? property.Value.GetString()
                            : throw invalid;
                    break;
                case "partial":
                    partial = property.Value.ValueKind == JsonValueKind.True ? true : throw invalid;
                    break;
                case "tools":
                    tools = ParseTools(property.Value, invalid);
                    break;
            }
        }

        return new MoonshotAIMessageProviderOptions
        {
            Name = name,
            Partial = partial,
            Tools = tools,
        };
    }

    /// <summary>Maps to <c>moonshotaiDynamicToolSchema</c>.</summary>
    private static List<LanguageModelTool> ParseTools(
        JsonElement value,
        InvalidArgumentException invalid
    )
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw invalid;
        }

        var tools = new List<LanguageModelTool>();
        foreach (var tool in value.EnumerateArray())
        {
            if (
                tool.ValueKind != JsonValueKind.Object
                || !tool.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String
                || type.GetString() != "function"
                || !tool.TryGetProperty("name", out var name)
                || name.ValueKind != JsonValueKind.String
                || !tool.TryGetProperty("inputSchema", out var inputSchema)
                || inputSchema.ValueKind != JsonValueKind.Object
            )
            {
                throw invalid;
            }

            string? description = null;
            if (tool.TryGetProperty("description", out var descriptionElement))
            {
                description =
                    descriptionElement.ValueKind == JsonValueKind.String
                        ? descriptionElement.GetString()
                        : throw invalid;
            }

            bool? strict = null;
            if (tool.TryGetProperty("strict", out var strictElement))
            {
                strict = strictElement.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => throw invalid,
                };
            }

            tools.Add(
                new LanguageModelTool(name.GetString()!, description, inputSchema.Clone(), strict)
            );
        }

        return tools;
    }
}
