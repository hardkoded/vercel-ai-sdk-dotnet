// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream;
using MiniMaxModel = Vercel.AI.MiniMax.MiniMaxChatLanguageModel;

namespace Vercel.AI.Tests.MiniMaxChatLanguageModel;

/// <summary>Port of <c>minimax-chat-language-model.test.ts</c> &gt; <c>MiniMax M3.1 Flash media input</c>.</summary>
public sealed class MiniMaxM31FlashMediaInputTests
{
    private static readonly byte[] Bytes = { 1, 2, 3 };

    private readonly MiniMaxServer _server = new MiniMaxServer();
    private readonly MiniMaxModel _model;

    public MiniMaxM31FlashMediaInputTests()
    {
        _model = MiniMaxParity.Provider(_server).Chat("minimax-m3.1-flash");
        _server.SetJson("{\"type\":\"message\",\"content\":[{\"type\":\"text\",\"text\":\"A cat.\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":10,\"output_tokens\":3}}");
    }

    [Theory]
    [InlineData("data-bytes")]
    [InlineData("data-base64")]
    [InlineData("url")]
    [InlineData("reference")]
    [InlineData("reference-with-prefix")]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::converts video $data.type sources", Coverage = UpstreamCoverage.Partial, Note = "The generate result has no request info, so only the request the server received is checked. The port has no base64 string input at the model level, so data-base64 sends the same bytes as data-bytes.")]
    public async Task Converts_video_data_type_sources(string kind)
    {
        FileContentPart file;
        string source;
        switch (kind)
        {
            case "url":
                file = MiniMaxParity.FileUrl("video/mp4", "https://example.com/video.mp4", "{\"fps\":1}");
                source = "{\"type\":\"url\",\"url\":\"https://example.com/video.mp4\",\"fps\":1}";
                break;
            case "reference":
                file = MiniMaxParity.FileReference("video/mp4", "minimax", "123", "{\"fps\":1}");
                source = "{\"type\":\"url\",\"url\":\"mm_file://123\",\"fps\":1}";
                break;
            case "reference-with-prefix":
                file = MiniMaxParity.FileReference("video/mp4", "minimax", "mm_file://123", "{\"fps\":1}");
                source = "{\"type\":\"url\",\"url\":\"mm_file://123\",\"fps\":1}";
                break;
            default:
                file = MiniMaxParity.File("video/mp4", Bytes, "{\"fps\":1}");
                source = "{\"type\":\"base64\",\"media_type\":\"video/mp4\",\"data\":\"AQID\",\"fps\":1}";
                break;
        }

        var result = await _model.DoGenerateAsync(Describe(file), CancellationToken.None);

        var messages = "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Describe.\"},{\"type\":\"video\",\"source\":" + source + "}]}]";
        MiniMaxParity.AssertMatch(_server.Calls[0].Body, "{\"messages\":" + messages + "}");
        Assert.Contains(result.Content, content => content is GeneratedText text && text.Text == "A cat.");
        Assert.False(_server.Calls[0].Headers.ContainsKey("anthropic-beta"));
    }

