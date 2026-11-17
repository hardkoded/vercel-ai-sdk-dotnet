// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.output &gt; json output</c>. Only the tests below are ported.</summary>
public sealed class OptionsOutputJsonOutputTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.output > json output::";

    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    [UpstreamTest(Prefix + "should stream null and empty string values")]
    public async Task Should_stream_null_and_empty_string_values(string json)
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text(json),
            OutputSpec.Json()
        );

        Assert.Equal(new[] { json }, await OptionsOutputSupport.Collect(result));
        Assert.Equal(json, (await result.Output)!.Value.GetRawText());
    }
}
