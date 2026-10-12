// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput.ConvertToOpenResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; provider reference</c>.</summary>
public sealed class ProviderReferenceTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > provider reference::";

    [Fact]
    [UpstreamTest(Prefix + "should throw for file parts with provider references", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_for_file_parts_with_provider_references()
    {
        var file = FileContentPart.FromProviderReference("image/png", new Dictionary<string, string> { ["openResponses"] = "file-ref-123" });

        var error = Assert.Throws<UnsupportedFunctionalityException>(() => Convert(User(file)));

        Assert.Contains("'file parts with provider references' functionality not supported", error.Message, StringComparison.Ordinal);
    }
}
