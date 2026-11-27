// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>citations</c>.</summary>
public sealed class CitationsTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::citations::";

    [Fact]
    [UpstreamTest(
        Prefix + "should not include citations by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_include_citations_by_default()
    {
        var result = Run(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"base64PDFdata\"},\"mediaType\":\"application/pdf\"}]}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"source\":{\"data\":\"base64PDFdata\",\"media_type\":\"application/pdf\",\"type\":\"base64\"},\"type\":\"document\"}]}]",
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Equal(["pdfs-2024-09-25"], result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include citations when enabled on file part",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_citations_when_enabled_on_file_part()
    {
        var result = Run(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"base64PDFdata\"},\"mediaType\":\"application/pdf\",\"providerOptions\":{\"anthropic\":{\"citations\":{\"enabled\":true}}}}]}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"citations\":{\"enabled\":true},\"source\":{\"data\":\"base64PDFdata\",\"media_type\":\"application/pdf\",\"type\":\"base64\"},\"type\":\"document\"}]}]",
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Equal(["pdfs-2024-09-25"], result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include custom title and context when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_custom_title_and_context_when_provided()
    {
        var result = Run(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"base64PDFdata\"},\"mediaType\":\"application/pdf\",\"filename\":\"original-name.pdf\",\"providerOptions\":{\"anthropic\":{\"title\":\"Custom Document Title\",\"context\":\"This is metadata about the document\",\"citations\":{\"enabled\":true}}}}]}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"citations\":{\"enabled\":true},\"context\":\"This is metadata about the document\",\"source\":{\"data\":\"base64PDFdata\",\"media_type\":\"application/pdf\",\"type\":\"base64\"},\"title\":\"Custom Document Title\",\"type\":\"document\"}]}]",
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Equal(["pdfs-2024-09-25"], result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle multiple documents with consistent citation settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_multiple_documents_with_consistent_citation_settings()
    {
        var result = Run(
            "[{\"role\":\"user\",\"content\":["
                + "{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"base64PDFdata1\"},\"mediaType\":\"application/pdf\",\"filename\":\"doc1.pdf\",\"providerOptions\":{\"anthropic\":{\"citations\":{\"enabled\":true},\"title\":\"Custom Title 1\"}}},"
                + "{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"base64PDFdata2\"},\"mediaType\":\"application/pdf\",\"filename\":\"doc2.pdf\",\"providerOptions\":{\"anthropic\":{\"citations\":{\"enabled\":true},\"title\":\"Custom Title 2\",\"context\":\"Additional context for document 2\"}}},"
                + "{\"type\":\"text\",\"text\":\"Analyze both documents\"}]}]"
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":["
                + "{\"citations\":{\"enabled\":true},\"source\":{\"data\":\"base64PDFdata1\",\"media_type\":\"application/pdf\",\"type\":\"base64\"},\"title\":\"Custom Title 1\",\"type\":\"document\"},"
                + "{\"citations\":{\"enabled\":true},\"context\":\"Additional context for document 2\",\"source\":{\"data\":\"base64PDFdata2\",\"media_type\":\"application/pdf\",\"type\":\"base64\"},\"title\":\"Custom Title 2\",\"type\":\"document\"},"
                + "{\"text\":\"Analyze both documents\",\"type\":\"text\"}]}]",
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Equal(["pdfs-2024-09-25"], result.Betas);
    }
}
