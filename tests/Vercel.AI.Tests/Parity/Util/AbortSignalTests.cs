// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class AbortSignalTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return a signal that is initially not aborted", Coverage = UpstreamCoverage.Covered)]
    public void Merged_signal_starts_unaborted()
    {
        var merged = AbortSignals.Merge(new AbortSignal(), new AbortSignal());
        Assert.NotNull(merged);
        Assert.False(merged!.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should abort when the first signal aborts", Coverage = UpstreamCoverage.Covered)]
    public void Aborts_when_the_first_signal_aborts()
    {
        var first = new AbortSignal();
        var merged = AbortSignals.Merge(first, new AbortSignal());
        first.Abort();
        Assert.True(merged!.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should abort when the second signal aborts", Coverage = UpstreamCoverage.Covered)]
    public void Aborts_when_the_second_signal_aborts()
    {
        var second = new AbortSignal();
        var merged = AbortSignals.Merge(new AbortSignal(), second);
        second.Abort();
        Assert.True(merged!.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should preserve the abort reason from the triggering signal",
        Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_triggering_reason()
    {
        var first = new AbortSignal();
        var reason = new InvalidOperationException("custom abort reason");
        var merged = AbortSignals.Merge(first, new AbortSignal());
        first.Abort(reason);
        Assert.Same(reason, merged!.Reason);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should preserve string abort reason", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_a_string_reason()
    {
        var first = new AbortSignal();
        var merged = AbortSignals.Merge(first);
        first.Abort("string reason");
        Assert.Equal("string reason", merged!.Reason);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should handle already-aborted signals", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_already_aborted_signal()
    {
        var first = new AbortSignal();
        var reason = new InvalidOperationException("already aborted");
        first.Abort(reason);
        var merged = AbortSignals.Merge(first);
        Assert.True(merged!.IsCancellationRequested);
        Assert.Same(reason, merged.Reason);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should use the first already-aborted signal reason when multiple are aborted",
        Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_first_already_aborted_reason()
    {
        var first = new AbortSignal();
        var second = new AbortSignal();
        var reason1 = new InvalidOperationException("first reason");
        var reason2 = new InvalidOperationException("second reason");
        first.Abort(reason1);
        second.Abort(reason2);
        var merged = AbortSignals.Merge(first, second);
        Assert.True(merged!.IsCancellationRequested);
        Assert.Same(reason1, merged.Reason);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return undefined when no signals provided", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_when_nothing_is_merged()
    {
        Assert.Null(AbortSignals.Merge());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return undefined when only null/undefined signals provided",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_when_every_source_is_null()
    {
        Assert.Null(AbortSignals.Merge(null, null, null));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should create a timeout signal from numeric input", Coverage = UpstreamCoverage.Covered)]
    public async Task Creates_a_timeout_signal_from_a_number()
    {
        var merged = AbortSignals.Merge(30);
        Assert.NotNull(merged);
        Assert.False(merged!.IsCancellationRequested);
        var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (merged.Token.Register(() => done.TrySetResult(true)))
        {
            var completed = await Task.WhenAny(done.Task, Task.Delay(2000));
            Assert.Same(done.Task, completed);
        }

        Assert.True(merged.IsCancellationRequested);
        var reason = Assert.IsType<TimeoutErrorException>(merged.Reason);
        Assert.Equal("TimeoutError", reason.Name);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should preserve the first abort reason when mixing signals and timeouts",
        Coverage = UpstreamCoverage.Covered)]
    public void Prefers_a_manual_abort_over_a_timeout()
    {
        var signal = new AbortSignal();
        var reason = new InvalidOperationException("manual abort reason");
        var merged = AbortSignals.Merge(signal, 100);
        signal.Abort(reason);
        Assert.True(merged!.IsCancellationRequested);
        Assert.Same(reason, merged.Reason);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should filter out null and undefined signals", Coverage = UpstreamCoverage.Covered)]
    public void Filters_null_sources()
    {
        var signal = new AbortSignal();
        var reason = new InvalidOperationException("abort reason");
        var merged = AbortSignals.Merge(null, signal, null);
        Assert.NotNull(merged);
        Assert.False(merged!.IsCancellationRequested);
        signal.Abort(reason);
        Assert.True(merged.IsCancellationRequested);
        Assert.Same(reason, merged.Reason);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return the signal directly when only one valid signal provided",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_only_valid_signal()
    {
        var signal = new AbortSignal();
        Assert.Same(signal, AbortSignals.Merge(null, signal, null));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should use the first aborting signal reason when multiple abort simultaneously",
        Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_first_abort_reason()
    {
        var first = new AbortSignal();
        var second = new AbortSignal();
        var reason1 = new InvalidOperationException("first reason");
        var reason2 = new InvalidOperationException("second reason");
        var merged = AbortSignals.Merge(first, second);
        first.Abort(reason1);
        second.Abort(reason2);
        Assert.Same(reason1, merged!.Reason);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return the original signal when only one signal provided",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_original_signal()
    {
        var signal = new AbortSignal();
        Assert.Same(signal, AbortSignals.Merge(signal));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should work with many signals", Coverage = UpstreamCoverage.Covered)]
    public void Aborts_from_one_of_many_signals()
    {
        var signals = new AbortSignal[10];
        for (var i = 0; i < signals.Length; i++)
        {
            signals[i] = new AbortSignal();
        }

        var reason = new InvalidOperationException("signal 5 reason");
        var merged = AbortSignals.Merge(signals.Cast<object?>().ToArray());
        Assert.False(merged!.IsCancellationRequested);
        signals[5].Abort(reason);
        Assert.True(merged.IsCancellationRequested);
        Assert.Same(reason, merged.Reason);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should not abort the controller before the timeout elapses",
        Coverage = UpstreamCoverage.Covered)]
    public void Does_not_abort_before_the_timeout_fires()
    {
        var signal = new AbortSignal();
        Action? fire = null;
        AbortSignals.SetAbortTimeout(signal, "Step", 100, (milliseconds, callback) =>
        {
            Assert.Equal(100, milliseconds);
            fire = callback;
            return new Noop();
        });
        Assert.NotNull(fire);
        Assert.False(signal.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should abort the controller when the timeout elapses", Coverage = UpstreamCoverage.Covered)]
    public void Aborts_when_the_timeout_fires()
    {
        var signal = new AbortSignal();
        Action? fire = null;
        AbortSignals.SetAbortTimeout(signal, "Step", 100, (_, callback) =>
        {
            fire = callback;
            return new Noop();
        });
        fire!();
        Assert.True(signal.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should abort with a TimeoutError DOMException", Coverage = UpstreamCoverage.Covered)]
    public void Aborts_with_a_timeout_error()
    {
        var signal = new AbortSignal();
        Action? fire = null;
        AbortSignals.SetAbortTimeout(signal, "Step", 100, (_, callback) =>
        {
            fire = callback;
            return new Noop();
        });
        fire!();
        var reason = Assert.IsType<TimeoutErrorException>(signal.Reason);
        Assert.Equal("TimeoutError", reason.Name);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should include the label and duration in the abort reason message",
        Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_label_and_duration()
    {
        var signal = new AbortSignal();
        Action? fire = null;
        AbortSignals.SetAbortTimeout(signal, "Chunk", 250, (_, callback) =>
        {
            fire = callback;
            return new Noop();
        });
        fire!();
        Assert.Equal("Chunk timeout of 250ms exceeded", ((Exception)signal.Reason!).Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should return a timeout id that can be cleared to cancel the abort",
        Coverage = UpstreamCoverage.Covered)]
    public void Clearing_the_timeout_prevents_the_abort()
    {
        var signal = new AbortSignal();
        var cleared = false;
        Action? fire = null;
        var handle = AbortSignals.SetAbortTimeout(signal, "Step", 100, (_, callback) =>
        {
            fire = callback;
            return new CallbackDisposable(() => cleared = true);
        });
        handle!.Dispose();
        Assert.True(cleared);
        Assert.False(signal.IsCancellationRequested);
        Assert.NotNull(fire);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should return undefined when abortController is undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_when_the_controller_is_null()
    {
        Assert.Null(AbortSignals.SetAbortTimeout(null, "Step", 100));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should return undefined when timeoutMs is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_when_the_timeout_is_null()
    {
        var signal = new AbortSignal();
        Assert.Null(AbortSignals.SetAbortTimeout(signal, "Step", null));
        Assert.False(signal.IsCancellationRequested);
    }

    private sealed class Noop : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private sealed class CallbackDisposable : IDisposable
    {
        private readonly Action _onDispose;

        public CallbackDisposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}
