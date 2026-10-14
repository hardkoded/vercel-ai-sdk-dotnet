// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; JSON response tool with manual thinking (%s)</c>.</summary>
public sealed class JsonResponseToolWithManualThinkingTests
{
    private const string SseBody = "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"model\":\"claude-sonnet-4-5\",\"usage\":{\"input_tokens\":1,\"output_tokens\":0}}}\n\ndata: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"json\",\"input\":{}}}\n\ndata: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\"{\\\"elements\\\":[]}\"}}\n\ndata: {\"type\":\"content_block_stop\",\"index\":0}\n\ndata: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"tool_use\"},\"usage\":{\"output_tokens\":1}}\n\ndata: {\"type\":\"message_stop\"}\n\n";
    private const string JsonBody = "{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-sonnet-4-5\",\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"json\",\"input\":{\"elements\":[]}}],\"stop_reason\":\"tool_use\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";

    [Theory]
    [InlineData("generate", "claude-sonnet-4-20250514", "auto")]
    [InlineData("generate", "claude-sonnet-4-5", "jsonTool")]
    [InlineData("stream", "claude-sonnet-4-20250514", "auto")]
    [InlineData("stream", "claude-sonnet-4-5", "jsonTool")]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > JSON response tool with manual thinking (%s)::should fall back to auto tool choice for $modelId in $structuredOutputMode mode", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_fall_back_to_auto_tool_choice_for_modelId_in_structuredOutputMode_mode(string method, string modelId, string structuredOutputMode)
    {
        var schema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            MaxOutputTokens = 1000,
            ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
            {
                ["anthropic"] = AnthropicParity.Json("{\"structuredOutputMode\":\"" + structuredOutputMode + "\",\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":5000}}"),
            },
            JsonResponse = true,
            JsonSchema = schema,
        };

        var handler = AnthropicParity.Ok(method == "generate" ? JsonBody : SseBody);
        IReadOnlyList<CallWarning> warnings;
        if (method == "generate")
        {
            var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), modelId, options);
            warnings = result.Warnings;
            var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
            Assert.Contains("\"elements\"", text.Text);
            Assert.Equal(FinishReason.Stop, result.FinishReason);
        }
        else
        {
            handler.ContentType = "text/event-stream";
            var parts = new List<LanguageModelStreamPart>();
            await foreach (var part in (await AnthropicParity.Client(handler).LanguageModel(modelId).DoStreamAsync(options, CancellationToken.None)).ConfigureAwait(false))
            {
                parts.Add(part);
            }

            warnings = Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings;
            Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        }

        AssertToolChoiceWarning(warnings);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("enabled", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(5000, body["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"auto\",\"disable_parallel_tool_use\":true}"), body["tool_choice"]));
        var tool = Assert.Single(body["tools"]!.AsArray());
        Assert.Equal("json", tool!["name"]!.GetValue<string>());
        Assert.Equal("Respond with a JSON object.", tool["description"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(schema.GetRawText()), tool["input_schema"]));
    }

    private static void AssertToolChoiceWarning(IReadOnlyList<CallWarning> warnings)
    {
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("toolChoice", warning.Feature);
        Assert.Contains("not supported with this model or thinking configuration", warning.Message);
    }
}
