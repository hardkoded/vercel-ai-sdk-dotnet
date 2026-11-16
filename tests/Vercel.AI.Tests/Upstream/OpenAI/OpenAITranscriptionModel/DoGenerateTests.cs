// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using TranscriptionModel = Vercel.AI.OpenAI.OpenAITranscriptionModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAITranscriptionModel;

/// <summary>
/// Port of <c>openai-transcription-model.test.ts</c> &gt; <c>doGenerate</c>. The other tests of this <c>describe</c> are in
/// <c>OpenAIMediaUpstreamTests</c>.
/// </summary>
public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::";

    private static OpenAITranscriptionCall Call(string? openai = null)
    {
        var call = new OpenAITranscriptionCall(
            new AudioInput(new byte[] { 1, 2, 3 }, "audio/mpeg", "audio.mp3")
        );
        if (openai != null)
        {
            call.ProviderOptions = OpenAIUpstream.Json("{\"openai\":" + openai + "}");
        }

        return call;
    }

    private const string WhisperJson =
        "{\"text\":\"Hello world\",\"language\":\"english\",\"duration\":2,\"words\":[{\"word\":\"Hello\",\"start\":0,\"end\":1},{\"word\":\"world\",\"start\":1,\"end\":2}]}";

    private const string DiarizedJson =
        "{\"task\":\"transcribe\",\"duration\":3.2,\"text\":\"Hello from Alice. Hello from Bob.\",\"segments\":["
        + "{\"type\":\"transcript.text.segment\",\"id\":\"seg_1\",\"start\":0,\"end\":1.5,\"text\":\"Hello from Alice.\",\"speaker\":\"A\"},"
        + "{\"type\":\"transcript.text.segment\",\"id\":\"seg_2\",\"start\":1.5,\"end\":3.2,\"text\":\"Hello from Bob.\",\"speaker\":\"B\"}]}";

    private static async Task<(OpenAICapture Capture, TranscriptionModelResult Result)> Generate(
        string modelId,
        string responseJson,
        string? openai = null,
        params string[] timestampGranularities
    )
    {
        var capture = new OpenAICapture { ResponseJson = responseJson };
        using var options = JsonDocument.Parse(
            openai == null ? "{}" : "{\"openai\":" + openai + "}"
        );
        var call = new TranscriptionModelCall(
            new byte[] { 1, 2, 3 },
            "audio/wav",
            options.RootElement.Clone(),
            new Dictionary<string, string>(),
            CancellationToken.None
        )
        {
            TimestampGranularities =
                timestampGranularities.Length == 0 ? null
                : timestampGranularities.SequenceEqual(new[] { string.Empty })
                    ? Array.Empty<string>()
                : timestampGranularities,
        };
        var result = await new TranscriptionModel(
            OpenAIUpstream.Provider(capture),
            modelId
        ).DoGenerateAsync(call, CancellationToken.None);
        return (capture, result);
    }

    private static Dictionary<string, List<string>> Fields(OpenAICapture capture)
    {
        return capture
            .Parts.Where(part => part.FileName == null)
            .GroupBy(part => part.Name)
            .ToDictionary(group => group.Key, group => group.Select(Text).ToList());
    }

    private static void AssertWarnings(
        TranscriptionModelResult result,
        string feature,
        string details
    )
    {
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal(feature, warning.Feature);
        Assert.Equal(details, warning.Details);
    }

    private static string Text(OpenAIMultipartPart part)
    {
        return System.Text.Encoding.UTF8.GetString(part.Data);
    }

    [Theory]
    [InlineData("gpt-live-transcribe")]
    [InlineData("gpt-live-transcribe-2026-01-01")]
    [UpstreamTest(
        Prefix + "should reject %s for non-streaming transcription",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3"
    )]
    public async Task Should_reject_model_for_non_streaming_transcription(string modelId)
    {
        var model = new TranscriptionModel(OpenAIUpstream.Provider(new OpenAICapture()), modelId);

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            model.TranscribeAsync(Call(), CancellationToken.None)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send gpt-transcribe to the REST transcription endpoint",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3"
    )]
    public async Task Should_send_gpt_transcribe_to_the_REST_transcription_endpoint()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"text\":\"Hello world\"}" };

        await new TranscriptionModel(
            OpenAIUpstream.Provider(capture),
            "gpt-transcribe"
        ).TranscribeAsync(Call(), CancellationToken.None);

        Assert.EndsWith("/audio/transcriptions", capture.Uri, StringComparison.Ordinal);
        Assert.Equal("gpt-transcribe", Text(capture.Parts.Single(part => part.Name == "model")));
    }

    [Theory]
    [InlineData("gpt-transcribe")]
    [InlineData("openai/gpt-transcribe")]
    [InlineData("custom-transcription-model")]
    [UpstreamTest(
        Prefix + "should send language and vocabulary hints for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3"
    )]
    public async Task Should_send_language_and_vocabulary_hints_for_model(string modelId)
    {
        var capture = new OpenAICapture { ResponseJson = "{\"text\":\"Hello world\"}" };

        var result = await new TranscriptionModel(
            OpenAIUpstream.Provider(capture),
            modelId
        ).TranscribeAsync(
            Call(
                "{\"languages\":[\"en\",\"fr\"],\"keywords\":[\"premium plan\",\"AC-42\"],\"prompt\":\"A customer support call.\"}"
            ),
            CancellationToken.None
        );

        Assert.Equal(
            new[] { "en", "fr" },
            capture.Parts.Where(part => part.Name == "languages[]").Select(Text).ToArray()
        );
        Assert.Equal(
            new[] { "premium plan", "AC-42" },
            capture.Parts.Where(part => part.Name == "keywords[]").Select(Text).ToArray()
        );
        Assert.Equal(
            "A customer support call.",
            Text(capture.Parts.Single(part => part.Name == "prompt"))
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit empty hint arrays while preserving the singular language option",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3"
    )]
    public async Task Should_omit_empty_hint_arrays_while_preserving_the_singular_language_option()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"text\":\"Hello world\"}" };

        await new TranscriptionModel(OpenAIUpstream.Provider(capture), "whisper-1").TranscribeAsync(
            Call("{\"language\":\"en\",\"languages\":[],\"keywords\":[]}"),
            CancellationToken.None
        );

        Assert.Equal("en", Text(capture.Parts.Single(part => part.Name == "language")));
        Assert.DoesNotContain(capture.Parts, part => part.Name == "languages[]");
        Assert.DoesNotContain(capture.Parts, part => part.Name == "keywords[]");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{}")]
    [InlineData("{\"prompt\":\"A spoken recording.\"}")]
    [UpstreamTest(
        Prefix + "should map shared timestamps with provider options $providerOptions",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_map_shared_timestamps_with_provider_options(string? openai)
    {
        var (capture, result) = await Generate("whisper-1", WhisperJson, openai, "word");

        Assert.Equal(new[] { "word" }, Fields(capture)["timestamp_granularities[]"]);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("word", "segment")]
    [InlineData("segment", "word")]
    [UpstreamTest(
        Prefix + "should warn when the OpenAI timestamp option excludes $excludedGranularity",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_warn_when_the_OpenAI_timestamp_option_excludes_granularity(
        string providerGranularity,
        string excludedGranularity
    )
    {
        var (capture, result) = await Generate(
            "whisper-1",
            WhisperJson,
            "{\"timestampGranularities\":[\"" + providerGranularity + "\"]}",
            "word",
            "segment"
        );

        Assert.Equal(new[] { providerGranularity }, Fields(capture)["timestamp_granularities[]"]);
        AssertWarnings(
            result,
            "timestampGranularities",
            "The \""
                + excludedGranularity
                + "\" timestamp granularity is excluded by providerOptions.openai.timestampGranularities."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not warn when the OpenAI timestamp option includes the shared request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_not_warn_when_the_OpenAI_timestamp_option_includes_the_shared_request()
    {
        var (capture, result) = await Generate(
            "whisper-1",
            WhisperJson,
            "{\"timestampGranularities\":[\"word\"]}",
            "word"
        );

        Assert.Equal(new[] { "word" }, Fields(capture)["timestamp_granularities[]"]);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "should preserve an empty timestamp request %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_preserve_an_empty_timestamp_request(bool providerOption)
    {
        var (capture, result) = providerOption
            ? await Generate("whisper-1", WhisperJson, "{\"timestampGranularities\":[]}")
            : await Generate("whisper-1", WhisperJson, null, string.Empty);

        var fields = Fields(capture);
        Assert.Equal("verbose_json", fields["response_format"][0]);
        Assert.False(fields.ContainsKey("timestamp_granularities[]"));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should let an empty OpenAI timestamp option override the shared request and warn",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_let_an_empty_OpenAI_timestamp_option_override_the_shared_request_and_warn()
    {
        var (capture, result) = await Generate(
            "whisper-1",
            WhisperJson,
            "{\"timestampGranularities\":[]}",
            "word",
            "segment"
        );

        Assert.False(Fields(capture).ContainsKey("timestamp_granularities[]"));
        Assert.Equal(
            new[]
            {
                "The \"word\" timestamp granularity is excluded by providerOptions.openai.timestampGranularities.",
                "The \"segment\" timestamp granularity is excluded by providerOptions.openai.timestampGranularities.",
            },
            result.Warnings.Select(warning => warning.Details)
        );
        Assert.All(result.Warnings, warning =>
            Assert.Equal("timestampGranularities", warning.Feature)
        );
    }

    [Theory]
    [InlineData("gpt-4o-transcribe")]
    [InlineData("gpt-4o-mini-transcribe")]
    [InlineData("gpt-transcribe")]
    [UpstreamTest(
        Prefix + "should omit unsupported shared timestamps for %s JSON responses and warn",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_omit_unsupported_shared_timestamps_for_JSON_responses_and_warn(
        string modelId
    )
    {
        var (capture, result) = await Generate(
            modelId,
            "{\"text\":\"Hello world\"}",
            null,
            "word",
            "segment"
        );

        var fields = Fields(capture);
        Assert.False(fields.ContainsKey("response_format"));
        Assert.False(fields.ContainsKey("timestamp_granularities[]"));
        Assert.Equal(
            new[] { "word", "segment" }.Select(granularity =>
                "OpenAI transcription with the default JSON response format does not support "
                + granularity
                + " timestamps."
            ),
            result.Warnings.Select(warning => warning.Details)
        );
        Assert.All(result.Warnings, warning =>
            Assert.Equal("timestampGranularities", warning.Feature)
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "should omit unsupported %s timestamps for an explicit JSON response and warn",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_omit_unsupported_timestamps_for_an_explicit_JSON_response_and_warn(
        bool shared
    )
    {
        var (capture, result) = shared
            ? await Generate(
                "whisper-1",
                "{\"text\":\"Hello world\"}",
                "{\"responseFormat\":\"json\"}",
                "word",
                "segment"
            )
            : await Generate(
                "whisper-1",
                "{\"text\":\"Hello world\"}",
                "{\"responseFormat\":\"json\",\"timestampGranularities\":[\"word\",\"segment\"]}"
            );

        var fields = Fields(capture);
        Assert.Equal("json", fields["response_format"][0]);
        Assert.False(fields.ContainsKey("timestamp_granularities[]"));
        Assert.Equal(
            new[] { "word", "segment" }.Select(granularity =>
                "OpenAI transcription with the json response format does not support "
                + granularity
                + " timestamps."
            ),
            result.Warnings.Select(warning => warning.Details)
        );
        Assert.All(result.Warnings, warning =>
            Assert.Equal(
                shared ? "timestampGranularities" : "providerOptions.openai.timestampGranularities",
                warning.Feature
            )
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "should return diarized segments without sending %s timestamp controls",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_return_diarized_segments_without_sending_timestamp_controls(
        bool shared
    )
    {
        var (capture, result) = shared
            ? await Generate("gpt-4o-transcribe-diarize", DiarizedJson, null, "segment")
            : await Generate(
                "gpt-4o-transcribe-diarize",
                DiarizedJson,
                "{\"timestampGranularities\":[\"segment\"]}"
            );

        var fields = Fields(capture);
        Assert.Equal("diarized_json", fields["response_format"][0]);
        Assert.False(fields.ContainsKey("timestamp_granularities[]"));
        Assert.Empty(result.Warnings);
        Assert.Equal(
            new[] { ("Hello from Alice.", 0d, 1.5), ("Hello from Bob.", 1.5, 3.2) },
            result.Segments.Select(segment =>
                (segment.Text, segment.StartSecond, segment.EndSecond)
            )
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix
            + "should warn only about unavailable diarized word timings requested through %s options",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_warn_only_about_unavailable_diarized_word_timings(bool shared)
    {
        var (capture, result) = shared
            ? await Generate("gpt-4o-transcribe-diarize", DiarizedJson, null, "word", "segment")
            : await Generate(
                "gpt-4o-transcribe-diarize",
                DiarizedJson,
                "{\"timestampGranularities\":[\"word\",\"segment\"]}"
            );

        Assert.False(Fields(capture).ContainsKey("timestamp_granularities[]"));
        Assert.Null(result.Words);
        Assert.Equal(2, result.Segments.Count);
        AssertWarnings(
            result,
            shared ? "timestampGranularities" : "providerOptions.openai.timestampGranularities",
            "OpenAI transcription with the diarized_json response format does not support word timestamps."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit unsupported provider timestamps for gpt-4o-transcribe and warn",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_omit_unsupported_provider_timestamps_for_gpt_4o_transcribe_and_warn()
    {
        var (capture, result) = await Generate(
            "gpt-4o-transcribe",
            "{\"text\":\"Hello world\"}",
            "{\"timestampGranularities\":[\"word\"]}"
        );

        var fields = Fields(capture);
        Assert.Equal("gpt-4o-transcribe", fields["model"][0]);
        Assert.Equal("0", fields["temperature"][0]);
        Assert.False(fields.ContainsKey("response_format"));
        Assert.False(fields.ContainsKey("timestamp_granularities[]"));
        AssertWarnings(
            result,
            "providerOptions.openai.timestampGranularities",
            "OpenAI transcription with the default JSON response format does not support word timestamps."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep word timings when segments are also present",
        Coverage = UpstreamCoverage.Covered,
        Commit = "41465fd1b634dfe8311885230589e19c9b4f431e"
    )]
    public async Task Should_keep_word_timings_when_segments_are_also_present()
    {
        var json =
            "{\"text\":\"Hello world\",\"words\":[{\"word\":\"Hello\",\"start\":0,\"end\":0.4},{\"word\":\"world\",\"start\":0.4,\"end\":1}],"
            + "\"segments\":[{\"id\":0,\"seek\":0,\"start\":0,\"end\":1,\"text\":\"Hello world\",\"tokens\":[1],\"temperature\":0,\"avg_logprob\":-0.2,\"compression_ratio\":1,\"no_speech_prob\":0.01}]}";
        var (_, result) = await Generate(
            "whisper-1",
            json,
            "{\"timestampGranularities\":[\"word\",\"segment\"]}"
        );

        Assert.Equal(
            new[] { ("Hello world", 0d, 1d) },
            result.Segments.Select(segment =>
                (segment.Text, segment.StartSecond, segment.EndSecond)
            )
        );
        Assert.Equal(
            new[] { ("Hello", 0d, 0.4), ("world", 0.4, 1d) },
            result.Words!.Select(word => (word.Text, word.StartSecond, word.EndSecond))
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var (_, result) = await Generate(
            "whisper-1",
            "{\"text\":\"Hello\",\"usage\":{\"type\":\"duration\",\"seconds\":37}}"
        );

        Assert.Equal("duration", result.Usage!.Value.GetProperty("type").GetString());
        Assert.Equal(37, result.Usage.Value.GetProperty("seconds").GetDouble());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should read the duration from duration usage when the response has no duration",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023"
    )]
    public async Task Should_read_the_duration_from_duration_usage_when_the_response_has_no_duration()
    {
        var (_, result) = await Generate(
            "gpt-transcribe",
            "{\"text\":\"Hello from the Vercel AI SDK!\",\"languages\":[{\"code\":\"en\"}],\"usage\":{\"type\":\"duration\",\"seconds\":37}}"
        );

        Assert.Equal(37d, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer the response duration over duration usage",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023"
    )]
    public async Task Should_prefer_the_response_duration_over_duration_usage()
    {
        var (_, result) = await Generate(
            "whisper-1",
            "{\"text\":\"Hello from the Vercel AI SDK!\",\"duration\":36.71,\"usage\":{\"type\":\"duration\",\"seconds\":37}}"
        );

        Assert.Equal(36.71, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not read a duration from token usage",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023"
    )]
    public async Task Should_not_read_a_duration_from_token_usage()
    {
        var (_, result) = await Generate(
            "gpt-4o-transcribe",
            "{\"text\":\"Hello from the Vercel AI SDK!\",\"usage\":{\"type\":\"tokens\",\"input_tokens\":14,\"output_tokens\":45}}"
        );

        Assert.Null(result.DurationInSeconds);
    }
}
