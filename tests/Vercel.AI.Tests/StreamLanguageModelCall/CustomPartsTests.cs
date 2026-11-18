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

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; custom parts</c>.</summary>
public sealed class CustomPartsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > custom parts::";

    [Fact]
    [UpstreamTest(Prefix + "should forward custom parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_custom_parts()
    {
        var result = await StreamLanguageModelCallResult([
            new CustomStreamPart(
                "openai.compaction",
                Json("{\"openai\":{\"itemId\":\"cmp_123\"}}")
            ),
            Finish(),
        ]);

        Assert.Equal(2, result.Count);
        var custom = Assert.IsType<CustomPart>(result[0]);
        Assert.Equal("openai.compaction", custom.Kind);
        Assert.Equal(
            "{\"openai\":{\"itemId\":\"cmp_123\"}}",
            custom.ProviderMetadata!.Value.GetRawText()
        );
        var end = ModelCallEnd(result[1]);
        Assert.Equal(FinishReason.Stop, end.FinishReason);
        Assert.Equal(0, end.Performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, end.Performance.EffectiveTotalTokensPerSecond);
        Assert.Null(end.Performance.InputTokensPerSecond);
        Assert.Null(end.Performance.OutputTokensPerSecond);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Null(end.Performance.TimeBetweenOutputChunksMs);
        Assert.Null(end.Performance.TimeToFirstOutputMs);
    }
}
