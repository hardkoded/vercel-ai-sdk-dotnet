// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>One read from an <see cref="AsyncIterableStream{T}"/>. A finished read has <see cref="HasValue"/> false.</summary>
public readonly struct StreamRead<T>
{
    private StreamRead(bool done, bool hasValue, T value)
    {
        Done = done;
        HasValue = hasValue;
        Value = value;
    }

    /// <summary>Whether the stream has no further values.</summary>
    public bool Done { get; }

    /// <summary>Whether <see cref="Value"/> was produced by the stream.</summary>
    public bool HasValue { get; }

    /// <summary>The value, when <see cref="HasValue"/> is true.</summary>
    public T Value { get; }

    /// <summary>A value read.</summary>
    public static StreamRead<T> Item(T value)
    {
        return new StreamRead<T>(false, true, value);
    }

    /// <summary>A finished read. Maps to <c>{ done: true, value: undefined }</c>.</summary>
    public static StreamRead<T> End()
    {
        return new StreamRead<T>(true, false, default(T)!);
    }
}

/// <summary>
/// A stream that can be read with a reader or with <c>await foreach</c>.
/// Maps to <c>ReadableStream</c> plus <c>AsyncIterableStream</c>.
/// </summary>
public sealed class AsyncIterableStream<T> : IAsyncEnumerable<T>
{
    private readonly object _gate = new object();
    private readonly Queue<T> _buffer = new Queue<T>();
    private readonly Queue<TaskCompletionSource<StreamRead<T>>> _waiters = new Queue<TaskCompletionSource<StreamRead<T>>>();
    private Exception? _error;
    private bool _hasError;
    private bool _completed;
    private bool _cancelled;
    private int _lockCount;
    private Action? _onWaiter;

    /// <summary>Called when the stream is canceled before it has finished.</summary>
    public Action? OnCancel { get; set; }

    /// <summary>Whether a reader currently holds the stream.</summary>
    public bool IsLocked
    {
        get
        {
            lock (_gate)
            {
                return _lockCount > 0;
            }
        }
    }

    /// <summary>Enqueues <paramref name="item"/> for the next reader.</summary>
    public void Enqueue(T item)
    {
        TaskCompletionSource<StreamRead<T>>? waiter = null;
        lock (_gate)
        {
            if (_completed || _hasError || _cancelled)
            {
                return;
            }

            if (_waiters.Count > 0)
            {
                waiter = _waiters.Dequeue();
            }
            else
            {
                _buffer.Enqueue(item);
            }
        }

        if (waiter is not null)
        {
            waiter.TrySetResult(StreamRead<T>.Item(item));
        }
    }

    /// <summary>Marks the stream finished after the buffered values are read.</summary>
    public void Complete()
    {
        List<TaskCompletionSource<StreamRead<T>>> waiters;
        lock (_gate)
        {
            if (_completed || _hasError)
            {
                return;
            }

            _completed = true;
            waiters = TakeWaiters();
        }

        CompleteWaiters(waiters);
    }

    /// <summary>Fails pending and future reads with <paramref name="error"/> without canceling the stream.</summary>
    public void Fail(Exception error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        List<TaskCompletionSource<StreamRead<T>>> waiters;
        lock (_gate)
        {
            if (_completed || _hasError)
            {
                return;
            }

            _hasError = true;
            _error = error;
            _buffer.Clear();
            waiters = TakeWaiters();
        }

        for (var i = 0; i < waiters.Count; i++)
        {
            waiters[i].TrySetException(error);
        }
    }

    /// <summary>Cancels the stream. Throws when a reader still holds the lock.</summary>
    public void Cancel()
    {
        lock (_gate)
        {
            if (_lockCount > 0)
            {
                throw new InvalidOperationException("Cannot cancel a locked stream.");
            }
        }

        CancelCore();
    }

    /// <summary>Creates a stream that emits <paramref name="values"/> and then finishes.</summary>
    public static AsyncIterableStream<T> From(IEnumerable<T> values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var stream = new AsyncIterableStream<T>();
        foreach (var value in values)
        {
            stream.Enqueue(value);
        }

        stream.Complete();
        return stream;
    }

    /// <summary>Locks the stream and returns its reader.</summary>
    public StreamReader GetReader()
    {
        lock (_gate)
        {
            if (_lockCount > 0)
            {
                throw new InvalidOperationException("The stream is locked.");
            }

            _lockCount++;
        }

        return new StreamReader(this);
    }

