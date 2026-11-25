// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.QuiverAI;

/// <summary>Reasoning effort. Maps to the <c>reasoningEffort</c> union of <c>QuiverAILanguageModelOptions</c>.</summary>
public enum QuiverAIReasoningEffort
{
    /// <summary><c>low</c>.</summary>
    Low,

    /// <summary><c>medium</c>.</summary>
    Medium,

    /// <summary><c>high</c>.</summary>
    High,

    /// <summary><c>xhigh</c>.</summary>
    Xhigh,
}

/// <summary>Reasoning summary. Maps to the <c>reasoningSummary</c> union of <c>QuiverAILanguageModelOptions</c>.</summary>
public enum QuiverAIReasoningSummary
{
    /// <summary><c>auto</c>.</summary>
    Auto,
}

/// <summary>The <c>quiverai</c> provider options of a language model call. Maps to <c>QuiverAILanguageModelOptions</c>.</summary>
public sealed class QuiverAILanguageModelOptions
{
    /// <summary>Controls the amount of reasoning used by the model.</summary>
    public QuiverAIReasoningEffort? ReasoningEffort { get; set; }

    /// <summary>Requests the model's safe reasoning summary.</summary>
    public QuiverAIReasoningSummary? ReasoningSummary { get; set; }

    /// <summary>
    /// Returns the options as call provider options under <paramref name="providerOptionsName"/>
    /// (<c>quiverai</c> by default), ready for <c>LanguageModelCallOptions.ProviderOptions</c>.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(
        string providerOptionsName = "quiverai"
    )
    {
        var json = new JsonObject();
        if (ReasoningEffort is { } effort)
        {
            json["reasoningEffort"] = effort.ToString().ToLowerInvariant();
        }

        if (ReasoningSummary is { } summary)
        {
            json["reasoningSummary"] = summary.ToString().ToLowerInvariant();
        }

        return new Dictionary<string, JsonElement>
        {
            [providerOptionsName] = JsonSerializer.SerializeToElement(json),
        };
    }
}
