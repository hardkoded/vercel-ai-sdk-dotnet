// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of the tests of <c>convert-to-language-model-prompt.test.ts</c> that are outside any <c>describe</c>.</summary>
public sealed class ConvertToLanguageModelPromptTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::";

    [Fact]
    [UpstreamTest(
        Id + "normalizes a legacy image data URL like a replacement file part",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c"
    )]
    public async Task Normalizes_a_legacy_image_data_URL_like_a_replacement_file_part()
    {
        const string url = "data:image/png;base64,iVBORw0KGgo=";
        var output = new UserContentPart[] { new ImageUrlContentPart(url) };
        var downloadCalls = new List<IReadOnlyList<DownloadRequestItem>>();

        var downloadedAssets = await LanguageModelPrompts.DownloadAssetsAsync(
            new ModelMessage[] { new ToolModelMessage("call", "asset", output) },
            requests =>
            {
                downloadCalls.Add(requests);
                return Task.FromResult<IReadOnlyList<DownloadResult>>(
                    Array.Empty<DownloadResult>()
                );
            },
            new Dictionary<string, string>()
        );

        Assert.Empty(Assert.Single(downloadCalls));
        var file = Assert.IsType<FileContentPart>(
            Assert.Single(LanguageModelPrompts.MapToolResultOutput(output, downloadedAssets))
        );
        Assert.Equal(Convert.FromBase64String("iVBORw0KGgo="), file.Data);
        Assert.Equal("image/png", file.MediaType);
    }

    [Theory]
    [InlineData("image-url", null, "image/png")]
    [InlineData("file-url", "application/pdf", "application/pdf")]
    [UpstreamTest(
        Id + "resolves the concrete media type of downloaded legacy %s tool content",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c"
    )]
    public void Resolves_the_concrete_media_type_of_downloaded_legacy_tool_content(
        string type,
        string? downloadedMediaType,
        string expectedMediaType
    )
    {
        const string url = "https://example.com/asset";
        var data = type == "image-url"
            ? new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }
            : new byte[] { 37, 80, 68, 70 };
        UserContentPart item = type == "file-url"
            ? new FileUrlContentPart(url, "application")
            : new ImageUrlContentPart(url);

        var mapped = LanguageModelPrompts.MapToolResultOutput(
            new[] { item },
            new Dictionary<string, DownloadResult>
            {
                [url] = new DownloadResult(data, downloadedMediaType),
            }
        );

        var file = Assert.IsType<FileContentPart>(Assert.Single(mapped));
        Assert.Equal(data, file.Data);
        Assert.Null(file.Url);
        Assert.Equal(expectedMediaType, file.MediaType);
        Assert.Null(file.ProviderOptions);
    }
}
