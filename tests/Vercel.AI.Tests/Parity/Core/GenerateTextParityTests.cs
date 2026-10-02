// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

public sealed class GenerateTextParityTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.reasoningText::should contain reasoning string from model response",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Result_reasoning_text_comes_from_the_model()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedReasoning("I will open the conversation with witty banter."),
                    new GeneratedText("Hello"),
                },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "prompt" });
        Assert.Equal("I will open the conversation with witty banter.", result.ReasoningText);
        Assert.Equal("Hello", result.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.sources::should contain sources",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Result_contains_sources()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedText("cited"),
                    new GeneratedSource("source-1", "https://example.com/a", "Source A"),
                },
                FinishReason.Stop,
                LanguageModelUsage.Empty,
                "stop"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "prompt" });
        var source = Assert.Single(result.Sources);
        Assert.Equal("source-1", source.Id);
        Assert.Equal("https://example.com/a", source.Url);
        Assert.Equal("Source A", source.Title);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.sources::should contain sources from all steps",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Result_contains_sources_from_every_step()
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedSource("source-0", "https://example.com/0", "Source 0"),
                    new GeneratedToolCall("c1", "lookup", "{}"),
                },
                FinishReason.ToolCalls,
                new LanguageModelUsage(10, 5, 15),
                "tool_calls")
            : new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedText("done"),
                    new GeneratedSource("source-1", "https://example.com/1", "Source 1"),
                },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop");
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            StopWhen = StopWhen.IsStepCount(2),
            Tools = new[] { Tool.Function("lookup", "Looks up", "{}", (_, _) => Task.FromResult("\"ok\"")) },
        });
        Assert.Equal(new[] { "source-0", "source-1" }, result.Sources.Select(source => source.Id).ToArray());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.toolCalls::should contain tool calls",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Result_contains_tool_calls()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "lookup", "{\"q\":\"x\"}") },
                FinishReason.ToolCalls,
                LanguageModelUsage.Empty,
                "tool_calls"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "find",
            Tools = new[] { Tool.Function("lookup", "Looks up", "{}", (_, _) => Task.FromResult("\"ok\"")) },
        });
        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("lookup", call.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.toolResults::should contain tool results",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Result_contains_tool_results()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "lookup", "{\"value\":\"value\"}") },
                FinishReason.ToolCalls,
                LanguageModelUsage.Empty,
                "tool_calls"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "find",
            Tools = new[] { Tool.Function("lookup", "Looks up", "{}", (_, _) => Task.FromResult("\"result1\"")) },
        });
        var toolResult = Assert.Single(result.ToolResults);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("\"result1\"", toolResult.OutputJson);
        Assert.False(toolResult.IsError);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.text should return text from last step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.toolCalls should contain tool calls from all steps",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.finalStep.toolCalls should return empty tool calls from last step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.finalStep.toolResults should return empty tool results from last step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.totalUsage should sum token usage",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.usage should sum token usage",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.finalStep.usage should contain token usage from final step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::onFinishResult.usage should sum token usage and finalStep should contain final step usage",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.steps should contain all steps",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result > callbacks::onStepFinish should be called for each step",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Two_steps_sum_usage_and_keep_the_last_step()
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "lookup", "{\"value\":\"value\"}") },
                FinishReason.ToolCalls,
                new LanguageModelUsage(10, 5, 15),
                "tool_calls")
            : new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("done") },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop");
        GenerateTextResult? finished = null;
        var stepNumbers = new List<int>();
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.IsStepCount(2),
            OnStepFinish = (step, _) =>
            {
                stepNumbers.Add(step.StepNumber);
                return Task.CompletedTask;
            },
            OnFinish = (value, _) =>
            {
                finished = value;
                return Task.CompletedTask;
            },
            Tools = new[] { Tool.Function("lookup", "Looks up", "{}", (_, _) => Task.FromResult("\"result1\"")) },
        });

        Assert.Equal("done", result.Text);
        Assert.Equal("call-1", Assert.Single(result.ToolCalls).ToolCallId);
        Assert.Empty(result.FinalStep.ToolCalls);
        Assert.Empty(result.FinalStep.ToolResults);
        Assert.Equal(13, result.Usage.InputTokens);
        Assert.Equal(15, result.Usage.OutputTokens);
        Assert.Equal(28, result.Usage.TotalTokens);
        Assert.Equal(3, result.FinalStep.Usage.InputTokens);
        Assert.Equal(10, result.FinalStep.Usage.OutputTokens);
        Assert.NotNull(finished);
        Assert.Equal(13, finished!.Usage.InputTokens);
        Assert.Equal(15, finished.Usage.OutputTokens);
        Assert.Equal(3, finished.FinalStep.Usage.InputTokens);
        Assert.Equal(2, result.Steps.Count);
        Assert.Equal(new[] { 0, 1 }, stepNumbers);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 stop conditions::result.steps should contain a single step",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 stop conditions::stopConditionCalls should be called for each stop condition",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Every_stop_condition_runs_and_any_true_stops_the_loop()
    {
        var seen = new List<int>();
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{\"value\":\"value\"}") },
                FinishReason.ToolCalls,
                new LanguageModelUsage(10, 5, 15),
                "tool_calls"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.Any(
                StopWhen.Custom(steps =>
                {
                    seen.Add(0);
                    Assert.Single(steps);
                    return false;
                }),
                StopWhen.Custom(steps =>
                {
                    seen.Add(1);
                    Assert.Single(steps);
                    return true;
                })),
            Tools = new[] { Tool.Function("tool1", "Tool", "{}", (_, _) => Task.FromResult("\"result1\"")) },
        });
        Assert.Single(result.Steps);
        Assert.Equal(new[] { 0, 1 }, seen);
        Assert.Single(model.Calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen::should complete tool loop with isLoopFinished()",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Is_loop_finished_lets_the_model_stop_the_loop()
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{\"value\":\"value\"}") },
                FinishReason.ToolCalls,
                LanguageModelUsage.Empty,
                "tool_calls")
            : TestLanguageModel.Text("Done!");
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.IsLoopFinished(),
            Tools = new[] { Tool.Function("tool1", "Tool", "{}", (_, _) => Task.FromResult("\"result1\"")) },
        });
        Assert.Equal("Done!", result.Text);
        Assert.Equal(2, result.Steps.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.headers::should pass headers to model",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Headers_are_forwarded_to_the_model()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("Hello, world!") };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Headers = new Dictionary<string, string?> { ["custom-request-header"] = "request-header-value" },
        });
        Assert.Equal("Hello, world!", result.Text);
        Assert.Equal("request-header-value", model.Calls[0].Headers!["custom-request-header"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.reasoning::should pass reasoning to model doGenerate call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_effort_is_forwarded()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("test") };
        await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "test-input", Reasoning = "high" });
        Assert.Equal("high", model.Calls[0].Reasoning);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.reasoning::should pass through provider-default",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_default_reasoning_is_forwarded()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("test") };
        await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "test-input", Reasoning = "provider-default" });
        Assert.Equal("provider-default", model.Calls[0].Reasoning);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.abortSignal::should forward abort signal to tool execution",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Abort_token_reaches_tool_execution()
    {
        using var source = new CancellationTokenSource();
        CancellationToken seen = default;
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{\"value\":\"value\"}") },
                FinishReason.ToolCalls,
                LanguageModelUsage.Empty,
                "tool_calls"),
        };
        await Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.Function("tool1", "Tool", "{}", (_, token) =>
                    {
                        seen = token;
                        return Task.FromResult("\"tool result\"");
                    }),
                },
            },
            source.Token);
        Assert.Equal(source.Token, seen);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.output > object output::should parse the output",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Object_output_parses_json()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("{ \"value\": \"test-value\" }") };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            Output = OutputSpec.Object("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}", "payload"),
        });
        Assert.Equal("test-value", result.Output!.Value.GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.output > object output::should expose parse diagnostics when output is truncated",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Truncated_object_output_reports_parse_diagnostics()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("{\"value\":\"test") },
                FinishReason.Length,
                new LanguageModelUsage(3, 10, 13),
                "length"),
        };
        var exception = await Assert.ThrowsAsync<NoObjectGeneratedException>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            Output = OutputSpec.Object("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}"),
        }));
        Assert.Equal("No object generated: could not parse the response.", exception.Message);
        Assert.Equal("{\"value\":\"test", exception.Text);
        Assert.Equal(FinishReason.Length, exception.FinishReason);
        Assert.Equal(10, exception.Usage.OutputTokens);
        Assert.IsAssignableFrom<JsonException>(exception.InnerException);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.output > object output::should set responseFormat to json and send schema as part of the responseFormat",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Object_output_sends_the_schema_to_the_model()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("{ \"value\": \"test-value\" }") };
        const string schema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}";
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            Output = OutputSpec.Object(schema, "payload"),
        });
        Assert.Equal("payload", model.Calls[0].JsonSchemaName);
        Assert.Contains("value", model.Calls[0].JsonSchema!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.output > text output::should set responseFormat to text and not change the prompt",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Text_output_leaves_the_prompt_unchanged()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("Hello") };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            Output = OutputSpec.Text(),
        });
        Assert.Null(model.Calls[0].JsonSchema);
        var user = Assert.IsType<UserModelMessage>(Assert.Single(model.Calls[0].Prompt));
        Assert.Equal("prompt", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
        Assert.Equal("Hello", result.Text);
        Assert.Null(result.Output);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution errors::should add tool error part to the content",
        Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution errors::should include error result in response messages",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_failures_are_recorded_on_the_step_and_in_response_messages()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{\"value\":\"value\"}") },
                FinishReason.ToolCalls,
                LanguageModelUsage.Empty,
                "tool_calls"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function("tool1", "Tool", "{}", (_, _) => throw new InvalidOperationException("test error")),
            },
        });
        var toolResult = Assert.Single(result.ToolResults);
        Assert.True(toolResult.IsError);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("tool1", toolResult.ToolName);
        Assert.Contains("test error", toolResult.OutputJson);
        var message = Assert.IsType<ToolModelMessage>(result.ResponseMessages[1]);
        Assert.True(message.IsError);
        Assert.Contains("test error", message.OutputJson);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/sum-token-counts.test.ts::sumTokenCounts::should sum known token counts",
        Coverage = UpstreamCoverage.Covered)]
    public void Sum_token_counts_adds_known_values()
    {
        Assert.Equal(13, TokenCounts.Sum(3, 10));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/sum-token-counts.test.ts::sumTokenCounts::should treat one unknown token count as 0",
        Coverage = UpstreamCoverage.Covered)]
    public void Sum_token_counts_treats_one_unknown_as_zero()
    {
        Assert.Equal(10, TokenCounts.Sum(null, 10));
        Assert.Equal(3, TokenCounts.Sum(3, null));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/sum-token-counts.test.ts::sumTokenCounts::should return undefined when both token counts are unknown",
        Coverage = UpstreamCoverage.Covered)]
    public void Sum_token_counts_stays_unknown_when_both_are_unknown()
    {
        Assert.Null(TokenCounts.Sum(null, null));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/filter-active-tools.test.ts::filterActiveTools::should return undefined when tools are not provided",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Missing_tools_stay_unset_when_active_tools_are_set()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("ok") };
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            ActiveTools = new[] { "tool1" },
        });
        Assert.Null(model.Calls[0].Tools);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/filter-active-tools.test.ts::filterActiveTools::should return all tools when activeTools is not provided",
        Coverage = UpstreamCoverage.Covered)]
    public async Task All_tools_are_sent_when_active_tools_are_unset()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("ok") };
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            Tools = new[]
            {
                Tool.Function("tool1", "Tool 1", "{}", null),
                Tool.Function("tool2", "Tool 2", "{}", null),
                Tool.Function("providerTool", "Provider", "{}", null),
            },
        });
        Assert.Equal(new[] { "tool1", "tool2", "providerTool" }, model.Calls[0].Tools!.Select(tool => tool.Name).ToArray());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/filter-active-tools.test.ts::filterActiveTools::should return no tools when activeTools is empty",
        Coverage = UpstreamCoverage.Covered)]
    public async Task An_empty_active_tool_list_sends_no_tools()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("ok") };
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            ActiveTools = Array.Empty<string>(),
            Tools = new[]
            {
                Tool.Function("tool1", "Tool 1", "{}", null),
                Tool.Function("providerTool", "Provider", "{}", null),
            },
        });
        Assert.Empty(model.Calls[0].Tools!);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/filter-active-tools.test.ts::filterActiveTools::should filter tools based on activeTools",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Active_tools_filter_the_tools_sent_to_the_model()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("ok") };
        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            ActiveTools = new[] { "tool1", "providerTool" },
            Tools = new[]
            {
                Tool.Function("tool1", "Tool 1", "{}", null),
                Tool.Function("tool2", "Tool 2", "{}", null),
                Tool.Function("providerTool", "Provider", "{}", null),
            },
        });
        Assert.Equal(new[] { "tool1", "providerTool" }, model.Calls[0].Tools!.Select(tool => tool.Name).ToArray());
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
