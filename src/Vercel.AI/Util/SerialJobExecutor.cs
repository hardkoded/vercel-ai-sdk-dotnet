// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Runs async jobs one at a time in submission order. Maps to <c>SerialJobExecutor</c>.</summary>
public sealed class SerialJobExecutor
{
    private readonly object _gate = new object();
    private readonly Queue<Func<Task>> _queue = new Queue<Func<Task>>();
    private bool _processing;

    /// <summary>Queues <paramref name="job"/> and runs it after every previously queued job.</summary>
    public Task Run(Func<Task> job)
    {
        if (job is null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        var completion = new TaskCompletionSource<bool>();
        lock (_gate)
        {
            _queue.Enqueue(async delegate
            {
                try
                {
                    await job().ConfigureAwait(false);
                    completion.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            });
        }

        Start();
        return completion.Task;
    }

    private void Start()
    {
        lock (_gate)
        {
            if (_processing)
            {
                return;
            }

            _processing = true;
        }

        _ = DrainAsync();
    }

    private async Task DrainAsync()
    {
        await Task.Yield();
        try
        {
            while (true)
            {
                Func<Task>? job;
                lock (_gate)
                {
                    if (_queue.Count == 0)
                    {
                        _processing = false;
                        return;
                    }

                    job = _queue.Dequeue();
                }

                await job().ConfigureAwait(true);
            }
        }
        catch (Exception)
        {
            lock (_gate)
            {
                _processing = false;
            }
        }
    }
}
