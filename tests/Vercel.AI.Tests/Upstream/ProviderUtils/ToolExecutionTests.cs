// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class ToolExecutionTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/types/executable-tool.test.ts::isExecutableTool::returns true for tools with an execute function",
        Coverage = UpstreamCoverage.Covered)]
    public void Executable_tool_detects_a_delegate()
    {
        Func<Task<string>> execute = () => Task.FromResult("sunny");
        Assert.True(ToolExecution.IsExecutable(execute));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/types/executable-tool.test.ts::isExecutableTool::returns false for tools without an execute function",
        Coverage = UpstreamCoverage.Covered)]
    public void Executable_tool_rejects_a_missing_delegate()
    {
        Assert.False(ToolExecution.IsExecutable(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/types/executable-tool.test.ts::isExecutableTool::returns false for undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Executable_tool_rejects_null()
    {
        Delegate? execute = null;
        Assert.False(ToolExecution.IsExecutable(execute));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/types/executable-tool.test.ts::isExecutableTool::allows executable tools to be passed to executeTool after narrowing",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Executable_tool_can_be_run_after_narrowing()
    {
        Func<string, string, Task<Weather>> execute = (city, requestId) => Task.FromResult(new Weather(city, requestId));
        Assert.True(ToolExecution.IsExecutable(execute));
        var steps = new List<ToolExecution.ToolStep<Weather>>();
        await foreach (var step in ToolExecution.ExecuteAsync(() => execute("Berlin", "req-1")))
        {
            steps.Add(step);
        }

        var result = Assert.Single(steps);
        Assert.Equal("final", result.Type);
        Assert.Equal("Berlin", result.Output.City);
        Assert.Equal("req-1", result.Output.RequestId);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/types/execute-tool.test.ts::executeTool::yields a single final output for non-streaming tools",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Execute_yields_one_final_value()
    {
        var steps = new List<ToolExecution.ToolStep<Weather>>();
        await foreach (var step in ToolExecution.ExecuteAsync(() => Task.FromResult(new Weather("Berlin", "req-1"))))
        {
            steps.Add(step);
        }

        var result = Assert.Single(steps);
        Assert.Equal("final", result.Type);
        Assert.Equal(new Weather("Berlin", "req-1"), result.Output);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/types/execute-tool.test.ts::executeTool::yields streamed values as preliminary output and repeats the last one as final",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Execute_stream_marks_values_preliminary_and_repeats_the_last()
    {
        var steps = new List<ToolExecution.ToolStep<string>>();
        await foreach (var step in ToolExecution.ExecuteStreamAsync(Stream()))
        {
            steps.Add(step);
        }

        Assert.Equal(3, steps.Count);
        Assert.Equal("preliminary", steps[0].Type);
        Assert.Equal("Berlin:req-2:1", steps[0].Output);
        Assert.Equal("preliminary", steps[1].Type);
        Assert.Equal("Berlin:req-2:2", steps[1].Output);
        Assert.Equal("final", steps[2].Type);
        Assert.Equal("Berlin:req-2:2", steps[2].Output);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/types/execute-tool.test.ts::executeTool::preserves `this` for a class-based tool.execute",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Execute_keeps_the_instance_of_a_method_delegate()
    {
        var calculator = new Calculator("calc");
        Func<int, int, Task<string>> execute = calculator.Execute;
        var steps = new List<ToolExecution.ToolStep<string>>();
        await foreach (var step in ToolExecution.ExecuteAsync(() => execute(1, 2)))
        {
            steps.Add(step);
        }

        var result = Assert.Single(steps);
        Assert.Equal("final", result.Type);
        Assert.Equal("calc:3", result.Output);
    }

    private static async IAsyncEnumerable<string> Stream()
    {
        yield return "Berlin:req-2:1";
        yield return "Berlin:req-2:2";
        await Task.CompletedTask;
    }

    private readonly record struct Weather(string City, string RequestId);

    private sealed class Calculator
    {
        private readonly string _prefix;

        public Calculator(string prefix)
        {
            _prefix = prefix;
        }

        public Task<string> Execute(int a, int b)
        {
            return Task.FromResult(_prefix + ":" + (a + b));
        }
    }
}
