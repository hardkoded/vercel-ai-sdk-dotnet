// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>One timed transcript segment.</summary>
public sealed class TranscriptSegment
{
    /// <summary>Creates a segment.</summary>
    public TranscriptSegment(string text, double startSecond, double endSecond)
    {
        Text = text ?? string.Empty;
        StartSecond = startSecond;
        EndSecond = endSecond;
    }

    /// <summary>Segment text.</summary>
    public string Text { get; }

    /// <summary>Start time in seconds.</summary>
    public double StartSecond { get; }

    /// <summary>End time in seconds.</summary>
    public double EndSecond { get; }
}

/// <summary>A part emitted by a transcription or translation model stream.</summary>
public sealed class ModelStreamPart
{
    /// <summary>Part type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Part id.</summary>
    public string? Id { get; set; }

    /// <summary>Final text.</summary>
    public string? Text { get; set; }

    /// <summary>Text delta.</summary>
    public string? Delta { get; set; }

    /// <summary>Transcript segments.</summary>
    public IReadOnlyList<TranscriptSegment>? Segments { get; set; }

    /// <summary>Detected language.</summary>
    public string? Language { get; set; }

    /// <summary>Duration in seconds.</summary>
    public double? DurationInSeconds { get; set; }

    /// <summary>Warnings carried by <c>stream-start</c>.</summary>
    public IReadOnlyList<OperationWarning>? Warnings { get; set; }

    /// <summary>Response timestamp.</summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>Response model id.</summary>
    public string? ModelId { get; set; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>Raw provider chunk.</summary>
    public JsonElement? Raw { get; set; }

    /// <summary>Error carried by an <c>error</c> part.</summary>
    public Exception? Error { get; set; }

    /// <summary>Translated audio bytes.</summary>
    public byte[]? Audio { get; set; }

    /// <summary>Source transcript on <c>finish</c>.</summary>
    public string? SourceText { get; set; }

    /// <summary>Translated text on <c>finish</c>.</summary>
    public string? OutputText { get; set; }

    /// <summary>Usage on <c>finish</c>.</summary>
    public OperationUsage? Usage { get; set; }
}

/// <summary>Audio the model may take ownership of.</summary>
public sealed class AudioChunkStream
{
    /// <summary>True after the model takes a reader.</summary>
    public bool Locked { get; private set; }

    /// <summary>Reason from a failed setup cancel.</summary>
    public Exception? CancelError { get; private set; }

    /// <summary>Marks the stream as owned by the model.</summary>
    public void Lock()
    {
        Locked = true;
    }

    /// <summary>Cancels an unowned stream. A locked stream throws.</summary>
    public void Cancel(Exception error)
    {
        if (Locked)
        {
            throw new InvalidOperationException("The audio stream is locked.");
        }

        CancelError = error;
    }
}

/// <summary>Model stream that stays open until <see cref="Cancel"/> when <see cref="Completes"/> is false.</summary>
public sealed class ModelPartStream
{
    private readonly IReadOnlyList<ModelStreamPart> _parts;
    private readonly TaskCompletionSource<bool> _done = new TaskCompletionSource<bool>();

    /// <summary>Creates a scripted stream.</summary>
    public ModelPartStream(IReadOnlyList<ModelStreamPart> parts, bool completes = true, Exception? failure = null)
    {
        _parts = parts ?? Array.Empty<ModelStreamPart>();
        Completes = completes;
        Failure = failure;
    }

    /// <summary>True when the stream ends after <see cref="Parts"/>.</summary>
    public bool Completes { get; }

    /// <summary>Error raised after the scripted parts.</summary>
    public Exception? Failure { get; }

    /// <summary>Scripted parts.</summary>
    public IReadOnlyList<ModelStreamPart> Parts
    {
        get { return _parts; }
    }

    /// <summary>True after <see cref="Cancel"/>.</summary>
    public bool Cancelled { get; private set; }

    /// <summary>Stops a stream that has not completed.</summary>
    public void Cancel()
    {
        Cancelled = true;
        _done.TrySetResult(true);
    }