    /// <summary>Enumerates the stream. Disposing the enumerator early cancels it.</summary>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new Enumerator(GetReader(), cancellationToken);
    }

    internal void SetWaiterCallback(Action? onWaiter)
    {
        lock (_gate)
        {
            _onWaiter = onWaiter;
        }
    }

    internal bool NeedsData
    {
        get
        {
            lock (_gate)
            {
                return _waiters.Count > 0 && _buffer.Count == 0 && !_completed && !_hasError && !_cancelled;
            }
        }
    }

    internal void ReleaseLock()
    {
        lock (_gate)
        {
            if (_lockCount > 0)
            {
                _lockCount--;
            }
        }
    }

    internal Task<StreamRead<T>> PullAsync()
    {
        TaskCompletionSource<StreamRead<T>>? waiter = null;
        Action? notify = null;
        lock (_gate)
        {
            if (_buffer.Count > 0)
            {
                return Task.FromResult(StreamRead<T>.Item(_buffer.Dequeue()));
            }

            if (_hasError)
            {
                return Task.FromException<StreamRead<T>>(_error ?? new InvalidOperationException("The stream failed."));
            }

            if (_completed || _cancelled)
            {
                return Task.FromResult(StreamRead<T>.End());
            }

            waiter = new TaskCompletionSource<StreamRead<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Enqueue(waiter);
            notify = _onWaiter;
        }

        if (notify is not null)
        {
            notify();
        }

        return waiter.Task;
    }

    internal void CancelFromReader()
    {
        CancelCore();
    }

    private void CancelCore()
    {
        List<TaskCompletionSource<StreamRead<T>>> waiters;
        Action? onCancel = null;
        lock (_gate)
        {
        if (_cancelled || _hasError)
        {
            return;
        }

        if (_completed && _buffer.Count == 0)
        {
            return;
        }

        var alreadyClosed = _completed;
        _cancelled = true;
        _completed = true;
        _buffer.Clear();
        if (!alreadyClosed)
        {
            onCancel = OnCancel;
        }

        waiters = TakeWaiters();
        }

        if (onCancel is not null)
        {
            onCancel();
        }

        CompleteWaiters(waiters);
    }

    private List<TaskCompletionSource<StreamRead<T>>> TakeWaiters()
    {
        var waiters = new List<TaskCompletionSource<StreamRead<T>>>(_waiters.Count);
        while (_waiters.Count > 0)
        {
            waiters.Add(_waiters.Dequeue());
        }

        return waiters;
    }

    private static void CompleteWaiters(List<TaskCompletionSource<StreamRead<T>>> waiters)
    {
        for (var i = 0; i < waiters.Count; i++)
        {
            waiters[i].TrySetResult(StreamRead<T>.End());
        }
    }

    /// <summary>A locked reader. <see cref="ReturnAsync"/> and <see cref="ThrowAsync"/> cancel the stream.</summary>
    public sealed class StreamReader
    {
        private readonly AsyncIterableStream<T> _stream;
        private bool _finished;

        internal StreamReader(AsyncIterableStream<T> stream)
        {
            _stream = stream;
        }

        /// <summary>Reads the next value, or a finished read when the stream is done.</summary>
        public async Task<StreamRead<T>> NextAsync()
        {
            if (_finished)
            {
                return StreamRead<T>.End();
            }

            try
            {
                var result = await _stream.PullAsync().ConfigureAwait(false);
                if (result.Done)
                {
                    await CleanupAsync(cancelStream: true).ConfigureAwait(false);
                }

                return result;
            }
            catch (Exception)
            {
                await CleanupAsync(cancelStream: false).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>Stops reading and cancels the stream when it has not already finished.</summary>
        public async Task<StreamRead<T>> ReturnAsync()
        {
            await CleanupAsync(cancelStream: true).ConfigureAwait(false);
            return StreamRead<T>.End();
        }

        /// <summary>Cancels the stream and throws <paramref name="error"/>.</summary>
        public async Task ThrowAsync(Exception error)
        {
            await CleanupAsync(cancelStream: true).ConfigureAwait(false);
            throw error;
        }

        /// <summary>Drops the lock without canceling.</summary>
        public void Release()
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            _stream.ReleaseLock();
        }

        private async Task CleanupAsync(bool cancelStream)
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            try
            {
                if (cancelStream)
                {
                    _stream.CancelFromReader();
                }
            }
            finally
            {
                _stream.ReleaseLock();
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

    private sealed class Enumerator : IAsyncEnumerator<T>
    {
        private readonly StreamReader _reader;
        private readonly CancellationToken _cancellationToken;
        private bool _completed;

        public Enumerator(StreamReader reader, CancellationToken cancellationToken)
        {
            _reader = reader;
            _cancellationToken = cancellationToken;
        }

        public T Current { get; private set; } = default(T)!;

        public async ValueTask<bool> MoveNextAsync()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var next = await _reader.NextAsync().ConfigureAwait(false);
            if (next.Done)
            {
                _completed = true;
                return false;
            }

            Current = next.Value;
            return true;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_completed)
            {
                await _reader.ReturnAsync().ConfigureAwait(false);
            }
        }
    }
}

