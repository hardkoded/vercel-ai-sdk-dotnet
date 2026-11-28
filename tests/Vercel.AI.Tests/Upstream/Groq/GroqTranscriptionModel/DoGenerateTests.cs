// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using Vercel.AI.Groq;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Groq.GroqTranscriptionModel.GroqTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Groq.GroqTranscriptionModel;

/// <summary>Port of <c>groq-transcription-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Id = "packages/groq/src/groq-transcription-model.test.ts::doGenerate::";
    private const string Commit = "36802d48afa7132420acddaeae3f601b2fccb7f1";

    [Fact]
    [UpstreamTest(
        Id + "sends the audio URL without a file attachment",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Sends_the_audio_URL_without_a_file_attachment()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(JsonBody()));
        var model = (ITranscriptionUrlCaller)
            CreateProvider(handler).TranscriptionModel("whisper-large-v3-turbo");
        var audio = new Uri("https://storage.example.com/large-audio.mp3");

        var result = await model.DoGenerateFromUrlAsync(
            new TranscriptionModelUrlCall(
                audio,
                JsonDocument
                    .Parse(
                        "{\"groq\":{\"responseFormat\":\"verbose_json\",\"language\":\"en\",\"prompt\":\"Galileo\",\"temperature\":0,\"timestampGranularities\":[\"word\",\"segment\"]}}"
                    )
                    .RootElement,
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        var call = Assert.Single(handler.Calls);
        Assert.Equal(
            new[]
            {
                "model=whisper-large-v3-turbo",
                "url=" + audio.AbsoluteUri,
                "language=en",
                "prompt=Galileo",
                "response_format=verbose_json",
                "temperature=0",
                "timestamp_granularities[]=word",
                "timestamp_granularities[]=segment",
            }.OrderBy(Order),
            Multipart(call).Select(field => field.Key + "=" + field.Value).OrderBy(Order)
        );
        Assert.DoesNotContain(Multipart(call), field => field.Key == "file");
        Assert.Equal("Hello world!", result!.Text);
        Assert.Equal("English", result.Language);
        Assert.Equal(2.5, result.DurationInSeconds);
    }

    [Theory]
    [InlineData(301)]
    [InlineData(302)]
    [InlineData(303)]
    [InlineData(307)]
    [InlineData(308)]
    [UpstreamTest(
        Id + "declines native URLs when Groq cannot follow a %s redirect",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Declines_native_URLs_when_Groq_cannot_follow_a_redirect(int status)
    {
        var handler = new ParityHandler(_ =>
            Error(
                HttpStatusCode.BadRequest,
                "failed to retrieve media: received status code: " + status
            )
        );
        var model = (ITranscriptionUrlCaller)
            CreateProvider(handler).TranscriptionModel("whisper-large-v3-turbo");

        var result = await model.DoGenerateFromUrlAsync(
            UrlCall("https://example.com/redirect"),
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Theory]
    [InlineData(400, "failed to retrieve media: received status code: 404")]
    [InlineData(400, "invalid audio format")]
    [InlineData(401, "invalid API key")]
    [InlineData(429, "rate limit exceeded")]
    [UpstreamTest(
        Id + "preserves other native URL errors (%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Preserves_other_native_URL_errors(int status, string message)
    {
        var handler = new ParityHandler(_ => Error((HttpStatusCode)status, message));
        var provider = CreateProvider(handler);
        var model = (ITranscriptionUrlCaller)provider.TranscriptionModel("whisper-large-v3-turbo");

        var error = await Assert.ThrowsAnyAsync<ApiException>(() =>
            model.DoGenerateFromUrlAsync(
                UrlCall("https://example.com/audio.mp3"),
                CancellationToken.None
            )
        );

        Assert.Equal(message, error.Message);
        Assert.Equal(status, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Id + "strips media fragments without mutating the input URL",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Strips_media_fragments_without_mutating_the_input_URL()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(JsonBody()));
        var model = (ITranscriptionUrlCaller)
            CreateProvider(handler).TranscriptionModel("whisper-large-v3-turbo");
        var audio = new Uri("https://example.com/audio.mp3#t=0");

        var result = await model.DoGenerateFromUrlAsync(
            new TranscriptionModelUrlCall(
                audio,
                OperationJson.Parse("{}"),
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        Assert.Equal("Hello world!", result!.Text);
        Assert.Equal("https://example.com/audio.mp3", Field(Assert.Single(handler.Calls), "url"));
        Assert.Equal("#t=0", audio.Fragment);
    }

    [Theory]
    [InlineData("ftp://example.com/audio.mp3")]
    [InlineData("https://user:secret@example.com/audio.mp3")]
    [UpstreamTest(
        Id + "rejects unsupported URL forms before HTTP: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_unsupported_URL_forms_before_HTTP(string value)
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(JsonBody()));
        var model = (ITranscriptionUrlCaller)
            CreateProvider(handler).TranscriptionModel("whisper-large-v3-turbo");

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateFromUrlAsync(UrlCall(value), CancellationToken.None)
        );

        Assert.Contains(
            "Groq URL transcription requires an HTTP(S) URL without userinfo.",
            error.Message
        );
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "should pass the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_model()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(JsonBody()));
        var model = CreateProvider(handler).TranscriptionModel("whisper-large-v3-turbo");

        await ((ITranscriptionCaller)model).DoGenerateAsync(FileCall(), CancellationToken.None);

        Assert.Equal("whisper-large-v3-turbo", Field(Assert.Single(handler.Calls), "model"));
    }

    [Fact]
    [UpstreamTest(Id + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(JsonBody()));
        var provider = CreateProvider(handler, options =>
            options.Headers["Custom-Provider-Header"] = "provider-header-value"
        );

        await (
            (ITranscriptionCaller)provider.TranscriptionModel("whisper-large-v3-turbo")
        ).DoGenerateAsync(
            new TranscriptionModelCall(
                AudioData,
                "audio/wav",
                OperationJson.Parse("{}"),
                new Dictionary<string, string>
                {
                    ["Custom-Request-Header"] = "request-header-value",
                },
                CancellationToken.None
            ),
            CancellationToken.None
        );

        var call = Assert.Single(handler.Calls);
        Assert.Equal("Bearer test-api-key", call.Header("authorization"));
        Assert.StartsWith("multipart/form-data; boundary=", call.Header("content-type"));
        Assert.Equal("provider-header-value", call.Header("custom-provider-header"));
        Assert.Equal("request-header-value", call.Header("custom-request-header"));
        Assert.Contains("ai-sdk/groq/", call.Header("user-agent"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should extract the transcription text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_the_transcription_text()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(JsonBody()));
        var model = (ITranscriptionCaller)
            CreateProvider(handler).TranscriptionModel("whisper-large-v3-turbo");

        var result = await model.DoGenerateAsync(FileCall(), CancellationToken.None);

        Assert.Equal("Hello world!", result.Text);
    }

    [Theory]
    [InlineData("file")]
    [InlineData("url")]
    [UpstreamTest(
        Id + "should extract a plain-text transcription response",
        Coverage = UpstreamCoverage.Covered
    )]
    [UpstreamTest(
        Id + "should extract a plain-text transcription response for %s input",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_extract_a_plain_text_transcription_response(string input)
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Bytes(
                System.Text.Encoding.UTF8.GetBytes(TextResponseBody),
                "text/plain; charset=UTF-8"
            )
        );
        var model = CreateProvider(handler).TranscriptionModel("whisper-large-v3-turbo");
        var providerOptions = OperationJson.Parse("{\"groq\":{\"responseFormat\":\"text\"}}");

        var result =
            input == "url"
                ? await ((ITranscriptionUrlCaller)model).DoGenerateFromUrlAsync(
                    new TranscriptionModelUrlCall(
                        new Uri("https://example.com/audio.mp3"),
                        providerOptions,
                        new Dictionary<string, string>(),
                        CancellationToken.None
                    ),
                    CancellationToken.None
                )
                : await ((ITranscriptionCaller)model).DoGenerateAsync(
                    new TranscriptionModelCall(
                        AudioData,
                        "audio/wav",
                        providerOptions,
                        new Dictionary<string, string>(),
                        CancellationToken.None
                    ),
                    CancellationToken.None
                );

        Assert.Equal("text", Field(Assert.Single(handler.Calls), "response_format"));
        Assert.Equal(TextResponseBody, result!.Text);
        Assert.Equal(TextResponseBody, result.Response.Body!.Value.GetString());
    }

    [Fact]
    [UpstreamTest(
        Id + "should include response data with timestamp, modelId and headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_response_data_with_timestamp_modelId_and_headers()
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Json(
                JsonBody(),
                ("x-request-id", "test-request-id"),
                ("x-ratelimit-remaining", "123")
            )
        );
        var testDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var model = new Vercel.AI.Groq.GroqTranscriptionModel(
            CreateProvider(handler),
            "whisper-large-v3-turbo",
            () => testDate
        );

        var result = await model.DoGenerateAsync(FileCall(), CancellationToken.None);

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("whisper-large-v3-turbo", result.Response.ModelId);
        Assert.Equal("application/json", result.Response.Headers!["content-type"]);
        Assert.Equal("test-request-id", result.Response.Headers["x-request-id"]);
        Assert.Equal("123", result.Response.Headers["x-ratelimit-remaining"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use real date when no custom date provider is specified",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_real_date_when_no_custom_date_provider_is_specified()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(JsonBody()));
        var testDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var model = new Vercel.AI.Groq.GroqTranscriptionModel(
            CreateProvider(handler),
            "whisper-large-v3-turbo",
            () => testDate
        );

        var result = await model.DoGenerateAsync(FileCall(), CancellationToken.None);

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("whisper-large-v3-turbo", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should correctly pass provider options when they are an array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_correctly_pass_provider_options_when_they_are_an_array()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(JsonBody()));
        var model = (ITranscriptionCaller)
            CreateProvider(handler).TranscriptionModel("whisper-large-v3-turbo");

        await model.DoGenerateAsync(
            new TranscriptionModelCall(
                AudioData,
                "audio/wav",
                OperationJson.Parse(
                    "{\"groq\":{\"timestampGranularities\":[\"segment\"],\"responseFormat\":\"verbose_json\"}}"
                ),
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        var call = Assert.Single(handler.Calls);
        Assert.Equal("segment", Field(call, "timestamp_granularities[]"));
        Assert.Equal("verbose_json", Field(call, "response_format"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should fallback to words when segments are not available",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fallback_to_words_when_segments_are_not_available()
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Json(
                JsonBody(
                    "null",
                    "[{\"word\":\"Hello\",\"start\":0,\"end\":1},{\"word\":\"world\",\"start\":1,\"end\":2}]"
                )
            )
        );
        var model = (ITranscriptionCaller)
            CreateProvider(handler).TranscriptionModel("whisper-large-v3");

        var result = await model.DoGenerateAsync(
            new TranscriptionModelCall(
                AudioData,
                "audio/wav",
                OperationJson.Parse(
                    "{\"groq\":{\"language\":\"en\",\"responseFormat\":\"verbose_json\",\"timestampGranularities\":[\"word\"]}}"
                ),
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        Assert.Collection(
            result.Segments,
            first =>
            {
                Assert.Equal("Hello", first.Text);
                Assert.Equal(0, first.StartSecond);
                Assert.Equal(1, first.EndSecond);
            },
            second =>
            {
                Assert.Equal("world", second.Text);
                Assert.Equal(1, second.StartSecond);
                Assert.Equal(2, second.EndSecond);
            }
        );
    }

    private static string Order(string value) => value;

    private static TranscriptionModelCall FileCall()
    {
        return new TranscriptionModelCall(
            AudioData,
            "audio/wav",
            OperationJson.Parse("{}"),
            new Dictionary<string, string>(),
            CancellationToken.None
        );
    }

    private static TranscriptionModelUrlCall UrlCall(string url)
    {
        return new TranscriptionModelUrlCall(
            new Uri(url),
            OperationJson.Parse("{}"),
            new Dictionary<string, string>(),
            CancellationToken.None
        );
    }
}
