// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; named tool choice with inline tools (%s)</c>.</summary>
public sealed class NamedToolChoiceWithInlineToolsTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > named tool choice with inline tools (%s)::";

    public static IEnumerable<object[]> Methods() => new[] { new object[] { "generate" }, new object[] { "stream" } };

    public static IEnumerable<object[]> InitialSystem()
    {
        foreach (var method in new[] { "generate", "stream" })
        {
            foreach (var useJsonTool in new[] { false, true })
            {
                yield return new object[] { method, useJsonTool };
            }
        }
    }

    public static IEnumerable<object[]> Preserved()
    {
        foreach (var method in new[] { "generate", "stream" })
        {
            yield return new object[] { method, "claude-haiku-4-5", "{\"type\":\"enabled\",\"budgetTokens\":1024}", "selected", "tool" };
            yield return new object[] { method, "claude-haiku-4-5", "{\"type\":\"enabled\",\"budgetTokens\":1024}", "other", "auto" };
            yield return new object[] { method, "claude-haiku-4-5", "{\"type\":\"enabled\",\"budgetTokens\":1024}", "other", "required" };
            yield return new object[] { method, "claude-sonnet-4-6", "{\"type\":\"adaptive\"}", "other", "tool" };
            yield return new object[] { method, "claude-haiku-5-5", "{\"type\":\"enabled\",\"budgetTokens\":1024}", "other", "tool" };
        }
    }

    [Theory]
    [MemberData(nameof(InitialSystem))]
    [UpstreamTest(Prefix + "should keep ignoring initial system tool additions with JSON tool %s", Coverage = UpstreamCoverage.Covered,
        Commit = "b3088a77b09ed53f2dbc6f90d71b1b2e117367d4")]
    public async Task Should_keep_ignoring_initial_system_tool_additions_with_JSON_tool(string method, bool useJsonTool)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage(
                    "Initial instructions",
                    InlineToolHarness.Options("{\"anthropic\":{\"toolChanges\":[{\"type\":\"tool_addition\",\"tool\":" + InlineToolHarness.Function("other") + "}]}}")),
                new UserModelMessage("Hello"),
            },
            Tools = new[] { InlineToolHarness.SelectedTool() },
            ToolChoice = ToolChoice.Tool("selected"),
        };
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["anthropic"] = AnthropicParity.Json("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":1024},\"structuredOutputMode\":\"jsonTool\"}"),
        };
        if (useJsonTool)
        {
            options.JsonResponse = true;
            options.JsonSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}");
        }

        var (handler, warnings) = await InlineToolHarness.Run(method, "claude-haiku-4-5", options);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("[{\"type\":\"text\",\"text\":\"Initial instructions\"}]"), body["system"]));
        Assert.Single(body["messages"]!.AsArray());
        Assert.Equal("user", body["messages"]![0]!["role"]!.GetValue<string>());
        Assert.Contains(warnings, warning => warning.Type == "other" && warning.Message == "tool changes on the initial system message are not supported by Anthropic. Configure the initial tool set via the tools option instead. The tool changes have been ignored.");
    }

    [Theory]
    [MemberData(nameof(Preserved))]
    [UpstreamTest(Prefix + "should preserve inline $toolName with $choice choice on $modelId", Coverage = UpstreamCoverage.Covered,
        Commit = "ba302a14fcd98fbfa363130a53d46d7b61d22cff")]
    public async Task Should_preserve_inline_toolName_with_choice_choice_on_modelId(string method, string modelId, string thinking, string toolName, string choice)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Hello"),
                InlineToolHarness.ToolChange(InlineToolHarness.Function(toolName)),
            },
            ToolChoice = choice == "tool" ? ToolChoice.Tool("selected") : choice == "auto" ? ToolChoice.Auto : ToolChoice.Required,
            ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = AnthropicParity.Json("{\"thinking\":" + thinking + "}") },
        };

        var (handler, _) = await InlineToolHarness.Run(method, modelId, options);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Null(body["tools"]);
        var part = body["messages"]![1]!["content"]![0]!;
        Assert.Equal("tool_addition", part["type"]!.GetValue<string>());
        Assert.Equal("tool_definition", part["tool"]!["type"]!.GetValue<string>());
        Assert.Equal(toolName, part["tool"]!["definition"]!["name"]!.GetValue<string>());
        var expectedChoice = choice == "tool" && modelId != "claude-haiku-4-5" ? "{\"type\":\"tool\",\"name\":\"selected\"}" : "{\"type\":\"auto\"}";
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expectedChoice), body["tool_choice"]));
    }
}

/// <summary>Shared request builders for the named tool choice with inline tools tests.</summary>
internal static class InlineToolHarness
{
    private const string StreamBody = "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-opus-4-8\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}}\n\ndata: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":1}}\n\ndata: {\"type\":\"message_stop\"}\n\n";

    private const string JsonToolStreamBody = "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"model\":\"claude-opus-4-8\",\"usage\":{\"input_tokens\":1,\"output_tokens\":0}}}\n\ndata: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"json\",\"input\":{}}}\n\ndata: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\"{}\"}}\n\ndata: {\"type\":\"content_block_stop\",\"index\":0}\n\ndata: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"tool_use\"},\"usage\":{\"output_tokens\":1}}\n\ndata: {\"type\":\"message_stop\"}\n\n";

    private const string JsonToolBody = "{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-opus-4-8\",\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"json\",\"input\":{}}],\"stop_reason\":\"tool_use\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";

    public static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    public static LanguageModelTool SelectedTool() => new LanguageModelTool("selected", null, AnthropicParity.Json("{\"type\":\"object\"}"));

    public static string Function(string name) => "{\"type\":\"function\",\"name\":\"" + name + "\",\"inputSchema\":{\"type\":\"object\"}}";

    public static SystemModelMessage ToolChange(string toolJson)
    {
        return new SystemModelMessage(string.Empty, Options("{\"anthropic\":{\"toolChanges\":[{\"type\":\"tool_addition\",\"tool\":" + toolJson + "}]}}"));
    }

    /// <summary>Runs one generate or stream call and returns the handler and the call warnings.</summary>
    public static async Task<(CaptureHandler Handler, IReadOnlyList<CallWarning> Warnings)> Run(string method, string modelId, LanguageModelCallOptions options, bool jsonTool = false)
    {
        var handler = AnthropicParity.Ok(method == "generate" ? (jsonTool ? JsonToolBody : null) : (jsonTool ? JsonToolStreamBody : StreamBody));
        var model = AnthropicParity.Client(handler).LanguageModel(modelId);
        if (method == "generate")
        {
            var result = await model.DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
            return (handler, result.Warnings);
        }

        handler.ContentType = "text/event-stream";
        IReadOnlyList<CallWarning> warnings = Array.Empty<CallWarning>();
        await foreach (var part in (await model.DoStreamAsync(options, CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false))
        {
            if (part is StreamStartStreamPart start)
            {
                warnings = start.Warnings;
            }
        }

        return (handler, warnings);
    }
}
