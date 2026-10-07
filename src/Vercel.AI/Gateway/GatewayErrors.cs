// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Sockets;
using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Gateway;

/// <summary>Base class for AI Gateway errors. The message includes <c>[generationId]</c> when one was returned.</summary>
public abstract class GatewayError : AiSdkException
{
    /// <summary>Creates a Gateway error.</summary>
    protected GatewayError(string message, int statusCode, Exception? cause, string? generationId, bool? isRetryable)
        : base(generationId == null ? message : message + " [" + generationId + "]", cause ?? new Exception(message))
    {
        StatusCode = statusCode;
        Cause = cause;
        GenerationId = generationId;
        IsRetryable = isRetryable ?? (statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500);
    }

    /// <summary>Gateway error type.</summary>
    public abstract string Type { get; }

    /// <summary>HTTP status.</summary>
    public int StatusCode { get; }

    /// <summary>Underlying failure, when one was supplied.</summary>
    public Exception? Cause { get; }

    /// <summary>Gateway generation id.</summary>
    public string? GenerationId { get; }

    /// <summary>Whether the caller should retry this status.</summary>
    public bool IsRetryable { get; }

    /// <summary>True when <paramref name="error"/> is a Gateway error.</summary>
    public static bool IsInstance(Exception? error)
    {
        return error is GatewayError;
    }
}

/// <summary>Authentication failed.</summary>
public sealed class GatewayAuthenticationError : GatewayError
{
    /// <summary>Creates an authentication error. The default message is <c>Authentication failed</c>.</summary>
    public GatewayAuthenticationError(string? message = null, int statusCode = 401, Exception? cause = null, string? generationId = null)
        : base(message ?? "Authentication failed", statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Type => "authentication_error";

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayAuthenticationError;
    }

    /// <summary>Builds the message for an API key, an OIDC token, or missing credentials.</summary>
    public static GatewayAuthenticationError CreateContextual(bool apiKeyProvided, bool oidcTokenProvided, int statusCode = 401, Exception? cause = null, string? generationId = null)
    {
        string message;
        if (apiKeyProvided)
        {
            message = "AI Gateway authentication failed: Invalid API key or token.\n\nCreate a new API key: https://vercel.com/d?to=%2F%5Bteam%5D%2F%7E%2Fai%2Fapi-keys\n\nProvide an API key or Vercel access token via 'apiKey' option or 'AI_GATEWAY_API_KEY' environment variable.";
        }
        else if (oidcTokenProvided)
        {
            message = "AI Gateway authentication failed: Invalid OIDC token.\n\nRun 'npx vercel link' to link your project, then 'vc env pull' to fetch the token.\n\nAlternatively, use an API key: https://vercel.com/d?to=%2F%5Bteam%5D%2F%7E%2Fai%2Fapi-keys\nor pass a Vercel access token via the 'apiKey' option.";
        }
        else
        {
            message = "AI Gateway authentication failed: No authentication provided.\n\nOption 1 - API key:\nCreate an API key: https://vercel.com/d?to=%2F%5Bteam%5D%2F%7E%2Fai%2Fapi-keys\nProvide via 'apiKey' option or 'AI_GATEWAY_API_KEY' environment variable.\n\nOption 2 - Vercel access token:\nPass a Vercel personal access token or Vercel app access token via the 'apiKey' option.\n\nOption 3 - OIDC token:\nRun 'npx vercel link' to link your project, then 'vc env pull' to fetch the token.";
        }

        return new GatewayAuthenticationError(message, statusCode, cause, generationId);
    }
}

/// <summary>The Gateway rejected the request.</summary>
public sealed class GatewayInvalidRequestError : GatewayError
{
    /// <summary>Creates an invalid-request error.</summary>
    public GatewayInvalidRequestError(string? message = null, int statusCode = 400, Exception? cause = null, string? generationId = null)
        : base(message ?? "Invalid request", statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Type => "invalid_request_error";

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayInvalidRequestError;
    }
}

