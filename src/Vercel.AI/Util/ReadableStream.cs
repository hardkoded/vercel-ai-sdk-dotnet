// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>One read from a <see cref="ReadableStream{T}"/>.</summary>
public struct StreamRead<T>
{
    /// <summary>The stream is exhausted.</summary>
    public bool Done { get; set; }

    /// <summary>Whether <see cref="Value"/> was produced.</summary>
    public bool HasValue { get; set; }

    /// <summary>Chunk, when <see cref="HasValue"/> is true.</summary>
    public T Value { get; set; }

    /// <summary>The stream failed.</summary>
    public bool Rejected { get; set; }

    /// <summary>Failure reason. <c>null</c> is the JavaScript <c>undefined</c> error.</summary>
    public object? Reason { get; set; }

    /// <summary>A successful chunk.</summary>
    public static StreamRead<T> Chunk(T value)
    {
        return new StreamRead<T> { HasValue = true, Value = value };
    }

    /// <summary>A completed read.</summary>
    public static StreamRead<T> Complete()
    {
        return new StreamRead<T> { Done = true };
    }

    /// <summary>A failed read.</summary>
    public static StreamRead<T> Fail(object? reason)
    {
        return new StreamRead<T> { Rejected = true, Reason = reason };
    }
}

/// <summary>Rejection whose reason is JavaScript <c>undefined</c>.</summary>
public sealed class UndefinedStreamError : Exception
{
    /// <summary>Creates the error.</summary>
    public UndefinedStreamError()
        : base("undefined")
    {
    }

    /// <summary>Always <c>null</c>, matching a missing rejection reason.</summary>
    public object? Reason
    {
        get { return null; }
    }
}

/// <summary>Result of one async-iterator step.</summary>
public struct IteratorResult<T>
{
    /// <summary>Whether iteration is finished.</summary>
    public bool Done { get; set; }

    /// <summary>Whether <see cref="Value"/> is present.</summary>
    public bool HasValue { get; set; }

    /// <summary>Yielded value.</summary>
    public T Value { get; set; }

    /// <summary>A finished step.</summary>
    public static IteratorResult<T> Complete()
    {
        return new IteratorResult<T> { Done = true };
    }

    /// <summary>A value step.</summary>
    public static IteratorResult<T> Of(T value)
    {
        return new IteratorResult<T> { HasValue = true, Value = value };
    }
}

/// <summary>Enqueues chunks into a <see cref="ReadableStream{T}"/>.</summary>
public sealed class ReadableStreamController<T>
{
    private readonly ReadableStream<T> _stream;

    internal ReadableStreamController(ReadableStream<T> stream)
    {
        _stream = stream;
    }

    /// <summary>Queues <paramref name="chunk"/> for the next reader.</summary>
    public void Enqueue(T chunk)
    {
        _stream.Enqueue(chunk);
    }

    /// <summary>Closes the stream.</summary>
    public void Close()
    {
        _stream.Close();
    }

    /// <summary>Fails the stream. <paramref name="reason"/> may be <c>null</c>.</summary>
    public void Error(object? reason)
    {
        _stream.Fail(reason);
    }
}

/// <summary>Default reader for a <see cref="ReadableStream{T}"/>.</summary>
public sealed class ReadableStreamReader<T>
{
    private readonly ReadableStream<T> _stream;

    internal ReadableStreamReader(ReadableStream<T> stream)
    {
        _stream = stream;
    }

    /// <summary>Reads the next chunk, a completion, or a failure.</summary>
    public Task<StreamRead<T>> ReadAsync()
    {
        return _stream.ReadAsync();
    }

    /// <summary>Cancels the stream and releases this reader.</summary>
    public Task CancelAsync()
    {
        return _stream.CancelFromReaderAsync(this);
    }

    /// <summary>Releases the lock without cancelling.</summary>
    public void ReleaseLock()
    {
        _stream.Release(this);
    }
}

