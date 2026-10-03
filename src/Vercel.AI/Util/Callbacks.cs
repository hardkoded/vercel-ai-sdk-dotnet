// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Invokes callbacks and ignores individual failures.</summary>
public static class Callbacks
{
    /// <summary>
    /// Invokes <paramref name="callbacks"/> in parallel with <paramref name="eventValue"/> and waits for them to settle.
    /// Null callbacks are skipped. Thrown and rejected callbacks are ignored.
    /// </summary>
    public static async Task NotifyAsync<T>(T eventValue, IEnumerable<Func<T, Task>?>? callbacks)
    {
        var tasks = new List<Task>();
        if (callbacks != null)
        {
            foreach (var callback in callbacks)
            {
                if (callback == null)
                {
                    continue;
                }

                tasks.Add(InvokeAsync(callback, eventValue));
            }
        }

        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
    }

    /// <summary>Notifies one callback. A null callback is ignored.</summary>
    public static Task NotifyAsync<T>(T eventValue, Func<T, Task>? callback)
    {
        if (callback == null)
        {
            return Task.CompletedTask;
        }

        return NotifyAsync(eventValue, new Func<T, Task>[] { callback });
    }

    /// <summary>
    /// Returns a callback that forwards each event to <paramref name="callbacks"/> in parallel
    /// and waits until they settle. Failures are ignored.
    /// </summary>
    public static Func<TEvent, Task> MergeCallbacks<TEvent>(params Func<TEvent, Task>?[] callbacks)
    {
        return delegate(TEvent eventValue)
        {
            return NotifyAsync(eventValue, callbacks);
        };
    }

    private static async Task InvokeAsync<T>(Func<T, Task> callback, T eventValue)
    {
        try
        {
            await callback(eventValue).ConfigureAwait(false);
        }
        catch (Exception)
        {
        }
    }
}
