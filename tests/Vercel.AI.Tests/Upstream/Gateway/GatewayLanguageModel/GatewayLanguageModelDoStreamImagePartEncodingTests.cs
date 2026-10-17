// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doStream &gt; Image part encoding</c>.</summary>
public sealed class GatewayLanguageModelDoStreamImagePartEncodingTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Image part encoding::";

    [Fact]
    [UpstreamTest(Prefix + "should not modify prompt without image parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_modify_prompt_without_image_parts()
    {
        var server = PrepareStreamResponse("response");

        await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);

        JsonAssert.Equal(server.RequestJson["prompt"], "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should encode Uint8Array image part to inline base64 data with default mime type", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_Uint8Array_image_part_to_inline_base64_data_with_default_mime_type()
    {
        var server = PrepareStreamResponse("response");
        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new TextContentPart("Describe:"), new FileContentPart("image/jpeg", null, imageBytes, null) }) },
        };

        await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);

        var imagePart = server.RequestJson["prompt"]![0]!["content"]![1]!;
        Assert.Equal("file", (string?)imagePart["type"]);
        JsonAssert.Equal(imagePart["data"], "{\"type\":\"data\",\"data\":\"" + Convert.ToBase64String(imageBytes) + "\"}");
        Assert.Equal("image/jpeg", (string?)imagePart["mediaType"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should encode Uint8Array image part to inline base64 data with specified mime type", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_Uint8Array_image_part_to_inline_base64_data_with_specified_mime_type()
    {
        var server = PrepareStreamResponse("response");
        var imageBytes = new byte[] { 5, 6, 7, 8 };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new TextContentPart("Describe:"), new FileContentPart("image/png", null, imageBytes, null) }) },
        };

        await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);

        var imagePart = server.RequestJson["prompt"]![0]!["content"]![1]!;
        Assert.Equal("file", (string?)imagePart["type"]);
        JsonAssert.Equal(imagePart["data"], "{\"type\":\"data\",\"data\":\"" + Convert.ToBase64String(imageBytes) + "\"}");
        Assert.Equal("image/png", (string?)imagePart["mediaType"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not modify image part with URL", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_modify_image_part_with_URL()
    {
        var server = PrepareStreamResponse("response");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new TextContentPart("URL:"), new FileContentPart("image/jpeg", "https://example.com/image.jpg", null, null) }) },
        };

        await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);

        var imagePart = server.RequestJson["prompt"]![0]!["content"]![1]!;
        Assert.Equal("file", (string?)imagePart["type"]);
        JsonAssert.Equal(imagePart["data"], "{\"type\":\"url\",\"url\":\"https://example.com/image.jpg\"}");
        Assert.Equal("image/jpeg", (string?)imagePart["mediaType"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle mixed content types correctly for streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_mixed_content_types_correctly_for_streaming()
    {
        var server = PrepareStreamResponse("response");
        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new TextContentPart("First text."), new FileContentPart("image/gif", null, imageBytes, null), new TextContentPart("Second text."), new FileContentPart("image/png", "https://example.com/image2.png", null, null) }) },
        };

        await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);

        var content = server.RequestJson["prompt"]![0]!["content"]!;
        JsonAssert.Equal(content[0], "{\"type\":\"text\",\"text\":\"First text.\"}");
        JsonAssert.Equal(content[1], "{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"" + Convert.ToBase64String(imageBytes) + "\"},\"mediaType\":\"image/gif\"}");
        JsonAssert.Equal(content[2], "{\"type\":\"text\",\"text\":\"Second text.\"}");
        JsonAssert.Equal(content[3], "{\"type\":\"file\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/image2.png\"},\"mediaType\":\"image/png\"}");
    }
}