/// <summary>
/// Pull-based readable stream. Supports enqueue, close, error, backpressure via pull, and a single reader.
/// </summary>
public class ReadableStream<T>
{
    private readonly object _gate = new object();
    private readonly Queue<T> _queue = new Queue<T>();
    private readonly Queue<TaskCompletionSource<StreamRead<T>>> _waiters = new Queue<TaskCompletionSource<StreamRead<T>>>();
    private readonly Func<ReadableStreamController<T>, Task>? _pull;
    private readonly Func<Task>? _onCancel;
    private readonly ReadableStreamController<T> _controller;
    private ReadableStreamReader<T>? _reader;
    private bool _pulling;
    private bool _closed;
    private bool _errored;
    private bool _cancelled;
    private bool _hasError;
    private object? _error;

    /// <summary>Creates a stream. <paramref name="start"/> runs before the constructor returns.</summary>
    public ReadableStream(Action<ReadableStreamController<T>>? start = null, Func<ReadableStreamController<T>, Task>? pull = null, Func<Task>? cancel = null)
    {
        _pull = pull;
        _onCancel = cancel;
        _controller = new ReadableStreamController<T>(this);
        if (start != null)
        {
            start(_controller);
        }
    }

    /// <summary>Whether a reader currently holds the lock.</summary>
    public bool Locked
    {
        get
        {
            lock (_gate)
            {
                return _reader != null;
            }
        }
    }

    /// <summary>Locks the stream and returns its reader.</summary>
    public ReadableStreamReader<T> GetReader()
    {
        lock (_gate)
        {
            if (_reader != null)
            {
                throw new InvalidOperationException("The stream is locked.");
            }

            _reader = new ReadableStreamReader<T>(this);
            return _reader;
        }
    }

    /// <summary>Cancels the stream. Throws when a reader holds the lock.</summary>
    public Task CancelAsync()
    {
        lock (_gate)
        {
            if (_reader != null)
            {
                throw new InvalidOperationException("The stream is locked.");
            }
        }

        return CancelCoreAsync();
    }

    /// <summary>Reads every chunk into a list.</summary>
    public async Task<List<T>> ToArrayAsync()
    {
        var reader = GetReader();
        var values = new List<T>();
        while (true)
        {
            var read = await reader.ReadAsync().ConfigureAwait(false);
            if (read.Rejected)
            {
                reader.ReleaseLock();
                var error = read.Reason as Exception;
                if (error != null)
                {
                    throw error;
                }

                throw new UndefinedStreamError();
            }

            if (read.Done)
            {
                reader.ReleaseLock();
                return values;
            }

            values.Add(read.Value);
        }
    }

