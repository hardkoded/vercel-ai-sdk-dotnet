// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt.PromptTestSupport;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelPrompt &gt; user message</c>.</summary>
public sealed class UserMessageTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelPrompt > user message::";

    [Fact]
    [UpstreamTest(
        Id + "should download files when intermediate file cannot be downloaded",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_download_files_when_intermediate_file_cannot_be_downloaded()
    {
        const string imageUrlA = "http://example.com/my-image-A.png"; // supported
        const string fileUrl = "http://127.0.0.1:3000/file"; // unsupported
        const string imageUrlB = "http://example.com/my-image-B.png"; // supported
        byte[] pngA = [137, 80, 78, 71, 13, 10, 26, 10, 0];
        byte[] pngB = [137, 80, 78, 71, 13, 10, 26, 10, 1];

        var result = await ConvertAsync(
            [
                new UserModelMessage([
                    new FileContentPart("image/png", imageUrlA, null, null),
                    new FileContentPart("application/octet-stream", fileUrl, null, null),
                    new FileContentPart("image/png", imageUrlB, null, null),
                ]),
            ],
            new Dictionary<string, string> { ["*"] = "^https://.*$" },
            _ =>
                Task.FromResult<IReadOnlyList<DownloadResult>>([
                    new DownloadResult(pngA, "image/png"),
                    null!,
                    new DownloadResult(pngB, "image/png"),
                ])
        );

        var content = Assert.IsType<UserModelMessage>(Assert.Single(result)).Content;
        Assert.Equal(3, content.Count);
        var first = Assert.IsType<FileContentPart>(content[0]);
        Assert.Equal(pngA, first.Data);
        Assert.Equal("image/png", first.MediaType);
        var middle = Assert.IsType<FileContentPart>(content[1]);
        Assert.Equal(fileUrl, middle.Url);
        Assert.Null(middle.Data);
        Assert.Equal("application/octet-stream", middle.MediaType);
        var last = Assert.IsType<FileContentPart>(content[2]);
        Assert.Equal(pngB, last.Data);
        Assert.Equal("image/png", last.MediaType);
    }
}
