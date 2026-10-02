// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Gateway;

/// <summary>Header names the AI Gateway reads on every V4 call.</summary>
public static class GatewayHeaders
{
    /// <summary>Protocol version header. Language, embedding, and image calls send <c>0.0.1</c>.</summary>
    public const string ProtocolVersion = "ai-gateway-protocol-version";

    /// <summary>How the caller authenticated: <c>api-key</c> or <c>oidc</c>.</summary>
    public const string AuthMethod = "ai-gateway-auth-method";

    /// <summary>Vercel team header used by the Gateway.</summary>
    public const string Team = "x-vercel-ai-gateway-team";
}

/// <summary>Builds Gateway errors from HTTP failures and Gateway error payloads.</summary>
public static class GatewayErrors
{
    /// <summary>Stand-in for a missing error value. Maps the same way as <c>null</c>.</summary>
    public static readonly object Undefined = new object();

    /// <summary>Reads <c>ai-gateway-auth-method</c>. Only <c>api-key</c> and <c>oidc</c> are recognized.</summary>
    public static string? ParseAuthMethod(IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null || !headers.TryGetValue(GatewayHeaders.AuthMethod, out var value) || value == null)
        {
            return null;
        }

        if (value == "api-key" || value == "oidc")
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Prefers parsed response data, then a JSON parse of the raw body.
    /// Invalid JSON is returned as the original string. A missing body becomes an empty object.
    /// </summary>
    public static object? ExtractApiCallResponse(GatewayApiCallException error)
    {
        if (error == null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        if (error.HasData)
        {
            return error.Data;
        }

        if (error.ResponseBody == null)
        {
            return new JsonObject();
        }

        try
        {
            var node = JsonNode.Parse(error.ResponseBody);
            if (node is JsonValue value)
            {
                if (value.TryGetValue<bool>(out var boolean))
                {
                    return boolean;
                }

                if (value.TryGetValue<int>(out var integer))
                {
                    return integer;
                }

                if (value.TryGetValue<long>(out var integer64))
                {
                    return integer64;
                }

                if (value.TryGetValue<double>(out var number))
                {
                    return number;
                }

                if (value.TryGetValue<string>(out var text))
                {
                    return text;
                }
            }

            return node;
        }
        catch (JsonException)
        {
            return error.ResponseBody;
        }
    }

    /// <summary>Maps a Gateway error payload onto the typed Gateway exception for that <c>error.type</c>.</summary>
    public static GatewayError CreateFromResponse(
        object? response,
        int statusCode,
        string? defaultMessage = null,
        object? cause = null,
        string? authMethod = null,
        bool? isRetryable = null)
    {
        var fallback = string.IsNullOrEmpty(defaultMessage) ? "Gateway request failed" : defaultMessage;
        if (!TryValidate(response, out var parsed, out var validationError))
        {
            return new GatewayResponseError(
                "Invalid error response format: " + fallback,
                statusCode,
                response,
                validationError,
                cause,
                TryReadGenerationId(response),
                isRetryable);
        }

        var message = parsed!.Message;
        var generationId = parsed.GenerationId;
        switch (parsed.Type)
        {
            case "authentication_error":
                return GatewayAuthenticationError.CreateContextual(
                    authMethod == "api-key",
                    authMethod == "oidc",
                    statusCode,
                    cause,
                    generationId);
            case "invalid_request_error":
                return new GatewayInvalidRequestError(message, statusCode, cause, generationId);
            case "rate_limit_exceeded":
                return new GatewayRateLimitError(message, statusCode, cause, generationId);
            case "model_not_found":
                return new GatewayModelNotFoundError(message, statusCode, ReadStringField(parsed.Param, "modelId"), cause, generationId);
            case "not_found":
                return new GatewayNotFoundError(message, statusCode, cause, generationId);
            case "internal_server_error":
                return new GatewayInternalServerError(message, statusCode, cause, generationId);
            case "failed_dependency":
                return new GatewayFailedDependencyError(message, statusCode, cause, generationId);
            case "forbidden":
                return new GatewayForbiddenError(message, statusCode, cause, generationId, ReadStringField(parsed.Param, "ruleId"));
            default:
                return new GatewayInternalServerError(message, statusCode, cause, generationId);
        }
    }

