// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Transformer = Vercel.AI.GenerateText.ExecuteToolsFromStream;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream;

internal static class ExecuteToolsFromStreamSupport
{
    internal const string Prefix =
        "packages/ai/src/generate-text/execute-tools-from-stream.test.ts::";

    internal const string ValueSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    // Maps to finishChunk of the upstream test.
    internal static ModelCallEndPart FinishChunk(FinishReason finishReason = FinishReason.Stop) =>
        new(
            finishReason,
            finishReason.ToString(),
            new LanguageModelUsage(3, 10, 13),
            null,
            new ModelCallPerformance { ResponseTimeMs = 0 }
        );

    // Maps to mockId({ prefix: 'id' }): the first id is id-0.
    internal static Func<string> MockId()
    {
        var counter = -1;
        return () => "id-" + Interlocked.Increment(ref counter);
    }

    internal static ToolCallPart Call(
        string toolCallId,
        string toolName,
        string input = "{\"value\":\"test\"}",
        bool providerExecuted = false
    ) =>
        new(
            new GeneratedToolCall(toolCallId, toolName, input)
            {
                ProviderExecuted = providerExecuted,
            }
        );

    internal static Tool ValueTool(
        string name,
        Func<string, CancellationToken, Task<string>>? execute
    ) =>
        Tool.Function(
            name,
            null,
            ValueSchema,
            execute == null
                ? null
                : (input, token) => execute(input.GetProperty("value").GetString()!, token)
        );

    internal static GenerateTextOptions Options(
        IReadOnlyList<Tool> tools,
        Action<GenerateTextOptions>? configure = null
    )
    {
        var options = new GenerateTextOptions { Tools = tools, GenerateId = MockId() };
        configure?.Invoke(options);
        return options;
    }

    internal static async IAsyncEnumerable<TextStreamPart> From(params TextStreamPart[] parts)
    {
        foreach (var part in parts)
        {
            await Task.Yield();
            yield return part;
        }
    }

    // Maps to convertReadableStreamToArray(executeToolsFromStream({ ... })) with callId 'test-telemetry-call-id' and no messages.
    internal static async Task<List<TextStreamPart>> Run(
        GenerateTextOptions options,
        params TextStreamPart[] parts
    )
    {
        var result = new List<TextStreamPart>();
        await foreach (
            var part in Transformer.Execute(
                From(parts),
                options,
                "test-telemetry-call-id",
                [],
                () => 0
            )
        )
        {
            result.Add(part);
        }

        return result;
    }

    internal static string Types(IEnumerable<TextStreamPart> parts) =>
        string.Join(",", parts.Select(part => part.Type));

    internal static string Raw(JsonElement? value) => value?.GetRawText() ?? "undefined";
}
