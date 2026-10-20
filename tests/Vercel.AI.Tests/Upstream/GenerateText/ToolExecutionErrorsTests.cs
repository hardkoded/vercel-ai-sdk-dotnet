// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution errors</c>.</summary>
public sealed class ToolExecutionErrorsTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution errors::";

    private const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    [Fact]
    [UpstreamTest(Prefix + "should add tool error part to the content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_tool_error_part_to_the_content()
    {
        var result = await RunAsync();

        Assert.Equal(new[] { "tool-call", "tool-result" }, result.Content.Select(part => part.Type));
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("{ \"value\": \"value\" }", call.ArgumentsJson);
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        var error = Assert.IsType<GeneratedToolResult>(result.Content[1]);
        Assert.True(error.IsError);
        Assert.False(error.Dynamic);
        Assert.Equal("Error: test error", JsonSerializer.Deserialize<string>(error.ResultJson));
        Assert.Equal("test error", error.Error!.Message);
        Assert.Equal("call-1", error.ToolCallId);
        Assert.Equal("tool1", error.ToolName);
        var executed = Assert.Single(result.ToolResults);
        Assert.Equal("{\"value\":\"value\"}", Compact(executed.InputJson!));
    }

    [Fact]
    [UpstreamTest(Prefix + "should include error result in response messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_error_result_in_response_messages()
    {
        var result = await RunAsync();

        Assert.Equal(new[] { "assistant", "tool" }, result.ResponseMessages.Select(message => message.Role));
        var assistant = Assert.IsType<AssistantModelMessage>(result.ResponseMessages[0]);
        var call = Assert.IsType<AssistantToolCallPart>(Assert.Single(assistant.Parts)).ToolCall;
        Assert.Equal("{ \"value\": \"value\" }", call.ArgumentsJson);
        Assert.False(call.ProviderExecuted);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        var tool = Assert.IsType<ToolModelMessage>(result.ResponseMessages[1]);
        Assert.Equal("error-text", tool.OutputType);
        Assert.Equal("Error: test error", tool.OutputJson);
        Assert.Equal("call-1", tool.ToolCallId);
        Assert.Equal("tool1", tool.ToolName);
    }

    private static Task<GenerateTextResult> RunAsync()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => ToolCallResponse("{ \"value\": \"value\" }", "tool1"),
        };
        return Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[]
            {
                Tool.Function("tool1", null, ValueSchema, (_, _) => throw new JsError("test error")),
            },
            Prompt = "test-input",
        });
    }
}
