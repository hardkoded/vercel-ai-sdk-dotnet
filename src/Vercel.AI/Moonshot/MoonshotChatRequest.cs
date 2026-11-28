// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Moonshot;

/// <summary>
/// Thinking and reasoning effort handling of the Moonshot AI chat request. Maps to the
/// <c>modelFamily</c> switch in <c>moonshotai-chat-language-model.ts</c> <c>getArgs</c>.
/// </summary>
internal static class MoonshotChatRequest
{
    /// <summary>Reasoning efforts supported by the Moonshot AI API. Maps to <c>moonshotReasoningEffortMap</c>.</summary>
    private static readonly IReadOnlyDictionary<string, string> MoonshotReasoningEffortMap =
        new Dictionary<string, string>
        {
            ["low"] = "low",
            ["high"] = "high",
            ["max"] = "max",
        };

    /// <summary>Request fields the shared request owns, which an unknown provider option must not remove.</summary>
    private static readonly HashSet<string> WireFields =
    [
        "model",
        "messages",
        "tools",
        "tool_choice",
        "stop",
        "seed",
        "response_format",
        "stream",
        "stream_options",
        "temperature",
        "top_p",
        "frequency_penalty",
        "presence_penalty",
        "max_completion_tokens",
    ];

    /// <summary>Sets <c>thinking</c> and <c>reasoning_effort</c> on the body from the reasoning option and the provider options.</summary>
    public static JsonObject Apply(
        string modelId,
        LanguageModelCallOptions options,
        JsonObject body,
        IList<CallWarning> warnings
    )
    {
        var moonshot = ParseOptions(options.ProviderOptions);
        if (options.Seed != null)
        {
            body.Remove("seed");
            warnings.Add(new CallWarning("unsupported", "seed", null));
        }

        ApplySamplingOptions(modelId, options, body, warnings);
        ApplyMaxOutputTokens(options, body);
        ApplyOptionFields(moonshot, options.ProviderOptions, body);
        ApplyTools(modelId, options, body, warnings);

        var requestedThinking = moonshot?.Thinking;
        var requestedThinkingType = requestedThinking?.Type;
        var requestedReasoningEffort = moonshot?.ReasoningEffort;
        var preserveReasoning = moonshot?.ReasoningHistory == "preserved";
        var reasoning = options.Reasoning;

        body.Remove("thinking");
        body.Remove("reasoning_effort");

        if (requestedThinking?.BudgetTokens != null)
        {
            warnings.Add(
                CallWarning.Deprecated(
                    "providerOptions.moonshotai.thinking.budgetTokens",
                    "Moonshot Chat Completions does not support budget_tokens. Remove budgetTokens; the option has been omitted."
                )
            );
        }

        string? thinkingType = null;
        var keepAll = false;
        string? reasoningEffort = null;

        void WarnUnsupportedReasoningEffort()
        {
            if (requestedReasoningEffort != null)
            {
                warnings.Add(
                    new CallWarning(
                        "unsupported",
                        "reasoningEffort",
                        $"reasoningEffort is only supported by Kimi K3 and has been omitted for model \"{modelId}\"."
                    )
                );
            }
        }

        void WarnUnsupportedReasoningHistory()
        {
            if (preserveReasoning)
            {
                warnings.Add(
                    new CallWarning(
                        "unsupported",
                        $"reasoningHistory 'preserved' is not supported by model \"{modelId}\"",
                        null
                    )
                );
            }
        }

        switch (MoonshotModelCatalog.ResolveModelFamily(modelId))
        {
            case MoonshotModelFamily.KimiK3:
                if (requestedThinking != null)
                {
                    warnings.Add(
                        new CallWarning(
                            "unsupported",
                            "thinking",
                            "Kimi K3 always reasons and does not accept the thinking field. The option has been omitted."
                        )
                    );
                }

                // Kimi K3 reasoning cannot be disabled, so reasoning 'none' maps to the lowest effort.
                reasoningEffort = MapEffort(reasoning, requestedReasoningEffort, warnings);
                break;
            case MoonshotModelFamily.KimiK27:
                WarnUnsupportedReasoningEffort();
                if (requestedThinkingType == "disabled")
                {
                    warnings.Add(
                        new CallWarning(
                            "unsupported",
                            "thinking.type \"disabled\"",
                            "Kimi K2.7 thinking cannot be disabled."
                        )
                    );
                }
                else if (requestedThinking == null && reasoning == "none")
                {
                    warnings.Add(
                        new CallWarning(
                            "compatibility",
                            "reasoning",
                            "reasoning \"none\" is not supported by Kimi K2.7; thinking cannot be disabled. Using thinking enabled instead."
                        )
                    );
                }
                else if (requestedThinkingType == "enabled")
                {
                    thinkingType = "enabled";
                }

                break;
            case MoonshotModelFamily.KimiK26:
                WarnUnsupportedReasoningEffort();
                thinkingType = requestedThinkingType ?? ThinkingTypeOf(reasoning);
                if (thinkingType != null || preserveReasoning)
                {
                    thinkingType ??= "enabled";
                    keepAll = preserveReasoning;
                }

                break;
            case MoonshotModelFamily.KimiK25:
                WarnUnsupportedReasoningEffort();
                thinkingType = requestedThinkingType ?? ThinkingTypeOf(reasoning);
                WarnUnsupportedReasoningHistory();
                break;
            case MoonshotModelFamily.MoonshotV1:
                WarnUnsupportedReasoningEffort();
                if (requestedThinking != null)
                {
                    warnings.Add(
                        new CallWarning(
                            "unsupported",
                            "thinking",
                            $"thinking is not supported by model \"{modelId}\" and has been omitted."
                        )
                    );
                }

                if (ReasoningMap.IsCustomReasoning(reasoning) && reasoning != "none")
                {
                    warnings.Add(
                        new CallWarning(
                            "unsupported",
                            "reasoning",
                            $"reasoning is not supported by model \"{modelId}\"."
                        )
                    );
                }

                WarnUnsupportedReasoningHistory();
                break;
            default:
                // The capabilities of unknown models are not known, so reasoning 'none' is sent as disabled thinking
                // and the API decides.
                reasoningEffort = requestedReasoningEffort
                    ?? (
                        ReasoningMap.IsCustomReasoning(reasoning) && reasoning != "none"
                            ? MapEffort(reasoning, null, warnings)
                            : null
                    );
                if (requestedThinkingType != null)
                {
                    thinkingType = requestedThinkingType;
                }
                else if (reasoning == "none")
                {
                    thinkingType = "disabled";
                }

                WarnUnsupportedReasoningHistory();
                break;
        }

        if (thinkingType != null)
        {
            var thinking = new JsonObject { ["type"] = thinkingType };
            if (keepAll)
            {
                thinking["keep"] = "all";
            }

            body["thinking"] = thinking;
        }

        if (reasoningEffort != null)
        {
            body["reasoning_effort"] = reasoningEffort;
        }

        return body;
    }

