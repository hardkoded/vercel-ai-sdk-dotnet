// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt.PromptRunner;

namespace Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt;

/// <summary>Port of <c>convert-to-cohere-chat-prompt.test.ts</c> &gt; <c>image processing</c>.</summary>
public sealed class ImageProcessingTests
{
    private const string Suite = Prefix + "image processing::";

    [Fact]
    [UpstreamTest(
        Suite + "should convert image file with data bytes into image_url data URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_image_file_with_data_bytes_into_image_url_data_URI()
    {
        var result = Run(
            User(new TextContentPart("What is in this image?"), File("image/png", Png))
        );

        JsonAssert.Equal(
            result,
            "{\"messages\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"What is in this image?\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AAECAw==\"}}]}],\"documents\":[],\"warnings\":[]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should convert image file with URL data into image_url URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_image_file_with_URL_data_into_image_url_URL()
    {
        var result = Run(
            User(new FileContentPart("image/png", "https://example.com/cat.png", null, null))
        );

        JsonAssert.Equal(
            result,
            "{\"messages\":[{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"https://example.com/cat.png\"}}]}],\"documents\":[],\"warnings\":[]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should pass through detail provider option as image_url.detail",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_detail_provider_option_as_image_url_detail()
    {
        var part = new FileContentPart("image/png", null, Png, null)
        {
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["cohere"] = Json("{\"detail\":\"high\"}"),
            },
        };

        var result = Run(User(part));

        JsonAssert.Equal(
            result["messages"]![0],
            "{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AAECAw==\",\"detail\":\"high\"}}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should omit detail when no provider option is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_detail_when_no_provider_option_is_set()
    {
        var result = Run(User(File("image/png", Png)));

        var message = result["messages"]![0]!;
        Assert.Equal("user", message["role"]!.GetValue<string>());
        var part = message["content"]!.AsArray()[0]!;
        Assert.Equal("image_url", part["type"]!.GetValue<string>());
        Assert.Null(part["image_url"]!["detail"]);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should send image inline and route non-image file to documents",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_send_image_inline_and_route_non_image_file_to_documents()
    {
        var result = Run(
            User(
                new TextContentPart("See attached:"),
                File("image/png", Png),
                File("text/plain", "Doc text", "note.txt")
            )
        );

        JsonAssert.Equal(
            result["messages"],
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"See attached:\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AAECAw==\"}}]}]"
        );
        JsonAssert.Equal(
            result["documents"],
            "[{\"data\":{\"text\":\"Doc text\",\"title\":\"note.txt\"}}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should accept top-level \"image\" media type and detect full type from bytes",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_top_level_image_media_type_and_detect_full_type_from_bytes()
    {
        var pngSignature = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

        var result = Run(User(File("image", pngSignature)));

        var part = result["messages"]![0]!["content"]!.AsArray()[0]!;
        Assert.Equal("image_url", part["type"]!.GetValue<string>());
        Assert.Matches("^data:image/png;base64,", part["image_url"]!["url"]!.GetValue<string>());
    }
}
