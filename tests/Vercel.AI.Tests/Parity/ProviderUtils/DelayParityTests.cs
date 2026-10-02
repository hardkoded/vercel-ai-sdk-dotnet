// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class DelayParityTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > basic delay functionality::should resolve after the specified delay", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_resolves_after_the_requested_time()
    {
        var clock = Install();
        try
        {
            var pending = Delays.DelayAsync(1000);
            Assert.False(pending.IsCompleted);
            await clock.Advance(500);
            Assert.False(pending.IsCompleted);
            await clock.Advance(500);
            await pending;
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > basic delay functionality::should resolve immediately when delayInMs is null", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_resolves_immediately_for_null()
    {
        await Delays.DelayAsync(null);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > basic delay functionality::should resolve immediately when delayInMs is undefined", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_resolves_immediately_when_omitted()
    {
        await Delays.DelayAsync();
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > basic delay functionality::should resolve immediately when delayInMs is 0", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_resolves_a_zero_delay_on_the_next_turn()
    {
        var clock = Install();
        try
        {
            var pending = Delays.DelayAsync(0);
            await clock.Advance(0);
            await pending;
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should reject immediately if signal is already aborted", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_rejects_when_already_aborted()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var error = await Assert.ThrowsAsync<DomException>(() => Delays.DelayAsync(1000, source.Token));
        Assert.Equal("Delay was aborted", error.Message);
        Assert.Equal(0, Delays.ActiveTimers);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should reject when signal is aborted during delay", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_rejects_when_aborted_during_the_wait()
    {
        var clock = Install();
        using var source = new CancellationTokenSource();
        try
        {
            var pending = Delays.DelayAsync(1000, source.Token);
            await clock.Advance(500);
            source.Cancel();
            var error = await Assert.ThrowsAsync<DomException>(() => pending);
            Assert.Equal("Delay was aborted", error.Message);
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should clean up timeout when aborted", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_clears_the_timer_when_aborted()
    {
        var clock = Install();
        using var source = new CancellationTokenSource();
        try
        {
            var pending = Delays.DelayAsync(1000, source.Token);
            await Task.Yield();
            Assert.Equal(1, Delays.ActiveTimers);
            source.Cancel();
            await Assert.ThrowsAsync<DomException>(() => pending);
            Assert.Equal(0, Delays.ActiveTimers);
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should clean up event listener when delay completes normally", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_removes_the_abort_registration()
    {
        var clock = Install();
        using var source = new CancellationTokenSource();
        try
        {
            var pending = Delays.DelayAsync(1000, source.Token);
            await Task.Yield();
            Assert.Equal(1, Delays.ActiveAbortRegistrations);
            await clock.Advance(1000);
            await pending;
            Assert.Equal(0, Delays.ActiveAbortRegistrations);
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should work without signal option", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_works_without_a_token()
    {
        var clock = Install();
        try
        {
            var pending = Delays.DelayAsync(1000);
            await clock.Advance(1000);
            await pending;
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > error handling::should create proper DOMException for abort", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_abort_is_a_dom_exception()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var error = await Assert.ThrowsAsync<DomException>(() => Delays.DelayAsync(1000, source.Token));
        Assert.Equal("Delay was aborted", error.Message);
        Assert.Equal("AbortError", error.ErrorName);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > edge cases::should handle very large delays", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_clamps_oversized_timeouts()
    {
        var clock = Install();
        try
        {
            var pending = Delays.DelayAsync(9007199254740991d);
            Assert.False(pending.IsCompleted);
            await clock.Advance(1000);
            Assert.True(pending.IsCompleted);
            await clock.Advance(1000);
            await pending;
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > edge cases::should handle negative delays (treated as 0)", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_treats_a_negative_duration_as_zero()
    {
        var clock = Install();
        try
        {
            var pending = Delays.DelayAsync(-100);
            await clock.Advance(0);
            await pending;
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delay.test.ts::delay > edge cases::should handle multiple delays simultaneously", Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_runs_several_waits_together()
    {
        var clock = Install();
        try
        {
            var first = Delays.DelayAsync(100);
            var second = Delays.DelayAsync(200);
            var third = Delays.DelayAsync(300);
            await clock.Advance(100);
            Assert.True(first.IsCompletedSuccessfully);
            Assert.False(second.IsCompleted);
            Assert.False(third.IsCompleted);
            await clock.Advance(100);
            Assert.True(second.IsCompletedSuccessfully);
            Assert.False(third.IsCompleted);
            await clock.Advance(100);
            Assert.True(third.IsCompletedSuccessfully);
            await Task.WhenAll(first, second, third);
        }
        finally
        {
            Delays.WaitOverride = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should resolve when accessed after resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_resolves_after_the_value_is_set()
    {
        var promise = new DelayedPromise<string>();
        promise.Resolve("success");
        Assert.Equal("success", await promise.Promise);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should reject when accessed after rejection", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_rejects_after_the_error_is_set()
    {
        var promise = new DelayedPromise<string>();
        promise.Reject(new Exception("failure"));
        var error = await Assert.ThrowsAsync<Exception>(() => promise.Promise);
        Assert.Equal("failure", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should resolve when accessed before resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_resolves_a_waiter()
    {
        var promise = new DelayedPromise<string>();
        var pending = promise.Promise;
        promise.Resolve("success");
        Assert.Equal("success", await pending);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should reject when accessed before rejection", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_rejects_a_waiter()
    {
        var promise = new DelayedPromise<string>();
        var pending = promise.Promise;
        promise.Reject(new Exception("failure"));
        var error = await Assert.ThrowsAsync<Exception>(() => pending);
        Assert.Equal("failure", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should maintain the resolved state after multiple accesses", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_stays_resolved()
    {
        var promise = new DelayedPromise<string>();
        promise.Resolve("success");
        Assert.Equal("success", await promise.Promise);
        Assert.Equal("success", await promise.Promise);
        Assert.True(promise.IsResolved());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should maintain the rejected state after multiple accesses", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_stays_rejected()
    {
        var promise = new DelayedPromise<string>();
        promise.Reject(new Exception("failure"));
        await Assert.ThrowsAsync<Exception>(() => promise.Promise);
        await Assert.ThrowsAsync<Exception>(() => promise.Promise);
        Assert.True(promise.IsRejected());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should block until resolved when accessed before resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_blocks_until_resolve()
    {
        var promise = new DelayedPromise<string>();
        var pending = promise.Promise;
        Assert.False(pending.IsCompleted);
        promise.Resolve("later");
        Assert.Equal("later", await pending);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should block until rejected when accessed before rejection", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_blocks_until_reject()
    {
        var promise = new DelayedPromise<string>();
        var pending = promise.Promise;
        Assert.False(pending.IsCompleted);
        promise.Reject(new Exception("later"));
        var error = await Assert.ThrowsAsync<Exception>(() => pending);
        Assert.Equal("later", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/delayed-promise.test.ts::DelayedPromise::should resolve all pending promises when resolved after access", Coverage = UpstreamCoverage.Covered)]
    public async Task Delayed_promise_resolves_every_waiter()
    {
        var promise = new DelayedPromise<string>();
        var first = promise.Promise;
        var second = promise.Promise;
        promise.Resolve("shared");
        Assert.Equal("shared", await first);
        Assert.Equal("shared", await second);
    }

    private static FakeDelayClock Install()
    {
        var clock = new FakeDelayClock();
        Delays.WaitOverride = clock.Wait;
        return clock;
    }

    private sealed class FakeDelayClock
    {
        private readonly List<Entry> _entries = new List<Entry>();
        private long _now;

        public Task Wait(long milliseconds, CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (cancellationToken.IsCancellationRequested)
            {
                source.TrySetCanceled();
                return source.Task;
            }

            var entry = new Entry { Due = _now + milliseconds, Source = source };
            if (cancellationToken.CanBeCanceled)
            {
                entry.Registration = cancellationToken.Register(() => source.TrySetCanceled());
            }

            _entries.Add(entry);
            CompleteDue();
            return source.Task;
        }

        public async Task Advance(long milliseconds)
        {
            _now += milliseconds;
            CompleteDue();
            await Task.Yield();
            await Task.Delay(1);
        }

        private void CompleteDue()
        {
            foreach (var entry in _entries.ToArray())
            {
                if (entry.Due <= _now)
                {
                    entry.Registration.Dispose();
                    entry.Source.TrySetResult(true);
                    _entries.Remove(entry);
                }
            }
        }

        private sealed class Entry
        {
            public long Due { get; set; }

            public TaskCompletionSource<bool> Source { get; set; } = new TaskCompletionSource<bool>();

            public CancellationTokenRegistration Registration { get; set; }
        }
    }
}
