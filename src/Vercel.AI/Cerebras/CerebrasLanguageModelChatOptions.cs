// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;

namespace Vercel.AI.Cerebras;

/// <summary>Request processing tier. Maps to the <c>serviceTier</c> enum of <c>CerebrasLanguageModelChatOptions</c>.</summary>
public enum CerebrasServiceTier
{
    /// <summary><c>auto</c>.</summary>
    Auto,

    /// <summary><c>default</c>.</summary>
    Default,

    /// <summary><c>flex</c>.</summary>
    Flex,

    /// <summary><c>priority</c>.</summary>
    Priority,
}

/// <summary>Reasoning effort. Maps to the <c>reasoningEffort</c> enum of <c>CerebrasLanguageModelChatOptions</c>.</summary>
public enum CerebrasReasoningEffort
{
    /// <summary><c>none</c>.</summary>
    None,

    /// <summary><c>low</c>.</summary>
    Low,

    /// <summary><c>medium</c>.</summary>
    Medium,

    /// <summary><c>high</c>.</summary>
    High,
}

/// <summary>Reasoning format. Maps to the <c>reasoningFormat</c> enum of <c>CerebrasLanguageModelChatOptions</c>.</summary>
public enum CerebrasReasoningFormat
{
    /// <summary><c>none</c>.</summary>
    None,

    /// <summary><c>parsed</c>.</summary>
    Parsed,

    /// <summary><c>text_parsed</c>.</summary>
    TextParsed,

    /// <summary><c>raw</c>.</summary>
    Raw,

    /// <summary><c>hidden</c>.</summary>
    Hidden,
}

/// <summary>Predicted output content. Maps to the <c>prediction</c> option of <c>CerebrasLanguageModelChatOptions</c>.</summary>
public sealed class CerebrasPrediction
{
    /// <summary>The predicted content as one string. Ignored when <see cref="ContentParts"/> is set.</summary>
    public string? Content { get; set; }

    /// <summary>The predicted content as text parts.</summary>
    public IReadOnlyList<string>? ContentParts { get; set; }
}

/// <summary>The <c>cerebras</c> provider options of a chat call. Maps to <c>CerebrasLanguageModelChatOptions</c>.</summary>
public sealed class CerebrasLanguageModelChatOptions
{
    /// <summary>End-user identifier for abuse monitoring.</summary>
    public string? User { get; set; }

    /// <summary>Use strict JSON schema validation.</summary>
    public bool? StrictJsonSchema { get; set; }

    /// <summary>Allow parallel function calls.</summary>
    public bool? ParallelToolCalls { get; set; }

    /// <summary>Return token log probabilities.</summary>
    public bool? Logprobs { get; set; }

    /// <summary>Number of token log probabilities to return (0 to 20).</summary>
    public int? TopLogprobs { get; set; }

    /// <summary>Bias token selection by token ID (each bias from -100 to 100).</summary>
    public IReadOnlyDictionary<string, double>? LogitBias { get; set; }

    /// <summary>Request processing tier.</summary>
    public CerebrasServiceTier? ServiceTier { get; set; }

    /// <summary>Maximum queue time in milliseconds for flex/auto requests (50 to 20000).</summary>
    public int? QueueThreshold { get; set; }

    /// <summary>Reasoning effort for configurable models.</summary>
    public CerebrasReasoningEffort? ReasoningEffort { get; set; }

    /// <summary>Format of the returned reasoning.</summary>
    public CerebrasReasoningFormat? ReasoningFormat { get; set; }

    /// <summary>Predicted output content.</summary>
    public CerebrasPrediction? Prediction { get; set; }

    /// <summary>Cache affinity key for shared prompts (at most 1024 characters).</summary>
    public string? PromptCacheKey { get; set; }

    /// <summary>
    /// Validates the options and returns them as call provider options under <paramref name="providerOptionsName"/>
    /// (<c>cerebras</c> by default), ready for <c>LanguageModelCallOptions.ProviderOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong value.</exception>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(
        string providerOptionsName = CerebrasProvider.ProviderId
    )
    {
        Check(TopLogprobs is null or (>= 0 and <= 20), "topLogprobs", "must be between 0 and 20.");
        Check(
            QueueThreshold is null or (>= 50 and <= 20000),
            "queueThreshold",
            "must be between 50 and 20000."
        );
        Check(
            PromptCacheKey is null || PromptCacheKey.Length <= 1024,
            "promptCacheKey",
            "must be at most 1024 characters."
        );
        if (LogitBias is not null)
        {
            foreach (var bias in LogitBias)
            {
                Check(
                    bias.Value is >= -100 and <= 100,
                    "logitBias",
                    "biases must be between -100 and 100."
                );
            }
        }

        var json = new JsonObject();
        Set(json, "user", User);
        Set(json, "strictJsonSchema", StrictJsonSchema);
        Set(json, "parallelToolCalls", ParallelToolCalls);
        Set(json, "logprobs", Logprobs);
        Set(json, "topLogprobs", TopLogprobs);
        if (LogitBias is not null)
        {
            var bias = new JsonObject();
            foreach (var pair in LogitBias)
            {
                bias[pair.Key] = pair.Value;
            }

            json["logitBias"] = bias;
        }

        Set(json, "serviceTier", ServiceTier?.ToWireValue());
        Set(json, "queueThreshold", QueueThreshold);
        Set(json, "reasoningEffort", ReasoningEffort?.ToWireValue());
        Set(json, "reasoningFormat", ReasoningFormat?.ToWireValue());
        if (Prediction is not null)
        {
            JsonNode content;
            if (Prediction.ContentParts is not null)
            {
                var parts = new JsonArray();
                foreach (var text in Prediction.ContentParts)
                {
                    parts.Add(new JsonObject { ["type"] = "text", ["text"] = text });
                }

                content = parts;
            }
            else
            {
                content = Prediction.Content ?? string.Empty;
            }

            json["prediction"] = new JsonObject { ["type"] = "content", ["content"] = content };
        }

        Set(json, "promptCacheKey", PromptCacheKey);
        return new Dictionary<string, JsonElement>
        {
            [providerOptionsName] = JsonSerializer.SerializeToElement(json),
        };
    }

    private static void Check(bool valid, string name, string detail)
    {
        if (!valid)
        {
            throw new InvalidArgumentException("providerOptions", name, name + " " + detail);
        }
    }

    private static void Set<T>(JsonObject json, string name, T? value)
    {
        if (value is not null)
        {
            json[name] = JsonValue.Create(value);
        }
    }
}

internal static class CerebrasOptionEnumExtensions
{
    internal static string ToWireValue(this CerebrasServiceTier value) =>
        value.ToString().ToLowerInvariant();

    internal static string ToWireValue(this CerebrasReasoningEffort value) =>
        value.ToString().ToLowerInvariant();

    internal static string ToWireValue(this CerebrasReasoningFormat value) =>
        value == CerebrasReasoningFormat.TextParsed
            ? "text_parsed"
            : value.ToString().ToLowerInvariant();
}
