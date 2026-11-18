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

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; text parts</c>.</summary>
public sealed class TextPartsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > text parts::";

    [Fact]
    [UpstreamTest(Prefix + "should convert text to delta", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_text_to_delta()
    {
        var result = await StreamLanguageModelCallResult([
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "text"),
            new TextEndStreamPart("1"),
            Finish(),
        ]);

        Assert.Equal(4, result.Count);
        Assert.Equal("1", Assert.IsType<TextStartPart>(result[0]).Id);
        var delta = Assert.IsType<TextDeltaPart>(result[1]);
        Assert.Equal("1", delta.Id);
        Assert.Equal("text", delta.Text);
        Assert.Null(delta.ProviderMetadata);
        Assert.Equal("1", Assert.IsType<TextEndPart>(result[2]).Id);
        var end = ModelCallEnd(result[3]);
        Assert.Equal(FinishReason.Stop, end.FinishReason);
        Assert.Equal("stop", end.RawFinishReason);
        Assert.Equal(0, end.Performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, end.Performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(0, end.Performance.InputTokensPerSecond);
        Assert.Equal(0, end.Performance.OutputTokensPerSecond);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Null(end.Performance.TimeBetweenOutputChunksMs);
        Assert.Equal(0, end.Performance.TimeToFirstOutputMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should measure time to first output from text deltas",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_measure_time_to_first_output_from_text_deltas()
    {
        var result = await StreamLanguageModelCallResult(
            [
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "text"),
                new TextEndStreamPart("1"),
                Finish(),
            ],
            now: Now(1000, 1250, 1600)
        );

        AssertTimeToFirstOutput(ModelCallEnd(result[^1]));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should measure time between output chunks from text deltas",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_measure_time_between_output_chunks_from_text_deltas()
    {
        var result = await StreamLanguageModelCallResult(
            [
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "a"),
                new TextDeltaStreamPart("1", "b"),
                new TextDeltaStreamPart("1", "c"),
                new TextDeltaStreamPart("1", "d"),
                new TextDeltaStreamPart("1", "e"),
                new TextDeltaStreamPart("1", "f"),
                new TextEndStreamPart("1"),
                Finish(),
            ],
            now: Now(1000, 1100, 1200, 1500, 1600, 2000, 2500, 2500)
        );

        var gaps = ModelCallEnd(result[^1]).Performance.TimeBetweenOutputChunksMs!;
        Assert.Equal(100, gaps.Min);
        Assert.Equal(100, gaps.P10);
        Assert.Equal(300, gaps.Median);
        Assert.Equal(280, gaps.Avg);
        Assert.Equal(500, gaps.P90);
        Assert.Equal(500, gaps.Max);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should measure time between all output chunk types",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_measure_time_between_all_output_chunk_types()
    {
        var result = await StreamLanguageModelCallResult(
            [
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "text"),
                new FileStreamPart(Encoding.UTF8.GetBytes("file"), "text/plain"),
                new ReasoningFileStreamPart(Encoding.UTF8.GetBytes("reasoning"), "text/plain"),
                new ToolCallStreamPart("call-1", "testTool", "{ \"value\": \"test\" }"),
                Finish(FinishReason.ToolCalls, "tool-calls"),
            ],
            tools: [TestTool()],
            now: Now(1000, 1100, 1300, 1600, 2000, 2500)
        );

        var performance = ModelCallEnd(result[^1]).Performance;
        Assert.Equal(100, performance.TimeToFirstOutputMs);
        var gaps = performance.TimeBetweenOutputChunksMs!;
        Assert.Equal(200, gaps.Min);
        Assert.Equal(200, gaps.P10);
        Assert.Equal(300, gaps.Median);
        Assert.Equal(300, gaps.Avg);
        Assert.Equal(400, gaps.P90);
        Assert.Equal(400, gaps.Max);
    }
}
