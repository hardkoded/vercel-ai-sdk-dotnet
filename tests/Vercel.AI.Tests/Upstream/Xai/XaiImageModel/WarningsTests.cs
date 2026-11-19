// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Xai.XaiImageModel.XaiImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiImageModel;

/// <summary>Port of <c>xai-image-model.test.ts</c> &gt; <c>XaiImageModel</c> &gt; <c>doGenerate</c> &gt; <c>warnings</c>.</summary>
public sealed class WarningsTests
{
    private const string Id =
        "packages/xai/src/xai-image-model.test.ts::XaiImageModel > doGenerate > warnings::";

    [Fact]
    [UpstreamTest(
        Id + "should warn when size is provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_when_size_is_provided()
    {
        var result = await CreateModel(Server())
            .DoGenerateAsync(Call(size: "1024x1024"), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("size", warning.Feature);
        Assert.Equal(
            "This model does not support the `size` option. Use `aspectRatio` instead.",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn when seed is provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_when_seed_is_provided()
    {
        var result = await CreateModel(Server())
            .DoGenerateAsync(Call(seed: 42), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("seed", warning.Feature);
        Assert.Null(warning.Details);
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn when mask is provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_when_mask_is_provided()
    {
        var result = await CreateModel(Server())
            .DoGenerateAsync(
                Call(
                    "Edit this",
                    files: [PngFile()],
                    mask: ImageModelFile.FromFile([255, 255, 255, 0], "image/png")
                ),
                CancellationToken.None
            );

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("mask", warning.Feature);
        Assert.Null(warning.Details);
    }

    [Fact]
    [UpstreamTest(
        Id + "should not warn when multiple files are provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_warn_when_multiple_files_are_provided()
    {
        var result = await CreateModel(Server())
            .DoGenerateAsync(
                Call("Edit images", files: [PngFile(), PngFile()]),
                CancellationToken.None
            );

        Assert.DoesNotContain(result.Warnings, warning => warning.Type == "other");
    }
}
