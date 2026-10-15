// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>abort signal handling</c>. The port cancels with a <see cref="CancellationToken"/>.</summary>
public sealed class AbortSignalHandlingTests
{
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::abort signal handling::should reject when the abort signal fires during tool execution",
        Coverage = UpstreamCoverage.Partial,
        Note = "Does not assert onLanguageModelCallEnd, which the port has no equivalent for (issue 468). Cancels with a CancellationToken instead of an abort signal.")]
    public async Task Rejects_when_the_abort_signal_fires_during_tool_execution()
    {
        using var cancellation = new CancellationTokenSource();
        var modelCalls = 0;
        var finished = 0;
        var model = ToolCallThenOther(() => modelCalls++);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                StopWhen = StopWhen.IsStepCount(10),
                OnFinish = (_, _) =>
                {
                    finished++;
                    return Task.CompletedTask;
                },
                Tools = new[]
                {
                    Tool.Function("tool1", null, Schema, (_, token) =>
                    {
                        cancellation.Cancel();
                        token.ThrowIfCancellationRequested();
                        return Task.FromResult("unreachable");
                    }),
                },
            },
            cancellation.Token));

        Assert.Equal(1, modelCalls);
        Assert.Equal(0, finished);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::abort signal handling::should reject before another model call when a tool completes after cancellation",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_before_another_model_call_when_a_tool_completes_after_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var modelCalls = 0;
        var model = ToolCallThenOther(() => modelCalls++);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                StopWhen = StopWhen.IsStepCount(10),
                Tools = new[]
                {
                    Tool.Function("tool1", null, Schema, (_, _) =>
                    {
                        cancellation.Cancel();
                        return Task.FromResult("tool result");
                    }),
                },
            },
            cancellation.Token));

        Assert.Equal(1, modelCalls);
    }

    private static TestLanguageModel ToolCallThenOther(Action onCall)
    {
        var calls = 0;
        return new TestLanguageModel
        {
            OnGenerate = _ =>
            {
                onCall();
                return ++calls == 1
                    ? new LanguageModelGenerateResult(
                        new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"value\" }") },
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(3, 10, 13))
                    : new LanguageModelGenerateResult(Array.Empty<GeneratedContent>(), FinishReason.Other, new LanguageModelUsage(3, 10, 13));
            },
        };
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
