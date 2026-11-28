// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Mistral.MistralTranscriptionModel.MistralTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralTranscriptionModel;

/// <summary>Port of <c>mistral-transcription-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Id =
        "packages/mistral/src/mistral-transcription-model.test.ts::doGenerate::";

    private static readonly JsonNode Fixture = JsonNode.Parse(TranscriptionResponse)!;

    [Fact]
    [UpstreamTest(
        Id + "warns about realtime-only options without sending them in file uploads",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Warns_about_realtime_only_options_without_sending_them_in_file_uploads()
    {
        var handler = JsonHandler();

        var result = await Transcription(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    "{\"mistral\":{\"targetStreamingDelayMs\":480,\"turnDetection\":{},\"speakerReferences\":{\"Jane\":\"AQI=\"}}}"
                ),
                CancellationToken.None
            );

        Assert.Equal(
            new[]
            {
                "providerOptions.mistral.targetStreamingDelayMs",
                "providerOptions.mistral.speakerReferences",
                "providerOptions.mistral.turnDetection",
            },
            result.Warnings.Select(warning => warning.Feature).ToArray()
        );
        Assert.All(result.Warnings, warning =>
        {
            Assert.Equal("unsupported", warning.Type);
            Assert.Equal(
                "This option is only supported for realtime transcription.",
                warning.Details
            );
        });
        Assert.Equal(
            new[] { "file", "model" },
            Multipart(Assert.Single(handler.Calls)).Select(part => part.Name).Order().ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send Uint8Array audio as a multipart file",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_Uint8Array_audio_as_a_multipart_file()
    {
        var handler = JsonHandler();

        await Transcription(CreateProvider(handler))
            .DoGenerateAsync(Call(), CancellationToken.None);

        var call = Assert.Single(handler.Calls);
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal(Url, call.Uri.AbsoluteUri);
        Assert.Equal(ModelId, Field(call, "model"));
        var file = Multipart(call).Single(part => part.Name == "file");
        Assert.Equal("audio/wav", file.ContentType);
        Assert.Equal("audio.wav", file.FileName);
        Assert.Equal(5, file.Size);
    }

    [Fact]
    [UpstreamTest(
        Id + "should send base64 audio with a media-type-derived filename",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_base64_audio_with_a_media_type_derived_filename()
    {
        var handler = JsonHandler();

        await Transcription(CreateProvider(handler))
            .DoGenerateAsync(
                Call(mediaType: "audio/mp4", audioBase64: "aGVsbG8="),
                CancellationToken.None
            );

        var file = Multipart(Assert.Single(handler.Calls)).Single(part => part.Name == "file");
        Assert.Equal("audio/mp4", file.ContentType);
        Assert.Equal("audio.m4a", file.FileName);
        Assert.Equal(5, file.Size);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass provider options as Mistral multipart fields",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_provider_options_as_Mistral_multipart_fields()
    {
        var handler = JsonHandler();

        await Transcription(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    "{\"mistral\":{\"temperature\":0.2,\"timestampGranularities\":[\"segment\",\"word\"],\"diarize\":true,\"contextBias\":[\"Vercel\",\"AI_SDK\"]}}"
                ),
                CancellationToken.None
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal("0.2", Field(call, "temperature"));
        Assert.Equal(new[] { "segment", "word" }, Values(call, "timestamp_granularities"));
        Assert.Equal("true", Field(call, "diarize"));
        Assert.Equal(new[] { "Vercel", "AI_SDK" }, Values(call, "context_bias"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should accept phrases, punctuation, and more than 100 context bias entries",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_accept_phrases_punctuation_and_more_than_100_context_bias_entries()
    {
        var handler = JsonHandler();
        var contextBias = new List<string>
        {
            "AI SDK",
            "A conversation about Vercel, TypeScript, and speech.",
        };
        contextBias.AddRange(Enumerable.Range(0, 100).Select(i => "Product " + i));
        var options = new JsonObject
        {
            ["mistral"] = new JsonObject
            {
                ["contextBias"] = new JsonArray(
                    contextBias.Select(entry => (JsonNode)JsonValue.Create(entry)!).ToArray()
                ),
            },
        };

        await Transcription(CreateProvider(handler), "voxtral-mini-transcribe-3-0")
            .DoGenerateAsync(Call(options.ToJsonString()), CancellationToken.None);

        Assert.Equal(contextBias, Values(Assert.Single(handler.Calls), "context_bias"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should reject empty context bias entries",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_empty_context_bias_entries()
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Transcription(CreateProvider(JsonHandler()))
                .DoGenerateAsync(
                    Call("{\"mistral\":{\"contextBias\":[\"\"]}}"),
                    CancellationToken.None
                )
        );

        Assert.Contains("invalid mistral provider options", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass language when timestamp granularities are not set",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_language_when_timestamp_granularities_are_not_set()
    {
        var handler = JsonHandler();

        await Transcription(CreateProvider(handler))
            .DoGenerateAsync(Call("{\"mistral\":{\"language\":\"en\"}}"), CancellationToken.None);

        Assert.Equal("en", Field(Assert.Single(handler.Calls), "language"));
    }

    [Theory]
    [InlineData("voxtral-mini-2507")]
    [InlineData("voxtral-mini-2602")]
    [InlineData("voxtral-mini-latest")]
    [UpstreamTest(
        Id + "should reject language combined with timestamps for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [UpstreamTest(
        Id + "should reject language combined with timestamp granularities",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream renamed this test and runs it for each legacy model id."
    )]
    public async Task Should_reject_language_combined_with_timestamps_for(string legacyModelId)
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Transcription(CreateProvider(JsonHandler()), legacyModelId)
                .DoGenerateAsync(
                    Call(
                        "{\"mistral\":{\"language\":\"en\",\"timestampGranularities\":[\"segment\"]}}"
                    ),
                    CancellationToken.None
                )
        );

        Assert.Equal("providerOptions", error.Parameter);
    }

    [Theory]
    [InlineData("voxtral-mini-transcribe-4-0")]
    [InlineData("voxtral-small-transcribe-3-1")]
    [InlineData("custom-transcription-model")]
    [UpstreamTest(
        Id + "should allow language combined with timestamps for unrecognized model %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_allow_language_combined_with_timestamps_for_unrecognized_model(
        string unknownModelId
    )
    {
        var handler = JsonHandler();

        await Transcription(CreateProvider(handler), unknownModelId)
            .DoGenerateAsync(
                Call(
                    "{\"mistral\":{\"language\":\"en\",\"timestampGranularities\":[\"segment\"]}}"
                ),
                CancellationToken.None
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal(unknownModelId, Field(call, "model"));
        Assert.Equal("en", Field(call, "language"));
        Assert.Equal("segment", Field(call, "timestamp_granularities"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should request and return the detected language",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_request_and_return_the_detected_language()
    {
        var body = (JsonObject)JsonNode.Parse(TranscriptionResponse)!;
        body["language"] = "en";
        var handler = JsonHandler(body.ToJsonString());

        var result = await Transcription(CreateProvider(handler), "voxtral-mini-transcribe-3-0")
            .DoGenerateAsync(
                Call("{\"mistral\":{\"returnLanguage\":true}}"),
                CancellationToken.None
            );

        Assert.Equal("true", Field(Assert.Single(handler.Calls), "return_language"));
        Assert.Equal("en", result.Language);
    }

    [Fact]
    [UpstreamTest(
        Id + "should reject language prediction when the language is pinned",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_language_prediction_when_the_language_is_pinned()
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Transcription(CreateProvider(JsonHandler()), "voxtral-mini-transcribe-3-0")
                .DoGenerateAsync(
                    Call("{\"mistral\":{\"language\":\"en\",\"returnLanguage\":true}}"),
                    CancellationToken.None
                )
        );

        Assert.Contains(
            "providerOptions.mistral.language cannot be combined with providerOptions.mistral.returnLanguage",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass headers, abort signal, and the Mistral user agent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_headers_abort_signal_and_the_Mistral_user_agent()
    {
        using var abort = new CancellationTokenSource();
        var forwarded = false;
        var handler = new ParityHandler(call =>
        {
            abort.Cancel();
            forwarded = call.CancellationToken.IsCancellationRequested;
            return ParityHandler.Json(TranscriptionResponse);
        });
        var provider = CreateProvider(handler, options =>
            options.Headers["Custom-Provider-Header"] = "provider-header-value"
        );

        await Transcription(provider)
            .DoGenerateAsync(
                Call(
                    headers: new Dictionary<string, string>
                    {
                        ["Custom-Request-Header"] = "request-header-value",
                    },
                    cancellationToken: abort.Token
                ),
                abort.Token
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal("Bearer test-api-key", call.Header("authorization"));
        Assert.StartsWith("multipart/form-data; boundary=", call.Header("content-type"));
        Assert.Equal("provider-header-value", call.Header("custom-provider-header"));
        Assert.Equal("request-header-value", call.Header("custom-request-header"));
        Assert.Contains("ai-sdk/mistral/" + AiSdkVersion.Version, call.Header("user-agent"));
        Assert.True(forwarded);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use a custom base URL and fetch implementation",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The custom fetch maps to the HttpClient handler passed to MistralProvider.Create."
    )]
    public async Task Should_use_a_custom_base_URL_and_fetch_implementation()
    {
        var handler = JsonHandler();
        var provider = CreateProvider(handler, options =>
            options.BaseUrl = "https://custom.mistral.example/v2/"
        );

        await Transcription(provider).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(
            "https://custom.mistral.example/v2/audio/transcriptions",
            Assert.Single(handler.Calls).Uri.AbsoluteUri
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map transcript fields, usage, diarization, and response metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_transcript_fields_usage_diarization_and_response_metadata()
    {
        var handler = JsonHandler(null, ("x-request-id", "test-request-id"));
        var testDate = new DateTime(0, DateTimeKind.Utc);
        var model = new Vercel.AI.Mistral.MistralTranscriptionModel(
            CreateProvider(handler),
            ModelId,
            () => testDate
        );

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal((string?)Fixture["text"], result.Text);
        Assert.Null(result.Language);
        var fixtureSegments = Fixture["segments"]!.AsArray();
        Assert.Equal(fixtureSegments.Count, result.Segments.Count);
        for (var i = 0; i < fixtureSegments.Count; i++)
        {
            Assert.Equal((string?)fixtureSegments[i]!["text"], result.Segments[i].Text);
            Assert.Equal((double)fixtureSegments[i]!["start"]!, result.Segments[i].StartSecond);
            Assert.Equal((double)fixtureSegments[i]!["end"]!, result.Segments[i].EndSecond);
        }

        Assert.Equal(36, result.DurationInSeconds);
        Assert.True(
            JsonNode.DeepEquals(Fixture["usage"], JsonNode.Parse(result.Usage!.Value.GetRawText()))
        );
        Assert.Empty(result.Warnings);
        Assert.Equal(testDate, result.Response.Timestamp);
        Assert.Equal("voxtral-mini-latest", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
        Assert.Equal("application/json", result.Response.Headers["content-type"]);
        Assert.True(result.Response.Headers.ContainsKey("content-length"));
        Assert.True(
            JsonNode.DeepEquals(Fixture, JsonNode.Parse(result.Response.Body!.Value.GetRawText()))
        );

        var expectedSegments = new JsonArray();
        foreach (var segment in fixtureSegments)
        {
            expectedSegments.Add(
                new JsonObject
                {
                    ["type"] = (string?)segment!["type"],
                    ["text"] = (string?)segment["text"],
                    ["startSecond"] = (double)segment["start"]!,
                    ["endSecond"] = (double)segment["end"]!,
                    ["speakerId"] = (string?)segment["speaker_id"],
                }
            );
        }

        var expected = new JsonObject
        {
            ["mistral"] = new JsonObject
            {
                ["usage"] = new JsonObject
                {
                    ["promptTokens"] = 13,
                    ["completionTokens"] = 151,
                    ["totalTokens"] = 164,
                    ["promptAudioSeconds"] = 36,
                    ["requestCount"] = 1,
                },
                ["segments"] = expectedSegments,
            },
        };
        Assert.True(
            JsonNode.DeepEquals(
                expected,
                JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle nullable response fields and use the last segment for duration",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_handle_nullable_response_fields_and_use_the_last_segment_for_duration()
    {
        var handler = JsonHandler(
            "{\"model\":\"voxtral-mini-transcribe-2602\",\"text\":\"Hello.\",\"language\":null,\"segments\":[{\"text\":\"Hello.\",\"start\":0,\"end\":2.5,\"score\":null,\"speaker_id\":null}],\"usage\":null}"
        );

        var result = await Transcription(CreateProvider(handler))
            .DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Null(result.Language);
        Assert.Equal(2.5, result.DurationInSeconds);
        Assert.Null(result.ProviderMetadata);
    }
}
