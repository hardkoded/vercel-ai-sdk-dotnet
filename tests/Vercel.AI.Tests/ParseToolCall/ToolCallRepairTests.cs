// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ParseToolCall.ParseToolCallFixture;

namespace Vercel.AI.Tests.ParseToolCall;

/// <summary>Port of <c>parse-tool-call.test.ts</c> &gt; <c>parseToolCall</c> &gt; <c>tool call repair</c>.</summary>
public sealed class ToolCallRepairTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool call repair::should invoke repairTool when provided and use its result",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldInvokeRepairToolWhenProvidedAndUseItsResult()
    {
        var calls = new List<ToolCallRepairContext>();
        var messages = new ModelMessage[] { new UserModelMessage("test message") };

        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "invalid json"), // This will trigger repair
            new[] { TestTool(ParamsSchema) },
            repairToolCall: context =>
            {
                calls.Add(context);
                return Task.FromResult<GeneratedToolCall?>(new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\", \"param2\": 42}"));
            },
            messages: messages,
            instructions: "test instructions");

        // Verify repair function was called
        var call = Assert.Single(calls);
        Assert.Equal("testTool", call.ToolCall.ToolName);
        Assert.Equal("invalid json", call.ToolCall.ArgumentsJson);
        Assert.NotNull(call.Tools);
        Assert.NotNull(call.InputSchema);
        Assert.Same(messages, call.Messages);
        Assert.Equal("test instructions", call.Instructions);
        Assert.Equal("test instructions", call.System);
        Assert.True(InvalidToolInputError.IsInstance(call.Error));
        Assert.Equal(default, call.CancellationToken);

        // Verify the repaired result was used
        Assert.False(result.Invalid);
        JsonAssert.Equal(result.Input, "{\"param1\":\"test\",\"param2\":42}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool call repair::should pass instructions to repairToolCall",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassInstructionsToRepairToolCall()
    {
        ToolCallRepairContext? seen = null;

        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "invalid json"),
            new[] { TestTool(ParamsSchema) },
            repairToolCall: context =>
            {
                seen = context;
                return Task.FromResult<GeneratedToolCall?>(new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\", \"param2\": 42}"));
            },
            instructions: "test instructions");

        Assert.Equal("test instructions", seen!.Instructions);
        Assert.Equal("test instructions", seen.System);
        JsonAssert.Equal(result.Input, "{\"param1\":\"test\",\"param2\":42}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool call repair::should re-throw error if tool call repair returns null",
        Coverage = UpstreamCoverage.Partial,
        Note = "The error message carries the .NET JSON parser text, not the Node SyntaxError text. See #604.")]
    public async Task ShouldReThrowErrorIfToolCallRepairReturnsNull()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "invalid json"),
            new[] { TestTool(ParamsSchema) },
            repairToolCall: _ => Task.FromResult<GeneratedToolCall?>(null));

        Assert.True(result.Dynamic);
        Assert.True(result.Invalid);
        Assert.True(InvalidToolInputError.IsInstance(result.Error));
        Assert.StartsWith("Invalid input for tool testTool: ", result.Error!.Message);
        Assert.Equal(JsonValueKind.String, result.Input.ValueKind);
        Assert.Equal("invalid json", result.Input.GetString());
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool call repair::should stop waiting for repair when aborted",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldStopWaitingForRepairWhenAborted()
    {
        using var abortController = new CancellationTokenSource();
        var repairStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var repair = new TaskCompletionSource<GeneratedToolCall?>(TaskCreationOptions.RunContinuationsAsynchronously);

        var result = ParseAsync(
            new GeneratedToolCall("123", "testTool", "invalid json"),
            new[] { TestTool(ParamsSchema) },
            repairToolCall: context =>
            {
                Assert.Equal(abortController.Token, context.CancellationToken);
                repairStarted.SetResult(true);
                return repair.Task;
            },
            cancellationToken: abortController.Token);

        await repairStarted.Task;
        abortController.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result);

        repair.SetResult(new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\", \"param2\": 42}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool call repair::should reject when aborted during repaired input validation",
        Coverage = UpstreamCoverage.Partial,
        Note = "Schema validation is synchronous, so the test cancels while the repaired input is refined. See #604.")]
    public async Task ShouldRejectWhenAbortedDuringRepairedInputValidation()
    {
        using var abortController = new CancellationTokenSource();
        var validationStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var validationFinished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var result = ParseAsync(
            new GeneratedToolCall("123", "testTool", "invalid json"),
            new[] { TestTool("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}") },
            repairToolCall: context => Task.FromResult<GeneratedToolCall?>(new GeneratedToolCall(context.ToolCall.ToolCallId, context.ToolCall.ToolName, "{\"value\":\"repaired\"}")),
            refineToolInput: new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
            {
                ["testTool"] = async (input, _) =>
                {
                    validationStarted.SetResult(true);
                    await validationFinished.Task;
                    return input;
                },
            },
            cancellationToken: abortController.Token);

        await validationStarted.Task;
        abortController.Cancel();
        validationFinished.SetResult(true);

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result);
        Assert.Equal(abortController.Token, exception.CancellationToken);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool call repair::should throw ToolCallRepairError if repairToolCall throws",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldThrowToolCallRepairErrorIfRepairToolCallThrows()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "invalid json"),
            new[] { TestTool(ParamsSchema) },
            repairToolCall: _ => Task.FromException<GeneratedToolCall?>(new JsError("test error")));

        Assert.True(result.Dynamic);
        Assert.True(result.Invalid);
        Assert.True(ToolCallRepairError.IsInstance(result.Error));
        Assert.Equal("Error repairing tool call: Error: test error", result.Error!.Message);
        Assert.Equal(JsonValueKind.String, result.Input.ValueKind);
        Assert.Equal("invalid json", result.Input.GetString());
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }
}
