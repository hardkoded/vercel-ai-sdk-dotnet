// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Transformer = Vercel.AI.GenerateText.InvokeToolCallbacksFromStream;

namespace Vercel.AI.Tests.Upstream.GenerateText.InvokeToolCallbacksFromStream;

/// <summary>Port of <c>invoke-tool-callbacks-from-stream.test.ts</c> &gt; <c>invokeToolCallbacksFromStream</c>.</summary>
public sealed class InvokeToolCallbacksFromStreamTests
{
    private const string Id =
        "packages/ai/src/generate-text/invoke-tool-callbacks-from-stream.test.ts::invokeToolCallbacksFromStream::";

    private const string ValueSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    private const string CountSchema =
        "{\"type\":\"object\",\"properties\":{\"count\":{\"type\":\"number\"}},\"required\":[\"count\"]}";

    private static Tool NewTool(string name, string schema = ValueSchema) =>
        Tool.Function(name, null, schema, null);

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static IEnumerable<TextStreamPart> InputParts(
        string toolName,
        string[] deltas,
        GeneratedToolCall call
    )
    {
        yield return new ToolInputStartPart("call-1", toolName, null, null, false);
        foreach (var delta in deltas)
        {
            yield return new ToolInputDeltaPart("call-1", delta, null);
        }

        yield return new ToolInputEndPart("call-1", null);
        yield return new ToolCallPart(call);
    }

    private static async IAsyncEnumerable<TextStreamPart> From(IEnumerable<TextStreamPart> parts)
    {
        foreach (var part in parts)
        {
            await Task.Yield();
            yield return part;
        }
    }

