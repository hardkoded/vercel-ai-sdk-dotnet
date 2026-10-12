// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenResponsesInputTests.ConvertToOpenResponsesInputHelpers;

namespace Vercel.AI.Tests.ConvertToOpenResponsesInputTests;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; top-level-only media type resolution</c>.</summary>
public sealed class TopLevelOnlyMediaTypeResolutionTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > top-level-only media type resolution::";

    private const string PngBase64 = "iVBORw0KGgo=";

    [Fact]
    [UpstreamTest(Prefix + "passes full image/png through unchanged for inline data", Coverage = UpstreamCoverage.Covered)]
    public void Passes_full_image_png_through_unchanged_for_inline_data()
    {
        var result = Convert(User(new FileContentPart("image/png", null, System.Convert.FromBase64String(PngBase64), null)));

        AssertFirstContent("{\"type\":\"input_image\",\"image_url\":\"data:image/png;base64," + PngBase64 + "\",\"detail\":\"auto\"}", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "detects image subtype from inline bytes for top-level \"image\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_image_subtype_from_inline_bytes_for_top_level_image()
    {
        var result = Convert(User(new FileContentPart("image", null, System.Convert.FromBase64String(PngBase64), null)));

        AssertFirstContent("{\"type\":\"input_image\",\"image_url\":\"data:image/png;base64," + PngBase64 + "\",\"detail\":\"auto\"}", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "passes through URL source for top-level-only image", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_URL_source_for_top_level_only_image()
    {
        var result = Convert(User(new FileContentPart("image", "https://example.com/x.png", null, null)));

        AssertFirstContent("{\"type\":\"input_image\",\"image_url\":\"https://example.com/x.png\",\"detail\":\"auto\"}", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "normalizes image/* wildcard via detection", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_image_wildcard_via_detection()
    {
        var result = Convert(User(new FileContentPart("image/*", null, System.Convert.FromBase64String(PngBase64), null)));

        AssertFirstContent("{\"type\":\"input_image\",\"image_url\":\"data:image/png;base64," + PngBase64 + "\",\"detail\":\"auto\"}", result);
    }

    private static void AssertFirstContent(string expectedJson, Vercel.AI.OpenResponses.OpenResponsesInputResult result)
    {
        var content = result.Input[0]!["content"]![0];
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(System.Text.Json.Nodes.JsonNode.Parse(expectedJson), content),
            "Expected " + expectedJson + " but was " + content!.ToJsonString());
    }
}