/// <summary>A Gateway rate limit.</summary>
public sealed class GatewayRateLimitError : GatewayError
{
    /// <summary>Creates a rate-limit error.</summary>
    public GatewayRateLimitError(string? message = null, int statusCode = 429, Exception? cause = null, string? generationId = null)
        : base(message ?? "Rate limit exceeded", statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Type => "rate_limit_exceeded";

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayRateLimitError;
    }
}

/// <summary>The requested model is not available.</summary>
public sealed class GatewayModelNotFoundError : GatewayError
{
    /// <summary>Creates a model-not-found error.</summary>
    public GatewayModelNotFoundError(string? message = null, int statusCode = 404, string? modelId = null, Exception? cause = null, string? generationId = null)
        : base(message ?? "Model not found", statusCode, cause, generationId, null)
    {
        ModelId = modelId;
    }

    /// <inheritdoc />
    public override string Type => "model_not_found";

    /// <summary>Model id from the error <c>param</c>, when it was a <c>{ modelId }</c> object.</summary>
    public string? ModelId { get; }

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayModelNotFoundError;
    }
}

/// <summary>A Gateway resource was not found.</summary>
public sealed class GatewayNotFoundError : GatewayError
{
    /// <summary>Creates a not-found error.</summary>
    public GatewayNotFoundError(string? message = null, int statusCode = 404, Exception? cause = null, string? generationId = null)
        : base(message ?? "Resource not found", statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Type => "not_found";

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayNotFoundError;
    }
}

/// <summary>The Gateway failed internally.</summary>
public sealed class GatewayInternalServerError : GatewayError
{
    /// <summary>Creates an internal-server error.</summary>
    public GatewayInternalServerError(string? message = null, int statusCode = 500, Exception? cause = null, string? generationId = null)
        : base(message ?? "Internal server error", statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Type => "internal_server_error";

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayInternalServerError;
    }
}

/// <summary>A dependency required by the Gateway request was unavailable. Status 424 is not retried.</summary>
public sealed class GatewayFailedDependencyError : GatewayError
{
    /// <summary>Creates a failed-dependency error.</summary>
    public GatewayFailedDependencyError(string? message = null, int statusCode = 424, Exception? cause = null, string? generationId = null)
        : base(message ?? "Failed dependency", statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Type => "failed_dependency";

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayFailedDependencyError;
    }
}

/// <summary>The request was rejected by policy.</summary>
public sealed class GatewayForbiddenError : GatewayError
{
    /// <summary>Creates a forbidden error.</summary>
    public GatewayForbiddenError(string? message = null, int statusCode = 403, Exception? cause = null, string? generationId = null, string? ruleId = null)
        : base(message ?? "Forbidden", statusCode, cause, generationId, null)
    {
        RuleId = ruleId;
    }

    /// <inheritdoc />
    public override string Type => "forbidden";

    /// <summary>Routing rule id, when <c>param.ruleId</c> was a string.</summary>
    public string? RuleId { get; }

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayForbiddenError;
    }
}

/// <summary>The Gateway error body could not be read.</summary>
public sealed class GatewayResponseError : GatewayError
{
    /// <summary>Creates a response error.</summary>
    public GatewayResponseError(string? message = null, int statusCode = 502, JsonElement? response = null, string? validationError = null, Exception? cause = null, string? generationId = null, bool? isRetryable = null)
        : base(message ?? "Invalid response from Gateway", statusCode, cause, generationId, isRetryable)
    {
        Response = response;
        ValidationError = validationError;
    }

    /// <inheritdoc />
    public override string Type => "response_error";

    /// <summary>Raw response body.</summary>
    public JsonElement? Response { get; }

    /// <summary>Why the body was rejected.</summary>
    public string? ValidationError { get; }

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayResponseError;
    }
}

