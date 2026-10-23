// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
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
