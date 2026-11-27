// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexTranscriptionModel.GoogleVertexTranscriptionModelFixture;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexTranscriptionModel;

/// <summary>Port of <c>google-vertex-transcription-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/google-vertex/src/google-vertex-transcription-model.test.ts::doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send the model, languageCodes, features and base64 content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_model_languageCodes_features_and_base64_content()
    {
        var handler = Server();
        await Model(handler).DoGenerateAsync(Call(), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            JsonDocument.Parse(handler.Body).RootElement,
            "{\"config\":{\"model\":\"chirp_2\",\"languageCodes\":[\"auto\"],\"autoDecodingConfig\":{},\"features\":{\"enableWordTimeOffsets\":true,\"enableAutomaticPunctuation\":true}},\"content\":\""
                + AudioBase64
                + "\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should target the regional Speech-to-Text endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_target_the_regional_Speech_to_Text_endpoint()
    {
        var handler = Server();
        await Model(handler).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(Url, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract text, segments, language and duration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_text_segments_language_and_duration()
    {
        var result = await Model(Server()).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal("hello world", result.Text);
        Assert.Collection(
            result.Segments,
            segment =>
            {
                Assert.Equal("hello", segment.Text);
                Assert.Equal(0, segment.StartSecond);
                Assert.Equal(0.5, segment.EndSecond);
            },
            segment =>
            {
                Assert.Equal("world", segment.Text);
                Assert.Equal(0.5, segment.StartSecond);
                Assert.Equal(1, segment.EndSecond);
            }
        );
        // BCP-47 `en-US` is reduced to the ISO-639-1 `en`.
        Assert.Equal("en", result.Language);
        Assert.Equal(1, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept a base64 string audio input directly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_a_base64_string_audio_input_directly()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(Call(audioBase64: AudioBase64), CancellationToken.None);

        Assert.Equal(
            AudioBase64,
            JsonDocument.Parse(handler.Body).RootElement.GetProperty("content").GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass languageCodes and features from provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_languageCodes_and_features_from_provider_options()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(
                Call(
                    providerOptions: "{\"googleVertex\":{\"languageCodes\":[\"en-US\",\"fr-FR\"],\"enableAutomaticPunctuation\":false,\"enableWordTimeOffsets\":false}}"
                ),
                CancellationToken.None
            );

        var config = JsonDocument.Parse(handler.Body).RootElement.GetProperty("config");
        GoogleUpstream.JsonEqual(config.GetProperty("languageCodes"), "[\"en-US\",\"fr-FR\"]");
        GoogleUpstream.JsonEqual(
            config.GetProperty("features"),
            "{\"enableAutomaticPunctuation\":false,\"enableWordTimeOffsets\":false}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept the legacy vertex provider options key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_the_legacy_vertex_provider_options_key()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(
                Call(providerOptions: "{\"vertex\":{\"region\":\"us\"}}"),
                CancellationToken.None
            );

        Assert.Equal(UsUrl, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept the google provider options key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_the_google_provider_options_key()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(
                Call(providerOptions: "{\"google\":{\"region\":\"us\"}}"),
                CancellationToken.None
            );

        Assert.Equal(UsUrl, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer googleVertex provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_googleVertex_provider_options()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(
                Call(
                    providerOptions: "{\"googleVertex\":{\"region\":\"us\"},\"vertex\":{\"region\":\"eu\"},\"google\":{\"region\":\"asia-southeast1\"}}"
                ),
                CancellationToken.None
            );

        Assert.Equal(UsUrl, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should route to a region override from provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_route_to_a_region_override_from_provider_options()
    {
        var handler = Server();
        await Model(handler)
            .DoGenerateAsync(
                Call(providerOptions: "{\"googleVertex\":{\"region\":\"us\"}}"),
                CancellationToken.None
            );

        Assert.Equal(UsUrl, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use the unprefixed host for the global location",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_the_unprefixed_host_for_the_global_location()
    {
        var handler = Server();
        await Model(handler, "latest_long")
            .DoGenerateAsync(
                Call(providerOptions: "{\"googleVertex\":{\"region\":\"global\"}}"),
                CancellationToken.None
            );

        Assert.Equal(GlobalUrl, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass headers and user agent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_headers_and_user_agent()
    {
        var handler = Server();
        await Model(handler, configure: options =>
                options.Headers["Custom-Provider-Header"] = "provider-header-value"
            )
            .DoGenerateAsync(
                Call(
                    headers: new Dictionary<string, string>
                    {
                        ["Custom-Request-Header"] = "request-header-value",
                    }
                ),
                CancellationToken.None
            );

        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/google", handler.RequestHeaders["User-Agent"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return empty text and segments when there are no results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_empty_text_and_segments_when_there_are_no_results()
    {
        var result = await Model(Server("{}")).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(string.Empty, result.Text);
        Assert.Empty(result.Segments);
        Assert.Null(result.Language);
        Assert.Null(result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit segments without word time offsets",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_segments_without_word_time_offsets()
    {
        var result = await Model(
                Server(
                    "{\"results\":[{\"alternatives\":[{\"transcript\":\"hello world\",\"words\":[{\"word\":\"hello\"},{\"word\":\"world\"}]}],\"languageCode\":\"en-US\"}]}"
                )
            )
            .DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Empty(result.Segments);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should canonicalize detected languages to ISO-639-1",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_canonicalize_detected_languages_to_ISO_639_1()
    {
        var result = await Model(
                Server(
                    "{\"results\":[{\"alternatives\":[{\"transcript\":\"你好\"}],\"languageCode\":\"cmn-Hans-CN\"}]}"
                )
            )
            .DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal("zh", result.Language);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit detected languages without an ISO-639-1 code",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_detected_languages_without_an_ISO_639_1_code()
    {
        var result = await Model(
                Server(
                    "{\"results\":[{\"alternatives\":[{\"transcript\":\"hello world\"}],\"languageCode\":\"ast-ES\"}]}"
                )
            )
            .DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Null(result.Language);
    }

    [Fact]
    [UpstreamTest(Prefix + "should parse billed duration", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_parse_billed_duration()
    {
        var result = await Model(
                Server(
                    "{\"results\":[{\"alternatives\":[{\"transcript\":\"hello world\"}],\"languageCode\":\"en-US\"}],\"metadata\":{\"totalBilledDuration\":\"1.250s\"}}"
                )
            )
            .DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(1.25, result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include response data with timestamp, modelId and headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_response_data_with_timestamp_modelId_and_headers()
    {
        var testDate = DateTimeOffset.FromUnixTimeMilliseconds(0);
        var handler = Server();
        handler.ResponseHeaders["x-request-id"] = "test-request-id";

        var result = await Model(handler, now: testDate)
            .DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("chirp_2", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
    }
}
