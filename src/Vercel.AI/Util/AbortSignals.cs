// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>DOM <c>TimeoutError</c> analog used as an abort reason.</summary>
public sealed class TimeoutError : Exception
{
    /// <summary>Creates a timeout error.</summary>
    public TimeoutError(string message)
        : base(message)
    {
    }

    /// <summary>Always <c>TimeoutError</c>.</summary>
    public string ErrorName
    {
        get { return "TimeoutError"; }
    }
}

/// <summary>Abort signal that records the first reason.</summary>
public sealed class AbortSignal
{
    private readonly List<Action> _handlers = new List<Action>();

    /// <summary>Whether <see cref="Abort"/> has been called.</summary>
    public bool Aborted { get; private set; }

    /// <summary>Reason passed to the abort that won.</summary>
    public object? Reason { get; private set; }

    /// <summary>Aborts unless this signal is already aborted.</summary>
    public void Abort(object? reason)
    {
        if (Aborted)
        {
            return;
        }

        Aborted = true;
        Reason = reason;
        Action[] handlers;
        lock (_handlers)
        {
            handlers = _handlers.ToArray();
        }

        for (var i = 0; i < handlers.Length; i++)
        {
            handlers[i]();
        }
    }

    /// <summary>Runs <paramref name="handler"/> when aborted. Runs immediately if already aborted.</summary>
    public void AddAbortHandler(Action handler)
    {
        if (handler == null)
        {
            return;
        }

        var runNow = false;
        lock (_handlers)
        {
            if (Aborted)
            {
                runNow = true;
            }
            else
            {
                _handlers.Add(handler);
            }
        }

        if (runNow)
        {
            handler();
        }
    }

    /// <summary>Removes a handler added by <see cref="AddAbortHandler"/>.</summary>
    public void RemoveAbortHandler(Action handler)
    {
        lock (_handlers)
        {
            _handlers.Remove(handler);
        }
    }

    /// <summary>Signal that aborts with <see cref="TimeoutError"/> after <paramref name="timeoutMs"/>.</summary>
    public static AbortSignal Timeout(int timeoutMs, Action<Action, int>? schedule = null)
    {
        var signal = new AbortSignal();
        var reason = new TimeoutError("The operation was aborted due to timeout");
        if (schedule != null)
        {
            schedule(delegate { signal.Abort(reason); }, timeoutMs);
            return signal;
        }

        var timer = new Timer(delegate { signal.Abort(reason); }, null, timeoutMs, System.Threading.Timeout.Infinite);
        signal.AddAbortHandler(delegate { timer.Dispose(); });
        return signal;
    }
}

/// <summary>Controller for an <see cref="AbortSignal"/>.</summary>
public sealed class AbortController
{
    /// <summary>Creates a controller whose signal is not aborted.</summary>
    public AbortController()
    {
        Signal = new AbortSignal();
    }

    /// <summary>Signal aborted by <see cref="Abort"/>.</summary>
    public AbortSignal Signal { get; }

    /// <summary>Aborts <see cref="Signal"/> with <paramref name="reason"/>.</summary>
    public void Abort(object? reason = null)
    {
        Signal.Abort(reason);
    }
}

/// <summary>Combines abort signals and timeout durations.</summary>
public static class MergeAbortSignals
{
    /// <summary>
    /// Returns a signal that aborts when any source aborts.
    /// Numbers are timeout durations in milliseconds. Null entries are ignored.
    /// A single signal is returned as-is. No sources returns <c>null</c>.
    /// </summary>
    public static AbortSignal? Merge(params object?[]? signals)
    {
        var valid = new List<AbortSignal>();
        if (signals != null)
        {
            for (var i = 0; i < signals.Length; i++)
            {
                var signal = signals[i];
                if (signal == null)
                {
                    continue;
                }

                var abortSignal = signal as AbortSignal;
                if (abortSignal != null)
                {
                    valid.Add(abortSignal);
                    continue;
                }

                if (signal is int || signal is long || signal is short)
                {
                    valid.Add(AbortSignal.Timeout(Convert.ToInt32(signal, System.Globalization.CultureInfo.InvariantCulture)));
                }
            }
        }

        if (valid.Count == 0)
        {
            return null;
        }

        if (valid.Count == 1)
        {
            return valid[0];
        }

        var controller = new AbortController();
        for (var i = 0; i < valid.Count; i++)
        {
            var signal = valid[i];
            if (signal.Aborted)
            {
                if (!controller.Signal.Aborted)
                {
                    controller.Abort(signal.Reason);
                }

                return controller.Signal;
            }

            signal.AddAbortHandler(delegate
            {
                if (!controller.Signal.Aborted)
                {
                    controller.Abort(signal.Reason);
                }
            });
        }

        return controller.Signal;
    }
}

/// <summary>Schedules an abort when a timeout elapses.</summary>
public static class SetAbortTimeout
{
    /// <summary>
    /// Aborts <paramref name="abortController"/> after <paramref name="timeoutMs"/> with a
    /// <see cref="TimeoutError"/> whose message includes <paramref name="label"/>.
    /// Returns <c>null</c> when the controller or timeout is missing. Disposing the result cancels the abort.
    /// </summary>
    public static IDisposable? Schedule(AbortController? abortController, string? label, int? timeoutMs, Action<Action, int>? schedule = null)
    {
        if (abortController == null || timeoutMs == null)
        {
            return null;
        }

        var controller = abortController;
        var timeoutLabel = label ?? string.Empty;
        var cancelled = false;
        var milliseconds = timeoutMs.Value;
        Action fire = delegate
        {
            if (!cancelled)
            {
                controller.Abort(new TimeoutError(timeoutLabel + " timeout of " + milliseconds + "ms exceeded"));
            }
        };

        if (schedule != null)
        {
            schedule(fire, milliseconds);
        }
        else
        {
            var timer = new Timer(delegate { fire(); }, null, milliseconds, System.Threading.Timeout.Infinite);
            return new CancelTimeout(delegate
            {
                cancelled = true;
                timer.Dispose();
            });
        }

        return new CancelTimeout(delegate { cancelled = true; });
    }

    private sealed class CancelTimeout : IDisposable
    {
        private Action? _cancel;

        public CancelTimeout(Action cancel)
        {
            _cancel = cancel;
        }

        public void Dispose()
        {
            var cancel = _cancel;
            _cancel = null;
            if (cancel != null)
            {
                cancel();
            }
        }
    }
}
