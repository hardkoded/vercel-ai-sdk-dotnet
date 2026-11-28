// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c>
/// (the tests outside a nested <c>describe</c>).
/// </summary>
public sealed class DoStreamTests
{
    private const string Prefix =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream::";

    private static LanguageModelCallOptions Options(string providerOptions)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.ProviderOptions = GoogleUpstream.ProviderOptions(providerOptions);
        return options;
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"application\":\"example\",\"safety_identifier\":\"opaque_user_123\"}")]
    [UpstreamTest(
        Prefix + "forwards labels %j on streaming requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = "a210a29f9b1be06d87e9e880840419defecbee2b"
    )]
    public async Task Forwards_labels_on_streaming_requests(string labels)
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("basic");

        await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler),
            Options("{\"google\":{\"labels\":" + labels + ",\"store\":false}}")
        );

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(labels), body["labels"]));
        Assert.False((bool)body["store"]!);
        Assert.True((bool)body["stream"]!);
    }

    [Theory]
    [InlineData("undefined")]
    [InlineData("null")]
    [UpstreamTest(
        Prefix + "omits labels set to %j on streaming requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = "a210a29f9b1be06d87e9e880840419defecbee2b"
    )]
    public async Task Omits_labels_set_to_on_streaming_requests(string labels)
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("basic");
        var providerOptions =
            labels == "null" ? "{\"google\":{\"labels\":null}}" : "{\"google\":{}}";

        await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler),
            Options(providerOptions)
        );

        Assert.False(JsonNode.Parse(handler.Bodies[0])!.AsObject().ContainsKey("labels"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps top-level reasoning on streaming requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8e60cb4ba35426e7f4a21e341a5e16f37a478a5b"
    )]
    public async Task Maps_top_level_reasoning_on_streaming_requests()
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("basic");
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.Reasoning = "xhigh";

        var parts = await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler, "gemini-4-hydrogen"),
            options
        );

        Assert.Equal(
            "xhigh",
            (string?)JsonNode.Parse(handler.Bodies[0])!["generation_config"]!["thinking_level"]
        );
        var start = Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Empty(start.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes video response format controls and streams inline video data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Serializes_video_response_format_controls_and_streams_inline_video_data()
    {
        var handler = new SequencedHandler().Sse(
            GoogleInteractionsStreamSupport.Sse(
                new[]
                {
                    "{\"event_type\":\"interaction.created\",\"interaction\":{\"id\":\"v1_video\",\"status\":\"in_progress\",\"model\":\"gemini-omni-flash-preview\"}}",
                    "{\"event_type\":\"step.start\",\"index\":0,\"step\":{\"type\":\"model_output\"}}",
                    "{\"event_type\":\"step.delta\",\"index\":0,\"delta\":{\"type\":\"video\",\"mime_type\":\"video/mp4\",\"data\":\"AAAAIGZ0eXBpc29t\"}}",
                    "{\"event_type\":\"step.stop\",\"index\":0}",
                    "{\"event_type\":\"interaction.completed\",\"interaction\":{\"id\":\"v1_video\",\"status\":\"completed\"}}",
                }.Select(json => JsonNode.Parse(json)!.AsObject())
            )
        );
        var options = Options(
            "{\"google\":{\"responseModalities\":[\"video\"],\"responseFormat\":[{\"type\":\"video\",\"aspectRatio\":\"9:16\",\"resolution\":\"4k\",\"duration\":\"8s\",\"delivery\":\"inline\"}]}}"
        );
        options.IncludeRawChunks = false;

        var parts = await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler),
            options
        );

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        GoogleUpstream.JsonEqual(
            body["response_format"],
            "[{\"type\":\"video\",\"aspect_ratio\":\"9:16\",\"resolution\":\"4k\",\"duration\":\"8s\",\"delivery\":\"inline\"}]"
        );
        Assert.Contains(parts, part =>
            part is FileStreamPart file
            && file.MediaType == "video/mp4"
            && file.Data != null
            && file.Data.SequenceEqual(Convert.FromBase64String("AAAAIGZ0eXBpc29t"))
        );
    }
}