    /// <summary>Converts a transport or API failure into a <see cref="GatewayError"/>.</summary>
    public static GatewayError AsGatewayError(object? error, string? authMethod = null)
    {
        if (error is GatewayError gatewayError)
        {
            return gatewayError;
        }

        if (IsTimeoutError(error))
        {
            var original = error is Exception exception ? exception.Message : "Unknown error";
            return GatewayTimeoutError.CreateTimeoutError(original, error);
        }

        if (error is ApiException providerError)
        {
            return AsGatewayError(GatewayApiCallException.FromApiException(providerError), authMethod);
        }

        if (error is GatewayApiCallException apiCall)
        {
            if (apiCall.Cause != null && IsTimeoutError(apiCall.Cause))
            {
                return GatewayTimeoutError.CreateTimeoutError(apiCall.Message, apiCall);
            }

            bool? retryable = null;
            if (apiCall.IsRetryable && (apiCall.StatusCode == null || apiCall.StatusCode < 400))
            {
                retryable = true;
            }

            return CreateFromResponse(
                ExtractApiCallResponse(apiCall),
                apiCall.StatusCode ?? 500,
                "Gateway request failed",
                apiCall,
                authMethod,
                retryable);
        }

        var defaultMessage = error is Exception generic
            ? "Gateway request failed: " + generic.Message
            : "Unknown Gateway error";
        return CreateFromResponse(new JsonObject(), 500, defaultMessage, error, authMethod);
    }

    private static bool IsTimeoutError(object? error)
    {
        if (error is not Exception exception)
        {
            return false;
        }

        var code = (exception as GatewayCodedException)?.Code;
        if (code == null && exception.Data.Contains("code"))
        {
            code = exception.Data["code"] as string;
        }

        return code == "UND_ERR_HEADERS_TIMEOUT"
            || code == "UND_ERR_BODY_TIMEOUT"
            || code == "UND_ERR_CONNECT_TIMEOUT";
    }

    private static bool TryValidate(object? response, out ParsedGatewayError? parsed, out GatewayValidationError validationError)
    {
        parsed = null;
        var obj = AsObject(response);
        if (obj == null)
        {
            validationError = new GatewayValidationError("Expected an object with an error message.");
            return false;
        }

        if (obj["error"] is not JsonObject error)
        {
            validationError = new GatewayValidationError("Missing error object.");
            return false;
        }

        if (error["message"] is not JsonValue messageValue || !messageValue.TryGetValue<string>(out var message))
        {
            validationError = new GatewayValidationError("error.message must be a string.");
            return false;
        }

        string? type = null;
        if (error["type"] is JsonNode typeNode)
        {
            if (typeNode is not JsonValue typeValue || !typeValue.TryGetValue<string>(out type))
            {
                validationError = new GatewayValidationError("error.type must be a string.");
                return false;
            }
        }

        if (error["code"] is JsonNode codeNode && !IsStringOrNumber(codeNode))
        {
            validationError = new GatewayValidationError("error.code must be a string or number.");
            return false;
        }

        string? generationId = null;
        if (obj["generationId"] is JsonNode generationNode)
        {
            if (generationNode is not JsonValue generationValue || !generationValue.TryGetValue<string>(out generationId))
            {
                validationError = new GatewayValidationError("generationId must be a string.");
                return false;
            }
        }

        validationError = new GatewayValidationError(string.Empty);
        parsed = new ParsedGatewayError(message, type, error["param"], generationId);
        return true;
    }

    private static bool IsStringOrNumber(JsonNode node)
    {
        if (node is not JsonValue value)
        {
            return false;
        }

        if (value.TryGetValue<string>(out _))
        {
            return true;
        }

        return value.TryGetValue<double>(out _);
    }

    private static string? TryReadGenerationId(object? response)
    {
        var obj = AsObject(response);
        if (obj?["generationId"] is JsonValue value && value.TryGetValue<string>(out var generationId))
        {
            return generationId;
        }

        return null;
    }

