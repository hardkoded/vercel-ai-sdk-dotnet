// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>options.onLanguageModelCallStart and onLanguageModelCallEnd</c>.</summary>
public sealed class OptionsOnLanguageModelCallStartAndOnLanguageModelCallEndTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onLanguageModelCallStart and onLanguageModelCallEnd::should expose provider metadata on model-call end", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_provider_metadata_on_model_call_end()
    {
        var events = new List<LanguageModelCallEndEvent>();
        using var metadata = JsonDocument.Parse("{\"gateway\":{\"generationId\":\"generation-id\"}}");
        var model = new StreamTextAbortSupport.ScriptedStreamModel(() => Finish(metadata.RootElement.Clone()));

        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            OnLanguageModelCallEnd = (e, _) =>
            {
                events.Add(e);
                return Task.CompletedTask;
            },
        }));

        var end = Assert.Single(events);
        Assert.Equal("generation-id", end.ProviderMetadata!.Value.GetProperty("gateway").GetProperty("generationId").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onLanguageModelCallStart and onLanguageModelCallEnd::should fire the model-call callbacks before tool execution and step finish",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no onStepStart, onLanguageModelCallStart, callId, or performance on the event. See #623.")]
    public async Task Should_fire_the_model_call_callbacks_before_tool_execution_and_step_finish()
    {
        var callOrder = new List<string>();
        var events = new List<LanguageModelCallEndEvent>();
        var model = new StreamTextAbortSupport.ScriptedStreamModel(Step);

        await ConsumeAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            OnLanguageModelCallEnd = (e, _) =>
            {
                callOrder.Add("onLanguageModelCallEnd");
                events.Add(e);
                return Task.CompletedTask;
            },
            OnToolExecutionStart = (_, _) =>
            {
                callOrder.Add("onToolExecutionStart");
                return Task.CompletedTask;
            },
            OnToolExecutionEnd = (_, _) =>
            {
                callOrder.Add("onToolExecutionEnd");
                return Task.CompletedTask;
            },
            OnStepEnd = (_, _) =>
            {
                callOrder.Add("onStepFinish");
                return Task.CompletedTask;
            },
        }));

        Assert.Equal(new[] { "onLanguageModelCallEnd", "onToolExecutionStart", "onToolExecutionEnd", "onStepFinish" }, callOrder);
        var end = Assert.Single(events);
        Assert.Equal("response-model", end.ModelId);
        Assert.Equal("response-1", end.ResponseId);
        Assert.Equal("test", end.Provider);
        Assert.Equal(FinishReason.ToolCalls, end.FinishReason);
        Assert.Equal(13, end.Usage.TotalTokens);
        Assert.Equal(2, end.Content.Count);
        Assert.Equal("Before tool.", Assert.IsType<GeneratedText>(end.Content[0]).Text);
        var toolCall = Assert.IsType<GeneratedToolCall>(end.Content[1]);
        Assert.Equal("call-1", toolCall.ToolCallId);
        Assert.Equal("tool1", toolCall.ToolName);
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Finish(JsonElement metadata)
    {
        yield return new StreamStartStreamPart(null);
        yield return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), providerMetadata: metadata);
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Step()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ResponseMetadataStreamPart("response-1", "response-model", DateTimeOffset.Parse("2025-01-01T00:00:00.000Z", System.Globalization.CultureInfo.InvariantCulture));
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "Before tool.");
        yield return new TextEndStreamPart("1");
        yield return new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"test-arg\" }");
        yield return new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13));
        await Task.CompletedTask;
    }
}
