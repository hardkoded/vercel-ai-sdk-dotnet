// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; result.responseMessages</c>.</summary>
public sealed class ResultResponseMessagesTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.responseMessages::";

    private const string ValueSchema = "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"type\":\"object\"}";

    private static readonly LanguageModelUsage TestUsage = new(3, 10, 13);

    private static JsonElement Metadata(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_reasoning()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel
            {
                StreamParts = new LanguageModelStreamPart[]
                {
                    new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                    new ReasoningStartStreamPart("1"),
                    new ReasoningDeltaStreamPart("1", "I will open the conversation"),
                    new ReasoningDeltaStreamPart("1", " with witty banter."),
                    new ReasoningDeltaStreamPart("1", string.Empty, Metadata("""{"testProvider":{"signature":"1234567890"}}""")),
                    new ReasoningEndStreamPart("1"),
                    new ReasoningStartStreamPart("2", Metadata("""{"testProvider":{"redactedData":"redacted-reasoning-data"}}""")),
                    new ReasoningEndStreamPart("2"),
                    new ReasoningStartStreamPart("3"),
                    new ReasoningDeltaStreamPart("3", " Once the user has relaxed,"),
                    new ReasoningDeltaStreamPart("3", " I will pry for valuable information."),
                    new ReasoningEndStreamPart("3", Metadata("""{"testProvider":{"signature":"1234567890"}}""")),
                    new ReasoningStartStreamPart("4", Metadata("""{"testProvider":{"signature":"1234567890"}}""")),
                    new ReasoningDeltaStreamPart("4", " I need to think about"),
                    new ReasoningDeltaStreamPart("4", " this problem carefully."),
                    new ReasoningStartStreamPart("5", Metadata("""{"testProvider":{"signature":"1234567890"}}""")),
                    new ReasoningDeltaStreamPart("5", " The best solution"),
                    new ReasoningDeltaStreamPart("5", " requires careful"),
                    new ReasoningDeltaStreamPart("5", " consideration of all factors."),
                    new ReasoningEndStreamPart("4", Metadata("""{"testProvider":{"signature":"0987654321"}}""")),
                    new ReasoningEndStreamPart("5", Metadata("""{"testProvider":{"signature":"0987654321"}}""")),
                    new TextStartStreamPart("1"),
                    new TextDeltaStreamPart("1", "Hi"),
                    new TextDeltaStreamPart("1", " there!"),
                    new TextEndStreamPart("1", null, Metadata("""{"testProvider":{"signature":"0987654321"}}""")),
                    new FinishStreamPart(FinishReason.Stop, TestUsage, "stop"),
                },
            },
            Prompt = "prompt",
        });

        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(await result.ResponseMessages));
        var expected = new (string Kind, string Text, string Key, string Value)[]
        {
            ("reasoning", "I will open the conversation with witty banter.", "signature", "1234567890"),
            ("reasoning", string.Empty, "redactedData", "redacted-reasoning-data"),
            ("reasoning", " Once the user has relaxed, I will pry for valuable information.", "signature", "1234567890"),
            ("reasoning", " I need to think about this problem carefully.", "signature", "0987654321"),
            ("reasoning", " The best solution requires careful consideration of all factors.", "signature", "0987654321"),
            ("text", "Hi there!", "signature", "0987654321"),
        };
        Assert.Equal(expected.Length, message.Parts.Count);
        for (var index = 0; index < expected.Length; index++)
        {
            var part = message.Parts[index];
            Assert.Equal(expected[index].Kind, part.Type);
            Assert.Equal(
                expected[index].Text,
                part switch
                {
                    AssistantReasoningPart reasoning => reasoning.Text,
                    AssistantTextPart text => text.Text,
                    _ => throw new InvalidOperationException("Unexpected part " + part.Type),
                });
            Assert.Equal(expected[index].Value, part.ProviderOptions!["testProvider"].GetProperty(expected[index].Key).GetString());
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain assistant response message when there are no tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_assistant_response_message_when_there_are_no_tool_calls()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel
            {
                StreamParts = new LanguageModelStreamPart[]
                {
                    new TextStartStreamPart("1"),
                    new TextDeltaStreamPart("1", "Hello, "),
                    new TextDeltaStreamPart("1", "world!"),
                    new TextEndStreamPart("1"),
                    new FinishStreamPart(FinishReason.Stop, TestUsage, "stop"),
                },
            },
            Prompt = "test-input",
        });

        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(await result.ResponseMessages));
        var text = Assert.IsType<AssistantTextPart>(Assert.Single(message.Parts));
        Assert.Equal("Hello, world!", text.Text);
        Assert.Null(text.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(Prefix + "should contain assistant response message and tool message when there are tool calls with results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_assistant_response_message_and_tool_message_when_there_are_tool_calls_with_results()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel
            {
                StreamParts = new LanguageModelStreamPart[]
                {
                    new TextStartStreamPart("1"),
                    new TextDeltaStreamPart("1", "Hello, "),
                    new TextDeltaStreamPart("1", "world!"),
                    new TextEndStreamPart("1"),
                    new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }"),
                    new FinishStreamPart(FinishReason.Stop, TestUsage, "stop"),
                },
            },
            Tools = new[]
            {
                Tool.Function("tool1", null, ValueSchema, (_, _) => Task.FromResult("result1")),
            },
            Prompt = "test-input",
        });

        var messages = await result.ResponseMessages;
        Assert.Equal(2, messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(messages[0]);
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
        var tool = Assert.IsType<ToolModelMessage>(messages[1]);
        Assert.Equal("text", tool.OutputType);
        Assert.Equal("result1", tool.OutputJson);
        Assert.Equal("call-1", tool.ToolCallId);
        Assert.Equal("tool1", tool.ToolName);
    }
}