    /// <summary>Omits the sampling options that Kimi models fix and warns. Maps to the <c>supportsSamplingOptions</c> checks in <c>getArgs</c>.</summary>
    private static void ApplySamplingOptions(
        string modelId,
        LanguageModelCallOptions options,
        JsonObject body,
        IList<CallWarning> warnings
    )
    {
        if (!MoonshotModelCatalog.IsMoonshotKimiModel(modelId))
        {
            return;
        }

        void Omit(string feature, bool present, string key)
        {
            if (!present)
            {
                return;
            }

            body.Remove(key);
            warnings.Add(
                new CallWarning(
                    "unsupported",
                    feature,
                    $"{feature} is fixed by model \"{modelId}\" and has been omitted."
                )
            );
        }

        Omit("temperature", options.Temperature != null, "temperature");
        Omit("topP", options.TopP != null, "top_p");
        Omit("frequencyPenalty", options.FrequencyPenalty != null, "frequency_penalty");
        Omit("presencePenalty", options.PresencePenalty != null, "presence_penalty");
    }

    private static string? ThinkingTypeOf(string? reasoning)
    {
        return ReasoningMap.IsCustomReasoning(reasoning)
            ? reasoning == "none"
                ? "disabled"
                : "enabled"
            : null;
    }

    private static string? MapEffort(
        string? reasoning,
        string? providerOptionsEffort,
        IList<CallWarning> warnings
    )
    {
        var mappingWarnings = new List<ModelWarning>();
        var effort = ReasoningMap.MapReasoningToProviderEffort(
            reasoning,
            MoonshotReasoningEffortMap,
            mappingWarnings,
            providerOptionsEffort
        );
        foreach (var warning in mappingWarnings)
        {
            if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
            {
                warnings.Add(callWarning);
            }
        }

        return effort;
    }