    /// <summary>Reads the scripted parts, then waits when the stream stays open.</summary>
    public async IAsyncEnumerable<ModelStreamPart> ReadAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var part in _parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return part;
        }

        if (Failure != null)
        {
            throw Failure;
        }

        if (!Completes)
        {
            using (cancellationToken.Register(() => _done.TrySetResult(true)))
            {
                await _done.Task.ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}

/// <summary>A single-consumer stream with an explicit <see cref="Cancel"/>.</summary>
public sealed class OperationFullStream<T> : IAsyncEnumerable<T>
{
    private readonly Func<IAsyncEnumerator<T>> _open;
    private readonly Action _cancel;

    /// <summary>Creates a stream.</summary>
    public OperationFullStream(Func<IAsyncEnumerator<T>> open, Action cancel)
    {
        _open = open;
        _cancel = cancel;
    }

    /// <summary>Cancels the stream and aborts a pending model call.</summary>
    public void Cancel()
    {
        _cancel();
    }

    /// <summary>Returns the single enumerator.</summary>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return _open();
    }
}

/// <summary>A promise that can be resolved once.</summary>
public sealed class PendingPromise<T>
{
    private readonly TaskCompletionSource<T> _source = new TaskCompletionSource<T>();

    /// <summary>True until <see cref="Resolve"/> or <see cref="Reject"/>.</summary>
    public bool IsPending { get; private set; } = true;

    /// <summary>True after <see cref="Resolve"/>.</summary>
    public bool IsResolved { get; private set; }

    /// <summary>The underlying task.</summary>
    public Task<T> Task
    {
        get { return _source.Task; }
    }

    /// <summary>Resolves the promise when it is still pending.</summary>
    public void Resolve(T value)
    {
        if (!IsPending)
        {
            return;
        }

        IsPending = false;
        IsResolved = true;
        _source.TrySetResult(value);
    }

    /// <summary>Rejects the promise when it is still pending.</summary>
    public void Reject(Exception error)
    {
        if (!IsPending)
        {
            return;
        }

        IsPending = false;
        _source.TrySetException(error);
    }
}

/// <summary>Async queue used to hand stream parts to one consumer.</summary>
public sealed class PartQueue<T>
{
    private readonly Queue<T> _items = new Queue<T>();
    private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
    private Exception? _error;
    private bool _completed;

    /// <summary>Adds a part.</summary>
    public void Enqueue(T item)
    {
        TaskCompletionSource<bool>? waiter = null;
        lock (_items)
        {
            if (_completed)
            {
                return;
            }

            _items.Enqueue(item);
            if (_waiters.Count > 0)
            {
                waiter = _waiters.Dequeue();
            }
        }

        waiter?.TrySetResult(true);
    }

    /// <summary>Completes the queue, optionally with an error.</summary>
    public void Complete(Exception? error)
    {
        List<TaskCompletionSource<bool>> waiters;
        lock (_items)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _error = error;
            waiters = new List<TaskCompletionSource<bool>>(_waiters);
            _waiters.Clear();
        }

        foreach (var waiter in waiters)
        {
            waiter.TrySetResult(true);
        }
    }

    /// <summary>Reads the next part. Returns false when the queue is complete.</summary>
    public async Task<bool> WaitAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            Task wait;
            lock (_items)
            {
                if (_items.Count > 0)
                {
                    return true;
                }

                if (_completed)
                {
                    if (_error != null)
                    {
                        throw _error;
                    }

                    return false;
                }

                var source = new TaskCompletionSource<bool>();
                _waiters.Enqueue(source);
                wait = source.Task;
            }

            if (cancellationToken.CanBeCanceled)
            {
                var aborted = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(() => aborted.TrySetResult(true)))
                {
                    var finished = await Task.WhenAny(wait, aborted.Task).ConfigureAwait(false);
                    if (finished != wait)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            else
            {
                await wait.ConfigureAwait(false);
            }
        }
    }

    /// <summary>Removes the next part. Call after <see cref="WaitAsync"/> returns true.</summary>
    public T Dequeue()
    {
        lock (_items)
        {
            return _items.Dequeue();
        }
    }
}
