// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ParseToolCall;

internal static class ParseToolCallFixture
{
    /// <summary><c>z.object({ param1: z.string(), param2: z.number() })</c>.</summary>
    public const string ParamsSchema = "{\"type\":\"object\",\"properties\":{\"param1\":{\"type\":\"string\"},\"param2\":{\"type\":\"number\"}},\"required\":[\"param1\",\"param2\"]}";

    /// <summary><c>z.object({})</c>.</summary>
    public const string EmptyObjectSchema = "{\"type\":\"object\",\"properties\":{}}";

    public const string LocationSchema = "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}},\"additionalProperties\":false}";

    public static Tool TestTool(string schema, string name = "testTool") =>
        Tool.Function(name, null, schema, (_, _) => Task.FromResult("null"));

    public static Task<ParsedToolCall> ParseAsync(
        GeneratedToolCall toolCall,
        IReadOnlyList<Tool>? tools,
        ToolCallRepairFunction? repairToolCall = null,
        IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? refineToolInput = null,
        IReadOnlyList<ModelMessage>? messages = null,
        string? instructions = null,
        CancellationToken cancellationToken = default) =>
        ToolCallParser.ParseToolCallAsync(
            new ParseToolCallOptions(toolCall)
            {
                Tools = tools,
                RepairToolCall = repairToolCall,
                RefineToolInput = refineToolInput,
                Messages = messages,
                Instructions = instructions,
            },
            cancellationToken);

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
