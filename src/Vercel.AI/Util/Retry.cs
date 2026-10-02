// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;

namespace Vercel.AI.Util;

/// <summary>A failed provider call that may carry retry headers. Maps to <c>APICallError</c>.</summary>
public class APICallError : Exception
{
    /// <summary>Creates a provider call error.</summary>
    public APICallError(
        string message,
        string url,
        object? requestBodyValues = null,
        bool? isRetryable = null,
        int? statusCode = null,
        IReadOnlyDictionary<string, string>? responseHeaders = null,
        object? data = null,
        Exception? cause = null)
        : base(message, cause)
    {
        Url = url;
        RequestBodyValues = requestBodyValues;
        StatusCode = statusCode;
        Data = data;
        IsRetryable = isRetryable ?? (statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500);
        if (responseHeaders is null)
        {
            ResponseHeaders = null;
        }
        else
        {
            var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in responseHeaders)
            {
                copy[pair.Key] = pair.Value;
            }

            ResponseHeaders = copy;
        }
    }

    /// <summary>Request URL.</summary>
    public string Url { get; }

    /// <summary>Request body that was sent.</summary>
    public object? RequestBodyValues { get; }

    /// <summary>HTTP status, when the provider returned one.</summary>
    public int? StatusCode { get; }

    /// <summary>Whether another attempt should be made.</summary>
    public bool IsRetryable { get; }

    /// <summary>Response headers, compared without regard to case.</summary>
    public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }

    /// <summary>Provider response data.</summary>
    public new object? Data { get; }

    /// <summary>Returns whether <paramref name="error"/> is an <see cref="APICallError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is APICallError;
    }
}

/// <summary>Gateway error base. Maps to <c>GatewayError</c>.</summary>
public abstract class GatewayError : Exception
{
    /// <summary>Creates a gateway error and derives <see cref="IsRetryable"/> from <paramref name="statusCode"/>.</summary>
    protected GatewayError(string message, int statusCode, Exception? cause)
        : base(message, cause)
    {
        StatusCode = statusCode;
        IsRetryable = statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500;
    }

    /// <summary>HTTP status.</summary>
    public int StatusCode { get; }

    /// <summary>Whether another attempt should be made.</summary>
    public bool IsRetryable { get; }

    /// <summary>Returns whether <paramref name="error"/> is a <see cref="GatewayError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is GatewayError;
    }
}

/// <summary>Gateway HTTP 5xx. Maps to <c>GatewayInternalServerError</c>.</summary>
public sealed class GatewayInternalServerError : GatewayError
{
    /// <summary>Creates the error.</summary>
    public GatewayInternalServerError(string? message = null, int statusCode = 500, Exception? cause = null)
        : base(message ?? "Internal server error", statusCode, cause)
    {
    }
}

/// <summary>Gateway HTTP 429. Maps to <c>GatewayRateLimitError</c>.</summary>
public sealed class GatewayRateLimitError : GatewayError
{
    /// <summary>Creates the error.</summary>
    public GatewayRateLimitError(string? message = null, int statusCode = 429, Exception? cause = null)
        : base(message ?? "Rate limit exceeded", statusCode, cause)
    {
    }
}

/// <summary>Gateway HTTP 401. Maps to <c>GatewayAuthenticationError</c>.</summary>
public sealed class GatewayAuthenticationError : GatewayError
{
    /// <summary>Creates the error.</summary>
    public GatewayAuthenticationError(string? message = null, int statusCode = 401, Exception? cause = null)
        : base(message ?? "Authentication failed", statusCode, cause)
    {
    }
}

/// <summary>Retries were exhausted or a later attempt was not retryable. Maps to <c>RetryError</c>.</summary>
public sealed class RetryError : Exception
{
    /// <summary>Creates a retry error.</summary>
    public RetryError(string message, string reason, IReadOnlyList<Exception> errors)
        : base(message)
    {
        Reason = reason;
        Errors = errors;
        LastError = errors.Count == 0 ? null : errors[errors.Count - 1];
    }

