// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; programmatic tool calling</c>.</summary>
public sealed class ProgrammaticToolCallingTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling::";

    [Fact]
    [UpstreamTest(
        Id + "should stop for client tool approval while a provider tool result is deferred",
        Coverage = UpstreamCoverage.Partial,
        Note = "The provider tool is declared without supportsDeferredResults, which the port does not have.")]
    public async Task Should_stop_for_client_tool_approval_while_a_provider_tool_result_is_deferred()
    {
        var executeCalls = 0;
        var modelCallCount = 0;
        using var args = JsonDocument.Parse("{}");
        using var programSchema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"code\":{\"type\":\"string\"}},\"required\":[\"code\"],\"additionalProperties\":false}");
        var getHours = Tool.Function(
            "getHours",
            null,
            "{\"type\":\"object\",\"properties\":{\"member\":{\"type\":\"string\"}},\"required\":[\"member\"],\"additionalProperties\":false}",
            (_, _) =>
            {
                executeCalls++;
                return Task.FromResult("8");
            });

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ =>
                {
                    modelCallCount++;
                    return new LanguageModelGenerateResult(
                        new GeneratedContent[]
                        {
                            new GeneratedToolCall("program-call", "program", "{\"code\":\"getHours()\"}") { ProviderExecuted = true },
                            new GeneratedToolCall("get-hours-call", "getHours", "{\"member\":\"Ada\"}"),
                        },
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(3, 10, 13));
                },
            },
            Tools = new[]
            {
                Tool.ProviderDefined("test.program", "program", args.RootElement.Clone(), programSchema.RootElement.Clone()),
                getHours,
            },
            ToolApproval = new ToolApprovalConfiguration { { "getHours", "user-approval" } },
            Prompt = "Get Ada hours with a program.",
            StopWhen = StopWhen.IsStepCount(3),
            GenerateId = MockId("id"),
        });

        Assert.Equal(1, modelCallCount);
        Assert.Equal(0, executeCalls);
        Assert.Single(result.Steps);
        var request = Assert.Single(result.Content.OfType<GeneratedToolApprovalRequest>());
        Assert.Equal("get-hours-call", request.ToolCall!.ToolCallId);
        Assert.Equal("getHours", request.ToolCall.ToolName);
    }
}
