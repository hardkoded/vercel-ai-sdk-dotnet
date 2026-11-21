// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicJsonResponseToolFallback;

/// <summary>Shared helpers of <c>anthropic-json-response-tool-fallback.test.ts</c>.</summary>
internal static class JsonResponseToolFallbackSupport
{
    public const string Schema =
        "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}";

    public const string CircularSchema =
        "{\"type\":\"object\",\"properties\":{\"root\":{\"$ref\":\"#/$defs/node\"}},\"required\":[\"root\"],\"additionalProperties\":false,\"$defs\":{\"node\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"children\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/$defs/node\"}}},\"required\":[\"name\",\"children\"],\"additionalProperties\":false}}}";

    // Mirrors provider adapters (e.g. Bedrock, Vertex) that disable native structured outputs by default.
    public sealed class HostProvider(CaptureHandler handler)
        : AnthropicProvider(
            new HttpClient(handler, disposeHandler: false),
            new AnthropicOptions { ApiKey = "test-api-key" }
        )
    {
        protected internal override bool SupportsNativeStructuredOutput => false;

        protected internal override bool SupportsStrictTools => false;
    }

    public static LanguageModelCallOptions JsonCall(string schema)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            JsonSchema = AnthropicParity.Json(schema),
        };
    }

    // Manual thinking also downgrades the forced JSON response tool to auto tool choice,
    // for models that otherwise accept forced tool use.
    public static LanguageModelCallOptions ManualThinkingCall()
    {
        var options = JsonCall(Schema);
        options.MaxOutputTokens = 10000;
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["anthropic"] = AnthropicParity.Json(
                "{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":5000}}"
            ),
        };
        return options;
    }

    public static string MessageResponse(params JsonNode[] content)
    {
        var hasToolUse = content.Any(part => part["type"]!.GetValue<string>() == "tool_use");
        return new JsonObject
        {
            ["id"] = "msg_1",
            ["type"] = "message",
            ["role"] = "assistant",
            ["model"] = "claude-sonnet-5-5",
            ["content"] = new JsonArray(content),
            ["stop_reason"] = hasToolUse ? "tool_use" : "end_turn",
            ["stop_sequence"] = null,
            ["usage"] = new JsonObject { ["input_tokens"] = 10, ["output_tokens"] = 20 },
        }.ToJsonString();
    }

    public static JsonNode Text(string text, JsonNode? citation = null)
    {
        var part = new JsonObject { ["type"] = "text", ["text"] = text };
        if (citation != null)
        {
            part["citations"] = new JsonArray(citation);
        }

        return part;
    }

    public static JsonNode ToolUse(string id, string name, string input)
    {
        return new JsonObject
        {
            ["type"] = "tool_use",
            ["id"] = id,
            ["name"] = name,
            ["input"] = JsonNode.Parse(input),
        };
    }

    public static JsonNode[] MessageEvents(
        IEnumerable<JsonNode> events,
        string id = "msg_1",
        string stopReason = "end_turn"
    )
    {
        var start = new JsonObject
        {
            ["type"] = "message_start",
            ["message"] = new JsonObject
            {
                ["id"] = id,
                ["type"] = "message",
                ["role"] = "assistant",
                ["model"] = "claude-sonnet-5-5",
                ["content"] = new JsonArray(),
                ["stop_reason"] = null,
                ["stop_sequence"] = null,
                ["usage"] = new JsonObject { ["input_tokens"] = 10, ["output_tokens"] = 1 },
            },
        };
        var delta = new JsonObject
        {
            ["type"] = "message_delta",
            ["delta"] = new JsonObject { ["stop_reason"] = stopReason, ["stop_sequence"] = null },
            ["usage"] = new JsonObject { ["output_tokens"] = 20 },
        };
        return [start, .. events, delta, new JsonObject { ["type"] = "message_stop" }];
    }

    public static IEnumerable<JsonNode> ToolBlockEvents(int index, string name)
    {
        return ToolBlock(index, "toolu_" + index, name, "{}");
    }

    public static IEnumerable<JsonNode> JsonToolBlockEvents(int index, string json)
    {
        return ToolBlock(index, "toolu_1", "json", json);
    }

    public static IEnumerable<JsonNode> TextBlockEvents(int index, params string[] deltas)
    {
        yield return new JsonObject
        {
            ["type"] = "content_block_start",
            ["index"] = index,
            ["content_block"] = new JsonObject { ["type"] = "text", ["text"] = "" },
        };
        foreach (var text in deltas)
        {
            yield return Delta(index, new JsonObject { ["type"] = "text_delta", ["text"] = text });
        }

        yield return Stop(index);
    }

    public static IEnumerable<JsonNode> ThinkingBlockEvents(int index, string thinking)
    {
        yield return new JsonObject
        {
            ["type"] = "content_block_start",
            ["index"] = index,
            ["content_block"] = new JsonObject { ["type"] = "thinking", ["thinking"] = "" },
        };
        yield return Delta(
            index,
            new JsonObject { ["type"] = "thinking_delta", ["thinking"] = thinking }
        );
        yield return Delta(
            index,
            new JsonObject { ["type"] = "signature_delta", ["signature"] = "sig" }
        );
        yield return Stop(index);
    }

    public static JsonNode Delta(int index, JsonNode delta)
    {
        return new JsonObject
        {
            ["type"] = "content_block_delta",
            ["index"] = index,
            ["delta"] = delta,
        };
    }

    public static JsonNode Stop(int index)
    {
        return new JsonObject { ["type"] = "content_block_stop", ["index"] = index };
    }

    public static async Task<(JsonNode Body, LanguageModelGenerateResult Result)> Generate(
        string modelId,
        LanguageModelCallOptions options,
        string response
    )
    {
        var handler = AnthropicParity.Ok(response);
        var result = await new HostProvider(handler)
            .LanguageModel(modelId)
            .DoGenerateAsync(options, CancellationToken.None);
        return (JsonNode.Parse(handler.Body)!, result);
    }

    // Each argument is one message in the same provider stream.
    public static async Task<List<LanguageModelStreamPart>> Stream(
        string modelId,
        LanguageModelCallOptions options,
        params JsonNode[][] messages
    )
    {
        var handler = AnthropicParity.Ok(
            string.Concat(
                messages
                    .SelectMany(message => message)
                    .Select(e => "data: " + e.ToJsonString() + "\n\n")
            ) + "data: [DONE]\n\n"
        );
        handler.ContentType = "text/event-stream";
        var stream = await new HostProvider(handler)
            .LanguageModel(modelId)
            .DoStreamAsync(options, CancellationToken.None);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in stream.Stream)
        {
            parts.Add(part);
        }

        return parts;
    }

    public static string TextOf(IEnumerable<LanguageModelStreamPart> parts)
    {
        return string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
    }

    private static IEnumerable<JsonNode> ToolBlock(int index, string id, string name, string json)
    {
        yield return new JsonObject
        {
            ["type"] = "content_block_start",
            ["index"] = index,
            ["content_block"] = new JsonObject
            {
                ["type"] = "tool_use",
                ["id"] = id,
                ["name"] = name,
                ["input"] = new JsonObject(),
            },
        };
        yield return Delta(
            index,
            new JsonObject { ["type"] = "input_json_delta", ["partial_json"] = json }
        );
        yield return Stop(index);
    }
}
