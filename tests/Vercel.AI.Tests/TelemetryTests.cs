// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

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
                await release.Task.ConfigureAwait(false);
                return new SpeechResult(new byte[] { 1 }, "audio/mpeg");
            },
        };
        var pending = Client(telemetry).GenerateSpeechAsync(new GenerateSpeechOptions
        {
            Model = model,
            Text = "hello",
            Voice = "alloy",
        });

        await started.Task.ConfigureAwait(false);
        Assert.False(telemetry.Disposed);
        Assert.Equal("generateSpeech", telemetry.Operation);
        Assert.Equal("tts-1", telemetry.ModelId);
        release.TrySetResult(true);

        var result = await pending.ConfigureAwait(false);
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
        }).ConfigureAwait(false);

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

        var speech = await Assert.ThrowsAsync<AiSdkException>(() => client.GenerateSpeechAsync(new GenerateSpeechOptions())).ConfigureAwait(false);
        var transcription = await Assert.ThrowsAsync<AiSdkException>(() => client.TranscribeAsync(new TranscribeOptions())).ConfigureAwait(false);

        Assert.Contains("speech", speech.Message, StringComparison.Ordinal);
        Assert.Contains("transcription", transcription.Message, StringComparison.Ordinal);
        Assert.Equal(0, telemetry.BeginCount);
        Assert.False(telemetry.Disposed);
    }

    private static AiClient Client(IAiTelemetry telemetry)
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }), telemetry);
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
