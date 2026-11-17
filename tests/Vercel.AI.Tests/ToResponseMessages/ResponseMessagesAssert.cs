// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Responses = Vercel.AI.GenerateText.ToResponseMessages;

namespace Vercel.AI.Tests.ToResponseMessages;

/// <summary>Builds the inputs of <c>toResponseMessages</c> and reads its messages.</summary>
internal static class ResponseMessagesAssert
{
    internal static readonly byte[] PngBytes = [137, 80, 78, 71, 13, 10, 26, 10];

    internal static List<ModelMessage> ToResponse(
        IReadOnlyList<GeneratedContent> content,
        IReadOnlyList<Tool>? tools = null
    ) => Responses.Convert(content, tools);

    internal static Tool TestTool(string name = "testTool", string description = "A test tool") =>
        new(name, description, Json("{\"type\":\"object\",\"properties\":{}}"), null);

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    internal static GeneratedToolResult ClientResult(
        string toolCallId,
        string toolName,
        string resultJson,
        bool isError = false
    ) => new(toolCallId, toolName, resultJson, isError) { ProviderExecuted = false };

    internal static AssistantModelMessage Assistant(IReadOnlyList<ModelMessage> messages, int index)
    {
        var assistant = Assert.IsType<AssistantModelMessage>(messages[index]);
        Assert.Equal("assistant", assistant.Role);
        return assistant;
    }

    internal static AssistantContentPart[] Parts(AssistantModelMessage message) =>
        [.. message.Parts];

    internal static T Part<T>(AssistantModelMessage message, int index)
        where T : AssistantContentPart => Assert.IsType<T>(message.Parts[index]);

    internal static AssistantTextPart Text(
        AssistantModelMessage message,
        int index,
        string text,
        string? providerOptionsJson = null
    )
    {
        var part = Part<AssistantTextPart>(message, index);
        Assert.Equal(text, part.Text);
        AssertOptions(providerOptionsJson, part.ProviderOptions);
        return part;
    }

    internal static AssistantToolCallPart ToolCall(
        AssistantModelMessage message,
        int index,
        string toolCallId,
        string toolName,
        string inputJson,
        bool providerExecuted = false,
        string? providerOptionsJson = null
    )
    {
        var part = Part<AssistantToolCallPart>(message, index);
        Assert.Equal(toolCallId, part.ToolCall.ToolCallId);
        Assert.Equal(toolName, part.ToolCall.ToolName);
        Assert.Equal(Canonical(inputJson), Canonical(part.ToolCall.ArgumentsJson));
        Assert.Equal(providerExecuted, part.ToolCall.ProviderExecuted);
        AssertOptions(providerOptionsJson, part.ProviderOptions);
        return part;
    }

    internal static AssistantToolApprovalRequestPart ApprovalRequest(
        AssistantModelMessage message,
        int index,
        string approvalId,
        string toolCallId,
        bool? isAutomatic = null
    )
    {
        var part = Part<AssistantToolApprovalRequestPart>(message, index);
        Assert.Equal(approvalId, part.ApprovalId);
        Assert.Equal(toolCallId, part.ToolCallId);
        Assert.Equal(isAutomatic, part.IsAutomatic);
        return part;
    }

    internal static void AssertOptions(
        string? expectedJson,
        IReadOnlyDictionary<string, JsonElement>? options
    )
    {
        if (expectedJson == null)
        {
            Assert.Null(options);
            return;
        }

        Assert.NotNull(options);
        Assert.Equal(Canonical(expectedJson), Canonical(JsonSerializer.Serialize(options)));
    }

    internal static ToolModelMessage ToolMessage(IReadOnlyList<ModelMessage> messages, int index) =>
        Assert.IsType<ToolModelMessage>(messages[index]);

    internal static void AssertToolResult(
        ToolModelMessage result,
        string toolCallId,
        string toolName,
        string outputType,
        string outputJson
    )
    {
        Assert.Equal(toolCallId, result.ToolCallId);
        Assert.Equal(toolName, result.ToolName);
        Assert.Equal(outputType, result.OutputType);
        Assert.Equal(
            outputType is "json" or "error-json" ? Canonical(outputJson) : outputJson,
            outputType is "json" or "error-json" ? Canonical(result.OutputJson) : result.OutputJson
        );
    }

    internal static string Canonical(string json) =>
        JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json));
}
