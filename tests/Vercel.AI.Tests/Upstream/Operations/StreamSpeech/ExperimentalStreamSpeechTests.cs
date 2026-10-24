// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.StreamSpeech.StreamSpeechSupport;
using StreamSpeechOperation = Vercel.AI.Operations.StreamSpeech;

namespace Vercel.AI.Tests.Upstream.Operations.StreamSpeech;

/// <summary>Port of the <c>experimental_streamSpeech</c> describe of <c>stream-speech.test.ts</c>.</summary>
[Collection("ConsoleError")]
public sealed class ExperimentalStreamSpeechTests
{
    private const string Prefix = "packages/ai/src/generate-speech/stream-speech.test.ts::experimental_streamSpeech::";

    private static StreamSpeechResult Start(IStreamingSpeechCaller model, Action<StreamSpeechRequest>? configure = null)
    {
        var request = new StreamSpeechRequest { Model = model, Text = "Hi", OnError = _ => Task.CompletedTask };
        configure?.Invoke(request);
        return StreamSpeechOperation.Start(request);
    }

    private static MockStreamingSpeechModel Model(Func<SpeechModelStreamCall, Task<SpeechModelStreamResult>> doStream)
    {
        return new MockStreamingSpeechModel { DoStream = (call, _) => doStream(call) };
    }