    private static string? ReadStringField(JsonNode? param, string name)
    {
        if (param is JsonObject obj && obj[name] is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }

    private static JsonObject? AsObject(object? response)
    {
        switch (response)
        {
            case JsonObject obj:
                return obj;
            case JsonElement element when element.ValueKind == JsonValueKind.Object:
                return JsonNode.Parse(element.GetRawText()) as JsonObject;
            case JsonNode node:
                return node as JsonObject;
            default:
                return null;
        }
    }

    private sealed class ParsedGatewayError
    {
        public ParsedGatewayError(string message, string? type, JsonNode? param, string? generationId)
        {
            Message = message;
            Type = type;
            Param = param;
            GenerationId = generationId;
        }

        public string Message { get; }

        public string? Type { get; }

        public JsonNode? Param { get; }

        public string? GenerationId { get; }
    }
}

/// <summary>Schema failure while reading a Gateway error payload.</summary>
public sealed class GatewayValidationError
{
    /// <summary>Creates a validation error.</summary>
    public GatewayValidationError(string message)
    {
        Message = message ?? string.Empty;
    }

    /// <summary>What was wrong with the payload.</summary>
    public string Message { get; }
}

/// <summary>An exception that carries an undici-style <c>code</c>, such as <c>UND_ERR_HEADERS_TIMEOUT</c>.</summary>
public sealed class GatewayCodedException : Exception
{
    /// <summary>Creates an exception with a transport code.</summary>
    public GatewayCodedException(string message, string code)
        : base(message)
    {
        Code = code ?? string.Empty;
    }

    /// <summary>Transport code.</summary>
    public string Code { get; }
}

/// <summary>
/// Failed Gateway HTTP call. This is the .NET stand-in for the AI SDK <c>APICallError</c>
/// that <see cref="GatewayErrors.AsGatewayError"/> understands.
/// </summary>
public sealed class GatewayApiCallException : Exception
{
#pragma warning disable CS8604
    private GatewayApiCallException(string message, int? statusCode, string? responseBody, object? data, bool hasData, bool isRetryable, object? cause, string? url)
        : base(message, cause as Exception)
#pragma warning restore CS8604
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Data = data;
        HasData = hasData;
        IsRetryable = isRetryable;
        Cause = cause;
        Url = url;
    }

    /// <summary>Creates a call error whose response data has not been parsed.</summary>
    public GatewayApiCallException(string message, int? statusCode = null, string? responseBody = null, bool isRetryable = false, object? cause = null, string? url = null)
        : this(message, statusCode, responseBody, null, false, isRetryable, cause, url)
    {
    }

    /// <summary>Creates a call error that already has parsed response data. <paramref name="data"/> may be null.</summary>
    public static GatewayApiCallException WithData(string message, int? statusCode, object? data, string? responseBody = null, bool isRetryable = false, object? cause = null, string? url = null)
    {
        return new GatewayApiCallException(message ?? string.Empty, statusCode, responseBody, data, true, isRetryable, cause, url);
    }

    /// <summary>Wraps an SDK <see cref="ApiException"/> so Gateway error mapping can read its body.</summary>
    public static GatewayApiCallException FromApiException(ApiException exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return new GatewayApiCallException(exception.Message, exception.StatusCode, exception.ResponseBody, null, false, false, exception, null);
    }

    /// <summary>HTTP status, when the transport received one.</summary>
    public int? StatusCode { get; }

    /// <summary>Raw response body.</summary>
    public string? ResponseBody { get; }

    /// <summary>Parsed response data. Only meaningful when <see cref="HasData"/> is true.</summary>
    public new object? Data { get; }

    /// <summary>True when <see cref="Data"/> was supplied, including when that value is null.</summary>
    public bool HasData { get; }

    /// <summary>Whether the AI SDK marked the transport failure as retryable.</summary>
    public bool IsRetryable { get; }

    /// <summary>Underlying failure.</summary>
    public object? Cause { get; }

    /// <summary>Request URL.</summary>
    public string? Url { get; }
}

/// <summary>Base type for errors produced by the AI Gateway.</summary>
public abstract class GatewayError : AiSdkException
{
    /// <summary>Marker shared by every Gateway error.</summary>
    public new const string Marker = "vercel.ai.gateway.error";

