// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>A stream that reads inner streams one at a time.</summary>
public sealed class StitchableStream<T>
{
    private readonly object _gate = new object();
    private readonly List<Inner> _inners = new List<Inner>();
    private readonly ReadableStream<T> _stream;
    private TaskCompletionSource<bool> _wait = NewWait();
    private bool _closed;
    private bool _cancelled;

    /// <summary>Creates an empty stitchable stream.</summary>
    public StitchableStream()
    {
        _stream = new ReadableStream<T>(pull: PullAsync, cancel: CancelAsync);
    }

    /// <summary>Outer stream.</summary>
    public ReadableStream<T> Stream
    {
        get { return _stream; }
    }

    /// <summary>Queues <paramref name="inner"/> to be read after the streams already added.</summary>
    public void AddStream(ReadableStream<T>? inner, Action<object>? onError = null, Action? onCancel = null)
    {
        if (inner == null)
        {
            throw new ArgumentNullException(nameof(inner));
        }

        lock (_gate)
        {
            if (_cancelled)
            {
                if (onCancel != null)
                {
                    onCancel();
                }
            }
            else if (_closed)
            {
                throw new InvalidOperationException("Cannot add inner stream: outer stream is closed");
            }
            else
            {
                _inners.Add(new Inner(inner.GetReader(), onError, onCancel));
                _wait.TrySetResult(true);
                return;
            }
        }

        inner.CancelAsync();
    }

    /// <summary>Lets current inner streams finish, then closes the outer stream.</summary>
    public void Close()
    {
        lock (_gate)
        {
            if (_cancelled)
            {
                return;
            }

            _closed = true;
            _wait.TrySetResult(true);
            if (_inners.Count == 0)
            {
                _stream.Close();
            }
        }
    }

    /// <summary>Cancels every inner stream and closes the outer stream.</summary>
    public void Terminate()
    {
        List<Inner> inners;
        lock (_gate)
        {
            if (_cancelled)
            {
                return;
            }

            _closed = true;
            _wait.TrySetResult(true);
            inners = new List<Inner>(_inners);
            _inners.Clear();
        }

        for (var i = 0; i < inners.Count; i++)
        {
            var onCancel = inners[i].OnCancel;
            if (onCancel != null)
            {
                onCancel();
            }

            inners[i].Reader.CancelAsync();
        }

        _stream.Close();
    }

    private async Task PullAsync(ReadableStreamController<T> controller)
    {
        while (true)
        {
            if (IsCancelled())
            {
                return;
            }

            Inner? current = null;
            Task? wait = null;
            var closeNow = false;
            lock (_gate)
            {
                if (_cancelled)
                {
                    return;
                }

                if (_closed && _inners.Count == 0)
                {
                    closeNow = true;
                }
                else if (_inners.Count == 0)
                {
                    _wait = NewWait();
                    wait = _wait.Task;
                }
                else
                {
                    current = _inners[0];
                }
            }

            if (closeNow)
            {
                controller.Close();
                return;
            }

            if (wait != null)
            {
                await wait.ConfigureAwait(false);
                continue;
            }

            if (current == null)
            {
                continue;
            }

            StreamRead<T> read;
            try
            {
                read = await current.Reader.ReadAsync().ConfigureAwait(false);
            }
            catch (Exception error)
            {
                Fail(controller, current, error);
                return;
            }

            if (IsCancelled())
            {
                return;
            }

            if (read.Rejected)
            {
                var error = read.Reason as Exception ?? new UndefinedStreamError();
                Fail(controller, current, error);
                return;
            }

            if (read.Done)
            {
                var close = false;
                lock (_gate)
                {
                    if (_inners.Count > 0 && _inners[0] == current)
                    {
                        _inners.RemoveAt(0);
                    }

                    close = _inners.Count == 0 && _closed;
                }

                if (close)
                {
                    controller.Close();
                    return;
                }

                continue;
            }

            controller.Enqueue(read.Value);
            return;
        }
    }

    private Task CancelAsync()
    {
        List<Inner> inners;
        lock (_gate)
        {
            _cancelled = true;
            _closed = true;
            _wait.TrySetResult(true);
            inners = new List<Inner>(_inners);
            _inners.Clear();
        }

        for (var i = 0; i < inners.Count; i++)
        {
            var onCancel = inners[i].OnCancel;
            if (onCancel != null)
            {
                onCancel();
            }

            inners[i].Reader.CancelAsync();
        }

        return Task.CompletedTask;
    }

    private void Fail(ReadableStreamController<T> controller, Inner current, Exception error)
    {
        if (IsCancelled())
        {
            return;
        }

        var onError = current.OnError;
        if (onError != null)
        {
            onError(error);
        }

        controller.Error(error);
        lock (_gate)
        {
            if (_inners.Count > 0 && _inners[0] == current)
            {
                _inners.RemoveAt(0);
            }
        }

        Terminate();
    }

    private bool IsCancelled()
    {
        lock (_gate)
        {
            return _cancelled;
        }
    }

    private static TaskCompletionSource<bool> NewWait()
    {
        return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class Inner
    {
        public Inner(ReadableStreamReader<T> reader, Action<object>? onError, Action? onCancel)
        {
            Reader = reader;
            OnError = onError;
            OnCancel = onCancel;
        }

        public ReadableStreamReader<T> Reader { get; }

        public Action<object>? OnError { get; }

        public Action? OnCancel { get; }
    }
}

/// <summary>Emits scripted chunks, optionally delaying each pull.</summary>
public static class SimulateReadableStream
{
    /// <summary>
    /// Creates a stream of <paramref name="chunks"/>.
    /// A null delay skips the wait. <c>0</c> still invokes the delay with zero.
    /// </summary>
    public static ReadableStream<T> Create<T>(IReadOnlyList<T> chunks, int? initialDelayInMs = 0, int? chunkDelayInMs = 0, Func<int?, Task>? delay = null)
    {
        var index = 0;
        var wait = delay ?? DefaultDelay;
        return new ReadableStream<T>(pull: async delegate (ReadableStreamController<T> controller)
        {
            if (index < chunks.Count)
            {
                await wait(index == 0 ? initialDelayInMs : chunkDelayInMs).ConfigureAwait(false);
                var value = chunks[index];
                index++;
                controller.Enqueue(value);
            }
            else
            {
                controller.Close();
            }
        });
    }

    private static Task DefaultDelay(int? milliseconds)
    {
        if (milliseconds == null || milliseconds.Value <= 0)
        {
            return Task.CompletedTask;
        }

        return Task.Delay(milliseconds.Value);
    }
}
