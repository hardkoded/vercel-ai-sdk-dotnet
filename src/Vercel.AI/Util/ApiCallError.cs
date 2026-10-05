// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>A failed provider HTTP call, including retry metadata and response headers.</summary>
public class ApiCallError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_APICallError";

    /// <summary>Creates the error.</summary>
    public ApiCallError(
        string message,
        string url,
        object requestBodyValues,
        int? statusCode = null,
        IReadOnlyDictionary<string, string>? responseHeaders = null,
        string? responseBody = null,
        Exception? cause = null,
        bool? isRetryable = null,
        object? data = null)
        : base("AI_APICallError", message, cause)
    {
        Url = url;
        RequestBodyValues = requestBodyValues;
        StatusCode = statusCode;
        ResponseHeaders = responseHeaders;
        ResponseBody = responseBody;
        IsRetryable = isRetryable ?? StreamProviderError.IsRetryableStatusCode(statusCode);
        Data = data;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Request URL.</summary>
    public string Url { get; }

    /// <summary>Request body that was sent.</summary>
    public object RequestBodyValues { get; }

    /// <summary>HTTP status, when the provider responded.</summary>
    public int? StatusCode { get; }

    /// <summary>Response headers. Lookup is case-insensitive.</summary>
    public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }

    /// <summary>Raw response body.</summary>
    public string? ResponseBody { get; }

    /// <summary>Whether the call may be retried.</summary>
    public bool IsRetryable { get; }

    /// <summary>Parsed provider payload.</summary>
    public new object? Data { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }

    /// <summary>Reads a response header, ignoring case.</summary>
    public string? GetHeader(string? name)
    {
        if (ResponseHeaders == null || name == null)
        {
            return null;
        }

        foreach (var header in ResponseHeaders)
        {
            if (string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return header.Value;
            }
        }

        return null;
    }
}

/// <summary>
/// Marker type used by retry tests. The public gateway errors live in <c>Vercel.AI.Gateway</c>.
/// This copy stays internal so the two <c>GatewayError</c> names do not collide.
/// </summary>
internal abstract class GatewayError : Exception
{
    /// <summary>Marker shared by gateway errors.</summary>
    public const string ErrorMarker = "vercel.ai.gateway.error";

    /// <summary>Creates a gateway error.</summary>
    protected GatewayError(string message, int statusCode, Exception? cause, string? generationId, bool? isRetryable)
        : base(generationId == null ? message : message + " [" + generationId + "]", cause)
    {
        StatusCode = statusCode;
        Cause = cause;
        GenerationId = generationId;
        IsRetryable = isRetryable ?? StreamProviderError.IsRetryableStatusCode(statusCode);
    }

    /// <summary>Upstream error name.</summary>
    public abstract string ErrorName { get; }

    /// <summary>Upstream error type.</summary>
    public abstract string Type { get; }

    /// <summary>HTTP status.</summary>
    public int StatusCode { get; }

    /// <summary>Underlying cause.</summary>
    public Exception? Cause { get; }

    /// <summary>Gateway generation id, when present.</summary>
    public string? GenerationId { get; }

    /// <summary>Whether the gateway call may be retried.</summary>
    public bool IsRetryable { get; }

    /// <summary>Returns whether <paramref name="error"/> is a gateway error.</summary>
    public static bool IsInstance(object? error)
    {
        return error is GatewayError;
    }
}

/// <summary>Gateway internal server error used by retry tests.</summary>
internal sealed class GatewayInternalServerError : GatewayError
{
    /// <summary>Creates the error.</summary>
    public GatewayInternalServerError(string message = "Internal server error", int statusCode = 500, Exception? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string ErrorName
    {
        get { return "GatewayInternalServerError"; }
    }

    /// <inheritdoc />
    public override string Type
    {
        get { return "internal_server_error"; }
    }

    /// <summary>Returns whether <paramref name="error"/> is this gateway error.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayInternalServerError;
    }
}

/// <summary>Gateway rate limit error used by retry tests.</summary>
internal sealed class GatewayRateLimitError : GatewayError
{
    /// <summary>Creates the error.</summary>
    public GatewayRateLimitError(string message = "Rate limit exceeded", int statusCode = 429, Exception? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string ErrorName
    {
        get { return "GatewayRateLimitError"; }
    }

    /// <inheritdoc />
    public override string Type
    {
        get { return "rate_limit_exceeded"; }
    }

    /// <summary>Returns whether <paramref name="error"/> is this gateway error.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayRateLimitError;
    }
}

/// <summary>Gateway authentication error used by retry tests.</summary>
internal sealed class GatewayAuthenticationError : GatewayError
{
    /// <summary>Creates the error.</summary>
    public GatewayAuthenticationError(string message = "Authentication failed", int statusCode = 401, Exception? cause = null, string? generationId = null)
        : base(message, statusCode, cause, generationId, null)
    {
    }

    /// <inheritdoc />
    public override string ErrorName
    {
        get { return "GatewayAuthenticationError"; }
    }

    /// <inheritdoc />
    public override string Type
    {
        get { return "authentication_error"; }
    }

    /// <summary>Returns whether <paramref name="error"/> is this gateway error.</summary>
    public static new bool IsInstance(object? error)
    {
        return error is GatewayAuthenticationError;
    }
}

/// <summary>Why <see cref="RetryError"/> stopped retrying.</summary>
public enum RetryErrorReason
{
    /// <summary>The retry budget was spent.</summary>
    MaxRetriesExceeded,

    /// <summary>A later attempt failed with an error that must not be retried.</summary>
    ErrorNotRetryable,

    /// <summary>The call was aborted.</summary>
    Abort,
}

/// <summary>Retries were exhausted or stopped.</summary>
public sealed class RetryError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_RetryError";

    /// <summary>Creates the error.</summary>
    public RetryError(string message, RetryErrorReason reason, IReadOnlyList<object> errors)
        : base("AI_RetryError", message)
    {
        Reason = reason;
        Errors = errors ?? Array.Empty<object>();
        LastError = Errors.Count == 0 ? null : Errors[Errors.Count - 1];
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Why retrying stopped.</summary>
    public RetryErrorReason Reason { get; }

    /// <summary>Errors from each attempt, in order.</summary>
    public IReadOnlyList<object> Errors { get; }

    /// <summary>Last attempt's error.</summary>
    public object? LastError { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
