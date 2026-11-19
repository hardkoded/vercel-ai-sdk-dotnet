// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiSpeechModel.XaiSpeechTestSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiSpeechModel;

/// <summary>Port of <c>xai-speech-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Id = "packages/xai/src/xai-speech-model.test.ts::doGenerate::";

    [Fact]
    [UpstreamTest(
        Id + "should send text with xAI defaults",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_text_with_xAI_defaults()
    {
        var handler = AudioHandler();

        await Speech(CreateProvider(handler)).DoGenerateAsync(Call(), CancellationToken.None);

        var call = Assert.Single(handler.Calls);
        AssertJson(
            "{\"text\":\"Hello from the AI SDK!\",\"voice_id\":\"eve\",\"language\":\"auto\",\"output_format\":{\"codec\":\"mp3\"}}",
            Body(call)
        );
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal(Url, call.Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass standard speech options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_standard_speech_options()
    {
        var handler = AudioHandler("audio/wav");

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(
                Call(voice: "ara", language: "en", outputFormat: "wav", speed: 1.2),
                CancellationToken.None
            );

        AssertJson(
            "{\"text\":\"Hello from the AI SDK!\",\"voice_id\":\"ara\",\"language\":\"en\",\"output_format\":{\"codec\":\"wav\"},\"speed\":1.2}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Theory]
    [InlineData("mp3")]
    [InlineData("wav")]
    [InlineData("pcm")]
    [InlineData("mulaw")]
    [InlineData("alaw")]
    [UpstreamTest(
        Id + "should accept the %s output format",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_accept_the_output_format(string outputFormat)
    {
        var handler = AudioHandler();

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(Call(outputFormat: outputFormat), CancellationToken.None);

        AssertMatches(
            "{\"output_format\":{\"codec\":\"" + outputFormat + "\"}}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map provider options onto xAI request fields",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_provider_options_onto_xAI_request_fields()
    {
        var handler = AudioHandler();

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    xai: "{\"sampleRate\":44100,\"bitRate\":192000,\"optimizeStreamingLatency\":1,\"textNormalization\":true}"
                ),
                CancellationToken.None
            );

        AssertMatches(
            "{\"output_format\":{\"codec\":\"mp3\",\"sample_rate\":44100,\"bit_rate\":192000},\"optimize_streaming_latency\":1,\"text_normalization\":true}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass withTimestamps and replace provider options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_withTimestamps_and_replace_provider_options()
    {
        var handler = TimestampsHandler();

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(
                Call(xai: "{\"withTimestamps\":true,\"replace\":{\"nginx\":\"/ˈɛndʒɪn ˈɛks/\"}}"),
                CancellationToken.None
            );

        AssertMatches(
            "{\"with_timestamps\":true,\"replace\":{\"nginx\":\"/ˈɛndʒɪn ˈɛks/\"}}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should decode the with_timestamps envelope and extract provider metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_decode_the_with_timestamps_envelope_and_extract_provider_metadata()
    {
        var handler = TimestampsHandler(("x-trace-id", "993675dc-8ea6-4f54-b4ad-a59ac2615026"));

        var result = await Speech(CreateProvider(handler))
            .DoGenerateAsync(Call("Hi", xai: "{\"withTimestamps\":true}"), CancellationToken.None);

        Assert.Equal(Audio, result.Audio);
        AssertJson(
            "{\"xai\":{\"traceId\":\"993675dc-8ea6-4f54-b4ad-a59ac2615026\",\"duration\":1.19,\"contentType\":\"audio/mpeg\",\"audioTimestamps\":{\"graphChars\":[\"H\",\"i\"],\"graphTimes\":[[0.04,0.06],[0.06,0.1]]}}}",
            Metadata(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should extract the trace id from binary responses",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_extract_the_trace_id_from_binary_responses()
    {
        var handler = AudioHandler(
            "audio/mpeg",
            ("x-trace-id", "06e3dab5-e3ba-4c6b-83a6-1e9ea11d78af")
        );

        var result = await Speech(CreateProvider(handler))
            .DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson(
            "{\"xai\":{\"traceId\":\"06e3dab5-e3ba-4c6b-83a6-1e9ea11d78af\"}}",
            Metadata(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should return empty provider metadata when xAI headers are absent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_return_empty_provider_metadata_when_xAI_headers_are_absent()
    {
        var result = await Speech(CreateProvider(AudioHandler()))
            .DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson("{\"xai\":{}}", Metadata(result));
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn and use mp3 for unsupported output formats",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_and_use_mp3_for_unsupported_output_formats()
    {
        var handler = AudioHandler();

        var result = await Speech(CreateProvider(handler))
            .DoGenerateAsync(Call(outputFormat: "flac"), CancellationToken.None);

        AssertMatches(
            "{\"output_format\":{\"codec\":\"mp3\"}}",
            Body(Assert.Single(handler.Calls))
        );
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "outputFormat"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn and ignore bitRate for non-mp3 output",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_and_ignore_bitRate_for_non_mp3_output()
    {
        var handler = AudioHandler();

        var result = await Speech(CreateProvider(handler))
            .DoGenerateAsync(
                Call(outputFormat: "wav", xai: "{\"bitRate\":192000}"),
                CancellationToken.None
            );

        var body = Body(Assert.Single(handler.Calls));
        AssertMatches("{\"output_format\":{\"codec\":\"wav\"}}", body);
        Assert.False(body["output_format"]!.AsObject().ContainsKey("bit_rate"));
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "providerOptions"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn when instructions are provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_when_instructions_are_provided()
    {
        var result = await Speech(CreateProvider(AudioHandler()))
            .DoGenerateAsync(Call(instructions: "Speak cheerfully"), CancellationToken.None);

        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "instructions"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass headers and the xAI user agent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The port's user agent is ai-sdk/xai/<version>, like every provider in the port."
    )]
    public async Task Should_pass_headers_and_the_xAI_user_agent()
    {
        var handler = AudioHandler();
        var provider = CreateProvider(handler, options =>
            options.Headers["Custom-Provider-Header"] = "provider-header-value"
        );

        await Speech(provider)
            .DoGenerateAsync(
                Call(
                    headers: new Dictionary<string, string>
                    {
                        ["Custom-Request-Header"] = "request-header-value",
                    }
                ),
                CancellationToken.None
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal("Bearer test-api-key", call.Header("authorization"));
        Assert.Equal("application/json", call.Header("content-type")?.Split(';')[0]);
        Assert.Equal("provider-header-value", call.Header("custom-provider-header"));
        Assert.Equal("request-header-value", call.Header("custom-request-header"));
        Assert.Contains("ai-sdk/xai/" + AiSdkVersion.Version, call.Header("user-agent"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should return binary audio data",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_return_binary_audio_data()
    {
        var result = await Speech(CreateProvider(AudioHandler()))
            .DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(Audio, result.Audio);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include response timestamp, model id, and headers",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_include_response_timestamp_model_id_and_headers()
    {
        var handler = AudioHandler("audio/mpeg", ("x-request-id", "test-request-id"));
        var testDate = new DateTime(0, DateTimeKind.Utc);
        var model = new Vercel.AI.Xai.XaiSpeechModel(CreateProvider(handler), "", () => testDate);

        var result = await ((ISpeechCaller)model).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(testDate, result.Response.Timestamp);
        Assert.Equal("", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle API errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_handle_API_errors()
    {
        var handler = new ParityHandler(_ =>
            Error(
                HttpStatusCode.BadRequest,
                "{\"error\":{\"message\":\"Invalid text\",\"type\":\"invalid_request_error\"}}"
            )
        );

        var error = await Assert.ThrowsAnyAsync<Vercel.AI.Provider.ApiException>(() =>
            Speech(CreateProvider(handler)).DoGenerateAsync(Call(), CancellationToken.None)
        );

        Assert.Equal("Invalid text", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use the real date when no custom date provider is specified",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_use_the_real_date_when_no_custom_date_provider_is_specified()
    {
        var before = DateTime.UtcNow;
        var result = await Speech(CreateProvider(AudioHandler()))
            .DoGenerateAsync(Call(), CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.InRange(result.Response.Timestamp!.Value, before, after);
        Assert.Equal("", result.Response.ModelId);
    }
}
