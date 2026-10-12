// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput.ConvertToXaiResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput;

/// <summary>Port of <c>convert-to-xai-responses-input.test.ts</c> &gt; <c>convertToXaiResponsesInput &gt; top-level-only media type resolution</c>.</summary>
public sealed class TopLevelOnlyMediaTypeResolutionTests
{
    private const string Prefix = TestFile + " > top-level-only media type resolution::";
    private const string PngBase64 = "iVBORw0KGgo=";

    [Fact]
    [UpstreamTest(Prefix + "passes full image/png through unchanged for inline data", Coverage = UpstreamCoverage.Covered)]
    public void Passes_full_image_png_through_unchanged_for_inline_data()
    {
        AssertPng(Convert(User(Inline("image/png"))));
    }

    [Fact]
    [UpstreamTest(Prefix + "detects image subtype from inline bytes for top-level \"image\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_image_subtype_from_inline_bytes_for_top_level_image()
    {
        AssertPng(Convert(User(Inline("image"))));
    }

    [Fact]
    [UpstreamTest(Prefix + "passes through URL source for top-level-only image", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_URL_source_for_top_level_only_image()
    {
        var input = Convert(User(new FileContentPart("image", "https://example.com/x.png", null, null)));

        JsonAssert.Equal(input[0]!["content"]![0], """{"type":"input_image","image_url":"https://example.com/x.png"}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "normalizes image/* wildcard via detection", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_image_wildcard_via_detection()
    {
        AssertPng(Convert(User(Inline("image/*"))));
    }

    private static FileContentPart Inline(string mediaType)
    {
        return new FileContentPart(mediaType, null, System.Convert.FromBase64String(PngBase64), null);
    }

    private static void AssertPng(System.Text.Json.Nodes.JsonArray input)
    {
        JsonAssert.Equal(input[0]!["content"]![0], "{\"type\":\"input_image\",\"image_url\":\"data:image/png;base64," + PngBase64 + "\"}");
    }
}
