// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using ConversationModel = Vercel.AI.Mistral.MistralConversationLanguageModel;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralConversationLanguageModel;

/// <summary>Shared constants and helpers of <c>mistral-conversation-language-model.test.ts</c>.</summary>
internal static class MistralConversationFixtures
{
    public const string UsageJson = "{\"prompt_tokens\":10,\"completion_tokens\":20,\"total_tokens\":30,\"connector_tokens\":5,\"connectors\":{\"web_search\":1}}";

    public const string Execution = "{\"type\":\"tool.execution\",\"id\":\"search-1\",\"name\":\"web_search\",\"arguments\":\"latest results\",\"info\":{\"status\":\"success\"}}";

    public const string Reference = "{\"type\":\"tool_reference\",\"tool\":\"web_search\",\"title\":\"Results\",\"url\":\"https://example.com/results\"}";

    public static readonly string Answer = "{\"type\":\"message.output\",\"model\":\"mistral-small-latest\",\"content\":[{\"type\":\"text\",\"text\":\"Spain won.\"}," + Reference + "]}";

    public static readonly IReadOnlyList<ModelMessage> PromptMessages = new ModelMessage[] { new UserModelMessage("Who won?") };

    /// <summary>Maps to <c>mockId()</c>.</summary>
    public static Func<string> MockId()
    {
        var count = 0;
        return () => "id-" + Interlocked.Increment(ref count).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public static MistralProvider Provider(HttpMessageHandler handler)
    {
        return MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key", GenerateId = MockId() }, handler);
    }

    public static ConversationModel Model(HttpMessageHandler handler, string modelId = "mistral-small-latest")
    {
        return Provider(handler).Conversation(modelId);
    }

    public static string GenerateBody(string outputs, string? usage = UsageJson)
    {
        return "{\"conversation_id\":\"conversation-1\",\"outputs\":" + outputs + (usage == null ? string.Empty : ",\"usage\":" + usage) + "}";
    }

    public static UpstreamCapture Capture(string? outputs = null)
    {
        return new UpstreamCapture { ResponseBody = GenerateBody(outputs ?? "[" + Execution + "," + Answer + "]") };
    }

    public static UpstreamCapture StreamCapture(params string[] events)
    {
        var capture = new UpstreamCapture();
        Stream(capture, events);
        return capture;
    }

    public static void Stream(UpstreamCapture capture, params string[] events)
    {
        capture.MediaType = "text/event-stream";
        capture.ResponseBody = string.Concat(events.Select(item => "data: " + item + "\n\n"));
    }

    public static LanguageModelTool Search(string id = "mistral.web_search", string name = "search")
    {
        return LanguageModelTool.ProviderDefined(id, name, Node("{}"));
    }

    public static IReadOnlyList<LanguageModelTool> Tools => new[] { Search() };

    public static JsonElement Node(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static JsonNode Body(UpstreamCapture capture, int index = 0)
    {
        return JsonNode.Parse(capture.Requests[index].Body)!;
    }

    public static Dictionary<string, JsonElement> MistralOptions(string json)
    {
        return new Dictionary<string, JsonElement> { ["mistral"] = Node(json) };
    }

    public static async Task<List<LanguageModelStreamPart>> Read(LanguageModelStreamResult result)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result.Stream.ConfigureAwait(false))
        {
            parts.Add(part);
        }

        return parts;
    }

    public static GeneratedToolCall ToolCall(LanguageModelStreamPart part)
    {
        var call = (ToolCallStreamPart)part;
        return new GeneratedToolCall(call.ToolCallId, call.ToolName, call.ArgumentsJson, call.ProviderMetadata) { ProviderExecuted = true };
    }

    public static IReadOnlyList<ModelMessage> Followup(GeneratedToolCall toolCall, string info = "{\"status\":\"success\"}")
    {
        return new ModelMessage[]
        {
            PromptMessages[0],
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(toolCall),
                new AssistantToolResultPart(
                    toolCall.ToolCallId,
                    toolCall.ToolName,
                    new ToolModelMessage(toolCall.ToolCallId, toolCall.ToolName, "{\"info\":" + info + "}", false, "json")),
                new AssistantTextPart("Spain won."),
            }),
            new UserModelMessage("Who did they beat?"),
        };
    }

    /// <summary>Asserts that the JSON equals the expected JSON text.</summary>
    public static void AssertJson(string expected, JsonNode? actual)
    {
        var left = JsonNode.Parse(expected);
        var right = actual == null ? null : JsonNode.Parse(actual.ToJsonString());
        Assert.True(JsonNode.DeepEquals(left, right), "Expected " + expected + " but was " + actual?.ToJsonString());
    }

    /// <summary>Maps to <c>toMatchObject</c>: every property of the expected object is in the actual object.</summary>
    public static void AssertMatches(string expected, JsonNode? actual)
    {
        Assert.True(Subset(JsonNode.Parse(expected), actual == null ? null : JsonNode.Parse(actual.ToJsonString())), "Expected " + expected + " to match " + actual?.ToJsonString());
    }

    private static bool Subset(JsonNode? expected, JsonNode? actual)
    {
        switch (expected)
        {
            case JsonObject expectedObject:
                return actual is JsonObject actualObject
                    && expectedObject.All(pair => actualObject.ContainsKey(pair.Key) && Subset(pair.Value, actualObject[pair.Key]));
            case JsonArray expectedArray:
                return actual is JsonArray actualArray
                    && actualArray.Count == expectedArray.Count
                    && expectedArray.Select((item, index) => Subset(item, actualArray[index])).All(match => match);
            default:
                return JsonNode.DeepEquals(expected, actual);
        }
    }
}
