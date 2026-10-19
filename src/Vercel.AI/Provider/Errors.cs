// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Provider;

/// <summary>Base exception for the community .NET port of the Vercel AI SDK.</summary>
public class AiSdkException : Exception
{
    /// <summary>Creates an exception with a message.</summary>
    public AiSdkException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public AiSdkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>The provider returned a non-success HTTP status after retries.</summary>
public class ApiException : AiSdkException
{
    /// <summary>Creates an API exception.</summary>
    public ApiException(string message, int statusCode, string? responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>Creates an API exception caused by <paramref name="innerException"/>.</summary>
    public ApiException(string message, int statusCode, string? responseBody, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>HTTP status code.</summary>
    public int StatusCode { get; }

    /// <summary>Raw response body, when the provider sent one.</summary>
    public string? ResponseBody { get; }

    /// <summary>URL of the failed request. Set by the response handlers.</summary>
    public string? Url { get; init; }

    /// <summary>Request body values the call was made with. Set by the response handlers.</summary>
    public object? RequestBodyValues { get; init; }

    /// <summary>Response headers, when the handler had them.</summary>
    public IReadOnlyDictionary<string, string>? ResponseHeaders { get; init; }

    /// <summary>
    /// The provider error parsed from the response body, when the handler could parse it.
    /// Maps to <c>APICallError.data</c>. Named <c>ErrorData</c> because <see cref="Exception.Data"/> already exists.
    /// </summary>
    public object? ErrorData { get; init; }

    /// <summary>True when the call can be retried. Defaults to 408, 429, and 500 and above.</summary>
    public bool IsRetryable
    {
        get => _isRetryable ?? (StatusCode == 408 || StatusCode == 429 || StatusCode >= 500);
        init => _isRetryable = value;
    }

    private readonly bool? _isRetryable;
}

/// <summary>HTTP 400.</summary>
public sealed class BadRequestException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public BadRequestException(string message, string? responseBody)
        : base(message, 400, responseBody)
    {
    }
}

/// <summary>HTTP 401.</summary>
public sealed class AuthenticationException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public AuthenticationException(string message, string? responseBody)
        : base(message, 401, responseBody)
    {
    }
}

/// <summary>HTTP 403.</summary>
public sealed class PermissionDeniedException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public PermissionDeniedException(string message, string? responseBody)
        : base(message, 403, responseBody)
    {
    }
}

/// <summary>HTTP 404.</summary>
public sealed class NotFoundException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public NotFoundException(string message, string? responseBody)
        : base(message, 404, responseBody)
    {
    }
}

/// <summary>HTTP 422.</summary>
public sealed class UnprocessableEntityException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public UnprocessableEntityException(string message, string? responseBody)
        : base(message, 422, responseBody)
    {
    }
}

/// <summary>HTTP 429.</summary>
public sealed class RateLimitException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public RateLimitException(string message, string? responseBody)
        : base(message, 429, responseBody)
    {
    }
}

/// <summary>HTTP 500-599.</summary>
public sealed class InternalServerException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public InternalServerException(string message, int statusCode, string? responseBody)
        : base(message, statusCode, responseBody)
    {
    }
}

/// <summary>The connection to the provider failed.</summary>
public class ApiConnectionException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public ApiConnectionException(string message, Exception? innerException = null)
        : base(message, innerException ?? new Exception(message))
    {
    }
}

/// <summary>The provider call timed out.</summary>
public sealed class ApiTimeoutException : ApiConnectionException
{
    /// <summary>Creates the exception.</summary>
    public ApiTimeoutException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The call was cancelled through a <see cref="CancellationToken"/>. It derives from <see cref="OperationCanceledException"/>,
/// so <c>catch (OperationCanceledException)</c> handles every caller cancel. It is not an <see cref="AiSdkException"/>.
/// </summary>
public sealed class ApiUserAbortException : OperationCanceledException
{
    /// <summary>Creates the exception.</summary>
    public ApiUserAbortException()
        : base("The operation was cancelled.")
    {
    }

    /// <summary>Creates the exception for a cancelled <paramref name="cancellationToken"/>, with the original cancel as the cause.</summary>
    public ApiUserAbortException(Exception? innerException, CancellationToken cancellationToken)
        : base("The operation was cancelled.", innerException, cancellationToken)
    {
    }
}

/// <summary>A UI message could not be converted to a model message. Maps to <c>MessageConversionError</c>.</summary>
public sealed class MessageConversionException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public MessageConversionException(Vercel.AI.Ui.UIMessage originalMessage, string message)
        : base(message)
    {
        OriginalMessage = originalMessage;
    }

    /// <summary>The message that could not be converted.</summary>
    public Vercel.AI.Ui.UIMessage OriginalMessage { get; }
}