/// <summary>The client timed out before the Gateway responded.</summary>
public sealed class GatewayTimeoutError : GatewayError
{
    /// <summary>Creates a timeout error.</summary>
    public GatewayTimeoutError(string? message = null, int statusCode = 408, Exception? cause = null, string? generationId = null)
        : base(message ?? "Request timed out", statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Type => "timeout_error";

    /// <summary>True when <paramref name="error"/> is this error.</summary>
    public static new bool IsInstance(Exception? error)
    {
        return error is GatewayTimeoutError;
    }

    /// <summary>Wraps a client timeout with the Gateway troubleshooting text.</summary>
    public static GatewayTimeoutError Create(string originalMessage, Exception? cause = null, int statusCode = 408, string? generationId = null)
    {
        var message = "Gateway request timed out: " + originalMessage + "\n\n    This is a client-side timeout. To resolve this, increase your timeout configuration: https://vercel.com/docs/ai-gateway/capabilities/video-generation#extending-timeouts-for-node.js";
        return new GatewayTimeoutError(message, statusCode, cause, generationId);
    }
}

/// <summary>Parses Gateway auth headers and error bodies.</summary>
public static class GatewayErrors
{
    private static readonly JsonElement EmptyObject = JsonSerializer.SerializeToElement(new Dictionary<string, object>());

    /// <summary>Header that carries <c>api-key</c> or <c>oidc</c>.</summary>
    public const string AuthMethodHeader = "ai-gateway-auth-method";

    /// <summary>Header that carries the Vercel AI Gateway team.</summary>
    public const string TeamHeader = "x-vercel-ai-gateway-team";

