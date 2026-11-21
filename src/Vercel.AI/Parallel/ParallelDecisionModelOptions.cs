// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Parallel;

/// <summary>Provider options of a Parallel decision model, under <c>providerOptions.parallel</c>. Maps to <c>ParallelDecisionModelOptions</c>.</summary>
public sealed class ParallelDecisionModelOptions
{
    /// <summary>Include evidence excerpts in citations. Defaults to true.</summary>
    public bool? IncludeExcerpts { get; init; }
}

/// <summary>Known decision model ids. Maps to <c>ParallelDecisionModelId</c>. Any other string is accepted.</summary>
public static class ParallelDecisionModelId
{
    /// <summary>The fast decision model.</summary>
    public const string Fast = "fast";
}

/// <summary>One piece of shared evidence of a Parallel decision.</summary>
public sealed class ParallelCitation
{
    /// <summary>Source URL.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Page title, when Parallel supplies one.</summary>
    public string? Title { get; init; }

    /// <summary>Evidence excerpts.</summary>
    public IReadOnlyList<string> Excerpts { get; init; } = [];
}

/// <summary>Provider metadata of a Parallel decision, under <c>providerMetadata.parallel</c>. Maps to <c>ParallelDecisionProviderMetadata</c>.</summary>
public sealed class ParallelDecisionProviderMetadata
{
    /// <summary>Shared evidence for the whole request, without a per-answer mapping.</summary>
    public IReadOnlyList<ParallelCitation> Citations { get; init; } = [];

    /// <summary>Confidence per question id. Boolean and refused questions have none.</summary>
    public IReadOnlyDictionary<string, double> Confidence { get; init; } =
        new Dictionary<string, double>();

    /// <summary>Decision id.</summary>
    public string DecisionId { get; init; } = string.Empty;

    /// <summary>Score legend per question id.</summary>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Legend { get; init; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>();

    /// <summary>Mode that answered.</summary>
    public string Mode { get; init; } = string.Empty;
}