/// <summary>Wraps streams so they can be enumerated. Maps to <c>createAsyncIterableStream</c> and <c>asAsyncIterableStream</c>.</summary>
public static class AsyncIterableStreams
{
    /// <summary>Forwards <paramref name="source"/> through a fresh stream that can be enumerated.</summary>
    public static AsyncIterableStream<T> Create<T>(AsyncIterableStream<T> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var output = new AsyncIterableStream<T>();
        var reader = source.GetReader();
        var pulling = 0;
        output.OnCancel = delegate { source.CancelFromReader(); };

        void Kick()
        {
            if (Interlocked.CompareExchange(ref pulling, 1, 0) != 0)
            {
                return;
            }

            _ = PullAsync();
        }

        async Task PullAsync()
        {
            try
            {
                while (output.NeedsData)
                {
                    StreamRead<T> next;
                    try
                    {
                        next = await reader.NextAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        output.Fail(ex);
                        return;
                    }

                    if (next.Done)
                    {
                        output.Complete();
                        return;
                    }

                    output.Enqueue(next.Value);
                }
            }
            finally
            {
                Interlocked.Exchange(ref pulling, 0);
                if (output.NeedsData)
                {
                    Kick();
                }
            }
        }

        output.SetWaiterCallback(Kick);
        return output;
    }

    /// <summary>Returns <paramref name="source"/> unchanged. The stream is already enumerable.</summary>
    public static AsyncIterableStream<T> As<T>(AsyncIterableStream<T> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return source;
    }

}

/// <summary>Emits scripted values with optional delays. Maps to <c>simulateReadableStream</c>.</summary>
public static class ReadableStreams
{
    /// <summary>
    /// Creates a stream of <paramref name="chunks"/>. <paramref name="initialDelayInMs"/> runs before the first
    /// value and <paramref name="chunkDelayInMs"/> runs before each later value. A null delay is passed through
    /// to <paramref name="delay"/> and does not wait. The defaults are zero milliseconds.
    /// </summary>
    public static AsyncIterableStream<T> Simulate<T>(
        IReadOnlyList<T> chunks,
        int? initialDelayInMs = 0,
        int? chunkDelayInMs = 0,
        Func<int?, Task>? delay = null)
    {
        if (chunks is null)
        {
            throw new ArgumentNullException(nameof(chunks));
        }

        var stream = new AsyncIterableStream<T>();
        _ = PumpAsync(stream, chunks, initialDelayInMs, chunkDelayInMs, delay);
        return stream;
    }

    private static async Task PumpAsync<T>(
        AsyncIterableStream<T> stream,
        IReadOnlyList<T> chunks,
        int? initialDelayInMs,
        int? chunkDelayInMs,
        Func<int?, Task>? delay)
    {
        try
        {
            for (var i = 0; i < chunks.Count; i++)
            {
                var milliseconds = i == 0 ? initialDelayInMs : chunkDelayInMs;
                if (delay is not null)
                {
                    await delay(milliseconds).ConfigureAwait(false);
                }
                else if (milliseconds is int wait && wait > 0)
                {
                    await Task.Delay(wait).ConfigureAwait(false);
                }

                stream.Enqueue(chunks[i]);
            }

            stream.Complete();
        }
        catch (Exception ex)
        {
            stream.Fail(ex);
        }
    }
}

/// <summary>
/// Concatenates inner streams one at a time. Maps to <c>createStitchableStream</c>.
/// </summary>
public sealed class StitchableStream<T>
{
    private readonly object _gate = new object();
    private readonly List<Inner> _inners = new List<Inner>();
    private readonly AsyncIterableStream<T> _output = new AsyncIterableStream<T>();
    private TaskCompletionSource<bool> _wait = NewWait();
    private bool _closed;
    private bool _cancelled;
    private int _pulling;

    /// <summary>Creates a stitchable stream.</summary>
    public StitchableStream()
    {
        _output.OnCancel = CancelInners;
        _output.SetWaiterCallback(Kick);
    }

