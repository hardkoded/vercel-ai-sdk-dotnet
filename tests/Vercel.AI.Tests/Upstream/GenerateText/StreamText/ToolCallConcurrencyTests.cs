// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency.ToolCallConcurrencySupport;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; toolCallConcurrency</c>.</summary>
public sealed class ToolCallConcurrencyTests
{
    private const string EmptySchema = "{\"type\":\"object\",\"properties\":{}}";

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > toolCallConcurrency::should execute tool calls sequentially in model order with concurrency 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_tool_calls_sequentially_in_model_order_with_concurrency_1()
    {
        var firstStarted = new DelayedPromise<bool>();
        var secondStarted = new DelayedPromise<bool>();
        var releaseFirst = new DelayedPromise<bool>();
        var events = new ConcurrentQueue<string>();

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel
            {
                StreamParts = new LanguageModelStreamPart[]
                {
                    new ToolCallStreamPart("call-1", "first", "{}"),
                    new ToolCallStreamPart("call-2", "second", "{}"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13)),
                },
            },
            Tools = new[]
            {
                Tool.Function("first", null, EmptySchema, async (_, _) =>
                {
                    events.Enqueue("first:start");
                    firstStarted.Resolve(true);
                    await releaseFirst.Promise;
                    events.Enqueue("first:end");
                    return "null";
                }),
                Tool.Function("second", null, EmptySchema, (_, _) =>
                {
                    events.Enqueue("second:start");
                    secondStarted.Resolve(true);
                    events.Enqueue("second:end");
                    return Task.FromResult("null");
                }),
            },
            Prompt = "test-input",
            ToolCallConcurrency = 1,
        });

        var stepsTask = result.Steps;

        await firstStarted.Promise.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new[] { "first:start" }, events.ToArray());
        releaseFirst.Resolve(true);
        await secondStarted.Promise.WaitAsync(TimeSpan.FromSeconds(5));
        await WaitForEvents(events, "first:start", "first:end", "second:start", "second:end");
        await stepsTask;
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > toolCallConcurrency::should reject invalid concurrency synchronously", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_invalid_concurrency_synchronously()
    {
        var error = Assert.Throws<InvalidArgumentError>(() => Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel(),
            Prompt = "test-input",
            ToolCallConcurrency = 0,
        }));

        Assert.Equal("Invalid argument for parameter toolCallConcurrency: toolCallConcurrency must be >= 1", error.Message);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