    private static MockStreamingSpeechModel Basic()
    {
        return Model(_ => Task.FromResult(CreateStreamResponse(BasicParts())));
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the condition.");
            await Task.Delay(5);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should send args to doStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_args_to_doStream()
    {
        using var abort = new CancellationTokenSource();
        SpeechModelStreamCall? captured = null;
        var result = StreamSpeechOperation.Start(new StreamSpeechRequest
        {
            Model = Model(call =>
            {
                captured = call;
                return Task.FromResult(CreateStreamResponse(BasicParts()));
            }),
            Text = "Hello world",
            Voice = "voice-id",
            OutputFormat = new SpeechAudioFormat("audio/mpeg"),
            Instructions = "calm",
            Speed = 1.1,
            Language = "en",
            ProviderOptions = OperationJson.Parse("{\"mock\":{\"foo\":\"bar\"}}"),
            CancellationToken = abort.Token,
            Headers = new Dictionary<string, string> { ["custom-header"] = "value" },
            IncludeRawChunks = true,
        });

        await result.Usage;

        Assert.NotNull(captured);
        Assert.Equal("voice-id", captured!.Voice);
        Assert.Equal("audio/mpeg", captured.OutputFormat!.Type);
        Assert.Equal("calm", captured.Instructions);
        Assert.Equal(1.1, captured.Speed);
        Assert.Equal("en", captured.Language);
        Assert.Equal("bar", captured.ProviderOptions.GetProperty("mock").GetProperty("foo").GetString());
        Assert.Equal("value", captured.Headers["custom-header"]);
        Assert.Equal(AiSdkVersion.UserAgent, captured.Headers["user-agent"]);
        Assert.True(captured.IncludeRawChunks);
        Assert.True(captured.CancellationToken.CanBeCanceled);
    }

    [Theory]
    [InlineData("a string")]
    [InlineData("an async iterable")]
    [InlineData("a readable stream")]
    [InlineData("an array of strings and text parts")]
    [InlineData("a sync iterable")]
    [InlineData("an async iterable of strings and text parts")]
    [InlineData("a mix of strings and text parts")]
    [UpstreamTest(Prefix + "should pass %s as a text part stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_a_text_value_as_a_text_part_stream(string kind)
    {
        var bobTurn = OperationJson.Parse("{\"mock\":{\"newTurn\":true}}");
        var (configure, expected) = kind switch
        {
            "a string" => ((Action<StreamSpeechRequest>)(r => r.Text = "Hello world"), new[] { new SpeechTextPart("Hello world") }),
            "an async iterable" => (r => r.TextStream = TextStream("Hello ", "world"), new[] { new SpeechTextPart("Hello "), new SpeechTextPart("world") }),
            "a readable stream" => (r => r.TextStream = TextStream("Hello ", "world"), new[] { new SpeechTextPart("Hello "), new SpeechTextPart("world") }),
            "an array of strings and text parts" => (r => r.TextParts = new SpeechTextPart[] { "Hi. ", new SpeechTextPart("Hello. ", "bob") }, new[] { new SpeechTextPart("Hi. "), new SpeechTextPart("Hello. ", "bob") }),
            "a sync iterable" => (r => r.TextParts = new List<SpeechTextPart> { "Hello ", "world" }.AsEnumerable(), new[] { new SpeechTextPart("Hello "), new SpeechTextPart("world") }),
            "an async iterable of strings and text parts" => (r => r.TextStream = TextStream("Hi. ", new SpeechTextPart("Hello. ", "bob")), new[] { new SpeechTextPart("Hi. "), new SpeechTextPart("Hello. ", "bob") }),
            _ => (r => r.TextStream = TextStream("Hi. ", new SpeechTextPart("Hello. ", "bob", bobTurn)), new[] { new SpeechTextPart("Hi. "), new SpeechTextPart("Hello. ", "bob", bobTurn) }),
        };
        List<SpeechTextPart> textChunks = new List<SpeechTextPart>();

        var result = Start(Model(async call =>
        {
            textChunks = await ToListAsync(call.Text);
            return CreateStreamResponse(BasicParts());
        }), r =>
        {
            r.Text = null;
            configure(r);
        });

        await result.Usage;
        Assert.Equal(expected.Length, textChunks.Count);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Text, textChunks[i].Text);
            Assert.Equal(expected[i].Voice, textChunks[i].Voice);
            Assert.Equal(expected[i].ProviderOptions?.GetRawText(), textChunks[i].ProviderOptions?.GetRawText());
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream audio bytes through audioStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_audio_bytes_through_audioStream()
    {
        var result = Start(Basic());

        var chunks = await ToListAsync(result.AudioStream);

        Assert.Equal(2, chunks.Count);
        Assert.Equal(new byte[] { 1, 2 }, chunks[0]);
        Assert.Equal(new byte[] { 3, 4 }, chunks[1]);
        Assert.Equal(11, (await result.Usage).InputCharacters);
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose audio, raw and error parts through fullStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_audio_raw_and_error_parts_through_fullStream()
    {
        var recoverable = new Exception("recoverable");
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(
            StreamStart(),
            new SpeechModelStreamPart { Type = "raw", RawValue = OperationJson.Parse("{\"audio\":\"AQI=\"}") },
            new SpeechModelStreamPart { Type = "audio", Id = "a1", AudioBase64 = "AQI=", AudioFormat = new SpeechAudioFormat("audio/pcm", 24000), ProviderMetadata = OperationJson.Parse("{\"mock\":{\"alignment\":\"x\"}}") },
            Error(recoverable),
            Finish(new SpeechStreamUsage())))), r => r.IncludeRawChunks = true);

        var parts = await ToListAsync(result.FullStream);

        Assert.Equal(3, parts.Count);
        Assert.Equal("raw", parts[0].Type);
        Assert.Equal("{\"audio\":\"AQI=\"}", parts[0].RawValue!.Value.GetRawText());
        Assert.Equal("audio", parts[1].Type);
        Assert.Equal(new byte[] { 1, 2 }, parts[1].Audio);
        Assert.Equal("audio/pcm", parts[1].AudioFormat!.Type);
        Assert.Equal(24000, parts[1].AudioFormat!.Rate);
        Assert.Equal("a1", parts[1].Id);
        Assert.Equal("x", parts[1].ProviderMetadata!.Value.GetProperty("mock").GetProperty("alignment").GetString());
        Assert.Equal("error", parts[2].Type);
        Assert.Same(recoverable, parts[2].Error);
    }

    [Fact]
    [UpstreamTest(Prefix + "should drop empty audio chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_drop_empty_audio_chunks()
    {
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(StreamStart(), AudioBase64(""), Audio(9), Finish()))));

        var chunks = await ToListAsync(result.AudioStream);

        Assert.Single(chunks);
        Assert.Equal(new byte[] { 9 }, chunks[0]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep empty audio parts that carry provider metadata in fullStream only", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_empty_audio_parts_that_carry_provider_metadata_in_fullStream_only()
    {
        var marker = OperationJson.Parse("{\"mock\":{\"isFinalAudioForTurn\":true}}");
        var parts = new[]
        {
            StreamStart(),
            Audio(9),
            new SpeechModelStreamPart { Type = "audio", AudioBase64 = "", AudioFormat = Mp3, ProviderMetadata = marker },
            Finish(),
        };
        MockStreamingSpeechModel NewModel() => Model(_ => Task.FromResult(CreateStreamResponse(parts)));

        var full = await ToListAsync(Start(NewModel()).FullStream);

        var audioParts = full.Where(part => part.Type == "audio").ToList();
        Assert.Equal(2, audioParts.Count);
        Assert.Equal(new byte[] { 9 }, audioParts[0].Audio);
        Assert.Null(audioParts[0].ProviderMetadata);
        Assert.Empty(audioParts[1].Audio);
        Assert.Equal("audio/mpeg", audioParts[1].AudioFormat!.Type);
        Assert.Equal(marker.GetRawText(), audioParts[1].ProviderMetadata!.Value.GetRawText());
        var audio = await ToListAsync(Start(NewModel()).AudioStream);
        Assert.Single(audio);
        Assert.Equal(new byte[] { 9 }, audio[0]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not count metadata-only audio parts as audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_count_metadata_only_audio_parts_as_audio()
    {
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(
            StreamStart(),
            new SpeechModelStreamPart { Type = "audio", AudioBase64 = "", AudioFormat = Mp3, ProviderMetadata = OperationJson.Parse("{\"mock\":{\"marker\":true}}") },
            Finish()))));

        await Assert.ThrowsAsync<NoSpeechGeneratedException>(() => result.Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve result promises", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_result_promises()
    {
        var logged = new List<WarningLogContext>();
        var previous = WarningLog.Observer;
        WarningLog.Observer = logged.Add;
        try
        {
            var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(
                new SpeechModelStreamPart { Type = "stream-start", Warnings = new[] { OperationWarning.Other("warning") } },
                new SpeechModelStreamPart { Type = "response-metadata", ModelId = "resolved-model", Timestamp = new DateTime(2024, 1, 2) },
                Audio(1),
                Finish(new SpeechStreamUsage { InputCharacters = 2 }, OperationJson.Parse("{\"mock\":{\"requestId\":\"r1\"}}"))))));

            var warnings = await result.Warnings;
            Assert.Single(warnings);
            Assert.Equal("other", warnings[0].Type);
            Assert.Equal("warning", warnings[0].Message);
            Assert.Equal(2, (await result.Usage).InputCharacters);
            Assert.Equal("{\"mock\":{\"requestId\":\"r1\"}}", (await result.ProviderMetadata).GetRawText());
            var responses = await result.Responses;
            Assert.Single(responses);
            Assert.Equal(new DateTime(2024, 1, 2), responses[0].Timestamp);
            Assert.Equal("resolved-model", responses[0].ModelId);
            Assert.Equal("value", responses[0].Headers!["x-test"]);
            Assert.Contains(logged, context => context.Provider == "mock-provider" && context.Model == "mock-model-id" && context.Warnings.Count == 1 && context.Warnings[0].Message == "warning");
        }
        finally
        {
            WarningLog.Observer = previous;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject when the stream finishes without audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_when_the_stream_finishes_without_audio()
    {
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(StreamStart(), Finish()))));

