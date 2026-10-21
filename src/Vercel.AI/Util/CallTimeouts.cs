// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Util;

/// <summary>
/// The abort sources of one <c>GenerateTextAsync</c> or <c>StreamTextAsync</c> call: the caller's signal,
/// the total timeout, the step timeout, and the chunk timeout. Maps to the merged abort signal in <c>generateText</c> and <c>streamText</c>.
/// </summary>
internal sealed class CallTimeouts : IDisposable
{
    private readonly int? _stepMs;
    private readonly int? _chunkMs;
    private readonly AbortController? _stepController;
    private readonly AbortController? _chunkController;
    private readonly Timer? _totalTimer;
    private readonly object _gate = new();
    private CancellationTokenSource? _cancellation;
    private Action? _cancel;
    private CancellationTokenRegistration _tokenRegistration;
    private IDisposable? _stepTimer;
    private IDisposable? _chunkTimer;
    private bool _disposed;

    private readonly List<KeyValuePair<AbortSignal, Action>> _registrations;

    private CallTimeouts(AbortSignal signal, List<KeyValuePair<AbortSignal, Action>> registrations, TimeoutConfiguration timeout, AbortController? stepController, AbortController? chunkController, Timer? totalTimer)
    {
        Signal = signal;
        _registrations = registrations;
        _stepMs = timeout.StepMs;
        _chunkMs = timeout.ChunkMs;
        _stepController = stepController;
        _chunkController = chunkController;
        _totalTimer = totalTimer;
    }

    /// <summary>Aborts when the caller's signal or any timeout aborts.</summary>
    public AbortSignal Signal { get; }

    /// <summary>The <see cref="TimeoutError"/> that aborted the call, or <c>null</c>.</summary>
    public TimeoutError? Expired => Signal.Reason as TimeoutError;

    /// <summary>
    /// Returns the abort sources for a call, or <c>null</c> when the call has no timeout that applies.
    /// Without a timeout the caller's <paramref name="signal"/> and <paramref name="token"/> stay as they are.
    /// </summary>
    public static CallTimeouts? Create(TimeoutConfiguration? timeout, AbortSignal? signal, bool streaming, CancellationToken token = default)
    {
        var totalMs = timeout?.TotalMs;
        var stepMs = timeout?.StepMs;
        var chunkMs = streaming ? timeout?.ChunkMs : null;
        if (timeout == null || (totalMs == null && stepMs == null && chunkMs == null))
        {
            return null;
        }

        var totalController = totalMs != null ? new AbortController() : null;
        var stepController = stepMs != null ? new AbortController() : null;
        var chunkController = chunkMs != null ? new AbortController() : null;
        var tokenController = token.CanBeCanceled ? new AbortController() : null;
        var sources = new List<AbortSignal>();
        foreach (var source in new[] { signal, tokenController?.Signal, totalController?.Signal, stepController?.Signal, chunkController?.Signal })
        {
            if (source != null)
            {
                sources.Add(source);
            }
        }

        // Like AbortSignal.any: one source is used as it is. More sources get one signal that follows them until the call ends.
        var registrations = new List<KeyValuePair<AbortSignal, Action>>();
        var merged = sources[0];
        if (sources.Count > 1)
        {
            var controller = new AbortController();
            merged = controller.Signal;
            foreach (var source in sources)
            {
                var from = source;
                Action follow = () => controller.Abort(from.Reason);
                registrations.Add(new KeyValuePair<AbortSignal, Action>(from, follow));
                from.AddAbortHandler(follow);
            }
        }

        Timer? totalTimer = null;
        if (totalController != null)
        {
            var reason = new TimeoutError("The operation was aborted due to timeout");
            totalTimer = new Timer(_ => totalController.Abort(reason), null, totalMs!.Value, System.Threading.Timeout.Infinite);
        }

        var timeouts = new CallTimeouts(merged, registrations, timeout, stepController, chunkController, totalTimer);
        if (tokenController != null)
        {
            timeouts._tokenRegistration = token.Register(() => tokenController.Abort(new OperationCanceledException(token)));
        }

        return timeouts;
    }

    /// <summary>A token that is cancelled when <see cref="Signal"/> aborts.</summary>
    public CancellationToken Token
    {
        get
        {
            lock (_gate)
            {
                if (_cancellation == null)
                {
                    var cancellation = new CancellationTokenSource();
                    _cancellation = cancellation;
                    _cancel = () =>
                    {
                        try
                        {
                            cancellation.Cancel();
                        }
                        catch (Exception exception) when (exception is AggregateException or ObjectDisposedException)
                        {
                            // A callback of the token threw, or the call ended. The token is cancelled either way.
                        }
                    };
                    Signal.AddAbortHandler(_cancel);
                }

                return _cancellation.Token;
            }
        }
    }

    /// <summary>Starts the step timeout. The budget of an earlier step ends.</summary>
    public void StartStep()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _stepTimer?.Dispose();
            _chunkTimer?.Dispose();
            _chunkTimer = null;
            _stepTimer = SetAbortTimeout.Schedule(_stepController, "Step", _stepMs);
        }
    }

    /// <summary>Stops the step and chunk timeouts. Called when the last step ended.</summary>
    public void EndStep()
    {
        lock (_gate)
        {
            _stepTimer?.Dispose();
            _stepTimer = null;
            _chunkTimer?.Dispose();
            _chunkTimer = null;
        }
    }

    /// <summary>Called for each chunk of model output. Starts the chunk timeout again.</summary>
    public void OnOutputChunk()
    {
        lock (_gate)
        {
            if (_disposed || Signal.Aborted)
            {
                return;
            }

            _chunkTimer?.Dispose();
            _chunkTimer = SetAbortTimeout.Schedule(_chunkController, "Chunk", _chunkMs);
        }
    }

    /// <summary>Stops the chunk timeout. Called when the model response ends.</summary>
    public void ClearChunk()
    {
        lock (_gate)
        {
            _chunkTimer?.Dispose();
            _chunkTimer = null;
        }
    }

    /// <summary>Stops every timer of the call.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
            _stepTimer?.Dispose();
            _stepTimer = null;
            _chunkTimer?.Dispose();
            _chunkTimer = null;
        }

        _totalTimer?.Dispose();
        _tokenRegistration.Dispose();
        foreach (var registration in _registrations)
        {
            registration.Key.RemoveAbortHandler(registration.Value);
        }

        if (_cancel != null)
        {
            Signal.RemoveAbortHandler(_cancel);
        }

        _cancellation?.Dispose();
    }
}
