// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt.PromptRunner;

namespace Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt;

/// <summary>Port of <c>convert-to-cohere-chat-prompt.test.ts</c> &gt; <c>file processing</c>.</summary>
public sealed class FileProcessingTests
{
    private const string Suite = Prefix + "file processing::";

    [Fact]
    [UpstreamTest(
        Suite + "should extract documents from file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_extract_documents_from_file_parts()
    {
        var result = Run(
            User(
                new TextContentPart("Analyze this file: "),
                File("text/plain", "This is file content", "test.txt")
            )
        );

        JsonAssert.Equal(
            result,
            "{\"messages\":[{\"role\":\"user\",\"content\":\"Analyze this file: \"}],\"documents\":[{\"data\":{\"text\":\"This is file content\",\"title\":\"test.txt\"}}],\"warnings\":[]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite
            + "should accept top-level-only mediaType without error (category D: mediaType not consumed)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_top_level_only_mediaType_without_error()
    {
        var result = Run(User(File("text", "This is file content", "test.txt")));

        JsonAssert.Equal(
            result,
            "{\"messages\":[{\"role\":\"user\",\"content\":\"\"}],\"documents\":[{\"data\":{\"text\":\"This is file content\",\"title\":\"test.txt\"}}],\"warnings\":[]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should not read mediaType (document payload carries only text + title)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_read_mediaType()
    {
        var result = Run(User(File("application/pdf", "PDF-like content", "test.pdf")));

        JsonAssert.Equal(
            result["documents"],
            "[{\"data\":{\"text\":\"PDF-like content\",\"title\":\"test.pdf\"}}]"
        );
        var payload = result.ToJsonString();
        Assert.DoesNotContain("application/pdf", payload);
        Assert.DoesNotContain("mediaType", payload);
    }
}
