// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; invalid tool calls &gt; single invalid tool call</c>.</summary>
public sealed class SingleInvalidToolCallTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > invalid tool calls > single invalid tool call::";

    private const string ErrorStart = "AI_InvalidToolInputError: Invalid input for tool cityAttractions: ";

    [Fact]
    [UpstreamTest(Prefix + "should not request approval or execute the invalid tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_request_approval_or_execute_the_invalid_tool_call()
    {
        var (result, executeCalls) = await RunAsync();

        Assert.Empty(result.Content.OfType<GeneratedToolApprovalRequest>());
        Assert.Equal(0, executeCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add tool error part to the content",
        Coverage = UpstreamCoverage.Partial,
        Note = "The error text carries the built-in JSON Schema validator message, not the Zod error list. See #604.")]
    public async Task Should_add_tool_error_part_to_the_content()
    {
        var (result, _) = await RunAsync();

        Assert.Equal(new[] { "tool-call", "tool-result" }, result.Content.Select(part => part.Type));
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.True(call.Dynamic);
        Assert.True(call.Invalid);
        Assert.True(InvalidToolInputError.IsInstance(call.Error));
        Assert.Equal("{\"cities\":\"San Francisco\"}", Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("cityAttractions", call.ToolName);
        var error = Assert.IsType<GeneratedToolResult>(result.Content[1]);
        Assert.True(error.IsError);
        Assert.StartsWith(ErrorStart, System.Text.Json.JsonSerializer.Deserialize<string>(error.ResultJson));
        Assert.Equal("call-1", error.ToolCallId);
        Assert.Equal("cityAttractions", error.ToolName);
        var toolResult = Assert.Single(result.ToolResults);
        Assert.Equal("{\"cities\":\"San Francisco\"}", Compact(toolResult.InputJson!));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include error result in response messages",
        Coverage = UpstreamCoverage.Partial,
        Note = "The error text carries the built-in JSON Schema validator message, not the Zod error list. See #604.")]
    public async Task Should_include_error_result_in_response_messages()
    {
        var (result, _) = await RunAsync();

        Assert.Equal(new[] { "assistant", "tool" }, result.ResponseMessages.Select(message => message.Role));
        var assistant = Assert.IsType<AssistantModelMessage>(result.ResponseMessages[0]);
        var call = Assert.IsType<AssistantToolCallPart>(Assert.Single(assistant.Parts)).ToolCall;
        Assert.Equal("{\"cities\":\"San Francisco\"}", Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("cityAttractions", call.ToolName);
        var tool = Assert.IsType<ToolModelMessage>(result.ResponseMessages[1]);
        Assert.Equal("error-text", tool.OutputType);
        Assert.StartsWith(ErrorStart, tool.OutputJson);
        Assert.Equal("call-1", tool.ToolCallId);
        Assert.Equal("cityAttractions", tool.ToolName);
    }

    private static async Task<(GenerateTextResult Result, int ExecuteCalls)> RunAsync()
    {
        var executeCalls = 0;
        var model = new Vercel.AI.Testing.TestLanguageModel
        {
            // wrong tool call arguments (city vs cities):
            OnGenerate = _ => ToolCallResponse("{ \"cities\": \"San Francisco\" }", "cityAttractions"),
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[]
            {
                new Tool(
                    "cityAttractions",
                    null,
                    System.Text.Json.JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}").RootElement.Clone(),
                    (_, _) =>
                    {
                        executeCalls++;
                        return Task.FromResult("null");
                    })
                {
                    NeedsApproval = true,
                },
            },
            Prompt = "What are the tourist attractions in San Francisco?",
        });
        return (result, executeCalls);
    }
}
