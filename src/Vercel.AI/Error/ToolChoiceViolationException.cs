// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using FinishReasonValue = Vercel.AI.Provider.FinishReason;
using ToolChoiceValue = Vercel.AI.Provider.ToolChoice;

namespace Vercel.AI.Error;

/// <summary>A model response did not satisfy an enforced tool choice.</summary>
public sealed class ToolChoiceViolationException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_ToolChoiceViolationError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_ToolChoiceViolationError";

    /// <summary>Creates the exception. <paramref name="toolChoice"/> is required or a named tool.</summary>
    public ToolChoiceViolationException(
        ToolChoiceValue toolChoice,
        FinishReasonValue finishReason,
        string provider,
        string modelId,
        IReadOnlyList<GeneratedContent> content,
        string? message = null)
        : base(ErrorName, message ?? DefaultMessage(toolChoice), null)
    {
        SetMarker(TypeMarker);
        ToolChoice = toolChoice;
        FinishReason = finishReason;
        Provider = provider;
        ModelId = modelId;
        Content = content;
    }

    /// <summary>Tool choice that the response did not satisfy.</summary>
    public ToolChoiceValue ToolChoice { get; }

    /// <summary>Why the model stopped.</summary>
    public FinishReasonValue FinishReason { get; }

    /// <summary>Provider that returned the response.</summary>
    public string Provider { get; }

    /// <summary>Model that returned the response.</summary>
    public string ModelId { get; }

    /// <summary>Normalized content returned by the model.</summary>
    public IReadOnlyList<GeneratedContent> Content { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    private static string DefaultMessage(ToolChoiceValue toolChoice)
    {
        if (toolChoice.Type == "required")
        {
            return "Model response did not contain a tool call even though tool choice was required.";
        }

        var toolName = toolChoice is ToolChoiceValue.NamedChoice named ? named.ToolName : string.Empty;
        return "Model response did not contain a call to the required tool '" + toolName + "'.";
    }
}
