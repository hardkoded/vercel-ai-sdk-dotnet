// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>A JavaScript <c>Error</c> with <c>code</c> and <c>cause</c>.</summary>
public sealed class JsCauseError : Exception
{
    /// <summary>Creates an error.</summary>
    public JsCauseError(string message, string? name = null, string? code = null, object? cause = null)
        : base(message)
    {
        ErrorName = name ?? "Error";
        Code = code;
        Cause = cause;
    }

    /// <summary>Error name, such as <c>TypeError</c>.</summary>
    public string ErrorName { get; }

    /// <summary>Network error code, when one was assigned.</summary>
    public string? Code { get; }

    /// <summary>Nested cause. May point at this error when the chain is cyclic.</summary>
    public object? Cause { get; set; }
}

/// <summary>Turns fetch failures into retryable <see cref="APICallError"/> values. Maps to <c>handleFetchError</c>.</summary>
public static class FetchErrors
{
    private static readonly string[] FetchFailedMessages = { "fetch failed", "failed to fetch" };

    private static readonly string[] RetryableCodes =
    {
        "ConnectionRefused",
        "ConnectionClosed",
        "FailedToOpenSocket",
        "ECONNRESET",
        "ECONNREFUSED",
        "ETIMEDOUT",
        "EPIPE",
        "UND_ERR_SOCKET",
        "UND_ERR_HEADERS_TIMEOUT",
        "UND_ERR_BODY_TIMEOUT",
        "UND_ERR_CONNECT_TIMEOUT",
    };

    /// <summary>Returns <paramref name="error"/>, or a retryable <see cref="APICallError"/> when the failure is a network error.</summary>
    public static object? HandleFetchError(object? error, string url, object? requestBodyValues)
    {
        if (AbortErrors.IsAbortError(error))
        {
            return error;
        }

        if (error is JsCauseError typeError
            && typeError.ErrorName == "TypeError"
            && IsFetchFailed(typeError.Message)
            && typeError.Cause != null)
        {
            var causeMessage = MessageOf(typeError.Cause);
            return new APICallError(
                "Cannot connect to API: " + causeMessage,
                url,
                requestBodyValues,
                cause: typeError.Cause,
                isRetryable: true);
        }

        if (FindNetworkError(error) != null)
        {
            if (error is APICallError api)
            {
                return new APICallError(
                    api.Message,
                    api.Url,
                    api.RequestBodyValues,
                    api.StatusCode,
                    api.ResponseHeaders,
                    api.ResponseBody,
                    api.Cause,
                    true,
                    api.Data);
            }

            return new APICallError(
                "Cannot connect to API: " + MessageOf(error),
                url,
                requestBodyValues,
                cause: error,
                isRetryable: true);
        }

        return error;
    }

    private static bool IsFetchFailed(string message)
    {
        var lower = message.ToLowerInvariant();
        foreach (var candidate in FetchFailedMessages)
        {
            if (lower == candidate)
            {
                return true;
            }
        }

        return false;
    }

    private static object? FindNetworkError(object? error)
    {
        var visited = new HashSet<object>();
        var current = error;
        while (current != null && visited.Add(current))
        {
            var code = CodeOf(current);
            if (code != null && ContainsCode(code))
            {
                return current;
            }

            current = CauseOf(current);
        }

        return null;
    }

    private static bool ContainsCode(string code)
    {
        foreach (var candidate in RetryableCodes)
        {
            if (candidate == code)
            {
                return true;
            }
        }

        return false;
    }

    private static string? CodeOf(object error)
    {
        return error is JsCauseError js ? js.Code : null;
    }

    private static object? CauseOf(object error)
    {
        if (error is JsCauseError js)
        {
            return js.Cause;
        }

        if (error is SdkError sdk)
        {
            return sdk.Cause;
        }

        return null;
    }

    private static string MessageOf(object? error)
    {
        if (error is Exception exception)
        {
            return exception.Message;
        }

        return error?.ToString() ?? string.Empty;
    }
}
