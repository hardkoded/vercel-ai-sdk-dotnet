// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>
/// Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; result.stream &gt; tool choice enforcement</c>, the interrupted finish reason. Not ported:
/// <c>should stop an interrupted stream with a deferred provider tool result</c>, because the port has no provider tool that defers its result.
/// </summary>
public sealed class ToolChoiceEnforcementTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.stream > tool choice enforcement::";

    public static IEnumerable<object[]> NoneToolChoiceCases() =>
    [
        [ToolChoice.None, null!, FinishReason.Stop],
        [ToolChoice.None, null!, FinishReason.ToolCalls],
        [ToolChoice.None, null!, FinishReason.Interrupted],
        [ToolChoice.Auto, ToolChoice.None, FinishReason.ToolCalls],
    ];

    private static AiClient Client() => new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    [Theory]
    [MemberData(nameof(NoneToolChoiceCases))]
    [UpstreamTest(Prefix + "should reject tool calls with none ($toolChoice, $stepToolChoice, $finishReason)", Coverage = UpstreamCoverage.Partial, Note = "The port has no onInputStart, onInputDelta and onInputAvailable tool callbacks, so they are not asserted. The port raises the error from the stream and faults finishReason and usage, where upstream emits an error part and resolves finishReason to error and usage.")]
    public async Task Should_reject_tool_calls_with_none(ToolChoice toolChoice, ToolChoice? stepToolChoice, FinishReason finishReason)
    {
        var executed = false;
        var errors = new List<Exception>();
        var usage = new LanguageModelUsage(3, 10, 13);
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolInputStartStreamPart("call-1", "tool1"),
                new ToolInputDeltaStreamPart("call-1", "{}"),
                new ToolInputEndStreamPart("call-1"),
                new ToolCallStreamPart("call-1", "tool1", "{}"),
                new FinishStreamPart(finishReason, usage, null),
            },
        };

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function("tool1", null, "{\"type\":\"object\"}", (_, _) =>
                {
                    executed = true;
                    return Task.FromResult("tool-result");
                }),
            },
            ToolChoice = toolChoice,
            PrepareStep = stepToolChoice == null ? null : _ => new PrepareStepUpdate { ToolChoice = stepToolChoice },
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
        });

        await Assert.ThrowsAsync<ToolChoiceViolationException>(async () =>
        {
            await foreach (var _ in result.Stream())
            {
            }
        });


        var violation = Assert.IsType<ToolChoiceViolationException>(Assert.Single(errors));
        Assert.Equal("AI_ToolChoiceViolationError", violation.Name);
        Assert.Equal("Model response contained a tool call even though tool choice was none.", violation.Message);
        Assert.Equal("none", violation.ToolChoice.Type);
        Assert.Equal(finishReason, violation.FinishReason);
        Assert.IsType<GeneratedToolCall>(Assert.Single(violation.Content));
        await Assert.ThrowsAsync<ToolChoiceViolationException>(() => result.FinishReason);
        await Assert.ThrowsAsync<ToolChoiceViolationException>(() => result.Usage);
        Assert.False(executed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should withhold tool parts with none and not repair or refine tool calls", Coverage = UpstreamCoverage.Partial, Note = "The port raises the error from the stream instead of emitting an error part, so the error part of the full and UI streams is not asserted.")]
    public async Task Should_withhold_tool_parts_with_none_and_not_repair_or_refine_tool_calls()
    {
        var repaired = false;
        var refined = false;
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolInputStartStreamPart("call-1", "tool1"),
                new ToolInputDeltaStreamPart("call-1", "invalid"),
                new ToolInputEndStreamPart("call-1"),
                new ToolCallStreamPart("call-1", "tool1", "invalid json"),
                new ToolCallStreamPart("call-2", "tool2", "{\"value\":\"x\"}"),
                new ToolCallStreamPart("call-3", "web_search", "{}") { ProviderExecuted = true, Dynamic = true },
                new ToolResultStreamPart("call-3", "web_search", JsonDocument.Parse("{\"ok\":true}").RootElement.Clone()),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13), null),
            },
        };
        StreamTextResult Start() => Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                // client-side tools without execute:
                Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}", null),
                Tool.Function("tool2", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}", null),
            },
            ToolChoice = ToolChoice.None,
            RepairToolCall = _ =>
            {
                repaired = true;
                return Task.FromResult<GeneratedToolCall?>(null);
            },
            RefineToolInput = new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
            {
                ["tool2"] = (input, _) =>
                {
                    refined = true;
                    return Task.FromResult(input);
                },
            },
            OnError = (_, _) => Task.CompletedTask,
        });

        var fullStreamTypes = new List<string>();
        await Assert.ThrowsAsync<ToolChoiceViolationException>(async () =>
        {
            await foreach (var part in Start().Stream())
            {
                fullStreamTypes.Add(part.Type);
            }
        });

        using var body = new MemoryStream();
        await Assert.ThrowsAsync<ToolChoiceViolationException>(() => Vercel.AI.AspNetCore.UIMessageStreamResult.WriteAsync(Start(), body));
        var uiStreamTypes = Encoding.UTF8.GetString(body.ToArray())
            .Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(frame => frame.Substring("data: ".Length))
            .Where(data => data != "[DONE]")
            .Select(data => JsonNode.Parse(data)!["type"]!.GetValue<string>())
            .ToList();

        Assert.DoesNotContain(fullStreamTypes, type => type.StartsWith("tool-", StringComparison.Ordinal));
        Assert.DoesNotContain(uiStreamTypes, type => type.StartsWith("tool-", StringComparison.Ordinal));
        Assert.False(repaired);
        Assert.False(refined);
    }

    [Fact]
    [UpstreamTest(Prefix + "should allow prepareStep to replace none with auto", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_allow_prepareStep_to_replace_none_with_auto()
    {
        var executed = 0;
        var errors = new List<Exception>();
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "tool1", "{}"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13), null),
            },
        };

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function("tool1", null, "{\"type\":\"object\"}", (_, _) =>
                {
                    executed++;
                    return Task.FromResult("tool-result");
                }),
            },
            ToolChoice = ToolChoice.None,
            PrepareStep = _ => new PrepareStepUpdate { ToolChoice = ToolChoice.Auto },
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
        });

        await foreach (var _ in result.Stream())
        {
        }


        Assert.Equal(1, executed);
        Assert.Empty(errors);
    }

    public static IEnumerable<object[]> ToolChoices() =>
    [
        [ToolChoice.Required],
        [ToolChoice.Tool("tool1")],
    ];

    [Theory]
    [MemberData(nameof(ToolChoices))]
    [UpstreamTest(Prefix + "should preserve interrupted output without enforcing tool choice %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_interrupted_output_without_enforcing_tool_choice(ToolChoice toolChoice)
    {
        var errors = new List<Exception>();
        var onEnd = new List<GenerateTextResult>();
        var onStepEnd = new List<StepResult>();
        var onLanguageModelCallEnd = new List<LanguageModelCallEndEvent>();
        using var metadata = JsonDocument.Parse("{\"test\":{\"partial\":true}}");
        var usage = new LanguageModelUsage(3, 10, 13);
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Let me check"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Interrupted, usage, null, metadata.RootElement.Clone()),
            },
        };

        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{}}", null) },
            ToolChoice = toolChoice,
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
            OnFinish = (e, _) =>
            {
                onEnd.Add(e);
                return Task.CompletedTask;
            },
            OnStepEnd = (e, _) =>
            {
                onStepEnd.Add(e);
                return Task.CompletedTask;
            },
            OnLanguageModelCallEnd = (e, _) =>
            {
                onLanguageModelCallEnd.Add(e);
                return Task.CompletedTask;
            },
        });

        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.DoesNotContain(parts, part => part.Type == "error");
        var finishStep = Assert.IsType<StepFinishPart>(Assert.Single(parts, part => part.Type == "finish-step"));
        Assert.Equal(FinishReason.Interrupted, finishStep.Step.FinishReason);
        Assert.Null(finishStep.RawFinishReason);
        var finish = Assert.IsType<FinishPart>(Assert.Single(parts, part => part.Type == "finish"));
        Assert.Equal(FinishReason.Interrupted, finish.FinishReason);
        Assert.Equal("Let me check", await result.Text);
        Assert.Equal(FinishReason.Interrupted, await result.FinishReason);
        Assert.Equal(13, (await result.Usage).TotalTokens);
        Assert.True((await result.Steps)[0].ProviderMetadata!.Value.GetProperty("test").GetProperty("partial").GetBoolean());
        Assert.Empty(errors);
        Assert.Equal(FinishReason.Interrupted, Assert.Single(onEnd).FinishReason);
        Assert.Equal(FinishReason.Interrupted, Assert.Single(onStepEnd).FinishReason);
        Assert.Equal(FinishReason.Interrupted, Assert.Single(onLanguageModelCallEnd).FinishReason);

        var chunks = await Upstream.GenerateText.SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(
            new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{}}", null) },
                ToolChoice = toolChoice,
            }));
        Assert.Contains(chunks, chunk => chunk!["type"]!.GetValue<string>() == "finish" && chunk["finishReason"]!.GetValue<string>() == "interrupted");
    }
}
