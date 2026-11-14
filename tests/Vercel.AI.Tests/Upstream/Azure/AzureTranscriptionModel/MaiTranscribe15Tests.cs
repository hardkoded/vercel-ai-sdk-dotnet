// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionModel;

/// <summary>Port of <c>azure-transcription-model.test.ts</c> &gt; <c>MAI-Transcribe-1.5</c>.</summary>
public sealed class MaiTranscribe15Tests
{
    private const string Prefix = "packages/azure/src/azure-transcription-model.test.ts::MAI-Transcribe-1.5::";

    [Fact]
    [UpstreamTest(Prefix + "omits the segment timestamps default", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Omits_the_segment_timestamps_default()
    {
        var harness = Setup();

        await harness.Transcription("mai-transcribe-1.5").DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson("{\"enhancedMode\":{\"enabled\":true,\"model\":\"MAI-Transcribe-1.5\",\"modelOptions\":{}}}", harness.Definition);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends explicit options unchanged", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Sends_explicit_options_unchanged()
    {
        var harness = Setup();

        await harness.Transcription("mai-transcribe-1.5").DoGenerateAsync(
            Call("{\"azure\":{\"timestamps\":\"word\",\"transcribeStyle\":\"verbatim\",\"locales\":[\"en\"],\"phraseList\":{\"phrases\":[\"STS-34\"]}}}"),
            CancellationToken.None);

        AssertJson(
            "{\"enhancedMode\":{\"enabled\":true,\"model\":\"MAI-Transcribe-1.5\",\"modelOptions\":{\"timestamps\":\"word\",\"transcribeStyle\":\"verbatim\"}},"
            + "\"locales\":[\"en\"],\"phraseList\":{\"phrases\":[\"STS-34\"]}}",
            harness.Definition);
    }

    [Fact]
    [UpstreamTest(Prefix + "surfaces Azure rejections of unsupported options", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Surfaces_Azure_rejections_of_unsupported_options()
    {
        const string message = "timestamps='word' is not supported by MAI transcription model 'MAI-Transcribe-1.5'.";
        var harness = Setup();
        harness.Handler.Responder = _ => Json(new JsonObject { ["code"] = "InvalidRequest", ["message"] = message }.ToJsonString(), HttpStatusCode.BadRequest);

        var error = await Assert.ThrowsAnyAsync<ApiException>(() => harness.Transcription("mai-transcribe-1.5").DoGenerateAsync(
            Call("{\"azure\":{\"timestamps\":\"word\"}}"),
            CancellationToken.None));

        Assert.Equal(message, error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps a recorded MAI-Transcribe-1.5 result to one timed segment", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Maps_a_recorded_MAI_Transcribe_1_5_result_to_one_timed_segment()
    {
        var body = JsonNode.Parse(Fixture("azure-speech-mai-transcribe-1-5.1"))!;
        var harness = Setup(body: body);

        var result = await harness.Transcription("mai-transcribe-1.5").DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(body["combinedPhrases"]![0]!["text"]!.GetValue<string>(), result.Text);
        Assert.Equal("en", result.Language);
        Assert.Equal(36.71, result.DurationInSeconds!.Value, 6);
        var segment = Assert.Single(result.Segments);
        Assert.Equal(body["phrases"]![0]!["text"]!.GetValue<string>(), segment.Text);
        Assert.Equal(0, segment.StartSecond, 6);
        Assert.Equal(36.712, segment.EndSecond, 6);
        Assert.Equal("mai-transcribe-1.5", result.Response.ModelId);
        Assert.True(JsonNode.DeepEquals(body, JsonNode.Parse(result.Response.Body!.Value.GetRawText())));
    }
}
