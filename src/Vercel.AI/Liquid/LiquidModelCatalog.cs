// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Liquid;

/// <summary>Known Liquid model ids, the adapter limits and the capability catalog. Upstream <c>liquid-model-catalog.ts</c>.</summary>
public static class LiquidModelCatalog
{
    /// <summary>Known decision model ids. Other ids are passed through unchanged. Upstream <c>decisionModelIds</c>.</summary>
    public static readonly IReadOnlyList<string> DecisionModelIds = ["d1.2", "d1.2-ultra"];

    /// <summary>Question types Liquid Decisions supports. Upstream <c>liquidDecisionModelLimits.supportedQuestionTypes</c>.</summary>
    public static readonly IReadOnlyList<string> DecisionSupportedQuestionTypes =
    [
        "choice",
        "score",
        "boolean",
        "box",
    ];

    /// <summary>Most questions per request. Upstream <c>liquidDecisionModelLimits.maxQuestions</c>.</summary>
    public const int DecisionMaxQuestions = 128;

    /// <summary>Fewest options of a choice question. Upstream <c>liquidDecisionModelLimits.minChoiceOptions</c>.</summary>
    public const int DecisionMinChoiceOptions = 2;

    /// <summary>Most options of a choice question. Upstream <c>liquidDecisionModelLimits.maxChoiceOptions</c>.</summary>
    public const int DecisionMaxChoiceOptions = 255;

    /// <summary>Fewest levels of a score question. Upstream <c>liquidDecisionModelLimits.minScoreLevels</c>.</summary>
    public const int DecisionMinScoreLevels = 2;

    /// <summary>Most levels of a score question. Upstream <c>liquidDecisionModelLimits.maxScoreLevels</c>.</summary>
    public const int DecisionMaxScoreLevels = 10;

    /// <summary>Most images per request. Upstream <c>liquidDecisionModelLimits.maxImages</c>.</summary>
    public const int DecisionMaxImages = 8;

    /// <summary>Most audio clips per request. Upstream <c>liquidDecisionModelLimits.maxAudioClips</c>.</summary>
    public const int DecisionMaxAudioClips = 8;

    /// <summary>
    /// Entries of every known model id. Upstream <c>liquidModelCatalog</c>; renamed because C# does not allow a member named like its class.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, ModelEntry<object>> Entries = BuildEntries();

    /// <summary>Returns the catalog entry of a decision model. Upstream <c>getLiquidDecisionModelEntry</c>.</summary>
    public static ModelEntry<object> GetLiquidDecisionModelEntry(string modelId)
    {
        return new ModelEntry<object>
        {
            Capabilities = new ModelCapabilities
            {
                Decision = new DecisionCapabilities
                {
                    QuestionTypes = new DecisionQuestionTypeCapabilities
                    {
                        Boolean = ModelCapability.Supported,
                        Choice = ModelCapability.Supported,
                        Score = ModelCapability.Supported,
                        Box = ModelCapability.Supported,
                    },
                    Inputs = new DecisionInputCapabilities { Image = ModelCapability.Supported },
                    Reasoning = ModelCapability.Unsupported,
                    MaxOutputTokens = ModelCapability.Unsupported,
                },
            },
        };
    }

    private static Dictionary<string, ModelEntry<object>> BuildEntries()
    {
        var entries = new Dictionary<string, ModelEntry<object>>(StringComparer.Ordinal);
        foreach (var id in DecisionModelIds)
        {
            entries[id] = GetLiquidDecisionModelEntry(id);
        }

        return entries;
    }
}
