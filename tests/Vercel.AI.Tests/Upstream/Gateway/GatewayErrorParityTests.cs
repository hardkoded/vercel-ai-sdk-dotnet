// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Sockets;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using ApiCallError = Vercel.AI.Util.ApiCallError;

namespace Vercel.AI.Tests;

public sealed class GatewayErrorFactoryTests
{
    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayAuthenticationError for authentication_error type", Coverage = UpstreamCoverage.Covered)]
    public void Creates_an_authentication_error_without_using_the_response_message()
    {
        var error = Create("{\"error\":{\"message\":\"Invalid API key\",\"type\":\"authentication_error\"}}", 401);
        Assert.IsType<GatewayAuthenticationError>(error);
        Assert.Contains("No authentication provided", error.Message);
        Assert.Equal(401, error.StatusCode);
        Assert.Equal("authentication_error", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayInvalidRequestError for invalid_request_error type", Coverage = UpstreamCoverage.Covered)]
    public void Creates_an_invalid_request_error()
    {
        var error = Create("{\"error\":{\"message\":\"Missing required parameter\",\"type\":\"invalid_request_error\"}}", 400);
        Assert.IsType<GatewayInvalidRequestError>(error);
        Assert.Equal("Missing required parameter", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayForbiddenError for forbidden type", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_forbidden_error_without_a_rule_id()
    {
        var error = Assert.IsType<GatewayForbiddenError>(Create("{\"error\":{\"message\":\"Request denied by a routing rule.\",\"type\":\"forbidden\"}}", 403));
        Assert.Equal("Request denied by a routing rule.", error.Message);
        Assert.Equal(403, error.StatusCode);
        Assert.Equal("forbidden", error.Type);
        Assert.Null(error.RuleId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::exposes the ruleId on GatewayForbiddenError when present in param", Coverage = UpstreamCoverage.Covered)]
    public void Exposes_the_forbidden_rule_id()
    {
        var error = Assert.IsType<GatewayForbiddenError>(Create("{\"error\":{\"message\":\"Request denied by a routing rule.\",\"type\":\"forbidden\",\"param\":{\"ruleId\":\"rule_abc123\"}}}", 403));
        Assert.Equal("rule_abc123", error.RuleId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::leaves ruleId undefined when the forbidden param has an unexpected shape", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_rule_id_unset_for_an_unexpected_param()
    {
        var error = Assert.IsType<GatewayForbiddenError>(Create("{\"error\":{\"message\":\"Request denied by a routing rule.\",\"type\":\"forbidden\",\"param\":\"model\"}}", 403));
        Assert.Null(error.RuleId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayRateLimitError for rate_limit_exceeded type", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_rate_limit_error()
    {
        var error = Assert.IsType<GatewayRateLimitError>(Create("{\"error\":{\"message\":\"Rate limit exceeded. Try again later.\",\"type\":\"rate_limit_exceeded\"}}", 429));
        Assert.Equal("Rate limit exceeded. Try again later.", error.Message);
        Assert.Equal(429, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayModelNotFoundError for model_not_found type", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_model_not_found_error()
    {
        var error = Assert.IsType<GatewayModelNotFoundError>(Create("{\"error\":{\"message\":\"Model not available\",\"type\":\"model_not_found\",\"param\":{\"modelId\":\"gpt-ai-sdk-test\"}}}", 404));
        Assert.Equal("Model not available", error.Message);
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("gpt-ai-sdk-test", error.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayNotFoundError for not_found type", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_not_found_error()
    {
        var error = Assert.IsType<GatewayNotFoundError>(Create("{\"error\":{\"message\":\"Async job not found.\",\"type\":\"not_found\"}}", 404));
        Assert.Equal("Async job not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("not_found", error.Type);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayModelNotFoundError without modelId for invalid param", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_model_id_unset_for_an_invalid_param()
    {
        var error = Assert.IsType<GatewayModelNotFoundError>(Create("{\"error\":{\"message\":\"Model not available\",\"type\":\"model_not_found\",\"param\":{\"invalidField\":\"value\"}}}", 404));
        Assert.Null(error.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayInternalServerError for internal_server_error type", Coverage = UpstreamCoverage.Covered)]
    public void Creates_an_internal_server_error()
    {
        var error = Assert.IsType<GatewayInternalServerError>(Create("{\"error\":{\"message\":\"Internal server error occurred\",\"type\":\"internal_server_error\"}}", 500));
        Assert.Equal("Internal server error occurred", error.Message);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayFailedDependencyError for failed_dependency type", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_failed_dependency_error()
    {
        var error = Assert.IsType<GatewayFailedDependencyError>(Create("{\"error\":{\"message\":\"A dependency required by the request was unavailable\",\"type\":\"failed_dependency\"}}", 424));
        Assert.Equal("failed_dependency", error.Type);
        Assert.Equal("A dependency required by the request was unavailable", error.Message);
        Assert.Equal(424, error.StatusCode);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayInternalServerError for unknown error type", Coverage = UpstreamCoverage.Covered)]
    public void Maps_an_unknown_type_to_an_internal_server_error()
    {
        var error = Assert.IsType<GatewayInternalServerError>(Create("{\"error\":{\"message\":\"Unknown error occurred\",\"type\":\"unknown_error_type\"}}", 500));
        Assert.Equal("Unknown error occurred", error.Message);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Error response edge cases::should preserve empty string messages from Gateway", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_contextual_message_for_an_empty_authentication_message()
    {
        var error = Create("{\"error\":{\"message\":\"\",\"type\":\"authentication_error\"}}", 401, defaultMessage: "Custom default message");
        Assert.Contains("No authentication provided", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Error response edge cases::should use defaultMessage when response message is null", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_default_message_when_the_response_message_is_null()
    {
        var error = Assert.IsType<GatewayResponseError>(Create("{\"error\":{\"message\":null,\"type\":\"authentication_error\"}}", 401, defaultMessage: "Custom default message"));
        Assert.Equal("Invalid error response format: Custom default message", error.Message);
        Assert.NotNull(error.Response);
        Assert.False(string.IsNullOrEmpty(error.ValidationError));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Error response edge cases::should handle error type as null", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_null_error_type_to_an_internal_server_error()
    {
        Assert.IsType<GatewayInternalServerError>(Create("{\"error\":{\"message\":\"Some error\",\"type\":null}}", 500));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Error response edge cases::should include cause in the created error", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_cause()
    {
        var cause = new Exception("Original network error");
        var error = Create("{\"error\":{\"message\":\"Gateway timeout\",\"type\":\"internal_server_error\"}}", 504, cause: cause);
        Assert.Same(cause, error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for completely invalid response", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_completely_invalid_response()
    {
        var error = Assert.IsType<GatewayResponseError>(Create("{\"invalidField\":\"value\"}", 500));
        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Equal(500, error.StatusCode);
        Assert.Equal("value", error.Response!.Value.GetProperty("invalidField").GetString());
        Assert.False(string.IsNullOrEmpty(error.ValidationError));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for null response", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_null_response()
    {
        var error = Assert.IsType<GatewayResponseError>(GatewayErrors.Create(default, 500));
        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Null(error.Response);
        Assert.False(string.IsNullOrEmpty(error.ValidationError));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should include generationId in error when present in response", Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_generation_id()
    {
        var error = Assert.IsType<GatewayInternalServerError>(Create("{\"error\":{\"message\":\"Internal server error\",\"type\":\"internal_server_error\"},\"generationId\":\"gen_01ABC123XYZ\"}", 500));
        Assert.Equal("gen_01ABC123XYZ", error.GenerationId);
        Assert.Contains("[gen_01ABC123XYZ]", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::authentication_error with authMethod context::should create contextual error for API key authentication failure", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_contextual_api_key_error()
    {
        var error = Create("{\"error\":{\"message\":\"Invalid API key\",\"type\":\"authentication_error\"}}", 401, "api-key");
        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::authentication_error with authMethod context::should create contextual error for OIDC authentication failure", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_contextual_oidc_error()
    {
        var error = Create("{\"error\":{\"message\":\"Invalid OIDC token\",\"type\":\"authentication_error\"}}", 401, "oidc");
        Assert.Contains("Invalid OIDC token", error.Message);
        Assert.Contains("npx vercel link", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::authentication_error with authMethod context::should create contextual error without authMethod context", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_contextual_error_without_an_auth_method()
    {
        var error = Create("{\"error\":{\"message\":\"Invalid API key\",\"type\":\"authentication_error\"}}", 401);
        Assert.Contains("No authentication provided", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for missing error field", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_response_without_an_error_field()
    {
        var error = Assert.IsType<GatewayResponseError>(Create("{\"data\":\"some data\"}", 500, defaultMessage: "Custom error message"));
        Assert.Equal("Invalid error response format: Custom error message", error.Message);
        Assert.Equal("some data", error.Response!.Value.GetProperty("data").GetString());
        Assert.False(string.IsNullOrEmpty(error.ValidationError));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for string response", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_string_response()
    {
        var error = Assert.IsType<GatewayResponseError>(Create("\"Error string\"", 500));
        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Equal("Error string", error.Response!.Value.GetString());
        Assert.False(string.IsNullOrEmpty(error.ValidationError));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for array response", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_array_response()
    {
        var error = Assert.IsType<GatewayResponseError>(Create("[\"error\",\"array\"]", 500));
        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Equal("[\"error\",\"array\"]", error.Response!.Value.GetRawText());
        Assert.False(string.IsNullOrEmpty(error.ValidationError));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Object parameter validation::should use default defaultMessage when not provided", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_default_message_when_none_is_given()
    {
        using var document = JsonDocument.Parse("{\"invalidField\":\"value\"}");
        var error = Assert.IsType<GatewayResponseError>(GatewayErrors.Create(document.RootElement, 500));
        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Equal("value", error.Response!.Value.GetProperty("invalidField").GetString());
        Assert.False(string.IsNullOrEmpty(error.ValidationError));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Object parameter validation::should handle undefined cause", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_the_cause_unset_when_none_is_given()
    {
        var error = Create("{\"error\":{\"message\":\"Test error\",\"type\":\"authentication_error\"}}", 401);
        Assert.Null(error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Complex scenarios::should handle model_not_found with missing param field", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_model_id_unset_without_a_param()
    {
        var error = Assert.IsType<GatewayModelNotFoundError>(Create("{\"error\":{\"message\":\"Model not found\",\"type\":\"model_not_found\"}}", 404));
        Assert.Null(error.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Complex scenarios::should handle response with extra fields", Coverage = UpstreamCoverage.Covered)]
    public void Ignores_extra_fields()
    {
        var error = Create("{\"error\":{\"message\":\"Test error\",\"type\":\"authentication_error\",\"code\":\"AUTH_FAILED\",\"param\":null,\"extraField\":\"should be ignored\"},\"metadata\":\"should be ignored\"}", 401);
        Assert.IsType<GatewayAuthenticationError>(error);
        Assert.Contains("No authentication provided", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Complex scenarios::should preserve error properties correctly", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_response_message_status_cause_and_type()
    {
        var cause = new InvalidCastException("Type error");
        var error = Assert.IsType<GatewayRateLimitError>(Create("{\"error\":{\"message\":\"Rate limit hit\",\"type\":\"rate_limit_exceeded\"}}", 429, defaultMessage: "Fallback message", cause: cause));
        Assert.Equal("Rate limit hit", error.Message);
        Assert.Equal(429, error.StatusCode);
        Assert.Same(cause, error.Cause);
        Assert.Equal("GatewayRateLimitError", error.GetType().Name);
        Assert.Equal("rate_limit_exceeded", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should include generationId in authentication error", Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_generation_id_in_an_authentication_error()
    {
        var error = Assert.IsType<GatewayAuthenticationError>(Create("{\"error\":{\"message\":\"Invalid API key\",\"type\":\"authentication_error\"},\"generationId\":\"gen_01AUTH456\"}", 401, "api-key"));
        Assert.Equal("gen_01AUTH456", error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should include generationId in rate limit error", Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_generation_id_in_a_rate_limit_error()
    {
        var error = Assert.IsType<GatewayRateLimitError>(Create("{\"error\":{\"message\":\"Rate limit exceeded\",\"type\":\"rate_limit_exceeded\"},\"generationId\":\"gen_01RATE789\"}", 429));
        Assert.Equal("gen_01RATE789", error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should include generationId in model not found error", Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_generation_id_in_a_model_not_found_error()
    {
        var error = Assert.IsType<GatewayModelNotFoundError>(Create("{\"error\":{\"message\":\"Model not found\",\"type\":\"model_not_found\",\"param\":{\"modelId\":\"gpt-5\"}},\"generationId\":\"gen_01MODEL000\"}", 404));
        Assert.Equal("gen_01MODEL000", error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should have undefined generationId when not present in response", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_the_generation_id_unset_when_absent()
    {
        var error = Create("{\"error\":{\"message\":\"Some error\",\"type\":\"internal_server_error\"}}", 500);
        Assert.Null(error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should extract generationId from malformed response when possible", Coverage = UpstreamCoverage.Covered)]
    public void Reads_the_generation_id_from_a_malformed_response()
    {
        var error = Assert.IsType<GatewayResponseError>(Create("{\"invalidField\":\"value\",\"generationId\":\"gen_01MALFORMED\"}", 500));
        Assert.Equal("gen_01MALFORMED", error.GenerationId);
    }

    private static GatewayError Create(string json, int status, string? authMethod = null, string defaultMessage = "Gateway request failed", Exception? cause = null)
    {
        using var document = JsonDocument.Parse(json);
        return GatewayErrors.Create(document.RootElement, status, authMethod, defaultMessage, cause);
    }
}

public sealed class GatewayErrorTypeTests
{
    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for GatewayInternalServerError (500)", Coverage = UpstreamCoverage.Covered)]
    public void Internal_server_errors_are_retryable()
    {
        Assert.True(new GatewayInternalServerError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for GatewayRateLimitError (429)", Coverage = UpstreamCoverage.Covered)]
    public void Rate_limit_errors_are_retryable()
    {
        Assert.True(new GatewayRateLimitError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for GatewayTimeoutError (408)", Coverage = UpstreamCoverage.Covered)]
    public void Timeout_errors_are_retryable()
    {
        Assert.True(new GatewayTimeoutError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for status codes >= 500", Coverage = UpstreamCoverage.Covered)]
    public void Status_codes_at_or_above_500_are_retryable()
    {
        Assert.True(new GatewayInternalServerError(statusCode: 503).IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be false for GatewayAuthenticationError (401)", Coverage = UpstreamCoverage.Covered)]
    public void Authentication_errors_are_not_retryable()
    {
        Assert.False(new GatewayAuthenticationError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be false for GatewayInvalidRequestError (400)", Coverage = UpstreamCoverage.Covered)]
    public void Invalid_request_errors_are_not_retryable()
    {
        Assert.False(new GatewayInvalidRequestError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be false for GatewayModelNotFoundError (404)", Coverage = UpstreamCoverage.Covered)]
    public void Model_not_found_errors_are_not_retryable()
    {
        Assert.False(new GatewayModelNotFoundError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for GatewayResponseError (502)", Coverage = UpstreamCoverage.Covered)]
    public void Response_errors_are_retryable()
    {
        Assert.True(new GatewayResponseError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void Authentication_error_defaults()
    {
        var error = new GatewayAuthenticationError();
        Assert.IsAssignableFrom<GatewayError>(error);
        Assert.Equal("GatewayAuthenticationError", error.GetType().Name);
        Assert.Equal("authentication_error", error.Type);
        Assert.Equal("Authentication failed", error.Message);
        Assert.Equal(401, error.StatusCode);
        Assert.Null(error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should create error for invalid API key only", Coverage = UpstreamCoverage.Covered)]
    public void Contextual_api_key_message()
    {
        var error = GatewayAuthenticationError.CreateContextual(true, false);
        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should create error for invalid OIDC token only", Coverage = UpstreamCoverage.Covered)]
    public void Contextual_oidc_message()
    {
        var error = GatewayAuthenticationError.CreateContextual(false, true);
        Assert.Contains("Invalid OIDC token", error.Message);
        Assert.Contains("npx vercel link", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should create error for no authentication provided", Coverage = UpstreamCoverage.Covered)]
    public void Contextual_missing_authentication_message()
    {
        var error = GatewayAuthenticationError.CreateContextual(false, false);
        Assert.Contains("No authentication provided", error.Message);
        Assert.Contains("Option 1", error.Message);
        Assert.Contains("Option 2", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayResponseError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void Response_error_defaults()
    {
        var error = new GatewayResponseError();
        Assert.Equal("GatewayResponseError", error.GetType().Name);
        Assert.Equal("response_error", error.Type);
        Assert.Equal("Invalid response from Gateway", error.Message);
        Assert.Equal(502, error.StatusCode);
        Assert.Null(error.Response);
        Assert.Null(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError::should create error with custom values", Coverage = UpstreamCoverage.Covered)]
    public void Authentication_error_custom_values()
    {
        var cause = new Exception("Original error");
        var error = new GatewayAuthenticationError("Custom auth failed", 403, cause);
        Assert.Equal("Custom auth failed", error.Message);
        Assert.Equal(403, error.StatusCode);
        Assert.Same(cause, error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void Authentication_error_instance_check()
    {
        var error = new GatewayAuthenticationError();
        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.False(GatewayAuthenticationError.IsInstance(new Exception("Not a gateway error")));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should prioritize API key error when both were provided", Coverage = UpstreamCoverage.Covered)]
    public void Contextual_message_prefers_the_api_key()
    {
        var error = GatewayAuthenticationError.CreateContextual(true, true);
        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should create error for neither provided (legacy test)", Coverage = UpstreamCoverage.Covered)]
    public void Contextual_message_without_credentials()
    {
        var error = GatewayAuthenticationError.CreateContextual(apiKeyProvided: false, oidcTokenProvided: false);
        Assert.Contains("No authentication provided", error.Message);
        Assert.Contains("Option 1", error.Message);
        Assert.Contains("Option 2", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInvalidRequestError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void Invalid_request_error_defaults()
    {
        var error = new GatewayInvalidRequestError();
        Assert.Equal("GatewayInvalidRequestError", error.GetType().Name);
        Assert.Equal("invalid_request_error", error.Type);
        Assert.Equal("Invalid request", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInvalidRequestError::should create error with custom values", Coverage = UpstreamCoverage.Covered)]
    public void Invalid_request_error_custom_values()
    {
        var error = new GatewayInvalidRequestError("Missing required field", 422);
        Assert.Equal("Missing required field", error.Message);
        Assert.Equal(422, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInvalidRequestError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void Invalid_request_error_instance_check()
    {
        var error = new GatewayInvalidRequestError();
        Assert.True(GatewayInvalidRequestError.IsInstance(error));
        Assert.False(GatewayAuthenticationError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayRateLimitError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void Rate_limit_error_defaults()
    {
        var error = new GatewayRateLimitError();
        Assert.Equal("GatewayRateLimitError", error.GetType().Name);
        Assert.Equal("rate_limit_exceeded", error.Type);
        Assert.Equal("Rate limit exceeded", error.Message);
        Assert.Equal(429, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayRateLimitError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void Rate_limit_error_instance_check()
    {
        var error = new GatewayRateLimitError();
        Assert.True(GatewayRateLimitError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayModelNotFoundError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void Model_not_found_error_defaults()
    {
        var error = new GatewayModelNotFoundError();
        Assert.Equal("GatewayModelNotFoundError", error.GetType().Name);
        Assert.Equal("model_not_found", error.Type);
        Assert.Equal("Model not found", error.Message);
        Assert.Equal(404, error.StatusCode);
        Assert.Null(error.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayModelNotFoundError::should create error with model ID", Coverage = UpstreamCoverage.Covered)]
    public void Model_not_found_error_with_a_model_id()
    {
        var error = new GatewayModelNotFoundError("Model gpt-4 not found", modelId: "gpt-4");
        Assert.Equal("Model gpt-4 not found", error.Message);
        Assert.Equal("gpt-4", error.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayModelNotFoundError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void Model_not_found_error_instance_check()
    {
        var error = new GatewayModelNotFoundError();
        Assert.True(GatewayModelNotFoundError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInternalServerError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void Internal_server_error_defaults()
    {
        var error = new GatewayInternalServerError();
        Assert.Equal("GatewayInternalServerError", error.GetType().Name);
        Assert.Equal("internal_server_error", error.Type);
        Assert.Equal("Internal server error", error.Message);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInternalServerError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void Internal_server_error_instance_check()
    {
        var error = new GatewayInternalServerError();
        Assert.True(GatewayInternalServerError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayResponseError::should create error with response and validation error details", Coverage = UpstreamCoverage.Covered)]
    public void Response_error_keeps_the_response_and_validation_error()
    {
        using var document = JsonDocument.Parse("{\"invalidField\":\"value\"}");
        var error = new GatewayResponseError("Custom parsing error", 422, document.RootElement, "error: Required");
        Assert.Equal("Custom parsing error", error.Message);
        Assert.Equal(422, error.StatusCode);
        Assert.Equal("value", error.Response!.Value.GetProperty("invalidField").GetString());
        Assert.Equal("error: Required", error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayResponseError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void Response_error_instance_check()
    {
        var error = new GatewayResponseError();
        Assert.True(GatewayResponseError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::Error inheritance chain::should maintain proper inheritance", Coverage = UpstreamCoverage.Covered)]
    public void Errors_inherit_from_gateway_error()
    {
        Exception error = new GatewayAuthenticationError();
        Assert.IsAssignableFrom<GatewayError>(error);
        Assert.IsType<GatewayAuthenticationError>(error);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::Error inheritance chain::should have proper stack traces", Coverage = UpstreamCoverage.Covered)]
    public void Thrown_errors_have_a_stack_trace()
    {
        var error = Assert.Throws<GatewayAuthenticationError>(Fail);
        Assert.NotNull(error.StackTrace);
        Assert.Contains("GatewayAuthenticationError", error.ToString());
        Assert.Contains("Test error", error.ToString());

        static void Fail() => throw new GatewayAuthenticationError("Test error");
    }
}

public sealed class GatewayAuthMethodTests
{
    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::gateway headers::should export the correct auth method header name", Coverage = UpstreamCoverage.Covered)]
    public void Exports_the_auth_method_header_name()
    {
        Assert.Equal("ai-gateway-auth-method", GatewayErrors.AuthMethodHeader);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::gateway headers::should export the correct Vercel AI Gateway team header name", Coverage = UpstreamCoverage.Covered)]
    public void Exports_the_team_header_name()
    {
        Assert.Equal("x-vercel-ai-gateway-team", GatewayErrors.TeamHeader);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > valid authentication methods::should parse \"api-key\" auth method", Coverage = UpstreamCoverage.Covered)]
    public void Parses_api_key()
    {
        Assert.Equal("api-key", GatewayErrors.ParseAuthMethod(Header("api-key")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > valid authentication methods::should parse \"oidc\" auth method", Coverage = UpstreamCoverage.Covered)]
    public void Parses_oidc()
    {
        Assert.Equal("oidc", GatewayErrors.ParseAuthMethod(Header("oidc")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > valid authentication methods::should handle headers with other fields present", Coverage = UpstreamCoverage.Covered)]
    public void Ignores_unrelated_headers()
    {
        var headers = new Dictionary<string, string?>
        {
            ["authorization"] = "Bearer token",
            ["content-type"] = "application/json",
            [GatewayErrors.AuthMethodHeader] = "api-key",
            ["user-agent"] = "test-agent",
        };
        Assert.Equal("api-key", GatewayErrors.ParseAuthMethod(headers));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for invalid auth method string", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_invalid_auth_method()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header("invalid-method")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for empty string", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_empty_auth_method()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header(string.Empty)));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for case-sensitive variations", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_api_key_case_variations()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header("API-KEY")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for OIDC case variations", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_oidc_case_variations()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header("OIDC")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > missing or undefined headers::should return undefined when header is missing", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_when_the_header_is_missing()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(new Dictionary<string, string?> { ["authorization"] = "Bearer token" }));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > missing or undefined headers::should return undefined when headers object is empty", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_an_empty_header_set()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(new Dictionary<string, string?>()));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > edge cases::should return undefined for whitespace-only strings", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_whitespace()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header("   ")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > edge cases::should return undefined for auth methods with extra whitespace", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_padded_values()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header(" api-key ")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for numeric value", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_numeric_value()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header("123")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for boolean-like strings", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_boolean_like_value()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header("true")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > missing or undefined headers::should return undefined when header is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_a_null_header_value()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Header(null)));
    }

    private static Dictionary<string, string?> Header(string? value)
    {
        return new Dictionary<string, string?> { [GatewayErrors.AuthMethodHeader] = value };
    }
}

public sealed class GatewayAsErrorTests
{
    private const string AsErrorPrefix = "packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > ";

    [Fact]
    [UpstreamTest(AsErrorPrefix + "timeout error detection::should detect error with UND_ERR_HEADERS_TIMEOUT code", Coverage = UpstreamCoverage.Covered)]
    public void Detects_an_api_timeout()
    {
        var error = GatewayErrors.AsGatewayError(new ApiTimeoutException("Request timeout"));
        Assert.IsType<GatewayTimeoutError>(error);
        Assert.Contains("Request timeout", error.Message);
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "timeout error detection::should detect error with UND_ERR_BODY_TIMEOUT code", Coverage = UpstreamCoverage.Covered)]
    public void Detects_a_timeout_exception()
    {
        Assert.IsType<GatewayTimeoutError>(GatewayErrors.AsGatewayError(new TimeoutException("Body timeout")));
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "timeout error detection::should detect error with UND_ERR_CONNECT_TIMEOUT code", Coverage = UpstreamCoverage.Covered)]
    public void Detects_a_socket_connect_timeout()
    {
        Assert.IsType<GatewayTimeoutError>(GatewayErrors.AsGatewayError(new SocketException((int)SocketError.TimedOut)));
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "non-timeout errors::should not treat network errors as timeout errors", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_a_network_error_as_a_response_error()
    {
        var error = GatewayErrors.AsGatewayError(new Exception("Network error"));
        Assert.IsType<GatewayResponseError>(error);
        Assert.Contains("Gateway request failed: Network error", error.Message);
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "non-timeout errors::should not treat connection errors as timeout errors", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_a_refused_connection_as_a_response_error()
    {
        Assert.IsType<GatewayResponseError>(GatewayErrors.AsGatewayError(new SocketException((int)SocketError.ConnectionRefused)));
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "non-timeout errors::should pass through existing GatewayError instances", Coverage = UpstreamCoverage.Covered)]
    public void Passes_gateway_errors_through()
    {
        var existing = GatewayTimeoutError.Create("existing timeout");
        Assert.Same(existing, GatewayErrors.AsGatewayError(existing));
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "non-timeout errors::should handle null", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_null_as_a_response_error()
    {
        Assert.IsType<GatewayResponseError>(GatewayErrors.AsGatewayError(null));
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "error properties::should preserve the original error as cause", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_timeout_as_the_cause()
    {
        var original = new ApiTimeoutException("timeout error");
        Assert.Same(original, GatewayErrors.AsGatewayError(original).Cause);
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "error properties::should set correct status code for timeout errors", Coverage = UpstreamCoverage.Covered)]
    public void Timeouts_use_status_408()
    {
        Assert.Equal(408, GatewayErrors.AsGatewayError(new ApiTimeoutException("timeout")).StatusCode);
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "error properties::should have correct error type", Coverage = UpstreamCoverage.Covered)]
    public void Timeouts_use_the_timeout_type()
    {
        Assert.Equal("timeout_error", GatewayErrors.AsGatewayError(new ApiTimeoutException("timeout")).Type);
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "APICallError with timeout cause::should detect timeout when APICallError has UND_ERR_HEADERS_TIMEOUT in cause", Coverage = UpstreamCoverage.Covered)]
    public void Detects_an_api_call_error_caused_by_a_timeout()
    {
        var apiCallError = new ApiCallError("Cannot connect to API: Request timeout", "https://example.com", new object(), cause: new ApiTimeoutException("Request timeout"));
        var error = GatewayErrors.AsGatewayError(apiCallError);
        Assert.IsType<GatewayTimeoutError>(error);
        Assert.Contains("Gateway request timed out", error.Message);
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "APICallError with timeout cause::should not treat APICallError as timeout if cause is not timeout-related", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_treat_an_api_call_error_with_a_network_cause_as_a_timeout()
    {
        var apiCallError = new ApiCallError(
            "Cannot connect to API: Network connection failed",
            "https://example.com",
            new object(),
            statusCode: 500,
            responseBody: "{\"error\":{\"message\":\"Internal error\",\"type\":\"internal_error\"}}",
            cause: new Exception("Network connection failed"));
        Assert.IsNotType<GatewayTimeoutError>(GatewayErrors.AsGatewayError(apiCallError));
    }

    [Fact]
    [UpstreamTest(AsErrorPrefix + "APICallError with timeout cause::should preserve retryability for transport errors after response headers", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_retryability_for_a_transport_error_after_a_success_status()
    {
        var apiCallError = new ApiCallError("Failed to process successful response", "https://example.com", new object(), statusCode: 200, isRetryable: true);
        var error = GatewayErrors.AsGatewayError(apiCallError);
        Assert.IsType<GatewayResponseError>(error);
        Assert.True(error.IsRetryable);
        Assert.Equal(200, error.StatusCode);
        Assert.Same(apiCallError, error.Cause);
    }
}

public sealed class GatewayApiCallResponseTests
{
    private const string ExtractPrefix = "packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > ";

    [Fact]
    [UpstreamTest(ExtractPrefix + "when error.data is available::should return error.data when successfully parsed by AI SDK", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_parsed_data_over_the_body()
    {
        var data = Json("{\"error\":{\"message\":\"Parsed error\",\"type\":\"authentication_error\"}}");
        var result = GatewayErrors.ExtractApiCallResponse(Error("{\"different\":\"data\"}", data));
        Assert.Equal(data.GetRawText(), result.GetRawText());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "when error.data is available::should return error.data even when it is an empty object", Coverage = UpstreamCoverage.Covered)]
    public void Returns_empty_object_data()
    {
        var result = GatewayErrors.ExtractApiCallResponse(Error("{\"fallback\":\"data\"}", Json("{}")));
        Assert.Equal("{}", result.GetRawText());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "when error.data is undefined::should parse and return responseBody as JSON when valid", Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_json_body()
    {
        var body = "{\"ferror\":{\"message\":\"Malformed error\",\"type\":\"model_not_found\"}}";
        Assert.Equal(body, GatewayErrors.ExtractApiCallResponse(Error(body)).GetRawText());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "when error.data is undefined::should return raw responseBody when JSON parsing fails", Coverage = UpstreamCoverage.Covered)]
    public void Returns_a_non_json_body_as_a_string()
    {
        Assert.Equal("This is not valid JSON", GatewayErrors.ExtractApiCallResponse(Error("This is not valid JSON")).GetString());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "when error.data is undefined::should handle HTML error responses", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_html_body_as_a_string()
    {
        var html = "<html><body><h1>500 Internal Server Error</h1></body></html>";
        Assert.Equal(html, GatewayErrors.ExtractApiCallResponse(Error(html)).GetString());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "when error.data is undefined::should handle empty string responseBody", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_empty_body_as_an_empty_string()
    {
        Assert.Equal(string.Empty, GatewayErrors.ExtractApiCallResponse(Error(string.Empty)).GetString());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "when error.data is undefined::should handle malformed JSON gracefully", Coverage = UpstreamCoverage.Covered)]
    public void Returns_malformed_json_as_a_string()
    {
        Assert.Equal("{\"incomplete\": json", GatewayErrors.ExtractApiCallResponse(Error("{\"incomplete\": json")).GetString());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "when error.data is undefined::should parse complex nested JSON structures", Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_nested_json_body()
    {
        var body = "{\"error\":{\"message\":\"Complex error\",\"type\":\"validation_error\",\"details\":{\"field\":\"prompt\",\"issues\":[{\"code\":\"too_long\",\"message\":\"Prompt exceeds maximum length\"},{\"code\":\"invalid_format\",\"message\":\"Contains invalid characters\"}]}},\"metadata\":{\"requestId\":\"12345\",\"timestamp\":\"2024-01-01T00:00:00Z\"}}";
        Assert.Equal(body, GatewayErrors.ExtractApiCallResponse(Error(body)).GetRawText());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "when responseBody is not available::should return empty object when both data and responseBody are undefined", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_empty_object_without_a_body()
    {
        Assert.Equal("{}", GatewayErrors.ExtractApiCallResponse(Error(null)).GetRawText());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "edge cases::should handle numeric responseBody", Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_numeric_body()
    {
        Assert.Equal(404, GatewayErrors.ExtractApiCallResponse(Error("404")).GetInt32());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "edge cases::should handle boolean responseBody", Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_boolean_body()
    {
        Assert.True(GatewayErrors.ExtractApiCallResponse(Error("true")).GetBoolean());
    }

    [Fact]
    [UpstreamTest(ExtractPrefix + "edge cases::should handle array responseBody", Coverage = UpstreamCoverage.Covered)]
    public void Parses_an_array_body()
    {
        var body = "[\"error1\",\"error2\",\"error3\"]";
        Assert.Equal(body, GatewayErrors.ExtractApiCallResponse(Error(body)).GetRawText());
    }

    private static ApiCallError Error(string? body, object? data = null)
    {
        return new ApiCallError("Request failed", "http://test.url", new object(), 500, new Dictionary<string, string>(), body, data: data);
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
