// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.BedrockStreamSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.ConvertToAmazonBedrockChatMessages;

public sealed class UserMessagesTests
{
    private const string Suite = "packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::";

    [Fact]
    [UpstreamTest(Suite + "should convert text part to guardContent when guardContent provider option is true", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_text_part_to_guardContent_when_guardContent_provider_option_is_true()
    {
        AssertText("Grounding text", """{"amazonBedrock":{"guardContent":true}}""", """{"guardContent":{"text":{"text":"Grounding text"}}}""");
    }

    [Fact]
    [UpstreamTest(Suite + "should convert text part to guardContent with qualifiers", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_text_part_to_guardContent_with_qualifiers()
    {
        AssertText(
            "Grounding text",
            """{"amazonBedrock":{"guardContent":true,"guardContentQualifiers":["grounding_source"]}}""",
            """{"guardContent":{"text":{"text":"Grounding text","qualifiers":["grounding_source"]}}}""");
    }

    [Fact]
    [UpstreamTest(Suite + "should convert text part to guardContent with multiple qualifiers", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_text_part_to_guardContent_with_multiple_qualifiers()
    {
        AssertText(
            "Query text",
            """{"amazonBedrock":{"guardContent":true,"guardContentQualifiers":["grounding_source","query"]}}""",
            """{"guardContent":{"text":{"text":"Query text","qualifiers":["grounding_source","query"]}}}""");
    }

    [Fact]
    [UpstreamTest(Suite + "should convert text part as normal text when guardContent is false", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_text_part_as_normal_text_when_guardContent_is_false()
    {
        AssertText("Normal text", """{"amazonBedrock":{"guardContent":false}}""", """{"text":"Normal text"}""");
    }

    [Fact]
    [UpstreamTest(Suite + "should convert text part as normal text when no provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_text_part_as_normal_text_when_no_provider_options()
    {
        AssertText("Normal text", null, """{"text":"Normal text"}""");
    }

    [Fact]
    [UpstreamTest(Suite + "should convert image part to guardContent when guardContent provider option is true", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_part_to_guardContent_when_guardContent_provider_option_is_true()
    {
        AssertImage("""{"amazonBedrock":{"guardContent":true}}""", """{"guardContent":{"image":{"format":"png","source":{"bytes":"AAECAw=="}}}}""");
    }

    [Fact]
    [UpstreamTest(Suite + "should convert image part as normal image when guardContent is false", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_part_as_normal_image_when_guardContent_is_false()
    {
        AssertImage("""{"amazonBedrock":{"guardContent":false}}""", """{"image":{"format":"png","source":{"bytes":"AAECAw=="}}}""");
    }

    [Fact]
    [UpstreamTest(Suite + "should convert image part as normal image when no provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_part_as_normal_image_when_no_provider_options()
    {
        AssertImage(null, """{"image":{"format":"png","source":{"bytes":"AAECAw=="}}}""");
    }

    private static void AssertText(string text, string? providerOptions, string expectedBlock)
    {
        var part = new TextContentPart(text) { ProviderOptions = providerOptions is null ? null : Options(providerOptions) };
        AssertUserContent(part, expectedBlock);
    }

    private static void AssertImage(string? providerOptions, string expectedBlock)
    {
        var part = new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null)
        {
            ProviderOptions = providerOptions is null ? null : Options(providerOptions),
        };
        AssertUserContent(part, expectedBlock);
    }

    private static void AssertUserContent(UserContentPart part, string expectedBlock)
    {
        AmazonBedrockMessages.Convert(
            ModelId,
            new ModelMessage[] { new UserModelMessage(new[] { part }) },
            out _,
            out var messages);

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("[{\"role\":\"user\",\"content\":[" + expectedBlock + "]}]"),
            messages));
    }
}