    /// <summary>A stream that emits <paramref name="values"/> and then closes.</summary>
    public static ReadableStream<T> FromArray(IReadOnlyList<T>? values)
    {
        return new ReadableStream<T>(delegate (ReadableStreamController<T> controller)
        {
            if (values != null)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    controller.Enqueue(values[i]);
                }
            }

            controller.Close();
        });
    }

    internal void Enqueue(T chunk)
    {
        lock (_gate)
        {
            if (_closed || _errored || _cancelled)
            {
                return;
            }

            if (_waiters.Count > 0)
            {
                _waiters.Dequeue().TrySetResult(StreamRead<T>.Chunk(chunk));
                return;
            }

            _queue.Enqueue(chunk);
        }
    }

    internal void Close()
    {
        List<TaskCompletionSource<StreamRead<T>>> waiters;
        lock (_gate)
        {
            if (_closed || _errored || _cancelled)
            {
                return;
            }

            _closed = true;
            waiters = DrainWaiters();
        }

        Complete(waiters, StreamRead<T>.Complete());
    }

    internal void Fail(object? reason)
    {
        List<TaskCompletionSource<StreamRead<T>>> waiters;
        lock (_gate)
        {
            if (_errored || _cancelled)
            {
                return;
            }

            _errored = true;
            _hasError = true;
            _error = reason;
            waiters = DrainWaiters();
        }

        Complete(waiters, StreamRead<T>.Fail(reason));
    }

    internal async Task<StreamRead<T>> ReadAsync()
    {
        Task<StreamRead<T>> pending;
        lock (_gate)
        {
            StreamRead<T> ready;
            if (TryDequeue(out ready))
            {
                return ready;
            }

            var source = new TaskCompletionSource<StreamRead<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Enqueue(source);
            pending = source.Task;
        }

        Pump();
        return await pending.ConfigureAwait(false);
    }

    internal void Release(ReadableStreamReader<T> reader)
    {
        lock (_gate)
        {
            if (_reader == reader)
            {
                _reader = null;
            }
        }
    }

    internal async Task CancelFromReaderAsync(ReadableStreamReader<T> reader)
    {
        Release(reader);
        await CancelCoreAsync().ConfigureAwait(false);
    }

    private async Task CancelCoreAsync()
    {
        Func<Task>? onCancel = null;
        lock (_gate)
        {
            if (_closed || _errored || _cancelled)
            {
                return;
            }

            _cancelled = true;
            var waiters = DrainWaiters();
            Complete(waiters, StreamRead<T>.Complete());
            onCancel = _onCancel;
        }

        if (onCancel != null)
        {
            try
            {
                await onCancel().ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }
    }

    private bool TryDequeue(out StreamRead<T> read)
    {
        if (_queue.Count > 0)
        {
            read = StreamRead<T>.Chunk(_queue.Dequeue());
            return true;
        }

        if (_hasError)
        {
            read = StreamRead<T>.Fail(_error);
            return true;
        }

        if (_closed || _cancelled)
        {
            read = StreamRead<T>.Complete();
            return true;
        }

        read = default(StreamRead<T>);
        return false;
    }

    private List<TaskCompletionSource<StreamRead<T>>> DrainWaiters()
    {
        var waiters = new List<TaskCompletionSource<StreamRead<T>>>();
        while (_waiters.Count > 0)
        {
            waiters.Add(_waiters.Dequeue());
        }

        return waiters;
    }

    private static void Complete(List<TaskCompletionSource<StreamRead<T>>>? waiters, StreamRead<T> read)
    {
        if (waiters == null)
        {
            return;
        }

        for (var i = 0; i < waiters.Count; i++)
        {
            waiters[i].TrySetResult(read);
        }
    }

    private void Pump()
    {
        Func<ReadableStreamController<T>, Task> pull;
        lock (_gate)
        {
            var candidate = _pull;
            if (_pulling || candidate == null || _closed || _errored || _cancelled || _waiters.Count == 0)
            {
                return;
            }

            pull = candidate;
            _pulling = true;
        }

        pull(_controller).ContinueWith(delegate (Task task)
        {
            var exception = task.Exception;
            if (task.IsFaulted && exception != null)
            {
                var error = exception.GetBaseException();
                _controller.Error(error);
            }

            lock (_gate)
            {
                _pulling = false;
            }

            Pump();
        }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
    }
}

/// <summary>Async iterator over a <see cref="ReadableStream{T}"/>.</summary>
public sealed class StreamIterator<T>
{
    private readonly ReadableStreamReader<T> _reader;
    private bool _finished;

    internal StreamIterator(ReadableStream<T> stream)
    {
        _reader = stream.GetReader();
    }

    /// <summary>Reads the next chunk.</summary>
    public async Task<IteratorResult<T>> NextAsync()
    {
        if (_finished)
        {
            return IteratorResult<T>.Complete();
        }

        var read = await _reader.ReadAsync().ConfigureAwait(false);
        if (read.Rejected)
        {
            await CleanupAsync(false).ConfigureAwait(false);
            var error = read.Reason as Exception;
            if (error != null)
            {
                throw error;
            }

            throw new UndefinedStreamError();
        }

        if (read.Done)
        {
            await CleanupAsync(true).ConfigureAwait(false);
            return IteratorResult<T>.Complete();
        }

        return IteratorResult<T>.Of(read.Value);
    }

    /// <summary>Stops iteration and cancels the stream.</summary>
    public async Task<IteratorResult<T>> ReturnAsync()
    {
        await CleanupAsync(true).ConfigureAwait(false);
        return IteratorResult<T>.Complete();
    }

    /// <summary>Stops iteration, cancels the stream, and throws <paramref name="error"/>.</summary>
    public async Task ThrowAsync(Exception error)
    {
        await CleanupAsync(true).ConfigureAwait(false);
        throw error;
    }

    private async Task CleanupAsync(bool cancelStream)
    {
        lock (this)
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
        }

        if (cancelStream)
        {
            await _reader.CancelAsync().ConfigureAwait(false);
            return;
        }

        _reader.ReleaseLock();
    }
}

