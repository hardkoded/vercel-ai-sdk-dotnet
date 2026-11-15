// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionModel;

/// <summary>Port of <c>azure-transcription-model.test.ts</c> &gt; <c>Speech responses</c>.</summary>
public sealed class SpeechResponsesTests
{
    private const string Prefix = "packages/azure/src/azure-transcription-model.test.ts::Speech responses::";

    [Fact]
    [UpstreamTest(Prefix + "normalizes results and preserves speaker, word, locale, and confidence metadata", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Normalizes_results_and_preserves_speaker_word_locale_and_confidence_metadata()
    {
        var harness = Setup();

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal("Hello world.", result.Text);
        Assert.Equal(2, result.DurationInSeconds);
        Assert.Equal("en", result.Language);
        var segment = Assert.Single(result.Segments);
        Assert.Equal("Hello world.", segment.Text);
        Assert.Equal(0.04, segment.StartSecond, 6);
        Assert.Equal(1.54, segment.EndSecond, 6);
        Assert.True(JsonNode.DeepEquals(
            new JsonObject { ["azure"] = new JsonObject { ["phrases"] = Response["phrases"]!.DeepClone() } },
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
        Assert.Empty(result.Warnings);
        Assert.Equal("mai-transcribe-2", result.Response.ModelId);
        Assert.True(JsonNode.DeepEquals(Response, JsonNode.Parse(result.Response.Body!.Value.GetRawText())));
        Assert.NotNull(result.Response.Timestamp);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Prefix + "accepts missing or null timestamps (%s)", Coverage = UpstreamCoverage.Covered, Note = "undefined is an absent key and null is a JSON null.", Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Accepts_missing_or_null_timestamps(bool nulls)
    {
        var phrase = new JsonObject { ["text"] = "Hello world.", ["locale"] = "en-US" };
        var body = new JsonObject { ["combinedPhrases"] = new JsonArray(new JsonObject { ["text"] = "Hello world." }), ["phrases"] = new JsonArray(phrase) };
        if (nulls)
        {
            body["durationMilliseconds"] = null;
            foreach (var name in new[] { "offsetMilliseconds", "durationMilliseconds", "words", "speaker" })
            {
                phrase[name] = null;
            }
        }

        var harness = Setup(body: body);

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call("{\"azure\":{\"timestamps\":\"none\"}}"),
            CancellationToken.None);

        Assert.Equal("Hello world.", result.Text);
        Assert.Empty(result.Segments);
        Assert.Null(result.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts empty transcripts and absent phrases", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Accepts_empty_transcripts_and_absent_phrases()
    {
        var harness = Setup(body: JsonNode.Parse("{\"combinedPhrases\":[],\"phrases\":null}"));

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(string.Empty, result.Text);
        Assert.Empty(result.Segments);
        Assert.Null(result.Language);
    }

    [Theory]
    [InlineData("en-US,en-GB", "en")]
    [InlineData("en-US,de-DE", null)]
    [InlineData("fil", null)]
    [InlineData("en,fil", null)]
    [InlineData("en-US,yue", null)]
    [UpstreamTest(Prefix + "handles phrase locales %j", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Handles_phrase_locales(string locales, string? expected)
    {
        var phrases = new JsonArray();
        foreach (var locale in locales.Split(','))
        {
            phrases.Add(new JsonObject { ["text"] = "text", ["locale"] = locale });
        }

        var body = (JsonObject)Response.DeepClone();
        body["phrases"] = phrases;
        var harness = Setup(body: body);

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(expected, result.Language);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(429)]
    [InlineData(500)]
    [UpstreamTest(Prefix + "returns structured API errors for HTTP %s", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Returns_structured_API_errors_for_HTTP(int status)
    {
        var harness = Setup();
        harness.Handler.Responder = _ =>
        {
            var response = Json("{\"error\":{\"code\":\"InvalidRequest\",\"message\":\"Bad audio\"}}", (HttpStatusCode)status);
            response.Headers.TryAddWithoutValidation("x-request-id", "request-1");
            return response;
        };

        var error = await Assert.ThrowsAnyAsync<ApiException>(() => harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Equal("Bad audio", error.Message);
        Assert.Equal(status, error.StatusCode);
        Assert.Equal(status != 400, error.IsRetryable);
        Assert.Equal("request-1", error.ResponseHeaders!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves non-JSON error bodies", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Preserves_non_JSON_error_bodies()
    {
        var harness = Setup();
        harness.Handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("Unavailable") };

        var error = await Assert.ThrowsAnyAsync<ApiException>(() => harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Equal("Unavailable", error.ResponseBody);
        Assert.Equal(503, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Prefix + "reads the top-level error message documented by the Speech REST API", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Reads_the_top_level_error_message_documented_by_the_Speech_REST_API()
    {
        var harness = Setup();
        harness.Handler.Responder = _ => Json("{\"code\":\"InvalidRequest\",\"message\":\"Bad audio\"}", HttpStatusCode.BadRequest);

        var error = await Assert.ThrowsAnyAsync<ApiException>(() => harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Equal("Bad audio", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects malformed success responses", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Rejects_malformed_success_responses()
    {
        var harness = Setup(body: JsonNode.Parse("{\"unrelated\":true}"));

        await Assert.ThrowsAnyAsync<ApiException>(() => harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves abort errors", Coverage = UpstreamCoverage.Covered, Note = "A cancelled call surfaces as ApiUserAbortException, an OperationCanceledException, not as an API error or a timeout.", Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Preserves_abort_errors()
    {
        var harness = Setup();
        using var cancellation = new CancellationTokenSource();
        harness.Handler.Responder = _ =>
        {
            cancellation.Cancel();
            throw new OperationCanceledException(cancellation.Token);
        };

        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call(cancellationToken: cancellation.Token),
            cancellation.Token));

        Assert.IsType<ApiUserAbortException>(error);
    }
}
