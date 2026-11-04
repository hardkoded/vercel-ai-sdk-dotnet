// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval;

internal static class ResolveToolApprovalSupport
{
    public const string CitySchema = "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}";

    public static readonly IReadOnlyList<ModelMessage> Messages = new ModelMessage[] { new UserModelMessage("hello") };

    public static GeneratedToolCall CreateToolCall(string toolName = "weather") => new("call-1", toolName, "{\"city\":\"Berlin\"}");

    public static Tool WeatherTool(string name = "weather", bool needsApproval = false, Func<System.Text.Json.JsonElement, ToolNeedsApprovalOptions, Task<bool>>? needsApprovalFunction = null)
    {
        var tool = Tool.Function(name, null, CitySchema, null);
        tool.NeedsApproval = needsApproval;
        tool.NeedsApprovalFunction = needsApprovalFunction;
        return tool;
    }

    public static Task<ToolApprovalStatus> ResolveAsync(IReadOnlyList<Tool>? tools, ToolApprovalConfiguration? toolApproval, GeneratedToolCall? toolCall = null, IReadOnlyList<ModelMessage>? messages = null) =>
        ToolApprovalResolution.ResolveToolApprovalAsync(tools, toolCall ?? CreateToolCall(), toolApproval, messages ?? Messages);

    public static void AssertStatus(ToolApprovalStatus status, string type, string? reason = null)
    {
        Assert.Equal(type, status.Type);
        Assert.Equal(reason, status.Reason);
    }
}
