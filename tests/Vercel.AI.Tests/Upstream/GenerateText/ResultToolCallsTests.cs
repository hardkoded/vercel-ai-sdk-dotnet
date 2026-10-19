// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; result.toolCalls</c>.</summary>
public sealed class ResultToolCallsTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.toolCalls::";

    private const string ValueSchema = "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"type\":\"object\"}";
    private const string SomethingElseSchema = "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"somethingElse\":{\"type\":\"string\"}},\"required\":[\"somethingElse\"],\"type\":\"object\"}";

    [Fact]
    [UpstreamTest(Prefix + "should contain tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_tool_calls()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => ToolCallResponse("{ \"value\": \"value\" }"),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[]
            {
                Tool.Function("tool1", null, ValueSchema, null),
                Tool.Function("tool2", null, SomethingElseSchema, null),
            },
            ToolChoice = ToolChoice.Required,
            Prompt = "test-input",
        });

        var call = Assert.Single(model.Calls);
        Assert.Equal(new[] { "tool1", "tool2" }, call.Tools!.Select(tool => tool.Name));
        Assert.Equal(ValueSchema, call.Tools![0].InputSchema.GetRawText());
        Assert.Equal(SomethingElseSchema, call.Tools![1].InputSchema.GetRawText());
        Assert.Equal("required", call.ToolChoice!.Type);
        var user = Assert.IsType<UserModelMessage>(Assert.Single(call.Prompt));
        Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);

        var toolCall = Assert.Single(result.ToolCalls);
        Assert.Equal("call-1", toolCall.ToolCallId);
        Assert.Equal("tool1", toolCall.ToolName);
        Assert.Equal("{ \"value\": \"value\" }", toolCall.ArgumentsJson);
        Assert.False(toolCall.ProviderExecuted);
        Assert.False(toolCall.Dynamic);
        Assert.False(toolCall.Invalid);
        Assert.Null(toolCall.ProviderMetadata);
        Assert.Null(toolCall.Title);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should refine tool input before tool execution, outputs, and callbacks",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no onToolExecutionStart callback, so the tool execution start event is not checked. See #623.")]
    public async Task Should_refine_tool_input_before_tool_execution_outputs_and_callbacks()
    {
        var modelCallEndEvents = new List<LanguageModelCallEndEvent>();
        var model = new TestLanguageModel
        {
            OnGenerate = _ => ToolCallResponse("{ \"value\": \" raw \" }"),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[]
            {
                Tool.Function("tool1", null, ValueSchema, (input, _) =>
                {
                    Assert.Equal("{\"value\":\"raw\"}", Compact(input));
                    return Task.FromResult("\"result:" + input.GetProperty("value").GetString() + "\"");
                }),
            },
            RefineToolInput = new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
            {
                ["tool1"] = (input, _) => Task.FromResult(JsonSerializer.SerializeToElement(new { value = input.GetProperty("value").GetString()!.Trim() })),
            },
            OnLanguageModelCallEnd = (@event, _) =>
            {
                modelCallEndEvents.Add(@event);
                return Task.CompletedTask;
            },
            Prompt = "test-input",
        });

        Assert.Equal("{\"value\":\"raw\"}", Compact(result.ToolCalls[0].ArgumentsJson));
        Assert.Equal("{\"value\":\"raw\"}", Compact(result.ToolResults[0].InputJson!));
        Assert.Equal("\"result:raw\"", result.ToolResults[0].OutputJson);
        var callEnd = Assert.IsType<GeneratedToolCall>(modelCallEndEvents[0].Content[0]);
        Assert.Equal("{\"value\":\"raw\"}", Compact(callEnd.ArgumentsJson));
    }

    internal static AiClient Client() => new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    internal static LanguageModelGenerateResult ToolCallResponse(string input, string toolName = "tool1") =>
        new(
            new GeneratedContent[] { new GeneratedToolCall("call-1", toolName, input) },
            FinishReason.ToolCalls,
            new LanguageModelUsage(3, 10, 13));

    internal static string Compact(string json) => Compact(JsonDocument.Parse(json).RootElement);

    internal static string Compact(JsonElement json) => JsonSerializer.Serialize(json);
}
