// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

internal static class ToolExecutionApprovalSupport
{
    public const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";

    public const string PathSchema = "{\"type\":\"object\",\"properties\":{\"path\":{\"type\":\"string\"}},\"required\":[\"path\"],\"additionalProperties\":false}";

    public static AiClient Client() => new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    // Like mockId({ prefix: 'id' }), but the first id is id-1: the approval ids in the upstream snapshots start at id-1.
    public static Func<string> MockId(string prefix)
    {
        var counter = 0;
        return () => prefix + "-" + Interlocked.Increment(ref counter);
    }

    public static Tool ValueTool(Func<JsonElement, CancellationToken, Task<string>>? execute, string name = "tool1", bool needsApproval = false)
    {
        var tool = Tool.Function(name, null, ValueSchema, execute);
        tool.NeedsApproval = needsApproval;
        return tool;
    }

    public static LanguageModelGenerateResult ToolCalls(string toolName, params (string Id, string Input)[] calls) =>
        new(
            calls.Select(call => (GeneratedContent)new GeneratedToolCall(call.Id, toolName, call.Input)).ToArray(),
            FinishReason.ToolCalls,
            new LanguageModelUsage(3, 10, 13));

    public static LanguageModelGenerateResult Text(string text, FinishReason finishReason = FinishReason.Stop) =>
        new(
            new GeneratedContent[] { new GeneratedText(text) },
            finishReason,
            new LanguageModelUsage(3, 10, 13));

    // The history of a conversation in which the model called the tools and asked for approval.
    public static List<ModelMessage> History(string toolName, IEnumerable<(string CallId, string Input, string ApprovalId)> calls, params ModelMessage[] trailing)
    {
        var parts = new List<AssistantContentPart>();
        foreach (var (callId, input, approvalId) in calls)
        {
            parts.Add(new AssistantToolCallPart(new GeneratedToolCall(callId, toolName, input)));
            parts.Add(new AssistantToolApprovalRequestPart(approvalId, callId));
        }

        var messages = new List<ModelMessage> { new UserModelMessage("test-input"), new AssistantModelMessage(parts) };
        messages.AddRange(trailing);
        return messages;
    }

    public static void AssertToolCall(GeneratedContent part, string toolCallId, string toolName, string compactInput)
    {
        var call = Assert.IsType<GeneratedToolCall>(part);
        Assert.Equal("tool-call", call.Type);
        Assert.Equal(toolCallId, call.ToolCallId);
        Assert.Equal(toolName, call.ToolName);
        Assert.Equal(compactInput, Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
    }

    public static void AssertApprovalRequest(GeneratedContent part, string approvalId, string toolCallId, string toolName, string compactInput)
    {
        var request = Assert.IsType<GeneratedToolApprovalRequest>(part);
        Assert.Equal("tool-approval-request", request.Type);
        Assert.Equal(approvalId, request.ApprovalId);
        AssertToolCall(request.ToolCall!, toolCallId, toolName, compactInput);
    }

    public static void AssertToolCallPart(AssistantContentPart part, string toolCallId, string toolName, string compactInput)
    {
        var call = Assert.IsType<AssistantToolCallPart>(part).ToolCall;
        Assert.Equal(toolCallId, call.ToolCallId);
        Assert.Equal(toolName, call.ToolName);
        Assert.Equal(compactInput, Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
    }

    public static void AssertApprovalRequestPart(AssistantContentPart part, string approvalId, string toolCallId, bool? isAutomatic = null)
    {
        var request = Assert.IsType<AssistantToolApprovalRequestPart>(part);
        Assert.Equal("tool-approval-request", request.Type);
        Assert.Equal(approvalId, request.ApprovalId);
        Assert.Equal(toolCallId, request.ToolCallId);
        Assert.Equal(isAutomatic, request.IsAutomatic);
    }

    public static void AssertToolResult(ToolModelMessage message, string toolCallId, string toolName, string outputType, string output)
    {
        Assert.Equal(toolCallId, message.ToolCallId);
        Assert.Equal(toolName, message.ToolName);
        Assert.Equal(outputType, message.OutputType);
        Assert.Equal(output, message.OutputJson);
    }

    public static void AssertText(ModelMessage message, string text)
    {
        var assistant = Assert.IsType<AssistantModelMessage>(message);
        var part = Assert.IsType<AssistantTextPart>(Assert.Single(assistant.Parts));
        Assert.Equal(text, part.Text);
    }

    public static string Compact(string json)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement);
    }

    public static string PromptText(IReadOnlyList<ModelMessage> prompt) =>
        Assert.IsType<TextContentPart>(Assert.Single(Assert.IsType<UserModelMessage>(prompt[0]).Content)).Text;
}
