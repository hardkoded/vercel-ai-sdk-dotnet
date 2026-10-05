// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>An evaluation model does not support the type of a requested question.</summary>
public sealed class EvaluationUnsupportedQuestionTypeError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_EvaluationUnsupportedQuestionTypeError";

    /// <summary>Creates the error for the unsupported question.</summary>
    public EvaluationUnsupportedQuestionTypeError(
        string questionId,
        string questionType,
        string provider,
        string modelId,
        string? message = null)
        : base(
            "AI_EvaluationUnsupportedQuestionTypeError",
            message ?? ("Question \"" + questionId + "\" has type \"" + questionType + "\", which is not supported by provider \"" + provider + "\" and model \"" + modelId + "\"."))
    {
        QuestionId = questionId;
        QuestionType = questionType;
        Provider = provider;
        ModelId = modelId;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Question identifier.</summary>
    public string QuestionId { get; }

    /// <summary>Question type the model rejected.</summary>
    public string QuestionType { get; }

    /// <summary>Evaluation provider id.</summary>
    public string Provider { get; }

    /// <summary>Evaluation model id.</summary>
    public string ModelId { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
