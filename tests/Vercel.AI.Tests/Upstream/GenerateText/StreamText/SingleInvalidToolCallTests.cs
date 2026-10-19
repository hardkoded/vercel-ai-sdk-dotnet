// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; invalid tool calls &gt; single invalid tool call</c>.</summary>
public sealed class SingleInvalidToolCallTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > invalid tool calls > single invalid tool call::";

    private const string ErrorStart = "AI_InvalidToolInputError: Invalid input for tool cityAttractions: ";
    private const string Input = "{ \"cities\": \"San Francisco\" }";

    [Fact]
    [UpstreamTest(
        Prefix + "should add tool call and result error parts to the content",
        Coverage = UpstreamCoverage.Partial,
        Note = "The error text carries the built-in JSON Schema validator message, not the Zod error list. See #604.")]
    public async Task Should_add_tool_call_and_result_error_parts_to_the_content()
    {
        var step = Assert.Single(await Run().Steps);

        Assert.Equal(new[] { "tool-call", "tool-result" }, step.Content.Select(part => part.Type));
        var call = Assert.IsType<GeneratedToolCall>(step.Content[0]);
        Assert.True(call.Dynamic);
        Assert.True(call.Invalid);
        Assert.True(InvalidToolInputError.IsInstance(call.Error));
        Assert.Equal("{\"cities\":\"San Francisco\"}", Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("cityAttractions", call.ToolName);
        var error = Assert.IsType<GeneratedToolResult>(step.Content[1]);
        Assert.True(error.IsError);
        Assert.StartsWith(ErrorStart, JsonSerializer.Deserialize<string>(error.ResultJson));
        Assert.Equal("call-1", error.ToolCallId);
        Assert.Equal("cityAttractions", error.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add tool call and result error parts to the full stream",
        Coverage = UpstreamCoverage.Partial,
        Note = "The error text carries the built-in JSON Schema validator message, not the Zod error list. See #604.")]
    public async Task Should_add_tool_call_and_result_error_parts_to_the_full_stream()
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in Run().Stream())
        {
            parts.Add(part);
        }

        Assert.Equal(
            new[] { "start", "start-step", "tool-input-start", "tool-input-delta", "tool-input-end", "tool-call", "tool-error", "finish-step", "finish" },
            parts.Select(part => part.Type).ToArray());
        var inputStart = Assert.IsType<ToolInputStartPart>(parts[2]);
        Assert.False(inputStart.Dynamic);
        Assert.Equal("call-1", inputStart.Id);
        Assert.Equal("cityAttractions", inputStart.ToolName);
        Assert.Equal(Input, Assert.IsType<ToolInputDeltaPart>(parts[3]).Delta);
        Assert.Equal("call-1", Assert.IsType<ToolInputEndPart>(parts[4]).Id);
        var call = Assert.IsType<ToolCallPart>(parts[5]).ToolCall;
        Assert.True(call.Dynamic);
        Assert.True(call.Invalid);
        Assert.True(InvalidToolInputError.IsInstance(call.Error));
        Assert.Equal("{\"cities\":\"San Francisco\"}", Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("cityAttractions", call.ToolName);
        var error = Assert.IsType<ToolErrorPart>(parts[6]).Result;
        Assert.StartsWith(ErrorStart, error.ModelOutput);
        Assert.Equal("{\"cities\":\"San Francisco\"}", Compact(error.InputJson!));
        Assert.Equal("call-1", error.ToolCallId);
        Assert.Equal("cityAttractions", error.ToolName);
        var finishStep = Assert.IsType<StepFinishPart>(parts[7]);
        Assert.Equal(FinishReason.Stop, finishStep.Step.FinishReason);
        Assert.Equal(13, finishStep.Step.Usage.TotalTokens);
        Assert.Equal("stop", finishStep.RawFinishReason);
        var finish = Assert.IsType<FinishPart>(parts[8]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(13, finish.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should add tool call and result error parts to the ui message stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_tool_call_and_result_error_parts_to_the_ui_message_stream()
    {
        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(Run());

        // The .NET stream always sends a generated messageId on start.
        Assert.NotNull(chunks[0]!["messageId"]);
        chunks[0]!.AsObject().Remove("messageId");
        var expected = JsonNode.Parse("""
            [
              { "type": "start" },
              { "type": "start-step" },
              { "toolCallId": "call-1", "toolName": "cityAttractions", "type": "tool-input-start" },
              { "inputTextDelta": "{ \"cities\": \"San Francisco\" }", "toolCallId": "call-1", "type": "tool-input-delta" },
              {
                "errorText": "An error occurred.",
                "input": { "cities": "San Francisco" },
                "toolCallId": "call-1",
                "toolName": "cityAttractions",
                "type": "tool-input-error"
              },
              { "errorText": "An error occurred.", "toolCallId": "call-1", "type": "tool-output-error" },
              { "type": "finish-step" },
              { "finishReason": "stop", "type": "finish" }
            ]
            """);
        Assert.True(JsonNode.DeepEquals(expected, chunks), chunks.ToJsonString());
    }

    private static string Compact(string json) => JsonSerializer.Serialize(JsonDocument.Parse(json).RootElement);

    private static StreamTextResult Run()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                new ToolInputStartStreamPart("call-1", "cityAttractions"),
                new ToolInputDeltaStreamPart("call-1", Input),
                new ToolInputEndStreamPart("call-1"),

                // wrong tool call arguments (city vs cities):
                new ToolCallStreamPart("call-1", "cityAttractions", Input),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13, noCacheInputTokens: 3, textTokens: 10), "stop"),
            },
        };
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function("cityAttractions", null, "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}", null),
            },
            NowMs = () => 0,
        });
    }
}