        await Assert.ThrowsAsync<NoSpeechGeneratedException>(() => result.Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject with no-speech when the stream ends without audio or a finish part", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_with_no_speech_when_the_stream_ends_without_audio_or_a_finish_part()
    {
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(StreamStart()))));

        await Assert.ThrowsAsync<NoSpeechGeneratedException>(() => ToListAsync(result.AudioStream));
    }

    [Fact]
    [UpstreamTest(Prefix + "should skip error parts in audioStream and report them through onError", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_skip_error_parts_in_audioStream_and_report_them_through_onError()
    {
        var recoverable = new Exception("recoverable");
        var reported = new TaskCompletionSource<Exception>();
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(
            StreamStart(),
            Audio(1),
            Error(recoverable),
            Audio(2),
            Finish(new SpeechStreamUsage { InputCharacters = 2 })))), r => r.OnError = error =>
            {
                reported.TrySetResult(error);
                return Task.CompletedTask;
            });

        var chunks = await ToListAsync(result.AudioStream);

        Assert.Equal(2, chunks.Count);
        Assert.Equal(new byte[] { 1 }, chunks[0]);
        Assert.Equal(new byte[] { 2 }, chunks[1]);
        Assert.Equal(2, (await result.Usage).InputCharacters);
        Assert.Same(recoverable, await reported.Task.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should log error parts with console.error by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_log_error_parts_with_console_error_by_default()
    {
        var original = Console.Error;
        var written = new StringWriter();
        Console.SetError(written);
        try
        {
            var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(new[] { StreamStart(), Error(new Exception("recoverable")) }.Concat(BasicParts().Skip(1)).ToArray()))), r => r.OnError = null);
            await result.Usage;
            await WaitUntil(() => written.ToString().Contains("recoverable"));
        }
        finally
        {
            Console.SetError(original);
        }
    }

    [Theory]
    [InlineData("throws")]
    [InlineData("rejects")]
    [UpstreamTest(Prefix + "should keep streaming when onError %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_streaming_when_onError_throws_or_rejects(string kind)
    {
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(new[] { StreamStart(), Error(new Exception("recoverable")) }.Concat(BasicParts().Skip(1)).ToArray()))), r => r.OnError = _ =>
            kind == "throws" ? throw new Exception("handler failed") : Task.FromException(new Exception("handler failed")));

        Assert.Equal(2, (await ToListAsync(result.AudioStream)).Count);
        Assert.Equal(11, (await result.Usage).InputCharacters);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject result promises and error the output on a user abort while doStream is pending", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_result_promises_and_error_the_output_on_a_user_abort_while_doStream_is_pending()
    {
        using var abort = new CancellationTokenSource();
        // A provider that ignores the abort token.
        var result = Start(new MockStreamingSpeechModel { DoStream = (_, _) => new TaskCompletionSource<SpeechModelStreamResult>().Task }, r =>
        {
            r.CancellationToken = abort.Token;
            r.AbortReason = new Exception("user abort");
        });
        await using var reader = result.FullStream.GetAsyncEnumerator();
        var usage = result.Usage;
        var read = reader.MoveNextAsync().AsTask();
        abort.Cancel();

        Assert.Equal("user abort", (await Assert.ThrowsAsync<Exception>(() => usage)).Message);
        Assert.Equal("user abort", (await Assert.ThrowsAsync<Exception>(() => read)).Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject a text value that is not a string, iterable, or stream", Coverage = UpstreamCoverage.Partial, Note = "The C# request types the text, so only a missing text value is rejected.")]
    public void Should_reject_a_text_value_that_is_not_a_string_iterable_or_stream()
    {
        var error = Assert.Throws<InvalidArgumentException>(() => StreamSpeechOperation.Start(new StreamSpeechRequest { Model = Basic() }));

        Assert.Contains("Invalid argument for parameter text", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept a readonly array of text parts directly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_accept_a_readonly_array_of_text_parts_directly()
    {
        var script = new[] { new SpeechTextPart("Did you hear? ", "alice"), new SpeechTextPart("No, what?", "bob") };
        List<SpeechTextPart> textChunks = new List<SpeechTextPart>();

        var result = Start(Model(async call =>
        {
            textChunks = await ToListAsync(call.Text);
            return CreateStreamResponse(BasicParts());
        }), r =>
        {
            r.Text = null;
            r.TextParts = script;
        });

        await result.Usage;
        Assert.Equal(script, textChunks);
    }

    [Fact]
    [UpstreamTest(Prefix + "should surface the provider error when the stream ends after audio without finishing", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_surface_the_provider_error_when_the_stream_ends_after_audio_without_finishing()
    {
        var upstream = new Exception("upstream failed");
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(StreamStart(), Audio(1), Error(upstream)))));

        var error = await Assert.ThrowsAsync<Exception>(() => ToListAsync(result.AudioStream));

        Assert.Same(upstream, error);
    }

    [Fact]
    [UpstreamTest(Prefix + "should fail with a stream error, not no-speech, when audio was streamed but no finish arrived", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_fail_with_a_stream_error_not_no_speech_when_audio_was_streamed_but_no_finish_arrived()
    {
        var result = Start(Model(_ => Task.FromResult(CreateStreamResponse(StreamStart(), Audio(1)))));

        var error = await Assert.ThrowsAsync<StreamProviderError>(() => result.Usage);

        Assert.Equal("The speech stream ended before it finished.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not throw when breaking out of fullStream early", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_throw_when_breaking_out_of_fullStream_early()
    {
        var result = Start(Model(_ => Task.FromResult(new SpeechModelStreamResult(OpenStream(() => { }, StreamStart(), Audio(1))))));

        await foreach (var part in result.FullStream)
        {
            Assert.Equal("audio", part.Type);
            break;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject result promises and cancel the text when doStream rejects", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_result_promises_and_cancel_the_text_when_doStream_rejects()
    {
        var text = new TextProbe();

        var result = Start(Model(_ => throw new Exception("connect failed")), r =>
        {
            r.Text = null;
            r.TextStream = text;
        });

        Assert.Equal("connect failed", (await Assert.ThrowsAsync<Exception>(() => result.Usage)).Message);
        await WaitUntil(() => text.Cancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should abort the model call when fullStream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_abort_the_model_call_when_fullStream_is_cancelled()
    {
        CancellationToken signal = default;
        var cancelModelStream = false;
        var result = Start(Model(call =>
        {
            signal = call.CancellationToken;
            return Task.FromResult(new SpeechModelStreamResult(OpenStream(() => cancelModelStream = true, StreamStart(), Audio(1))));
        }));

        var reader = result.FullStream.GetAsyncEnumerator();
        await reader.MoveNextAsync();
        await reader.DisposeAsync();

        await WaitUntil(() => signal.IsCancellationRequested);
        await WaitUntil(() => cancelModelStream);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve the response body from doStream and response-metadata parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_the_response_body_from_doStream_and_response_metadata_parts()
    {
        var fromDoStream = Start(Model(_ => Task.FromResult(new SpeechModelStreamResult(
            Enumerate(BasicParts()),
            new SpeechModelStreamResponse { ModelId = "test-model-id", Body = OperationJson.Parse("{\"setup\":true}") }))));
        Assert.Equal("{\"setup\":true}", (await fromDoStream.Responses)[0].Body!.Value.GetRawText());

        var fromPart = Start(Model(_ => Task.FromResult(CreateStreamResponse(new[]
        {
            StreamStart(),
            new SpeechModelStreamPart { Type = "response-metadata", Body = OperationJson.Parse("{\"final\":true}") },
        }.Concat(BasicParts().Skip(1)).ToArray()))));
        Assert.Equal("{\"final\":true}", (await fromPart.Responses)[0].Body!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep a preceding error part as the cause of a no-audio failure", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_a_preceding_error_part_as_the_cause_of_a_no_audio_failure()
    {
        var upstream = new Exception("voice not found");
        var finishing = Start(Model(_ => Task.FromResult(CreateStreamResponse(StreamStart(), Error(upstream), Finish()))));
        var finishingError = await Assert.ThrowsAsync<NoSpeechGeneratedException>(() => finishing.Usage);
        Assert.Same(upstream, finishingError.InnerException);

        var ending = Start(Model(_ => Task.FromResult(CreateStreamResponse(StreamStart(), Error(upstream)))));
        var endingError = await Assert.ThrowsAsync<NoSpeechGeneratedException>(() => ending.Usage);
        Assert.Same(upstream, endingError.InnerException);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the model stream without unhandled rejections when audioStream is cancelled early", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_cancel_the_model_stream_without_unhandled_rejections_when_audioStream_is_cancelled_early()
    {
        var cancelModelStream = false;
        var unhandled = new List<Exception>();
        EventHandler<UnobservedTaskExceptionEventArgs> handler = (_, args) => unhandled.Add(args.Exception);
        TaskScheduler.UnobservedTaskException += handler;
        try
        {
            var result = Start(Model(_ => Task.FromResult(new SpeechModelStreamResult(OpenStream(() => cancelModelStream = true, StreamStart(), Audio(1))))));

            await foreach (var chunk in result.AudioStream)
            {
                Assert.Equal(new byte[] { 1 }, chunk);
                break;
            }

            await WaitUntil(() => cancelModelStream);
            await Task.Delay(10);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            // The handler is process-wide, so ignore faults that other tests leave for the finalizer.
            Assert.DoesNotContain(unhandled, error => error.ToString().Contains("StreamSpeech", StringComparison.Ordinal));
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= handler;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should allow only one stream owner", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_only_one_stream_owner()
    {
        var result = Start(Basic());

        _ = result.FullStream;

        var error = Assert.Throws<InvalidOperationException>(() => result.AudioStream);
        Assert.Equal("Only one of audioStream or fullStream can be accessed, and only once.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not allow stream access after a result promise", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_allow_stream_access_after_a_result_promise()
    {
        var result = Start(Basic());

        _ = result.Usage;

        var error = Assert.Throws<InvalidOperationException>(() => result.FullStream);
        Assert.Equal("fullStream cannot be accessed after a result promise.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when the model does not support streaming", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_when_the_model_does_not_support_streaming()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => StreamSpeechOperation.Start(new StreamSpeechRequest { Model = new MockSpeechModel(), Text = "Hi" }));
    }
}
