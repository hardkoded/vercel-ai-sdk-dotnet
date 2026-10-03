// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using Vercel.AI.Provider;

namespace Vercel.AI.Util;

/// <summary>Options for <see cref="RetryWithExponentialBackoffRespectingRetryHeaders"/>.</summary>
public sealed class RetryOptions
{
    /// <summary>Maximum retries after the first attempt. Default is 2.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Delay before the first retry, in milliseconds. Default is 2000.</summary>
    public int InitialDelayInMs { get; set; } = 2000;

    /// <summary>Multiplier applied after each retry. Default is 2.</summary>
    public double BackoffFactor { get; set; } = 2;

    /// <summary>Aborts the wait between attempts.</summary>
    public CancellationToken AbortSignal { get; set; }

    /// <summary>Extra predicate consulted when the error is not an API or gateway retry.</summary>
    public Func<object, Task<bool>>? AdditionalRetryableError { get; set; }

    /// <summary>
    /// Waits <c>milliseconds</c> before the next attempt.
    /// The default is <see cref="Task.Delay(int, CancellationToken)"/>.
    /// </summary>
    public Func<int, CancellationToken, Task>? Delay { get; set; }

    /// <summary>Clock used to parse HTTP-date <c>Retry-After</c> values. Default is UTC now.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }
}

/// <summary>
/// Retries a failed operation with exponential backoff, honoring <c>retry-after-ms</c> and <c>retry-after</c>
/// when the delay is between 0 and 60 seconds, or shorter than the backoff delay.
/// </summary>
public sealed class RetryWithExponentialBackoffRespectingRetryHeaders
{
    private readonly RetryOptions _options;

    /// <summary>Creates a retry strategy.</summary>
    public RetryWithExponentialBackoffRespectingRetryHeaders(RetryOptions? options = null)
    {
        _options = options ?? new RetryOptions();
    }

