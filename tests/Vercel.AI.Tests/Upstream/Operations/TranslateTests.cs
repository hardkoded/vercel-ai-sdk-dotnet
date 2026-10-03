// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for speech translation.</summary>
public sealed class TranslateTests
{
    private const string Prefix = "packages/ai/src/translate/stream-translate.test.ts::experimental_streamTranslate::";

    [Fact]
    [UpstreamTest(Prefix + "should send args to doStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_arguments_to_do_stream()
    {
        var audio = new AudioChunkStream();
        var model = new TranslateFake { Parts = Finish("hola", "hello") };
        var result = StreamTranslate.Start(new StreamTranslateRequest
        {
            Model = model,
            Audio = audio,
            InputAudioFormat = "pcm16",
            TargetLanguage = "es",
            SourceLanguage = "en",
            OutputAudioFormat = "mp3",
            IncludeRawChunks = false,
            ProviderOptions = OperationJson.Parse("{\"p\":true}"),
            Headers = new Dictionary<string, string> { ["X-Test"] = "1" },
        });
        await model.Started.Task;
        Assert.Same(audio, model.Call!.Audio);
        Assert.Equal("pcm16", model.Call.InputAudioFormat);
        Assert.Equal("es", model.Call.TargetLanguage);
        Assert.Equal("en", model.Call.SourceLanguage);
        Assert.Equal("mp3", model.Call.OutputAudioFormat);
        Assert.False(model.Call.IncludeRawChunks);
        Assert.Equal("1", model.Call.Headers["x-test"]);
        Assert.Equal("ai/0.0.0-test", model.Call.Headers["user-agent"]);
        Assert.Equal("hola", await result.TranslationText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream translation parts and resolve final metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_translation_parts_and_resolves_metadata()
    {
        var model = new TranslateFake
        {
            Parts = new[]
            {
                new ModelStreamPart { Type = "stream-start", Warnings = new[] { OperationWarning.Other("note") } },
                new ModelStreamPart { Type = "output-text-delta", Delta = "ho" },
                new ModelStreamPart { Type = "finish", SourceText = "hello", OutputText = "hola", DurationInSeconds = 1.2, Usage = new OperationUsage(3, 2, 5), ProviderMetadata = OperationJson.Parse("{\"p\":true}") },
            },
        };
        var result = StreamTranslate.Start(Request(model));
        var parts = await Read(result.FullStream);
        Assert.Equal("output-text-delta", parts[0].Type);
        Assert.Equal("hello", await result.SourceText);
        Assert.Equal("hola", await result.TranslationText);
        Assert.Equal((double?)1.2, await result.DurationInSeconds);
        Assert.Equal((int?)5, (await result.Usage)!.TotalTokens);
        Assert.Equal("note", (await result.Warnings)[0].Message);
        Assert.True((await result.ProviderMetadata).GetProperty("p").GetBoolean());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass raw chunks through when includeRawChunks is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_raw_chunks_when_requested()
    {
        var raw = OperationJson.Parse("{\"chunk\":1}");
        var model = new TranslateFake { Parts = new[] { new ModelStreamPart { Type = "raw", Raw = raw }, new ModelStreamPart { Type = "finish", OutputText = "hola" } } };
        var request = Request(model);
        request.IncludeRawChunks = true;
        var result = StreamTranslate.Start(request);
        var parts = await Read(result.FullStream);
        Assert.Equal("raw", parts[0].Type);
        Assert.Equal(1, parts[0].Raw!.Value.GetProperty("chunk").GetInt32());
        Assert.True(model.Call!.IncludeRawChunks);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject final promises when no translation is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_when_no_translation_is_returned()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = Finish(null, null) }));
        await Assert.ThrowsAsync<NoTranslationGeneratedException>(async () => await Read(result.FullStream));
        await Assert.ThrowsAsync<NoTranslationGeneratedException>(() => result.TranslationText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep already-resolved promises resolved when the stream errors later", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_resolved_warnings_when_translation_fails()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake
        {
            Parts = new[] { new ModelStreamPart { Type = "stream-start", Warnings = new[] { OperationWarning.Other("test warning") } }, new ModelStreamPart { Type = "finish" } },
        }));
        await Assert.ThrowsAsync<NoTranslationGeneratedException>(async () => await Read(result.FullStream));
        Assert.Equal("test warning", (await result.Warnings)[0].Message);
        await Assert.ThrowsAsync<NoTranslationGeneratedException>(() => result.TranslationText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the audio stream when doStream rejects", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_audio_when_do_stream_rejects()
    {
        var audio = new AudioChunkStream();
        var result = StreamTranslate.Start(Request(new TranslateFake { SetupError = new InvalidOperationException("authentication failed") }, audio));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Read(result.FullStream));
        Assert.Equal("authentication failed", audio.CancelError!.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not interfere with a model-owned audio stream when the model stream errors mid-pipe", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_a_locked_audio_stream_alone()
    {
        var audio = new AudioChunkStream();
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = new[] { new ModelStreamPart { Type = "output-text-delta", Delta = "x" } }, Failure = new InvalidOperationException("mid"), LockAudio = true }, audio));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Read(result.FullStream));
        Assert.True(audio.Locked);
        Assert.Null(audio.CancelError);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the model stream when fullStream is cancelled early", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_model_stream_when_full_stream_is_cancelled()
    {
        var model = new TranslateFake { Parts = new[] { new ModelStreamPart { Type = "output-text-delta", Delta = "x" } }, Completes = false };
        var result = StreamTranslate.Start(Request(model));
        var stream = result.FullStream;
        await model.Started.Task;
        stream.Cancel();
        await Task.Delay(30);
        Assert.True(model.Stream!.Cancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should abort a still-pending doStream when fullStream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Aborts_a_pending_do_stream()
    {
        var model = new TranslateFake { HoldSetup = true };
        var result = StreamTranslate.Start(Request(model));
        await model.Started.Task;
        result.FullStream.Cancel();
        Assert.True(await model.SetupCancelled.Task);
        Assert.True(model.Call!.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve the result promises without consuming fullStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_promises_without_full_stream()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = Finish("hola", "hello") }));
        Assert.Equal("hola", await result.TranslationText);
        Assert.Equal("hello", await result.SourceText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject the result promises without consuming fullStream when no translation is produced", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_promises_without_full_stream()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = Finish(null, null) }));
        await Assert.ThrowsAsync<NoTranslationGeneratedException>(() => result.TranslationText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject fullStream access after a result promise claimed the stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_full_stream_after_a_result_promise()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = Finish("hola", "hello") }));
        Assert.Equal("hola", await result.TranslationText);
        var error = Assert.Throws<InvalidOperationException>(() => result.FullStream);
        Assert.Equal("fullStream cannot be accessed after a result promise.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support iterating fullStream before awaiting promises", Coverage = UpstreamCoverage.Covered)]
    public async Task Iterates_full_stream_before_promises()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = new[] { new ModelStreamPart { Type = "output-text-delta", Delta = "ho" }, new ModelStreamPart { Type = "finish", OutputText = "hola", SourceText = "hello" } } }));
        Assert.Equal("ho", (await Read(result.FullStream))[0].Delta);
        Assert.Equal("hola", await result.TranslationText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve a result promise while fullStream is actively consumed", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_translation_while_full_stream_is_consumed()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = new[] { new ModelStreamPart { Type = "output-text-delta", Delta = "ho" }, new ModelStreamPart { Type = "finish", OutputText = "hola" } } }));
        var reading = Read(result.FullStream);
        Assert.Equal("hola", await result.TranslationText);
        Assert.NotEmpty(await reading);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject a second fullStream access", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_second_full_stream_access()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = Finish("hola", "hello"), Completes = false }));
        _ = result.FullStream;
        var error = Assert.Throws<InvalidOperationException>(() => result.FullStream);
        Assert.Equal("fullStream can only be accessed once.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should succeed with an empty translationText for an audio-only stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Succeeds_with_empty_text_when_audio_was_produced()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake
        {
            Parts = new[] { new ModelStreamPart { Type = "audio", Audio = new byte[] { 1, 2, 3 } }, new ModelStreamPart { Type = "finish", OutputText = string.Empty } },
        }));
        var parts = await Read(result.FullStream);
        Assert.Equal("audio", parts[0].Type);
        Assert.Equal(string.Empty, await result.TranslationText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve sourceText to an empty string when only output text was produced", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_a_missing_source_text_to_empty()
    {
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = new[] { new ModelStreamPart { Type = "finish", OutputText = "hola", SourceText = null } } }));
        Assert.Equal(string.Empty, await result.SourceText);
        Assert.Equal("hola", await result.TranslationText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass error parts through on fullStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_error_parts_through()
    {
        var failure = new InvalidOperationException("provider");
        var result = StreamTranslate.Start(Request(new TranslateFake { Parts = new[] { new ModelStreamPart { Type = "error", Error = failure }, new ModelStreamPart { Type = "finish", OutputText = "hola" } } }));
        var parts = await Read(result.FullStream);
        Assert.Same(failure, parts[0].Error);
    }

    [Fact]
    [UpstreamTest(Prefix + "should error the stream when the external abort signal fires mid-stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_abort_reason_when_the_token_fires()
    {
        using var source = new CancellationTokenSource();
        var reason = new InvalidOperationException("Cancelled");
        var model = new TranslateFake { Parts = new[] { new ModelStreamPart { Type = "output-text-delta", Delta = "x" } }, Completes = false };
        var request = Request(model);
        request.CancellationToken = source.Token;
        request.AbortReason = reason;
        var result = StreamTranslate.Start(request);
        var stream = result.FullStream;
        await model.Started.Task;
        source.Cancel();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await Read(stream));
        Assert.Same(reason, error);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when a string model cannot be resolved", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_a_string_model_cannot_be_resolved()
    {
        var previous = SpeechTranslationModels.Default;
        SpeechTranslationModels.Default = null;
        try
        {
            var error = Assert.Throws<InvalidOperationException>(() => StreamTranslate.Start(new StreamTranslateRequest { Model = "gpt-translate", Audio = new AudioChunkStream(), InputAudioFormat = "pcm16", TargetLanguage = "es" }));
            Assert.Equal(StreamTranslate.MissingProviderMessage, error.Message);
        }
        finally
        {
            SpeechTranslationModels.Default = previous;
        }
    }

    private static ModelStreamPart[] Finish(string? output, string? source)
    {
        return new[] { new ModelStreamPart { Type = "finish", OutputText = output, SourceText = source } };
    }

    private static StreamTranslateRequest Request(TranslateFake model, AudioChunkStream? audio = null)
    {
        return new StreamTranslateRequest
        {
            Model = model,
            Audio = audio ?? new AudioChunkStream(),
            InputAudioFormat = "pcm16",
            TargetLanguage = "es",
        };
    }

    private static async Task<List<ModelStreamPart>> Read(IAsyncEnumerable<ModelStreamPart> stream)
    {
        var parts = new List<ModelStreamPart>();
        await foreach (var part in stream)
        {
            parts.Add(part);
        }

        return parts;
    }

    private sealed class TranslateFake : ISpeechTranslationCaller
    {
        public string Provider => "test-provider";

        public string ModelId => "test-model";

        public string SpecificationVersion => "v4";

        public TranslationStreamCall? Call { get; private set; }

        public ModelPartStream? Stream { get; private set; }

        public IReadOnlyList<ModelStreamPart> Parts { get; set; } = Array.Empty<ModelStreamPart>();

        public bool Completes { get; set; } = true;

        public Exception? Failure { get; set; }

        public Exception? SetupError { get; set; }

        public bool HoldSetup { get; set; }

        public bool LockAudio { get; set; }

        public TaskCompletionSource<bool> Started { get; } = new TaskCompletionSource<bool>();

        public TaskCompletionSource<bool> SetupCancelled { get; } = new TaskCompletionSource<bool>();

        public async Task<TranslationStreamStart> DoStreamAsync(TranslationStreamCall call, CancellationToken cancellationToken)
        {
            Call = call;
            Started.TrySetResult(true);
            if (LockAudio)
            {
                call.Audio.Lock();
            }

            if (SetupError != null)
            {
                throw SetupError;
            }

            if (HoldSetup)
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    SetupCancelled.TrySetResult(true);
                    throw;
                }
            }

            Stream = new ModelPartStream(Parts, Completes, Failure);
            return new TranslationStreamStart(Stream);
        }
    }
}
