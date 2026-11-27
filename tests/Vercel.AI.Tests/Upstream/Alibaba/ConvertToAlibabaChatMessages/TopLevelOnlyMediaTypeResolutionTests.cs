// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Alibaba;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Alibaba.ConvertToAlibabaChatMessages;

/// <summary>Port of <c>convert-to-alibaba-chat-messages.test.ts</c> &gt; <c>convertToAlibabaChatMessages &gt; top-level-only media type resolution</c>.</summary>
public sealed class TopLevelOnlyMediaTypeResolutionTests
{
    private const string Prefix =
        "packages/alibaba/src/convert-to-alibaba-chat-messages.test.ts::convertToAlibabaChatMessages > top-level-only media type resolution::";

    private const string PngBase64 = "iVBORw0KGgo=";

    // FileContentPart holds inline data as bytes, so the upstream base64 string is decoded first.
    private static FileContentPart Inline(string mediaType)
    {
        return new FileContentPart(
            mediaType,
            null,
            System.Convert.FromBase64String(PngBase64),
            null
        );
    }

    private static JsonNode? FirstPart(FileContentPart file)
    {
        var result = AlibabaChatMessages.Convert(
            [new UserModelMessage([file])],
            preserveThinking: false
        );
        return result[0]!["content"]![0];
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes full image/png through unchanged for inline data",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_full_image_png_through_unchanged_for_inline_data()
    {
        JsonAssert.Equal(
            FirstPart(Inline("image/png")),
            """{"type":"image_url","image_url":{"url":"data:image/png;base64,iVBORw0KGgo="}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "detects image subtype from inline bytes for top-level \"image\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Detects_image_subtype_from_inline_bytes_for_top_level_image()
    {
        JsonAssert.Equal(
            FirstPart(Inline("image")),
            """{"type":"image_url","image_url":{"url":"data:image/png;base64,iVBORw0KGgo="}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes through URL source for top-level-only image",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_through_URL_source_for_top_level_only_image()
    {
        JsonAssert.Equal(
            FirstPart(new FileContentPart("image", "https://example.com/x.png", null, null)),
            """{"type":"image_url","image_url":{"url":"https://example.com/x.png"}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "normalizes image/* wildcard via detection",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Normalizes_image_wildcard_via_detection()
    {
        JsonAssert.Equal(
            FirstPart(Inline("image/*")),
            """{"type":"image_url","image_url":{"url":"data:image/png;base64,iVBORw0KGgo="}}"""
        );
    }
}
