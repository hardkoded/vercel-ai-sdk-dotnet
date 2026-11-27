// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>user messages &gt; top-level-only media type resolution</c>.</summary>
public sealed class TopLevelOnlyMediaTypeResolutionTests
{
    private const string TopLevelPrefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages > top-level-only media type resolution::";

    private static readonly string PngBase64 = System.Convert.ToBase64String([
        0x89,
        0x50,
        0x4e,
        0x47,
        0x0d,
        0x0a,
        0x1a,
        0x0a,
    ]);

    private static readonly string PdfBase64 = System.Convert.ToBase64String([
        0x25,
        0x50,
        0x44,
        0x46,
        0x2d,
    ]);

    private static AnthropicPromptResult RunFile(string filePart) =>
        Run("[{\"role\":\"user\",\"content\":[" + filePart + "]}]", sendReasoning: true);

    private static string File(string data, string mediaType) =>
        "{\"type\":\"file\",\"data\":" + data + ",\"mediaType\":\"" + mediaType + "\"}";

    private static string Data(string data) => "{\"type\":\"data\",\"data\":\"" + data + "\"}";

    private static string Url(string url) => "{\"type\":\"url\",\"url\":\"" + url + "\"}";

    [Fact]
    [UpstreamTest(
        TopLevelPrefix + "detects image subtype from inline bytes for top-level \"image\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Detects_image_subtype_from_inline_bytes_for_top_level_image()
    {
        var result = RunFile(File(Data(PngBase64), "image"));

        AssertJson(
            "{\"type\":\"image\",\"source\":{\"type\":\"base64\",\"media_type\":\"image/png\",\"data\":\""
                + PngBase64
                + "\"}}",
            result.Messages[0]!["content"]![0]
        );
    }

    [Fact]
    [UpstreamTest(
        TopLevelPrefix + "normalizes image/* via detection",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Normalizes_image_wildcard_via_detection()
    {
        var result = RunFile(File(Data(PngBase64), "image/*"));

        Assert.Equal(
            "image/png",
            (string?)result.Messages[0]!["content"]![0]!["source"]!["media_type"]
        );
    }

    [Fact]
    [UpstreamTest(
        TopLevelPrefix
            + "passes through URL for top-level-only image (Anthropic accepts URL source)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_through_URL_for_top_level_only_image()
    {
        var result = RunFile(File(Url("https://example.com/x.png"), "image"));

        AssertJson(
            "{\"type\":\"image\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/x.png\"}}",
            result.Messages[0]!["content"]![0]
        );
    }

    [Fact]
    [UpstreamTest(
        TopLevelPrefix + "detects PDF subtype from inline bytes for top-level \"application\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Detects_PDF_subtype_from_inline_bytes_for_top_level_application()
    {
        var result = RunFile(File(Data(PdfBase64), "application"));

        Assert.Equal(
            "application/pdf",
            (string?)result.Messages[0]!["content"]![0]!["source"]!["media_type"]
        );
    }

    [Fact]
    [UpstreamTest(
        TopLevelPrefix + "preserves full image/png pass-through",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_full_image_png_pass_through()
    {
        var result = RunFile(File(Data(PngBase64), "image/png"));

        Assert.Equal(
            "image/png",
            (string?)result.Messages[0]!["content"]![0]!["source"]!["media_type"]
        );
    }

    [Fact]
    [UpstreamTest(
        TopLevelPrefix + "still routes text/plain inline text through document source",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Still_routes_text_plain_inline_text_through_document_source()
    {
        var result = RunFile(File("{\"type\":\"text\",\"text\":\"hello\"}", "text/plain"));

        var part = result.Messages[0]!["content"]![0]!;
        Assert.Equal("document", (string?)part["type"]);
        AssertJson(
            "{\"type\":\"text\",\"media_type\":\"text/plain\",\"data\":\"hello\"}",
            part["source"]
        );
    }
}
