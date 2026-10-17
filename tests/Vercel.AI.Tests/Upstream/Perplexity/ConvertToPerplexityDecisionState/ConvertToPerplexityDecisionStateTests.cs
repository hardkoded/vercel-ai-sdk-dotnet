// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Perplexity.ConvertToPerplexityDecisionState.ConvertToPerplexityDecisionStateSupport;

namespace Vercel.AI.Tests.Upstream.Perplexity.ConvertToPerplexityDecisionState;

/// <summary>Port of <c>convert-to-perplexity-decision-state.test.ts</c>.</summary>
public sealed class ConvertToPerplexityDecisionStateTests
{
    private const string Prefix = "packages/perplexity/src/convert-to-perplexity-decision-state.test.ts::";

    public static TheoryData<int> NativeStates => new() { 0, 1, 2, 3, 4, 5, 6 };

    [Theory]
    [MemberData(nameof(NativeStates))]
    [UpstreamTest(Prefix + "preserves native state without stringifying JSON: $expected", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_native_state_without_stringifying_JSON(int index)
    {
        var cases = new (object?[] State, string Expected)[]
        {
            (new object?[] { Text("Review") }, "\"Review\""),
            (new object?[] { Json(new Dictionary<string, object?> { ["ticket"] = "Review" }) }, "{\"ticket\":\"Review\"}"),
            (new object?[] { Json(new List<object?> { "Review", null, 1 }) }, "[\"Review\",null,1]"),
            (new object?[] { Json(null) }, "[null]"),
            (new object?[] { Json(false) }, "[false]"),
            (new object?[] { Json(1) }, "[1]"),
            (new object?[0], "[]"),
        };
        JsonAssert.Equal(Convert(cases[index].State), cases[index].Expected);
    }

    public static TheoryData<bool> ImageData => new() { false, true };

    [Theory]
    [MemberData(nameof(ImageData))]
    [UpstreamTest(Prefix + "preserves order across text, JSON and image parts", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_order_across_text_JSON_and_image_parts(bool bytes)
    {
        object data = bytes ? new byte[] { 0x89, 0x50, 0x4e, 0x47 } : "iVBORw==";
        var result = Convert(
            Text("Inspect"),
            Json(new Dictionary<string, object?> { ["product"] = "Vase" }),
            File("image/png", data),
            Text("Look for cracks"));
        JsonAssert.Equal(result, "[\"Inspect\",{\"product\":\"Vase\"}," + Image + ",\"Look for cracks\"]");
    }

    public static TheoryData<string> SupportedMediaTypes => new() { "image/png", "image/jpeg", "image/webp" };

    [Theory]
    [MemberData(nameof(SupportedMediaTypes))]
    [UpstreamTest(Prefix + "supports %s and image-only state", Coverage = UpstreamCoverage.Covered)]
    public void Supports_media_type_and_image_only_state(string mediaType)
    {
        JsonAssert.Equal(Convert(File(mediaType, "AQID")), "{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:" + mediaType + ";base64,AQID\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "detects image media types for direct provider calls", Coverage = UpstreamCoverage.Covered)]
    public void Detects_image_media_types_for_direct_provider_calls()
    {
        JsonAssert.Equal(Convert(File("image/*", "iVBORw==")), Image);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves images nested inside JSON at any depth", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_images_nested_inside_JSON_at_any_depth()
    {
        var value = "{\"caption\":\"Inspect\",\"product\":{\"photos\":[" + Image + ",{\"detail\":" + Image + "}]}}";
        using var document = System.Text.Json.JsonDocument.Parse(value);
        JsonAssert.Equal(Convert(Json(document.RootElement.Clone())), value);
    }
}
