// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.HeyGen;

/// <summary>Known HeyGen model ids and the adapter limits. Upstream <c>heygen-model-catalog.ts</c>.</summary>
public static class HeyGenModelCatalog
{
    /// <summary>Known decision model ids. Other ids are passed through unchanged.</summary>
    public static readonly IReadOnlyList<string> DecisionModelIds = ["heygen-decision-1", "heygen-decision-1-turbo"];

    /// <summary>Question types HeyGen Decision supports. Upstream <c>heyGenDecisionModelLimits.supportedQuestionTypes</c>.</summary>
    public static readonly IReadOnlyList<string> DecisionSupportedQuestionTypes = ["choice", "score", "boolean"];

    /// <summary>Most questions per request. Upstream <c>heyGenDecisionModelLimits.maxQuestions</c>.</summary>
    public const int DecisionMaxQuestions = 400;

    /// <summary>Most options of a choice question. Upstream <c>heyGenDecisionModelLimits.maxChoiceOptions</c>.</summary>
    public const int DecisionMaxChoiceOptions = 255;

    /// <summary>Most levels of a score question. Upstream <c>heyGenDecisionModelLimits.maxScoreLevels</c>.</summary>
    public const int DecisionMaxScoreLevels = 10;

    /// <summary>Most images per request. Upstream <c>heyGenDecisionModelLimits.maxImages</c>.</summary>
    public const int DecisionMaxImages = 4;

    /// <summary>Most videos per request. Upstream <c>heyGenDecisionModelLimits.maxVideos</c>.</summary>
    public const int DecisionMaxVideos = 1;
}
