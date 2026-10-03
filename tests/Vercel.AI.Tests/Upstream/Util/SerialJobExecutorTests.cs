// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class SerialJobExecutorTests
{
    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should execute a single job successfully", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Runs_one_job()
    {
        var executor = new SerialJobExecutor();
        var done = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        await executor.RunAsync(delegate
        {
            done.SetResult("done");
            return Task.CompletedTask;
        });
        Assert.Equal("done", await done.Task);
    }

    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should execute multiple jobs in serial order", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Runs_jobs_in_submission_order()
    {
        var executor = new SerialJobExecutor();
        var order = new List<int>();
        await Task.WhenAll(
            executor.RunAsync(delegate { order.Add(1); return Task.CompletedTask; }),
            executor.RunAsync(delegate { order.Add(2); return Task.CompletedTask; }),
            executor.RunAsync(delegate { order.Add(3); return Task.CompletedTask; }));
        Assert.Equal(new[] { 1, 2, 3 }, order);
    }

    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should handle job errors correctly", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Surfaces_the_job_exception()
    {
        var executor = new SerialJobExecutor();
        var error = new InvalidOperationException("test error");
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(delegate
        {
            return executor.RunAsync(delegate { throw error; });
        });
        Assert.Same(error, thrown);
    }

    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should execute jobs one at a time", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Never_overlaps_jobs()
    {
        var executor = new SerialJobExecutor();
        var concurrent = 0;
        var max = 0;
        var first = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var job1 = executor.RunAsync(async delegate
        {
            concurrent++;
            max = Math.Max(max, concurrent);
            await first.Task;
            concurrent--;
        });
        var job2 = executor.RunAsync(async delegate
        {
            concurrent++;
            max = Math.Max(max, concurrent);
            await second.Task;
            concurrent--;
        });
        first.SetResult(true);
        second.SetResult(true);
        await Task.WhenAll(job1, job2);
        Assert.Equal(1, max);
    }

    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should handle mixed success and failure jobs", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Continues_after_a_failed_job()
    {
        var executor = new SerialJobExecutor();
        var results = new List<string>();
        var error = new InvalidOperationException("test error");
        var first = executor.RunAsync(delegate { results.Add("job1"); return Task.CompletedTask; });
        var second = executor.RunAsync(delegate { throw error; });
        var third = executor.RunAsync(delegate { results.Add("job3"); return Task.CompletedTask; });
        await first;
        Assert.Equal(new[] { "job1" }, results);
        Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(delegate { return second; }));
        await third;
        Assert.Equal(new[] { "job1", "job3" }, results);
    }

    [UpstreamTest("packages/ai/src/util/serial-job-executor.test.ts::SerialJobExecutor::should handle concurrent calls to run()", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Keeps_start_and_execution_order()
    {
        var executor = new SerialJobExecutor();
        var startOrder = new List<int>();
        var executionOrder = new List<int>();
        var gates = new[]
        {
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously),
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously),
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously),
        };
        var jobs = new[]
        {
            executor.RunAsync(async delegate { startOrder.Add(1); await gates[0].Task; executionOrder.Add(1); }),
            executor.RunAsync(async delegate { startOrder.Add(2); await gates[1].Task; executionOrder.Add(2); }),
            executor.RunAsync(async delegate { startOrder.Add(3); await gates[2].Task; executionOrder.Add(3); }),
        };
        gates[2].SetResult(true);
        gates[1].SetResult(true);
        gates[0].SetResult(true);
        await Task.WhenAll(jobs);
        Assert.Equal(new[] { 1, 2, 3 }, startOrder);
        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
    }
}