    /// <summary><c>maxRetriesExceeded</c> or <c>errorNotRetryable</c>.</summary>
    public string Reason { get; }

    /// <summary>Errors from each attempt, in order.</summary>
    public IReadOnlyList<Exception> Errors { get; }

    /// <summary>The last error in <see cref="Errors"/>.</summary>
    public Exception? LastError { get; }

    /// <summary>Returns whether <paramref name="error"/> is a <see cref="RetryError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is RetryError;
    }
}

/// <summary>Validated retry settings. Maps to the object returned by <c>prepareRetries</c>.</summary>
public sealed class PreparedRetries
{
    /// <summary>Creates the prepared settings.</summary>
    public PreparedRetries(int maxRetries, RetryFunction retry)
    {
        MaxRetries = maxRetries;
        Retry = retry;
    }

    /// <summary>The retry count that will be used, including the default of 2.</summary>
    public int MaxRetries { get; }

    /// <summary>The retry function bound to <see cref="MaxRetries"/>.</summary>
    public RetryFunction Retry { get; }
}

/// <summary>Runs an operation with exponential backoff. Maps to <c>retryWithExponentialBackoff</c>.</summary>
public sealed class RetryFunction
{
    private readonly int _maxRetries;
    private readonly int _initialDelayInMs;
    private readonly double _backoffFactor;
    private readonly CancellationToken _cancellationToken;
    private readonly Func<Exception, Task<bool>>? _additionalRetryableError;
    private readonly Func<int, CancellationToken, Task>? _delay;

    internal RetryFunction(
        int maxRetries,
        int initialDelayInMs,
        double backoffFactor,
        CancellationToken cancellationToken,
        Func<Exception, Task<bool>>? additionalRetryableError,
        Func<int, CancellationToken, Task>? delay)
    {
        _maxRetries = maxRetries;
        _initialDelayInMs = initialDelayInMs;
        _backoffFactor = backoffFactor;
        _cancellationToken = cancellationToken;
        _additionalRetryableError = additionalRetryableError;
        _delay = delay;
    }

