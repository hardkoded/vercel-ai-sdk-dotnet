// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Xai.XaiImageModel.XaiImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiImageModel;

/// <summary>Port of <c>xai-image-model.test.ts</c> &gt; <c>XaiImageModel</c> &gt; <c>doGenerate</c> &gt; <c>response metadata</c>.</summary>
public sealed class ResponseMetadataTests
{
    private const string Id =
        "packages/xai/src/xai-image-model.test.ts::XaiImageModel > doGenerate > response metadata::";

    [Fact]
    [UpstreamTest(
        Id + "should include timestamp, headers and modelId in response",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_include_timestamp_headers_and_modelId_in_response()
    {
        var testDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var model = CreateModel(Server(), currentDate: () => testDate);

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(testDate, result.Response.Timestamp);
        Assert.Equal("grok-imagine-image", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }
}