    /// <summary>The outer stream readers consume.</summary>
    public AsyncIterableStream<T> Stream
    {
        get { return _output; }
    }

    /// <summary>Appends <paramref name="inner"/>. Throws after <see cref="Close"/> or <see cref="Terminate"/>.</summary>
    public void AddStream(AsyncIterableStream<T> inner, Action<Exception>? onError = null, Action? onCancel = null)
    {
        if (inner is null)
        {
            throw new ArgumentNullException(nameof(inner));
        }

        lock (_gate)
        {
            if (_cancelled)
            {
                if (onCancel is not null)
                {
                    onCancel();
                }

                inner.Cancel();
                return;
            }

            if (_closed)
            {
                throw new InvalidOperationException("Cannot add inner stream: outer stream is closed");
            }

            _inners.Add(new Inner(inner.GetReader(), onError, onCancel));
            _wait.TrySetResult(true);
        }

        Kick();
    }

    /// <summary>Finishes the outer stream after the inner streams that are already added.</summary>
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
                _output.Complete();
            }
        }

        Kick();
    }

    /// <summary>Cancels every inner stream and finishes the outer stream.</summary>
    public void Terminate()
    {
        Inner[] inners;
        lock (_gate)
        {
            if (_cancelled)
            {
                return;
            }

            _closed = true;
            _wait.TrySetResult(true);
            inners = _inners.ToArray();
            _inners.Clear();
        }

        Cancel(inners);
        _output.Complete();
    }

    private void CancelInners()
    {
        Inner[] inners;
        lock (_gate)
        {
            _cancelled = true;
            _closed = true;
            _wait.TrySetResult(true);
            inners = _inners.ToArray();
            _inners.Clear();
        }

        Cancel(inners);
    }

    private static void Cancel(Inner[] inners)
    {
        for (var i = 0; i < inners.Length; i++)
        {
            inners[i].OnCancel?.Invoke();
            _ = inners[i].Reader.ReturnAsync();
        }
    }

    private void Kick()
    {
        if (Interlocked.CompareExchange(ref _pulling, 1, 0) != 0)
        {
            return;
        }

        _ = PullLoopAsync();
    }

    private async Task PullLoopAsync()
    {
        try
        {
            while (_output.NeedsData)
            {
                if (_cancelled)
                {
                    return;
                }

                Inner? current = null;
                Task? wait = null;
                var finish = false;
                lock (_gate)
                {
                    if (_cancelled)
                    {
                        return;
                    }

                    if (_inners.Count == 0)
                    {
                        if (_closed)
                        {
                            finish = true;
                        }
                        else
                        {
                            _wait = NewWait();
                            wait = _wait.Task;
                        }
                    }
                    else
                    {
                        current = _inners[0];
                    }
                }

                if (finish)
                {
                    _output.Complete();
                    return;
                }

                if (wait is not null || current is null)
                {
                    if (wait is not null)
                    {
                        await wait.ConfigureAwait(false);
                    }

                    continue;
                }

                var active = current;
                StreamRead<T> next;
                try
                {
                    next = await active.Reader.NextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (_cancelled)
                    {
                        return;
                    }

                    if (active.OnError is not null)
                    {
                        active.OnError(ex);
                    }

                    lock (_gate)
                    {
                        if (_inners.Count > 0 && ReferenceEquals(_inners[0], active))
                        {
                            _inners.RemoveAt(0);
                        }
                    }

                    _output.Fail(ex);
                    Terminate();
                    return;
                }

                if (_cancelled)
                {
                    return;
                }

                if (next.Done)
                {
                    lock (_gate)
                    {
                        if (_inners.Count > 0 && ReferenceEquals(_inners[0], active))
                        {
                            _inners.RemoveAt(0);
                        }

                        finish = _inners.Count == 0 && _closed;
                    }

                    if (finish)
                    {
                        _output.Complete();
                        return;
                    }

                    continue;
                }

                _output.Enqueue(next.Value);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _pulling, 0);
            if (_output.NeedsData)
            {
                Kick();
            }
        }
    }

    private static TaskCompletionSource<bool> NewWait()
    {
        return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class Inner
    {
        public Inner(AsyncIterableStream<T>.StreamReader reader, Action<Exception>? onError, Action? onCancel)
        {
            Reader = reader;
            OnError = onError;
            OnCancel = onCancel;
        }

        public AsyncIterableStream<T>.StreamReader Reader { get; }

        public Action<Exception>? OnError { get; }

        public Action? OnCancel { get; }
    }
}
