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

namespace Vercel.AI.Tests.Upstream.Operations.Transcribe;

/// <summary>Port of <c>transcribe.test.ts</c> &gt; <c>transcribe</c>.</summary>
[Collection("LogWarnings")]
public sealed class TranscribeTests
{
    private const string Once = "packages/ai/src/transcribe/transcribe.test.ts::transcribe::";
    private const string NativeUrlCommit = "36802d48afa7132420acddaeae3f601b2fccb7f1";
    [Theory]
    [InlineData("http:")]
    [InlineData("https:")]
    [UpstreamTest(Once + "passes %s audio URLs directly to capable models with accurate telemetry", Coverage = UpstreamCoverage.Partial, Commit = NativeUrlCommit, Note = "The port has no transcribe telemetry events, so the start/end audio metadata assertions are not ported.")]
    public async Task Partial_Passes_audio_URLs_directly_to_capable_models(string protocol)
    {
        var url = new Uri(protocol + "//example.com/large-audio.mp3");
        var model = new TranscriptFake { FromUrl = _ => Task.FromResult<TranscriptionModelResult?>(new TranscriptionModelResult("hello")) };
        using var cancellation = new CancellationTokenSource();
        var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest
        {
            Model = model,
            AudioUrl = url.AbsoluteUri,
            MaxRetries = 0,
            CancellationToken = cancellation.Token,
            Headers = new Dictionary<string, string> { ["custom-header"] = "value" },
            ProviderOptions = OperationJson.Parse("{\"groq\":{\"language\":\"en\"}}"),
            TimestampGranularities = new[] { "word", "segment" },
        });
        Assert.Equal("hello", result.Text);
        Assert.Equal(0, model.GenerateCalls);
        var call = model.UrlGenerated!;
        Assert.Equal(url, call.Audio);
        Assert.Equal(cancellation.Token, call.CancellationToken);
        Assert.Equal("value", call.Headers["custom-header"]);
        Assert.Equal("en", call.ProviderOptions.GetProperty("groq").GetProperty("language").GetString());
        Assert.Equal(new[] { "word", "segment" }, call.TimestampGranularities);
    }

    [Fact]
    [UpstreamTest(Once + "downloads locally when native URL transcription is declined and retries only the file call", Coverage = UpstreamCoverage.Partial, Commit = NativeUrlCommit, Note = "The port has no transcribe telemetry events, so the onEnd audio metadata assertion is not ported.")]
    public async Task Partial_Downloads_locally_when_native_URL_transcription_is_declined_and_retries_only_the_file_call()
    {
        var audio = new byte[] { 1, 2, 3, 4 };
        var previous = Download.Fetch;
        var fetches = 0;
        var fileCalls = 0;
        try
        {
            Download.Fetch = (url, request) =>
            {
                fetches++;
                return Task.FromResult(DownloadResponse.Bytes(audio, "Audio/MPEG; charset=binary"));
            };
            var model = new TranscriptFake
            {
                FromUrl = _ => Task.FromResult<TranscriptionModelResult?>(null),
                Generate = call =>
                {
                    fileCalls++;
                    return fileCalls == 1
                        ? throw new Vercel.AI.Util.ApiCallError("Retry file", "https://example.com/transcriptions", new { }, 429, new Dictionary<string, string> { ["retry-after-ms"] = "0" }, isRetryable: true)
                        : Task.FromResult(new TranscriptionModelResult("hello"));
                },
            };
            var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest
            {
                Model = model,
                AudioUrl = "https://example.com/redirect",
                MaxRetries = 1,
                TimestampGranularities = Array.Empty<string>(),
            });
            Assert.Equal("hello", result.Text);
            Assert.Equal(1, model.UrlCalls);
            Assert.Empty(model.UrlGenerated!.TimestampGranularities!);
            Assert.Equal(1, fetches);
            Assert.Equal(2, fileCalls);
            Assert.Equal(audio, model.Generated!.Audio);
            Assert.Empty(model.Generated.TimestampGranularities!);
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    [Fact]
    [UpstreamTest(Once + "retries native URL calls without switching to file upload", Coverage = UpstreamCoverage.Covered, Commit = NativeUrlCommit)]
    public async Task Retries_native_URL_calls_without_switching_to_file_upload()
    {
        var audio = new Uri("https://example.com/audio.mp3");
        var attempts = 0;
        var model = new TranscriptFake
        {
            FromUrl = _ => ++attempts == 1
                ? throw new Vercel.AI.Util.ApiCallError("Retry this request", "https://example.com/transcriptions", new { }, 429, new Dictionary<string, string> { ["retry-after-ms"] = "0" }, isRetryable: true)
                : Task.FromResult<TranscriptionModelResult?>(new TranscriptionModelResult("hello")),
        };
        var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = model, AudioUrl = audio.AbsoluteUri, MaxRetries = 1 });
        Assert.Equal("hello", result.Text);
        Assert.Equal(2, model.UrlCalls);
        Assert.Equal(audio, model.UrlGenerated!.Audio);
        Assert.Equal(0, model.GenerateCalls);
    }

    [Fact]
    [UpstreamTest(Once + "preserves explicit custom downloads even for URL-capable models", Coverage = UpstreamCoverage.Covered, Commit = NativeUrlCommit)]
    public async Task Preserves_explicit_custom_downloads_even_for_URL_capable_models()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var downloads = 0;
        var model = new TranscriptFake { FromUrl = _ => throw new InvalidOperationException("native URL must not be used") };
        await TranscribeOperation.TranscribeAsync(new TranscribeRequest
        {
            Model = model,
            AudioUrl = "https://example.com/audio.mp3",
            Download = (url, token) =>
            {
                downloads++;
                return Task.FromResult(new DownloadedMedia(bytes, "audio/mpeg"));
            },
        });
        Assert.Equal(1, downloads);
        Assert.Equal(0, model.UrlCalls);
        Assert.Equal(bytes, model.Generated!.Audio);
    }

    [Fact]
    [UpstreamTest(Once + "continues sending bytes to file transcription for URL-capable models", Coverage = UpstreamCoverage.Covered, Commit = NativeUrlCommit)]
    public async Task Continues_sending_bytes_to_file_transcription_for_URL_capable_models()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var model = new TranscriptFake { FromUrl = _ => throw new InvalidOperationException("native URL must not be used") };
        await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = model, Audio = bytes });
        Assert.Equal(0, model.UrlCalls);
        Assert.Equal(bytes, model.Generated!.Audio);
    }

    [Fact]
    [UpstreamTest(Once + "should send args to doGenerate", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_arguments_to_do_generate()
    {
        var audio = new byte[] { 1, 2, 3, 4 };
        var model = new TranscriptFake();
        await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = model, Audio = audio, ProviderOptions = OperationJson.Parse("{\"p\":true}"), Headers = new Dictionary<string, string> { ["X-Test"] = "1" } });
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
        await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = model, Audio = new byte[] { 0, 0, 0, 0x18, 0x66, 0x74, 0x79, 0x70, 0, 0, 0, 0 } });
        Assert.Equal("audio/mp4", model.Generated!.MediaType);
    }

    [Fact]
    [UpstreamTest(Once + "should detect ADTS AAC audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_adts_aac_audio()
    {
        var model = new TranscriptFake();
        await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = model, Audio = new byte[] { 0xFF, 0xF1, 0x50, 0x80 } });
        Assert.Equal("audio/aac", model.Generated!.MediaType);
    }

    [Fact]
    [UpstreamTest(Once + "should return warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_warnings()
    {
        var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Warnings = new[] { OperationWarning.Other("note") } }, Audio = new byte[] { 1 } });
        Assert.Equal("note", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Once + "should call logWarnings with the correct warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_transcript_warnings()
    {
        var seen = Watch();
        try
        {
            await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Provider = "speech", ModelId = "whisper", Warnings = new[] { OperationWarning.Other("note") } }, Audio = new byte[] { 1 } });
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
            await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake(), Audio = new byte[] { 1 } });
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
        var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Text = "hello world", Language = "en", Duration = 2 }, Audio = new byte[] { 1 } });
        Assert.Equal("hello world", result.Text);
        Assert.Equal("en", result.Language);
        Assert.Equal((double?)2, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(Once + "should return provider word timings with $timestampGranularities", Coverage = UpstreamCoverage.Covered, Commit = "41465fd1b634dfe8311885230589e19c9b4f431e")]
    public async Task Returns_provider_word_timings_for_each_granularity()
    {
        var words = new[] { new TranscriptSegment("Hello", 0, 0.4), new TranscriptSegment("world", 0.4, 1) };
        foreach (var granularities in new IReadOnlyList<string>?[] { null, Array.Empty<string>(), new[] { "word" } })
        {
            var model = new TranscriptFake { Words = words };
            var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = model, Audio = new byte[] { 1 }, TimestampGranularities = granularities });
            Assert.Equal(granularities, model.Generated!.TimestampGranularities);
            Assert.Empty(result.Segments);
            Assert.Equal(words, result.Words);
        }
    }

    [Fact]
    [UpstreamTest(Once + "should preserve an empty array of provider word timings", Coverage = UpstreamCoverage.Covered, Commit = "41465fd1b634dfe8311885230589e19c9b4f431e")]
    public async Task Preserves_an_empty_array_of_provider_word_timings()
    {
        var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Words = Array.Empty<TranscriptSegment>() }, Audio = new byte[] { 1 } });
        Assert.NotNull(result.Words);
        Assert.Empty(result.Words);
    }

    [Fact]
    [UpstreamTest(Once + "should preserve word-level segments returned by the provider", Coverage = UpstreamCoverage.Covered, Commit = "41465fd1b634dfe8311885230589e19c9b4f431e")]
    public async Task Preserves_word_level_segments_returned_by_the_provider()
    {
        var segments = new[] { new TranscriptSegment("Hello", 0, 0.4), new TranscriptSegment("world", 0.4, 1) };
        var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Segments = segments }, Audio = new byte[] { 1 } });
        Assert.Equal(segments, result.Segments);
        Assert.Null(result.Words);
    }

    [Fact]
    [UpstreamTest(Once + "should return an empty transcript", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_an_empty_transcript()
    {
        var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Text = string.Empty, Language = "en", Duration = 0 }, Audio = new byte[] { 1 } });
        Assert.Equal(string.Empty, result.Text);
        Assert.Empty(result.Segments);
        Assert.Equal("en", result.Language);
        Assert.Equal((double?)0, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(Once + "should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_response_metadata()
    {
        var response = new ProviderResponse(id: "response", modelId: "whisper", body: OperationJson.Parse("{\"raw\":true}"));
        var result = await TranscribeOperation.TranscribeAsync(new TranscribeRequest { Model = new TranscriptFake { Response = response, Metadata = OperationJson.Parse("{\"p\":1}") }, Audio = new byte[] { 1 } });
        Assert.Equal("response", result.Responses[0].Id);
        Assert.Equal(1, result.ProviderMetadata.GetProperty("p").GetInt32());
    }
}
