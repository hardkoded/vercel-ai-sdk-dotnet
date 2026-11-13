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

    private const string PrefixEffort = "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages > effort-only system messages::";

    private const string UserJson = "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"hi\"}]}";

    private static JsonNode? Node(string json) => JsonNode.Parse(json);

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(Node(expected), actual), "Expected " + expected + " but was " + (actual?.ToJsonString() ?? "null"));

    private static SystemModelMessage Effort(string effort) =>
        new SystemModelMessage(string.Empty, Options("{\"anthropic\":{\"effort\":\"" + effort + "\"}}"));

    private static string EffortMessage(string effort) =>
        "{\"role\":\"system\",\"content\":[],\"output_config\":{\"effort\":\"" + effort + "\"}}";

    private static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    private const string BetaSystem = "mid-conversation-system-2026-04-07";
    private const string BetaOutputConfig = "mid-conversation-output-config-2026-07-01";
    private const string BetaClearAt = "mid-conversation-system-clear-at-2026-08-21";
    private const string BetaToolChanges = "mid-conversation-tool-changes-2026-07-01";

    private static readonly SystemModelMessage Instruction = new("initial");

    private static ModelMessage[] Hi(params ModelMessage[] before) => [.. before, new UserModelMessage("hi")];

    private static AnthropicPromptResult Run(params ModelMessage[] prompt) => AnthropicPrompt.Convert(prompt);

    private static ModelMessage[] Dialogue(params ModelMessage[] tail) =>
        [new SystemModelMessage("initial"), new UserModelMessage("hi"), new AssistantModelMessage("hello", null, null), .. tail];

    [Fact]
    [UpstreamTest(Prefix + "should convert a single system message into an anthropic system message", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_a_single_system_message_into_an_anthropic_system_message()
    {
        var result = Run(new SystemModelMessage("This is a system message"));

        AssertJson("[{\"type\":\"text\",\"text\":\"This is a system message\"}]", result.System);
        Assert.Empty(result.Messages);
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert multiple system messages into an anthropic system message", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_multiple_system_messages_into_an_anthropic_system_message()
    {
        var result = Run(new SystemModelMessage("This is a system message"), new SystemModelMessage("This is another system message"));

        AssertJson("[{\"type\":\"text\",\"text\":\"This is a system message\"},{\"type\":\"text\",\"text\":\"This is another system message\"}]", result.System);
        Assert.Empty(result.Messages);
        Assert.Empty(result.Betas);
    }

    public static IEnumerable<object[]> InitialEffortCases()
    {
        yield return ["alone", new ModelMessage[] { Effort("low") }, false, new[] { "low" }];
        yield return ["after initial instructions", new ModelMessage[] { Instruction, Effort("low") }, true, new[] { "low" }];
        yield return ["before initial instructions", new ModelMessage[] { Effort("low"), Instruction }, true, new[] { "low" }];
        yield return ["consecutively", new ModelMessage[] { Effort("low"), Effort("high") }, false, new[] { "low", "high" }];
        yield return ["around initial instructions", new ModelMessage[] { Effort("low"), Instruction, Effort("high") }, true, new[] { "low", "high" }];
    }

    [Theory]
    [MemberData(nameof(InitialEffortCases))]
    [UpstreamTest(PrefixEffort + "should preserve initial effort messages $name", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_initial_effort_messages(string name, ModelMessage[] initial, bool hasText, string[] efforts)
    {
        Assert.NotEmpty(name);
        var result = Run([.. initial, new UserModelMessage("hi")]);

        AssertJson(hasText ? "[{\"type\":\"text\",\"text\":\"initial\"}]" : "[]", result.System ?? new JsonArray());
        AssertJson("[" + string.Join(",", efforts.Select(EffortMessage)) + "," + UserJson + "]", result.Messages);
        Assert.Equal([BetaOutputConfig], result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(PrefixEffort + "should preserve later consecutive effort messages with initial instructions: %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_later_consecutive_effort_messages_with_initial_instructions(bool hasInitial)
    {
        var prompt = new List<ModelMessage>();
        if (hasInitial)
        {
            prompt.Add(Instruction);
        }

        prompt.AddRange([new UserModelMessage("hi"), Effort("low"), Effort("high"), new SystemModelMessage("later instructions")]);
        var result = AnthropicPrompt.Convert(prompt);

        if (hasInitial)
        {
            AssertJson("[{\"type\":\"text\",\"text\":\"initial\"}]", result.System);
        }
        else
        {
            Assert.Null(result.System);
        }

        AssertJson(
            "[" + UserJson + "," + EffortMessage("low") + "," + EffortMessage("high")
            + ",{\"role\":\"system\",\"content\":[{\"type\":\"text\",\"text\":\"later instructions\"}]}]",
            result.Messages);
        Assert.Equal(new[] { BetaSystem, BetaOutputConfig }.OrderBy(b => b), result.Betas.OrderBy(b => b));
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("initial", "{\"effort\":\"low\"}")]
    [InlineData("", "{\"clearAt\":\"next_user_message\"}")]
    [InlineData("", "{\"clearAt\":\"next_user_message\",\"effort\":\"low\"}")]
    [UpstreamTest(PrefixEffort + "should warn and ignore unsupported initial system options: $options", Coverage = UpstreamCoverage.Covered)]
    public void Should_warn_and_ignore_unsupported_initial_system_options(string content, string options)
    {
        var result = Run(new SystemModelMessage(content, Options("{\"anthropic\":" + options + "}")), new UserModelMessage("hi"));

        AssertJson("[" + UserJson + "]", result.Messages);
        Assert.Empty(result.Betas);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Contains("These options have been ignored.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should emit a mid-conversation system message inline and add the beta", Coverage = UpstreamCoverage.Covered)]
    public void Should_emit_a_mid_conversation_system_message_inline_and_add_the_beta()
    {
        var result = Run(Dialogue(new SystemModelMessage("switch tone"), new UserModelMessage("go")));

        AssertJson("[{\"type\":\"text\",\"text\":\"initial\"}]", result.System);
        Assert.Contains(result.Messages, message => JsonNode.DeepEquals(
            Node("{\"role\":\"system\",\"content\":[{\"type\":\"text\",\"text\":\"switch tone\"}]}"), message));
        Assert.Contains(BetaSystem, result.Betas);
    }

    [Fact]
    [UpstreamTest(Prefix + "should serialize effort updates and turn-scoped reminders on separate system messages", Coverage = UpstreamCoverage.Covered)]
    public void Should_serialize_effort_updates_and_turn_scoped_reminders_on_separate_system_messages()
    {
        var result = Run(Dialogue(
            Effort("high"),
            new UserModelMessage("go"),
            new SystemModelMessage("this instruction applies to the current turn", Options("{\"anthropic\":{\"clearAt\":\"next_user_message\"}}")),
            new SystemModelMessage("this instruction persists")));

        AssertJson(
            "[" + UserJson + ",{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"hello\"}]}," + EffortMessage("high")
            + ",{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"go\"}]},"
            + "{\"role\":\"system\",\"content\":[{\"type\":\"text\",\"text\":\"this instruction applies to the current turn\"}],\"clear_at\":\"next_user_message\"},"
            + "{\"role\":\"system\",\"content\":[{\"type\":\"text\",\"text\":\"this instruction persists\"}]}]",
            result.Messages);
        Assert.Contains(BetaClearAt, result.Betas);
        Assert.Contains(BetaOutputConfig, result.Betas);
    }

    [Fact]
    [UpstreamTest(Prefix + "should emit tool change blocks on a mid-conversation system message and add the beta", Coverage = UpstreamCoverage.Covered)]
    public void Should_emit_tool_change_blocks_on_a_mid_conversation_system_message_and_add_the_beta()
    {
        var result = Run(Dialogue(
            new SystemModelMessage(
                "tools have changed",
                Options("{\"anthropic\":{\"toolChanges\":[{\"type\":\"tool_addition\",\"toolName\":\"get_forecast\"},{\"type\":\"tool_removal\",\"toolName\":\"get_weather\"}]}}")),
            new UserModelMessage("go")));

        Assert.Contains(result.Messages, message => JsonNode.DeepEquals(
            Node("{\"role\":\"system\",\"content\":[{\"type\":\"text\",\"text\":\"tools have changed\"},"
                + "{\"type\":\"tool_addition\",\"tool\":{\"type\":\"tool_reference\",\"name\":\"get_forecast\"}},"
                + "{\"type\":\"tool_removal\",\"tool\":{\"type\":\"tool_reference\",\"name\":\"get_weather\"}}]}"),
            message));
        Assert.Contains(BetaSystem, result.Betas);
        Assert.Contains(BetaToolChanges, result.Betas);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not emit an empty text block for a system message that only carries tool changes", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_emit_an_empty_text_block_for_a_system_message_that_only_carries_tool_changes()
    {
        var result = Run(Dialogue(
            new SystemModelMessage(string.Empty, Options("{\"anthropic\":{\"toolChanges\":[{\"type\":\"tool_removal\",\"toolName\":\"get_weather\"}]}}"))));

        Assert.Contains(result.Messages, message => JsonNode.DeepEquals(
            Node("{\"role\":\"system\",\"content\":[{\"type\":\"tool_removal\",\"tool\":{\"type\":\"tool_reference\",\"name\":\"get_weather\"}}]}"),
            message));
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

    [Fact]
    [UpstreamTest(Prefix + "should warn and drop tool changes on the initial system message", Coverage = UpstreamCoverage.Covered)]
    public void Should_warn_and_drop_tool_changes_on_the_initial_system_message()
    {
        var result = Run(
            new SystemModelMessage("initial", Options("{\"anthropic\":{\"toolChanges\":[{\"type\":\"tool_addition\",\"toolName\":\"get_forecast\"}]}}")),
            new UserModelMessage("hi"));

        AssertJson("[{\"type\":\"text\",\"text\":\"initial\"}]", result.System);
        Assert.DoesNotContain(BetaToolChanges, result.Betas);
        Assert.Contains(result.Warnings, warning => warning.Type == "other" && warning.Message!.Contains("initial system message"));
    }
}
