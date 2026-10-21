// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.StreamTextTimeout;

/// <summary>
/// Port of <c>stream-text-timeout.test.ts</c> &gt; <c>streamText first chunk timeout</c>.
/// The tests use real timers with a short firstChunkMs, because the port has no fake clock.
/// </summary>
public sealed class StreamTextFirstChunkTimeoutTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text-timeout.test.ts::streamText first chunk timeout::";

    private static readonly TimeSpan Overrun = TimeSpan.FromMilliseconds(300);

    private static StreamTextResult Stream(DelegateModel model, IReadOnlyList<Tool>? tools = null, StopCondition? stopWhen = null)
    {
        return Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = tools,
            StopWhen = stopWhen,
            Timeout = new TimeoutConfiguration { FirstChunkMs = 50 },
            OnError = (_, _) => Task.CompletedTask,
        });
    }

    // Yields the parts, then waits until the call is aborted. The wait ends with the abort, like a provider stream that errors on abort.
    private static async IAsyncEnumerable<LanguageModelStreamPart> PartsThenStall(IEnumerable<LanguageModelStreamPart> parts, Task aborted)
    {
        foreach (var part in parts)
        {
            yield return part;
        }

        await aborted.ConfigureAwait(false);
    }

    private static Task AbortedTask(AbortSignal signal)
    {
        var aborted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        signal.AddAbortHandler(() => aborted.TrySetResult());
        return aborted.Task;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Fail(Exception error, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        throw error;
#pragma warning disable CS0162
        yield break;
#pragma warning restore CS0162
    }

    [Fact]
    [UpstreamTest(Prefix + "should abort when only non-output chunks arrive before firstChunkMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_abort_when_only_non_output_chunks_arrive_before_firstChunkMs()
    {
        var model = new DelegateModel
        {
            Stream = (options, _) => Task.FromResult(new LanguageModelStreamResult(PartsThenStall(
                new LanguageModelStreamPart[]
                {
                    new StreamStartStreamPart(Array.Empty<CallWarning>()),
                    new ResponseMetadataStreamPart("response-1", null, null),
                    new TextStartStreamPart("1"),
                    new TextDeltaStreamPart("1", string.Empty),
                    new ReasoningStartStreamPart("2"),
                    new ReasoningDeltaStreamPart("2", string.Empty),
                    new ToolInputStartStreamPart("call-1", "tool1"),
                    new ToolInputDeltaStreamPart("call-1", string.Empty),
                    new RawStreamPart("\": ping\""),
                },
                AbortedTask(options.AbortSignal!)))),
        };

        var result = Stream(model);

        var error = await Assert.ThrowsAnyAsync<Exception>(() => result.Text.WaitAsync(SettleTimeout));

        var signal = Assert.Single(model.Calls).AbortSignal!;
        Assert.True(signal.Aborted);
        var reason = Assert.IsType<Vercel.AI.Util.TimeoutError>(signal.Reason);
        Assert.Equal("TimeoutError", reason.ErrorName);
        Assert.Equal("First chunk timeout of 50ms exceeded", reason.Message);
        Assert.Equal("TimeoutError", Assert.IsType<Vercel.AI.Util.TimeoutError>(error).ErrorName);
    }

    private static LanguageModelStreamPart[] OutputChunks(string name)
    {
        return name switch
        {
            "text delta" => new LanguageModelStreamPart[] { new TextStartStreamPart("1"), new TextDeltaStreamPart("1", "Hello") },
            "reasoning delta" => new LanguageModelStreamPart[] { new ReasoningStartStreamPart("1"), new ReasoningDeltaStreamPart("1", "Thinking") },
            "tool input delta" => new LanguageModelStreamPart[] { new ToolInputStartStreamPart("call-1", "tool1"), new ToolInputDeltaStreamPart("call-1", "{\"value\":") },
            "tool call" => new LanguageModelStreamPart[] { new ToolCallStreamPart("call-1", "tool1", "{\"value\":\"test\"}") },
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };
    }

    [Theory]
    [InlineData("text delta")]
    [InlineData("reasoning delta")]
    [InlineData("tool input delta")]
    [InlineData("tool call")]
    [UpstreamTest(Prefix + "should disarm firstChunkMs before forwarding the first ${name}", Coverage = UpstreamCoverage.Partial, Note = "The file and reasoning file cases are not ported: the port has no file stream part.")]
    public async Task Should_disarm_firstChunkMs_before_forwarding_the_first_output_chunk(string name)
    {
        var finishStream = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var model = new DelegateModel
        {
            Stream = (_, _) => Task.FromResult(new LanguageModelStreamResult(StallAfter(
                OutputChunks(name),
                finishStream.Task,
                new LanguageModelStreamPart[] { new FinishStreamPart(FinishReason.Stop, Usage, "stop") }))),
        };

        var consume = Consume(Stream(model));

        await Task.Delay(Overrun);

        Assert.False(Assert.Single(model.Calls).AbortSignal!.Aborted);

        finishStream.SetResult();
        await consume.WaitAsync(SettleTimeout);
    }

    [Fact]
    [UpstreamTest(Prefix + "should re-arm firstChunkMs for each model-call step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_re_arm_firstChunkMs_for_each_model_call_step()
    {
        var stepCount = 0;
        var secondStepStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var model = new DelegateModel
        {
            Stream = (options, _) =>
            {
                if (++stepCount == 1)
                {
                    return Task.FromResult(new LanguageModelStreamResult(Yield(ToolCallParts())));
                }

                secondStepStarted.TrySetResult();
                return Task.FromResult(new LanguageModelStreamResult(PartsThenStall(
                    new LanguageModelStreamPart[]
                    {
                        new ResponseMetadataStreamPart("response-2", null, null),
                        new TextStartStreamPart("2"),
                    },
                    AbortedTask(options.AbortSignal!))));
            },
        };

        var consume = Consume(Stream(model, new[] { ToolOne() }, StopWhen.IsStepCount(2)));

        await secondStepStarted.Task.WaitAsync(SettleTimeout);
        await Task.Delay(Overrun);

        Assert.Equal(2, stepCount);
        var signal = model.Calls[1].AbortSignal!;
        Assert.True(signal.Aborted);
        Assert.Equal("TimeoutError", Assert.IsType<Vercel.AI.Util.TimeoutError>(signal.Reason).ErrorName);

        await consume.WaitAsync(SettleTimeout);
    }

    [Fact]
    [UpstreamTest(Prefix + "should clear firstChunkMs when the provider stream errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_clear_firstChunkMs_when_the_provider_stream_errors()
    {
        var providerError = new InvalidOperationException("simulated provider stream error");
        var model = new DelegateModel
        {
            Stream = (_, _) => Task.FromResult(new LanguageModelStreamResult(Fail(providerError))),
        };

        var result = Stream(model);
        var text = result.Text;

        // consumeStream swallows the error. Here the stream rethrows it.
        await Assert.ThrowsAnyAsync<Exception>(() => Consume(result).WaitAsync(SettleTimeout));
        var error = await Assert.ThrowsAnyAsync<Exception>(() => text.WaitAsync(SettleTimeout));
        Assert.Contains(providerError.Message, error.ToString());

        var signal = Assert.Single(model.Calls).AbortSignal!;
        Assert.False(signal.Aborted);

        await Task.Delay(Overrun);

        Assert.False(signal.Aborted);
    }
}
