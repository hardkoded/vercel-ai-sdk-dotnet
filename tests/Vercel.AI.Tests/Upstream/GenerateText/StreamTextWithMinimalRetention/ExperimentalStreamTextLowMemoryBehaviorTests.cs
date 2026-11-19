// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamTextWithMinimalRetention;

/// <summary>Port of <c>stream-text-with-minimal-retention.test.ts</c> &gt; <c>experimental_streamTextLowMemory behavior</c>. Only the tests below are ported.</summary>
public sealed class ExperimentalStreamTextLowMemoryBehaviorTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text-with-minimal-retention.test.ts::%s behavior::";

    [Fact]
    [UpstreamTest(Prefix + "supports element streams")]
    public async Task Supports_element_streams()
    {
        var result = StreamingOnlyContractTests.LowMemory(
            [
                new TextStartStreamPart("text"),
                new TextDeltaStreamPart("text", "{\"elements\":[{\"value\":1},{\"value\":2}]}"),
                new TextEndStreamPart("text"),
                StreamingOnlyContractTests.Finish(),
            ],
            OutputSpec.Array(
                "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\"}},\"required\":[\"value\"],\"additionalProperties\":false}"
            )
        );

        var elements = new List<string>();
        await foreach (var element in result.ElementStream())
        {
            elements.Add(element.GetRawText());
        }

        Assert.Equal(new[] { "{\"value\":1}", "{\"value\":2}" }, elements);
    }
}