    /// <summary>Reads <see cref="AuthMethodHeader"/>. Any other value, including a different case, is ignored.</summary>
    public static string? ParseAuthMethod(IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null || !headers.TryGetValue(AuthMethodHeader, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (string.Equals(value, "api-key", StringComparison.Ordinal) || string.Equals(value, "oidc", StringComparison.Ordinal))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Maps any failure to a Gateway error. Gateway errors pass through, transport timeouts become <see cref="GatewayTimeoutError"/>,
    /// and API call errors are read as Gateway error bodies. Maps to <c>asGatewayError</c>.
    /// </summary>
    public static GatewayError AsGatewayError(Exception? error, string? authMethod = null)
    {
        if (error is GatewayError gateway)
        {
            return gateway;
        }

        if (IsTimeout(error))
        {
            return GatewayTimeoutError.Create(error!.Message, error);
        }

        if (error is Util.ApiCallError api)
        {
            if (IsTimeout(api.Cause))
            {
                return GatewayTimeoutError.Create(api.Message, api);
            }

            var retryable = api.IsRetryable && (api.StatusCode == null || api.StatusCode < 400) ? true : (bool?)null;
            return Create(ExtractApiCallResponse(api), api.StatusCode ?? 500, authMethod, "Gateway request failed", api, retryable);
        }

        if (error is ApiException http)
        {
            return FromResponseBody(http.ResponseBody, http.StatusCode, authMethod, http);
        }

        return Create(EmptyObject, 500, authMethod, error == null ? "Unknown Gateway error" : "Gateway request failed: " + error.Message, error);
    }

    /// <summary>
    /// Returns <see cref="Util.ApiCallError.Data"/> when set, else the response body parsed as JSON, else the raw body as a JSON string.
    /// A missing body yields an empty object. Maps to <c>extractApiCallResponse</c>.
    /// </summary>
    public static JsonElement ExtractApiCallResponse(Util.ApiCallError error)
    {
        if (error == null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        if (error.Data != null)
        {
            return error.Data is JsonElement data ? data : JsonSerializer.SerializeToElement(error.Data);
        }

        if (error.ResponseBody == null)
        {
            return EmptyObject;
        }

        try
        {
            return ProviderUtils.SecureJson.Parse(error.ResponseBody);
        }
        catch (JsonException)
        {
            return JsonSerializer.SerializeToElement(error.ResponseBody);
        }
    }

    /// <summary>Maps a Gateway JSON body to a typed error. Invalid bodies become <see cref="GatewayResponseError"/>.</summary>
    public static GatewayError Create(JsonElement response, int statusCode, string? authMethod = null, string defaultMessage = "Gateway request failed", Exception? cause = null, bool? isRetryable = null)
    {
        if (!TryRead(response, out var message, out var type, out var param, out var generationId, out var validationError))
        {
            string? malformedGeneration = null;
            if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("generationId", out var rawGeneration) && rawGeneration.ValueKind == JsonValueKind.String)
            {
                malformedGeneration = rawGeneration.GetString();
            }

            return new GatewayResponseError("Invalid error response format: " + defaultMessage, statusCode, response.ValueKind == JsonValueKind.Undefined ? null : response.Clone(), validationError, cause, malformedGeneration, isRetryable);
        }

        switch (type)
        {
            case "authentication_error":
                return GatewayAuthenticationError.CreateContextual(apiKeyProvided: authMethod == "api-key", oidcTokenProvided: authMethod == "oidc", statusCode, cause, generationId);
            case "invalid_request_error":
                return new GatewayInvalidRequestError(message, statusCode, cause, generationId);
            case "rate_limit_exceeded":
                return new GatewayRateLimitError(message, statusCode, cause, generationId);
            case "model_not_found":
                return new GatewayModelNotFoundError(message, statusCode, ReadStringProperty(param, "modelId"), cause, generationId);
            case "not_found":
                return new GatewayNotFoundError(message, statusCode, cause, generationId);
            case "internal_server_error":
                return new GatewayInternalServerError(message, statusCode, cause, generationId);
            case "failed_dependency":
                return new GatewayFailedDependencyError(message, statusCode, cause, generationId);
            case "forbidden":
                return new GatewayForbiddenError(message, statusCode, cause, generationId, ReadStringProperty(param, "ruleId"));
            default:
                return new GatewayInternalServerError(message, statusCode, cause, generationId);
        }
    }

    /// <summary>Maps an HTTP error body. Non-JSON bodies become <see cref="GatewayResponseError"/>.</summary>
    public static GatewayError FromResponseBody(string? body, int statusCode, string? authMethod, Exception? cause)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return new GatewayResponseError("Invalid error response format: Gateway request failed", statusCode, null, "Response body was empty.", cause, null);
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return Create(document.RootElement, statusCode, authMethod, "Gateway request failed", cause);
        }
        catch (JsonException exception)
        {
            return new GatewayResponseError("Invalid error response format: Gateway request failed", statusCode, null, exception.Message, cause, null);
        }
    }

    private static bool TryRead(JsonElement response, out string message, out string? type, out JsonElement param, out string? generationId, out string validationError)
    {
        message = string.Empty;
        type = null;
        param = default;
        generationId = null;
        validationError = "Gateway error response did not match the expected schema.";
        if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("error", out var error) || error.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!error.TryGetProperty("message", out var messageElement) || messageElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        message = messageElement.GetString() ?? string.Empty;
        if (error.TryGetProperty("type", out var typeElement))
        {
            if (typeElement.ValueKind == JsonValueKind.String)
            {
                type = typeElement.GetString();
            }
            else if (typeElement.ValueKind != JsonValueKind.Null)
            {
                return false;
            }
        }

        if (error.TryGetProperty("param", out var paramElement))
        {
            param = paramElement;
        }

        if (response.TryGetProperty("generationId", out var generationElement) && generationElement.ValueKind == JsonValueKind.String)
        {
            generationId = generationElement.GetString();
        }

        validationError = string.Empty;
        return true;
    }

    private static bool IsTimeout(Exception? error)
    {
        return error is ApiTimeoutException
            || error is TimeoutException
            || (error is SocketException socket && socket.SocketErrorCode == SocketError.TimedOut);
    }

    private static string? ReadStringProperty(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }
}
