// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.ParseGoogleInteractionsOutputs;

/// <summary>Port of <c>parse-google-interactions-outputs.test.ts</c> &gt; <c>parseGoogleInteractionsOutputs</c> &gt; <c>image content in model_output steps</c>.</summary>
public sealed class ParseGoogleInteractionsOutputsImageContentInModelOutputStepsTests
{
    private const string Prefix =
        ParseGoogleInteractionsOutputsSupport.Id + "image content in model_output steps::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits a file content part with mediaType + base64 data when an image block carries inline data",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_file_content_part_with_mediaType_base64_data_when_an_image_block_carries_inline_data()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"model_output","content":[{"type":"image","mime_type":"image/png","data":"aGVsbG8td29ybGQ="}]}]""",
            "v1_image-out"
        );

        GoogleUpstream.JsonEqual(
            ParseGoogleInteractionsOutputsSupport.Snapshot(content),
            """[{"type":"file","mediaType":"image/png","data":{"type":"data","data":"aGVsbG8td29ybGQ="},"providerMetadata":{"google":{"interactionId":"v1_image-out"}}}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits a file content part with a URL data variant when an image block carries a uri",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_file_content_part_with_a_URL_data_variant_when_an_image_block_carries_a_uri()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"model_output","content":[{"type":"image","mime_type":"image/jpeg","uri":"https://example.test/img.jpg"}]}]"""
        );

        var file = Assert.IsType<GoogleGeneratedFile>(content[0]);
        Assert.Equal("image/jpeg", file.MediaType);
        Assert.Equal("https://example.test/img.jpg", file.Url!.ToString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "skips an image block with neither data nor uri",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Skips_an_image_block_with_neither_data_nor_uri()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"model_output","content":[{"type":"image","mime_type":"image/png"}]}]"""
        );

        Assert.Empty(content);
    }
}
