// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.StreamLanguageModelCall.StreamLanguageModelCallFixture;

namespace Vercel.AI.Tests.StreamLanguageModelCall;

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; stream-start parts</c>.</summary>
public sealed class StreamStartPartsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > stream-start parts::";

    [Fact]
    [UpstreamTest(
        Prefix + "should convert stream-start to init-model-call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_stream_start_to_init_model_call()
    {
        var parts = await StreamLanguageModelCallResult([
            new StreamStartStreamPart([
                new CallWarning(
                    "compatibility",
                    "tool-approval",
                    "approval fallback is being used"
                ),
                new CallWarning("other", "custom warning"),
            ]),
            Finish(),
        ]);

        Assert.Equal(2, parts.Count);
        var start = Assert.IsType<ModelCallStartPart>(parts[0]);
        Assert.Equal("model-call-start", start.Type);
        Assert.Equal(2, start.Warnings.Count);
        Assert.Equal("compatibility", start.Warnings[0].Type);
        Assert.Equal("tool-approval", start.Warnings[0].Feature);
        Assert.Equal("approval fallback is being used", start.Warnings[0].Message);
        Assert.Equal("other", start.Warnings[1].Type);
        Assert.Equal("custom warning", start.Warnings[1].Message);
        var end = ModelCallEnd(parts[1]);
        Assert.Equal(FinishReason.Stop, end.FinishReason);
        Assert.Equal("stop", end.RawFinishReason);
        Assert.Null(end.ProviderMetadata);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Null(end.Performance.TimeToFirstOutputMs);
        Assert.Null(end.Performance.InputTokensPerSecond);
        Assert.Null(end.Performance.OutputTokensPerSecond);
        Assert.Null(end.Performance.TimeBetweenOutputChunksMs);
    }
}
