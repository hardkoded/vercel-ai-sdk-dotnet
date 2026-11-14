// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>createExpandedParallelToolCallPrompt</c> and its constants from <c>convert-to-openai-responses-input.test.ts</c>.</summary>
internal static class ExpandedParallelToolCallPrompt
{
    public const string ParallelToolCallInput =
        "{\"tool_uses\":[{\"recipient_name\":\"functions.weather\",\"parameters\":{\"location\":\"San Francisco\"}},{\"recipient_name\":\"functions.cityAttractions\",\"parameters\":{\"city\":\"Rome\"}}]}";

    public const string ParallelToolCallOutput = "{\"temperature\":72}\nColosseum";

    /// <summary>The <c>parallelToolCall</c> metadata of a child. Callers can change it before the prompt is built.</summary>
    public static JsonObject Metadata(int index, bool compactMetadata)
    {
        var metadata = new JsonObject
        {
            ["itemId"] = "fc_parallel",
            ["toolCallId"] = "call_parallel",
            ["toolName"] = "parallel",
        };
        if (!compactMetadata || index == 0)
        {
            metadata["input"] = ParallelToolCallInput;
        }

        metadata["index"] = index;
        metadata["count"] = 2;
        return metadata;
    }

    public static IReadOnlyList<ModelMessage> Create(
        bool withPromptCacheBreakpoints = false,
        bool compactMetadata = false,
        Action<int, JsonObject>? changeMetadata = null,
        Func<int, JsonNode?>? additionalTools = null,
        bool reverseResults = false)
    {
        JsonElement Options(int index, JsonObject? extra = null)
        {
            var metadata = Metadata(index, compactMetadata);
            changeMetadata?.Invoke(index, metadata);
            var openai = new JsonObject { ["parallelToolCall"] = metadata };
            if (extra != null)
            {
                foreach (var property in extra.ToArray())
                {
                    extra.Remove(property.Key);
                    openai[property.Key] = property.Value;
                }
            }

            return JsonDocument.Parse(new JsonObject { ["openai"] = openai }.ToJsonString()).RootElement.Clone();
        }

        JsonObject? ResultExtra(int index, bool breakpoint)
        {
            var extra = new JsonObject();
            if (breakpoint)
            {
                extra["promptCacheBreakpoint"] = new JsonObject { ["mode"] = "explicit" };
            }

            if (additionalTools?.Invoke(index) is { } tools)
            {
                extra["additionalTools"] = tools;
            }

            return extra;
        }

        var weather = new ToolModelMessage(
            "call_parallel_0", "weather", "{\"temperature\":72}", false, "json", Options(0, ResultExtra(0, false)))
        {
            OutputProviderOptions = withPromptCacheBreakpoints
                ? JsonDocument.Parse("{\"openai\":{\"promptCacheBreakpoint\":{\"mode\":\"explicit\"}}}").RootElement.Clone()
                : null,
        };
        var attractions = new ToolModelMessage(
            "call_parallel_1", "cityAttractions", "Colosseum", false, "text", Options(1, ResultExtra(1, withPromptCacheBreakpoints)));
        var results = new List<ToolModelMessage> { weather, attractions };
        if (reverseResults)
        {
            results.Reverse();
        }

        return new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall("call_parallel_0", "weather", "{\"location\":\"San Francisco\"}"), ToOptions(Options(0))),
                new AssistantToolCallPart(new GeneratedToolCall("call_parallel_1", "cityAttractions", "{\"city\":\"Rome\"}"), ToOptions(Options(1))),
            }),
            new ToolModelMessage(results),
        };
    }

    public static (JsonArray Input, int WarningCount) Convert(
        IReadOnlyList<ModelMessage> prompt,
        string systemMessageMode = "system",
        bool hasConversation = false,
        bool hasPreviousResponseId = false)
    {
        var converted = OpenAIResponsesLanguageModel.ConvertInput(
            prompt,
            systemMessageMode,
            false,
            null,
            "openai",
            hasConversation || hasPreviousResponseId,
            true,
            hasConversation: hasConversation,
            hasPreviousResponseId: hasPreviousResponseId);
        return (converted.Input, converted.Warnings.Count);
    }

    public static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    private static Dictionary<string, JsonElement> ToOptions(JsonElement options) =>
        options.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
}
