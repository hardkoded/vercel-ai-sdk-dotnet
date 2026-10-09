// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; abort signal</c>.</summary>
public sealed class AbortSignalTests
{
    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(5);

    public static TheoryData<string> Apis => new() { "streamText", "ToolLoopAgent" };

    [Theory]
    [MemberData(nameof(Apis))]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > abort signal::%s should reject on abort without waiting for callbacks or cancellation", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_on_abort_without_waiting_for_callbacks_or_cancellation(string api)
    {
        var abortController = new AbortController();
        var reason = new JsError("manual abort");
        var outputReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var abortCallback = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var model = new StreamTextAbortSupport.OpenStreamModel(
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "Hello"));
        var onAbortCalls = new List<StreamAbortContext>();
        var onErrorCalls = 0;
        var onFinishCalls = 0;
        Task OnAbort(StreamAbortContext context, CancellationToken cancellationToken)
        {
            onAbortCalls.Add(context);
            return abortCallback.Task;
        }

        StreamTextResult result;
        if (api == "streamText")
        {
            result = StreamTextAbortSupport.Client().StreamTextAsync(new StreamTextOptions
            {
                Model = model,
                Prompt = "test-input",
                AbortSignal = abortController.Signal,
                OnAbort = OnAbort,
                OnError = (_, _) =>
                {
                    onErrorCalls++;
                    return Task.CompletedTask;
                },
                OnFinish = (_, _) =>
                {
                    onFinishCalls++;
                    return Task.CompletedTask;
                },
            });
        }
        else
        {
            result = new Agent(new AgentOptions { Model = model, OnAbort = OnAbort }, StreamTextAbortSupport.Client())
                .Stream("test-input", abortSignal: abortController.Signal);
        }

        var stream = StreamTextAbortSupport.Collect(result, outputReceived);
        var results = new Task[] { result.Text, result.Steps, result.FinishReason, result.Usage };

        await outputReceived.Task.WaitAsync(SettleTimeout);
        abortController.Abort(reason);

        try
        {
            await StreamTextAbortSupport.Settle(results);

            // Only let OnAbort finish after the result tasks have rejected.
            abortCallback.TrySetResult();
            var parts = await stream.WaitAsync(SettleTimeout);
            var abort = Assert.IsType<AbortPart>(parts[^1]);
            Assert.Equal("Error: manual abort", abort.Reason);
            Assert.Equal(1, model.Disposals);
            var context = Assert.Single(onAbortCalls);
            Assert.Empty(context.Steps);
            Assert.Same(reason, context.Exception);
            Assert.Equal(0, onErrorCalls);
            Assert.Equal(0, onFinishCalls);
        }
        finally
        {
            abortCallback.TrySetResult();
        }

        foreach (var task in results)
        {
            Assert.True(task.IsFaulted);
            Assert.Same(reason, task.Exception!.InnerException);
        }
    }
}
