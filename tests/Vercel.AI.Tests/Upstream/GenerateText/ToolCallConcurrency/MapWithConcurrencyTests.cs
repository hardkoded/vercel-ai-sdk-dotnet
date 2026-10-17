// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using Vercel.AI.GenerateText;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency.ToolCallConcurrencySupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency;

/// <summary>Port of <c>tool-call-concurrency.test.ts</c> &gt; <c>mapWithConcurrency</c>.</summary>
public sealed class MapWithConcurrencyTests
{
    private static DelayedPromise<bool>[] Releases() => new[] { new DelayedPromise<bool>(), new DelayedPromise<bool>(), new DelayedPromise<bool>() };

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-call-concurrency.test.ts::mapWithConcurrency::should execute all items concurrently by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_all_items_concurrently_by_default()
    {
        var releases = Releases();
        var started = new ConcurrentQueue<int>();

        var resultTask = ToolCallConcurrencyScheduler.MapWithConcurrency<int, int>(
            new[] { 0, 1, 2 },
            null,
            async (item, _) =>
            {
                started.Enqueue(item);
                await releases[item].Promise;
                return item;
            });

        await WaitForStarted(started, 0, 1, 2);
        foreach (var release in releases)
        {
            release.Resolve(true);
        }

        Assert.Equal(new[] { 0, 1, 2 }, await resultTask);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-call-concurrency.test.ts::mapWithConcurrency::should execute items sequentially in input order with concurrency 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_items_sequentially_in_input_order_with_concurrency_1()
    {
        var releases = Releases();
        var events = new ConcurrentQueue<string>();

        var resultTask = ToolCallConcurrencyScheduler.MapWithConcurrency<int, int>(
            new[] { 0, 1, 2 },
            1,
            async (item, _) =>
            {
                events.Enqueue(item + ":start");
                await releases[item].Promise;
                events.Enqueue(item + ":end");
                return item;
            });

        await WaitForEvents(events, "0:start");
        releases[0].Resolve(true);
        await WaitForEvents(events, "0:start", "0:end", "1:start");
        releases[1].Resolve(true);
        await WaitForEvents(events, "0:start", "0:end", "1:start", "1:end", "2:start");
        releases[2].Resolve(true);

        Assert.Equal(new[] { 0, 1, 2 }, await resultTask);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-call-concurrency.test.ts::mapWithConcurrency::should limit concurrent executions and preserve result order", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_limit_concurrent_executions_and_preserve_result_order()
    {
        var releases = Releases();
        var started = new ConcurrentQueue<int>();

        var resultTask = ToolCallConcurrencyScheduler.MapWithConcurrency<int, int>(
            new[] { 0, 1, 2 },
            2,
            async (item, _) =>
            {
                started.Enqueue(item);
                await releases[item].Promise;
                return item;
            });

        await WaitForStarted(started, 0, 1);
        releases[1].Resolve(true);
        await WaitForStarted(started, 0, 1, 2);
        releases[2].Resolve(true);
        releases[0].Resolve(true);

        Assert.Equal(new[] { 0, 1, 2 }, await resultTask);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-call-concurrency.test.ts::mapWithConcurrency::should not start queued items after abort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_start_queued_items_after_abort()
    {
        using var abort = new CancellationTokenSource();
        var release = new DelayedPromise<bool>();
        var started = new ConcurrentQueue<int>();

        var resultTask = ToolCallConcurrencyScheduler.MapWithConcurrency<int, int>(
            new[] { 0, 1, 2 },
            1,
            async (item, _) =>
            {
                started.Enqueue(item);
                await release.Promise;
                return item;
            },
            abort.Token);

        await WaitForStarted(started, 0);
        abort.Cancel();
        release.Resolve(true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => resultTask);
        Assert.Equal(new[] { 0 }, started.ToArray());
    }
}
