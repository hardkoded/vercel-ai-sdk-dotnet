// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt.PromptTestSupport;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelPrompt &gt; user message &gt; image parts</c>.</summary>
public sealed class UserMessageImagePartsTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelPrompt > user message > image parts::";

    private static async Task AssertImageIsDownloadedAsync()
    {
        var result = await ConvertAsync(
            [
                new UserModelMessage([
                    new FileContentPart("image", "https://example.com/image.png", null, null),
                ]),
            ],
            NoUrls,
            DownloadOf("https://example.com/image.png", [0, 1, 2, 3], "image/png")
        );

        var file = OnlyFile(result);
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal(new byte[] { 0, 1, 2, 3 }, file.Data);
        Assert.Null(file.Url);
    }

    // The C# part carries the URL as a string, so the URL-object and string-URL cases of upstream share one shape.
    [Fact]
    [UpstreamTest(
        Id
            + "should download images for user image parts with URLs when model does not support image URLs",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_download_images_for_user_image_parts_with_URLs_when_model_does_not_support_image_URLs() =>
        AssertImageIsDownloadedAsync();

    [Fact]
    [UpstreamTest(
        Id
            + "should download images for user image parts with string URLs when model does not support image URLs",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_download_images_for_user_image_parts_with_string_URLs_when_model_does_not_support_image_URLs() =>
        AssertImageIsDownloadedAsync();

    [Fact]
    [UpstreamTest(
        Id + "should pass through provider reference for image parts without conversion",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_provider_reference_for_image_parts_without_conversion()
    {
        var reference = new Dictionary<string, string>
        {
            ["openai"] = "file-abc123",
            ["anthropic"] = "file-xyz789",
        };

        var result = await ConvertAsync([
            new UserModelMessage([FileContentPart.FromProviderReference("image/png", reference)]),
        ]);

        var file = OnlyFile(result);
        Assert.Equal("image/png", file.MediaType);
        Assert.Null(file.FileName);
        Assert.Equal(reference, file.ProviderReference);
        Assert.Null(file.Url);
        Assert.Null(file.Data);
    }
}
