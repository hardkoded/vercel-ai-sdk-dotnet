// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.OpenAI;

/// <summary>One tool call that an internal <c>parallel</c> wrapper expanded into.</summary>
internal sealed record OpenAIExpandedToolCall(string ToolCallId, string ToolName, string Input, JsonElement ProviderMetadata);

/// <summary>
/// Preserves the original wrapper identity so child results can be sent back as one function output when
/// Responses API server-side state is used. <paramref name="Input"/> is retained only on the first child; older histories may repeat it.
/// </summary>
internal sealed record OpenAIParallelToolCallMetadata(string ItemId, string ToolCallId, string ToolName, string? Input, int Index, int Count);

/// <summary>
/// Expands the internal <c>parallel</c> tool wrapper that OpenAI models can emit as a regular function call.
/// The wrapper is only recognized when every nested recipient is a declared client-side function tool.
/// </summary>
internal static class OpenAIParallelToolCall
{
    private const string ParallelToolName = "parallel";
    private const string RecipientNamePrefix = "functions.";

    /// <summary>The wrapper identity stored under <c>parallelToolCall</c> in the provider options, or null when it is missing or malformed.</summary>
    public static OpenAIParallelToolCallMetadata? GetParallelToolCallMetadata(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName)
    {
        if (providerOptions is null
            || !providerOptions.TryGetValue(providerOptionsName, out var named)
            || named.ValueKind != JsonValueKind.Object
            || !named.TryGetProperty("parallelToolCall", out var metadata)
            || metadata.ValueKind != JsonValueKind.Object
            || !TryString(metadata, "itemId", out var itemId)
            || !TryString(metadata, "toolCallId", out var toolCallId)
            || !TryString(metadata, "toolName", out var toolName)
            || !TryInteger(metadata, "index", out var index)
            || !TryInteger(metadata, "count", out var count)
            || index < 0
            || count <= index)
        {
            return null;
        }

        string? input = null;
        if (metadata.TryGetProperty("input", out var inputValue))
        {
            if (inputValue.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            input = inputValue.GetString();
        }

        return new OpenAIParallelToolCallMetadata(itemId!, toolCallId!, toolName!, input, index, count);
    }

    /// <summary>Whether two metadata values describe the same wrapper call. A missing input matches any input.</summary>
    public static bool HasSameParallelToolCall(OpenAIParallelToolCallMetadata first, OpenAIParallelToolCallMetadata second) =>
        first.ItemId == second.ItemId
        && first.ToolCallId == second.ToolCallId
        && first.ToolName == second.ToolName
        && (first.Input is null || second.Input is null || first.Input == second.Input)
        && first.Count == second.Count;

    private static bool TryString(JsonElement parent, string name, out string? value)
    {
        value = null;
        if (!parent.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return true;
    }

    private static bool TryInteger(JsonElement parent, string name, out int value)
    {
        value = 0;
        if (!parent.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        if (property.TryGetInt32(out value))
        {
            return true;
        }

        return property.TryGetDouble(out var number) && number == Math.Floor(number) && number is >= 0 and <= int.MaxValue
            && (value = (int)number) >= 0;
    }

    public static bool IsUndeclared(string toolName, IReadOnlyCollection<string> functionToolNames) =>
        toolName == ParallelToolName && !functionToolNames.Contains(ParallelToolName);

    public static IReadOnlyCollection<string> FunctionToolNames(IReadOnlyList<Vercel.AI.Provider.LanguageModelTool>? tools) =>
        tools?.Where(tool => tool.ProviderToolId == null).Select(tool => tool.Name).ToList() ?? new List<string>();

    public static List<OpenAIExpandedToolCall>? Expand(
        string toolCallId,
        string toolName,
        string input,
        IReadOnlyCollection<string> functionToolNames,
        string itemId,
        string providerName = "openai")
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

            var wrapper = new JsonObject
            {
                ["itemId"] = itemId,
                ["toolCallId"] = toolCallId,
                ["toolName"] = toolName,
            };

            // Retained only on the first child; siblings keep the wrapper identity, index and count.
            if (index == 0)
            {
                wrapper["input"] = input;
            }

            wrapper["index"] = index;
            wrapper["count"] = toolUses.Count;
            var metadata = OpenAIJson.ProviderMetadata(new JsonObject { ["parallelToolCall"] = wrapper }, providerName);
            expanded.Add(new OpenAIExpandedToolCall(
                toolCallId + "_" + index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                name,
                parameters.ToJsonString(),
                metadata));
        }

        return expanded;
    }

    /// <summary>Provider metadata of a plain function call: item id, plus async mode, namespace and caller when present. The async mode falls back to the one seen when the item was added.</summary>
    public static JsonElement? CallMetadata(JsonElement item, bool? ongoingAsync = null, string providerName = "openai")
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
        else if (ongoingAsync is { } fallbackAsync)
        {
            values["async"] = fallbackAsync;
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

        return values.Count > 0 ? OpenAIJson.ProviderMetadata(values, providerName) : null;
    }
}
