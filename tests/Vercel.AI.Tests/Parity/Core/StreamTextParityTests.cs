// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class StreamTextParityTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.textStream::should filter out empty text deltas",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Text_stream_filters_empty_deltas()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", string.Empty),
                new TextDeltaStreamPart("1", "Hello"),
                new TextDeltaStreamPart("1", string.Empty),
                new TextDeltaStreamPart("1", ", "),
                new TextDeltaStreamPart("1", string.Empty),
                new TextDeltaStreamPart("1", "world!"),
                new TextDeltaStreamPart("1", string.Empty),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input" });
        var deltas = new List<string>();
        await foreach (var delta in stream.TextStream())
        {
            deltas.Add(delta);
        }

        Assert.Equal(new[] { "Hello", ", ", "world!" }, deltas);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.textStream::should not include reasoning content in textStream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Text_stream_omits_reasoning()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new ReasoningDeltaStreamPart("1", "I will open the conversation"),
                new ReasoningDeltaStreamPart("1", " with witty banter."),
                new TextDeltaStreamPart("2", "Hi"),
                new TextDeltaStreamPart("2", " there!"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "prompt" });
        var deltas = new List<string>();
        await foreach (var delta in stream.TextStream())
        {
            deltas.Add(delta);
        }

        Assert.Equal(new[] { "Hi", " there!" }, deltas);
        Assert.Equal("I will open the conversation with witty banter.", await stream.ReasoningText);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.text::should resolve with full text",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Text_promise_resolves_to_the_joined_deltas()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("1", "Hello"),
                new TextDeltaStreamPart("1", ", "),
                new TextDeltaStreamPart("1", "world!"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input" });
        Assert.Equal("Hello, world!", await stream.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.usage::should resolve with token usage",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Usage_promise_resolves_with_the_finish_usage()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("1", "Hello"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input" });
        var usage = await stream.Usage;
        Assert.Equal(3, usage.InputTokens);
        Assert.Equal(10, usage.OutputTokens);
        Assert.Equal(13, usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.reasoningText::should contain reasoning text from model response",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_text_joins_reasoning_deltas()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new ReasoningDeltaStreamPart("1", "I will open the conversation"),
                new ReasoningDeltaStreamPart("1", " with witty banter."),
                new TextDeltaStreamPart("2", "Hi"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "prompt" });
        Assert.Equal("I will open the conversation with witty banter.", await stream.ReasoningText);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.sources::should contain sources",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_result_contains_sources()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new SourceStreamPart("source-1", "https://example.com/a", "Source A"),
                new TextDeltaStreamPart("1", "cited"),
                new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty, "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "prompt" });
        var source = Assert.Single(await stream.Sources);
        Assert.Equal("https://example.com/a", source.Url);
        Assert.Equal("Source A", source.Title);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.sources::should contain sources from all steps",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_result_contains_sources_from_every_step()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = call => call == 1
                ? new LanguageModelStreamPart[]
                {
                    new SourceStreamPart("source-0", "https://example.com/0", "Source 0"),
                    new ToolCallStreamPart("c1", "lookup", "{}"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(10, 5, 15), "tool_calls"),
                }
                : new LanguageModelStreamPart[]
                {
                    new TextDeltaStreamPart("1", "done"),
                    new SourceStreamPart("source-1", "https://example.com/1", "Source 1"),
                    new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
                },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "prompt",
            StopWhen = StopWhen.IsStepCount(2),
            Tools = new[] { Tool.Function("lookup", "Looks up", "{}", (_, _) => Task.FromResult("\"ok\"")) },
        });
        Assert.Equal(new[] { "source-0", "source-1" }, (await stream.Sources).Select(source => source.Id).ToArray());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.toolCalls::should resolve with tool calls",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.toolResults::should resolve with tool results",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_result_resolves_tool_calls_and_results()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "lookup", "{\"q\":\"x\"}"),
                new FinishStreamPart(FinishReason.ToolCalls, LanguageModelUsage.Empty, "tool_calls"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "find",
            Tools = new[] { Tool.Function("lookup", "Looks up", "{}", (_, _) => Task.FromResult("\"ok\"")) },
        });
        var steps = await stream.Steps;
        Assert.Equal("call-1", Assert.Single(steps[0].ToolCalls).ToolCallId);
        Assert.Equal("\"ok\"", Assert.Single(steps[0].ToolResults).OutputJson);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.toolCalls::should resolve with tool calls from all steps",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.toolResults::should resolve with tool results from all steps",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_result_keeps_tool_calls_from_every_step()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = call => call == 1
                ? new LanguageModelStreamPart[]
                {
                    new ToolCallStreamPart("call-1", "lookup", "{}"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(10, 5, 15), "tool_calls"),
                }
                : new LanguageModelStreamPart[]
                {
                    new ToolCallStreamPart("call-2", "lookup", "{}"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13), "tool_calls"),
                },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "find",
            StopWhen = StopWhen.IsStepCount(2),
            Tools = new[] { Tool.Function("lookup", "Looks up", "{}", (_, _) => Task.FromResult("\"ok\"")) },
        });
        var steps = await stream.Steps;
        Assert.Equal(new[] { "call-1", "call-2" }, steps.SelectMany(step => step.ToolCalls).Select(call => call.ToolCallId).ToArray());
        Assert.Equal(2, steps.SelectMany(step => step.ToolResults).Count());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 steps: initial, tool-result > value promises::result.totalUsage should contain total token usage",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 steps: initial, tool-result > value promises::result.usage should contain total token usage",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 steps: initial, tool-result > value promises::result.finalStep.usage should contain token usage from final step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 steps: initial, tool-result > value promises::onFinishResult.usage should sum token usage and finalStep should contain final step usage",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 steps: initial, tool-result > value promises::result.finishReason should contain finish reason from final step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 steps: initial, tool-result > value promises::result.text should contain text from final step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 steps: initial, tool-result > value promises::result.steps should contain all steps",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_two_steps_sum_usage_and_keep_the_last_step()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = call => call == 1
                ? new LanguageModelStreamPart[]
                {
                    new ToolCallStreamPart("call-1", "lookup", "{}"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(10, 5, 15), "tool_calls"),
                }
                : new LanguageModelStreamPart[]
                {
                    new TextDeltaStreamPart("1", "done"),
                    new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
                },
        };
        GenerateTextResult? finished = null;
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.IsStepCount(2),
            OnFinish = (value, _) =>
            {
                finished = value;
                return Task.CompletedTask;
            },
            Tools = new[] { Tool.Function("lookup", "Looks up", "{}", (_, _) => Task.FromResult("\"result1\"")) },
        });
        Assert.Equal("done", await stream.Text);
        Assert.Equal(FinishReason.Stop, await stream.FinishReason);
        var usage = await stream.Usage;
        Assert.Equal(13, usage.InputTokens);
        Assert.Equal(15, usage.OutputTokens);
        Assert.Equal(28, usage.TotalTokens);
        var finalStep = await stream.FinalStep;
        Assert.Equal(3, finalStep.Usage.InputTokens);
        Assert.Equal(10, finalStep.Usage.OutputTokens);
        Assert.Equal(2, (await stream.Steps).Count);
        Assert.NotNull(finished);
        Assert.Equal(13, finished!.Usage.InputTokens);
        Assert.Equal(3, finished.FinalStep.Usage.InputTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 stop conditions::result.steps should contain a single step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen > 2 stop conditions::stopConditionCalls should be called for each stop condition",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_evaluates_every_stop_condition()
    {
        var seen = new List<int>();
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "tool1", "{}"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(10, 5, 15), "tool_calls"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.Any(
                StopWhen.Custom(steps =>
                {
                    seen.Add(0);
                    return false;
                }),
                StopWhen.Custom(_ =>
                {
                    seen.Add(1);
                    return true;
                })),
            Tools = new[] { Tool.Function("tool1", "Tool", "{}", (_, _) => Task.FromResult("\"result1\"")) },
        });
        Assert.Single(await stream.Steps);
        Assert.Equal(new[] { 0, 1 }, seen);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.stopWhen::should complete tool loop with isLoopFinished()",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_is_loop_finished_stops_when_the_model_stops_calling_tools()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = call => call == 1
                ? new LanguageModelStreamPart[]
                {
                    new ToolCallStreamPart("call-1", "tool1", "{}"),
                    new FinishStreamPart(FinishReason.ToolCalls, LanguageModelUsage.Empty, "tool_calls"),
                }
                : new LanguageModelStreamPart[]
                {
                    new TextDeltaStreamPart("1", "Done!"),
                    new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty, "stop"),
                },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.IsLoopFinished(),
            Tools = new[] { Tool.Function("tool1", "Tool", "{}", (_, _) => Task.FromResult("\"result1\"")) },
        });
        Assert.Equal("Done!", await stream.Text);
        Assert.Equal(2, (await stream.Steps).Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.reasoning::should pass reasoning to model doStream call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_forwards_reasoning_effort()
    {
        var model = new ScriptedLanguageModel();
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input", Reasoning = "high" });
        await stream.Text;
        Assert.Equal("high", model.Calls[0].Reasoning);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.reasoning::should pass through provider-default",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_forwards_provider_default_reasoning()
    {
        var model = new ScriptedLanguageModel();
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input", Reasoning = "provider-default" });
        await stream.Text;
        Assert.Equal("provider-default", model.Calls[0].Reasoning);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.abortSignal::should forward abort signal to tool execution during streaming",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_forwards_the_abort_token_to_tools()
    {
        using var source = new CancellationTokenSource();
        CancellationToken seen = default;
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "tool1", "{}"),
                new FinishStreamPart(FinishReason.ToolCalls, LanguageModelUsage.Empty, "tool_calls"),
            },
        };
        var stream = Client().StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.Function("tool1", "Tool", "{}", (_, token) =>
                    {
                        seen = token;
                        return Task.FromResult("\"ok\"");
                    }),
                },
            },
            source.Token);
        await stream.Text;
        Assert.Equal(source.Token, seen);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onError::should invoke onError",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_invokes_on_error_for_provider_errors()
    {
        Exception? seen = null;
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[] { new ErrorStreamPart("provider failed") },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            OnError = (exception, _) =>
            {
                seen = exception;
                return Task.CompletedTask;
            },
        });
        var exception = await Assert.ThrowsAsync<AiSdkException>(() => stream.Text);
        Assert.Equal("provider failed", exception.Message);
        Assert.Same(exception, seen);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > abort signal > basic abort::should not call onError for abort errors",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Abort_does_not_call_on_error()
    {
        var called = false;
        using var source = new CancellationTokenSource();
        source.Cancel();
        var stream = Client().StreamTextAsync(
            new StreamTextOptions
            {
                Model = new ScriptedLanguageModel(),
                Prompt = "test-input",
                OnError = (_, _) =>
                {
                    called = true;
                    return Task.CompletedTask;
                },
            },
            source.Token);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stream.Text);
        Assert.False(called);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution errors::should include tool error part in the full stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_includes_a_tool_error_part()
    {
        var model = new ScriptedLanguageModel
        {
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "tool1", "{\"value\":\"value\"}"),
                new FinishStreamPart(FinishReason.ToolCalls, LanguageModelUsage.Empty, "tool_calls"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function("tool1", "Tool", "{}", (_, _) => throw new InvalidOperationException("test error")),
            },
        });
        var parts = new List<TextStreamPart>();
        await foreach (var part in stream.Stream())
        {
            parts.Add(part);
        }

        var error = Assert.Single(parts.OfType<ToolResultPart>());
        Assert.True(error.Result.IsError);
        Assert.Contains("test error", error.Result.OutputJson);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
