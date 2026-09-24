// Copyright 2023 Vercel, Inc.
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

    /// <summary>HTTP status code.</summary>
    public int StatusCode { get; }

    /// <summary>Raw response body, when the provider sent one.</summary>
    public string? ResponseBody { get; }
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

/// <summary>The call was cancelled through a <see cref="CancellationToken"/>.</summary>
public sealed class ApiUserAbortException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public ApiUserAbortException()
        : base("The operation was cancelled.")
    {
    }
}
