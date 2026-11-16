// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.Google;

/// <summary>Gemini Live Translation configuration. Maps to <c>translationConfig</c> of <c>GoogleRealtimeModelOptions</c>.</summary>
public sealed class GoogleRealtimeTranslationConfig
{
    /// <summary>BCP-47 language code of the language to translate into. Defaults to <c>en</c> in the Gemini API.</summary>
    public string? TargetLanguageCode { get; set; }

    /// <summary>Whether input audio already in the target language should be echoed instead of producing silence.</summary>
    public bool? EchoTargetLanguage { get; set; }
}

/// <summary>Gemini Live thinking configuration. Maps to <c>thinkingConfig</c> of <c>GoogleRealtimeModelOptions</c>.</summary>
public sealed class GoogleRealtimeThinkingConfig
{
    /// <summary>
    /// Thinking effort level: <c>low</c>, <c>medium</c> or <c>high</c>. Background-reasoning Live models require exactly
    /// one of <see cref="ThinkingLevel"/> or <see cref="ThinkingBudget"/>; when neither is set, the provider sends
    /// <c>thinkingLevel: "low"</c> on those models.
    /// </summary>
    public string? ThinkingLevel { get; set; }

    /// <summary>Token budget for background thinking. Mutually exclusive with <see cref="ThinkingLevel"/>.</summary>
    public int? ThinkingBudget { get; set; }

    /// <summary>Whether thought summaries should be included in the response.</summary>
    public bool? IncludeThoughts { get; set; }
}

/// <summary>
/// Google realtime provider options, read from <c>providerOptions.google</c>. Maps to
/// <c>GoogleRealtimeModelOptions</c> of <c>google-realtime-model-options.ts</c>. Put <see cref="ToJsonObject"/> under the
/// <c>google</c> key of <see cref="Provider.RealtimeModelV4SessionConfig.ProviderOptions"/>.
/// </summary>
public sealed class GoogleRealtimeModelOptions
{
    /// <summary>
    /// Gemini Live Translation configuration. Required for <c>gemini-3.5-live-translate-preview</c> when translating
    /// speech to a target language.
    /// </summary>
    public GoogleRealtimeTranslationConfig? TranslationConfig { get; set; }

    /// <summary>
    /// Gemini Live thinking configuration. Supported by Live models with background reasoning (for example
    /// <c>gemini-3.8-live-extended-thinking</c>). Not supported by latency-optimized models (for example <c>gemini-3.8-live</c>).
    /// </summary>
    public GoogleRealtimeThinkingConfig? ThinkingConfig { get; set; }

    /// <summary>
    /// Default <c>behavior</c> stamped onto every function declaration in the session setup: <c>BLOCKING</c> or
    /// <c>NON_BLOCKING</c>. Background-reasoning models accept only <c>NON_BLOCKING</c>.
    /// </summary>
    public string? DefaultToolBehavior { get; set; }

    /// <summary>Converts the options to the JSON shape the session builder reads from <c>providerOptions.google</c>.</summary>
    public JsonObject ToJsonObject()
    {
        var result = new JsonObject();
        if (TranslationConfig is { } translation)
        {
            var node = new JsonObject();
            if (translation.TargetLanguageCode != null)
            {
                node["targetLanguageCode"] = translation.TargetLanguageCode;
            }

            if (translation.EchoTargetLanguage is { } echo)
            {
                node["echoTargetLanguage"] = echo;
            }

            result["translationConfig"] = node;
        }

        if (ThinkingConfig is { } thinking)
        {
            var node = new JsonObject();
            if (thinking.ThinkingLevel != null)
            {
                node["thinkingLevel"] = thinking.ThinkingLevel;
            }

            if (thinking.ThinkingBudget is { } budget)
            {
                node["thinkingBudget"] = budget;
            }

            if (thinking.IncludeThoughts is { } include)
            {
                node["includeThoughts"] = include;
            }

            result["thinkingConfig"] = node;
        }

        if (DefaultToolBehavior != null)
        {
            result["defaultToolBehavior"] = DefaultToolBehavior;
        }

        return result;
    }
}
