// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.OpenAI;

/// <summary>One tool call that an internal <c>parallel</c> wrapper expanded into.</summary>
internal sealed record OpenAIExpandedToolCall(string ToolCallId, string ToolName, string Input, JsonElement ProviderMetadata);

/// <summary>
/// Expands the internal <c>parallel</c> tool wrapper that OpenAI models can emit as a regular function call.
/// The wrapper is only recognized when every nested recipient is a declared client-side function tool.
/// </summary>
internal static class OpenAIParallelToolCall
{
    private const string ParallelToolName = "parallel";
    private const string RecipientNamePrefix = "functions.";

    public static bool IsUndeclared(string toolName, IReadOnlyCollection<string> functionToolNames) =>
        toolName == ParallelToolName && !functionToolNames.Contains(ParallelToolName);

    public static IReadOnlyCollection<string> FunctionToolNames(IReadOnlyList<Vercel.AI.Provider.LanguageModelTool>? tools) =>
        tools?.Where(tool => tool.ProviderToolId == null).Select(tool => tool.Name).ToList() ?? new List<string>();

    public static List<OpenAIExpandedToolCall>? Expand(
        string toolCallId,
        string toolName,
        string input,
        IReadOnlyCollection<string> functionToolNames,
        string itemId)
    {
        if (!IsUndeclared(toolName, functionToolNames))
        {
            return null;
        }

        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(input);
        }
        catch (JsonException)
        {
            return null;
        }

        if (parsed is not JsonObject root || root["tool_uses"] is not JsonArray toolUses || toolUses.Count == 0)
        {
            return null;
        }

        var expanded = new List<OpenAIExpandedToolCall>();
        for (var index = 0; index < toolUses.Count; index++)
        {
            if (toolUses[index] is not JsonObject toolUse
                || toolUse["recipient_name"] is not JsonValue recipient
                || !recipient.TryGetValue<string>(out var recipientName)
                || !recipientName.StartsWith(RecipientNamePrefix, StringComparison.Ordinal)
                || toolUse["parameters"] is not JsonObject parameters)
            {
                return null;
            }

            var name = recipientName.Substring(RecipientNamePrefix.Length);
            if (name.Length == 0 || !functionToolNames.Contains(name))
            {
                return null;
            }

            var metadata = OpenAIJson.ProviderMetadata(new JsonObject
            {
                ["parallelToolCall"] = new JsonObject
                {
                    ["itemId"] = itemId,
                    ["toolCallId"] = toolCallId,
                    ["toolName"] = toolName,
                    ["input"] = input,
                    ["index"] = index,
                    ["count"] = toolUses.Count,
                },
            });
            expanded.Add(new OpenAIExpandedToolCall(
                toolCallId + "_" + index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                name,
                parameters.ToJsonString(),
                metadata));
        }

        return expanded;
    }

    /// <summary>Provider metadata of a plain function call: item id, plus async mode and namespace when present.</summary>
    public static JsonElement? CallMetadata(JsonElement item)
    {
        var values = new JsonObject();
        if (OpenAIJson.String(item, "id") is { Length: > 0 } itemId)
        {
            values["itemId"] = itemId;
        }

        if (item.TryGetProperty("async", out var asyncMode) && asyncMode.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            values["async"] = asyncMode.GetBoolean();
        }

        if (OpenAIJson.String(item, "namespace") is { } ns)
        {
            values["namespace"] = ns;
        }

        if (item.TryGetProperty("caller", out var caller) && caller.ValueKind == JsonValueKind.Object)
        {
            values["caller"] = OpenAIJson.String(caller, "type") == "program"
                ? new JsonObject { ["type"] = "program", ["callerId"] = OpenAIJson.String(caller, "caller_id") }
                : JsonNode.Parse(caller.GetRawText());
        }

        return values.Count > 0 ? OpenAIJson.ProviderMetadata(values) : null;
    }
}
