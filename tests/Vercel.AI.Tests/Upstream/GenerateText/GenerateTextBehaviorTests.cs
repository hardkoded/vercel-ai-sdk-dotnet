// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class GenerateTextBehaviorTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText::should not execute tools when the finish reason is %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_execute_tools_for_disallowed_finish_reasons()
    {
        foreach (var reason in new[] { FinishReason.Length, FinishReason.Error, FinishReason.ContentFilter, FinishReason.Other })
        {
            var executed = false;
            var model = new TestLanguageModel
            {
                OnGenerate = _ => new LanguageModelGenerateResult(
                    new GeneratedContent[] { new GeneratedToolCall("call-1", "testTool", "{\"value\":\"test\"}") },
                    reason,
                    new LanguageModelUsage(3, 10, 13),
                    reason.ToString()),
            };
            var result = await Client().GenerateTextAsync(new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.Function("testTool", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}", (_, _) =>
                    {
                        executed = true;
                        return Task.FromResult("tool-result");
                    }),
                },
            });
            Assert.Single(result.ToolCalls);
            Assert.Empty(result.ToolResults);
            Assert.False(executed);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > result.reasoningText::should contain reasoning string from model response", Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_text_concatenates_reasoning_parts()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedReasoning("I will open the conversation with witty banter."),
                    new GeneratedReasoning(string.Empty),
                    new GeneratedText("Hello, world!"),
                },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "prompt" });
        Assert.Equal("I will open the conversation with witty banter.", result.ReasoningText);
        Assert.Equal("Hello, world!", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > result.warnings::should contain warnings from all steps", Coverage = UpstreamCoverage.Covered)]
    public async Task Warnings_come_from_every_step()
    {
        var warning0 = new CallWarning("other", "step 0 warning");
        var warning1 = new CallWarning("other", "step 1 warning");
        var result = await TwoStepAsync(
            new[] { warning0 },
            new[] { warning1 });
        Assert.Equal(new[] { "step 0 warning", "step 1 warning" }, result.Warnings.Select(warning => warning.Message).ToArray());
        Assert.Equal(new[] { "step 1 warning" }, result.FinalStep.Warnings.Select(warning => warning.Message).ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > result.warnings::should send warnings from all steps to onFinish", Coverage = UpstreamCoverage.Covered)]
    public async Task On_finish_receives_warnings_from_every_step()
    {
        GenerateTextResult? finished = null;
        var warning0 = new CallWarning("other", "step 0 warning");
        var warning1 = new CallWarning("other", "step 1 warning");
        await TwoStepAsync(new[] { warning0 }, new[] { warning1 }, (result, _) =>
        {
            finished = result;
            return Task.CompletedTask;
        });
        Assert.NotNull(finished);
        Assert.Equal(new[] { "step 0 warning", "step 1 warning" }, finished.Warnings.Select(warning => warning.Message).ToArray());
        Assert.Equal(new[] { "step 1 warning" }, finished.Steps[finished.Steps.Count - 1].Warnings.Select(warning => warning.Message).ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > result.sources::should contain sources from all steps", Coverage = UpstreamCoverage.Covered)]
    public async Task Sources_come_from_every_step()
    {
        var result = await TwoStepAsync(
            sources0: new[] { new GeneratedSource("source-0", "https://example.com/0", "Source 0") },
            sources1: new[] { new GeneratedSource("source-1", "https://example.com/1", "Source 1") });
        Assert.Equal(2, result.Sources.Count);
        Assert.Equal("source", result.Sources[0].Type);
        Assert.Equal("source-0", result.Sources[0].Id);
        Assert.Equal("https://example.com/0", result.Sources[0].Url);
        Assert.Equal("Source 0", result.Sources[0].Title);
        Assert.Equal("source-1", result.Sources[1].Id);
        Assert.Equal("https://example.com/1", result.Sources[1].Url);
        Assert.Equal("Source 1", result.Sources[1].Title);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > result.files::should contain files from all steps", Coverage = UpstreamCoverage.Covered)]
    public async Task Files_come_from_every_step()
    {
        var result = await TwoStepAsync(
            files0: new[] { File("c3RlcC0w") },
            files1: new[] { File("c3RlcC0x") });
        Assert.Equal(new[] { "c3RlcC0w", "c3RlcC0x" }, result.Files.Select(file => Convert.ToBase64String(file.Data)).ToArray());
        Assert.All(result.Files, file => Assert.Equal("text/plain", file.MediaType));
        Assert.Equal("c3RlcC0x", Convert.ToBase64String(result.FinalStep.Files[0].Data));
        Assert.Equal("text/plain", result.FinalStep.Files[0].MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > result.files::should send files from all steps to onFinish", Coverage = UpstreamCoverage.Covered)]
    public async Task On_finish_receives_files_from_every_step()
    {
        GenerateTextResult? finished = null;
        await TwoStepAsync(
            files0: new[] { File("c3RlcC0w") },
            files1: new[] { File("c3RlcC0x") },
            onFinish: (result, _) =>
            {
                finished = result;
                return Task.CompletedTask;
            });
        Assert.NotNull(finished);
        Assert.Equal(new[] { "c3RlcC0w", "c3RlcC0x" }, finished.Files.Select(file => Convert.ToBase64String(file.Data)).ToArray());
        var last = finished.Steps[finished.Steps.Count - 1];
        Assert.Equal("c3RlcC0x", Convert.ToBase64String(last.Files[0].Data));
        Assert.Equal("text/plain", last.Files[0].MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > result.steps::should expose the final step", Coverage = UpstreamCoverage.Covered)]
    public async Task Final_step_is_the_last_step()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("Hello!") },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "test-input" });
        Assert.Same(result.Steps[result.Steps.Count - 1], result.FinalStep);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > tool choice enforcement::should reject when a required tool choice produces no tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Required_tool_choice_without_a_call_throws()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedReasoning("I will not call the tool."),
                    new GeneratedText("No tool call."),
                },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
        };
        var error = await Assert.ThrowsAsync<ToolChoiceViolationException>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            ToolChoice = ToolChoice.Required,
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}", null) },
        }));
        Assert.Equal("AI_ToolChoiceViolationError", error.Name);
        Assert.Equal("Model response did not contain a tool call even though tool choice was required.", error.Message);
        Assert.Equal("required", error.ToolChoice.Type);
        Assert.Equal(FinishReason.Stop, error.FinishReason);
        Assert.Equal("test", error.Provider);
        Assert.Equal("test", error.ModelId);
        Assert.Equal("reasoning", error.Content[0].Type);
        Assert.Equal("I will not call the tool.", ((GeneratedReasoning)error.Content[0]).Text);
        Assert.Equal("text", error.Content[1].Type);
        Assert.Equal("No tool call.", ((GeneratedText)error.Content[1]).Text);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > tool choice enforcement::should expose a tool call serialized as text for opt-in recovery", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_choice_violation_exposes_serialized_text()
    {
        const string serialized = "{\"toolName\":\"tool1\",\"input\":{\"value\":\"value\"}}";
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText(serialized) },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
        };
        var error = await Assert.ThrowsAsync<ToolChoiceViolationException>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            ToolChoice = ToolChoice.Required,
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\"}", null) },
        }));
        var text = Assert.IsType<GeneratedText>(Assert.Single(error.Content));
        Assert.Equal(serialized, text.Text);
        using var document = JsonDocument.Parse(text.Text);
        Assert.Equal("tool1", document.RootElement.GetProperty("toolName").GetString());
        Assert.Equal("value", document.RootElement.GetProperty("input").GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > tool choice enforcement::should reject when a different tool is called instead of the required tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Named_tool_choice_rejects_a_different_tool()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "tool2", "{ \"value\": \"value\" }") },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
        };
        var error = await Assert.ThrowsAsync<ToolChoiceViolationException>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            ToolChoice = ToolChoice.Tool("tool1"),
            Tools = new[]
            {
                Tool.Function("tool1", null, "{\"type\":\"object\"}", null),
                Tool.Function("tool2", null, "{\"type\":\"object\"}", null),
            },
        }));
        Assert.Equal("AI_ToolChoiceViolationError", error.Name);
        Assert.Equal("Model response did not contain a call to the required tool 'tool1'.", error.Message);
        Assert.Equal("tool", error.ToolChoice.Type);
        var named = Assert.IsType<ToolChoice.NamedChoice>(error.ToolChoice);
        Assert.Equal("tool1", named.ToolName);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > tool choice enforcement::should enforce the tool choice returned by prepareStep", Coverage = UpstreamCoverage.Covered)]
    public async Task Prepare_step_can_require_a_tool_call()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => TestLanguageModel.Text("No tool call."),
        };
        var error = await Assert.ThrowsAsync<ToolChoiceViolationException>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            ToolChoice = ToolChoice.Auto,
            PrepareStep = _ => new PrepareStepUpdate { ToolChoice = ToolChoice.Required },
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\"}", null) },
        }));
        Assert.Equal("AI_ToolChoiceViolationError", error.Name);
        Assert.Equal("required", error.ToolChoice.Type);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > tool choice enforcement::should allow prepareStep to replace a required tool choice", Coverage = UpstreamCoverage.Covered)]
    public async Task Prepare_step_can_relax_a_required_tool_choice()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => TestLanguageModel.Text("No tool call."),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            ToolChoice = ToolChoice.Required,
            PrepareStep = _ => new PrepareStepUpdate { ToolChoice = ToolChoice.Auto },
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\"}", null) },
        });
        Assert.Equal("No tool call.", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.text should return text from last step", Coverage = UpstreamCoverage.Covered)]
    public async Task Last_step_text_is_the_result_text()
    {
        var result = await ToolLoopAsync();
        Assert.Equal("Hello, world!", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.toolCalls should contain tool calls from all steps", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_calls_include_every_step()
    {
        var result = await ToolLoopAsync();
        Assert.Equal(new[] { "call-1" }, result.ToolCalls.Select(call => call.ToolCallId).ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.finalStep.toolCalls should return empty tool calls from last step", Coverage = UpstreamCoverage.Covered)]
    public async Task Final_step_has_no_tool_calls()
    {
        var result = await ToolLoopAsync();
        Assert.Empty(result.FinalStep.ToolCalls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::result.finalStep.toolResults should return empty tool results from last step", Coverage = UpstreamCoverage.Covered)]
    public async Task Final_step_has_no_tool_results()
    {
        var result = await ToolLoopAsync();
        Assert.Empty(result.FinalStep.ToolResults);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen > 2 steps: initial, tool-result::onFinishResult.usage should sum token usage and finalStep should contain final step usage", Coverage = UpstreamCoverage.Covered)]
    public async Task On_finish_usage_is_the_sum_and_final_step_is_last()
    {
        GenerateTextResult? finished = null;
        var result = await ToolLoopAsync((result, _) =>
        {
            finished = result;
            return Task.CompletedTask;
        });
        Assert.NotNull(finished);
        Assert.Same(result, finished);
        Assert.Equal(13, result.Usage.InputTokens);
        Assert.Equal(15, result.Usage.OutputTokens);
        Assert.Equal(28, result.Usage.TotalTokens);
        Assert.Equal(result.Usage.InputTokens, finished.Usage.InputTokens);
        Assert.Equal(result.Usage.OutputTokens, finished.Usage.OutputTokens);
        Assert.Equal(result.Usage.TotalTokens, finished.Usage.TotalTokens);
        Assert.Same(finished.Steps[finished.Steps.Count - 1], finished.FinalStep);
        Assert.Equal(3, finished.FinalStep.Usage.InputTokens);
        Assert.Equal(10, finished.FinalStep.Usage.OutputTokens);
        Assert.Equal(13, finished.FinalStep.Usage.TotalTokens);
        Assert.Equal(result.FinalStep.Usage.InputTokens, finished.FinalStep.Usage.InputTokens);
        Assert.Equal(result.FinalStep.Usage.OutputTokens, finished.FinalStep.Usage.OutputTokens);
        Assert.Equal(result.FinalStep.Usage.TotalTokens, finished.FinalStep.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.stopWhen::should complete tool loop with isLoopFinished()", Coverage = UpstreamCoverage.Covered)]
    public async Task Loop_finished_condition_stops_when_the_model_stops_calling_tools()
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"value\" }") },
                FinishReason.ToolCalls,
                new LanguageModelUsage(3, 10, 13),
                null)
            : new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("Done!") },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop");
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.IsLoopFinished(),
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\"}", (_, _) => Task.FromResult("result1")) },
        });
        Assert.Equal("Done!", result.Text);
        Assert.Equal(2, result.Steps.Count);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.headers::should pass headers to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_headers_to_the_model()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = options =>
            {
                Assert.Equal("request-header-value", options.Headers!["custom-request-header"]);
                return TestLanguageModel.Text("Hello, world!");
            },
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Headers = new Dictionary<string, string?> { ["custom-request-header"] = "request-header-value" },
        });
        Assert.Equal("Hello, world!", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.providerOptions::should pass provider options to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_options_to_the_model()
    {
        using var document = JsonDocument.Parse("{\"someKey\":\"someValue\"}");
        var options = new Dictionary<string, JsonElement> { ["aProvider"] = document.RootElement.Clone() };
        var model = new TestLanguageModel
        {
            OnGenerate = call =>
            {
                Assert.Equal("someValue", call.ProviderOptions!["aProvider"].GetProperty("someKey").GetString());
                return TestLanguageModel.Text("provider metadata test");
            },
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            ProviderOptions = options,
        });
        Assert.Equal("provider metadata test", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.reasoning::should pass reasoning to model doGenerate call", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_reasoning_to_generate()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("test") };
        await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "test-input", Reasoning = "high" });
        Assert.Equal("high", model.Calls[0].Reasoning);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.reasoning::should pass through provider-default", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_default_reasoning()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("test") };
        await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "test-input", Reasoning = "provider-default" });
        Assert.Equal("provider-default", model.Calls[0].Reasoning);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > options.reasoning::should pass reasoning to model doStream call", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_reasoning_to_stream()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("1", "test"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input", Reasoning = "high" });
        Assert.Equal("test", await stream.Text);
        Assert.Equal("high", model.Calls[0].Reasoning);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > options.reasoning::should pass through provider-default", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_default_reasoning_to_stream()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("1", "test"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input", Reasoning = "provider-default" });
        Assert.Equal("test", await stream.Text);
        Assert.Equal("provider-default", model.Calls[0].Reasoning);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.stream::should send tool call deltas",
        Coverage = UpstreamCoverage.Partial,
        Note = "The .NET full stream has no start or start-step parts, and it reports a tool without execute as a tool error result.")]
    public async Task Streams_tool_input_deltas()
    {
        const string id = "call_O17Uplv4lJvD6DVdIvFFeRMw";
        var deltas = new[] { "{\"", "value", "\":\"", "Spark", "le", " Day", "\"}" };
        var streamParts = new List<LanguageModelStreamPart> { new ToolInputStartStreamPart(id, "test-tool") };
        streamParts.AddRange(deltas.Select(delta => new ToolInputDeltaStreamPart(id, delta)));
        streamParts.Add(new ToolInputEndStreamPart(id));
        streamParts.Add(new ToolCallStreamPart(id, "test-tool", "{\"value\":\"Sparkle Day\"}"));
        streamParts.Add(new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13)));

        var parts = await StreamToolInputAsync(streamParts);

        Assert.Equal(
            new[] { "tool-input-start" }
                .Concat(deltas.Select(_ => "tool-input-delta"))
                .Concat(new[] { "tool-input-end", "tool-call" }),
            parts.Take(10).Select(part => part.Type));
        var start = Assert.IsType<ToolInputStartPart>(parts[0]);
        Assert.Equal(id, start.Id);
        Assert.Equal("test-tool", start.ToolName);
        Assert.False(start.Dynamic);
        Assert.Null(start.ProviderExecuted);
        Assert.Null(start.ProviderMetadata);
        Assert.Equal(deltas, parts.OfType<ToolInputDeltaPart>().Select(delta => delta.Delta));
        Assert.All(parts.OfType<ToolInputDeltaPart>(), delta => Assert.Equal(id, delta.Id));
        Assert.Equal(id, Assert.IsType<ToolInputEndPart>(parts[8]).Id);
        var call = Assert.IsType<ToolCallPart>(parts[9]).ToolCall;
        Assert.Equal(id, call.ToolCallId);
        Assert.Equal("{\"value\":\"Sparkle Day\"}", call.ArgumentsJson);
        var finish = Assert.IsType<FinishPart>(parts[^1]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal(13, finish.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.stream::should pass through providerMetadata on tool-input-start",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_metadata_on_tool_input_start()
    {
        using var metadata = JsonDocument.Parse("{\"testProvider\":{\"someKey\":\"someValue\"}}");
        var parts = await StreamToolInputAsync(new LanguageModelStreamPart[]
        {
            new ToolInputStartStreamPart("call-1", "test-tool", metadata.RootElement.Clone()),
            new ToolInputDeltaStreamPart("call-1", "{\"value\":\"test\"}"),
            new ToolInputEndStreamPart("call-1"),
            new ToolCallStreamPart("call-1", "test-tool", "{\"value\":\"test\"}"),
            new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13)),
        });

        var start = Assert.Single(parts.OfType<ToolInputStartPart>());
        Assert.Equal("{\"testProvider\":{\"someKey\":\"someValue\"}}", start.ProviderMetadata!.Value.GetRawText());
    }

    private static async Task<List<TextStreamPart>> StreamToolInputAsync(IReadOnlyList<LanguageModelStreamPart> streamParts)
    {
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel { StreamParts = streamParts },
            Prompt = "test-input",
            Tools = new[] { Tool.Function("test-tool", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}", null) },
            ToolChoice = ToolChoice.Required,
        });
        var parts = new List<TextStreamPart>();
        await foreach (var part in stream.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }

    private static Task<GenerateTextResult> TwoStepAsync(
        IReadOnlyList<CallWarning>? warnings0 = null,
        IReadOnlyList<CallWarning>? warnings1 = null,
        Func<GenerateTextResult, CancellationToken, Task>? onFinish = null,
        IReadOnlyList<GeneratedSource>? sources0 = null,
        IReadOnlyList<GeneratedSource>? sources1 = null,
        IReadOnlyList<GeneratedFile>? files0 = null,
        IReadOnlyList<GeneratedFile>? files1 = null)
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ =>
        {
            if (model.Calls.Count == 1)
            {
                var content = new List<GeneratedContent>();
                if (sources0 != null)
                {
                    content.AddRange(sources0);
                }

                if (files0 != null)
                {
                    content.AddRange(files0);
                }

                content.Add(new GeneratedToolCall("call-1", "tool1", "{}"));
                return new LanguageModelGenerateResult(content, FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13), null, warnings0);
            }

            var second = new List<GeneratedContent>();
            if (sources1 != null)
            {
                second.AddRange(sources1);
            }

            if (files1 != null)
            {
                second.AddRange(files1);
            }

            second.Add(new GeneratedText("Done."));
            return new LanguageModelGenerateResult(second, FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop", warnings1);
        };
        return Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            StopWhen = StopWhen.IsStepCount(3),
            OnFinish = onFinish,
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\"}", (_, _) => Task.FromResult("result1")) },
        });
    }

    private static Task<GenerateTextResult> ToolLoopAsync(Func<GenerateTextResult, CancellationToken, Task>? onFinish = null)
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"value\" }") },
                FinishReason.ToolCalls,
                new LanguageModelUsage(10, 5, 15),
                null)
            : new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("Hello, world!") },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop");
        return Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.IsStepCount(3),
            OnFinish = onFinish,
            Tools = new[]
            {
                Tool.Function("tool1", "Tool One", "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}", (_, _) => Task.FromResult("result1")),
            },
        });
    }

    private static GeneratedFile File(string base64)
    {
        return new GeneratedFile(Convert.FromBase64String(base64), "text/plain");
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
