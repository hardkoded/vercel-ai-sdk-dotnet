// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.Transcribe.TranscribeSupport;
using StreamTranscribeOperation = Vercel.AI.Operations.StreamTranscribe;
using TranscribeOperation = Vercel.AI.Operations.Transcribe;

namespace Vercel.AI.Tests.Upstream.Operations.StreamTranscribe;

/// <summary>Port of <c>stream-transcribe.test.ts</c> &gt; <c>experimental_streamTranscribe</c>.</summary>
[Collection("LogWarnings")]
public sealed class ExperimentalStreamTranscribeTests
{
    private const string Stream = "packages/ai/src/transcribe/stream-transcribe.test.ts::experimental_streamTranscribe::";
    [Fact]
    [UpstreamTest(Stream + "should send args to doStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_arguments_to_do_stream()
    {
        var audio = new AudioChunkStream();
        var model = new TranscriptFake { Parts = Finish("hello") };
        var options = OperationJson.Parse("{\"p\":true}");
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest
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
        Assert.Equal("pcm16", model.Call.InputAudioFormat.Type);
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
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = model, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
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
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { CanStream = false }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" }));
        Assert.Equal("streaming transcription", error.Functionality);
        Assert.Contains("does not support streaming transcription", error.Message);
        Assert.DoesNotContain("AI Gateway", error.Message);
    }

    [Fact]
    [UpstreamTest(Stream + "should reject final promises when no transcript is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_when_no_transcript_is_returned()
    {
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish(string.Empty) }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        await Assert.ThrowsAsync<NoTranscriptGeneratedException>(async () => await Read(result.FullStream));
        await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() => result.Text);
    }

    [Fact]
    [UpstreamTest(Stream + "should keep already-resolved promises resolved when the stream errors later", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_resolved_warnings_when_the_transcript_is_empty()
    {
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest
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
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { SetupError = new InvalidOperationException("authentication failed") }, Audio = audio, InputAudioFormat = "pcm16" });
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
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = model, Audio = audio, InputAudioFormat = "pcm16" });
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Read(result.FullStream));
        Assert.True(audio.Locked);
        Assert.Null(audio.CancelError);
    }

    [Fact]
    [UpstreamTest(Stream + "should cancel the model stream when fullStream is cancelled early", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_model_stream_when_full_stream_is_cancelled()
    {
        var model = new TranscriptFake { Parts = new[] { new ModelStreamPart { Type = "transcript-delta", Delta = "x" } }, Completes = false };
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = model, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        await foreach (var part in result.FullStream)
        {
            Assert.Equal("transcript-delta", part.Type);
            break;
        }

        // The pump cancels the model stream before it rejects the result promises.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result.Text);
        Assert.True(model.Stream!.Cancelled);
    }

    [Fact]
    [UpstreamTest(Stream + "should abort a still-pending doStream when fullStream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Aborts_a_pending_do_stream_when_full_stream_is_cancelled()
    {
        var model = new TranscriptFake { HoldSetup = true };
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = model, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        await model.Started.Task;
        result.FullStream.Cancel();
        Assert.True(await model.SetupCancelled.Task);
        Assert.True(model.Call!.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Stream + "should resolve the result promises without consuming fullStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_promises_without_full_stream()
    {
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish("hello") }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        Assert.Equal("hello", await result.Text);
        Assert.NotNull(await result.Responses);
    }

    [Fact]
    [UpstreamTest(Stream + "should reject the result promises without consuming fullStream when no transcript is produced", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_promises_without_full_stream()
    {
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish(string.Empty) }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() => result.Text);
    }

    [Fact]
    [UpstreamTest(Stream + "should reject fullStream access after a result promise claimed the stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_full_stream_after_a_result_promise()
    {
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish("hello") }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        Assert.Equal("hello", await result.Text);
        var error = Assert.Throws<InvalidOperationException>(() => result.FullStream);
        Assert.Equal("fullStream cannot be accessed after a result promise.", error.Message);
    }

    [Fact]
    [UpstreamTest(Stream + "should support iterating fullStream before awaiting promises", Coverage = UpstreamCoverage.Covered)]
    public async Task Iterates_full_stream_before_promises()
    {
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = new[] { new ModelStreamPart { Type = "transcript-delta", Delta = "hi" }, new ModelStreamPart { Type = "finish", Text = "hi" } } }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        var parts = await Read(result.FullStream);
        Assert.Equal("hi", parts[0].Delta);
        Assert.Equal("hi", await result.Text);
    }

    [Fact]
    [UpstreamTest(Stream + "should resolve a result promise while fullStream is actively consumed", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_text_while_full_stream_is_consumed()
    {
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = new[] { new ModelStreamPart { Type = "transcript-delta", Delta = "hi" }, new ModelStreamPart { Type = "finish", Text = "hi" } } }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        var reading = Read(result.FullStream);
        Assert.Equal("hi", await result.Text);
        Assert.NotEmpty(await reading);
    }

    [Fact]
    [UpstreamTest(Stream + "should reject a second fullStream access", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_second_full_stream_access()
    {
        var result = StreamTranscribeOperation.Start(new StreamTranscribeRequest { Model = new TranscriptFake { Parts = Finish("hello"), Completes = false }, Audio = new AudioChunkStream(), InputAudioFormat = "pcm16" });
        _ = result.FullStream;
        var error = Assert.Throws<InvalidOperationException>(() => result.FullStream);
        Assert.Equal("fullStream can only be accessed once.", error.Message);
    }
}
