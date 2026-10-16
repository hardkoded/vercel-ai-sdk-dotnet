// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>abort signal handling</c>.</summary>
public sealed class AbortSignalHandlingTests
{
    private const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::abort signal handling::should reject when the abort signal fires during tool execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_when_the_abort_signal_fires_during_tool_execution()
    {
        using var cancellation = new CancellationTokenSource();
        var modelCallCount = 0;
        var modelCallEnds = new List<LanguageModelCallEndEvent>();
        var finished = false;
        using var metadata = JsonDocument.Parse("{\"gateway\":{\"generationId\":\"generation-id\"}}");
        var model = new TestLanguageModel
        {
            OnGenerate = _ =>
            {
                modelCallCount++;
                return modelCallCount == 1
                    ? new LanguageModelGenerateResult(
                        new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"value\" }") },
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13),
                        providerMetadata: metadata.RootElement.Clone())
                    : new LanguageModelGenerateResult(Array.Empty<GeneratedContent>(), FinishReason.Other, new LanguageModelUsage(3, 10, 13));
            },
        };

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.Function("tool1", null, ValueSchema, (_, token) =>
                    {
                        cancellation.Cancel();
                        token.ThrowIfCancellationRequested();
                        return Task.FromResult("null");
                    }),
                },
                StopWhen = StopWhen.IsStepCount(10),
                MaxRetries = 0,
                OnLanguageModelCallEnd = (e, _) =>
                {
                    modelCallEnds.Add(e);
                    return Task.CompletedTask;
                },
                OnFinish = (_, _) =>
                {
                    finished = true;
                    return Task.CompletedTask;
                },
            },
            cancellation.Token));

        Assert.NotNull(exception);
        Assert.Equal(1, modelCallCount);
        var end = Assert.Single(modelCallEnds);
        Assert.Equal("generation-id", end.ProviderMetadata!.Value.GetProperty("gateway").GetProperty("generationId").GetString());
        Assert.False(finished);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::abort signal handling::should reject before another model call when a tool completes after cancellation", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_before_another_model_call_when_a_tool_completes_after_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var modelCallCount = 0;
        var model = new TestLanguageModel
        {
            OnGenerate = _ =>
            {
                modelCallCount++;
                return modelCallCount == 1
                    ? new LanguageModelGenerateResult(
                        new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"value\" }") },
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13))
                    : new LanguageModelGenerateResult(Array.Empty<GeneratedContent>(), FinishReason.Other, new LanguageModelUsage(3, 10, 13));
            },
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.Function("tool1", null, ValueSchema, (_, _) =>
                    {
                        cancellation.Cancel();
                        return Task.FromResult("\"tool result\"");
                    }),
                },
                StopWhen = StopWhen.IsStepCount(10),
                MaxRetries = 0,
            },
            cancellation.Token));

        Assert.Equal(1, modelCallCount);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
