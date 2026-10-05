// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class DelayedPromiseTests
{
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
}
