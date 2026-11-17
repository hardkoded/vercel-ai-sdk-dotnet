// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.StreamLanguageModelCall;

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; reasoning parts</c>.</summary>
public sealed class ReasoningPartsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > reasoning parts::";

    [Fact]
    [UpstreamTest(
        Prefix + "should measure time to first output from reasoning-file parts",
        Coverage = UpstreamCoverage.Partial,
        Note = "Runs through StreamText and reads the step performance. The port has no StreamLanguageModelCall function, so there is no model-call-end part."
    )]
    public async Task Should_measure_time_to_first_output_from_reasoning_file_parts_through_StreamText()
    {
        var times = new Queue<double>([1000, 1250, 1600]);
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ReasoningFileStreamPart(
                    System.Text.Encoding.UTF8.GetBytes("reasoning"),
                    "text/plain"
                ),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };

        var result = new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "test-input",
                NowMs = () => times.Count > 0 ? times.Dequeue() : 1600,
            }
        );
        var step = Assert.Single(await result.Steps);

        var performance = step.Performance;
        Assert.Equal(600, performance.ResponseTimeMs);
        Assert.Equal(250, performance.TimeToFirstOutputMs);
        Assert.Equal(16.666666666666668, performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(28.571428571428573, performance.OutputTokensPerSecond);
        Assert.Equal(12, performance.InputTokensPerSecond);
        Assert.Equal(21.666666666666668, performance.EffectiveTotalTokensPerSecond);
    }
}
