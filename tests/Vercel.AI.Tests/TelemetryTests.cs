// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
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

    [Fact]
    public async Task Generate_records_token_usage()
    {
        var activity = await StoppedGenerate(new LanguageModelUsage(3, 5, 8));

        AssertIdentity(activity, "generateText", "test");
        Assert.Equal(3, activity.GetTagItem("gen_ai.usage.input_tokens"));
        Assert.Equal(5, activity.GetTagItem("gen_ai.usage.output_tokens"));
        Assert.Equal(8, activity.GetTagItem("gen_ai.usage.total_tokens"));
    }

    [Fact]
    public async Task OnUsage_omits_absent_token_counts()
    {
        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(null, stopped);
        var telemetry = new OpenTelemetryAiTelemetry();
        using (var span = telemetry.Begin("generateText", "test"))
        {
            telemetry.OnUsage(span, new LanguageModelUsage(null, 5, 8));
        }

        var activity = await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Null(activity.GetTagItem("gen_ai.usage.input_tokens"));
        Assert.Equal(5, activity.GetTagItem("gen_ai.usage.output_tokens"));
        Assert.Equal(8, activity.GetTagItem("gen_ai.usage.total_tokens"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.characters"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.seconds"));
    }

    [Fact]
    public async Task Stream_records_token_usage()
    {
        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(null, stopped);
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("text", "response"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 5, 8)),
            },
        };

        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "hi" });
        await foreach (var _ in stream.Stream())
        {
        }

        var activity = await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new LanguageModelUsage(3, 5, 8).InputTokens, (await stream.Usage).InputTokens);
        AssertIdentity(activity, "streamText", "test");
        Assert.Equal(3, activity.GetTagItem("gen_ai.usage.input_tokens"));
        Assert.Equal(5, activity.GetTagItem("gen_ai.usage.output_tokens"));
        Assert.Equal(8, activity.GetTagItem("gen_ai.usage.total_tokens"));
    }

    [Fact]
    public async Task GenerateSpeech_records_character_usage_and_leaves_token_tags_unset()
    {
        var activity = await StoppedSpeech(new SpeechResult(new byte[] { 1 }, "audio/mpeg", new AudioUsage(characters: 12)));

        AssertIdentity(activity, "generateSpeech", "tts-1");
        Assert.Equal(12, activity.GetTagItem("gen_ai.usage.characters"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.input_tokens"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.output_tokens"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.total_tokens"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.seconds"));
    }

    [Fact]
    public async Task GenerateSpeech_records_token_usage()
    {
        var activity = await StoppedSpeech(new SpeechResult(
            new byte[] { 1 },
            "audio/mpeg",
            new AudioUsage(inputTokens: 3, outputTokens: 5, totalTokens: 8)));

        Assert.Equal(3, activity.GetTagItem("gen_ai.usage.input_tokens"));
        Assert.Equal(5, activity.GetTagItem("gen_ai.usage.output_tokens"));
        Assert.Equal(8, activity.GetTagItem("gen_ai.usage.total_tokens"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.characters"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.seconds"));
    }

    [Fact]
    public async Task Transcribe_records_duration_usage()
    {
        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(null, stopped);
        var result = await Client().TranscribeAsync(new TranscribeOptions
        {
            Model = new StubTranscriptionModel("whisper-1", new AudioUsage(seconds: 1.5)),
            Audio = new AudioInput(new byte[] { 9 }, "audio/wav", "a.wav"),
        });

        var activity = await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("noted", result.Text);
        Assert.Equal(1.5, result.Usage!.Seconds);
        AssertIdentity(activity, "transcribe", "whisper-1");
        Assert.Equal(1.5, activity.GetTagItem("gen_ai.usage.seconds"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.characters"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.input_tokens"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.output_tokens"));
        Assert.Null(activity.GetTagItem("gen_ai.usage.total_tokens"));
    }

    [Fact]
    public async Task Speech_and_transcription_without_usage_set_no_usage_tags()
    {
        var speech = await StoppedSpeech(new SpeechResult(new byte[] { 1 }, "audio/mpeg"));
        AssertNoUsageTags(speech);
        AssertIdentity(speech, "generateSpeech", "tts-1");

        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(null, stopped);
        var result = await Client().TranscribeAsync(new TranscribeOptions
        {
            Model = new StubTranscriptionModel("whisper-1"),
            Audio = new AudioInput(new byte[] { 9 }, "audio/wav", "a.wav"),
        });

        var transcription = await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Null(result.Usage);
        AssertIdentity(transcription, "transcribe", "whisper-1");
        AssertNoUsageTags(transcription);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public async Task Non_finite_duration_stays_off_the_span(double seconds)
    {
        var activity = await StoppedSpeech(new SpeechResult(new byte[] { 1 }, "audio/mpeg", new AudioUsage(seconds: seconds)));

        Assert.Null(activity.GetTagItem("gen_ai.usage.seconds"));
        AssertNoUsageTags(activity);
    }

    [Fact]
    public async Task Thrown_speech_call_disposes_the_span_without_usage_tags()
    {
        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(null, stopped);
        var model = new StubSpeechModel("tts-1")
        {
            OnGenerate = (_, _) => throw new InvalidOperationException("fail"),
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => Client().GenerateSpeechAsync(new GenerateSpeechOptions
        {
            Model = model,
            Text = "hello",
        }));

        var activity = await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
        AssertIdentity(activity, "generateSpeech", "tts-1");
        AssertNoUsageTags(activity);
    }

    private static async Task<Activity> StoppedGenerate(LanguageModelUsage usage)
    {
        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(null, stopped);
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("response") },
                FinishReason.Stop,
                usage),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "hi" });
        Assert.Equal(usage.InputTokens, result.Usage.InputTokens);
        Assert.Equal(usage.OutputTokens, result.Usage.OutputTokens);
        Assert.Equal(usage.TotalTokens, result.Usage.TotalTokens);
        return await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static async Task<Activity> StoppedSpeech(SpeechResult speech)
    {
        var stopped = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = Listen(null, stopped);
        var model = new StubSpeechModel("tts-1")
        {
            OnGenerate = (_, _) => Task.FromResult(speech),
        };
        var result = await Client().GenerateSpeechAsync(new GenerateSpeechOptions { Model = model, Text = "hello" });
        Assert.Same(speech, result);
        return await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static void AssertIdentity(Activity activity, string operation, string modelId)
    {
        Assert.Equal(operation, activity.OperationName);
        Assert.Equal(operation, activity.GetTagItem("gen_ai.operation.name"));
        Assert.Equal(modelId, activity.GetTagItem("gen_ai.request.model"));
    }

    private static void AssertNoUsageTags(Activity activity)
    {
        foreach (var tag in activity.TagObjects)
        {
            Assert.False(tag.Key.StartsWith("gen_ai.usage.", StringComparison.Ordinal), tag.Key);
        }
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

        public void OnUsage(IDisposable span, LanguageModelUsage usage)
        {
        }

        public void OnUsage(IDisposable span, AudioUsage? usage)
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
        private readonly AudioUsage? _usage;

        public StubTranscriptionModel(string modelId, AudioUsage? usage = null)
        {
            ModelId = modelId;
            _usage = usage;
        }

        public string Provider => "test";

        public string ModelId { get; }

        public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TranscriptionResult("noted", _usage));
        }
    }
}
