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

    /// <summary>Sets <c>thinking</c> and <c>reasoning_effort</c> on the body from the reasoning option and the provider options.</summary>
    public static JsonObject Apply(
        string modelId,
        LanguageModelCallOptions options,
        JsonObject body,
        IList<CallWarning> warnings
    )
    {
        ApplySamplingOptions(modelId, options, body, warnings);
        ApplyMaxOutputTokens(body);
        ApplyTools(modelId, options, body, warnings);
        ApplyPromptCacheOptions(body);

        var moonshot = ReadOptions(options.ProviderOptions);
        var requestedThinking = moonshot.TryGetPropertyValue("thinking", out var thinkingNode)
            ? thinkingNode as JsonObject
            : null;
        var requestedThinkingType = StringOf(requestedThinking, "type");
        var requestedReasoningEffort = StringOf(moonshot, "reasoningEffort");
        var preserveReasoning = StringOf(moonshot, "reasoningHistory") == "preserved";
        var reasoning = options.Reasoning;

        body.Remove("thinking");
        body.Remove("reasoningHistory");
        body.Remove("reasoning_effort");

        if (requestedThinking?["budgetTokens"] != null)
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
    /// Sends <c>providerOptions.moonshotai.promptCacheOptions</c> as <c>prompt_cache_options</c>. The mode defaults to
    /// <c>implicit</c>. Maps to the <c>prompt_cache_options</c> field in <c>getArgs</c>.
    /// </summary>
    private static void ApplyPromptCacheOptions(JsonObject body)
    {
        var node = body["promptCacheOptions"];
        body.Remove("promptCacheOptions");
        if (node is not JsonObject cache)
        {
            return;
        }

        var options = new JsonObject { ["mode"] = StringOf(cache, "mode") ?? "implicit" };
        if (StringOf(cache, "ttl") is { } ttl)
        {
            options["ttl"] = ttl;
        }

        body["prompt_cache_options"] = options;
    }

    /// <summary>Sends <c>maxOutputTokens</c> as <c>max_completion_tokens</c>, never <c>max_tokens</c>.</summary>
    private static void ApplyMaxOutputTokens(JsonObject body)
    {
        if (body["max_tokens"] is { } maxTokens)
        {
            body.Remove("max_tokens");
            body["max_completion_tokens"] = maxTokens;
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

    private static JsonObject ReadOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var key = OpenAICompatibleChat.ResolveProviderOptionsKey(
            MoonshotProvider.ProviderId,
            providerOptions
        );
        if (
            OpenAICompatibleChat.HasOptions(providerOptions, key)
            && providerOptions![key].ValueKind == JsonValueKind.Object
            && JsonNode.Parse(providerOptions[key].GetRawText()) is JsonObject bag
        )
        {
            return bag;
        }

        return new JsonObject();
    }

    private static string? StringOf(JsonObject? node, string name)
    {
        return
            node != null
            && node.TryGetPropertyValue(name, out var value)
            && value is JsonValue json
            && json.TryGetValue<string>(out var text)
            ? text
            : null;
    }
}
