// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; result.responseMessages</c>.</summary>
public sealed class ResultResponseMessagesTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.responseMessages::";

    private const string ValueSchema = "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"type\":\"object\"}";

    private static readonly LanguageModelUsage TestUsage = new(3, 10, 13);

    private static JsonElement Metadata(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain assistant response message when there are no tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_assistant_response_message_when_there_are_no_tool_calls()
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ => new LanguageModelGenerateResult(new GeneratedContent[] { new GeneratedText("Hello, world!") }, FinishReason.Stop, TestUsage, "stop"),
            },
            Prompt = "test-input",
        });

        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(result.ResponseMessages));
        var text = Assert.IsType<AssistantTextPart>(Assert.Single(message.Parts));
        Assert.Equal("Hello, world!", text.Text);
        Assert.Null(text.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain assistant response message and tool message when there are tool calls with results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_assistant_response_message_and_tool_message_when_there_are_tool_calls_with_results()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedText("Hello, world!"),
                    new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"value\" }"),
                },
                FinishReason.Stop,
                TestUsage,
                "stop"),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[]
            {
                Tool.Function("tool1", null, ValueSchema, (input, _) =>
                {
                    Assert.Equal("{\"value\":\"value\"}", Compact(input));
                    var user = Assert.IsType<UserModelMessage>(Assert.Single(model.Calls[0].Prompt));
                    Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
                    return Task.FromResult("result1");
                }),
            },
            Prompt = "test-input",
        });

        Assert.Equal(2, result.ResponseMessages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(result.ResponseMessages[0]);
        Assert.Equal(2, assistant.Parts.Count);
        var text = Assert.IsType<AssistantTextPart>(assistant.Parts[0]);
        Assert.Equal("Hello, world!", text.Text);
        Assert.Null(text.ProviderOptions);
        var call = Assert.IsType<AssistantToolCallPart>(assistant.Parts[1]);
        Assert.Equal("{\"value\":\"value\"}", Compact(call.ToolCall.ArgumentsJson));
        Assert.False(call.ToolCall.ProviderExecuted);
        Assert.Null(call.ProviderOptions);
        Assert.Equal("call-1", call.ToolCall.ToolCallId);
        Assert.Equal("tool1", call.ToolCall.ToolName);
        var tool = Assert.IsType<ToolModelMessage>(result.ResponseMessages[1]);
        Assert.Equal("text", tool.OutputType);
        Assert.Equal("result1", tool.OutputJson);
        Assert.Equal("call-1", tool.ToolCallId);
        Assert.Equal("tool1", tool.ToolName);
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_reasoning()
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ => new LanguageModelGenerateResult(
                    new GeneratedContent[]
                    {
                        new GeneratedReasoning("I will open the conversation with witty banter.", Metadata("""{"testProvider":{"signature":"signature"}}""")),
                        new GeneratedReasoning(string.Empty, Metadata("""{"testProvider":{"redactedData":"redacted-reasoning-data"}}""")),
                        new GeneratedText("Hello, world!"),
                    },
                    FinishReason.Stop,
                    TestUsage,
                    "stop"),
            },
            Prompt = "test-input",
        });

        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(result.ResponseMessages));
        Assert.Equal(3, message.Parts.Count);
        var first = Assert.IsType<AssistantReasoningPart>(message.Parts[0]);
        Assert.Equal("I will open the conversation with witty banter.", first.Text);
        Assert.Equal("signature", first.ProviderOptions!["testProvider"].GetProperty("signature").GetString());
        var second = Assert.IsType<AssistantReasoningPart>(message.Parts[1]);
        Assert.Equal(string.Empty, second.Text);
        Assert.Equal("redacted-reasoning-data", second.ProviderOptions!["testProvider"].GetProperty("redactedData").GetString());
        var text = Assert.IsType<AssistantTextPart>(message.Parts[2]);
        Assert.Equal("Hello, world!", text.Text);
        Assert.Null(text.ProviderOptions);
    }
}
