// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using Vercel.AI.GenerateText;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency.ToolCallConcurrencySupport;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>execute-tools-from-stream.test.ts</c> &gt; <c>executeToolsFromStream</c>.</summary>
public sealed class ExecuteToolsFromStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/execute-tools-from-stream.test.ts::executeToolsFromStream::should execute tool calls sequentially in stream order with concurrency 1",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port has no executeToolsFromStream transformer. The test drives the scheduler that Generation.FinishStepAsync uses for generate and stream.")]
    public async Task Should_execute_tool_calls_sequentially_in_stream_order_with_concurrency_1()
    {
        var firstStarted = new DelayedPromise<bool>();
        var secondStarted = new DelayedPromise<bool>();
        var releaseFirst = new DelayedPromise<bool>();
        var events = new ConcurrentQueue<string>();

        var resultTask = ToolCallConcurrencyScheduler.MapWithConcurrency<string, bool>(
            new[] { "first", "second" },
            1,
            async (name, _) =>
            {
                events.Enqueue(name + ":start");
                if (name == "first")
                {
                    firstStarted.Resolve(true);
                    await releaseFirst.Promise;
                }
                else
                {
                    secondStarted.Resolve(true);
                }

                events.Enqueue(name + ":end");
                return true;
            });

        await firstStarted.Promise.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new[] { "first:start" }, events.ToArray());
        releaseFirst.Resolve(true);
        await secondStarted.Promise.WaitAsync(TimeSpan.FromSeconds(5));
        await WaitForEvents(events, "first:start", "first:end", "second:start", "second:end");
        await resultTask;
    }
}
