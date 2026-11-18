// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.StreamLanguageModelCall.StreamLanguageModelCallFixture;

namespace Vercel.AI.Tests.StreamLanguageModelCall;

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; reasoning parts</c>.</summary>
public sealed class ReasoningPartsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > reasoning parts::";

    [Fact]
    [UpstreamTest(Prefix + "should convert text to delta", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_text_to_delta()
    {
        var result = await StreamLanguageModelCallResult([
            new ReasoningStartStreamPart("1"),
            new ReasoningDeltaStreamPart("1", "text"),
            new ReasoningEndStreamPart("1"),
        ]);

        Assert.Equal(3, result.Count);
        Assert.Equal("1", Assert.IsType<ReasoningStartPart>(result[0]).Id);
        var delta = Assert.IsType<ReasoningDeltaPart>(result[1]);
        Assert.Equal("1", delta.Id);
        Assert.Equal("text", delta.Text);
        Assert.Null(delta.ProviderMetadata);
        Assert.Equal("1", Assert.IsType<ReasoningEndPart>(result[2]).Id);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should measure time to first output from reasoning-file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_measure_time_to_first_output_from_reasoning_file_parts()
    {
        var result = await StreamLanguageModelCallResult(
            [
                new ReasoningFileStreamPart(Encoding.UTF8.GetBytes("reasoning"), "text/plain"),
                Finish(),
            ],
            now: Now(1000, 1250, 1600)
        );

        AssertTimeToFirstOutput(ModelCallEnd(result[^1]));
    }
}
