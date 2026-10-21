// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.StreamTextTimeout;

/// <summary>
/// Port of <c>stream-text-timeout.test.ts</c> &gt; <c>streamText model output timeout boundaries</c>. Only the toolMs test is ported here.
/// The test uses real timers with short budgets, because the port has no fake clock.
/// </summary>
public sealed class StreamTextModelOutputTimeoutBoundariesTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text-timeout.test.ts::streamText model output timeout boundaries::";

    [Fact]
    [UpstreamTest(Prefix + "should keep toolMs active after the model finishes", Coverage = UpstreamCoverage.Covered, Note = "The budgets are scaled up for real timers: toolMs 300, firstChunkMs 150, chunkMs 150, and a tool that waits 600 ms.")]
    public async Task Should_keep_toolMs_active_after_the_model_finishes()
    {
        AbortSignal? signal = null;
        var model = new DelegateModel
        {
            Stream = (options, _) =>
            {
                signal = options.AbortSignal;
                return Task.FromResult(new LanguageModelStreamResult(Yield(new LanguageModelStreamPart[]
                {
                    new ToolCallStreamPart("call-1", "slow", "{}"),
                    new FinishStreamPart(FinishReason.ToolCalls, Usage, "tool-calls"),
                })));
            },
        };
        var parts = new List<TextStreamPart>();

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test",
            Tools = new[]
            {
                Tool.Function("slow", null, "{\"type\":\"object\",\"properties\":{}}", async (_, token) =>
                {
                    await Task.Delay(600, token);
                    return "\"done\"";
                }),
            },
            Timeout = new TimeoutConfiguration { ToolMs = 300, FirstChunkMs = 150, ChunkMs = 150 },
        });
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.False(signal?.Aborted);
        Assert.Contains(parts, part => part is ToolErrorPart);
        Assert.IsType<FinishPart>(parts[^1]);
    }
}
