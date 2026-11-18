// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.StreamLanguageModelCall;

/// <summary>The shared helpers of <c>stream-language-model-call.test.ts</c>.</summary>
internal static class StreamLanguageModelCallFixture
{
    /// <summary><c>testUsage</c>: 3 input tokens and 10 output tokens.</summary>
    public static LanguageModelUsage TestUsage => new(3, 10, 13);

    public static FinishStreamPart Finish(
        FinishReason reason = FinishReason.Stop,
        string? raw = "stop"
    ) => new(reason, TestUsage, raw);

    public static TestLanguageModel Model(params LanguageModelStreamPart[] parts) =>
        new() { StreamParts = parts };

    /// <summary><c>mockNow.mockReturnValueOnce(...)</c>: the values in order, then 0 like the mock's default.</summary>
    public static Func<double> Now(params double[] values)
    {
        var queue = new Queue<double>(values);
        return () => queue.Count > 0 ? queue.Dequeue() : 0;
    }

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary><c>streamLanguageModelCallResult</c>.</summary>
    public static async Task<List<TextStreamPart>> StreamLanguageModelCallResult(
        LanguageModelStreamPart[] streamParts,
        IReadOnlyList<Tool>? tools = null,
        ToolCallRepairFunction? repairToolCall = null,
        IReadOnlyDictionary<
            string,
            Func<JsonElement, CancellationToken, Task<JsonElement>>
        >? refineToolInput = null,
        Func<double>? now = null
    )
    {
        var result = await LanguageModelCallStreaming.StreamLanguageModelCall(
            new StreamLanguageModelCallOptions
            {
                Model = Model(streamParts),
                Tools = tools,
                Prompt = "test prompt",
                RepairToolCall = repairToolCall,
                RefineToolInput = refineToolInput,
                NowMs = now ?? (() => 0),
            }
        );
        return await Collect(result);
    }

    public static async Task<List<TextStreamPart>> Collect(StreamLanguageModelCallResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream)
        {
            parts.Add(part);
        }

        return parts;
    }

    public static ModelCallEndPart ModelCallEnd(TextStreamPart part)
    {
        var end = Assert.IsType<ModelCallEndPart>(part);
        Assert.Equal(3, end.Usage.InputTokens);
        Assert.Equal(10, end.Usage.OutputTokens);
        Assert.Equal(13, end.Usage.TotalTokens);
        return end;
    }

    public static void AssertTimeToFirstOutput(ModelCallEndPart end)
    {
        Assert.Equal(600, end.Performance.ResponseTimeMs);
        Assert.Equal(16.666666666666668, end.Performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(28.571428571428573, end.Performance.OutputTokensPerSecond);
        Assert.Equal(12, end.Performance.InputTokensPerSecond);
        Assert.Equal(21.666666666666668, end.Performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(250, end.Performance.TimeToFirstOutputMs);
    }

    public static Tool TestTool() =>
        Tool.Function(
            "testTool",
            null,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
            (_, _) => Task.FromResult("\"result\"")
        );
}
