// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;

namespace Vercel.AI.Anthropic;

/// <summary>Guards and input mapping for computer toolset member calls.</summary>
internal static class AnthropicToolset
{
    internal const string ToolsetType = "computer_toolset_20260801";

    /// <summary>Member tools of the computer toolset.</summary>
    internal static readonly string[] Members =
    {
        "screenshot",
        "zoom",
        "left_click",
        "right_click",
        "middle_click",
        "double_click",
        "triple_click",
        "left_click_drag",
        "mouse_move",
        "left_mouse_down",
        "left_mouse_up",
        "cursor_position",
        "scroll",
        "type",
        "key",
        "hold_key",
        "wait",
    };

    /// <summary>
    /// Rejects a member the prepared tools do not allow: an unknown toolset or member, an undeclared toolset,
    /// or a member the application disabled.
    /// </summary>
    internal static void Validate(string toolsetName, string memberName, JsonArray? preparedTools)
    {
        JsonObject? toolset = null;
        if (preparedTools != null)
        {
            foreach (var tool in preparedTools)
            {
                if (tool is JsonObject candidate && (string?)candidate["type"] == ToolsetType)
                {
                    toolset = candidate;
                    break;
                }
            }
        }

        if (toolsetName != "computer"
            || toolset == null
            || Array.IndexOf(Members, memberName) < 0
            || IsDisabled(toolset, memberName))
        {
            throw new InvalidResponseDataException(
                new JsonObject { ["toolsetName"] = toolsetName, ["memberName"] = memberName }.ToJsonString(),
                "Received an unavailable toolset member: " + toolsetName + "." + memberName + ".");
        }
    }

    /// <summary>Copies the member input, then sets the provider-selected member as the authoritative <c>action</c>.</summary>
    internal static string MemberInput(string memberName, JsonNode? input)
    {
        if (input is not JsonObject source)
        {
            throw new InvalidResponseDataException(
                input?.ToJsonString() ?? "null",
                "Invalid toolset member input: expected an object.");
        }

        var wrapped = new JsonObject();
        foreach (var property in source)
        {
            wrapped[property.Key] = property.Value?.DeepClone();
        }

        wrapped["action"] = memberName;
        return wrapped.ToJsonString();
    }

    /// <summary>Parses streamed member input. Empty input is an empty object. Malformed JSON is rejected.</summary>
    internal static JsonNode? ParseStreamedInput(string input)
    {
        if (input.Length == 0)
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(input);
        }
        catch (JsonException)
        {
            throw new InvalidResponseDataException(input, "Invalid toolset member input JSON.");
        }
    }

    private static bool IsDisabled(JsonObject toolset, string memberName)
    {
        return toolset["configs"] is JsonObject configs
            && configs[memberName] is JsonObject config
            && config["enabled"] is JsonValue enabled
            && enabled.TryGetValue<bool>(out var value)
            && !value;
    }
}
