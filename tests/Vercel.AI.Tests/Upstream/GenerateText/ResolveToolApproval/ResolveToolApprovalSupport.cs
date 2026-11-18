// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval;

internal static class ResolveToolApprovalSupport
{
    public const string CitySchema =
        "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}";

    public static readonly IReadOnlyList<ModelMessage> Messages = new ModelMessage[]
    {
        new UserModelMessage("hello"),
    };

    public static GeneratedToolCall CreateToolCall(string toolName = "weather") =>
        new("call-1", toolName, "{\"city\":\"Berlin\"}");

    public static Tool WeatherTool(
        string name = "weather",
        bool needsApproval = false,
        Func<
            System.Text.Json.JsonElement,
            ToolNeedsApprovalOptions,
            Task<bool>
        >? needsApprovalFunction = null,
        Func<JsonElement, JsonElement>? contextSchema = null
    )
    {
        var tool = Tool.Function(name, null, CitySchema, null);
        tool.ContextSchema = contextSchema;
        tool.NeedsApproval = needsApproval;
        tool.NeedsApprovalFunction = needsApprovalFunction;
        return tool;
    }

    public static Task<ToolApprovalStatus> ResolveAsync(
        IReadOnlyList<Tool>? tools,
        ToolApprovalConfiguration? toolApproval,
        GeneratedToolCall? toolCall = null,
        IReadOnlyList<ModelMessage>? messages = null,
        IReadOnlyDictionary<string, JsonElement>? toolsContext = null,
        IReadOnlyDictionary<string, object?>? runtimeContext = null
    ) =>
        ToolApprovalResolution.ResolveToolApprovalAsync(
            tools,
            toolCall ?? CreateToolCall(),
            toolApproval,
            messages ?? Messages,
            toolsContext ?? new Dictionary<string, JsonElement>(),
            runtimeContext ?? new Dictionary<string, object?>()
        );

    /// <summary>Stands in for <c>z.object({ apiKey: z.string() })</c>: it throws unless <c>apiKey</c> is a string.</summary>
    public static JsonElement ApiKeySchema(JsonElement value)
    {
        if (
            value.ValueKind != JsonValueKind.Object
            || !value.TryGetProperty("apiKey", out var apiKey)
            || apiKey.ValueKind != JsonValueKind.String
        )
        {
            throw new FormatException("Invalid tool context.");
        }

        return value;
    }

    public static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    public static void AssertStatus(ToolApprovalStatus status, string type, string? reason = null)
    {
        Assert.Equal(type, status.Type);
        Assert.Equal(reason, status.Reason);
    }
}
