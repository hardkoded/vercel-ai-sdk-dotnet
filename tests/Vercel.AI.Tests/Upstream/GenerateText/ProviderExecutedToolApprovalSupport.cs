// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText;

internal static class ProviderExecutedToolApprovalSupport
{
    public static Tool McpTool()
    {
        using var args = JsonDocument.Parse("{}");
        return Tool.ProviderDefined("test.mcp_tool", "mcp_tool", args.RootElement.Clone());
    }

    public static GenerateTextOptions Options(ILanguageModel model, IReadOnlyList<ModelMessage>? messages)
    {
        return new GenerateTextOptions
        {
            Model = model,
            Prompt = messages == null ? "test-input" : null,
            Messages = messages,
            Tools = new[] { McpTool() },
            StopWhen = StopWhen.IsStepCount(3),
        };
    }

    public static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    public static IReadOnlyList<ModelMessage> History(ToolApprovalResponseModelMessage response)
    {
        return new ModelMessage[]
        {
            new UserModelMessage("Shorten this URL: https://example.com"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall("mcp-call-1", "mcp_tool", "{ \"query\": \"test\" }") { ProviderExecuted = true }),
                new AssistantToolApprovalRequestPart("mcp-approval-1", "mcp-call-1"),
            }),
            response,
        };
    }

    // Asserts the prompt of the first model call: the user message, the assistant tool call without its approval request, then the tool message.
    public static ToolApprovalResponseModelMessage AssertPrompt(IReadOnlyList<ModelMessage> prompt)
    {
        Assert.Equal(3, prompt.Count);
        var user = Assert.IsType<UserModelMessage>(prompt[0]);
        Assert.Equal("Shorten this URL: https://example.com", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
        var assistant = Assert.IsType<AssistantModelMessage>(prompt[1]);
        var call = Assert.IsType<AssistantToolCallPart>(Assert.Single(assistant.Parts));
        Assert.True(call.ToolCall.ProviderExecuted);
        Assert.Equal("mcp-call-1", call.ToolCall.ToolCallId);
        Assert.Equal("mcp_tool", call.ToolCall.ToolName);
        using (var input = JsonDocument.Parse(call.ToolCall.ArgumentsJson))
        {
            Assert.Equal("test", input.RootElement.GetProperty("query").GetString());
        }

        Assert.Null(call.ProviderOptions);
        return Assert.IsType<ToolApprovalResponseModelMessage>(prompt[2]);
    }
}
