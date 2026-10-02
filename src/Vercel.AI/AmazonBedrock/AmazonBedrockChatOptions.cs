// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Parsed Bedrock chat settings.</summary>
public sealed class AmazonBedrockChatSettings
{
    /// <summary>Structured output mode: <c>outputFormat</c>, <c>jsonTool</c>, or <c>auto</c>.</summary>
    public string? StructuredOutputMode { get; set; }

    /// <summary>Service tier: <c>reserved</c>, <c>priority</c>, <c>default</c>, or <c>flex</c>.</summary>
    public string? ServiceTier { get; set; }

    /// <summary>Extra Converse fields.</summary>
    public JsonElement? AdditionalModelRequestFields { get; set; }

    /// <summary>Reasoning configuration.</summary>
    public AmazonBedrockReasoningSettings? Reasoning { get; set; }

    /// <summary>Anthropic beta headers expressed as body fields.</summary>
    public IReadOnlyList<string>? AnthropicBeta { get; set; }

    /// <summary>String map copied to <c>requestMetadata</c>.</summary>
    public JsonObject? RequestMetadata { get; set; }

    /// <summary>Provider-option keys that are not interpreted and are copied onto the command.</summary>
    public JsonObject? Passthrough { get; set; }
}

/// <summary>Bedrock reasoning configuration.</summary>
public sealed class AmazonBedrockReasoningSettings
{
    /// <summary><c>enabled</c>, <c>disabled</c>, or <c>adaptive</c>.</summary>
    public string? Type { get; set; }

    /// <summary>Token budget for enabled thinking.</summary>
    public int? BudgetTokens { get; set; }

    /// <summary>Effort for adaptive or non-Anthropic reasoning.</summary>
    public string? MaxReasoningEffort { get; set; }

    /// <summary>Adaptive thinking display, <c>omitted</c> or <c>summarized</c>.</summary>
    public string? Display { get; set; }
}

/// <summary>Validates Amazon Bedrock chat provider options.</summary>
public static class AmazonBedrockChatOptions
{
    private static readonly string[] StructuredOutputModes = { "outputFormat", "jsonTool", "auto" };

    private static readonly string[] ServiceTiers = { "reserved", "priority", "default", "flex" };

    private static readonly string[] ReasoningTypes = { "enabled", "disabled", "adaptive" };

    private static readonly string[] Efforts = { "low", "medium", "high", "xhigh", "max" };

    private static readonly string[] Displays = { "omitted", "summarized" };

    /// <summary>
    /// Reads chat settings from the first present provider key:
    /// <c>amazon-bedrock</c>, then <c>amazonBedrock</c>, then <c>bedrock</c>.
    /// </summary>
    public static AmazonBedrockChatSettings Parse(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var settings = new AmazonBedrockChatSettings();
        if (providerOptions == null)
        {
            return settings;
        }

        JsonElement provider;
        if (!providerOptions.TryGetValue("amazon-bedrock", out provider)
            && !providerOptions.TryGetValue("amazonBedrock", out provider)
            && !providerOptions.TryGetValue("bedrock", out provider))
        {
            return settings;
        }

        if (provider.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Amazon Bedrock provider options must be a JSON object.");
        }

        var passthrough = new JsonObject();
        foreach (var property in provider.EnumerateObject())
        {
            switch (property.Name)
            {
                case "structuredOutputMode":
                    settings.StructuredOutputMode = RequireEnum(property.Value, StructuredOutputModes, "structuredOutputMode");
                    break;
                case "serviceTier":
                    settings.ServiceTier = RequireEnum(property.Value, ServiceTiers, "serviceTier");
                    break;
                case "additionalModelRequestFields":
                    if (property.Value.ValueKind != JsonValueKind.Object)
                    {
                        throw new ArgumentException("additionalModelRequestFields must be a JSON object.");
                    }

                    settings.AdditionalModelRequestFields = property.Value.Clone();
                    break;
                case "reasoningConfig":
                    settings.Reasoning = ParseReasoning(property.Value);
                    break;
                case "anthropicBeta":
                    settings.AnthropicBeta = ReadStringArray(property.Value, "anthropicBeta");
                    break;
                case "requestMetadata":
                    settings.RequestMetadata = ReadRequestMetadata(property.Value);
                    break;
                default:
                    passthrough[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                    break;
            }
        }

        if (passthrough.Count > 0)
        {
            settings.Passthrough = passthrough;
        }

        return settings;
    }

    /// <summary>Returns whether <paramref name="mode"/> is a structured-output mode.</summary>
    public static bool IsStructuredOutputMode(string? mode)
    {
        return Contains(StructuredOutputModes, mode);
    }

    /// <summary>Returns whether <paramref name="tier"/> is a service tier.</summary>
    public static bool IsServiceTier(string? tier)
    {
        return Contains(ServiceTiers, tier);
    }

    private static AmazonBedrockReasoningSettings ParseReasoning(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("reasoningConfig must be a JSON object.");
        }

        var reasoning = new AmazonBedrockReasoningSettings();
        foreach (var property in value.EnumerateObject())
        {
            switch (property.Name)
            {
                case "type":
                    reasoning.Type = RequireEnum(property.Value, ReasoningTypes, "reasoningConfig.type");
                    break;
                case "budgetTokens":
                    if (property.Value.ValueKind != JsonValueKind.Number)
                    {
                        throw new ArgumentException("reasoningConfig.budgetTokens must be a number.");
                    }

                    reasoning.BudgetTokens = property.Value.GetInt32();
                    break;
                case "maxReasoningEffort":
                    reasoning.MaxReasoningEffort = RequireEnum(property.Value, Efforts, "reasoningConfig.maxReasoningEffort");
                    break;
                case "display":
                    reasoning.Display = RequireEnum(property.Value, Displays, "reasoningConfig.display");
                    break;
                default:
                    throw new ArgumentException("Unknown reasoningConfig field '" + property.Name + "'.");
            }
        }

        return reasoning;
    }

    private static JsonObject ReadRequestMetadata(JsonElement metadata)
    {
        if (metadata.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
        }

        var result = new JsonObject();
        foreach (var property in metadata.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
            }

            var text = property.Value.GetString();
            if (text == null)
            {
                throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
            }

            result[property.Name] = text;
        }

        return result;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException(name + " must be an array of strings.");
        }

        var values = new List<string>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException(name + " must be an array of strings.");
            }

            values.Add(item.GetString() ?? string.Empty);
        }

        return values;
    }

    private static string RequireEnum(JsonElement value, string[] allowed, string name)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException(name + " is invalid.");
        }

        var text = value.GetString();
        if (!Contains(allowed, text))
        {
            throw new ArgumentException(name + " is invalid.");
        }

        return text!;
    }

    private static bool Contains(string[] allowed, string? value)
    {
        if (value == null)
        {
            return false;
        }

        foreach (var item in allowed)
        {
            if (item == value)
            {
                return true;
            }
        }

        return false;
    }
}
