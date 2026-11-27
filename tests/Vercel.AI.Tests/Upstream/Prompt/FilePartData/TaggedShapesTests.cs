// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Prompt.FilePartData;

/// <summary>Port of <c>file-part-data.test.ts</c> &gt; <c>convertToLanguageModelV4FilePart</c> &gt; <c>tagged shapes</c>.</summary>
public sealed class TaggedShapesTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::preserves the original opaque URI in a tagged URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_the_original_opaque_URI_in_a_tagged_URL()
    {
        const string originalUrl = "gs://my-bucket/folder/My File.pdf";
        var url = new Uri(originalUrl.Replace(" ", "%20", StringComparison.Ordinal));

        var result = FileParts.ConvertToLanguageModelV4FilePart(
            FilePartInput.TaggedUrl(url, originalUrl)
        );

        Assert.Equal("url", result.DataType);
        Assert.Same(url, result.Url);
        Assert.Equal(originalUrl, result.OriginalUrl);
        Assert.Null(result.MediaType);
    }
}
