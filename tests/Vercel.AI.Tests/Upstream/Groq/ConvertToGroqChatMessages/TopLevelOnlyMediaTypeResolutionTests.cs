// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Groq.ConvertToGroqChatMessages.ConvertToGroqChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.Groq.ConvertToGroqChatMessages;

/// <summary>Port of <c>convert-to-groq-chat-messages.test.ts</c> &gt; <c>top-level-only media type resolution</c>.</summary>
public sealed class TopLevelOnlyMediaTypeResolutionTests
{
    private const string Prefix = TestFile + "::top-level-only media type resolution::";

    private const string PngBase64 = "iVBORw0KGgo=";

    private static void AssertPngDataUrl(JsonArray result) =>
        JsonAssert.Equal(
            Content(result),
            """{"type":"image_url","image_url":{"url":"data:image/png;base64,iVBORw0KGgo="}}"""
        );

    [Fact]
    [UpstreamTest(
        Prefix + "passes full image/png through unchanged for inline data",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Passes_full_image_png_through_unchanged_for_inline_data()
    {
        AssertPngDataUrl(Run(User(new FileContentPart("image/png", null, PngBytes, null))));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "detects image subtype from inline bytes for top-level \"image\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Detects_image_subtype_from_inline_bytes_for_top_level_image()
    {
        AssertPngDataUrl(Run(User(new FileContentPart("image", null, PngBytes, null))));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes through URL source for top-level-only image",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Passes_through_URL_source_for_top_level_only_image()
    {
        var result = Run(
            User(new FileContentPart("image", "https://example.com/x.png", null, null))
        );

        JsonAssert.Equal(
            Content(result),
            """{"type":"image_url","image_url":{"url":"https://example.com/x.png"}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "normalizes image/* wildcard via detection",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Normalizes_image_wildcard_via_detection()
    {
        AssertPngDataUrl(Run(User(new FileContentPart("image/*", null, PngBytes, null))));
    }
}
