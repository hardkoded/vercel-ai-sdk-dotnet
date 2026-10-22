// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.StreamTextTimeout;

/// <summary>
/// Port of <c>stream-text-timeout.test.ts</c> &gt; <c>streamText model output timeout boundaries</c>.
/// The tests use real timers with scaled budgets, because the port has no fake clock.
/// The tests that need stream retries, a streaming tool, or request retries are not ported, because the port has none of them.
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

    private static readonly LanguageModelStreamPart[] ToolChunks = new LanguageModelStreamPart[]
    {
        new ToolCallStreamPart("call-1", "slow", "{}"),
        new FinishStreamPart(FinishReason.ToolCalls, Usage, "tool-calls"),
    };

    private static Tool SlowTool(int delayMs)
    {
        return Tool.Function("slow", null, "{\"type\":\"object\",\"properties\":{}}", async (_, token) =>
        {
            await Task.Delay(delayMs, token);
            return "\"done\"";
        });
    }

    [Theory]
    [InlineData("stepMs")]
    [InlineData("totalMs")]
    [UpstreamTest(Prefix + "should keep ${budget} active during local tool execution", Coverage = UpstreamCoverage.Covered, Note = "The budgets are scaled up for real timers: the budget 300, firstChunkMs 150, chunkMs 150, and a tool that waits 3000 ms.")]
    public async Task Should_keep_budget_active_during_local_tool_execution(string budget)
    {
        AbortSignal? signal = null;
        var model = new DelegateModel
        {
            Stream = (options, _) =>
            {
                signal = options.AbortSignal;
                return Task.FromResult(new LanguageModelStreamResult(Yield(ToolChunks)));
            },
        };
        var parts = new List<TextStreamPart>();

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test",
            Tools = new[] { SlowTool(3000) },
            Timeout = new TimeoutConfiguration
            {
                StepMs = budget == "stepMs" ? 300 : null,
                TotalMs = budget == "totalMs" ? 300 : null,
                FirstChunkMs = 150,
                ChunkMs = 150,
            },
            OnError = (_, _) => Task.CompletedTask,
        });
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        var reason = Assert.IsType<Vercel.AI.Util.TimeoutError>(signal?.Reason);
        Assert.Equal("TimeoutError", reason.ErrorName);
        if (budget == "stepMs")
        {
            Assert.Equal("Step timeout of 300ms exceeded", reason.Message);
        }
        else
        {
            Assert.Equal("The operation was aborted due to timeout", reason.Message);
        }

        Assert.IsType<AbortPart>(parts[^1]);
    }

    [Theory]
    [InlineData("finish")]
    [InlineData("error")]
    [InlineData("close")]
    [UpstreamTest(Prefix + "should clear output timers on ${terminal} before slow stream processing", Coverage = UpstreamCoverage.Covered, Note = "The budgets are scaled up for real timers: firstChunkMs 300, chunkMs 300, and callbacks that wait 700 ms.")]
    public async Task Should_clear_output_timers_on_terminal_before_slow_stream_processing(string terminal)
    {
        AbortSignal? signal = null;
        var parts = terminal switch
        {
            "finish" => new LanguageModelStreamPart[] { new FinishStreamPart(FinishReason.Stop, Usage, "stop") },
            "error" => new LanguageModelStreamPart[] { new ErrorStreamPart("provider error") },
            _ => HelloParts().Take(3).ToArray(),
        };
        var model = new DelegateModel
        {
            Stream = (options, _) =>
            {
                signal = options.AbortSignal;
                return Task.FromResult(new LanguageModelStreamResult(Yield(parts)));
            },
        };

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test",
            Timeout = new TimeoutConfiguration { FirstChunkMs = 300, ChunkMs = 300 },
            OnError = (_, token) => Task.Delay(700, token),
            OnStepEnd = (_, token) => Task.Delay(700, token),
        });
        try
        {
            await Consume(result).WaitAsync(SettleTimeout);
        }
        catch (AiSdkException) when (terminal == "error")
        {
            // Upstream consumeStream swallows the provider error. Reading the stream here rethrows it.
        }

        Assert.False(signal?.Aborted);
    }

    [Fact]
    [UpstreamTest(Prefix + "should start chunkMs only after output begins in the next ${continuation}", Coverage = UpstreamCoverage.Partial, Note = "Only the step case is ported: the retry case needs streamRetries, which the port does not have. The budgets are scaled up for real timers: tool 300 ms, second call 300 ms before its first chunk, firstChunkMs 450, chunkMs 150. The test measures the time from the first chunk to the abort instead of an instant on a fake clock.")]
    public async Task Should_start_chunkMs_only_after_output_begins_in_the_next_step()
    {
        var clock = Stopwatch.StartNew();
        var attempts = 0;
        var outputAt = 0L;
        var abortedAt = 0L;
        AbortSignal? signal = null;
        var model = new DelegateModel
        {
            Stream = (options, _) =>
            {
                signal = options.AbortSignal;
                attempts++;
                if (attempts == 1)
                {
                    return Task.FromResult(new LanguageModelStreamResult(Yield(ToolChunks)));
                }

                options.AbortSignal!.AddAbortHandler(() => abortedAt = clock.ElapsedMilliseconds);
                return Task.FromResult(new LanguageModelStreamResult(NextStep()));

                async IAsyncEnumerable<LanguageModelStreamPart> NextStep()
                {
                    await Task.Delay(300);
                    outputAt = clock.ElapsedMilliseconds;
                    yield return new TextStartStreamPart("2");
                    yield return new TextDeltaStreamPart("2", "Next");
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            },
        };
        var parts = new List<TextStreamPart>();

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test",
            Tools = new[] { SlowTool(300) },
            Timeout = new TimeoutConfiguration { FirstChunkMs = 450, ChunkMs = 150 },
            StopWhen = StopWhen.IsStepCount(2),
            OnError = (_, _) => Task.CompletedTask,
        });
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Equal(2, attempts);
        Assert.Equal("Chunk timeout of 150ms exceeded", Assert.IsType<Vercel.AI.Util.TimeoutError>(signal?.Reason).Message);
        Assert.IsType<AbortPart>(parts[^1]);

        // The timeout ran from the first chunk. A timeout that ran from the start of the call would have fired before the first chunk.
        Assert.True(abortedAt - outputAt >= 100, "The chunk timeout fired " + (abortedAt - outputAt) + " ms after the first chunk.");
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep chunkMs active during provider-executed tools without resetting on their results", Coverage = UpstreamCoverage.Partial, Note = "The tool result has no preliminary flag, because the port's tool-result stream part has none. The budgets are scaled up for real timers: chunkMs 300 and a result after 240 ms.")]
    public async Task Should_keep_chunkMs_active_during_provider_executed_tools_without_resetting_on_their_results()
    {
        var clock = Stopwatch.StartNew();
        var callAt = 0L;
        var abortedAt = 0L;
        AbortSignal? signal = null;
        var model = new DelegateModel
        {
            Stream = (options, _) =>
            {
                signal = options.AbortSignal;
                options.AbortSignal!.AddAbortHandler(() => abortedAt = clock.ElapsedMilliseconds);
                return Task.FromResult(new LanguageModelStreamResult(ToolThenStall(options.AbortSignal!)));
            },
        };

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test",
            Tools = new[] { Tool.Function("slow", null, "{\"type\":\"object\",\"properties\":{}}", null) },
            Timeout = new TimeoutConfiguration { ChunkMs = 300 },
            OnError = (_, _) => Task.CompletedTask,
        });
        await Consume(result).WaitAsync(SettleTimeout);

        Assert.Equal("Chunk timeout of 300ms exceeded", Assert.IsType<Vercel.AI.Util.TimeoutError>(signal?.Reason).Message);

        // The timeout ran from the tool call. A reset by the result after 240 ms would put the abort at least 540 ms after the call.
        Assert.True(abortedAt - callAt < 450, "The chunk timeout fired " + (abortedAt - callAt) + " ms after the tool call.");

        async IAsyncEnumerable<LanguageModelStreamPart> ToolThenStall(AbortSignal signal)
        {
            callAt = clock.ElapsedMilliseconds;
            yield return new ToolCallStreamPart("call-1", "slow", "{}") { ProviderExecuted = true };
            await Task.Delay(240);
            yield return new ToolResultStreamPart("call-1", "slow", JsonDocument.Parse("\"progress\"").RootElement);
            var aborted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            signal.AddAbortHandler(() => aborted.TrySetResult());
            await aborted.Task;
        }
    }
}
