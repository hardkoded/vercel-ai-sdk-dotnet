// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Callback fan-out. Maps to <c>notify</c> and <c>mergeCallbacks</c>.</summary>
public static class Callbacks
{
    /// <summary>
    /// Invokes every callback with <paramref name="eventValue"/> and waits for all of them.
    /// Null callbacks are ignored. Exceptions from callbacks are swallowed.
    /// </summary>
    public static Task NotifyAsync<TEvent>(TEvent eventValue, params Func<TEvent, Task>?[] callbacks)
    {
        return NotifyAsync(eventValue, (IEnumerable<Func<TEvent, Task>?>?)callbacks);
    }

    /// <summary>
    /// Invokes every callback with <paramref name="eventValue"/> and waits for all of them.
    /// A null list is ignored. Exceptions from callbacks are swallowed.
    /// </summary>
    public static async Task NotifyAsync<TEvent>(TEvent eventValue, IEnumerable<Func<TEvent, Task>?>? callbacks)
    {
        if (callbacks is null)
        {
            return;
        }

        var pending = new List<Task>();
        foreach (var callback in callbacks)
        {
            pending.Add(Invoke(callback, eventValue));
        }

        await Task.WhenAll(pending).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a callback that invokes <paramref name="callbacks"/> together and waits until they settle.
    /// Null callbacks are ignored. Exceptions are swallowed.
    /// </summary>
    public static Func<TEvent, Task> MergeCallbacks<TEvent>(params Func<TEvent, Task>?[] callbacks)
    {
        return eventValue => NotifyAsync(eventValue, callbacks);
    }

    private static Task Invoke<TEvent>(Func<TEvent, Task>? callback, TEvent eventValue)
    {
        if (callback is null)
        {
            return Task.CompletedTask;
        }

        try
        {
            var task = callback(eventValue) ?? Task.CompletedTask;
            return Await(task);
        }
        catch (Exception)
        {
            return Task.CompletedTask;
        }
    }

    private static async Task Await(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception)
        {
        }
    }
}
