// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream.ExecuteToolsFromStreamSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval.ResolveToolApprovalSupport;
using Transformer = Vercel.AI.GenerateText.ExecuteToolsFromStream;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream;

/// <summary>Port of <c>execute-tools-from-stream.test.ts</c> &gt; <c>executeToolsFromStream</c> &gt; <c>tool execution error handling</c>.</summary>
public sealed class ToolExecutionErrorHandlingTests
{
    private const string Id = Prefix + "executeToolsFromStream > tool execution error handling::";

    private static void AssertFailed(IReadOnlyList<TextStreamPart> result, string message)
    {
        Assert.Equal("tool-call,model-call-end,tool-execution-end,tool-error", Types(result));
        var end = Assert.IsType<Transformer.ToolExecutionEndPart>(result[2]);
        Assert.Equal("call-1", end.ToolCallId);
        Assert.Equal(0, end.ToolExecutionMs);
        var error = Assert.IsType<ToolErrorPart>(result[3]).Result;
        Assert.False(error.Dynamic);
        Assert.Equal(message, error.Error!.Message);
        Assert.Equal("{\"value\":\"test\"}", error.InputJson);
        Assert.Equal("call-1", error.ToolCallId);
        Assert.Equal("failingTool", error.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw TypeValidationError before approval callbacks run",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_TypeValidationError_before_approval_callbacks_run()
    {
        var needsApprovalCalls = 0;
        var tool = Tool.Function("guardedTool", null, ValueSchema, (input, _) =>
            Task.FromResult(
                JsonSerializer.Serialize(input.GetProperty("value").GetString() + "-result")
            )
        );
        tool.ContextSchema = ApiKeySchema;
        tool.NeedsApprovalFunction = (_, _) =>
        {
            needsApprovalCalls++;
            return Task.FromResult(true);
        };

        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            Run(
                Options(new[] { tool }, options =>
                    options.ToolsContext = new Dictionary<string, JsonElement>
                    {
                        ["guardedTool"] = JsonDocument
                            .Parse("{\"apiKey\":123}")
                            .RootElement.Clone(),
                    }
                ),
                Call("call-1", "guardedTool"),
                FinishChunk()
            )
        );

        Assert.Equal(0, needsApprovalCalls);
        Assert.Equal("{\"apiKey\":123}", ((JsonElement)error.Value!).GetRawText());
        Assert.Equal("tool context", error.Context.Field);
        Assert.Equal("guardedTool", error.Context.EntityName);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle error thrown in async tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_error_thrown_in_async_tool_execution()
    {
        var tools = new[] { ValueTool("failingTool", async (_, _) =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Tool execution failed!");
            }) };

        var result = await Run(Options(tools), Call("call-1", "failingTool"), FinishChunk());

        AssertFailed(result, "Tool execution failed!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle error thrown in sync tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_error_thrown_in_sync_tool_execution()
    {
        var tools = new[]
        {
            ValueTool("failingTool", (_, _) =>
                throw new InvalidOperationException("Sync tool failed!")
            ),
        };

        var result = await Run(Options(tools), Call("call-1", "failingTool"), FinishChunk());

        AssertFailed(result, "Sync tool failed!");
    }
}
