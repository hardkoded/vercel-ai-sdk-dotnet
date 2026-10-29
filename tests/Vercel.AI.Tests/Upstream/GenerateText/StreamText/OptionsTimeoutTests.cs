// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using Vercel.AI.GenerateText;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.timeout</c>.</summary>
public sealed class OptionsTimeoutTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.timeout::";

    private static DelegateModel HelloModel()
    {
        return new DelegateModel { Stream = (_, _) => Task.FromResult(new LanguageModelStreamResult(Yield(HelloParts()))) };
    }

    private static StreamTextResult Stream(
        DelegateModel model,
        TimeoutConfiguration? timeout,
        AbortSignal? signal = null,
        IReadOnlyList<Tool>? tools = null,
        StopCondition? stopWhen = null,
        int? maxRetries = null,
        List<Exception>? errors = null)
    {
        return Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = timeout,
            AbortSignal = signal,
            Tools = tools,
            StopWhen = stopWhen,
            MaxRetries = maxRetries,
            OnError = (error, _) =>
            {
                errors?.Add(error);
                return Task.CompletedTask;
            },
        });
    }

    // Reads the first step with a tool call, then the final response.
    private static DelegateModel TwoStepModel()
    {
        var stepCount = 0;
        return new DelegateModel
        {
            Stream = (_, _) => Task.FromResult(new LanguageModelStreamResult(Yield(++stepCount == 1 ? ToolCallParts() : FinalResponseParts()))),
        };
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward timeout as abort signal to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_timeout_as_abort_signal_to_model()
    {
        var model = HelloModel();

        await Consume(Stream(model, TimeSpan.FromMilliseconds(5000)));

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge timeout with abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_timeout_with_abort_signal()
    {
        var abortController = new AbortController();
        var model = HelloModel();

        await Consume(Stream(model, TimeSpan.FromMilliseconds(5000), abortController.Signal));

        // The merged signal should be different from the original
        var received = Assert.Single(model.Calls).AbortSignal;
        Assert.NotNull(received);
        Assert.NotSame(abortController.Signal, received);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass undefined when no timeout or abortSignal provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_undefined_when_no_timeout_or_abortSignal_provided()
    {
        var model = HelloModel();

        await Consume(Stream(model, null));

        Assert.Null(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw Timeout error when abort signal is aborted", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_Timeout_error_when_abort_signal_is_aborted()
    {
        var delayed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var abortController = new AbortController();
        var model = new DelegateModel
        {
            Stream = async (_, _) =>
            {
                await delayed.Task;
                return new LanguageModelStreamResult(Yield(HelloParts()));
            },
        };

        var result = Stream(model, null, abortController.Signal);

        abortController.Abort();
        delayed.SetResult();

        // The abort error of the port is an OperationCanceledException.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support timeout object with totalMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_timeout_object_with_totalMs()
    {
        var model = HelloModel();

        await Consume(Stream(model, new TimeoutConfiguration { TotalMs = 5000 }));

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge timeout object with abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_timeout_object_with_abort_signal()
    {
        var abortController = new AbortController();
        var model = HelloModel();

        await Consume(Stream(model, new TimeoutConfiguration { TotalMs = 5000 }, abortController.Signal));

        // The merged signal should be different from the original
        var received = Assert.Single(model.Calls).AbortSignal;
        Assert.NotNull(received);
        Assert.NotSame(abortController.Signal, received);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass undefined when timeout object has no totalMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_undefined_when_timeout_object_has_no_totalMs()
    {
        var model = HelloModel();

        await Consume(Stream(model, new TimeoutConfiguration()));

        Assert.Null(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward stepMs as abort signal to each step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_stepMs_as_abort_signal_to_each_step()
    {
        var model = HelloModel();

        await Consume(Stream(model, new TimeoutConfiguration { StepMs = 5000 }));

        Assert.Single(model.Calls);
        Assert.NotNull(model.Calls[0].AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reuse the same abort signal for all steps when stepMs is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reuse_the_same_abort_signal_for_all_steps_when_stepMs_is_set()
    {
        var model = TwoStepModel();

        await Consume(Stream(model, new TimeoutConfiguration { StepMs = 5000 }, tools: new[] { ToolOne() }, stopWhen: StopWhen.IsStepCount(2)));

        Assert.Equal(2, model.Calls.Count);
        Assert.NotNull(model.Calls[0].AbortSignal);
        Assert.NotNull(model.Calls[1].AbortSignal);

        // The same abort signal is reused for all steps (the timeout starts again for each step).
        Assert.Same(model.Calls[0].AbortSignal, model.Calls[1].AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should clear the step timeout on normal completion of a multi-step run", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_clear_the_step_timeout_on_normal_completion_of_a_multi_step_run()
    {
        var model = TwoStepModel();

        // Both steps finish well within stepMs.
        await Consume(Stream(model, new TimeoutConfiguration { StepMs = 150 }, tools: new[] { ToolOne() }, stopWhen: StopWhen.IsStepCount(2)));

        // Wait well past stepMs: because each step's timeout is cleared on finish, the (shared) abort signal
        // must not fire. If the timers leaked, a stale step timer would abort the signal here.
        await Task.Delay(400);

        Assert.Equal(2, model.Calls.Count);
        Assert.False(model.Calls[0].AbortSignal!.Aborted);
        Assert.False(model.Calls[1].AbortSignal!.Aborted);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support both totalMs and stepMs together", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_both_totalMs_and_stepMs_together()
    {
        var model = HelloModel();

        await Consume(Stream(model, new TimeoutConfiguration { TotalMs = 10000, StepMs = 5000 }));

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should abort when step timeout expires", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_abort_when_step_timeout_expires()
    {
        var delayed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Open the stream but stall before producing any content, modelling a model that returns 200 OK and then idles.
        // The step timeout must abort this stalled step.
        var model = new DelegateModel
        {
            Stream = (_, _) => Task.FromResult(new LanguageModelStreamResult(StallAfter(Array.Empty<LanguageModelStreamPart>(), delayed.Task, HelloParts()))),
        };

        var result = Stream(model, new TimeoutConfiguration { StepMs = 50 });

        // Start consuming the stream (it does not complete until the step timeout aborts it).
        var consume = Consume(result);
        await WaitUntil(() => model.Calls.Count == 1 && model.Calls[0].AbortSignal!.Aborted);

        // Resolve the delayed promise to allow the stream to finish
        delayed.SetResult();
        await consume.WaitAsync(SettleTimeout);

        // The abort signal should have been triggered due to step timeout
        var signal = model.Calls[0].AbortSignal!;
        Assert.True(signal.Aborted);
        Assert.Equal("TimeoutError", Assert.IsType<TimeoutError>(signal.Reason).ErrorName);
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward chunkMs as abort signal to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_chunkMs_as_abort_signal_to_model()
    {
        var model = HelloModel();

        await Consume(Stream(model, new TimeoutConfiguration { ChunkMs = 5000 }));

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should complete successfully when chunks arrive within chunkMs timeout", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_complete_successfully_when_chunks_arrive_within_chunkMs_timeout()
    {
        var model = new DelegateModel
        {
            Stream = (_, _) => Task.FromResult(new LanguageModelStreamResult(Yield(new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello"),
                new TextDeltaStreamPart("1", " World"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, Usage, "stop"),
            }))),
        };

        await Consume(Stream(model, new TimeoutConfiguration { ChunkMs = 5000 }));

        // Stream should complete successfully without abort
        Assert.False(Assert.Single(model.Calls).AbortSignal!.Aborted);
    }

    [Fact]
    [UpstreamTest(Prefix + "should abort when chunk timeout expires", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_abort_when_chunk_timeout_expires()
    {
        var delayed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Send the first chunk immediately, then wait for the stream to stall (resolved after the timeout fires).
        var model = new DelegateModel
        {
            Stream = (_, _) => Task.FromResult(new LanguageModelStreamResult(StallAfter(
                new LanguageModelStreamPart[] { new TextStartStreamPart("1"), new TextDeltaStreamPart("1", "Hello") },
                delayed.Task,
                new LanguageModelStreamPart[] { new TextEndStreamPart("1"), new FinishStreamPart(FinishReason.Stop, Usage, "stop") }))),
        };

        var result = Stream(model, new TimeoutConfiguration { ChunkMs = 50 });

        // Start consuming the stream (it does not complete until the chunk timeout aborts it).
        var consume = Consume(result);
        await WaitUntil(() => model.Calls.Count == 1 && model.Calls[0].AbortSignal!.Aborted);

        // Resolve the delayed promise to allow the stream to finish
        delayed.SetResult();
        await consume.WaitAsync(SettleTimeout);

        // The abort signal should have been triggered due to chunk timeout
        var signal = model.Calls[0].AbortSignal!;
        Assert.True(signal.Aborted);
        Assert.Equal("TimeoutError", Assert.IsType<TimeoutError>(signal.Reason).ErrorName);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support chunkMs alongside totalMs and stepMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_chunkMs_alongside_totalMs_and_stepMs()
    {
        var model = HelloModel();

        await Consume(Stream(model, new TimeoutConfiguration { TotalMs = 30000, StepMs = 10000, ChunkMs = 5000 }));

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should clean up step timeout when doStream throws an error", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_clean_up_step_timeout_when_doStream_throws_an_error()
    {
        var model = new DelegateModel { Stream = (_, _) => throw new InvalidOperationException("Fail immediately") };
        var errors = new List<Exception>();

        var result = Stream(model, new TimeoutConfiguration { StepMs = 10000 }, maxRetries: 0, errors: errors);

        await Consume(result);

        Assert.Equal("Fail immediately", Assert.Single(errors).Message);
        Assert.Equal(
            "No output generated. Check the stream for errors.",
            (await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Text)).Message);
    }
}
