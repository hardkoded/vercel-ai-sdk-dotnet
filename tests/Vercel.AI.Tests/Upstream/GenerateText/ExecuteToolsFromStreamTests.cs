// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency.ToolCallConcurrencySupport;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>execute-tools-from-stream.test.ts</c> &gt; <c>executeToolsFromStream</c>.</summary>
public sealed class ExecuteToolsFromStreamTests
{
    [Theory]
    [InlineData(FinishReason.Length)]
    [InlineData(FinishReason.Error)]
    [InlineData(FinishReason.ContentFilter)]
    [InlineData(FinishReason.Interrupted)]
    [InlineData(FinishReason.Other)]
    [UpstreamTest(
        "packages/ai/src/generate-text/execute-tools-from-stream.test.ts::executeToolsFromStream::should not execute tools when the finish reason is %s",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port has no executeToolsFromStream transformer. The test drives the tool loop that StreamTextAsync uses.")]
    public async Task Should_not_execute_tools_when_the_finish_reason_is(FinishReason finishReason)
    {
        var executed = false;
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "testTool", "{\"value\":\"test\"}"),
                new FinishStreamPart(finishReason, new LanguageModelUsage(3, 10, 13), finishReason.ToString()),
            },
        };

        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function("testTool", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}", (_, _) =>
                {
                    executed = true;
                    return Task.FromResult("tool-result");
                }),
            },
        });
        await foreach (var _ in result.Stream())
        {
        }

        Assert.False(executed);
    }

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
