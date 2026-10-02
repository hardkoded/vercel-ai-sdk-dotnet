// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class ExecuteToolParityTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/types/executable-tool.test.ts::isExecutableTool::returns true for tools with an execute function", Coverage = UpstreamCoverage.Covered)]
    public void Executable_tool_is_detected_when_execute_is_set()
    {
        var tool = new ExecutableToolDefinition
        {
            Execute = new Func<object?, ToolExecutionOptions, Task<object?>>((_, _) => Task.FromResult<object?>("sunny")),
        };
        Assert.True(ToolExecutionRunner.IsExecutableTool(tool));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/types/executable-tool.test.ts::isExecutableTool::returns false for tools without an execute function", Coverage = UpstreamCoverage.Covered)]
    public void Tool_without_execute_is_not_executable()
    {
        Assert.False(ToolExecutionRunner.IsExecutableTool(new ExecutableToolDefinition()));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/types/executable-tool.test.ts::isExecutableTool::returns false for undefined", Coverage = UpstreamCoverage.Covered)]
    public void Undefined_tool_is_not_executable()
    {
        Assert.False(ToolExecutionRunner.IsExecutableTool(null));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/types/executable-tool.test.ts::isExecutableTool::allows executable tools to be passed to executeTool after narrowing", Coverage = UpstreamCoverage.Covered)]
    public async Task Narrowed_executable_tool_returns_one_final_output()
    {
        var tool = WeatherTool();
        if (!ToolExecutionRunner.IsExecutableTool(tool))
        {
            throw new Exception("Expected weatherTool to be executable");
        }

        var results = await Collect(tool, City("Berlin"), Context("req-1"));
        Assert.Single(results);
        Assert.Equal("final", results[0].Type);
        var output = Assert.IsType<Dictionary<string, object?>>(results[0].Output);
        Assert.Equal("Berlin", output["city"]);
        Assert.Equal("req-1", output["requestId"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/types/execute-tool.test.ts::executeTool::preserves `this` for a class-based tool.execute", Coverage = UpstreamCoverage.Covered)]
    public async Task Class_execute_keeps_its_instance()
    {
        var calculator = new CalculatorTool();
        var tool = new ExecutableToolDefinition { Execute = (Func<object?, ToolExecutionOptions, Task<object?>>)calculator.Execute };
        var results = await Collect(
            tool,
            new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 },
            null);
        Assert.Single(results);
        Assert.Equal("final", results[0].Type);
        var output = Assert.IsType<Dictionary<string, object?>>(results[0].Output);
        Assert.Equal("calc", output["id"]);
        Assert.Equal(3, output["sum"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/types/execute-tool.test.ts::executeTool::yields a single final output for non-streaming tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Non_streaming_tool_yields_one_final_output()
    {
        var results = await Collect(WeatherTool(), City("Berlin"), Context("req-1"));
        Assert.Single(results);
        Assert.Equal("final", results[0].Type);
        var output = Assert.IsType<Dictionary<string, object?>>(results[0].Output);
        Assert.Equal("Berlin", output["city"]);
        Assert.Equal("req-1", output["requestId"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/types/execute-tool.test.ts::executeTool::yields streamed values as preliminary output and repeats the last one as final", Coverage = UpstreamCoverage.Covered)]
    public async Task Streaming_tool_yields_preliminary_values_and_repeats_the_last_as_final()
    {
        var tool = new ExecutableToolDefinition
        {
            Execute = new Func<object?, ToolExecutionOptions, IAsyncEnumerable<object?>>(StreamWeather),
        };
        var results = await Collect(tool, City("Berlin"), Context("req-2"));
        Assert.Equal(3, results.Count);
        Assert.Equal("preliminary", results[0].Type);
        Assert.Equal("Berlin:req-2:1", results[0].Output);
        Assert.Equal("preliminary", results[1].Type);
        Assert.Equal("Berlin:req-2:2", results[1].Output);
        Assert.Equal("final", results[2].Type);
        Assert.Equal("Berlin:req-2:2", results[2].Output);
    }

    private static async Task<List<ToolExecution>> Collect(
        ExecutableToolDefinition tool,
        object? input,
        IReadOnlyDictionary<string, object?>? context)
    {
        var results = new List<ToolExecution>();
        await foreach (var result in ToolExecutionRunner.ExecuteTool(
            tool,
            input,
            new ToolExecutionOptions
            {
                ToolCallId = "tool-call-1",
                Messages = new List<object?>(),
                Context = context,
            }))
        {
            results.Add(result);
        }

        return results;
    }

    private static ExecutableToolDefinition WeatherTool()
    {
        return new ExecutableToolDefinition
        {
            Execute = new Func<object?, ToolExecutionOptions, Task<object?>>((input, options) =>
            {
                var city = ((IReadOnlyDictionary<string, object?>)input!)["city"];
                var requestId = options.Context!["requestId"];
                return Task.FromResult<object?>(new Dictionary<string, object?>
                {
                    ["city"] = city,
                    ["requestId"] = requestId,
                });
            }),
        };
    }

    private static async IAsyncEnumerable<object?> StreamWeather(object? input, ToolExecutionOptions options)
    {
        var city = ((IReadOnlyDictionary<string, object?>)input!)["city"];
        var requestId = options.Context!["requestId"];
        yield return city + ":" + requestId + ":1";
        yield return city + ":" + requestId + ":2";
        await Task.CompletedTask;
    }

    private static Dictionary<string, object?> City(string city)
    {
        return new Dictionary<string, object?> { ["city"] = city };
    }

    private static Dictionary<string, object?> Context(string requestId)
    {
        return new Dictionary<string, object?> { ["requestId"] = requestId };
    }

    private sealed class CalculatorTool
    {
        private readonly string _prefix = "calc";

        public Task<object?> Execute(object? input, ToolExecutionOptions options)
        {
            var values = (IReadOnlyDictionary<string, object?>)input!;
            return Task.FromResult<object?>(new Dictionary<string, object?>
            {
                ["id"] = _prefix,
                ["sum"] = Convert.ToInt32(values["a"]) + Convert.ToInt32(values["b"]),
            });
        }
    }
}
