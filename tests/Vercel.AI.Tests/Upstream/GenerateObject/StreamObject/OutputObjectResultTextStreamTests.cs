// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.textStream</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultTextStreamTests
{
    private const string Id = Prefix + "output = \"object\" > result.textStream::";

    [Fact]
    [UpstreamTest(Id + "should send text stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_text_stream()
    {
        var result = Client().StreamObjectAsync(ObjectOptions(CreateTestModel()));

        Assert.Equal(
            ["{ ", "\"content\": \"Hello, ", "world", "!\"", " }"],
            await Collect(result.TextStream())
        );
    }
}
