// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText</c> &gt; <c>provider-executed tools</c> &gt; <c>provider-executed tool result replay across steps</c>.</summary>
public sealed class ProviderExecutedToolResultReplayAcrossStepsTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > provider-executed tool result replay across steps::should preserve provider metadata when replaying the next step",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_provider_metadata_when_replaying_the_next_step()
    {
        var weatherTool = JsonDocument.Parse("""
            {"type":"function","name":"get_weather","description":"Get the current weather at a specific location","parameters":{"type":"object","properties":{"location":{"type":"string"}},"required":["location"]}}
            """).RootElement.Clone();
        var searchResult = JsonDocument.Parse("{\"tools\":[" + weatherTool.GetRawText() + "]}").RootElement.Clone();
        var model = new ReplayModel(
            new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("tool-search-call-1", "toolSearch", "{\"arguments\":{\"paths\":[\"get_weather\"]},\"call_id\":null}", JsonDocument.Parse("{\"openai\":{\"itemId\":\"tsc_123\"}}").RootElement.Clone())
                {
                    ProviderExecuted = true,
                },
                new ToolResultStreamPart("tool-search-call-1", "toolSearch", searchResult)
                {
                    ProviderMetadata = JsonDocument.Parse("{\"openai\":{\"itemId\":\"tso_123\"}}").RootElement.Clone(),
                },
                new ToolCallStreamPart("call-2", "get_weather", "{\"location\":\"San Francisco, CA\"}"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13), null),
            },
            new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("1", "Sunny."),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            });
        var args = JsonDocument.Parse("{}").RootElement;

        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.ProviderDefined("openai.tool_search", "toolSearch", args),
                Tool.Function(
                    "get_weather",
                    null,
                    "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}},\"required\":[\"location\"]}",
                    (_, _) => Task.FromResult("sunny")),
            },
            StopWhen = StopWhen.IsStepCount(3),
        });
        await result.Text;

        Assert.Equal(2, model.Prompts.Count);
        var prompt = model.Prompts[1];
        Assert.Equal(new[] { "user", "assistant", "tool" }, prompt.Select(message => message.Role).ToArray());
        var user = Assert.IsType<UserModelMessage>(prompt[0]);
        Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);

        var assistant = Assert.IsType<AssistantModelMessage>(prompt[1]);
        Assert.Equal(new[] { "tool-call", "tool-result", "tool-call" }, assistant.Parts.Select(part => part.Type).ToArray());
        var searchCall = Assert.IsType<AssistantToolCallPart>(assistant.Parts[0]);
        Assert.Equal("tool-search-call-1", searchCall.ToolCall.ToolCallId);
        Assert.Equal("toolSearch", searchCall.ToolCall.ToolName);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"arguments\":{\"paths\":[\"get_weather\"]},\"call_id\":null}"),
            JsonNode.Parse(searchCall.ToolCall.ArgumentsJson)));
        Assert.True(searchCall.ToolCall.ProviderExecuted);
        Assert.Equal("tsc_123", searchCall.ProviderOptions!["openai"].GetProperty("itemId").GetString());

        var searchOutput = Assert.IsType<AssistantToolResultPart>(assistant.Parts[1]);
        Assert.Equal("tool-search-call-1", searchOutput.ToolCallId);
        Assert.Equal("toolSearch", searchOutput.ToolName);
        Assert.Equal("json", searchOutput.Output.OutputType);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(searchResult.GetRawText()), JsonNode.Parse(searchOutput.Output.OutputJson)));
        Assert.Equal("tso_123", searchOutput.ProviderOptions!["openai"].GetProperty("itemId").GetString());

        var weatherCall = Assert.IsType<AssistantToolCallPart>(assistant.Parts[2]);
        Assert.Equal("call-2", weatherCall.ToolCall.ToolCallId);
        Assert.Equal("get_weather", weatherCall.ToolCall.ToolName);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"location\":\"San Francisco, CA\"}"), JsonNode.Parse(weatherCall.ToolCall.ArgumentsJson)));
        Assert.False(weatherCall.ToolCall.ProviderExecuted);
        Assert.Null(weatherCall.ProviderOptions);

        var tool = Assert.IsType<ToolModelMessage>(prompt[2]);
        Assert.Equal("call-2", tool.ToolCallId);
        Assert.Equal("get_weather", tool.ToolName);
        Assert.Equal("text", tool.OutputType);
        Assert.Equal("sunny", tool.OutputJson);
    }

    private sealed class ReplayModel : ILanguageModel
    {
        private readonly LanguageModelStreamPart[][] _responses;

        public ReplayModel(params LanguageModelStreamPart[][] responses)
        {
            _responses = responses;
        }

        public List<IReadOnlyList<ModelMessage>> Prompts { get; } = new();

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "replay";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            Prompts.Add(options.Prompt.ToArray());
            if (Prompts.Count > _responses.Length)
            {
                throw new InvalidOperationException("Unexpected response count: " + Prompts.Count);
            }

            return Task.FromResult(new LanguageModelStreamResult(Parts(_responses[Prompts.Count - 1])));
        }

        private static async IAsyncEnumerable<LanguageModelStreamPart> Parts(LanguageModelStreamPart[] parts)
        {
            foreach (var part in parts)
            {
                yield return part;
                await Task.Yield();
            }
        }
    }
}
