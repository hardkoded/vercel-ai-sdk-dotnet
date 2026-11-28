// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Converter = Vercel.AI.Mistral.ConvertToMistralChatMessages;

namespace Vercel.AI.Tests.Upstream.Mistral.ConvertToMistralChatMessages;

/// <summary>Port of <c>convert-to-mistral-chat-messages.test.ts</c> &gt; <c>top-level-only media type resolution</c>.</summary>
public sealed class TopLevelOnlyMediaTypeResolutionTests
{
    private const string Prefix =
        "packages/mistral/src/convert-to-mistral-chat-messages.test.ts::top-level-only media type resolution::";

    private static void AssertJson(JsonNode? actual, string expected) =>
        Assert.True(JsonNode.DeepEquals(actual, JsonNode.Parse(expected)), actual?.ToJsonString());

    private const string PngBase64 = "iVBORw0KGgo=";

    [Fact]
    [UpstreamTest(
        Prefix + "passes full image/png through unchanged for inline data",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_full_image_png_through_unchanged_for_inline_data()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new FileContentPart(
                            "image/png",
                            null,
                            Convert.FromBase64String(PngBase64),
                            null
                        ),
                    }
                ),
            }
        );

        AssertJson(
            result[0]!["content"]![0],
            "{\"type\":\"image_url\",\"image_url\":\"data:image/png;base64," + PngBase64 + "\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "detects image subtype from inline bytes for top-level \"image\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Detects_image_subtype_from_inline_bytes_for_top_level_image()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new FileContentPart(
                            "image",
                            null,
                            Convert.FromBase64String(PngBase64),
                            null
                        ),
                    }
                ),
            }
        );

        AssertJson(
            result[0]!["content"]![0],
            "{\"type\":\"image_url\",\"image_url\":\"data:image/png;base64," + PngBase64 + "\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "throws for top-level-only application with URL source",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Throws_for_top_level_only_application_with_URL_source()
    {
        var error = Assert.ThrowsAny<Exception>(() =>
            Converter.Convert(
                new ModelMessage[]
                {
                    new UserModelMessage(
                        new UserContentPart[]
                        {
                            new FileContentPart(
                                "application",
                                "https://example.com/x.pdf",
                                null,
                                null
                            ),
                        }
                    ),
                }
            )
        );

        Assert.Contains("Only images and PDF file parts are supported", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "normalizes image/* wildcard via detection",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Normalizes_image_wildcard_via_detection()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new FileContentPart(
                            "image/*",
                            null,
                            Convert.FromBase64String(PngBase64),
                            null
                        ),
                    }
                ),
            }
        );

        AssertJson(
            result[0]!["content"]![0],
            "{\"type\":\"image_url\",\"image_url\":\"data:image/png;base64," + PngBase64 + "\"}"
        );
    }
}
