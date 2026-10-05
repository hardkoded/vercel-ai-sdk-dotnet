// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Raised when <see cref="Delay"/> is cancelled. <see cref="Name"/> is <c>AbortError</c>.</summary>
public sealed class DelayAbortedException : OperationCanceledException
{
    /// <summary>Creates the exception.</summary>
    public DelayAbortedException()
        : base("Delay was aborted")
    {
    }

    /// <summary>Abort error name.</summary>
    public string Name
    {
        get { return "AbortError"; }
    }
}

/// <summary>An exception identified by a name such as <c>AbortError</c> or <c>ResponseAborted</c>.</summary>
public sealed class NamedException : Exception
{
    /// <summary>Creates a named exception.</summary>
    public NamedException(string name, string message)
        : base(message)
    {
        Name = name ?? string.Empty;
    }

    /// <summary>Error name.</summary>
    public string Name { get; }
}

/// <summary>Recognizes abort and timeout failures that must not be retried.</summary>
public static class AbortErrors
{
    /// <summary>
    /// True for <see cref="DelayAbortedException"/>, cancellation, <see cref="TimeoutException"/>,
    /// and exceptions named <c>AbortError</c>, <c>ResponseAborted</c>, or <c>TimeoutError</c>.
    /// </summary>
    public static bool IsAbortError(Exception? error)
    {
        if (error is null)
        {
            return false;
        }

        if (error is DelayAbortedException || error is OperationCanceledException || error is TimeoutException)
        {
            return true;
        }

        var name = error is NamedException named ? named.Name : error.GetType().Name;
        return name == "AbortError" || name == "ResponseAborted" || name == "TimeoutError";
    }
}

/// <summary>Waits for a duration, or completes immediately when the duration is null. Negative durations wait zero.</summary>
public static class Delay
{
    /// <summary>Completes immediately. This is the null and undefined duration.</summary>
    public static Task WaitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <summary>Waits <paramref name="milliseconds"/>, or completes immediately when it is null.</summary>
    public static Task WaitAsync(int? milliseconds, CancellationToken cancellationToken = default)
    {
        if (milliseconds is null)
        {
            return WaitAsync(cancellationToken);
        }

        var duration = milliseconds.Value;
        if (duration < 0)
        {
            duration = 0;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromException(new DelayAbortedException());
        }

        return WaitCoreAsync(duration, cancellationToken);
    }

    private static async Task WaitCoreAsync(int milliseconds, CancellationToken cancellationToken)
    {
        using (cancellationToken.Register(static state => { }, null))
        {
            try
            {
                await Task.Delay(milliseconds, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw new DelayAbortedException();
            }
        }
    }
}