    /// <summary>Runs <paramref name="operation"/> until it succeeds or retries stop.</summary>
    public Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        return ExecuteAsync(operation, _options.InitialDelayInMs, new List<object>());
    }

    /// <summary>
    /// Delay in milliseconds for <paramref name="error"/> given the current exponential delay.
    /// Header delays are used when they are at least 0 and either under 60 seconds or shorter than the backoff.
    /// </summary>
    public static double GetRetryDelayInMs(object error, double exponentialBackoffDelay, DateTimeOffset? utcNow = null)
    {
        var headers = GetHeaders(error);
        if (headers == null)
        {
            return exponentialBackoffDelay;
        }

        double? milliseconds = null;
        var retryAfterMs = GetHeader(headers, "retry-after-ms");
        if (!string.IsNullOrEmpty(retryAfterMs))
        {
            double parsed;
            if (double.TryParse(retryAfterMs, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                milliseconds = parsed;
            }
        }

        var retryAfter = GetHeader(headers, "retry-after");
        if (!string.IsNullOrEmpty(retryAfter) && milliseconds == null)
        {
            double seconds;
            if (double.TryParse(retryAfter, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds))
            {
                milliseconds = seconds * 1000d;
            }
            else
            {
                DateTimeOffset date;
                if (DateTimeOffset.TryParse(retryAfter, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out date))
                {
                    var now = utcNow ?? DateTimeOffset.UtcNow;
                    milliseconds = (date - now).TotalMilliseconds;
                }
            }
        }

        if (milliseconds != null && !double.IsNaN(milliseconds.Value) && 0 <= milliseconds.Value
            && (milliseconds.Value < 60 * 1000 || milliseconds.Value < exponentialBackoffDelay))
        {
            return milliseconds.Value;
        }

        return exponentialBackoffDelay;
    }

    private async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, double delayInMs, List<object> errors)
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

            if (_options.MaxRetries == 0)
            {
                throw;
            }

            var errorMessage = ErrorMessage.GetErrorMessage(error);
            var newErrors = new List<object>(errors) { error };
            var tryNumber = newErrors.Count;
            if (tryNumber > _options.MaxRetries)
            {
                throw new RetryError(
                    "Failed after " + tryNumber + " attempts. Last error: " + errorMessage,
                    RetryErrorReason.MaxRetriesExceeded,
                    newErrors);
            }

            if (await ShouldRetryAsync(error).ConfigureAwait(false) && tryNumber <= _options.MaxRetries)
            {
                var now = _options.UtcNow == null ? (DateTimeOffset?)null : _options.UtcNow();
                var wait = GetRetryDelayInMs(error, delayInMs, now);
                var delay = _options.Delay ?? DefaultDelay;
                await delay((int)wait, _options.AbortSignal).ConfigureAwait(false);
                return await ExecuteAsync(operation, _options.BackoffFactor * delayInMs, newErrors).ConfigureAwait(false);
            }

            if (tryNumber == 1)
            {
                throw;
            }

            throw new RetryError(
                "Failed after " + tryNumber + " attempts with non-retryable error: '" + errorMessage + "'",
                RetryErrorReason.ErrorNotRetryable,
                newErrors);
        }
    }

    private async Task<bool> ShouldRetryAsync(Exception error)
    {
        if ((error is ApiCallError api && api.IsRetryable) || (error is GatewayError gateway && gateway.IsRetryable))
        {
            return true;
        }

        if (_options.AdditionalRetryableError != null && await _options.AdditionalRetryableError(error).ConfigureAwait(false))
        {
            return true;
        }

        return false;
    }

    private static Task DefaultDelay(int milliseconds, CancellationToken cancellationToken)
    {
        if (milliseconds <= 0)
        {
            return Task.CompletedTask;
        }

        return Task.Delay(milliseconds, cancellationToken);
    }

    private static bool IsAbort(Exception error)
    {
        return error is OperationCanceledException || (error is JsError js && js.ErrorName == "AbortError");
    }

    private static IReadOnlyDictionary<string, string>? GetHeaders(object error)
    {
        var api = error as ApiCallError;
        if (api != null)
        {
            return api.ResponseHeaders;
        }

        var gateway = error as GatewayError;
        if (gateway != null && gateway.Cause is ApiCallError cause)
        {
            return cause.ResponseHeaders;
        }

        var exception = error as Exception;
        if (exception != null && exception.InnerException is ApiCallError inner)
        {
            return inner.ResponseHeaders;
        }

        return null;
    }

    private static string? GetHeader(IReadOnlyDictionary<string, string> headers, string name)
    {
        foreach (var header in headers)
        {
            if (string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return header.Value;
            }
        }

        return null;
    }
}

/// <summary>Validates <c>maxRetries</c> and builds the default retry strategy.</summary>
public static class PrepareRetries
{
    /// <summary>Result of <see cref="Prepare"/>.</summary>
    public sealed class Result
    {
        /// <summary>Creates the prepared retry settings.</summary>
        public Result(int maxRetries, RetryWithExponentialBackoffRespectingRetryHeaders retry)
        {
            MaxRetries = maxRetries;
            Retry = retry;
        }

        /// <summary>Retry count after validation, or the default.</summary>
        public int MaxRetries { get; }

        /// <summary>Retry strategy bound to that count.</summary>
        public RetryWithExponentialBackoffRespectingRetryHeaders Retry { get; }
    }

    /// <summary>
    /// Validates <paramref name="maxRetries"/> and returns the default of 2 when it is null.
    /// </summary>
    public static Result Prepare(int? maxRetries, CancellationToken abortSignal = default, string parameter = "maxRetries", int defaultMaxRetries = 2)
    {
        if (maxRetries != null)
        {
            var value = maxRetries.Value;
            if ((double)value != Math.Truncate((double)value))
            {
                throw new InvalidArgumentError(parameter, value, parameter + " must be an integer");
            }

            if (value < 0)
            {
                throw new InvalidArgumentError(parameter, value, parameter + " must be >= 0");
            }
        }

        var resolved = maxRetries ?? defaultMaxRetries;
        return new Result(
            resolved,
            new RetryWithExponentialBackoffRespectingRetryHeaders(new RetryOptions
            {
                MaxRetries = resolved,
                AbortSignal = abortSignal,
            }));
    }
}
