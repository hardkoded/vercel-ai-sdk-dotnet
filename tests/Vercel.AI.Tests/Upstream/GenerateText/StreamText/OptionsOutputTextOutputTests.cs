// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.output &gt; text output</c>. Only the tests below are ported.</summary>
public sealed class OptionsOutputTextOutputTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.output > text output::";

    [Fact]
    [UpstreamTest(Prefix + "should send partial output stream")]
    public async Task Should_send_partial_output_stream()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("Hello, ", "world!"),
            OutputSpec.Text()
        );

        Assert.Equal(
            new[] { "\"Hello, \"", "\"Hello, world!\"" },
            await OptionsOutputSupport.Collect(result)
        );
    }
}
