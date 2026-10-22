// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>response metadata</c>.</summary>
public sealed class ResponseMetadataTests
{
    [Fact]
    [UpstreamTest(
        "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > response metadata::should report the model id and timestamp",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReportTheModelIdAndTimestamp()
    {
        var currentDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = await CreateModel(CreateServer(), () => currentDate).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal("flux-3-video", result.Response.ModelId);
        Assert.Equal(currentDate.UtcDateTime, result.Response.Timestamp);
    }
}
