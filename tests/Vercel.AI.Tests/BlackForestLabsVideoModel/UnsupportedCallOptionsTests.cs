// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>unsupported call options</c>.</summary>
public sealed class UnsupportedCallOptionsTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > unsupported call options::";

    [Fact]
    [UpstreamTest(Id + "should warn about fps, seed, and n", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutFpsSeedAndN()
    {
        var result = await CreateModel(CreateServer()).DoGenerateAsync(Call(fps: 30, seed: 42, n: 2), CancellationToken.None);

        Assert.Equal(
            new[]
            {
                ("unsupported", (string?)"fps", (string?)"FLUX 3 video does not support a custom frame rate."),
                ("unsupported", (string?)"seed", (string?)"FLUX 3 video does not accept a seed."),
                (
                    "unsupported",
                    (string?)"n",
                    (string?)"FLUX 3 video generates a single video per call. Only 1 video will be generated."),
            },
            result.Warnings.Select(Warning).ToArray());
    }
}
