// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class ErrorTests
{
    [UpstreamTest("packages/ai/src/error/evaluation-unsupported-question-type-error.test.ts::EvaluationUnsupportedQuestionTypeError::identifies the unsupported question and model", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Evaluation_error_identifies_the_question()
    {
        var error = new EvaluationUnsupportedQuestionTypeError("requestsRefund", "boolean", "test.evaluation", "test-model");
        Assert.IsAssignableFrom<Exception>(error);
        Assert.True(AiSdkError.IsInstance(error));
        Assert.True(EvaluationUnsupportedQuestionTypeError.IsInstance(error));
        Assert.Equal("AI_EvaluationUnsupportedQuestionTypeError", error.ErrorName);
        Assert.Equal("requestsRefund", error.QuestionId);
        Assert.Equal("boolean", error.QuestionType);
        Assert.Equal("test.evaluation", error.Provider);
        Assert.Equal("test-model", error.ModelId);
        Assert.Equal("Question \"requestsRefund\" has type \"boolean\", which is not supported by provider \"test.evaluation\" and model \"test-model\".", error.Message);
    }

    [UpstreamTest("packages/ai/src/error/evaluation-unsupported-question-type-error.test.ts::EvaluationUnsupportedQuestionTypeError::accepts a provider-specific message", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Evaluation_error_accepts_a_custom_message()
    {
        var error = new EvaluationUnsupportedQuestionTypeError(
            "requestsRefund",
            "boolean",
            "test.evaluation",
            "test-model",
            "This model cannot return a probability for a boolean question.");
        Assert.Equal("This model cannot return a probability for a boolean question.", error.Message);
    }

    [UpstreamTest("packages/ai/src/error/evaluation-unsupported-question-type-error.test.ts::EvaluationUnsupportedQuestionTypeError::recognizes the marker across package copies", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Evaluation_error_recognizes_a_copied_marker()
    {
        var copy = new ErrorMarkerBag(EvaluationUnsupportedQuestionTypeError.ErrorMarker, true);
        Assert.True(EvaluationUnsupportedQuestionTypeError.IsInstance(copy));
    }

    [UpstreamTest("packages/ai/src/error/evaluation-unsupported-question-type-error.test.ts::EvaluationUnsupportedQuestionTypeError::rejects values without the marker: %s", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Evaluation_error_rejects_values_without_the_marker()
    {
        Assert.False(EvaluationUnsupportedQuestionTypeError.IsInstance(null));
        Assert.False(EvaluationUnsupportedQuestionTypeError.IsInstance(JsUndefined.Value));
        Assert.False(EvaluationUnsupportedQuestionTypeError.IsInstance("error"));
        Assert.False(EvaluationUnsupportedQuestionTypeError.IsInstance(new Exception("Unrelated error")));
        Assert.False(EvaluationUnsupportedQuestionTypeError.IsInstance(new ErrorMarkerBag()));
        Assert.False(EvaluationUnsupportedQuestionTypeError.IsInstance(new ErrorMarkerBag(EvaluationUnsupportedQuestionTypeError.ErrorMarker, false)));
    }

    [UpstreamTest("packages/ai/src/error/stream-provider-error.test.ts::StreamProviderError::exposes provider metadata and preserves the raw payload", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Stream_provider_error_keeps_the_payload()
    {
        var data = new JsonObject
        {
            ["message"] = "Overloaded",
            ["type"] = "overloaded_error",
            ["code"] = "provider_overloaded",
        };
        var error = new StreamProviderError("Overloaded", "overloaded_error", "provider_overloaded", 529, data: data);
        Assert.IsAssignableFrom<Exception>(error);
        Assert.Equal("Overloaded", error.Message);
        Assert.Equal("overloaded_error", error.Type);
        Assert.Equal("provider_overloaded", error.Code);
        Assert.Equal(529, error.StatusCode);
        Assert.True(error.IsRetryable);
        Assert.Same(data, error.Data);
        Assert.True(StreamProviderError.IsInstance(error));
    }

    [UpstreamTest("packages/ai/src/error/stream-provider-error.test.ts::StreamProviderError::uses an explicit retryability value", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Stream_provider_error_honors_explicit_retryability()
    {
        var error = new StreamProviderError("Do not retry", statusCode: 503, isRetryable: false);
        Assert.False(error.IsRetryable);
    }

    [UpstreamTest("packages/ai/src/error/stream-provider-error.test.ts::StreamProviderError::supports marker-based identification across package copies", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Stream_provider_error_recognizes_a_copied_marker()
    {
        Assert.True(StreamProviderError.IsInstance(new ErrorMarkerBag(StreamProviderError.ErrorMarker, true)));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > null and undefined::should return \"unknown error\" for null", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Unknown_error_for_null()
    {
        Assert.Equal("unknown error", ErrorMessage.GetErrorMessage(null));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > null and undefined::should return \"unknown error\" for undefined", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Unknown_error_for_undefined()
    {
        Assert.Equal("unknown error", ErrorMessage.GetErrorMessage(JsUndefined.Value));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > string errors::should return the string as-is", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_a_string_error()
    {
        Assert.Equal("something went wrong", ErrorMessage.GetErrorMessage("something went wrong"));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > string errors::should return an empty string as-is", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_an_empty_string()
    {
        Assert.Equal(string.Empty, ErrorMessage.GetErrorMessage(string.Empty));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should include the Error type prefix for a basic Error", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Formats_a_basic_error()
    {
        Assert.Equal("Error: API crashed", ErrorMessage.GetErrorMessage(new JsError("API crashed")));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should include the TypeError prefix", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Formats_a_type_error()
    {
        Assert.Equal("TypeError: invalid argument", ErrorMessage.GetErrorMessage(new JsTypeError("invalid argument")));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should include the RangeError prefix", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Formats_a_range_error()
    {
        Assert.Equal("RangeError: out of bounds", ErrorMessage.GetErrorMessage(new JsRangeError("out of bounds")));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should return just the error name when message is empty", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Formats_an_empty_error()
    {
        Assert.Equal("Error", ErrorMessage.GetErrorMessage(new JsError(string.Empty)));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should return just the type name when TypeError message is empty", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Formats_an_empty_type_error()
    {
        Assert.Equal("TypeError", ErrorMessage.GetErrorMessage(new JsTypeError(string.Empty)));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should handle custom error subclasses", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Formats_a_custom_error_name()
    {
        Assert.Equal("CustomError: custom failure", ErrorMessage.GetErrorMessage(new NamedJsError("CustomError", "custom failure")));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should respect custom toString() overrides", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Respects_a_custom_to_string()
    {
        Assert.Equal("API Error 429: rate limited", ErrorMessage.GetErrorMessage(new ApiStyleError("rate limited", 429)));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should handle custom error subclass with empty message", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Formats_a_custom_error_with_an_empty_message()
    {
        Assert.Equal("CustomError", ErrorMessage.GetErrorMessage(new NamedJsError("CustomError", string.Empty)));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > other types::should JSON.stringify plain objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Serializes_plain_objects()
    {
        var value = new JsonObject { ["code"] = "FAIL", ["detail"] = "oops" };
        Assert.Equal("{\"code\":\"FAIL\",\"detail\":\"oops\"}", ErrorMessage.GetErrorMessage(value));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > other types::should JSON.stringify numbers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Serializes_numbers()
    {
        Assert.Equal("42", ErrorMessage.GetErrorMessage(42));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > other types::should JSON.stringify booleans", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Serializes_booleans()
    {
        Assert.Equal("false", ErrorMessage.GetErrorMessage(false));
    }

    [UpstreamTest("packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > other types::should JSON.stringify arrays", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Serializes_arrays()
    {
        Assert.Equal("[\"a\",\"b\"]", ErrorMessage.GetErrorMessage(new[] { "a", "b" }));
    }

    private sealed class NamedJsError : JsError
    {
        private readonly string _name;

        public NamedJsError(string name, string message)
            : base(message)
        {
            _name = name;
        }

        public override string ErrorName
        {
            get { return _name; }
        }
    }

    private sealed class ApiStyleError : JsError
    {
        public ApiStyleError(string message, int code)
            : base(message)
        {
            Code = code;
        }

        public int Code { get; }

        public override string ToString()
        {
            return "API Error " + Code + ": " + Message;
        }
    }
}
