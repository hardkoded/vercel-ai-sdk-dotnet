// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Parity.Gateway;

/// <summary>Ports the Gateway error, auth-method, and API-call response unit tests.</summary>
public sealed class GatewayErrorTests
{
    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void AuthenticationErrorUsesDefaults()
    {
        var error = new GatewayAuthenticationError();

        Assert.IsAssignableFrom<Exception>(error);
        Assert.IsAssignableFrom<GatewayError>(error);
        Assert.Equal("GatewayAuthenticationError", error.Name);
        Assert.Equal("authentication_error", error.Type);
        Assert.Equal("Authentication failed", error.Message);
        Assert.Equal(401, error.StatusCode);
        Assert.Null(error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError::should create error with custom values", Coverage = UpstreamCoverage.Covered)]
    public void AuthenticationErrorKeepsCustomValues()
    {
        var cause = new InvalidOperationException("Original error");
        var error = new GatewayAuthenticationError("Custom auth failed", 403, cause);

        Assert.Equal("Custom auth failed", error.Message);
        Assert.Equal(403, error.StatusCode);
        Assert.Same(cause, error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void AuthenticationErrorIsIdentifiable()
    {
        var error = new GatewayAuthenticationError();

        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.False(GatewayAuthenticationError.IsInstance(new Exception("Not a gateway error")));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should create error for invalid API key only", Coverage = UpstreamCoverage.Covered)]
    public void ContextualErrorDescribesAnInvalidApiKey()
    {
        var error = GatewayAuthenticationError.CreateContextual(true, false);

        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should create error for invalid OIDC token only", Coverage = UpstreamCoverage.Covered)]
    public void ContextualErrorDescribesAnInvalidOidcToken()
    {
        var error = GatewayAuthenticationError.CreateContextual(false, true);

        Assert.Contains("Invalid OIDC token", error.Message);
        Assert.Contains("npx vercel link", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should create error for no authentication provided", Coverage = UpstreamCoverage.Covered)]
    public void ContextualErrorDescribesMissingCredentials()
    {
        var error = GatewayAuthenticationError.CreateContextual(false, false);

        Assert.Contains("No authentication provided", error.Message);
        Assert.Contains("Option 1", error.Message);
        Assert.Contains("Option 2", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should prioritize API key error when both were provided", Coverage = UpstreamCoverage.Covered)]
    public void ContextualErrorPrefersTheApiKeyHint()
    {
        var error = GatewayAuthenticationError.CreateContextual(true, true);

        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayAuthenticationError > createContextualError::should create error for neither provided (legacy test)", Coverage = UpstreamCoverage.Covered)]
    public void ContextualErrorLegacyPathDescribesMissingCredentials()
    {
        var error = GatewayAuthenticationError.CreateContextual(false, false);

        Assert.Contains("No authentication provided", error.Message);
        Assert.Contains("Option 1", error.Message);
        Assert.Contains("Option 2", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInvalidRequestError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void InvalidRequestErrorUsesDefaults()
    {
        var error = new GatewayInvalidRequestError();

        Assert.Equal("GatewayInvalidRequestError", error.Name);
        Assert.Equal("invalid_request_error", error.Type);
        Assert.Equal("Invalid request", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInvalidRequestError::should create error with custom values", Coverage = UpstreamCoverage.Covered)]
    public void InvalidRequestErrorKeepsCustomValues()
    {
        var error = new GatewayInvalidRequestError("Missing required field", 422);

        Assert.Equal("Missing required field", error.Message);
        Assert.Equal(422, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInvalidRequestError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void InvalidRequestErrorIsIdentifiable()
    {
        var error = new GatewayInvalidRequestError();

        Assert.True(GatewayInvalidRequestError.IsInstance(error));
        Assert.False(GatewayAuthenticationError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayRateLimitError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void RateLimitErrorUsesDefaults()
    {
        var error = new GatewayRateLimitError();

        Assert.Equal("GatewayRateLimitError", error.Name);
        Assert.Equal("rate_limit_exceeded", error.Type);
        Assert.Equal("Rate limit exceeded", error.Message);
        Assert.Equal(429, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayRateLimitError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void RateLimitErrorIsIdentifiable()
    {
        var error = new GatewayRateLimitError();

        Assert.True(GatewayRateLimitError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayModelNotFoundError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void ModelNotFoundErrorUsesDefaults()
    {
        var error = new GatewayModelNotFoundError();

        Assert.Equal("GatewayModelNotFoundError", error.Name);
        Assert.Equal("model_not_found", error.Type);
        Assert.Equal("Model not found", error.Message);
        Assert.Equal(404, error.StatusCode);
        Assert.Null(error.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayModelNotFoundError::should create error with model ID", Coverage = UpstreamCoverage.Covered)]
    public void ModelNotFoundErrorKeepsTheModelId()
    {
        var error = new GatewayModelNotFoundError("Model gpt-4 not found", modelId: "gpt-4");

        Assert.Equal("Model gpt-4 not found", error.Message);
        Assert.Equal("gpt-4", error.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayModelNotFoundError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void ModelNotFoundErrorIsIdentifiable()
    {
        var error = new GatewayModelNotFoundError();

        Assert.True(GatewayModelNotFoundError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInternalServerError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void InternalServerErrorUsesDefaults()
    {
        var error = new GatewayInternalServerError();

        Assert.Equal("GatewayInternalServerError", error.Name);
        Assert.Equal("internal_server_error", error.Type);
        Assert.Equal("Internal server error", error.Message);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayInternalServerError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void InternalServerErrorIsIdentifiable()
    {
        var error = new GatewayInternalServerError();

        Assert.True(GatewayInternalServerError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for GatewayInternalServerError (500)", Coverage = UpstreamCoverage.Covered)]
    public void InternalServerErrorIsRetryable()
    {
        Assert.True(new GatewayInternalServerError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for GatewayRateLimitError (429)", Coverage = UpstreamCoverage.Covered)]
    public void RateLimitErrorIsRetryable()
    {
        Assert.True(new GatewayRateLimitError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for GatewayTimeoutError (408)", Coverage = UpstreamCoverage.Covered)]
    public void TimeoutErrorIsRetryable()
    {
        Assert.True(new GatewayTimeoutError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for status codes >= 500", Coverage = UpstreamCoverage.Covered)]
    public void ServerStatusCodesAreRetryable()
    {
        Assert.True(new GatewayInternalServerError(statusCode: 503).IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be false for GatewayAuthenticationError (401)", Coverage = UpstreamCoverage.Covered)]
    public void AuthenticationErrorIsNotRetryable()
    {
        Assert.False(new GatewayAuthenticationError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be false for GatewayInvalidRequestError (400)", Coverage = UpstreamCoverage.Covered)]
    public void InvalidRequestErrorIsNotRetryable()
    {
        Assert.False(new GatewayInvalidRequestError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be false for GatewayModelNotFoundError (404)", Coverage = UpstreamCoverage.Covered)]
    public void ModelNotFoundErrorIsNotRetryable()
    {
        Assert.False(new GatewayModelNotFoundError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::isRetryable::should be true for GatewayResponseError (502)", Coverage = UpstreamCoverage.Covered)]
    public void ResponseErrorIsRetryable()
    {
        Assert.True(new GatewayResponseError().IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayResponseError::should create error with default values", Coverage = UpstreamCoverage.Covered)]
    public void ResponseErrorUsesDefaults()
    {
        var error = new GatewayResponseError();

        Assert.Equal("GatewayResponseError", error.Name);
        Assert.Equal("response_error", error.Type);
        Assert.Equal("Invalid response from Gateway", error.Message);
        Assert.Equal(502, error.StatusCode);
        Assert.Null(error.Response);
        Assert.Null(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayResponseError::should create error with response and validation error details", Coverage = UpstreamCoverage.Covered)]
    public void ResponseErrorKeepsThePayloadAndValidationError()
    {
        var response = JsonNode.Parse("{\"invalidField\":\"value\"}");
        var validationError = new GatewayValidationError("Required");
        var error = new GatewayResponseError("Custom parsing error", 422, response, validationError);

        Assert.Equal("Custom parsing error", error.Message);
        Assert.Equal(422, error.StatusCode);
        Assert.Same(response, error.Response);
        Assert.Same(validationError, error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::GatewayResponseError::should be identifiable via instance check", Coverage = UpstreamCoverage.Covered)]
    public void ResponseErrorIsIdentifiable()
    {
        var error = new GatewayResponseError();

        Assert.True(GatewayResponseError.IsInstance(error));
        Assert.True(GatewayError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::Cross-realm instance checking::should work with symbol-based type checking", Coverage = UpstreamCoverage.Covered)]
    public void MarkersIdentifyGatewayErrors()
    {
        var error = new GatewayAuthenticationError();

        Assert.Equal("vercel.ai.gateway.error", GatewayError.Marker);
        Assert.Equal("vercel.ai.gateway.error.GatewayAuthenticationError", GatewayAuthenticationError.TypeMarkerName);
        Assert.True(error.HasMarker(GatewayError.Marker));
        Assert.True(error.HasMarker(GatewayAuthenticationError.TypeMarkerName));
        Assert.True(GatewayError.HasMarker(error));
        Assert.True(GatewayAuthenticationError.IsInstance(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::Error inheritance chain::should maintain proper inheritance", Coverage = UpstreamCoverage.Covered)]
    public void AuthenticationErrorInheritsTheGatewayHierarchy()
    {
        var error = new GatewayAuthenticationError();

        Assert.IsAssignableFrom<Exception>(error);
        Assert.IsAssignableFrom<GatewayError>(error);
        Assert.IsType<GatewayAuthenticationError>(error);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/gateway-error-types.test.ts::Error inheritance chain::should have proper stack traces", Coverage = UpstreamCoverage.Covered)]
    public void AuthenticationErrorCapturesAStackTrace()
    {
        var error = new GatewayAuthenticationError("Test error");

        Assert.Contains("GatewayAuthenticationError", error.ToString());
        Assert.Contains("Test error", error.ToString());
        var thrown = Assert.Throws<GatewayAuthenticationError>((Action)(() => throw error));
        Assert.False(string.IsNullOrEmpty(thrown.StackTrace));
        Assert.Contains("Test error", thrown.ToString());
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayAuthenticationError for authentication_error type", Coverage = UpstreamCoverage.Covered)]
    public void MapsAuthenticationErrors()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Invalid API key\",\"type\":\"authentication_error\"}}"), 401);

        var auth = Assert.IsType<GatewayAuthenticationError>(error);
        Assert.Contains("No authentication provided", auth.Message);
        Assert.Equal(401, auth.StatusCode);
        Assert.Equal("authentication_error", auth.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayInvalidRequestError for invalid_request_error type", Coverage = UpstreamCoverage.Covered)]
    public void MapsInvalidRequestErrors()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Missing required parameter\",\"type\":\"invalid_request_error\"}}"), 400);

        Assert.IsType<GatewayInvalidRequestError>(error);
        Assert.Equal("Missing required parameter", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayForbiddenError for forbidden type", Coverage = UpstreamCoverage.Covered)]
    public void MapsForbiddenErrorsWithoutARule()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Request denied by a routing rule.\",\"type\":\"forbidden\"}}"), 403);

        var forbidden = Assert.IsType<GatewayForbiddenError>(error);
        Assert.Equal("Request denied by a routing rule.", forbidden.Message);
        Assert.Equal(403, forbidden.StatusCode);
        Assert.Equal("forbidden", forbidden.Type);
        Assert.Null(forbidden.RuleId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::exposes the ruleId on GatewayForbiddenError when present in param", Coverage = UpstreamCoverage.Covered)]
    public void MapsForbiddenRuleId()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Request denied by a routing rule.\",\"type\":\"forbidden\",\"param\":{\"ruleId\":\"rule_abc123\"}}}"), 403);

        var forbidden = Assert.IsType<GatewayForbiddenError>(error);
        Assert.Equal("rule_abc123", forbidden.RuleId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::leaves ruleId undefined when the forbidden param has an unexpected shape", Coverage = UpstreamCoverage.Covered)]
    public void IgnoresANonObjectForbiddenParam()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Request denied by a routing rule.\",\"type\":\"forbidden\",\"param\":\"model\"}}"), 403);

        var forbidden = Assert.IsType<GatewayForbiddenError>(error);
        Assert.Null(forbidden.RuleId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayRateLimitError for rate_limit_exceeded type", Coverage = UpstreamCoverage.Covered)]
    public void MapsRateLimitErrors()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Rate limit exceeded. Try again later.\",\"type\":\"rate_limit_exceeded\"}}"), 429);

        Assert.IsType<GatewayRateLimitError>(error);
        Assert.Equal("Rate limit exceeded. Try again later.", error.Message);
        Assert.Equal(429, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayModelNotFoundError for model_not_found type", Coverage = UpstreamCoverage.Covered)]
    public void MapsModelNotFoundErrors()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Model not available\",\"type\":\"model_not_found\",\"param\":{\"modelId\":\"gpt-ai-sdk-test\"}}}"), 404);

        var missing = Assert.IsType<GatewayModelNotFoundError>(error);
        Assert.Equal("Model not available", missing.Message);
        Assert.Equal(404, missing.StatusCode);
        Assert.Equal("gpt-ai-sdk-test", missing.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayNotFoundError for not_found type", Coverage = UpstreamCoverage.Covered)]
    public void MapsNotFoundErrors()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Async job not found.\",\"type\":\"not_found\"}}"), 404);

        var missing = Assert.IsType<GatewayNotFoundError>(error);
        Assert.Equal("Async job not found.", missing.Message);
        Assert.Equal(404, missing.StatusCode);
        Assert.Equal("not_found", missing.Type);
        Assert.False(missing.IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayModelNotFoundError without modelId for invalid param", Coverage = UpstreamCoverage.Covered)]
    public void ModelNotFoundWithoutAModelIdLeavesItUnset()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Model not available\",\"type\":\"model_not_found\",\"param\":{\"invalidField\":\"value\"}}}"), 404);

        var missing = Assert.IsType<GatewayModelNotFoundError>(error);
        Assert.Null(missing.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayInternalServerError for internal_server_error type", Coverage = UpstreamCoverage.Covered)]
    public void MapsInternalServerErrors()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Internal server error occurred\",\"type\":\"internal_server_error\"}}"), 500);

        Assert.IsType<GatewayInternalServerError>(error);
        Assert.Equal("Internal server error occurred", error.Message);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayFailedDependencyError for failed_dependency type", Coverage = UpstreamCoverage.Covered)]
    public void MapsFailedDependencyErrors()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"A dependency required by the request was unavailable\",\"type\":\"failed_dependency\"}}"), 424);

        var dependency = Assert.IsType<GatewayFailedDependencyError>(error);
        Assert.Equal("failed_dependency", dependency.Type);
        Assert.Equal("A dependency required by the request was unavailable", dependency.Message);
        Assert.Equal(424, dependency.StatusCode);
        Assert.False(dependency.IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Valid error responses::should create GatewayInternalServerError for unknown error type", Coverage = UpstreamCoverage.Covered)]
    public void UnknownErrorTypesBecomeInternalServerErrors()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Unknown error occurred\",\"type\":\"unknown_error_type\"}}"), 500);

        Assert.IsType<GatewayInternalServerError>(error);
        Assert.Equal("Unknown error occurred", error.Message);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Error response edge cases::should preserve empty string messages from Gateway", Coverage = UpstreamCoverage.Covered)]
    public void EmptyAuthenticationMessageStillUsesTheContextualHint()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"\",\"type\":\"authentication_error\"}}"), 401, "Custom default message");

        Assert.Contains("No authentication provided", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Error response edge cases::should use defaultMessage when response message is null", Coverage = UpstreamCoverage.Covered)]
    public void NullMessageBecomesAResponseError()
    {
        var response = Parse("{\"error\":{\"message\":null,\"type\":\"authentication_error\"}}");
        var error = GatewayErrors.CreateFromResponse(response, 401, "Custom default message");

        var responseError = Assert.IsType<GatewayResponseError>(error);
        Assert.Equal("Invalid error response format: Custom default message", responseError.Message);
        Assert.Same(response, responseError.Response);
        Assert.NotNull(responseError.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Error response edge cases::should handle error type as null", Coverage = UpstreamCoverage.Covered)]
    public void NullErrorTypeBecomesAnInternalServerError()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Some error\",\"type\":null}}"), 500);

        Assert.IsType<GatewayInternalServerError>(error);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Error response edge cases::should include cause in the created error", Coverage = UpstreamCoverage.Covered)]
    public void CreatedErrorKeepsTheCause()
    {
        var cause = new InvalidOperationException("Original network error");
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Gateway timeout\",\"type\":\"internal_server_error\"}}"), 504, cause: cause);

        Assert.Same(cause, error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for completely invalid response", Coverage = UpstreamCoverage.Covered)]
    public void InvalidObjectBecomesAResponseError()
    {
        var response = Parse("{\"invalidField\":\"value\"}");
        var error = ResponseError(response, 500);

        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Equal(500, error.StatusCode);
        Assert.Same(response, error.Response);
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for missing error field", Coverage = UpstreamCoverage.Covered)]
    public void MissingErrorFieldBecomesAResponseError()
    {
        var response = Parse("{\"data\":\"some data\"}");
        var error = ResponseError(response, 500, "Custom error message");

        Assert.Equal("Invalid error response format: Custom error message", error.Message);
        Assert.Same(response, error.Response);
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for null response", Coverage = UpstreamCoverage.Covered)]
    public void NullResponseBecomesAResponseError()
    {
        var error = ResponseError(null, 500);

        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Null(error.Response);
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for string response", Coverage = UpstreamCoverage.Covered)]
    public void StringResponseBecomesAResponseError()
    {
        const string response = "Error string";
        var error = ResponseError(response, 500);

        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Same(response, error.Response);
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Malformed responses::should create GatewayResponseError for array response", Coverage = UpstreamCoverage.Covered)]
    public void ArrayResponseBecomesAResponseError()
    {
        var response = Parse("[\"error\",\"array\"]");
        var error = ResponseError(response, 500);

        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Same(response, error.Response);
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Object parameter validation::should use default defaultMessage when not provided", Coverage = UpstreamCoverage.Covered)]
    public void MissingDefaultMessageUsesTheStandardText()
    {
        var response = Parse("{\"invalidField\":\"value\"}");
        var error = ResponseError(response, 500);

        Assert.Equal("Invalid error response format: Gateway request failed", error.Message);
        Assert.Same(response, error.Response);
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Object parameter validation::should handle undefined cause", Coverage = UpstreamCoverage.Covered)]
    public void OmittedCauseStaysUnset()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Test error\",\"type\":\"authentication_error\"}}"), 401, cause: null);

        Assert.Null(error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Complex scenarios::should handle model_not_found with missing param field", Coverage = UpstreamCoverage.Covered)]
    public void ModelNotFoundWithoutParamLeavesModelIdUnset()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Model not found\",\"type\":\"model_not_found\"}}"), 404);

        var missing = Assert.IsType<GatewayModelNotFoundError>(error);
        Assert.Null(missing.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Complex scenarios::should handle response with extra fields", Coverage = UpstreamCoverage.Covered)]
    public void ExtraErrorFieldsAreIgnored()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Test error\",\"type\":\"authentication_error\",\"code\":\"AUTH_FAILED\",\"param\":null,\"extraField\":\"should be ignored\"},\"metadata\":\"should be ignored\"}"), 401);

        var auth = Assert.IsType<GatewayAuthenticationError>(error);
        Assert.Contains("No authentication provided", auth.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::Complex scenarios::should preserve error properties correctly", Coverage = UpstreamCoverage.Covered)]
    public void RateLimitMappingKeepsMessageStatusCauseAndType()
    {
        var cause = new InvalidOperationException("Type error");
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Rate limit hit\",\"type\":\"rate_limit_exceeded\"}}"), 429, "Fallback message", cause);

        var limit = Assert.IsType<GatewayRateLimitError>(error);
        Assert.Equal("Rate limit hit", limit.Message);
        Assert.Equal(429, limit.StatusCode);
        Assert.Same(cause, limit.Cause);
        Assert.Equal("GatewayRateLimitError", limit.Name);
        Assert.Equal("rate_limit_exceeded", limit.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should include generationId in error when present in response", Coverage = UpstreamCoverage.Covered)]
    public void InternalErrorKeepsGenerationId()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Internal server error\",\"type\":\"internal_server_error\"},\"generationId\":\"gen_01ABC123XYZ\"}"), 500);

        Assert.IsType<GatewayInternalServerError>(error);
        Assert.Equal("gen_01ABC123XYZ", error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should include generationId in authentication error", Coverage = UpstreamCoverage.Covered)]
    public void AuthenticationErrorKeepsGenerationId()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Invalid API key\",\"type\":\"authentication_error\"},\"generationId\":\"gen_01AUTH456\"}"), 401, authMethod: "api-key");

        Assert.IsType<GatewayAuthenticationError>(error);
        Assert.Equal("gen_01AUTH456", error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should include generationId in rate limit error", Coverage = UpstreamCoverage.Covered)]
    public void RateLimitErrorKeepsGenerationId()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Rate limit exceeded\",\"type\":\"rate_limit_exceeded\"},\"generationId\":\"gen_01RATE789\"}"), 429);

        Assert.IsType<GatewayRateLimitError>(error);
        Assert.Equal("gen_01RATE789", error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should include generationId in model not found error", Coverage = UpstreamCoverage.Covered)]
    public void ModelNotFoundErrorKeepsGenerationId()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Model not found\",\"type\":\"model_not_found\",\"param\":{\"modelId\":\"gpt-5\"}},\"generationId\":\"gen_01MODEL000\"}"), 404);

        Assert.IsType<GatewayModelNotFoundError>(error);
        Assert.Equal("gen_01MODEL000", error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should have undefined generationId when not present in response", Coverage = UpstreamCoverage.Covered)]
    public void MissingGenerationIdStaysUnset()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"message\":\"Some error\",\"type\":\"internal_server_error\"}}"), 500);

        Assert.Null(error.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::generationId support::should extract generationId from malformed response when possible", Coverage = UpstreamCoverage.Covered)]
    public void MalformedResponseStillExposesGenerationId()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"invalidField\":\"value\",\"generationId\":\"gen_01MALFORMED\"}"), 500);

        var responseError = Assert.IsType<GatewayResponseError>(error);
        Assert.Equal("gen_01MALFORMED", responseError.GenerationId);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::authentication_error with authMethod context::should create contextual error for API key authentication failure", Coverage = UpstreamCoverage.Covered)]
    public void ApiKeyAuthMethodSelectsTheKeyHint()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"type\":\"authentication_error\",\"message\":\"Invalid API key\"}}"), 401, authMethod: "api-key");

        Assert.IsType<GatewayAuthenticationError>(error);
        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::authentication_error with authMethod context::should create contextual error for OIDC authentication failure", Coverage = UpstreamCoverage.Covered)]
    public void OidcAuthMethodSelectsTheOidcHint()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"type\":\"authentication_error\",\"message\":\"Invalid OIDC token\"}}"), 401, authMethod: "oidc");

        Assert.IsType<GatewayAuthenticationError>(error);
        Assert.Contains("Invalid OIDC token", error.Message);
        Assert.Contains("npx vercel link", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/create-gateway-error.test.ts::authentication_error with authMethod context::should create contextual error without authMethod context", Coverage = UpstreamCoverage.Covered)]
    public void MissingAuthMethodSelectsTheNoCredentialHint()
    {
        var error = GatewayErrors.CreateFromResponse(Parse("{\"error\":{\"type\":\"authentication_error\",\"message\":\"Authentication failed\"}}"), 401);

        Assert.IsType<GatewayAuthenticationError>(error);
        Assert.Contains("No authentication provided", error.Message);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > timeout error detection::should detect error with UND_ERR_HEADERS_TIMEOUT code", Coverage = UpstreamCoverage.Covered)]
    public void HeadersTimeoutBecomesAGatewayTimeout()
    {
        var result = GatewayErrors.AsGatewayError(new GatewayCodedException("Request timeout", "UND_ERR_HEADERS_TIMEOUT"));

        Assert.True(GatewayTimeoutError.IsInstance(result));
        Assert.Contains("Request timeout", result.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > timeout error detection::should detect error with UND_ERR_BODY_TIMEOUT code", Coverage = UpstreamCoverage.Covered)]
    public void BodyTimeoutBecomesAGatewayTimeout()
    {
        var result = GatewayErrors.AsGatewayError(new GatewayCodedException("Body timeout", "UND_ERR_BODY_TIMEOUT"));

        Assert.True(GatewayTimeoutError.IsInstance(result));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > timeout error detection::should detect error with UND_ERR_CONNECT_TIMEOUT code", Coverage = UpstreamCoverage.Covered)]
    public void ConnectTimeoutBecomesAGatewayTimeout()
    {
        var result = GatewayErrors.AsGatewayError(new GatewayCodedException("Connect timeout", "UND_ERR_CONNECT_TIMEOUT"));

        Assert.True(GatewayTimeoutError.IsInstance(result));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > non-timeout errors::should not treat network errors as timeout errors", Coverage = UpstreamCoverage.Covered)]
    public void NetworkErrorsAreResponseErrors()
    {
        var result = GatewayErrors.AsGatewayError(new Exception("Network error"));

        Assert.False(GatewayTimeoutError.IsInstance(result));
        Assert.True(GatewayResponseError.IsInstance(result));
        Assert.Contains("Gateway request failed: Network error", result.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > non-timeout errors::should not treat connection errors as timeout errors", Coverage = UpstreamCoverage.Covered)]
    public void ConnectionRefusedIsNotATimeout()
    {
        var result = GatewayErrors.AsGatewayError(new GatewayCodedException("Connection refused", "ECONNREFUSED"));

        Assert.False(GatewayTimeoutError.IsInstance(result));
        Assert.True(GatewayResponseError.IsInstance(result));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > non-timeout errors::should pass through existing GatewayError instances", Coverage = UpstreamCoverage.Covered)]
    public void ExistingGatewayErrorsPassThrough()
    {
        var existing = GatewayTimeoutError.CreateTimeoutError("existing timeout");
        var result = GatewayErrors.AsGatewayError(existing);

        Assert.Same(existing, result);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > non-timeout errors::should handle non-Error objects", Coverage = UpstreamCoverage.Covered)]
    public void NonExceptionObjectsAreResponseErrors()
    {
        var result = GatewayErrors.AsGatewayError(new Dictionary<string, string> { ["message"] = "timeout occurred" });

        Assert.False(GatewayTimeoutError.IsInstance(result));
        Assert.True(GatewayResponseError.IsInstance(result));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > non-timeout errors::should handle null", Coverage = UpstreamCoverage.Covered)]
    public void NullBecomesAResponseError()
    {
        var result = GatewayErrors.AsGatewayError(null);

        Assert.False(GatewayTimeoutError.IsInstance(result));
        Assert.True(GatewayResponseError.IsInstance(result));
        Assert.Contains("Unknown Gateway error", result.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > non-timeout errors::should handle undefined", Coverage = UpstreamCoverage.Covered)]
    public void UndefinedBecomesAResponseError()
    {
        var result = GatewayErrors.AsGatewayError(GatewayErrors.Undefined);

        Assert.False(GatewayTimeoutError.IsInstance(result));
        Assert.True(GatewayResponseError.IsInstance(result));
        Assert.Contains("Unknown Gateway error", result.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > error properties::should preserve the original error as cause", Coverage = UpstreamCoverage.Covered)]
    public void TimeoutKeepsTheOriginalErrorAsCause()
    {
        var original = new GatewayCodedException("timeout error", "UND_ERR_HEADERS_TIMEOUT");
        var result = GatewayErrors.AsGatewayError(original);

        Assert.Same(original, result.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > error properties::should set correct status code for timeout errors", Coverage = UpstreamCoverage.Covered)]
    public void TimeoutStatusCodeIs408()
    {
        var result = GatewayErrors.AsGatewayError(new GatewayCodedException("timeout", "UND_ERR_HEADERS_TIMEOUT"));

        Assert.Equal(408, result.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > error properties::should have correct error type", Coverage = UpstreamCoverage.Covered)]
    public void TimeoutTypeIsTimeoutError()
    {
        var result = GatewayErrors.AsGatewayError(new GatewayCodedException("timeout", "UND_ERR_HEADERS_TIMEOUT"));

        Assert.Equal("timeout_error", result.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > APICallError with timeout cause::should detect timeout when APICallError has UND_ERR_HEADERS_TIMEOUT in cause", Coverage = UpstreamCoverage.Covered)]
    public void ApiCallWithTimeoutCauseBecomesATimeout()
    {
        var timeout = new GatewayCodedException("Request timeout", "UND_ERR_HEADERS_TIMEOUT");
        var apiCall = new GatewayApiCallException("Cannot connect to API: Request timeout", cause: timeout);
        var result = GatewayErrors.AsGatewayError(apiCall);

        Assert.True(GatewayTimeoutError.IsInstance(result));
        Assert.Contains("Gateway request timed out", result.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > APICallError with timeout cause::should not treat APICallError as timeout if cause is not timeout-related", Coverage = UpstreamCoverage.Covered)]
    public void ApiCallWithANetworkCauseIsNotATimeout()
    {
        var apiCall = new GatewayApiCallException(
            "Cannot connect to API: Network connection failed",
            500,
            "{\"error\":{\"message\":\"Internal error\",\"type\":\"internal_error\"}}",
            cause: new Exception("Network connection failed"));
        var result = GatewayErrors.AsGatewayError(apiCall);

        Assert.False(GatewayTimeoutError.IsInstance(result));
        Assert.IsType<GatewayInternalServerError>(result);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/as-gateway-error.test.ts::asGatewayError > APICallError with timeout cause::should preserve retryability for transport errors after response headers", Coverage = UpstreamCoverage.Covered)]
    public void SuccessfulStatusTransportErrorsStayRetryable()
    {
        var apiCall = new GatewayApiCallException("Failed to process successful response", 200, isRetryable: true);
        var result = GatewayErrors.AsGatewayError(apiCall);

        var responseError = Assert.IsType<GatewayResponseError>(result);
        Assert.True(responseError.IsRetryable);
        Assert.Equal(200, responseError.StatusCode);
        Assert.Same(apiCall, responseError.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::gateway headers::should export the correct auth method header name", Coverage = UpstreamCoverage.Covered)]
    public void AuthMethodHeaderNameMatchesTheGateway()
    {
        Assert.Equal("ai-gateway-auth-method", GatewayHeaders.AuthMethod);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::gateway headers::should export the correct Vercel AI Gateway team header name", Coverage = UpstreamCoverage.Covered)]
    public void TeamHeaderNameMatchesTheGateway()
    {
        Assert.Equal("x-vercel-ai-gateway-team", GatewayHeaders.Team);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > valid authentication methods::should parse \"api-key\" auth method", Coverage = UpstreamCoverage.Covered)]
    public void ParsesApiKeyAuthMethod()
    {
        Assert.Equal("api-key", GatewayErrors.ParseAuthMethod(Auth("api-key")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > valid authentication methods::should parse \"oidc\" auth method", Coverage = UpstreamCoverage.Covered)]
    public void ParsesOidcAuthMethod()
    {
        Assert.Equal("oidc", GatewayErrors.ParseAuthMethod(Auth("oidc")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > valid authentication methods::should handle headers with other fields present", Coverage = UpstreamCoverage.Covered)]
    public void ParsesAuthMethodAlongsideOtherHeaders()
    {
        var headers = new Dictionary<string, string?>
        {
            ["authorization"] = "Bearer token",
            ["content-type"] = "application/json",
            [GatewayHeaders.AuthMethod] = "api-key",
            ["user-agent"] = "test-agent",
        };

        Assert.Equal("api-key", GatewayErrors.ParseAuthMethod(headers));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for invalid auth method string", Coverage = UpstreamCoverage.Covered)]
    public void RejectsAnUnknownAuthMethod()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth("invalid-method")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for empty string", Coverage = UpstreamCoverage.Covered)]
    public void RejectsAnEmptyAuthMethod()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth(string.Empty)));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for numeric value", Coverage = UpstreamCoverage.Covered)]
    public void RejectsANumericAuthMethod()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth("123")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for boolean-like strings", Coverage = UpstreamCoverage.Covered)]
    public void RejectsABooleanAuthMethod()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth("true")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for case-sensitive variations", Coverage = UpstreamCoverage.Covered)]
    public void AuthMethodIsCaseSensitive()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth("API-KEY")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > invalid authentication methods::should return undefined for OIDC case variations", Coverage = UpstreamCoverage.Covered)]
    public void OidcAuthMethodIsCaseSensitive()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth("OIDC")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > missing or undefined headers::should return undefined when header is missing", Coverage = UpstreamCoverage.Covered)]
    public void MissingAuthHeaderReturnsNull()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(new Dictionary<string, string?> { ["authorization"] = "Bearer token" }));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > missing or undefined headers::should return undefined when header is undefined", Coverage = UpstreamCoverage.Covered)]
    public void NullAuthHeaderReturnsNull()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth(null)));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > missing or undefined headers::should return undefined when headers object is empty", Coverage = UpstreamCoverage.Covered)]
    public void EmptyHeadersReturnNull()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(new Dictionary<string, string?>()));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > edge cases::should return undefined for whitespace-only strings", Coverage = UpstreamCoverage.Covered)]
    public void WhitespaceAuthMethodIsRejected()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth("   ")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > edge cases::should return undefined for auth methods with extra whitespace", Coverage = UpstreamCoverage.Covered)]
    public void PaddedAuthMethodIsRejected()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth(" api-key ")));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/parse-auth-method.test.ts::parseAuthMethod > edge cases::should handle null values", Coverage = UpstreamCoverage.Covered)]
    public void NullAuthValueIsRejected()
    {
        Assert.Null(GatewayErrors.ParseAuthMethod(Auth(null)));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is available::should return error.data when successfully parsed by AI SDK", Coverage = UpstreamCoverage.Covered)]
    public void ExtractPrefersParsedData()
    {
        var data = Parse("{\"error\":{\"message\":\"Parsed error\",\"type\":\"authentication_error\"}}");
        var error = GatewayApiCallException.WithData("Request failed", 401, data, "{\"different\":\"data\"}");

        Assert.Same(data, GatewayErrors.ExtractApiCallResponse(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is available::should return error.data even when it is null", Coverage = UpstreamCoverage.Covered)]
    public void ExtractReturnsNullDataInsteadOfTheBody()
    {
        var error = GatewayApiCallException.WithData("Request failed", 500, null, "{\"fallback\":\"data\"}");

        Assert.Null(GatewayErrors.ExtractApiCallResponse(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is available::should return error.data even when it is an empty object", Coverage = UpstreamCoverage.Covered)]
    public void ExtractReturnsAnEmptyDataObject()
    {
        var data = new JsonObject();
        var error = GatewayApiCallException.WithData("Request failed", 400, data, "{\"fallback\":\"data\"}");

        Assert.Same(data, GatewayErrors.ExtractApiCallResponse(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is undefined::should parse and return responseBody as JSON when valid", Coverage = UpstreamCoverage.Covered)]
    public void ExtractParsesAJsonBody()
    {
        const string body = "{\"ferror\":{\"message\":\"Malformed error\",\"type\":\"model_not_found\"}}";
        var error = new GatewayApiCallException("Request failed", 404, body);
        var result = Assert.IsAssignableFrom<JsonNode>(GatewayErrors.ExtractApiCallResponse(error));

        Assert.True(JsonNode.DeepEquals(Parse(body), result));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is undefined::should return raw responseBody when JSON parsing fails", Coverage = UpstreamCoverage.Covered)]
    public void ExtractReturnsInvalidJsonUnchanged()
    {
        const string body = "This is not valid JSON";
        var error = new GatewayApiCallException("Request failed", 500, body);

        Assert.Same(body, GatewayErrors.ExtractApiCallResponse(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is undefined::should handle HTML error responses", Coverage = UpstreamCoverage.Covered)]
    public void ExtractReturnsHtmlUnchanged()
    {
        const string body = "<html><body><h1>500 Internal Server Error</h1></body></html>";
        var error = new GatewayApiCallException("Request failed", 500, body);

        Assert.Same(body, GatewayErrors.ExtractApiCallResponse(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is undefined::should handle empty string responseBody", Coverage = UpstreamCoverage.Covered)]
    public void ExtractReturnsAnEmptyBody()
    {
        var error = new GatewayApiCallException("Request failed", 502, string.Empty);

        Assert.Equal(string.Empty, GatewayErrors.ExtractApiCallResponse(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is undefined::should handle malformed JSON gracefully", Coverage = UpstreamCoverage.Covered)]
    public void ExtractReturnsMalformedJsonUnchanged()
    {
        const string body = "{\"incomplete\": json";
        var error = new GatewayApiCallException("Request failed", 500, body);

        Assert.Same(body, GatewayErrors.ExtractApiCallResponse(error));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when error.data is undefined::should parse complex nested JSON structures", Coverage = UpstreamCoverage.Covered)]
    public void ExtractParsesNestedJson()
    {
        const string body = "{\"error\":{\"message\":\"Complex error\",\"type\":\"validation_error\",\"details\":{\"field\":\"prompt\",\"issues\":[{\"code\":\"too_long\",\"message\":\"Prompt exceeds maximum length\"},{\"code\":\"invalid_format\",\"message\":\"Contains invalid characters\"}]}},\"metadata\":{\"requestId\":\"12345\",\"timestamp\":\"2024-01-01T00:00:00Z\"}}";
        var error = new GatewayApiCallException("Request failed", 400, body);
        var result = Assert.IsAssignableFrom<JsonNode>(GatewayErrors.ExtractApiCallResponse(error));

        Assert.True(JsonNode.DeepEquals(Parse(body), result));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when responseBody is not available::should return empty object when both data and responseBody are undefined", Coverage = UpstreamCoverage.Covered)]
    public void ExtractReturnsAnEmptyObjectWhenTheBodyIsMissing()
    {
        var error = new GatewayApiCallException("Request failed", 500, null);
        var result = Assert.IsType<JsonObject>(GatewayErrors.ExtractApiCallResponse(error));

        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > when responseBody is not available::should return empty object when responseBody is null", Coverage = UpstreamCoverage.Covered)]
    public void ExtractReturnsAnEmptyObjectWhenTheBodyIsNull()
    {
        var error = new GatewayApiCallException("Request failed", 500, responseBody: null);
        var result = Assert.IsType<JsonObject>(GatewayErrors.ExtractApiCallResponse(error));

        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > edge cases::should handle numeric responseBody", Coverage = UpstreamCoverage.Covered)]
    public void ExtractParsesANumericBody()
    {
        var error = new GatewayApiCallException("Request failed", 500, "404");

        Assert.Equal(404, Assert.IsType<int>(GatewayErrors.ExtractApiCallResponse(error)));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > edge cases::should handle boolean responseBody", Coverage = UpstreamCoverage.Covered)]
    public void ExtractParsesABooleanBody()
    {
        var error = new GatewayApiCallException("Request failed", 500, "true");

        Assert.True(Assert.IsType<bool>(GatewayErrors.ExtractApiCallResponse(error)));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/errors/extract-api-call-response.test.ts::extractResponseFromAPICallError > edge cases::should handle array responseBody", Coverage = UpstreamCoverage.Covered)]
    public void ExtractParsesAnArrayBody()
    {
        const string body = "[\"error1\",\"error2\",\"error3\"]";
        var error = new GatewayApiCallException("Request failed", 400, body);
        var result = Assert.IsAssignableFrom<JsonNode>(GatewayErrors.ExtractApiCallResponse(error));

        Assert.True(JsonNode.DeepEquals(Parse(body), result));
    }

    private static JsonNode Parse(string json)
    {
        return JsonNode.Parse(json) ?? new JsonObject();
    }

    private static GatewayResponseError ResponseError(object? response, int statusCode, string? defaultMessage = null)
    {
        return Assert.IsType<GatewayResponseError>(GatewayErrors.CreateFromResponse(response, statusCode, defaultMessage));
    }

    private static Dictionary<string, string?> Auth(string? value)
    {
        return new Dictionary<string, string?> { [GatewayHeaders.AuthMethod] = value };
    }
}
