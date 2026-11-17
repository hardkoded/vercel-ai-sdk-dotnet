// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; doGenerate &gt; resolvedInputs</c>.</summary>
public sealed class DoGenerateResolvedInputsTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > doGenerate > resolvedInputs::";

    private static async Task Check(MiniMaxCall options, string expected)
    {
        var result = await Generate(Server(), options);
        JsonAssert.Equal(Resolve(result.ProviderMetadata, "resolvedInputs"), expected);
    }

    private static string Resolved(int imageCount, params string[] urls)
    {
        return "{\"imageCount\":"
            + imageCount
            + ",\"referenceVideoUrls\":["
            + string.Join(",", urls.Select(url => "\"" + url + "\""))
            + "]}";
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report nothing sent for a text-only request",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_report_nothing_sent_for_a_text_only_request()
    {
        return Check(new MiniMaxCall(), Resolved(0));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should count both a first_frame and a last_frame",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_count_both_a_first_frame_and_a_last_frame()
    {
        return Check(
            new MiniMaxCall
            {
                FrameImages = new[]
                {
                    Frame(ImageUrlFile, "first_frame"),
                    Frame(ImageReference(1), "last_frame"),
                },
            },
            Resolved(2)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should count a reference with no media type as an image",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_count_a_reference_with_no_media_type_as_an_image()
    {
        return Check(
            new MiniMaxCall
            {
                InputReferences = new[]
                {
                    VideoModelFile.FromUrl("https://cdn.example.com/unknown"),
                },
            },
            Resolved(1)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should split mixed references into images and videos",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_split_mixed_references_into_images_and_videos()
    {
        return Check(
            new MiniMaxCall { InputReferences = new[] { ImageReference(0), VideoReference(0) } },
            Resolved(1, "https://cdn.example.com/vid-0.mp4")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cap the reported image count at 9 reference images",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_cap_the_reported_image_count_at_9_reference_images()
    {
        return Check(
            new MiniMaxCall
            {
                InputReferences = Enumerable.Range(0, 12).Select(ImageReference).ToList(),
            },
            Resolved(9)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report only the first 3 reference videos",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_report_only_the_first_3_reference_videos()
    {
        return Check(
            new MiniMaxCall
            {
                InputReferences = Enumerable.Range(0, 5).Select(VideoReference).ToList(),
            },
            Resolved(
                0,
                "https://cdn.example.com/vid-0.mp4",
                "https://cdn.example.com/vid-1.mp4",
                "https://cdn.example.com/vid-2.mp4"
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit inline reference videos, which have no URL to report",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_omit_inline_reference_videos_which_have_no_URL_to_report()
    {
        return Check(
            new MiniMaxCall
            {
                InputReferences = new[] { VideoModelFile.FromBase64("AAAA", "video/mp4") },
            },
            Resolved(0)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report no references in frame-image mode, which drops them",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_report_no_references_in_frame_image_mode_which_drops_them()
    {
        return Check(
            new MiniMaxCall
            {
                FrameImages = new[] { Frame(ImageUrlFile, "first_frame") },
                InputReferences = new[] { ImageReference(0), VideoReference(0) },
            },
            Resolved(1)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should treat a standalone image as a frame image, dropping references",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_treat_a_standalone_image_as_a_frame_image_dropping_references()
    {
        return Check(
            new MiniMaxCall
            {
                Image = ImageUrlFile,
                InputReferences = new[] { ImageReference(0), VideoReference(0) },
            },
            Resolved(1)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report nothing for a video passed as a first_frame",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_report_nothing_for_a_video_passed_as_a_first_frame()
    {
        return Check(
            new MiniMaxCall { FrameImages = new[] { Frame(VideoReference(0), "first_frame") } },
            Resolved(0)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report nothing for a last_frame with no first_frame",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_report_nothing_for_a_last_frame_with_no_first_frame()
    {
        return Check(
            new MiniMaxCall { FrameImages = new[] { Frame(ImageUrlFile, "last_frame") } },
            Resolved(0)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should count a duplicated first_frame once",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_count_a_duplicated_first_frame_once()
    {
        return Check(
            new MiniMaxCall
            {
                FrameImages = new[]
                {
                    Frame(ImageUrlFile, "first_frame"),
                    Frame(ImageReference(1), "first_frame"),
                },
            },
            Resolved(1)
        );
    }
}
