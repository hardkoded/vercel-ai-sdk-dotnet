// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class AbortTests
{
    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return a signal that is initially not aborted", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Merged_signal_starts_open()
    {
        var merged = MergeAbortSignals.Merge(new AbortController().Signal, new AbortController().Signal)!;
        Assert.NotNull(merged);
        Assert.False(merged.Aborted);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should abort when the first signal aborts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Aborts_when_the_first_signal_aborts()
    {
        var first = new AbortController();
        var merged = MergeAbortSignals.Merge(first.Signal, new AbortController().Signal)!;
        first.Abort();
        Assert.True(merged.Aborted);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should abort when the second signal aborts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Aborts_when_the_second_signal_aborts()
    {
        var second = new AbortController();
        var merged = MergeAbortSignals.Merge(new AbortController().Signal, second.Signal)!;
        second.Abort();
        Assert.True(merged.Aborted);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should preserve the abort reason from the triggering signal", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_the_triggering_reason()
    {
        var first = new AbortController();
        var reason = new InvalidOperationException("custom abort reason");
        var merged = MergeAbortSignals.Merge(first.Signal, new AbortController().Signal)!;
        first.Abort(reason);
        Assert.Same(reason, merged.Reason);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should preserve string abort reason", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_a_string_reason()
    {
        var controller = new AbortController();
        var merged = MergeAbortSignals.Merge(controller.Signal)!;
        controller.Abort("string reason");
        Assert.Equal("string reason", merged.Reason);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should handle already-aborted signals", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_an_already_aborted_signal()
    {
        var controller = new AbortController();
        var reason = new InvalidOperationException("already aborted");
        controller.Abort(reason);
        var merged = MergeAbortSignals.Merge(controller.Signal)!;
        Assert.True(merged.Aborted);
        Assert.Same(reason, merged.Reason);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should use the first already-aborted signal reason when multiple are aborted", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Uses_the_first_already_aborted_reason()
    {
        var first = new AbortController();
        var second = new AbortController();
        var reason1 = new InvalidOperationException("first reason");
        var reason2 = new InvalidOperationException("second reason");
        first.Abort(reason1);
        second.Abort(reason2);
        var merged = MergeAbortSignals.Merge(first.Signal, second.Signal)!;
        Assert.True(merged.Aborted);
        Assert.Same(reason1, merged.Reason);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return undefined when no signals provided", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_null_without_sources()
    {
        Assert.Null(MergeAbortSignals.Merge());
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return undefined when only null/undefined signals provided", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_null_for_null_sources()
    {
        Assert.Null(MergeAbortSignals.Merge(null, null, null));
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should create a timeout signal from numeric input", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Creates_a_timeout_signal()
    {
        var merged = MergeAbortSignals.Merge(10)!;
        Assert.NotNull(merged);
        Assert.False(merged.Aborted);
        var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        merged.AddAbortHandler(delegate { done.TrySetResult(true); });
        await done.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(merged.Aborted);
        var reason = Assert.IsType<TimeoutError>(merged.Reason!);
        Assert.Equal("TimeoutError", reason.ErrorName);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should preserve the first abort reason when mixing signals and timeouts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Prefers_a_manual_abort_over_a_timeout()
    {
        var controller = new AbortController();
        var reason = new InvalidOperationException("manual abort reason");
        var merged = MergeAbortSignals.Merge(controller.Signal, 100)!;
        controller.Abort(reason);
        Assert.True(merged.Aborted);
        Assert.Same(reason, merged.Reason);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should filter out null and undefined signals", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Skips_null_sources()
    {
        var controller = new AbortController();
        var reason = new InvalidOperationException("abort reason");
        var merged = MergeAbortSignals.Merge(null, controller.Signal, null)!;
        Assert.NotNull(merged);
        Assert.False(merged.Aborted);
        controller.Abort(reason);
        Assert.True(merged.Aborted);
        Assert.Same(reason, merged.Reason);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return the signal directly when only one valid signal provided", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_the_only_valid_signal()
    {
        var controller = new AbortController();
        Assert.Same(controller.Signal, MergeAbortSignals.Merge(null, controller.Signal, null));
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should use the first aborting signal reason when multiple abort simultaneously", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Uses_the_first_abort_among_live_signals()
    {
        var first = new AbortController();
        var second = new AbortController();
        var reason1 = new InvalidOperationException("first reason");
        var reason2 = new InvalidOperationException("second reason");
        var merged = MergeAbortSignals.Merge(first.Signal, second.Signal)!;
        first.Abort(reason1);
        second.Abort(reason2);
        Assert.Same(reason1, merged.Reason);
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should return the original signal when only one signal provided", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_the_original_signal()
    {
        var controller = new AbortController();
        Assert.Same(controller.Signal, MergeAbortSignals.Merge(controller.Signal));
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should accept a signal when the global AbortSignal is not a constructor", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_the_original_signal_without_constructing_another()
    {
        var controller = new AbortController();
        Assert.Same(controller.Signal, MergeAbortSignals.Merge(controller.Signal));
    }

    [UpstreamTest("packages/ai/src/util/merge-abort-signals.test.ts::mergeAbortSignals::should work with many signals", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Aborts_from_one_of_many_signals()
    {
        var controllers = new AbortController[10];
        var signals = new object?[10];
        for (var i = 0; i < controllers.Length; i++)
        {
            controllers[i] = new AbortController();
            signals[i] = controllers[i].Signal;
        }

        var merged = MergeAbortSignals.Merge(signals)!;
        var reason = new InvalidOperationException("signal 5 reason");
        Assert.False(merged.Aborted);
        controllers[5].Abort(reason);
        Assert.True(merged.Aborted);
        Assert.Same(reason, merged.Reason);
    }

    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should not abort the controller before the timeout elapses", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Does_not_abort_before_the_scheduled_callback()
    {
        var controller = new AbortController();
        var fire = Capture(delegate(Action callback, int milliseconds)
        {
            Assert.Equal(100, milliseconds);
            return callback;
        }, controller, "Step", 100);
        Assert.False(controller.Signal.Aborted);
    }

    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should abort the controller when the timeout elapses", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Aborts_when_the_scheduled_callback_runs()
    {
        var controller = new AbortController();
        Capture(delegate(Action callback, int milliseconds) { return callback; }, controller, "Step", 100)();
        Assert.True(controller.Signal.Aborted);
    }

    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should abort with a TimeoutError DOMException", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Aborts_with_a_timeout_error()
    {
        var controller = new AbortController();
        Capture(delegate(Action callback, int milliseconds) { return callback; }, controller, "Step", 100)();
        var reason = Assert.IsType<TimeoutError>(controller.Signal.Reason);
        Assert.Equal("TimeoutError", reason.ErrorName);
    }

    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should include the label and duration in the abort reason message", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Includes_the_label_and_duration()
    {
        var controller = new AbortController();
        Capture(delegate(Action callback, int milliseconds) { return callback; }, controller, "Chunk", 250)();
        Assert.Equal("Chunk timeout of 250ms exceeded", Assert.IsType<TimeoutError>(controller.Signal.Reason!).Message);
    }

    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should return a timeout id that can be cleared to cancel the abort", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Disposing_the_handle_cancels_the_abort()
    {
        var controller = new AbortController();
        Action? fire = null;
        var handle = SetAbortTimeout.Schedule(controller, "Step", 100, delegate(Action callback, int milliseconds) { fire = callback; });
        Assert.NotNull(handle);
        Assert.NotNull(fire);
        handle!.Dispose();
        fire!();
        Assert.False(controller.Signal.Aborted);
    }

    private static Action Capture(Func<Action, int, Action> schedule, AbortController controller, string label, int timeoutMs)
    {
        Action? captured = null;
        SetAbortTimeout.Schedule(controller, label, timeoutMs, delegate(Action callback, int milliseconds)
        {
            captured = schedule(callback, milliseconds);
        });
        Assert.NotNull(captured);
        return captured!;
    }

    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should return undefined when abortController is undefined", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_null_without_a_controller()
    {
        Assert.Null(SetAbortTimeout.Schedule(null, "Step", 100, delegate { throw new InvalidOperationException("should not schedule"); }));
    }

    [UpstreamTest("packages/ai/src/util/set-abort-timeout.test.ts::setAbortTimeout::should return undefined when timeoutMs is undefined", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_null_without_a_timeout()
    {
        Assert.Null(SetAbortTimeout.Schedule(new AbortController(), "Step", null, delegate { throw new InvalidOperationException("should not schedule"); }));
    }
}