    /// <summary>
    /// Sets the fields <c>getArgs</c> builds from <c>moonshotaiLanguageModelOptions</c> and drops the raw provider
    /// options the shared request copied into the body, because the schema strips properties it does not know.
    /// </summary>
    private static void ApplyOptionFields(
        MoonshotAILanguageModelOptions? moonshot,
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        JsonObject body
    )
    {
        var key = OpenAICompatibleChat.ResolveProviderOptionsKey(
            MoonshotProvider.ProviderId,
            providerOptions
        );
        if (
            OpenAICompatibleChat.HasOptions(providerOptions, key)
            && providerOptions![key].ValueKind == JsonValueKind.Object
        )
        {
            foreach (var property in providerOptions[key].EnumerateObject())
            {
                if (!WireFields.Contains(property.Name))
                {
                    body.Remove(property.Name);
                }
            }
        }

        if (moonshot == null)
        {
            return;
        }

        if (moonshot.Logprobs == true || moonshot.TopLogprobs != null)
        {
            body["logprobs"] = true;
        }

        if (moonshot.TopLogprobs is { } topLogprobs)
        {
            body["top_logprobs"] = topLogprobs;
        }

        if (moonshot.Prediction != null)
        {
            body["prediction"] = moonshot.Prediction.DeepClone();
        }

        if (moonshot.PromptCacheKey != null)
        {
            body["prompt_cache_key"] = moonshot.PromptCacheKey;
        }

        if (moonshot.PromptCacheOptions is { } cache)
        {
            var promptCacheOptions = new JsonObject { ["mode"] = cache.Mode ?? "implicit" };
            if (cache.Ttl != null)
            {
                promptCacheOptions["ttl"] = cache.Ttl;
            }

            body["prompt_cache_options"] = promptCacheOptions;
        }

        if (moonshot.SafetyIdentifier != null)
        {
            body["safety_identifier"] = moonshot.SafetyIdentifier;
        }
    }

    /// <summary>Sends <c>maxOutputTokens</c> as <c>max_completion_tokens</c>, never <c>max_tokens</c>.</summary>
    private static void ApplyMaxOutputTokens(LanguageModelCallOptions options, JsonObject body)
    {
        body.Remove("max_tokens");
        if (options.MaxOutputTokens is { } maxOutputTokens)
        {
            body["max_completion_tokens"] = maxOutputTokens;
        }
    }

    /// <summary>Sends <c>tools</c> and <c>tool_choice</c> from <see cref="MoonshotPrepareTools.PrepareTools"/>. Maps to the <c>prepareTools</c> call in <c>getArgs</c>.</summary>
    private static void ApplyTools(
        string modelId,
        LanguageModelCallOptions options,
        JsonObject body,
        IList<CallWarning> warnings
    )
    {
        var prepared = MoonshotPrepareTools.PrepareTools(
            modelId,
            options.Tools,
            options.ToolChoice
        );
        foreach (var warning in prepared.ToolWarnings)
        {
            warnings.Add(warning);
        }

        if (prepared.Tools != null)
        {
            body["tools"] = prepared.Tools;
        }

        if (prepared.ToolChoice != null)
        {
            body["tool_choice"] = prepared.ToolChoice;
        }
    }

    /// <summary>Validates the <c>moonshotai</c> provider options. Maps to <c>parseProviderOptions</c> in <c>getArgs</c>.</summary>
    private static MoonshotAILanguageModelOptions? ParseOptions(
        IReadOnlyDictionary<string, JsonElement>? providerOptions
    )
    {
        var key = OpenAICompatibleChat.ResolveProviderOptionsKey(
            MoonshotProvider.ProviderId,
            providerOptions
        );
        return MoonshotChatOptions.ParseLanguageModelOptions(providerOptions, key);
    }
}
