// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.repairToolCall</c>.</summary>
public sealed class OptionsRepairToolCallTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.repairToolCall::";

    private const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";

    [Fact]
    [UpstreamTest(Prefix + "should use the repaired tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_repaired_tool_call()
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = InvalidToolCallModel(),
            Tools = RepairToolCallTools(),
            Prompt = "test-input",
            RepairToolCall = RepairInput,
        });

        var toolCall = Assert.Single(result.ToolCalls);
        Assert.Equal("call-1", toolCall.ToolCallId);
        Assert.Equal("tool1", toolCall.ToolName);
        Assert.Equal("{\"value\":\"repaired\"}", Compact(toolCall.ArgumentsJson));
        Assert.False(toolCall.Invalid);
        var toolResult = Assert.Single(result.ToolResults);
        Assert.Equal("\"result-repaired\"", toolResult.OutputJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not execute a repaired tool when aborted during input validation",
        Coverage = UpstreamCoverage.Partial,
        Note = "Schema validation is synchronous, so the test cancels while the repaired input is refined. See #604.")]
    public async Task Should_not_execute_a_repaired_tool_when_aborted_during_input_validation()
    {
        using var cancellation = new CancellationTokenSource();
        var validationStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var validationFinished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var executed = false;

        var result = Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = InvalidToolCallModel(),
                Tools = new[]
                {
                    Tool.Function("tool1", null, ValueSchema, (_, _) =>
                    {
                        executed = true;
                        return Task.FromResult("\"tool result\"");
                    }),
                },
                RefineToolInput = new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
                {
                    ["tool1"] = async (input, _) =>
                    {
                        validationStarted.SetResult(true);
                        await validationFinished.Task;
                        return input;
                    },
                },
                Prompt = "test-input",
                RepairToolCall = RepairInput,
            },
            cancellation.Token);

        await validationStarted.Task;
        cancellation.Cancel();
        validationFinished.SetResult(true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result);
        Assert.False(executed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support the deprecated experimental_repairToolCall option", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_the_deprecated_experimental_repairToolCall_option()
    {
#pragma warning disable CS0618
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = InvalidToolCallModel(),
            Tools = RepairToolCallTools(),
            Prompt = "test-input",
            ExperimentalRepairToolCall = RepairInput,
        });
#pragma warning restore CS0618

        var toolCall = Assert.Single(result.ToolCalls);
        Assert.Equal("call-1", toolCall.ToolCallId);
        Assert.Equal("tool1", toolCall.ToolName);
        Assert.Equal("{\"value\":\"repaired\"}", Compact(toolCall.ArgumentsJson));
        var toolResult = Assert.Single(result.ToolResults);
        Assert.Equal("\"result-repaired\"", toolResult.OutputJson);
    }

    private static TestLanguageModel InvalidToolCallModel() =>
        new() { OnGenerate = _ => ToolCallResponse("{ \"value\": broken") };

    private static IReadOnlyList<Tool> RepairToolCallTools() =>
        new[] { Tool.Function("tool1", null, ValueSchema, (input, _) => Task.FromResult("\"result-" + input.GetProperty("value").GetString() + "\"")) };

    private static Task<GeneratedToolCall?> RepairInput(ToolCallRepairContext context) =>
        Task.FromResult<GeneratedToolCall?>(new GeneratedToolCall(context.ToolCall.ToolCallId, context.ToolCall.ToolName, "{ \"value\": \"repaired\" }"));
}
