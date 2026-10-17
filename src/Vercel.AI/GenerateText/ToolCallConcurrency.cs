// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>Tool call concurrency helpers. Maps to <c>tool-call-concurrency.ts</c>.</summary>
public static class ToolCallConcurrencyScheduler
{
    /// <summary>
    /// Validates the maximum number of tool calls that may execute concurrently.
    /// Null passes through. Maps to <c>prepareToolCallConcurrency</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentError">Thrown when <paramref name="toolCallConcurrency"/> is below 1.</exception>
    public static int? PrepareToolCallConcurrency(int? toolCallConcurrency)
    {
        if (toolCallConcurrency is null)
        {
            return null;
        }

        if (toolCallConcurrency < 1)
        {
            throw new InvalidArgumentError("toolCallConcurrency", toolCallConcurrency, "toolCallConcurrency must be >= 1");
        }

        return toolCallConcurrency;
    }

    /// <summary>
    /// Maps items with an optional concurrency limit while preserving result order.
    /// Workers claim items in input order. When <paramref name="cancellationToken"/> is canceled or an execution throws,
    /// no more queued items start. Maps to <c>mapWithConcurrency</c>.
    /// </summary>
    public static async Task<IReadOnlyList<TOutput>> MapWithConcurrency<TInput, TOutput>(
        IReadOnlyList<TInput> items,
        int? concurrency,
        Func<TInput, int, Task<TOutput>> execute,
        CancellationToken cancellationToken = default)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (execute is null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var results = new TOutput[items.Count];
        if (concurrency is null || concurrency >= items.Count)
        {
            var tasks = new Task<TOutput>[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                tasks[i] = execute(items[i], i);
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        var nextIndex = -1;
        var stopped = false;

        async Task Worker()
        {
            while (!Volatile.Read(ref stopped))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var index = Interlocked.Increment(ref nextIndex);
                if (index >= items.Count)
                {
                    return;
                }

                try
                {
                    results[index] = await execute(items[index], index).ConfigureAwait(false);
                }
                catch
                {
                    Volatile.Write(ref stopped, true);
                    throw;
                }
            }
        }

        var workers = new Task[Math.Min(concurrency.Value, items.Count)];
        for (var i = 0; i < workers.Length; i++)
        {
            workers[i] = Worker();
        }

        await Task.WhenAll(workers).ConfigureAwait(false);
        return results;
    }
}
