// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>A decision model does not support the type of a requested question.</summary>
public class DecisionUnsupportedQuestionTypeError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_DecisionUnsupportedQuestionTypeError";

    /// <summary>Creates the error for the unsupported question.</summary>
    public DecisionUnsupportedQuestionTypeError(
        string questionId,
        string questionType,
        string provider,
        string modelId,
        string? message = null)
        : base(
            "AI_DecisionUnsupportedQuestionTypeError",
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

    /// <summary>Decision provider id.</summary>
    public string Provider { get; }

    /// <summary>Decision model id.</summary>
    public string ModelId { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}

/// <summary>Deprecated alias of <see cref="DecisionUnsupportedQuestionTypeError"/>. It carries the decision error marker.</summary>
[Obsolete("Use DecisionUnsupportedQuestionTypeError instead.")]
public sealed class EvaluationUnsupportedQuestionTypeError : DecisionUnsupportedQuestionTypeError
{
    /// <summary>Marker. It equals <see cref="DecisionUnsupportedQuestionTypeError.ErrorMarker"/>.</summary>
    public new const string ErrorMarker = DecisionUnsupportedQuestionTypeError.ErrorMarker;

    /// <summary>Creates the error for the unsupported question.</summary>
    public EvaluationUnsupportedQuestionTypeError(
        string questionId,
        string questionType,
        string provider,
        string modelId,
        string? message = null)
        : base(questionId, questionType, provider, modelId, message)
    {
    }
}