    [Theory]
    [InlineData("low")]
    [InlineData("default")]
    [InlineData("high")]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::forwards image detail %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_image_detail(string detail)
    {
        await _model.DoGenerateAsync(Describe(MiniMaxParity.File("image/png", Bytes, "{\"detail\":\"" + detail + "\"}")), CancellationToken.None);

        JsonAssert.Equal(
            _server.Calls[0].Body["messages"]![0]!["content"]![1],
            "{\"type\":\"image\",\"source\":{\"type\":\"base64\",\"media_type\":\"image/png\",\"data\":\"AQID\",\"detail\":\"" + detail + "\"}}");
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::forwards image URLs and leaves detail at the server default when omitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_image_URLs_and_leaves_detail_at_the_server_default_when_omitted()
    {
        await _model.DoGenerateAsync(Describe(MiniMaxParity.FileUrl("image/png", "https://example.com/image.png")), CancellationToken.None);

        JsonAssert.Equal(
            _server.Calls[0].Body["messages"]![0]!["content"]![1],
            "{\"type\":\"image\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/image.png\"}}");
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::leaves fps at the server default when omitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_fps_at_the_server_default_when_omitted()
    {
        await _model.DoGenerateAsync(Describe(MiniMaxParity.File("video/mp4", Bytes)), CancellationToken.None);

        var source = _server.Calls[0].Body["messages"]![0]!["content"]![1]!["source"]!.AsObject();
        Assert.False(source.ContainsKey("fps"));
    }

    [Theory]
    [InlineData(0.2)]
    [InlineData(5.0)]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::accepts boundary fps %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_boundary_fps(double fps)
    {
        var json = fps.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await _model.DoGenerateAsync(Describe(MiniMaxParity.File("video/mp4", Bytes, "{\"fps\":" + json + "}")), CancellationToken.None);

        Assert.Equal(fps, _server.Calls[0].Body["messages"]![0]!["content"]![1]!["source"]!["fps"]!.GetValue<double>());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.1")]
    [InlineData("5.1")]
    [InlineData("\"1\"")]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::rejects invalid fps %s before sending", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_fps_before_sending(string fps)
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            _model.DoGenerateAsync(Describe(MiniMaxParity.File("video/mp4", Bytes, "{\"fps\":" + fps + "}")), CancellationToken.None));

        Assert.Empty(_server.Calls);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::rejects unsupported image detail", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_image_detail()
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            _model.DoGenerateAsync(Describe(MiniMaxParity.File("image/png", Bytes, "{\"detail\":\"auto\"}")), CancellationToken.None));

        Assert.Empty(_server.Calls);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::requires a MiniMax file reference", Coverage = UpstreamCoverage.Covered)]
    public async Task Requires_a_MiniMax_file_reference()
    {
        await Assert.ThrowsAsync<NoSuchProviderReferenceError>(() =>
            _model.DoGenerateAsync(Describe(MiniMaxParity.FileReference("video/mp4", "anthropic", "file_123")), CancellationToken.None));

        Assert.Empty(_server.Calls);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::keeps media in order across merged user messages and preserves reasoning options", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_media_in_order_across_merged_user_messages_and_preserves_reasoning_options()
    {
        await _model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                MaxOutputTokens = 1000,
                Reasoning = "high",
                Prompt = new ModelMessage[]
                {
                    new SystemModelMessage("Analyze the media."),
                    new UserModelMessage(new UserContentPart[]
                    {
                        new TextContentPart("First"),
                        MiniMaxParity.File("image/png", Bytes, "{\"detail\":\"high\"}"),
                    }),
                    new UserModelMessage(new UserContentPart[]
                    {
                        MiniMaxParity.FileReference("video/mp4", "minimax", "456", "{\"fps\":2}"),
                        new TextContentPart("Last"),
                    }),
                },
            },
            CancellationToken.None);

        var body = _server.Calls[0].Body;
        JsonAssert.Equal(
            body["messages"]![0]!["content"],
            "[{\"type\":\"text\",\"text\":\"First\"},{\"type\":\"image\",\"source\":{\"type\":\"base64\",\"media_type\":\"image/png\",\"data\":\"AQID\",\"detail\":\"high\"}},{\"type\":\"video\",\"source\":{\"type\":\"url\",\"url\":\"mm_file://456\",\"fps\":2}},{\"type\":\"text\",\"text\":\"Last\"}]");
        JsonAssert.Equal(body["output_config"], "{\"effort\":\"high\"}");
        JsonAssert.Equal(body["system"], "[{\"type\":\"text\",\"text\":\"Analyze the media.\"}]");
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::keeps media separate across concurrent calls on the same model", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_media_separate_across_concurrent_calls_on_the_same_model()
    {
        await Task.WhenAll(new[] { "first", "second" }.Select(id =>
            _model.DoGenerateAsync(Describe(MiniMaxParity.FileReference("video/mp4", "minimax", id)), CancellationToken.None)));

        var urls = _server.Calls.Select(call => call.Body["messages"]![0]!["content"]![1]!["source"]!["url"]!.GetValue<string>()).OrderBy(url => url, StringComparer.Ordinal);
        Assert.Equal(new[] { "mm_file://first", "mm_file://second" }, urls);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::sends video when streaming and exposes the actual request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_video_when_streaming_and_exposes_the_actual_request_body()
    {
        _server.SetStream(
            "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_media\",\"model\":\"minimax-m3.1-flash\",\"usage\":{\"input_tokens\":10,\"output_tokens\":0}}}\n\n",
            "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":0}}\n\n",
            "data: {\"type\":\"message_stop\"}\n\n");

        var result = await _model.DoStreamAsync(
            MiniMaxParity.Media(MiniMaxParity.File("video/mp4", Bytes, "{\"fps\":1}")),
            CancellationToken.None);
        var parts = await MiniMaxParity.Collect(result);

        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        var messages = "[{\"role\":\"user\",\"content\":[{\"type\":\"video\",\"source\":{\"type\":\"base64\",\"media_type\":\"video/mp4\",\"data\":\"AQID\",\"fps\":1}}]}]";
        MiniMaxParity.AssertMatch(_server.Calls[0].Body, "{\"stream\":true,\"messages\":" + messages + "}");
        MiniMaxParity.AssertMatch(JsonNode.Parse(result.Request!.Body!.Value.GetRawText()), "{\"messages\":" + messages + "}");
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::advertises native image/video URLs for M3.1 but not M3", Coverage = UpstreamCoverage.Covered)]
    public void Advertises_native_image_video_URLs_for_M3_1_but_not_M3()
    {
        var expected = new Dictionary<string, string> { ["image/*"] = "^https?://", ["video/*"] = "^https?://" };
        Assert.Equal(expected, _model.SupportedUrls);
        Assert.Equal(_model.SupportedUrls, MiniMaxParity.Provider(_server).Chat("MiniMax-M3.1-Flash").SupportedUrls);
        Assert.Empty(MiniMaxParity.Provider(_server).Chat("minimax-m3").SupportedUrls);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::preserves media conversion after workflow serialization", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_media_conversion_after_workflow_serialization()
    {
        var restored = MiniMaxModel.Deserialize(_model.Serialize(), "test-api-key", _server);

        await restored.DoGenerateAsync(MiniMaxParity.Media(MiniMaxParity.FileReference("video/mp4", "minimax", "123", "{\"fps\":1}")), CancellationToken.None);

        JsonAssert.Equal(
            _server.Calls[0].Body["messages"]![0]!["content"],
            "[{\"type\":\"video\",\"source\":{\"type\":\"url\",\"url\":\"mm_file://123\",\"fps\":1}}]");
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax M3.1 Flash media input::does not enable video input for M3", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_enable_video_input_for_M3()
    {
        var model = MiniMaxParity.Provider(_server).Chat("minimax-m3");

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            model.DoGenerateAsync(new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { MiniMaxParity.File("video/mp4", Bytes) }) } }, CancellationToken.None));

        Assert.Empty(_server.Calls);
    }

    private static LanguageModelCallOptions Describe(UserContentPart file)
    {
        return MiniMaxParity.Media(new TextContentPart("Describe."), file);
    }
}
