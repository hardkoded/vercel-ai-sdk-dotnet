// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Resolves after a delay and can be aborted. Maps to <c>delay</c>.</summary>
public static class Delays
{
    /// <summary>Number of in-flight delay waits.</summary>
    internal static int ActiveTimers;

    /// <summary>Number of abort registrations that have not been disposed.</summary>
    internal static int ActiveAbortRegistrations;

    /// <summary>Test hook that replaces <see cref="Task.Delay(int, CancellationToken)"/>.</summary>
    internal static Func<long, CancellationToken, Task>? WaitOverride;

    /// <summary>Resolves immediately. Maps to calling <c>delay()</c> with an omitted duration.</summary>
    public static Task DelayAsync(CancellationToken cancellationToken = default)
    {
        return DelayAsync(null, cancellationToken);
    }

    /// <summary>
    /// Resolves after <paramref name="delayInMs"/> milliseconds.
    /// Null resolves immediately. Negative values resolve like a zero delay.
    /// Aborting <paramref name="cancellationToken"/> rejects with a <see cref="DomException"/> named <c>AbortError</c>.
    /// </summary>
    public static async Task DelayAsync(double? delayInMs, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new DomException("Delay was aborted", "AbortError");
        }

        if (delayInMs is null)
        {
            return;
        }

        var milliseconds = delayInMs.Value;
        if (double.IsNaN(milliseconds) || milliseconds < 0)
        {
            milliseconds = 0;
        }
        else if (milliseconds > 2147483647d)
        {
            milliseconds = 1;
        }

        var registered = false;
        if (cancellationToken.CanBeCanceled)
        {
            System.Threading.Interlocked.Increment(ref ActiveAbortRegistrations);
            registered = true;
        }

        System.Threading.Interlocked.Increment(ref ActiveTimers);
        try
        {
            using (registered ? cancellationToken.Register(static () => { }) : default)
            {
                await WaitAsync((long)milliseconds, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw new DomException("Delay was aborted", "AbortError");
        }
        finally
        {
            System.Threading.Interlocked.Decrement(ref ActiveTimers);
            if (registered)
            {
                System.Threading.Interlocked.Decrement(ref ActiveAbortRegistrations);
            }
        }
    }

    private static async Task WaitAsync(long milliseconds, CancellationToken cancellationToken)
    {
        if (WaitOverride != null)
        {
            await WaitOverride(milliseconds, cancellationToken).ConfigureAwait(false);
            return;
        }

        while (milliseconds > int.MaxValue)
        {
            await Task.Delay(int.MaxValue, cancellationToken).ConfigureAwait(false);
            milliseconds -= int.MaxValue;
        }

        await Task.Delay((int)milliseconds, cancellationToken).ConfigureAwait(false);
    }
}
