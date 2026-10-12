// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput.ConvertToOpenResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; top-level-only media type resolution</c>.</summary>
public sealed class TopLevelOnlyMediaTypeResolutionTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > top-level-only media type resolution::";
    private const string PngBase64 = "iVBORw0KGgo=";

    [Fact]
    [UpstreamTest(Prefix + "passes full image/png through unchanged for inline data", Coverage = UpstreamCoverage.Covered)]
    public void Passes_full_image_png_through_unchanged_for_inline_data()
    {
        var result = Convert(User(File("image/png", PngBase64)));

        JsonAssert.Equal(result.Input[0]!["content"]![0], """{"type":"input_image","image_url":"data:image/png;base64,iVBORw0KGgo=","detail":"auto"}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "detects image subtype from inline bytes for top-level \"image\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_image_subtype_from_inline_bytes_for_top_level_image()
    {
        var result = Convert(User(File("image", PngBase64)));

        JsonAssert.Equal(result.Input[0]!["content"]![0], """{"type":"input_image","image_url":"data:image/png;base64,iVBORw0KGgo=","detail":"auto"}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "passes through URL source for top-level-only image", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_URL_source_for_top_level_only_image()
    {
        var result = Convert(User(UrlFile("image", "https://example.com/x.png")));

        JsonAssert.Equal(result.Input[0]!["content"]![0], """{"type":"input_image","image_url":"https://example.com/x.png","detail":"auto"}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "normalizes image/* wildcard via detection", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_image_wildcard_via_detection()
    {
        var result = Convert(User(File("image/*", PngBase64)));

        JsonAssert.Equal(result.Input[0]!["content"]![0], """{"type":"input_image","image_url":"data:image/png;base64,iVBORw0KGgo=","detail":"auto"}""");
    }
}
