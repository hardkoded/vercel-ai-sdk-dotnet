// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class EvaluationUnsupportedQuestionTypeExceptionTests
{
    private const string QuestionId = "requestsRefund";
    private const string QuestionType = "boolean";
    private const string ProviderId = "test.evaluation";
    private const string ModelId = "test-model";
    private const string Marker = "vercel.ai.error.AI_EvaluationUnsupportedQuestionTypeError";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/error/evaluation-unsupported-question-type-error.test.ts::EvaluationUnsupportedQuestionTypeError::identifies the unsupported question and model",
        Coverage = UpstreamCoverage.Covered)]
    public void Identifies_the_unsupported_question_and_model()
    {
        var error = new EvaluationUnsupportedQuestionTypeException(QuestionId, QuestionType, ProviderId, ModelId);

        Assert.IsAssignableFrom<Exception>(error);
        Assert.True(AiSdkException.IsInstance(error));
        Assert.True(EvaluationUnsupportedQuestionTypeException.IsInstance(error));
        Assert.Equal(QuestionId, error.QuestionId);
        Assert.Equal(QuestionType, error.QuestionType);
        Assert.Equal(ProviderId, error.Provider);
        Assert.Equal(ModelId, error.ModelId);
        Assert.Equal("AI_EvaluationUnsupportedQuestionTypeError", error.Name);
        Assert.Equal(
            "Question \"requestsRefund\" has type \"boolean\", which is not supported by provider \"test.evaluation\" and model \"test-model\".",
            error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/error/evaluation-unsupported-question-type-error.test.ts::EvaluationUnsupportedQuestionTypeError::accepts a provider-specific message",
        Coverage = UpstreamCoverage.Covered)]
    public void Accepts_a_provider_specific_message()
    {
        var error = new EvaluationUnsupportedQuestionTypeException(
            QuestionId,
            QuestionType,
            ProviderId,
            ModelId,
            "This model cannot return a probability for a boolean question.");

        Assert.Equal("This model cannot return a probability for a boolean question.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/error/evaluation-unsupported-question-type-error.test.ts::EvaluationUnsupportedQuestionTypeError::recognizes the marker across package copies",
        Coverage = UpstreamCoverage.Covered)]
    public void Recognizes_the_marker_across_package_copies()
    {
        var foreign = new Dictionary<string, object>
        {
            [Marker] = true,
        };

        Assert.True(EvaluationUnsupportedQuestionTypeException.IsInstance(foreign));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/error/evaluation-unsupported-question-type-error.test.ts::EvaluationUnsupportedQuestionTypeError::rejects values without the marker: %s",
        Coverage = UpstreamCoverage.Covered,
        Note = "JavaScript undefined is not a separate .NET value. Null is rejected by the same check.")]
    public void Rejects_values_without_the_marker()
    {
        Assert.False(EvaluationUnsupportedQuestionTypeException.IsInstance(null));
        Assert.False(EvaluationUnsupportedQuestionTypeException.IsInstance("error"));
        Assert.False(EvaluationUnsupportedQuestionTypeException.IsInstance(new Exception("Unrelated error")));
        Assert.False(EvaluationUnsupportedQuestionTypeException.IsInstance(new Dictionary<string, object>
        {
            ["name"] = "AI_EvaluationUnsupportedQuestionTypeError",
        }));
        Assert.False(EvaluationUnsupportedQuestionTypeException.IsInstance(new Dictionary<string, object>
        {
            [Marker] = false,
        }));
    }
}
