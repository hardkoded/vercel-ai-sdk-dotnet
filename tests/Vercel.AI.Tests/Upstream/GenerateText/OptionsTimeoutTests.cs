// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>
/// Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.timeout</c>.
/// A call has no <c>abortSignal</c> option, so the <see cref="CancellationToken"/> stands in for it.
/// </summary>
[Collection("LogWarnings")]
public sealed class OptionsTimeoutTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.timeout::";

    private static DelegateModel TextModel()
    {
        return new DelegateModel { Generate = (_, _) => Task.FromResult(TextResult("Hello, world!")) };
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn before the model responds when $warning.feature is configured", Coverage = UpstreamCoverage.Partial, Note = "Only the chunkMs case. TimeoutConfiguration has no firstChunkMs.")]
    public async Task Should_warn_before_the_model_responds_when_chunkMs_is_configured()
    {
        var logged = new List<LogWarningsOptions>();
        using var recorder = new Upstream.Logger.LogWarningsRecorder((Action<LogWarningsOptions>)logged.Add);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var called = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var model = new DelegateModel
        {
            Generate = async (_, _) =>
            {
                called.TrySetResult();
                await gate.Task;
                return TextResult("Hello, world!");
            },
        };

        var generation = Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = new TimeoutConfiguration { ChunkMs = 5000 },
        });

        await called.Task.WaitAsync(SettleTimeout);
        var warned = Assert.Single(logged);
        var warning = Assert.IsType<UnsupportedWarning>(Assert.Single(warned.Warnings));
        Assert.Equal("timeout.chunkMs", warning.Feature);
        Assert.Equal("The chunkMs timeout is only supported by streaming functions.", warning.Details);
        Assert.Equal("mock-provider", warned.Provider);
        Assert.Equal("mock-model-id", warned.Model);

        gate.SetResult();
        await generation;
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward timeout as abort signal to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_timeout_as_abort_signal_to_model()
    {
        var model = TextModel();

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = TimeSpan.FromMilliseconds(5000),
        });

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge timeout with abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_timeout_with_abort_signal()
    {
        using var abort = new CancellationTokenSource();
        var model = TextModel();

        await Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Timeout = TimeSpan.FromMilliseconds(5000),
            },
            abort.Token);

        // The merged signal differs from the caller's token.
        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
        Assert.NotEqual(abort.Token, Assert.Single(model.Tokens));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass undefined when no timeout or abortSignal provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_undefined_when_no_timeout_or_abortSignal_provided()
    {
        var model = TextModel();

        await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "test-input" });

        Assert.Null(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward timeout abort signal to tool execution", Coverage = UpstreamCoverage.Partial, Note = "A tool Execute delegate receives the input and the token, not toolCallId, messages, or context.")]
    public async Task Should_forward_timeout_abort_signal_to_tool_execution()
    {
        var toolCalls = new List<(string Input, CancellationToken Token)>();
        var model = new DelegateModel { Generate = (_, _) => Task.FromResult(ToolCallResult("{ \"value\": \"value\" }")) };

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                ToolOne((input, token) =>
                {
                    toolCalls.Add((input.GetProperty("value").GetString()!, token));
                    return Task.FromResult("\"tool result\"");
                }),
            },
            Timeout = TimeSpan.FromMilliseconds(5000),
        });

        var call = Assert.Single(toolCalls);
        Assert.Equal("value", call.Input);
        Assert.True(call.Token.CanBeCanceled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support timeout object with totalMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_timeout_object_with_totalMs()
    {
        var model = TextModel();

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = new TimeoutConfiguration { TotalMs = 5000 },
        });

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge timeout object with abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_timeout_object_with_abort_signal()
    {
        using var abort = new CancellationTokenSource();
        var model = TextModel();

        await Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Timeout = new TimeoutConfiguration { TotalMs = 5000 },
            },
            abort.Token);

        // The merged signal differs from the caller's token.
        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
        Assert.NotEqual(abort.Token, Assert.Single(model.Tokens));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass undefined when timeout object has no totalMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_undefined_when_timeout_object_has_no_totalMs()
    {
        var model = TextModel();

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = new TimeoutConfiguration(),
        });

        Assert.Null(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward stepMs as abort signal to each step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_stepMs_as_abort_signal_to_each_step()
    {
        var model = TextModel();

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = new TimeoutConfiguration { StepMs = 5000 },
        });

        Assert.Single(model.Calls);
        Assert.NotNull(model.Calls[0].AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reuse the same abort signal for all steps when stepMs is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reuse_the_same_abort_signal_for_all_steps_when_stepMs_is_set()
    {
        var stepCount = 0;
        var model = new DelegateModel
        {
            Generate = (_, _) => Task.FromResult(++stepCount == 1 ? ToolCallResult("{ \"value\": \"test\" }") : TextResult("Final response")),
        };

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { ToolOne() },
            Timeout = new TimeoutConfiguration { StepMs = 5000 },
            StopWhen = StopWhen.IsStepCount(2),
        });

        Assert.Equal(2, model.Calls.Count);
        Assert.NotNull(model.Calls[0].AbortSignal);
        Assert.NotNull(model.Calls[1].AbortSignal);

        // The same abort signal is reused for all steps (the timeout starts again for each step).
        Assert.Same(model.Calls[0].AbortSignal, model.Calls[1].AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward stepMs abort signal to tool execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_stepMs_abort_signal_to_tool_execution()
    {
        CancellationToken? toolAbortSignal = null;
        var model = new DelegateModel { Generate = (_, _) => Task.FromResult(ToolCallResult("{ \"value\": \"test\" }")) };

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                ToolOne((_, token) =>
                {
                    toolAbortSignal = token;
                    return Task.FromResult("\"tool result\"");
                }),
            },
            Timeout = new TimeoutConfiguration { StepMs = 5000 },
        });

        Assert.True(toolAbortSignal?.CanBeCanceled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support both totalMs and stepMs together", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_both_totalMs_and_stepMs_together()
    {
        var model = TextModel();

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = new TimeoutConfiguration { TotalMs = 10000, StepMs = 5000 },
        });

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should abort when step timeout expires", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_abort_when_step_timeout_expires()
    {
        var delayed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var model = new DelegateModel
        {
            Generate = async (_, _) =>
            {
                await delayed.Task;
                return TextResult("Hello, world!");
            },
        };

        var generation = Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Timeout = new TimeoutConfiguration { StepMs = 50 },
            MaxRetries = 0,
        });

        // The step timeout fires and aborts the signal the model received.
        await WaitUntil(() => model.Calls.Count == 1 && model.Calls[0].AbortSignal!.Aborted);

        // Let the model finish, so the test cleans up.
        delayed.SetResult();
        try
        {
            await generation;
        }
        catch (Exception)
        {
            // The call may fail with the timeout. The test checks the signal.
        }

        var signal = model.Calls[0].AbortSignal!;
        Assert.True(signal.Aborted);
        Assert.Equal("TimeoutError", Assert.IsType<TimeoutError>(signal.Reason).ErrorName);
    }
}
