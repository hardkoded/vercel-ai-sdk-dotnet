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

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>toolCallConcurrency</c>.</summary>
public sealed class ToolCallConcurrencyTests
{
    private const string EmptySchema = "{\"type\":\"object\",\"properties\":{}}";

    private static TestLanguageModel Model()
    {
        return new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedToolCall("call-1", "first", "{}"),
                    new GeneratedToolCall("call-2", "second", "{}"),
                },
                FinishReason.ToolCalls,
                new LanguageModelUsage(3, 10, 13)),
        };
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::toolCallConcurrency::should execute tool calls concurrently by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_tool_calls_concurrently_by_default()
    {
        var release = new DelayedPromise<bool>();
        var events = new ConcurrentQueue<string>();

        var resultTask = Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = Model(),
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function("first", null, EmptySchema, async (_, _) =>
                {
                    events.Enqueue("first:start");
                    await release.Promise;
                    events.Enqueue("first:end");
                    return "null";
                }),
                Tool.Function("second", null, EmptySchema, async (_, _) =>
                {
                    events.Enqueue("second:start");
                    await release.Promise;
                    events.Enqueue("second:end");
                    return "null";
                }),
            },
        });

        await WaitForEvents(events, "first:start", "second:start");
        release.Resolve(true);
        await resultTask;
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::toolCallConcurrency::should execute tool calls sequentially in model order with concurrency 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_tool_calls_sequentially_in_model_order_with_concurrency_1()
    {
        var releaseFirst = new DelayedPromise<bool>();
        var events = new ConcurrentQueue<string>();

        var resultTask = Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = Model(),
            Prompt = "test-input",
            ToolCallConcurrency = 1,
            Tools = new[]
            {
                Tool.Function("first", null, EmptySchema, async (_, _) =>
                {
                    events.Enqueue("first:start");
                    await releaseFirst.Promise;
                    events.Enqueue("first:end");
                    return "null";
                }),
                Tool.Function("second", null, EmptySchema, (_, _) =>
                {
                    events.Enqueue("second:start");
                    events.Enqueue("second:end");
                    return Task.FromResult("null");
                }),
            },
        });

        await WaitForEvents(events, "first:start");
        releaseFirst.Resolve(true);
        await resultTask;

        Assert.Equal(new[] { "first:start", "first:end", "second:start", "second:end" }, events.ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::toolCallConcurrency::should reject invalid concurrency before calling the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_invalid_concurrency_before_calling_the_model()
    {
        var model = new TestLanguageModel();

        var error = await Assert.ThrowsAsync<InvalidArgumentError>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            ToolCallConcurrency = 0,
        }));

        Assert.Equal("toolCallConcurrency", error.Parameter);
        Assert.Equal(0, error.Value);
        Assert.Empty(model.Calls);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
