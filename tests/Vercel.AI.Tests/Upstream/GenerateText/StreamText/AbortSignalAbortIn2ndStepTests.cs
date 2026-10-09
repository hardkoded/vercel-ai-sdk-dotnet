// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; abort signal &gt; abort in 2nd step</c>.</summary>
public sealed class AbortSignalAbortIn2ndStepTests
{
    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(5);

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > abort signal > abort in 2nd step::should reject result promises when aborting after a completed step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_result_promises_when_aborting_after_a_completed_step()
    {
        using var cancellation = new CancellationTokenSource();
        var streamCalls = 0;
        var model = new StreamTextAbortSupport.ScriptedStreamModel(() =>
        {
            streamCalls++;
            return streamCalls == 1 ? StreamTextAbortSupport.FirstStep() : StreamTextAbortSupport.SecondStep(cancellation);
        });
        var result = StreamTextAbortSupport.Client().StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.Function(
                        "tool1",
                        null,
                        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
                        (_, _) => Task.FromResult("\"result1\"")),
                },
                StopWhen = StopWhen.IsStepCount(3),
            },
            cancellation.Token);

        await StreamTextAbortSupport.Collect(result, null).WaitAsync(SettleTimeout);

        await StreamTextAbortSupport.Settle(new Task[] { result.Text, result.Steps, result.FinishReason, result.Usage });
        foreach (var task in new Task[] { result.Text, result.Steps, result.FinishReason, result.Usage })
        {
            Assert.True(task.IsFaulted);
            Assert.IsAssignableFrom<OperationCanceledException>(task.Exception!.InnerException);
        }
    }
}
