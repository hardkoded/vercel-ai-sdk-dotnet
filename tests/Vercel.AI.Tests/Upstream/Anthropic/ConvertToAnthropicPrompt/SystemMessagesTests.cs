// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>system messages</c>.</summary>
public sealed class SystemMessagesTests
{
    private const string Prefix = "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::";

    private static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert a single system message into an anthropic system message", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_a_single_system_message_into_an_anthropic_system_message()
    {
        await AnthropicCases.Run(Prefix + "should convert a single system message into an anthropic system message").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Prefix + "should emit tool definitions for tools added by value and prefer the inline tools beta", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public void Should_emit_tool_definitions_for_tools_added_by_value_and_prefer_the_inline_tools_beta()
    {
        const string definitionJson = "{\"name\":\"get_forecast\",\"description\":\"Get forecast\",\"input_schema\":{\"type\":\"object\"}}";

        var result = AnthropicPrompt.Convert(
            new ModelMessage[]
            {
                new SystemModelMessage("initial"),
                new UserModelMessage("hi"),
                new AssistantModelMessage("hello", null, null),
                new SystemModelMessage(
                    string.Empty,
                    Options("{\"anthropic\":{\"toolChanges\":[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"function\",\"name\":\"get_forecast\",\"description\":\"Get forecast\",\"inputSchema\":{\"type\":\"object\"}}},{\"type\":\"tool_removal\",\"toolName\":\"get_weather\"}]}}")),
            },
            convertInlineTool: _ => new AnthropicInlineToolResult((JsonObject)JsonNode.Parse(definitionJson)!));

        var expected = JsonNode.Parse(
            "{\"role\":\"system\",\"content\":[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"tool_definition\",\"definition\":" + definitionJson + "}},"
            + "{\"type\":\"tool_removal\",\"tool\":{\"type\":\"tool_reference\",\"name\":\"get_weather\"}}]}");
        Assert.Contains(result.Messages, message => JsonNode.DeepEquals(expected, message));
        Assert.Contains("inline-tools-2026-09-15", result.Betas);
        Assert.DoesNotContain("mid-conversation-tool-changes-2026-07-01", result.Betas);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when a tool is added by value without an inline tool converter", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public void Should_throw_when_a_tool_is_added_by_value_without_an_inline_tool_converter()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => AnthropicPrompt.Convert(
            new ModelMessage[]
            {
                new UserModelMessage("hi"),
                new SystemModelMessage(
                    string.Empty,
                    Options("{\"anthropic\":{\"toolChanges\":[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"function\",\"name\":\"get_forecast\",\"inputSchema\":{\"type\":\"object\"}}}]}}")),
            }));

        Assert.Matches("tool additions by value", error.Message);
    }
}