    /// <summary>Creates a Gateway error.</summary>
#pragma warning disable CS8604 // AiSdkException annotates the inner exception as non-null; a missing cause is a null inner exception.
    protected GatewayError(string message, int statusCode, object? cause, string? generationId, bool? isRetryable)
        : base(FormatMessage(message, generationId), cause as Exception)
#pragma warning restore CS8604
    {
        StatusCode = statusCode;
        Cause = cause;
        GenerationId = generationId;
        IsRetryable = isRetryable ?? (statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500);
    }

    /// <summary>Stable error name.</summary>
    public abstract new string Name { get; }

    /// <summary>Gateway <c>error.type</c> value.</summary>
    public abstract string Type { get; }

    /// <summary>Type-specific marker used by <see cref="HasMarker(string)"/>.</summary>
    protected abstract string TypeMarker { get; }

    /// <summary>HTTP status associated with the failure.</summary>
    public int StatusCode { get; }

    /// <summary>Underlying failure. May be an <see cref="Exception"/> or another object.</summary>
    public new object? Cause { get; }

    /// <summary>Gateway generation id, when the response included one.</summary>
    public string? GenerationId { get; }

    /// <summary>Whether the caller should retry the request.</summary>
    public bool IsRetryable { get; }

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayError;
    }

    /// <summary>True when <paramref name="error"/> carries the Gateway error marker.</summary>
    public static bool HasMarker(object? error)
    {
        return error is GatewayError;
    }

    /// <summary>True when this error carries <paramref name="marker"/>.</summary>
    public bool HasMarker(string marker)
    {
        return marker == Marker || marker == TypeMarker;
    }

    private static string FormatMessage(string message, string? generationId)
    {
        if (string.IsNullOrEmpty(generationId))
        {
            return message ?? string.Empty;
        }

        return (message ?? string.Empty) + " [" + generationId + "]";
    }
}

/// <summary>Authentication failed for an API key or OIDC token.</summary>
public sealed class GatewayAuthenticationError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayAuthenticationError";

    /// <summary>Creates an authentication error.</summary>
    public GatewayAuthenticationError(string message = "Authentication failed", int statusCode = 401, object? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Name => "GatewayAuthenticationError";

    /// <inheritdoc />
    public override string Type => "authentication_error";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayAuthenticationError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayAuthenticationError;
    }

    /// <summary>Builds the credential hint for an authentication failure.</summary>
    public static GatewayAuthenticationError CreateContextual(bool apiKeyProvided, bool oidcTokenProvided, int statusCode = 401, object? cause = null, string? generationId = null)
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

/// <summary>The Gateway rejected the request as invalid.</summary>
public sealed class GatewayInvalidRequestError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayInvalidRequestError";

    /// <summary>Creates an invalid-request error.</summary>
    public GatewayInvalidRequestError(string message = "Invalid request", int statusCode = 400, object? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Name => "GatewayInvalidRequestError";

    /// <inheritdoc />
    public override string Type => "invalid_request_error";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayInvalidRequestError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayInvalidRequestError;
    }
}

/// <summary>The Gateway rate limit was exceeded.</summary>
public sealed class GatewayRateLimitError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayRateLimitError";

    /// <summary>Creates a rate-limit error.</summary>
    public GatewayRateLimitError(string message = "Rate limit exceeded", int statusCode = 429, object? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Name => "GatewayRateLimitError";

    /// <inheritdoc />
    public override string Type => "rate_limit_exceeded";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayRateLimitError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayRateLimitError;
    }
}

/// <summary>The requested model is not available through the Gateway.</summary>
public sealed class GatewayModelNotFoundError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayModelNotFoundError";

    /// <summary>Creates a model-not-found error.</summary>
    public GatewayModelNotFoundError(string message = "Model not found", int statusCode = 404, string? modelId = null, object? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
        ModelId = modelId;
    }

    /// <inheritdoc />
    public override string Name => "GatewayModelNotFoundError";

    /// <inheritdoc />
    public override string Type => "model_not_found";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>Model id from the error <c>param</c>, when it was a string.</summary>
    public string? ModelId { get; }

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayModelNotFoundError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayModelNotFoundError;
    }
}