    private static async Task<List<TextStreamPart>> Run(
        IReadOnlyList<TextStreamPart> chunks,
        IReadOnlyList<Tool> tools,
        IReadOnlyList<ModelMessage> messages,
        CancellationToken abortSignal,
        IReadOnlyDictionary<string, JsonElement> toolsContext
    )
    {
        var result = new List<TextStreamPart>();
        await foreach (
            var part in Transformer.Invoke(From(chunks), tools, messages, abortSignal, toolsContext)
        )
        {
            result.Add(part);
        }

        return result;
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should invoke tool callbacks in order with the tool context and pass through the stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_invoke_tool_callbacks_in_order_with_the_tool_context_and_pass_through_the_stream()
    {
        var recorded = new List<string>();
        using var abort = new CancellationTokenSource();
        IReadOnlyList<ModelMessage> stepInputMessages = [new UserModelMessage("test-input")];

        void Record(string type, ToolExecutionOptions options, string? extra = null)
        {
            Assert.Equal("call-1", options.ToolCallId);
            Assert.Same(stepInputMessages, options.Messages);
            Assert.Equal(abort.Token, options.AbortSignal);
            Assert.Equal("{\"prefix\":\"tool-context\"}", options.Context?.GetRawText());
            recorded.Add(type + (extra == null ? string.Empty : ":" + extra));
        }

        var tool = NewTool("test-tool");
        tool.ContextSchema = context =>
            context.TryGetProperty("prefix", out var prefix)
            && prefix.ValueKind == JsonValueKind.String
                ? context
                : throw new InvalidOperationException("invalid");
        tool.OnInputStart = options =>
        {
            Record("onInputStart", options);
            return Task.CompletedTask;
        };
        tool.OnInputDelta = options =>
        {
            Record("onInputDelta", options, options.InputTextDelta);
            return Task.CompletedTask;
        };
        tool.OnInputAvailable = options =>
        {
            Record("onInputAvailable", options, options.Input.GetRawText());
            return Task.CompletedTask;
        };

        var chunks = new List<TextStreamPart> { new TextDeltaPart("text-1", "hello", null) };
        chunks.AddRange(
            InputParts(
                "test-tool",
                ["{\"value\":\"", "Sparkle Day\"}"],
                new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"Sparkle Day\"}")
            )
        );

        var result = await Run(
            chunks,
            [tool],
            stepInputMessages,
            abort.Token,
            new Dictionary<string, JsonElement>
            {
                ["test-tool"] = Json("{\"prefix\":\"tool-context\"}"),
            }
        );

        Assert.Equal(chunks, result);
        Assert.Equal(
            [
                "onInputStart",
                "onInputDelta:{\"value\":\"",
                "onInputDelta:Sparkle Day\"}",
                "onInputAvailable:{\"value\":\"Sparkle Day\"}",
            ],
            recorded
        );
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should validate context once per tool call and reuse the transformed value for all callbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_validate_context_once_per_tool_call_and_reuse_the_transformed_value_for_all_callbacks()
    {
        var validationCount = 0;
        var callbackContexts = new List<JsonElement?>();

        var tool = NewTool("test-tool");
        tool.ContextSchema = context =>
        {
            var transformed = JsonNode.Parse(context.GetRawText())!.AsObject();
            transformed["validationCount"] = ++validationCount;
            return Json(transformed.ToJsonString());
        };
        tool.OnInputStart = options =>
        {
            callbackContexts.Add(options.Context);
            return Task.CompletedTask;
        };
        tool.OnInputDelta = options =>
        {
            callbackContexts.Add(options.Context);
            return Task.CompletedTask;
        };
        tool.OnInputAvailable = options =>
        {
            callbackContexts.Add(options.Context);
            return Task.CompletedTask;
        };

        var chunks = InputParts(
                "test-tool",
                ["{\"value\":\"", "Sparkle Day\"}"],
                new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"Sparkle Day\"}")
            )
            .ToList();

        var result = await Run(
            chunks,
            [tool],
            [],
            default,
            new Dictionary<string, JsonElement>
            {
                ["test-tool"] = Json("{\"prefix\":\"tool-context\",\"validationCount\":0}"),
            }
        );

        Assert.Equal(chunks, result);
        Assert.Equal(1, validationCount);
        Assert.Equal(4, callbackContexts.Count);
        Assert.Equal(
            "{\"prefix\":\"tool-context\",\"validationCount\":1}",
            callbackContexts[0]!.Value.GetRawText()
        );
        Assert.All(callbackContexts, context =>
            Assert.Equal(callbackContexts[0]!.Value.GetRawText(), context!.Value.GetRawText())
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should skip onInputAvailable for invalid tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_skip_onInputAvailable_for_invalid_tool_calls()
    {
        var recorded = new List<string>();
        var tool = NewTool("test-tool");
        tool.OnInputStart = _ =>
        {
            recorded.Add("onInputStart");
            return Task.CompletedTask;
        };
        tool.OnInputDelta = _ =>
        {
            recorded.Add("onInputDelta");
            return Task.CompletedTask;
        };
        tool.OnInputAvailable = _ =>
        {
            recorded.Add("onInputAvailable");
            return Task.CompletedTask;
        };

        var chunks = InputParts(
                "test-tool",
                ["{\"value\":42}"],
                new GeneratedToolCall("call-1", "test-tool", "{\"value\":42}")
                {
                    Invalid = true,
                    Dynamic = true,
                    Error = new InvalidOperationException("invalid tool input"),
                }
            )
            .ToList();

        var result = await Run(chunks, [tool], [], default, new Dictionary<string, JsonElement>());

        Assert.Equal(chunks, result);
        Assert.Equal(["onInputStart", "onInputDelta"], recorded);
    }

    [Fact]
    [UpstreamTest(
        Id + "should invoke onInputAvailable for the tool named by the completed tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_invoke_onInputAvailable_for_the_tool_named_by_the_completed_tool_call()
    {
        var recorded = new List<string>();
        var callbackContexts = new List<string>();

        var originalTool = NewTool("originalTool");
        originalTool.OnInputStart = options =>
        {
            recorded.Add("originalTool.onInputStart");
            callbackContexts.Add("onInputStart:" + options.Context?.GetRawText());
            return Task.CompletedTask;
        };
        originalTool.OnInputDelta = options =>
        {
            recorded.Add("originalTool.onInputDelta");
            callbackContexts.Add("onInputDelta:" + options.Context?.GetRawText());
            return Task.CompletedTask;
        };
        originalTool.OnInputAvailable = _ =>
        {
            recorded.Add("originalTool.onInputAvailable");
            return Task.CompletedTask;
        };
        var repairedTool = NewTool("repairedTool", CountSchema);
        repairedTool.OnInputAvailable = options =>
        {
            callbackContexts.Add("onInputAvailable:" + options.Context?.GetRawText());
            recorded.Add("repairedTool.onInputAvailable:" + options.Input.GetRawText());
            return Task.CompletedTask;
        };

        var chunks = InputParts(
                "originalTool",
                ["{\"count\":3}"],
                new GeneratedToolCall("call-1", "repairedTool", "{\"count\":3}")
            )
            .ToList();

        var result = await Run(
            chunks,
            [originalTool, repairedTool],
            [],
            default,
            new Dictionary<string, JsonElement>
            {
                ["originalTool"] = Json("{\"source\":\"original\"}"),
                ["repairedTool"] = Json("{\"source\":\"repaired\"}"),
            }
        );

        Assert.Equal(chunks, result);
        Assert.Equal(
            [
                "originalTool.onInputStart",
                "originalTool.onInputDelta",
                "repairedTool.onInputAvailable:{\"count\":3}",
            ],
            recorded
        );
        Assert.Equal(
            [
                "onInputStart:{\"source\":\"original\"}",
                "onInputDelta:{\"source\":\"original\"}",
                "onInputAvailable:{\"source\":\"repaired\"}",
            ],
            callbackContexts
        );
    }
}
