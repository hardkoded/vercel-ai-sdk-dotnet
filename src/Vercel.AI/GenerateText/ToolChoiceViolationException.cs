// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>The model response did not satisfy <c>toolChoice</c>. Maps to <c>ToolChoiceViolationError</c>.</summary>
public sealed class ToolChoiceViolationException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public ToolChoiceViolationException(
        string message,
        ToolChoice toolChoice,
        FinishReason finishReason,
        string provider,
        string modelId,
        IReadOnlyList<GeneratedContent> content)
        : base(message)
    {
        ToolChoice = toolChoice ?? throw new ArgumentNullException(nameof(toolChoice));
        FinishReason = finishReason;
        Provider = provider ?? string.Empty;
        ModelId = modelId ?? string.Empty;
        Content = content ?? Array.Empty<GeneratedContent>();
    }

    /// <summary>Stable error name.</summary>
    public string Name { get; } = "AI_ToolChoiceViolationError";

    /// <summary>Tool choice that was violated.</summary>
    public ToolChoice ToolChoice { get; }

    /// <summary>Finish reason from the model call.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Model provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Content from the rejected model call.</summary>
    public IReadOnlyList<GeneratedContent> Content { get; }
}
