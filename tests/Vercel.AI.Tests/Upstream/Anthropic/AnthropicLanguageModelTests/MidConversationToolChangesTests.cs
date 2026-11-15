// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>mid-conversation tool changes</c>.</summary>
public sealed class MidConversationToolChangesTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::mid-conversation tool changes::";

    private const string StreamBody = "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-opus-4-8\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}}\n\ndata: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":1}}\n\ndata: {\"type\":\"message_stop\"}\n\n";

    private static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    private static SystemModelMessage ToolChange(string toolChangesJson)
    {
        return new SystemModelMessage(string.Empty, Options("{\"anthropic\":{\"toolChanges\":" + toolChangesJson + "}}"));
    }

    private static string ToolUseResponse(string content, string stopReason)
    {
        return "{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-opus-4-8\",\"content\":[" + content + "],\"stop_reason\":\"" + stopReason + "\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";
    }

    [Fact]
    [UpstreamTest(Prefix + "should send tool change blocks and the beta header", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_tool_change_blocks_and_the_beta_header()
    {
        var handler = AnthropicParity.Ok();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Say OK."),
                ToolChange("[{\"type\":\"tool_removal\",\"toolName\":\"get_weather\"}]"),
            },
            Tools = new[]
            {
                new LanguageModelTool("get_weather", "Get weather", AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}")),
            },
        };

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-4-8", options);

        var body = JsonNode.Parse(handler.Body)!;
        var expected = JsonNode.Parse("{\"role\":\"system\",\"content\":[{\"type\":\"tool_removal\",\"tool\":{\"type\":\"tool_reference\",\"name\":\"get_weather\"}}]}");
        Assert.Contains(body["messages"]!.AsArray(), message => JsonNode.DeepEquals(expected, message));
        Assert.Contains("mid-conversation-tool-changes-2026-07-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send tools added by value as tool definitions with the inline tools beta", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public async Task Should_send_tools_added_by_value_as_tool_definitions_with_the_inline_tools_beta()
    {
        var handler = AnthropicParity.Ok();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Say OK."),
                ToolChange(
                    "[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"function\",\"name\":\"get_forecast\",\"description\":\"Get forecast\",\"inputSchema\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}},\"providerOptions\":{\"anthropic\":{\"deferLoading\":true}}}},"
                    + "{\"type\":\"tool_addition\",\"tool\":{\"type\":\"provider\",\"id\":\"anthropic.web_search_20260209\",\"name\":\"search\",\"args\":{\"maxUses\":3}}},"
                    + "{\"type\":\"tool_removal\",\"toolName\":\"get_weather\"}]"),
            },
            Tools = new[]
            {
                new LanguageModelTool("get_weather", "Get weather", AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}")),
            },
        };

        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-4-8", options);

        var body = JsonNode.Parse(handler.Body)!;
        var expected = JsonNode.Parse(
            "{\"content\":[{\"tool\":{\"definition\":{\"defer_loading\":true,\"description\":\"Get forecast\",\"input_schema\":{\"properties\":{\"city\":{\"type\":\"string\"}},\"type\":\"object\"},\"name\":\"get_forecast\"},\"type\":\"tool_definition\"},\"type\":\"tool_addition\"},"
            + "{\"tool\":{\"definition\":{\"max_uses\":3,\"name\":\"web_search\",\"type\":\"web_search_20260209\"},\"type\":\"tool_definition\"},\"type\":\"tool_addition\"},"
            + "{\"tool\":{\"name\":\"get_weather\",\"type\":\"tool_reference\"},\"type\":\"tool_removal\"}],\"role\":\"system\"}");
        Assert.True(JsonNode.DeepEquals(expected, body["messages"]![1]));
        // the definitions live in the message, not in the top-level tools:
        Assert.Equal(new[] { "get_weather" }, body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()).ToArray());

        var betas = handler.Headers["anthropic-beta"];
        Assert.Contains("inline-tools-2026-09-15", betas);
        Assert.DoesNotContain("mid-conversation-tool-changes-2026-07-01", betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send the same definition for streaming and non-streaming requests", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public async Task Should_send_the_same_definition_for_streaming_and_non_streaming_requests()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Say OK."),
            ToolChange("[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"function\",\"name\":\"get_forecast\",\"inputSchema\":{\"type\":\"object\"}}}]"),
        };

        var generateHandler = AnthropicParity.Ok();
        await AnthropicParity.Generate(AnthropicParity.Client(generateHandler), "claude-opus-4-8", new LanguageModelCallOptions { Prompt = prompt });
        var generateBody = JsonNode.Parse(generateHandler.Body)!;

        var streamHandler = AnthropicParity.Ok(StreamBody);
        streamHandler.ContentType = "text/event-stream";
        var stream = await AnthropicParity.Client(streamHandler).LanguageModel("claude-opus-4-8")
            .DoStreamAsync(new LanguageModelCallOptions { Prompt = prompt }, CancellationToken.None);
        await foreach (var part in stream)
        {
            _ = part;
        }

        var streamBody = JsonNode.Parse(streamHandler.Body)!;

        Assert.True(JsonNode.DeepEquals(generateBody["messages"], streamBody["messages"]));
        Assert.DoesNotContain("eager_input_streaming", streamBody["messages"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn and ignore computer tools added by value", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public async Task Should_warn_and_ignore_computer_tools_added_by_value()
    {
        var handler = AnthropicParity.Ok();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Say OK."),
                ToolChange("[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"provider\",\"id\":\"anthropic.computer_20251124\",\"name\":\"computer\",\"args\":{\"displayWidthPx\":1024,\"displayHeightPx\":768}}}]"),
            },
        };

        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-4-8", options);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Single(body["messages"]!.AsArray());
        Assert.DoesNotContain("inline-tools-2026-09-15", handler.Headers.TryGetValue("anthropic-beta", out var betas) ? betas : string.Empty);
        Assert.Contains(result.Warnings, warning => warning.Type == "other" && warning.Message!.Contains("cannot be added by value"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep the response name of a declared computer toolset when an inline computer tool is ignored", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public async Task Should_keep_the_response_name_of_a_declared_computer_toolset_when_an_inline_computer_tool_is_ignored()
    {
        var handler = AnthropicParity.Ok(ToolUseResponse(
            "{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"left_click\",\"input\":{\"coordinate\":[1,2]},\"toolset_name\":\"computer\"}",
            "tool_use"));
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Click."),
                ToolChange("[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"provider\",\"id\":\"anthropic.computer_20251124\",\"name\":\"otherComputer\",\"args\":{\"displayWidthPx\":1024,\"displayHeightPx\":768}}}]"),
            },
            Tools = new[] { LanguageModelTool.ProviderDefined("anthropic.computer_toolset_20260801", "myComputer", AnthropicParity.Json("{}")) },
        };

        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-4-8", options);

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("myComputer", call.ToolName);
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep the response name of a declared provider tool when an inline provider tool has the same provider name", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public async Task Should_keep_the_response_name_of_a_declared_provider_tool_when_an_inline_provider_tool_has_the_same_provider_name()
    {
        var handler = AnthropicParity.Ok(ToolUseResponse(
            "{\"type\":\"server_tool_use\",\"id\":\"srvtoolu_1\",\"name\":\"web_search\",\"input\":{\"query\":\"weather\"}}",
            "pause_turn"));
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Search."),
                ToolChange("[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"provider\",\"id\":\"anthropic.web_search_20260209\",\"name\":\"inlineSearch\",\"args\":{}}}]"),
            },
            Tools = new[] { LanguageModelTool.ProviderDefined("anthropic.web_search_20260209", "declaredSearch", AnthropicParity.Json("{}")) },
        };

        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-4-8", options);

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("declaredSearch", call.ToolName);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send the tool choice and disabled parallel tool use when all tools are added by value", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public async Task Should_send_the_tool_choice_and_disabled_parallel_tool_use_when_all_tools_are_added_by_value()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Say OK."),
            ToolChange("[{\"type\":\"tool_addition\",\"tool\":{\"type\":\"function\",\"name\":\"get_forecast\",\"inputSchema\":{\"type\":\"object\"}}}]"),
        };

        var bodies = new List<JsonNode>();
        foreach (var options in new[]
        {
            new LanguageModelCallOptions { Prompt = prompt, ToolChoice = ToolChoice.None },
            AnthropicParity.WithProvider("{\"disableParallelToolUse\":true}", new LanguageModelCallOptions { Prompt = prompt }),
            new LanguageModelCallOptions { Prompt = prompt, ToolChoice = ToolChoice.Tool("get_forecast") },
        })
        {
            var handler = AnthropicParity.Ok();
            await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-4-8", options);
            bodies.Add(JsonNode.Parse(handler.Body)!);
        }

        // the tools only exist in the message, so there is no top-level list:
        Assert.All(bodies, body => Assert.Null(body["tools"]));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"none\"}"), bodies[0]["tool_choice"]));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"auto\",\"disable_parallel_tool_use\":true}"), bodies[1]["tool_choice"]));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"tool\",\"name\":\"get_forecast\"}"), bodies[2]["tool_choice"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send a tool choice without any tools", Coverage = UpstreamCoverage.Covered,
        Commit = "457dc40d069f65ef489e9b4002d2e6d799ac46d8")]
    public async Task Should_not_send_a_tool_choice_without_any_tools()
    {
        var handler = AnthropicParity.Ok();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Say OK.") },
            ToolChoice = ToolChoice.None,
        };

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-4-8", options);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Null(body["tool_choice"]);
        Assert.Null(body["tools"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send separate effort updates and turn-scoped reminders with their beta headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_separate_effort_updates_and_turn_scoped_reminders_with_their_beta_headers()
    {
        var handler = AnthropicParity.Ok();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Draft an answer."),
                new AssistantModelMessage("Draft.", null, null),
                new SystemModelMessage(string.Empty, Options("{\"anthropic\":{\"effort\":\"xhigh\"}}")),
                new UserModelMessage("Now finalize it."),
                new SystemModelMessage("Verify every claim in this answer.", Options("{\"anthropic\":{\"clearAt\":\"next_user_message\"}}")),
            },
        };

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-fable-5-1", options);

        var body = JsonNode.Parse(handler.Body)!;
        var expected = JsonNode.Parse(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Draft an answer.\"}]},"
            + "{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Draft.\"}]},"
            + "{\"role\":\"system\",\"content\":[],\"output_config\":{\"effort\":\"xhigh\"}},"
            + "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Now finalize it.\"}]},"
            + "{\"role\":\"system\",\"content\":[{\"type\":\"text\",\"text\":\"Verify every claim in this answer.\"}],\"clear_at\":\"next_user_message\"}]");
        Assert.True(JsonNode.DeepEquals(expected, body["messages"]), body["messages"]!.ToJsonString());
        var betas = handler.Headers["anthropic-beta"];
        Assert.Contains("mid-conversation-system-clear-at-2026-08-21", betas);
        Assert.Contains("mid-conversation-output-config-2026-07-01", betas);
    }
}
