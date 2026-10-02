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

    [Fact]
    public async Task GenerateSpeechAsync_records_generateSpeech_and_disposes_after_the_model_call()
    {
        var telemetry = new RecordingTelemetry();
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var model = new StubSpeechModel("tts-1")
        {
            OnGenerate = async (_, _) =>
            {
                Assert.False(telemetry.Disposed);
                started.TrySetResult(true);
                await release.Task;
                return new SpeechResult(new byte[] { 1 }, "audio/mpeg");
            },
        };
        var pending = Client(telemetry).GenerateSpeechAsync(new GenerateSpeechOptions
        {
            Model = model,
            Text = "hello",
            Voice = "alloy",
        });

        await started.Task;
        Assert.False(telemetry.Disposed);
        Assert.Equal("generateSpeech", telemetry.Operation);
        Assert.Equal("tts-1", telemetry.ModelId);
        release.TrySetResult(true);

        var result = await pending;
        Assert.Equal(new byte[] { 1 }, result.Audio);
        Assert.True(telemetry.Disposed);
        Assert.Equal(1, telemetry.BeginCount);
    }

    [Fact]
    public async Task TranscribeAsync_records_transcribe()
    {
        var telemetry = new RecordingTelemetry();
        var model = new StubTranscriptionModel("whisper-1");
        var result = await Client(telemetry).TranscribeAsync(new TranscribeOptions
        {
            Model = model,
            Audio = new AudioInput(new byte[] { 9 }, "audio/wav", "a.wav"),
        });

        Assert.Equal("noted", result.Text);
        Assert.Equal("transcribe", telemetry.Operation);
        Assert.Equal("whisper-1", telemetry.ModelId);
        Assert.True(telemetry.Disposed);
        Assert.Equal(1, telemetry.BeginCount);
    }

    [Fact]
    public async Task Null_model_throws_and_records_nothing()
    {
        var telemetry = new RecordingTelemetry();
        var client = Client(telemetry);

        var speech = await Assert.ThrowsAsync<AiSdkException>(() => client.GenerateSpeechAsync(new GenerateSpeechOptions()));
        var transcription = await Assert.ThrowsAsync<AiSdkException>(() => client.TranscribeAsync(new TranscribeOptions()));

        Assert.Contains("speech", speech.Message, StringComparison.Ordinal);
        Assert.Contains("transcription", transcription.Message, StringComparison.Ordinal);
        Assert.Equal(0, telemetry.BeginCount);
        Assert.False(telemetry.Disposed);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }), new OpenTelemetryAiTelemetry());
    }

    private static AiClient Client(IAiTelemetry telemetry)
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }), telemetry);
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

    private sealed class RecordingTelemetry : IAiTelemetry
    {
        public string? Operation { get; private set; }

        public string? ModelId { get; private set; }

        public int BeginCount { get; private set; }

        public bool Disposed { get; private set; }

        public IDisposable Begin(string operation, string modelId)
        {
            BeginCount++;
            Operation = operation;
            ModelId = modelId;
            return new Scope(this);
        }

        public void OnFinish(IDisposable span, FinishReason finishReason)
        {
        }

        private sealed class Scope : IDisposable
        {
            private readonly RecordingTelemetry _owner;

            public Scope(RecordingTelemetry owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                _owner.Disposed = true;
            }
        }
    }

    private sealed class StubSpeechModel : ISpeechModel
    {
        public StubSpeechModel(string modelId)
        {
            ModelId = modelId;
        }

        public string Provider => "test";

        public string ModelId { get; }

        public Func<SpeechCallOptions, CancellationToken, Task<SpeechResult>>? OnGenerate { get; set; }

        public Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            if (OnGenerate != null)
            {
                return OnGenerate(options, cancellationToken);
            }

            return Task.FromResult(new SpeechResult(new byte[] { 1 }, "audio/mpeg"));
        }
    }

    private sealed class StubTranscriptionModel : ITranscriptionModel
    {
        public StubTranscriptionModel(string modelId)
        {
            ModelId = modelId;
        }

        public string Provider => "test";

        public string ModelId { get; }

        public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TranscriptionResult("noted"));
        }
    }
}