/// <summary>The requested Gateway resource does not exist.</summary>
public sealed class GatewayNotFoundError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayNotFoundError";

    /// <summary>Creates a not-found error.</summary>
    public GatewayNotFoundError(string message = "Resource not found", int statusCode = 404, object? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Name => "GatewayNotFoundError";

    /// <inheritdoc />
    public override string Type => "not_found";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayNotFoundError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayNotFoundError;
    }
}

/// <summary>The Gateway failed while handling the request.</summary>
public sealed class GatewayInternalServerError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayInternalServerError";

    /// <summary>Creates an internal-server error.</summary>
    public GatewayInternalServerError(string message = "Internal server error", int statusCode = 500, object? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Name => "GatewayInternalServerError";

    /// <inheritdoc />
    public override string Type => "internal_server_error";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayInternalServerError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayInternalServerError;
    }
}

/// <summary>A dependency required by the request was not available. This is not retryable.</summary>
public sealed class GatewayFailedDependencyError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayFailedDependencyError";

    /// <summary>Creates a failed-dependency error.</summary>
    public GatewayFailedDependencyError(string message = "Failed dependency", int statusCode = 424, object? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Name => "GatewayFailedDependencyError";

    /// <inheritdoc />
    public override string Type => "failed_dependency";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayFailedDependencyError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayFailedDependencyError;
    }
}

/// <summary>A routing rule or other policy rejected the request.</summary>
public sealed class GatewayForbiddenError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayForbiddenError";

    /// <summary>Creates a forbidden error.</summary>
    public GatewayForbiddenError(string message = "Forbidden", int statusCode = 403, object? cause = null, string? generationId = null, string? ruleId = null)
        : base(message, statusCode, cause, generationId, null)
    {
        RuleId = ruleId;
    }

    /// <inheritdoc />
    public override string Name => "GatewayForbiddenError";

    /// <inheritdoc />
    public override string Type => "forbidden";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>Routing rule id from <c>param.ruleId</c>, when that field was a string.</summary>
    public string? RuleId { get; }

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayForbiddenError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayForbiddenError;
    }
}

/// <summary>The Gateway response was not a recognized error payload.</summary>
public sealed class GatewayResponseError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayResponseError";

    /// <summary>Creates a response-parse error.</summary>
    public GatewayResponseError(
        string message = "Invalid response from Gateway",
        int statusCode = 502,
        object? response = null,
        GatewayValidationError? validationError = null,
        object? cause = null,
        string? generationId = null,
        bool? isRetryable = null)
        : base(message, statusCode, cause, generationId, isRetryable)
    {
        Response = response;
        ValidationError = validationError;
    }

    /// <inheritdoc />
    public override string Name => "GatewayResponseError";

    /// <inheritdoc />
    public override string Type => "response_error";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>Original response value that failed validation.</summary>
    public object? Response { get; }

    /// <summary>Why the payload was rejected.</summary>
    public GatewayValidationError? ValidationError { get; }

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayResponseError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayResponseError;
    }
}

/// <summary>The client timed out before the Gateway finished responding.</summary>
public sealed class GatewayTimeoutError : GatewayError
{
    /// <summary>Type marker.</summary>
    public const string TypeMarkerName = "vercel.ai.gateway.error.GatewayTimeoutError";

    /// <summary>Creates a timeout error.</summary>
    public GatewayTimeoutError(string message = "Request timed out", int statusCode = 408, object? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string Name => "GatewayTimeoutError";

    /// <inheritdoc />
    public override string Type => "timeout_error";

    /// <inheritdoc />
    protected override string TypeMarker => TypeMarkerName;

    /// <summary>True when <paramref name="error"/> is a <see cref="GatewayTimeoutError"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayTimeoutError;
    }

    /// <summary>Builds a timeout error that includes the original transport message.</summary>
    public static GatewayTimeoutError CreateTimeoutError(string originalMessage, object? cause = null, int statusCode = 408, string? generationId = null)
    {
        var message = "Gateway request timed out: " + originalMessage + "\n\n    This is a client-side timeout. To resolve this, increase your timeout configuration: https://vercel.com/docs/ai-gateway/capabilities/video-generation#extending-timeouts-for-node.js";
        return new GatewayTimeoutError(message, statusCode, cause, generationId);
    }
}