/// <summary>A readable stream that can also be enumerated with <c>await foreach</c>.</summary>
public sealed class AsyncIterableStream<T> : IAsyncEnumerable<T>
{
    /// <summary>Creates an iterable view of <paramref name="stream"/>.</summary>
    public AsyncIterableStream(ReadableStream<T> stream)
    {
        Stream = stream;
    }

    /// <summary>Underlying stream.</summary>
    public ReadableStream<T> Stream { get; }

    /// <summary>Whether a reader holds the lock.</summary>
    public bool Locked
    {
        get { return Stream.Locked; }
    }

    /// <summary>Locks the stream.</summary>
    public ReadableStreamReader<T> GetReader()
    {
        return Stream.GetReader();
    }

    /// <summary>Cancels the stream.</summary>
    public Task CancelAsync()
    {
        return Stream.CancelAsync();
    }

    /// <summary>Starts an iterator that cancels the stream when exited early.</summary>
    public StreamIterator<T> GetIterator()
    {
        return new StreamIterator<T>(Stream);
    }

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new Enumerator(GetIterator());
    }

    /// <summary>Reads every value.</summary>
    public async Task<List<T>> ToArrayAsync()
    {
        var values = new List<T>();
        var enumerator = GetAsyncEnumerator();
        try
        {
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                values.Add(enumerator.Current);
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }

        return values;
    }

    private sealed class Enumerator : IAsyncEnumerator<T>
    {
        private readonly StreamIterator<T> _iterator;
        private T _current = default!;

        public Enumerator(StreamIterator<T> iterator)
        {
            _iterator = iterator;
        }

        public T Current
        {
            get { return _current; }
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            var next = await _iterator.NextAsync().ConfigureAwait(false);
            if (next.Done)
            {
                return false;
            }

            _current = next.Value;
            return true;
        }

        public async ValueTask DisposeAsync()
        {
            await _iterator.ReturnAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>Wraps readable streams so they can be consumed with async iteration.</summary>
public static class AsyncIterableStreams
{
    /// <summary>Attaches an async iterator to <paramref name="stream"/> without inserting another transform.</summary>
    public static AsyncIterableStream<T> AsAsyncIterableStream<T>(ReadableStream<T> stream)
    {
        return new AsyncIterableStream<T>(stream);
    }

    /// <summary>
    /// Pipes <paramref name="source"/> through an identity transform and returns an async iterable.
    /// Cancelling the result cancels <paramref name="source"/>.
    /// </summary>
    public static AsyncIterableStream<T> CreateAsyncIterableStream<T>(ReadableStream<T> source)
    {
        var reader = source.GetReader();
        var outer = new ReadableStream<T>(
            pull: async delegate (ReadableStreamController<T> controller)
            {
                var read = await reader.ReadAsync().ConfigureAwait(false);
                if (read.Rejected)
                {
                    controller.Error(read.Reason);
                    return;
                }

                if (read.Done)
                {
                    controller.Close();
                    return;
                }

                controller.Enqueue(read.Value);
            },
            cancel: delegate { return reader.CancelAsync(); });
        return new AsyncIterableStream<T>(outer);
    }
}
