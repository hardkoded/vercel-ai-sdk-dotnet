// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; programmatic tool calling</c>.</summary>
public sealed class ProgrammaticToolCallingTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > programmatic tool calling::";

    private static Tool DeferredProgramTool(JsonElement args, JsonElement schema)
    {
        var tool = Tool.ProviderDefined("test.program", "program", args, schema);
        tool.SupportsDeferredResults = true;
        return tool;
    }

    [Fact]
    [UpstreamTest(
        Id + "should stop an interrupted stream with a deferred provider tool result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stop_an_interrupted_stream_with_a_deferred_provider_tool_result()
    {
        var errors = new List<Exception>();
        using var args = JsonDocument.Parse("{}");
        using var schema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{}}");
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "program", "{}") { ProviderExecuted = true },
                new FinishStreamPart(FinishReason.Interrupted, new LanguageModelUsage(3, 10, 13)),
            },
        };

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "Run a program",
                    Tools = new[]
                    {
                        DeferredProgramTool(args.RootElement.Clone(), schema.RootElement.Clone()),
                    },
                    StopWhen = StopWhen.IsStepCount(3),
                    OnError = (error, _) =>
                    {
                        errors.Add(error);
                        return Task.CompletedTask;
                    },
                }
            );

        await foreach (var _ in result.Stream()) { }

        Assert.Single(model.Calls);
        Assert.Single(await result.Steps);
        Assert.Equal(FinishReason.Interrupted, await result.FinishReason);
        Assert.Empty(errors);
    }

    [Fact]
    [UpstreamTest(
        Id + "should stop for client tool approval while a provider tool result is deferred",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stop_for_client_tool_approval_while_a_provider_tool_result_is_deferred()
    {
        var executeCalls = 0;
        using var args = JsonDocument.Parse("{}");
        using var programSchema = JsonDocument.Parse(
            "{\"type\":\"object\",\"properties\":{\"code\":{\"type\":\"string\"}},\"required\":[\"code\"],\"additionalProperties\":false}"
        );
        var getHours = Tool.Function(
            "getHours",
            null,
            "{\"type\":\"object\",\"properties\":{\"member\":{\"type\":\"string\"}},\"required\":[\"member\"],\"additionalProperties\":false}",
            (_, _) =>
            {
                executeCalls++;
                return Task.FromResult("8");
            }
        );

        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("program-call", "program", "{\"code\":\"getHours()\"}")
                {
                    ProviderExecuted = true,
                },
                new ToolCallStreamPart("get-hours-call", "getHours", "{\"member\":\"Ada\"}"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13)),
            },
        };

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools = new[]
                    {
                        DeferredProgramTool(
                            args.RootElement.Clone(),
                            programSchema.RootElement.Clone()
                        ),
                        getHours,
                    },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        { "getHours", "user-approval" },
                    },
                    Prompt = "Get Ada hours with a program.",
                    StopWhen = StopWhen.IsStepCount(3),
                    GenerateId = MockId("id"),
                }
            );

        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Single(model.Calls);
        Assert.Equal(0, executeCalls);
        Assert.Single(await result.Steps);
        Assert.DoesNotContain(parts, part => part.Type == "error");
        var request = Assert.IsType<ToolApprovalRequestPart>(
            Assert.Single(parts, part => part.Type == "tool-approval-request")
        );
        Assert.Equal("get-hours-call", request.ToolCall.ToolCallId);
        Assert.Equal("getHours", request.ToolCall.ToolName);
    }

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
}
