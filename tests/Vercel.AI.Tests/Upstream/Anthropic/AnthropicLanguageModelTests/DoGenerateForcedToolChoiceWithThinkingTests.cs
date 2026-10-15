// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; forced tool choice with thinking &gt; $type</c>.</summary>
public sealed class DoGenerateForcedToolChoiceWithThinkingTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > forced tool choice with thinking > $type::";

    private static IReadOnlyList<LanguageModelTool> Tools() => new[] { "lookup", "other" }
        .Select(name => new LanguageModelTool(name, null, AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}")))
        .ToArray();

    private static ToolChoice Choice(string type) => type == "tool" ? ToolChoice.Tool("lookup") : ToolChoice.Required;

    [Theory]
    [InlineData("required", "providerOptions")]
    [InlineData("required", "reasoning")]
    [InlineData("tool", "providerOptions")]
    [InlineData("tool", "reasoning")]
    [UpstreamTest(Prefix + "should fall back to auto for manual thinking set through $source", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_fall_back_to_auto_for_manual_thinking_set_through_source(string type, string source)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Tools = Tools(),
            ToolChoice = Choice(type),
        };
        if (source == "reasoning")
        {
            options.Reasoning = "low";
        }
        else
        {
            options.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
            {
                ["anthropic"] = AnthropicParity.Json("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":5000}}"),
            };
        }

        var handler = AnthropicParity.Ok();
        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-haiku-4-5", options);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("enabled", body["thinking"]!["type"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"auto\"}"), body["tool_choice"]));
        Assert.Equal(
            type == "tool" ? new[] { "lookup" } : new[] { "lookup", "other" },
            body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()).ToArray());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("toolChoice", warning.Feature);
        Assert.Contains("not supported with this model or thinking configuration", warning.Message);
    }

    [Theory]
    [InlineData("required", "claude-haiku-4-5", "disabled")]
    [InlineData("required", "claude-sonnet-4-6", "adaptive")]
    [InlineData("required", "claude-haiku-5-5", "enabled")]
    [InlineData("tool", "claude-haiku-4-5", "disabled")]
    [InlineData("tool", "claude-sonnet-4-6", "adaptive")]
    [InlineData("tool", "claude-haiku-5-5", "enabled")]
    [UpstreamTest(Prefix + "should preserve forced tool choice for $modelId with $thinkingType thinking", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_forced_tool_choice_for_modelId_with_thinkingType_thinking(string type, string modelId, string thinkingType)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Tools = Tools(),
            ToolChoice = Choice(type),
            ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
            {
                ["anthropic"] = AnthropicParity.Json("{\"thinking\":{\"type\":\"" + thinkingType + "\"}}"),
            },
        };

        var handler = AnthropicParity.Ok();
        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), modelId, options);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal(thinkingType == "disabled" ? "disabled" : "adaptive", body["thinking"]!["type"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(type == "tool" ? "{\"type\":\"tool\",\"name\":\"lookup\"}" : "{\"type\":\"any\"}"),
            body["tool_choice"]));
        Assert.Equal(2, body["tools"]!.AsArray().Count);
        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "toolChoice");
    }
}
