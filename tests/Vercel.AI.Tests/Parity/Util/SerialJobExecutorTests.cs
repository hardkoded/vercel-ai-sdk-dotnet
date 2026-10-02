// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class SerialJobExecutorTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should execute a single job successfully", Coverage = UpstreamCoverage.Covered)]
    public async Task Executes_a_single_job()
    {
        var executor = new SerialJobExecutor();
        var done = false;
        await executor.Run(() =>
        {
            done = true;
            return Task.CompletedTask;
        });
        Assert.True(done);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should execute multiple jobs in serial order", Coverage = UpstreamCoverage.Covered)]
    public async Task Executes_jobs_in_submission_order()
    {
        var executor = new SerialJobExecutor();
        var order = new List<int>();
        await Task.WhenAll(
            executor.Run(() =>
            {
                order.Add(1);
                return Task.CompletedTask;
            }),
            executor.Run(() =>
            {
                order.Add(2);
                return Task.CompletedTask;
            }),
            executor.Run(() =>
            {
                order.Add(3);
                return Task.CompletedTask;
            }));
        Assert.Equal(new[] { 1, 2, 3 }, order);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should handle job errors correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Propagates_the_job_exception()
    {
        var executor = new SerialJobExecutor();
        var error = new InvalidOperationException("test error");
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => executor.Run(() => Task.FromException(error)));
        Assert.Same(error, thrown);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should execute jobs one at a time", Coverage = UpstreamCoverage.Covered)]
    public async Task Runs_one_job_at_a_time()
    {
        var executor = new SerialJobExecutor();
        var concurrent = 0;
        var maxConcurrent = 0;
        var first = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var promise1 = executor.Run(async () =>
        {
            concurrent++;
            maxConcurrent = Math.Max(maxConcurrent, concurrent);
            await first.Task.ConfigureAwait(false);
            concurrent--;
        });
        var promise2 = executor.Run(async () =>
        {
            concurrent++;
            maxConcurrent = Math.Max(maxConcurrent, concurrent);
            await second.Task.ConfigureAwait(false);
            concurrent--;
        });
        first.TrySetResult(true);
        second.TrySetResult(true);
        await Task.WhenAll(promise1, promise2);
        Assert.Equal(1, maxConcurrent);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should handle mixed success and failure jobs", Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_after_a_failed_job()
    {
        var executor = new SerialJobExecutor();
        var results = new List<string>();
        var error = new InvalidOperationException("test error");
        var promise1 = executor.Run(() =>
        {
            results.Add("job1");
            return Task.CompletedTask;
        });
        var promise2 = executor.Run(() => Task.FromException(error));
        var promise3 = executor.Run(() =>
        {
            results.Add("job3");
            return Task.CompletedTask;
        });
        await promise1;
        Assert.Equal(new[] { "job1" }, results);
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => promise2);
        Assert.Same(error, thrown);
        await promise3;
        Assert.Equal(new[] { "job1", "job3" }, results);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should handle concurrent calls to run()", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_submission_order_when_gates_complete_in_reverse()
    {
        var executor = new SerialJobExecutor();
        var startOrder = new List<int>();
        var executionOrder = new List<int>();
        var job1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var job2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var job3 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var promises = new[]
        {
            executor.Run(async () =>
            {
                startOrder.Add(1);
                await job1.Task.ConfigureAwait(false);
                executionOrder.Add(1);
            }),
            executor.Run(async () =>
            {
                startOrder.Add(2);
                await job2.Task.ConfigureAwait(false);
                executionOrder.Add(2);
            }),
            executor.Run(async () =>
            {
                startOrder.Add(3);
                await job3.Task.ConfigureAwait(false);
                executionOrder.Add(3);
            }),
        };
        job3.TrySetResult(true);
        job2.TrySetResult(true);
        job1.TrySetResult(true);
        await Task.WhenAll(promises);
        Assert.Equal(new[] { 1, 2, 3 }, startOrder);
        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
    }
}
