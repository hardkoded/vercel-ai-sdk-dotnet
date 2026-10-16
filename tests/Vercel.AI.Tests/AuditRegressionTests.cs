// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.StreamText;

namespace Vercel.AI.Tests;

/// <summary>Regression tests for cancellation and cleanup fixes found by a C# audit. None has an upstream test.</summary>
public sealed class AuditRegressionTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task Stream_stops_the_provider_read_when_the_consumer_breaks_early()
    {
        var model = new StreamTextAbortSupport.OpenStreamModel(new TextStartStreamPart("1"), new TextDeltaStreamPart("1", "Hello"));
        var result = StreamTextAbortSupport.Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input" });

        await foreach (var part in result.Stream())
        {
            if (part is TextDeltaPart)
            {
                break;
            }
        }

        await WaitUntil(() => model.Disposals == 1);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result.Text.WaitAsync(Timeout));
    }

    [Fact]
    public async Task Stream_keeps_reading_while_another_consumer_is_active()
    {
        var model = new StreamTextAbortSupport.OpenStreamModel(new TextStartStreamPart("1"), new TextDeltaStreamPart("1", "Hello"));
        var result = StreamTextAbortSupport.Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input" });
        var seen = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = StreamTextAbortSupport.Collect(result, seen);
        await seen.Task.WaitAsync(Timeout);

        await foreach (var part in result.Stream())
        {
            if (part is TextDeltaPart)
            {
                break;
            }
        }

        await Task.Delay(100);
        Assert.Equal(0, model.Disposals);
    }

    [Fact]
    public async Task GenerateText_throws_cancellation_when_a_tool_is_cancelled_and_a_stop_condition_is_met()
    {
        using var cancellation = new CancellationTokenSource();
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{}") },
                FinishReason.ToolCalls,
                new LanguageModelUsage(3, 10, 13)),
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => StreamTextAbortSupport.Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.Function("tool1", null, "{\"type\":\"object\"}", (_, token) =>
                    {
                        cancellation.Cancel();
                        token.ThrowIfCancellationRequested();
                        return Task.FromResult("unreachable");
                    }),
                },
            },
            cancellation.Token));
    }

    [Fact]
    public async Task Transcribe_full_stream_observes_the_enumerator_token()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new StalledTranscriber(), Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        var read = Task.Run(async () =>
        {
            await foreach (var _ in result.FullStream.WithCancellation(cancellation.Token))
            {
            }
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read.WaitAsync(Timeout));
    }

    [Fact]
    public async Task Embed_reports_the_provider_error_when_the_telemetry_error_callback_throws()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Embed.EmbedAsync(new EmbedRequest
        {
            Model = new FailingEmbedder(),
            Value = "a",
            MaxRetries = 0,
            Telemetry = new EmbedTelemetry { OnError = (_, _) => throw new ApplicationException("telemetry failed") },
        }));
        Assert.Equal("provider failed", error.Message);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + Timeout;
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "The condition did not become true in time.");
            await Task.Delay(10);
        }
    }

    private sealed class StalledTranscriber : ITranscriptionCaller
    {
        public string Provider => "test";

        public string ModelId => "stalled";

        public string SpecificationVersion => "v4";

        public bool CanStream => true;

        public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
        {
            return Task.FromResult<TranscriptionStreamStart?>(new TranscriptionStreamStart(new ModelPartStream(Array.Empty<ModelStreamPart>(), completes: false)));
        }
    }

    private sealed class FailingEmbedder : IEmbeddingCaller
    {
        public string Provider => "test";

        public string ModelId => "failing";

        public string SpecificationVersion => "v4";

        public int? MaxEmbeddingsPerCall => null;

        public double? MaxInputBytesPerCall => null;

        public bool SupportsParallelCalls => true;

        public Task<JsonElement?> TransformProviderOptionsAsync(JsonElement? providerOptions, IReadOnlyList<string> values, int startIndex, int endIndex, CancellationToken cancellationToken) => Task.FromResult(providerOptions);

        public Task<EmbeddingModelResponse> DoEmbedAsync(EmbeddingModelCall call, CancellationToken cancellationToken) => throw new InvalidOperationException("provider failed");
    }
}
