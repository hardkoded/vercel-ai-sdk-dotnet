// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>
/// The interrupt of one model call of a step. It watches the steering run until the model call produces output, and cancels the model call
/// when an interrupting message arrives. Maps to the <c>interruptController</c>, <c>watchInterrupt</c> and <c>modelCallAbortSignal</c> that
/// <c>generateText</c> and <c>streamText</c> create for each model call.
/// </summary>
internal sealed class StepInterrupt : IDisposable
{
    private readonly AbortController _controller = new();
    private readonly CancellationTokenSource _cancellation;
    private readonly TaskCompletionSource<bool> _requested = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );
    private readonly Action _stopWatching;
    private int _isRequested;
    private int _isClosed;

    public StepInterrupt(SteeringRun steeringRun, AbortSignal? abortSignal, CancellationToken token)
    {
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        Signal = MergeAbortSignals.Merge(abortSignal, _controller.Signal)!;
        _stopWatching = steeringRun.WatchInterrupt(Interrupt);
    }

    /// <summary>The abort signal of the caller and the timeouts together with this interrupt.</summary>
    public AbortSignal Signal { get; }

    /// <summary>The token of the caller together with this interrupt.</summary>
    public CancellationToken Token => _cancellation.Token;

    /// <summary>Completes when an interrupting message cancelled the model call.</summary>
    public Task Requested => _requested.Task;

    /// <summary>Whether an interrupting message cancelled the model call.</summary>
    public bool IsRequested => Volatile.Read(ref _isRequested) == 1;

    /// <summary>Stops watching, because the model call produced output.</summary>
    public void StopWatching() => _stopWatching();

    /// <summary>
    /// Ends the interrupt window, because the model call produced output. Returns <c>false</c> when an interrupt won the race and already
    /// cancelled the model call, so the output must be discarded.
    /// </summary>
    public bool TryClose()
    {
        var closed = Interlocked.CompareExchange(ref _isClosed, 1, 0) == 0 && !IsRequested;
        _stopWatching();
        return closed;
    }

    public void Dispose()
    {
        _stopWatching();
        _cancellation.Dispose();
    }

    private void Interrupt()
    {
        if (Interlocked.CompareExchange(ref _isClosed, 1, 0) != 0)
        {
            return;
        }

        Volatile.Write(ref _isRequested, 1);
        _requested.TrySetResult(true);
        try
        {
            _cancellation.Cancel();
        }
        catch (Exception exception)
            when (exception is AggregateException or ObjectDisposedException)
        {
            // A callback of the token threw, or the model call ended. The token is cancelled either way.
        }

        try
        {
            _controller.Abort(null);
        }
        catch (Exception)
        {
            // A handler of the model call threw. The steer call never faults.
        }
    }
}
