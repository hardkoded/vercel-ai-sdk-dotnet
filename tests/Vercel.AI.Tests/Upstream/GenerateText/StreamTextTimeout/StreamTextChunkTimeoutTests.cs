// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.StreamTextTimeout;

/// <summary>Port of <c>stream-text-timeout.test.ts</c> &gt; <c>streamText chunk timeout</c>.</summary>
public sealed class StreamTextChunkTimeoutTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text-timeout.test.ts::streamText chunk timeout::should not reset chunkMs for non-output chunks", Coverage = UpstreamCoverage.Covered, Note = "Real timers with a longer chunkMs, because the test cannot advance fake time.")]
    public async Task Should_not_reset_chunkMs_for_non_output_chunks()
    {
        var clock = Stopwatch.StartNew();
        var deltaSentAt = 0L;
        var abortedAt = 0L;
        var model = new DelegateModel
        {
            Stream = (options, _) =>
            {
                options.AbortSignal!.AddAbortHandler(() => abortedAt = clock.ElapsedMilliseconds);
                return Task.FromResult(new LanguageModelStreamResult(ChunksThenStall()));

                async IAsyncEnumerable<LanguageModelStreamPart> ChunksThenStall()
                {
                    yield return new TextStartStreamPart("1");
                    deltaSentAt = clock.ElapsedMilliseconds;
                    yield return new TextDeltaStreamPart("1", "Hello");

                    // Chunks without output do not reset the timeout.
                    await Task.Delay(150);
                    yield return new ResponseMetadataStreamPart("response-1", null, null);
                    await Task.Delay(150);
                    yield return new RawStreamPart("\": ping\"");
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            },
        };

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = new TimeoutConfiguration { ChunkMs = 500 },
            OnError = (_, _) => Task.CompletedTask,
        });

        await Consume(result).WaitAsync(SettleTimeout);

        var signal = Assert.Single(model.Calls).AbortSignal!;
        Assert.True(signal.Aborted);
        Assert.Equal("TimeoutError", Assert.IsType<Vercel.AI.Util.TimeoutError>(signal.Reason).ErrorName);

        // The timeout ran from the text delta. A reset by the two later chunks would add at least 300 ms.
        Assert.True(abortedAt - deltaSentAt < 650, "The chunk timeout fired " + (abortedAt - deltaSentAt) + " ms after the text delta.");
    }
}
