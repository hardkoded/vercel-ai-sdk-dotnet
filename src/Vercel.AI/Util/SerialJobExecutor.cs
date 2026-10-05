// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Runs async jobs one at a time, in submission order.</summary>
public sealed class SerialJobExecutor
{
    private readonly object _gate = new object();
    private readonly Queue<Func<Task>> _queue = new Queue<Func<Task>>();
    private bool _processing;

    /// <summary>Queues <paramref name="job"/> and runs it after earlier jobs finish.</summary>
    public Task RunAsync(Func<Task> job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_gate)
        {
            _queue.Enqueue(delegate
            {
                return RunJobAsync(job, completion);
            });
            if (_processing)
            {
                return completion.Task;
            }

            _processing = true;
        }

        ProcessQueue();
        return completion.Task;
    }

    private void ProcessQueue()
    {
        Func<Task> next;
        lock (_gate)
        {
            if (_queue.Count == 0)
            {
                _processing = false;
                return;
            }

            next = _queue.Dequeue();
        }

        next().ContinueWith(delegate { ProcessQueue(); }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
    }

    private static async Task RunJobAsync(Func<Task> job, TaskCompletionSource<bool> completion)
    {
        try
        {
            await job().ConfigureAwait(false);
            completion.TrySetResult(true);
        }
        catch (Exception error)
        {
            completion.TrySetException(error);
        }
    }
}
