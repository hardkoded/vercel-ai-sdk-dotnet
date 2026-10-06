// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for transcription.</summary>
[Collection("WarningLog")]
public sealed class TranscribeTests
{
    private const string Stream = "packages/ai/src/transcribe/stream-transcribe.test.ts::experimental_streamTranscribe::";
    private const string Once = "packages/ai/src/transcribe/transcribe.test.ts::transcribe::";
    private const string Error = "packages/ai/src/transcribe/transcribe.test.ts::transcribe > error handling::";

    [Fact]
    [UpstreamTest(Stream + "should send args to doStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_arguments_to_do_stream()
    {
        var audio = new AudioChunkStream();
        var model = new TranscriptFake { Parts = Finish("hello") };
        var options = OperationJson.Parse("{\"p\":true}");
        var result = StreamTranscribe.Start(new StreamTranscribeRequest
        {
            Model = model,
            Audio = audio,
            InputAudioFormat = "pcm16",
            IncludeRawChunks = true,
            ProviderOptions = options,
            Headers = new Dictionary<string, string> { ["X-Test"] = "1" },
        });
        await model.Started.Task;
        Assert.Same(audio, model.Call!.Audio);
        Assert.Equal("pcm16", model.Call.InputAudioFormat);
        Assert.True(model.Call.IncludeRawChunks);
        Assert.True(model.Call.ProviderOptions.GetProperty("p").GetBoolean());
        Assert.Equal("1", model.Call.Headers["x-test"]);
        Assert.Equal(AiSdkVersion.UserAgent, model.Call.Headers["user-agent"]);
        Assert.Equal("hello", await result.Text);
    }

    [Fact]
    [UpstreamTest(Stream + "should stream transcript parts and resolve final metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_transcript_parts_and_resolves_metadata()
    {
        var segment = new TranscriptSegment("hello", 0, 1);
        var model = new TranscriptFake
        {
            Parts = new[]
            {
                new ModelStreamPart { Type = "stream-start", Warnings = new[] { OperationWarning.Other("note") } },
                new ModelStreamPart { Type = "transcript-delta", Delta = "hel" },
                new ModelStreamPart { Type = "finish", Text = "hello", Segments = new[] { segment }, Language = "en", DurationInSeconds = 1.5, ProviderMetadata = OperationJson.Parse("{\"p\":true}") },
            },
        };
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = model, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        var parts = await Read(result.FullStream);
        Assert.Equal("transcript-delta", parts[0].Type);
        Assert.Equal("hello", await result.Text);
        Assert.Equal("en", await result.Language);
        Assert.Equal((double?)1.5, await result.DurationInSeconds);
        Assert.Equal("note", (await result.Warnings)[0].Message);
        Assert.True((await result.ProviderMetadata).GetProperty("p").GetBoolean());
    }

    [Fact]
    [UpstreamTest(Stream + "should throw UnsupportedFunctionalityError when doStream is unavailable", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_streaming_is_unavailable()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { CanStream = false }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" }));
        Assert.Equal("streaming transcription", error.Functionality);
        Assert.Contains("does not support streaming transcription", error.Message);
        Assert.DoesNotContain("AI Gateway", error.Message);
    }

    [Fact]
    [UpstreamTest(Stream + "should reject final promises when no transcript is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_when_no_transcript_is_returned()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish(string.Empty) }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        await Assert.ThrowsAsync<NoTranscriptGeneratedException>(async () => await Read(result.FullStream));
        await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() => result.Text);
    }

    [Fact]
    [UpstreamTest(Stream + "should keep already-resolved promises resolved when the stream errors later", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_resolved_warnings_when_the_transcript_is_empty()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest
        {
            Model = new TranscriptFake { Parts = new[] { new ModelStreamPart { Type = "stream-start", Warnings = new[] { OperationWarning.Other("test warning") } }, new ModelStreamPart { Type = "finish", Text = string.Empty } } },
            Audio = new AudioChunkStream(),
            InputAudioFormat = "pcm16",
        });
        await Assert.ThrowsAsync<NoTranscriptGeneratedException>(async () => await Read(result.FullStream));
        Assert.Equal("test warning", (await result.Warnings)[0].Message);
        await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() => result.Text);
    }

    [Fact]
    [UpstreamTest(Stream + "should cancel the audio stream when doStream rejects", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_audio_when_do_stream_rejects()
    {
        var audio = new AudioChunkStream();
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { SetupError = new InvalidOperationException("authentication failed") }, Audio = audio, InputAudioFormat = "pcm16" });
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await Read(result.FullStream));
        Assert.Equal("authentication failed", error.Message);
        await Assert.ThrowsAsync<InvalidOperationException>(() => result.Text);
        Assert.Equal("authentication failed", audio.CancelError!.Message);
    }

    [Fact]
    [UpstreamTest(Stream + "should not interfere with a model-owned audio stream when the model stream errors mid-pipe", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_a_locked_audio_stream_alone()
    {
        var audio = new AudioChunkStream();
        var model = new TranscriptFake { Parts = new[] { new ModelStreamPart { Type = "transcript-delta", Delta = "x" } }, Failure = new InvalidOperationException("mid"), LockAudio = true };
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = model, Audio = audio, InputAudioFormat = "pcm16" });
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Read(result.FullStream));
        Assert.True(audio.Locked);
        Assert.Null(audio.CancelError);
    }

    [Fact]
    [UpstreamTest(Stream + "should cancel the model stream when fullStream is cancelled early", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_model_stream_when_full_stream_is_cancelled()
    {
        var model = new TranscriptFake { Parts = new[] { new ModelStreamPart { Type = "transcript-delta", Delta = "x" } }, Completes = false };
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = model, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        var stream = result.FullStream;
        await model.Started.Task;
        stream.Cancel();
        await Task.Delay(30);
        Assert.True(model.Stream!.Cancelled);
    }

    [Fact]
    [UpstreamTest(Stream + "should abort a still-pending doStream when fullStream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Aborts_a_pending_do_stream_when_full_stream_is_cancelled()
    {
        var model = new TranscriptFake { HoldSetup = true };
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = model, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        await model.Started.Task;
        result.FullStream.Cancel();
        Assert.True(await model.SetupCancelled.Task);
        Assert.True(model.Call!.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Stream + "should resolve the result promises without consuming fullStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_promises_without_full_stream()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish("hello") }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        Assert.Equal("hello", await result.Text);
        Assert.NotNull(await result.Responses);
    }

    [Fact]
    [UpstreamTest(Stream + "should reject the result promises without consuming fullStream when no transcript is produced", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_promises_without_full_stream()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish(string.Empty) }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() => result.Text);
    }

    [Fact]
    [UpstreamTest(Stream + "should reject fullStream access after a result promise claimed the stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_full_stream_after_a_result_promise()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish("hello") }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        Assert.Equal("hello", await result.Text);
        var error = Assert.Throws<InvalidOperationException>(() => result.FullStream);
        Assert.Equal("fullStream cannot be accessed after a result promise.", error.Message);
    }

    [Fact]
    [UpstreamTest(Stream + "should support iterating fullStream before awaiting promises", Coverage = UpstreamCoverage.Covered)]
    public async Task Iterates_full_stream_before_promises()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = new[] { new ModelStreamPart { Type = "transcript-delta", Delta = "hi" }, new ModelStreamPart { Type = "finish", Text = "hi" } } }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        var parts = await Read(result.FullStream);
        Assert.Equal("hi", parts[0].Delta);
        Assert.Equal("hi", await result.Text);
    }

    [Fact]
    [UpstreamTest(Stream + "should resolve a result promise while fullStream is actively consumed", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_text_while_full_stream_is_consumed()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = new[] { new ModelStreamPart { Type = "transcript-delta", Delta = "hi" }, new ModelStreamPart { Type = "finish", Text = "hi" } } }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        var reading = Read(result.FullStream);
        Assert.Equal("hi", await result.Text);
        Assert.NotEmpty(await reading);
    }

    [Fact]
    [UpstreamTest(Stream + "should reject a second fullStream access", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_second_full_stream_access()
    {
        var result = StreamTranscribe.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish("hello"), Completes = false }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        _ = result.FullStream;
        var error = Assert.Throws<InvalidOperationException>(() => result.FullStream);
        Assert.Equal("fullStream can only be accessed once.", error.Message);
    }

    [Fact]
    [UpstreamTest(Once + "should send args to doGenerate", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_arguments_to_do_generate()
    {
        var audio = new byte[] { 1, 2, 3, 4 };
        var model = new TranscriptFake();
        await Transcribe.TranscribeAsync(new TranscribeRequest { Model = model, Audio = audio, ProviderOptions = OperationJson.Parse("{\"p\":true}"), Headers = new Dictionary<string, string> { ["X-Test"] = "1" } });
        Assert.Equal(audio, model.Generated!.Audio);
        Assert.Equal("audio/wav", model.Generated.MediaType);
        Assert.True(model.Generated.ProviderOptions.GetProperty("p").GetBoolean());
        Assert.Equal("1", model.Generated.Headers["x-test"]);
        Assert.Equal(AiSdkVersion.UserAgent, model.Generated.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Once + "should detect MP4 audio with an ftyp box", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_mp4_audio()
    {
        var model = new TranscriptFake();
        await Transcribe.TranscribeAsync(new TranscribeRequest { Model = model, Audio = new byte[] { 0, 0, 0, 0x18, 0x66, 0x74, 0x79, 0x70, 0, 0, 0, 0 } });
        Assert.Equal("audio/mp4", model.Generated!.MediaType);
    }

    [Fact]
    [UpstreamTest(Once + "should detect ADTS AAC audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_adts_aac_audio()
    {
        var model = new TranscriptFake();
        await Transcribe.TranscribeAsync(new TranscribeRequest { Model = model, Audio = new byte[] { 0xFF, 0xF1, 0x50, 0x80 } });
        Assert.Equal("audio/aac", model.Generated!.MediaType);
    }

    [Fact]
    [UpstreamTest(Once + "should return warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_warnings()
    {
        var result = await Transcribe.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Warnings = new[] { OperationWarning.Other("note") } }, Audio = new byte[] { 1 } });
        Assert.Equal("note", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Once + "should call logWarnings with the correct warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_transcript_warnings()
    {
        var seen = Watch();
        try
        {
            await Transcribe.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Provider = "speech", ModelId = "whisper", Warnings = new[] { OperationWarning.Other("note") } }, Audio = new byte[] { 1 } });
            Assert.Equal("note", seen[0].Warnings[0].Message);
            Assert.Equal("speech", seen[0].Provider);
            Assert.Equal("whisper", seen[0].Model);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Once + "should call logWarnings with empty array when no warnings are present", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_an_empty_transcript_warning_list()
    {
        var seen = Watch();
        try
        {
            await Transcribe.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake(), Audio = new byte[] { 1 } });
            Assert.Empty(seen[0].Warnings);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Once + "should return the transcript", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_transcript()
    {
        var result = await Transcribe.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Text = "hello world", Language = "en", Duration = 2 }, Audio = new byte[] { 1 } });
        Assert.Equal("hello world", result.Text);
        Assert.Equal("en", result.Language);
        Assert.Equal((double?)2, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(Error + "should throw NoTranscriptGeneratedError when no transcript is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_no_transcript_is_returned()
    {
        var error = await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() => Transcribe.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Text = string.Empty }, Audio = new byte[] { 1 } }));
        Assert.Equal("No transcript generated.", error.Message);
    }

    [Fact]
    [UpstreamTest(Error + "should include response headers in error when no transcript generated", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_response_headers_when_no_transcript_is_generated()
    {
        var model = new TranscriptFake { Text = string.Empty, Response = new ProviderResponse(new Dictionary<string, string> { ["x-request-id"] = "req" }) };
        var error = await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() => Transcribe.TranscribeAsync(new TranscribeRequest { Model = model, Audio = new byte[] { 1 } }));
        Assert.Equal("req", error.Responses[0].Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Once + "should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_response_metadata()
    {
        var response = new ProviderResponse(id: "response", modelId: "whisper", body: OperationJson.Parse("{\"raw\":true}"));
        var result = await Transcribe.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Response = response, Metadata = OperationJson.Parse("{\"p\":1}") }, Audio = new byte[] { 1 } });
        Assert.Equal("response", result.Responses[0].Id);
        Assert.Equal(1, result.ProviderMetadata.GetProperty("p").GetInt32());
    }

    private static ModelStreamPart[] Finish(string text)
    {
        return new[] { new ModelStreamPart { Type = "finish", Text = text } };
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

    private static List<WarningLogContext> Watch()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = seen.Add;
        return seen;
    }

    private sealed class TranscriptFake : ITranscriptionCaller
    {
        public string Provider { get; set; } = "test-provider";

        public string ModelId { get; set; } = "test-model";

        public string SpecificationVersion => "v4";

        public bool CanStream { get; set; } = true;

        public string Text { get; set; } = "hello";

        public string? Language { get; set; }

        public double? Duration { get; set; }

        public IReadOnlyList<OperationWarning>? Warnings { get; set; }

        public JsonElement? Metadata { get; set; }

        public ProviderResponse? Response { get; set; }

        public TranscriptionModelCall? Generated { get; private set; }

        public TranscriptionStreamCall? Call { get; private set; }

        public ModelPartStream? Stream { get; private set; }

        public IReadOnlyList<ModelStreamPart> Parts { get; set; } = Array.Empty<ModelStreamPart>();

        public bool Completes { get; set; } = true;

        public Exception? Failure { get; set; }

        public Exception? SetupError { get; set; }

        public bool HoldSetup { get; set; }

        public bool LockAudio { get; set; }

        public TaskCompletionSource<bool> Started { get; } = new TaskCompletionSource<bool>();

        public TaskCompletionSource<bool> SetupCancelled { get; } = new TaskCompletionSource<bool>();

        public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
        {
            Generated = call;
            return Task.FromResult(new TranscriptionModelResult(Text, language: Language, durationInSeconds: Duration, warnings: Warnings, providerMetadata: Metadata, response: Response));
        }

        public async Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
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
            return new TranscriptionStreamStart(Stream);
        }
    }
}
