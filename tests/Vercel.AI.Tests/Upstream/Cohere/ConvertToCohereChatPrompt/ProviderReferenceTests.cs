// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt.PromptRunner;

namespace Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt;

/// <summary>Port of <c>convert-to-cohere-chat-prompt.test.ts</c> &gt; <c>provider reference</c>.</summary>
public sealed class ProviderReferenceTests
{
    [Fact]
    [UpstreamTest(
        Prefix + "provider reference::should throw for file parts with provider references",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_file_parts_with_provider_references()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() =>
            Run(
                User(
                    FileContentPart.FromProviderReference(
                        "text/plain",
                        new Dictionary<string, string> { ["cohere"] = "doc-ref-123" }
                    )
                )
            )
        );

        Assert.Contains(
            "'file parts with provider references' functionality not supported",
            error.Message
        );
    }
}