    /// <summary>Runs <paramref name="operation"/> and retries it according to this function.</summary>
    public Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        return AttemptAsync(operation, _initialDelayInMs, new List<Exception>());
    }

    private async Task<T> AttemptAsync<T>(Func<Task<T>> operation, int delayInMs, List<Exception> errors)
    {
        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (Exception error)
        {
            if (IsAbort(error))
            {
                throw;
            }

            if (_maxRetries == 0)
            {
                throw;
            }

            var attempts = new List<Exception>(errors) { error };
            var tryNumber = attempts.Count;
            if (tryNumber > _maxRetries)
            {
                throw new RetryError(
                    "Failed after " + tryNumber.ToString(CultureInfo.InvariantCulture) + " attempts. Last error: " + error.Message,
                    "maxRetriesExceeded",
                    attempts);
            }

            if (await ShouldRetryAsync(error).ConfigureAwait(false) && tryNumber <= _maxRetries)
            {
                var wait = GetRetryDelayInMs(error, delayInMs);
                await DelayAsync(wait).ConfigureAwait(false);
                return await AttemptAsync(operation, (int)(_backoffFactor * delayInMs), attempts).ConfigureAwait(false);
            }

            if (tryNumber == 1)
            {
                throw;
            }

            throw new RetryError(
                "Failed after " + tryNumber.ToString(CultureInfo.InvariantCulture) + " attempts with non-retryable error: '" + error.Message + "'",
                "errorNotRetryable",
                attempts);
        }
    }

    private async Task DelayAsync(int milliseconds)
    {
        if (_delay is not null)
        {
            await _delay(milliseconds, _cancellationToken).ConfigureAwait(false);
            return;
        }

        await Task.Delay(milliseconds, _cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> ShouldRetryAsync(Exception error)
    {
        var retryable = (error is APICallError api && api.IsRetryable)
            || (error is GatewayError gateway && gateway.IsRetryable);
        if (retryable)
        {
            return true;
        }

        if (_additionalRetryableError is not null && await _additionalRetryableError(error).ConfigureAwait(false))
        {
            return true;
        }

        return false;
    }

    private static int GetRetryDelayInMs(Exception error, int exponentialBackoffDelay)
    {
        IReadOnlyDictionary<string, string>? headers = null;
        if (error is APICallError api)
        {
            headers = api.ResponseHeaders;
        }
        else if (error.InnerException is APICallError cause)
        {
            headers = cause.ResponseHeaders;
        }

        if (headers is null)
        {
            return exponentialBackoffDelay;
        }

        double? milliseconds = null;
        if (headers.TryGetValue("retry-after-ms", out var header))
        {
            double parsed;
            if (double.TryParse(header, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                milliseconds = parsed;
            }
        }

        if (milliseconds is null && headers.TryGetValue("retry-after", out var retryAfter))
        {
            double seconds;
            if (double.TryParse(retryAfter, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds))
            {
                milliseconds = seconds * 1000d;
            }
            else
            {
                DateTimeOffset when;
                if (DateTimeOffset.TryParse(retryAfter, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out when))
                {
                    milliseconds = (when - DateTimeOffset.UtcNow).TotalMilliseconds;
                }
            }
        }

        if (milliseconds is double delay && !double.IsNaN(delay) && delay >= 0d && (delay < 60d * 1000d || delay < exponentialBackoffDelay))
        {
            return (int)delay;
        }

        return exponentialBackoffDelay;
    }

    private static bool IsAbort(Exception error)
    {
        var name = error.GetType().Name;
        return error is OperationCanceledException
            || error is TimeoutErrorException
            || string.Equals(name, "AbortError", StringComparison.Ordinal)
            || string.Equals(name, "TimeoutError", StringComparison.Ordinal);
    }
}

/// <summary>
/// Retry helpers. Maps to <c>prepareRetries</c> and <c>retryWithExponentialBackoffRespectingRetryHeaders</c>.
/// </summary>
public static class Retries
{
    /// <summary>
    /// Validates <paramref name="maxRetries"/> and returns the exponential-backoff retry used by the SDK.
    /// A null count uses <paramref name="defaultMaxRetries"/> (2).
    /// </summary>
    public static PreparedRetries PrepareRetries(
        int? maxRetries = null,
        CancellationToken cancellationToken = default,
        Func<Exception, Task<bool>>? additionalRetryableError = null,
        string parameter = "maxRetries",
        int defaultMaxRetries = 2,
        Func<int, CancellationToken, Task>? delay = null)
    {
        if (maxRetries is int value && value < 0)
        {
            throw new InvalidArgumentException(parameter, value, parameter + " must be >= 0");
        }

        var resolved = maxRetries ?? defaultMaxRetries;
        return new PreparedRetries(
            resolved,
            RetryWithExponentialBackoffRespectingRetryHeaders(
                resolved,
                cancellationToken: cancellationToken,
                additionalRetryableError: additionalRetryableError,
                delay: delay));
    }

    /// <summary>
    /// Retries retryable <see cref="APICallError"/> and <see cref="GatewayError"/> failures.
    /// A <c>retry-after-ms</c> or <c>retry-after</c> header is used when it is between 0 and 60 seconds,
    /// or when it is shorter than the exponential delay.
    /// </summary>
    public static RetryFunction RetryWithExponentialBackoffRespectingRetryHeaders(
        int maxRetries = 2,
        int initialDelayInMs = 2000,
        double backoffFactor = 2,
        CancellationToken cancellationToken = default,
        Func<Exception, Task<bool>>? additionalRetryableError = null,
        Func<int, CancellationToken, Task>? delay = null)
    {
        return new RetryFunction(maxRetries, initialDelayInMs, backoffFactor, cancellationToken, additionalRetryableError, delay);
    }
}
