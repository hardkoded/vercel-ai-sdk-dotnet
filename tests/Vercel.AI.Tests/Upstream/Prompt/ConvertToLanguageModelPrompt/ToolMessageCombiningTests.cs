// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt.PromptTestSupport;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelPrompt &gt; tool message</c>.</summary>
public sealed class ToolMessageCombiningTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelPrompt > tool message::";

    private static IReadOnlyDictionary<string, JsonElement> Options(string json) =>
        new Dictionary<string, JsonElement>
        {
            ["test"] = JsonDocument.Parse(json).RootElement.Clone(),
        };

    private static AssistantModelMessage Calls(params string[] ids) =>
        new(
            ids.Select(id =>
                    (AssistantContentPart)
                        new AssistantToolCallPart(new GeneratedToolCall(id, "toolName", "{}"))
                )
                .ToArray()
        );

    [Fact]
    [UpstreamTest(
        Id + "should combine 2 consecutive tool messages into a single tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_combine_2_consecutive_tool_messages_into_a_single_tool_message()
    {
        var result = await ConvertAsync([
            new AssistantModelMessage([
                new AssistantToolCallPart(new GeneratedToolCall("toolCallId", "toolName", "{}")),
                new AssistantToolApprovalRequestPart("approvalId", "toolCallId", null),
            ]),
            new ToolModelMessage([new ToolApprovalResponseModelMessage("approvalId", true)]),
            new ToolModelMessage("toolCallId", "toolName", "{\"some\":\"result\"}", false),
        ]);

        Assert.Equal(2, result.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(result[0]);
        var call = Assert.IsType<AssistantToolCallPart>(Assert.Single(assistant.Parts));
        Assert.Equal("toolCallId", call.ToolCall.ToolCallId);
        Assert.False(call.ToolCall.ProviderExecuted);
        var tool = Assert.IsType<ToolModelMessage>(result[1]);
        var toolResult = Assert.Single(tool.Results);
        Assert.Equal("toolCallId", toolResult.ToolCallId);
        Assert.Equal("toolName", toolResult.ToolName);
        Assert.Equal("json", toolResult.OutputType);
        Assert.Equal("{\"some\":\"result\"}", toolResult.OutputJson);
        Assert.Null(toolResult.ProviderOptions);
        Assert.Null(tool.MessageProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should preserve provider options from an empty tool message when combining consecutive tool messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_provider_options_from_an_empty_tool_message_when_combining_consecutive_tool_messages()
    {
        var result = await ConvertAsync([
            new AssistantModelMessage([
                new AssistantToolCallPart(new GeneratedToolCall("toolCallId", "toolName", "{}")),
                new AssistantToolApprovalRequestPart("approvalId", "toolCallId", null),
            ]),
            new ToolModelMessage([new ToolApprovalResponseModelMessage("approvalId", true)])
            {
                MessageProviderOptions = Options("{\"cacheControl\":\"approval\"}"),
            },
            new ToolModelMessage("toolCallId", "toolName", "result", false),
        ]);

        var tool = Assert.IsType<ToolModelMessage>(result[^1]);
        var toolResult = Assert.Single(tool.Results);
        Assert.Equal("text", toolResult.OutputType);
        Assert.Equal("result", toolResult.OutputJson);
        Assert.Null(toolResult.ProviderOptions);
        Assert.Equal(
            "{\"cacheControl\":\"approval\"}",
            tool.MessageProviderOptions!["test"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should preserve provider options at tool message boundaries when combining consecutive tool messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_provider_options_at_tool_message_boundaries_when_combining_consecutive_tool_messages()
    {
        var result = await ConvertAsync([
            Calls("toolCallId1", "toolCallId2"),
            new ToolModelMessage("toolCallId1", "toolName", "result1", false)
            {
                ProviderOptions = Options("{\"cacheControl\":\"part\",\"partOnly\":true}"),
                MessageProviderOptions = Options(
                    "{\"cacheControl\":\"first-message\",\"messageOnly\":true}"
                ),
            },
            new ToolModelMessage("toolCallId2", "toolName", "result2", false)
            {
                MessageProviderOptions = Options("{\"cacheControl\":\"second-message\"}"),
            },
        ]);

        Assert.Equal(2, result.Count);
        var tool = Assert.IsType<ToolModelMessage>(result[1]);
        Assert.Equal(2, tool.Results.Count);
        var first = tool.Results[0];
        Assert.Equal("toolCallId1", first.ToolCallId);
        Assert.Equal("result1", first.OutputJson);
        // The options of the first message merge into its result: part options win over message options.
        Assert.Equal(
            "{\"cacheControl\":\"part\",\"messageOnly\":true,\"partOnly\":true}",
            Normalize(first.ProviderOptions!["test"])
        );
        var second = tool.Results[1];
        Assert.Equal("toolCallId2", second.ToolCallId);
        Assert.Equal("result2", second.OutputJson);
        Assert.Null(second.ProviderOptions);
        Assert.Equal(
            "{\"cacheControl\":\"second-message\"}",
            tool.MessageProviderOptions!["test"].GetRawText()
        );
    }

    private static string Normalize(JsonElement element)
    {
        var sorted = new SortedDictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            sorted[property.Name] = property.Value;
        }

        return JsonSerializer.Serialize(sorted);
    }
}
