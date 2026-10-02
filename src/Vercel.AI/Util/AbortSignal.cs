// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Abort reason produced for a timeout. Maps to a <c>TimeoutError</c> <c>DOMException</c>.</summary>
public sealed class TimeoutErrorException : Exception
{
    /// <summary>Creates a timeout error with <paramref name="message"/>.</summary>
    public TimeoutErrorException(string message)
        : base(message)
    {
    }

    /// <summary>The DOM exception name <c>TimeoutError</c>.</summary>
    public string Name
    {
        get { return "TimeoutError"; }
    }
}

/// <summary>
/// A cancelable signal that keeps the first abort reason.
/// Maps to the parts of <c>AbortSignal</c> that <see cref="CancellationToken"/> does not store.
/// </summary>
public sealed class AbortSignal : IDisposable
{
    private readonly CancellationTokenSource _source = new CancellationTokenSource();
    private readonly object _gate = new object();
    private readonly List<CancellationTokenRegistration> _registrations = new List<CancellationTokenRegistration>();
    private Timer? _timer;
    private bool _disposed;

    /// <summary>The token that cancels when <see cref="Abort"/> runs.</summary>
    public CancellationToken Token
    {
        get { return _source.Token; }
    }

    /// <summary>Whether <see cref="Abort"/> has already run.</summary>
    public bool IsCancellationRequested
    {
        get { return _source.IsCancellationRequested; }
    }

    /// <summary>The first reason passed to <see cref="Abort"/>.</summary>
    public object? Reason { get; private set; }

    /// <summary>Cancels <see cref="Token"/> and records <paramref name="reason"/> when this is the first abort.</summary>
    public void Abort(object? reason = null)
    {
        lock (_gate)
        {
            if (_disposed || _source.IsCancellationRequested)
            {
                return;
            }

            Reason = reason;
            try
            {
                _source.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    /// <summary>Creates a signal that aborts after <paramref name="milliseconds"/> with a <see cref="TimeoutErrorException"/>.</summary>
    public static AbortSignal Timeout(int milliseconds)
    {
        var signal = new AbortSignal();
        signal._timer = new Timer(
            _ => signal.Abort(new TimeoutErrorException("The operation was aborted due to timeout")),
            null,
            milliseconds,
            System.Threading.Timeout.Infinite);
        return signal;
    }

    /// <summary>Releases the timeout timer and linked registrations.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_timer is not null)
            {
                _timer.Dispose();
                _timer = null;
            }

            for (var i = 0; i < _registrations.Count; i++)
            {
                _registrations[i].Dispose();
            }

            _registrations.Clear();
            _source.Dispose();
        }
    }

    internal void Link(AbortSignal source)
    {
        var registration = source.Token.Register(delegate
        {
            Abort(source.Reason);
        });
        lock (_gate)
        {
            _registrations.Add(registration);
        }
    }
}

/// <summary>Combines abort sources. Maps to <c>mergeAbortSignals</c> and <c>setAbortTimeout</c>.</summary>
public static class AbortSignals
{
    /// <summary>
    /// Returns a signal that aborts when any source aborts, keeping the first reason.
    /// Null entries are ignored. Integers are timeout durations in milliseconds.
    /// A single valid <see cref="AbortSignal"/> is returned as the same instance.
    /// Returns <c>null</c> when every entry is null.
    /// </summary>
    public static AbortSignal? Merge(params object?[] sources)
    {
        var signals = new List<AbortSignal>();
        if (sources is not null)
        {
            for (var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                if (source is null)
                {
                    continue;
                }

                if (source is AbortSignal signal)
                {
                    signals.Add(signal);
                }
                else if (source is int milliseconds)
                {
                    signals.Add(AbortSignal.Timeout(milliseconds));
                }
                else if (source is CancellationToken token)
                {
                    signals.Add(FromToken(token));
                }
            }
        }

        if (signals.Count == 0)
        {
            return null;
        }

        if (signals.Count == 1)
        {
            return signals[0];
        }

        for (var i = 0; i < signals.Count; i++)
        {
            if (signals[i].IsCancellationRequested)
            {
                var already = new AbortSignal();
                already.Abort(signals[i].Reason);
                return already;
            }
        }

        var merged = new AbortSignal();
        for (var i = 0; i < signals.Count; i++)
        {
            merged.Link(signals[i]);
        }

        return merged;
    }

    /// <summary>
    /// Aborts <paramref name="abortController"/> after <paramref name="timeoutMs"/> with a
    /// <see cref="TimeoutErrorException"/> whose message includes <paramref name="label"/> and the duration.
    /// Returns <c>null</c> when the controller or the timeout is null. Disposing the result cancels the abort.
    /// <paramref name="schedule"/> replaces the timer so tests can fire the callback themselves.
    /// </summary>
    public static IDisposable? SetAbortTimeout(
        AbortSignal? abortController,
        string label,
        int? timeoutMs,
        Func<int, Action, IDisposable>? schedule = null)
    {
        if (abortController is null || timeoutMs is null)
        {
            return null;
        }

        var milliseconds = timeoutMs.Value;
        Action fire = delegate
        {
            abortController.Abort(new TimeoutErrorException(label + " timeout of " + milliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ms exceeded"));
        };
        if (schedule is not null)
        {
            return schedule(milliseconds, fire);
        }

        var timer = new Timer(_ => fire(), null, milliseconds, System.Threading.Timeout.Infinite);
        return new TimerStop(timer);
    }

    private static AbortSignal FromToken(CancellationToken token)
    {
        var signal = new AbortSignal();
        if (token.IsCancellationRequested)
        {
            signal.Abort(null);
            return signal;
        }

        token.Register(delegate
        {
            signal.Abort(null);
        });
        return signal;
    }

    private sealed class TimerStop : IDisposable
    {
        private Timer? _timer;

        public TimerStop(Timer timer)
        {
            _timer = timer;
        }

        public void Dispose()
        {
            var timer = _timer;
            _timer = null;
            if (timer is not null)
            {
                timer.Dispose();
            }
        }
    }
}
