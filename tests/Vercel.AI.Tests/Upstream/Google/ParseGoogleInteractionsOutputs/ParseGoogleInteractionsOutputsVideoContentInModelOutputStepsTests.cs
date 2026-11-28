// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.ParseGoogleInteractionsOutputs;

/// <summary>Port of <c>parse-google-interactions-outputs.test.ts</c> &gt; <c>parseGoogleInteractionsOutputs</c> &gt; <c>video content in model_output steps</c>.</summary>
public sealed class ParseGoogleInteractionsOutputsVideoContentInModelOutputStepsTests
{
    private const string Prefix =
        ParseGoogleInteractionsOutputsSupport.Id + "video content in model_output steps::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits a file content part with mediaType + base64 data when a video block carries inline data",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_file_content_part_with_mediaType_base64_data_when_a_video_block_carries_inline_data()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"model_output","content":[{"type":"video","mime_type":"video/mp4","data":"AAAAIGZ0eXBpc29t"}]}]""",
            "v1_video-out"
        );

        GoogleUpstream.JsonEqual(
            ParseGoogleInteractionsOutputsSupport.Snapshot(content),
            """[{"type":"file","mediaType":"video/mp4","data":{"type":"data","data":"AAAAIGZ0eXBpc29t"},"providerMetadata":{"google":{"interactionId":"v1_video-out"}}}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "defaults mediaType to video/mp4 when mime_type is absent",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Defaults_mediaType_to_video_mp4_when_mime_type_is_absent()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"model_output","content":[{"type":"video","data":"AAAAIGZ0eXBpc29t"}]}]"""
        );

        var file = Assert.IsType<GoogleGeneratedFile>(content[0]);
        Assert.Equal("video/mp4", file.MediaType);
        Assert.Null(file.Url);
        Assert.Equal("AAAAIGZ0eXBpc29t", System.Convert.ToBase64String(file.Data));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits a file content part with a URL data variant when a video block carries a uri",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_file_content_part_with_a_URL_data_variant_when_a_video_block_carries_a_uri()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"model_output","content":[{"type":"video","mime_type":"video/mp4","uri":"https://example.test/clip.mp4"}]}]"""
        );

        var file = Assert.IsType<GoogleGeneratedFile>(content[0]);
        Assert.Equal("video/mp4", file.MediaType);
        Assert.Equal("https://example.test/clip.mp4", file.Url!.ToString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "skips a video block with neither data nor uri",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Skips_a_video_block_with_neither_data_nor_uri()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"model_output","content":[{"type":"video","mime_type":"video/mp4"}]}]"""
        );

        Assert.Empty(content);
    }
}
