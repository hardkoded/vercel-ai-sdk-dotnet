// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;

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

    private static Dictionary<string, string?> Header(string? value)
    {
        return new Dictionary<string, string?> { [GatewayErrors.AuthMethodHeader] = value };
    }
}
