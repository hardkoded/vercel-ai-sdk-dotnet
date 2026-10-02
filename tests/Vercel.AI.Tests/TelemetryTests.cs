// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.Gateway;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

public sealed class TelemetryTests
{
    [Fact]
    public void Begin_returns_a_disposable_span()
    {
        var telemetry = new OpenTelemetryAiTelemetry();
        using var span = telemetry.Begin("generateText", "test-model");
        Assert.NotNull(span);
    }

    [Theory]
    [InlineData(FinishReason.Error, ActivityStatusCode.Error)]
    [InlineData(FinishReason.Stop, ActivityStatusCode.Unset)]
    public async Task Generate_span_status_follows_the_finish_reason(FinishReason finishReason, ActivityStatusCode expected)
    {
        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(started: null, stopped);
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("response") },
                finishReason,
                LanguageModelUsage.Empty),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "hi" });

        Assert.Equal(finishReason, result.FinishReason);
        var activity = await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("generateText", activity.OperationName);
        Assert.Equal(expected, activity.Status);
    }

    [Theory]
    [InlineData(FinishReason.Error, ActivityStatusCode.Error)]
    [InlineData(FinishReason.Stop, ActivityStatusCode.Unset)]
    public async Task Stream_span_stays_open_until_finish_and_status_follows_the_finish_reason(FinishReason finishReason, ActivityStatusCode expected)
    {
        var started = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stepReached = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(started, stopped);
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("text", "response"),
                new FinishStreamPart(finishReason, LanguageModelUsage.Empty),
            },
        };

        StreamTextResult stream;
        try
        {
            stream = Client().StreamTextAsync(new StreamTextOptions
            {
                Model = model,
                Prompt = "hi",
                OnStepEnd = async (_, _) =>
                {
                    stepReached.TrySetResult(true);
                    await release.Task;
                },
            });

            Assert.True(await stepReached.Task.WaitAsync(TimeSpan.FromSeconds(5)));
            var activity = await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(activity.IsStopped);
            Assert.False(stream.FinishReason.IsCompleted);
        }
        finally
        {
            release.TrySetResult(true);
        }

        var parts = new List<TextStreamPart>();
        await foreach (var part in stream.Stream())
        {
            parts.Add(part);
        }

        var stoppedActivity = await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("streamText", stoppedActivity.OperationName);
        Assert.Equal(expected, stoppedActivity.Status);
        Assert.Equal(finishReason, await stream.FinishReason);
        Assert.Contains(parts, part => part is FinishPart finish && finish.FinishReason == finishReason);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }), new OpenTelemetryAiTelemetry());
    }

    private static ActivityListener Listen(TaskCompletionSource<Activity>? started, TaskCompletionSource<Activity> stopped)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == OpenTelemetryAiTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => started?.TrySetResult(activity),
            ActivityStopped = activity => stopped.TrySetResult(activity),
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }
}
