// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall;

/// <summary>Helpers shared by the tests of <c>execute-tool-call.test.ts</c>: <c>createToolCall</c>, the mocked <c>now</c>, and a call to <c>executeToolCall</c>.</summary>
internal static class ExecuteToolCallSupport
{
    internal const string ValueSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    internal static GeneratedToolCall CreateToolCall(
        string toolCallId = "call-1",
        string toolName = "testTool",
        bool dynamic = false,
        JsonElement? providerMetadata = null,
        IReadOnlyDictionary<string, JsonElement>? toolMetadata = null
    ) =>
        new(toolCallId, toolName, "{\"value\":\"test\"}", providerMetadata)
        {
            Dynamic = dynamic,
            ToolMetadata = toolMetadata,
        };

    internal static string MessageText(ModelMessage message) =>
        Assert
            .IsType<TextContentPart>(
                Assert.Single(Assert.IsType<UserModelMessage>(message).Content)
            )
            .Text;

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>The mocked <c>now</c>: returns the queued values in order, then 0 (<c>mockReturnValue(0)</c>).</summary>
    internal static Func<double> MockNow(params double[] values)
    {
        var queue = new Queue<double>(values);
        return () => queue.Count > 0 ? queue.Dequeue() : 0;
    }

    internal static Tool ValueTool(
        Func<string, CancellationToken, Task<string>> execute,
        Func<JsonElement, JsonElement>? contextSchema = null,
        bool dynamic = false
    )
    {
        var tool = Tool.Function("testTool", null, ValueSchema, (input, token) =>
            execute(input.GetProperty("value").GetString()!, token)
        );
        tool.ContextSchema = contextSchema;
        tool.IsDynamic = dynamic;
        return tool;
    }

    internal static Tool ResultTool(Func<JsonElement, JsonElement>? contextSchema = null) =>
        ValueTool(
            (value, _) => Task.FromResult(JsonSerializer.Serialize(value + "-result")),
            contextSchema
        );

    internal static Tool FailingTool(
        Exception error,
        Func<JsonElement, JsonElement>? contextSchema = null,
        bool dynamic = false
    ) => ValueTool((_, _) => throw error, contextSchema, dynamic);

    /// <summary>The <c>contextSchema</c> <c>z.object({ key1: z.string() })</c>.</summary>
    internal static JsonElement Key1StringSchema(JsonElement context)
    {
        if (
            context.ValueKind != JsonValueKind.Object
            || !context.TryGetProperty("key1", out var key1)
            || key1.ValueKind != JsonValueKind.String
        )
        {
            throw new FormatException("key1 must be a string");
        }

        return context;
    }

    internal static IReadOnlyDictionary<string, JsonElement> Key1Context(string value) =>
        new Dictionary<string, JsonElement> { ["testTool"] = Json("{\"key1\":\"" + value + "\"}") };

    internal static Task<(ExecutedTool Output, double ToolExecutionMs)?> Execute(
        GeneratedToolCall toolCall,
        IReadOnlyList<Tool>? tools,
        IReadOnlyList<ModelMessage>? messages = null,
        IReadOnlyDictionary<string, JsonElement>? toolsContext = null,
        Func<double>? now = null,
        ISandboxSession? experimentalSandbox = null,
        IReadOnlyList<
            Func<ToolExecutionStartEvent, CancellationToken, Task>
        >? onToolExecutionStart = null,
        IReadOnlyList<Func<ToolExecutionEndEvent, CancellationToken, Task>>? onToolExecutionEnd =
            null,
        Func<ExecuteToolInTelemetryContextParams, Task>? executeToolInTelemetryContext = null,
        TimeoutConfiguration? timeout = null,
        CancellationToken abortSignal = default
    ) =>
        ToolCallExecution.ExecuteToolCall(
            toolCall,
            tools,
            toolsContext ?? new Dictionary<string, JsonElement>(),
            "test-telemetry-call-id",
            messages ?? Array.Empty<ModelMessage>(),
            abortSignal,
            now ?? MockNow(),
            timeout,
            experimentalSandbox,
            null,
            onToolExecutionStart,
            onToolExecutionEnd,
            executeToolInTelemetryContext
        );
}
