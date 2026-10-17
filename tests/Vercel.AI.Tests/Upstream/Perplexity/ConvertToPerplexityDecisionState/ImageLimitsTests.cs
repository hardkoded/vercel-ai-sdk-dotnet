// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Perplexity.ConvertToPerplexityDecisionState.ConvertToPerplexityDecisionStateSupport;

namespace Vercel.AI.Tests.Upstream.Perplexity.ConvertToPerplexityDecisionState;

/// <summary>Port of <c>convert-to-perplexity-decision-state.test.ts</c> &gt; <c>image limits</c>.</summary>
public sealed class ImageLimitsTests
{
    private const string Prefix = "packages/perplexity/src/convert-to-perplexity-decision-state.test.ts::image limits::";

    public static TheoryData<string> UnsupportedUrls => new()
    {
        "https://example.com/image.png",
        "http://example.com/image.png",
        "data:image/gif;base64,AQID",
        "data:image/png,unencoded",
    };

    public static TheoryData<string> UnsupportedMediaTypes => new() { "image/gif", "application/pdf", "audio/wav", "image/*" };

    [Fact]
    [UpstreamTest(Prefix + "counts native nested images together with SDK file parts", Coverage = UpstreamCoverage.Covered)]
    public void Counts_native_nested_images_together_with_SDK_file_parts()
    {
        var photos = string.Join(",", Enumerable.Repeat(Image, 7));
        using var document = JsonDocument.Parse("{\"photos\":[" + photos + "]}");
        var state = new object?[] { Json(document.RootElement.Clone()), File("image/png", "iVBORw==") };
        Assert.Equal(2, Assert.IsType<JsonArray>(Convert(state)).Count);
        using var extra = JsonDocument.Parse(Image);
        Assert.Throws<InvalidArgumentException>(() => Convert(state.Append(Json(extra.RootElement.Clone())).ToArray()));
    }

    [Theory]
    [MemberData(nameof(UnsupportedUrls))]
    [UpstreamTest(Prefix + "rejects unsupported native image URL %s", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_unsupported_native_image_URL(string url)
    {
        using var document = JsonDocument.Parse("{\"photo\":{\"type\":\"image_url\",\"image_url\":{\"url\":" + JsonSerializer.Serialize(url) + "}}}");
        Assert.Throws<UnsupportedFunctionalityException>(() => Convert(Json(document.RootElement.Clone())));
    }

    [Theory]
    [MemberData(nameof(UnsupportedMediaTypes))]
    [UpstreamTest(Prefix + "rejects unsupported or unrecognized file type %s", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_unsupported_or_unrecognized_file_type(string mediaType)
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Convert(File(mediaType, "AQID")));
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects URLs on direct provider calls instead of asking Perplexity to fetch them", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_URLs_on_direct_provider_calls_instead_of_asking_Perplexity_to_fetch_them()
    {
        var part = new Dictionary<string, object?>
        {
            ["type"] = "file",
            ["mediaType"] = "image/png",
            ["data"] = new Dictionary<string, object?> { ["type"] = "url", ["url"] = "https://example.com/image.png" },
        };
        Assert.Throws<UnsupportedFunctionalityException>(() => Convert(part));
    }
}
