// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Why <see cref="RetryException"/> was thrown.</summary>
public enum RetryErrorReason
{
    /// <summary>The retry budget was used up.</summary>
    MaxRetriesExceeded,

    /// <summary>A later attempt failed with an error that must not be retried.</summary>
    ErrorNotRetryable,
}

/// <summary>Failure of <see cref="ExponentialBackoff"/> after at least one retry.</summary>
public sealed class RetryException : Exception
{
    /// <summary>Creates the exception.</summary>
    public RetryException(string message, RetryErrorReason reason, IReadOnlyList<Exception> errors)
        : base(message)
    {
        Reason = reason;
        Errors = errors ?? Array.Empty<Exception>();
    }

    /// <summary>Why retries stopped.</summary>
    public RetryErrorReason Reason { get; }

    /// <summary>Errors from each attempt, in order.</summary>
    public IReadOnlyList<Exception> Errors { get; }
}

/// <summary>Options for <see cref="ExponentialBackoff"/>.</summary>
public sealed class RetryOptions
{
    /// <summary>Retries after the first attempt. <c>0</c> disables wrapping and retry.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Delay before the first retry, in milliseconds.</summary>
    public int InitialDelayInMs { get; set; } = 2000;

    /// <summary>Multiplier applied after each retry.</summary>
    public double BackoffFactor { get; set; } = 2;

    /// <summary>Replaces the computed delay. The second argument is the current backoff delay.</summary>
    public Func<Exception, int, int>? GetDelayInMs { get; set; }
}

/// <summary>
/// Retries a failed operation with exponential backoff.
/// Abort errors are rethrown. The first non-retryable error is rethrown without wrapping.
/// </summary>
public static class ExponentialBackoff
{
    /// <summary>Runs <paramref name="action"/> until it succeeds or retries are exhausted.</summary>
    public static Task<T> RetryAsync<T>(
        Func<Task<T>> action,
        Func<Exception, Task<bool>> shouldRetry,
        RetryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (shouldRetry is null)
        {
            throw new ArgumentNullException(nameof(shouldRetry));
        }

        options = options ?? new RetryOptions();
        return RetryCoreAsync(action, shouldRetry, options, options.InitialDelayInMs, new List<Exception>(), cancellationToken);
    }

    private static async Task<T> RetryCoreAsync<T>(
        Func<Task<T>> action,
        Func<Exception, Task<bool>> shouldRetry,
        RetryOptions options,
        int delayInMs,
        List<Exception> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (Exception error)
        {
            if (AbortErrors.IsAbortError(error))
            {
                throw;
            }

            if (options.MaxRetries == 0)
            {
                throw;
            }

            errors.Add(error);
            var tryNumber = errors.Count;
            var message = error.Message ?? string.Empty;
            if (tryNumber > options.MaxRetries)
            {
                throw new RetryException(
                    "Failed after " + tryNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) + " attempts. Last error: " + message,
                    RetryErrorReason.MaxRetriesExceeded,
                    errors);
            }

            if (await shouldRetry(error).ConfigureAwait(false))
            {
                var delay = options.GetDelayInMs != null ? options.GetDelayInMs(error, delayInMs) : delayInMs;
                await Delay.WaitAsync(delay, cancellationToken).ConfigureAwait(false);
                var nextDelay = (int)Math.Round(options.BackoffFactor * delayInMs, MidpointRounding.AwayFromZero);
                return await RetryCoreAsync(action, shouldRetry, options, nextDelay, errors, cancellationToken).ConfigureAwait(false);
            }

            if (tryNumber == 1)
            {
                throw;
            }

            throw new RetryException(
                "Failed after " + tryNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) + " attempts with non-retryable error: '" + message + "'",
                RetryErrorReason.ErrorNotRetryable,
                errors);
        }
    }
}
