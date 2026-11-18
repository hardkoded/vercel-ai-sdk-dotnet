// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Tests.Upstream.Output;

/// <summary>Upstream parity for <c>describe('Output.text')</c> > <c>describe('parseCompleteOutput')</c> in <c>output.test.ts</c>.</summary>
public sealed class OutputTextParseCompleteOutputTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/output.test.ts::Output.text > parseCompleteOutput::";

    [Fact]
    [UpstreamTest(Prefix + "should return the text as is", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_the_text_as_is()
    {
        var result = OutputSpec.Text().ParseCompleteOutput("some output");
        Assert.Equal(JsonValueKind.String, result.ValueKind);
        Assert.Equal("some output", result.GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle empty string", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_empty_string()
    {
        var result = OutputSpec.Text().ParseCompleteOutput(string.Empty);
        Assert.Equal(JsonValueKind.String, result.ValueKind);
        Assert.Equal(string.Empty, result.GetString());
    }
}
