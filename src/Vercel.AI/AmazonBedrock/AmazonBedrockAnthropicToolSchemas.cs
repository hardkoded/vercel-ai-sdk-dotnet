// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.AmazonBedrock;

/// <summary>
/// Input schemas of the Anthropic provider tools. Upstream <c>prepareTools</c> reads them off the
/// <c>anthropicTools</c> factories, which the port does not have, so the port keeps them in one table.
/// </summary>
internal static class AmazonBedrockAnthropicToolSchemas
{
    /// <summary>Returns the input schema of <paramref name="toolId"/>, or null for an unknown tool.</summary>
    internal static JsonObject? Find(string toolId)
    {
        switch (toolId)
        {
            case "anthropic.advisor_20260301":
                return Object(new JsonObject());
            case "anthropic.bash_20241022":
            case "anthropic.bash_20250124":
                return Object(
                    new JsonObject { ["command"] = String(), ["restart"] = Boolean() },
                    "command"
                );
            case "anthropic.code_execution_20250522":
                return Object(new JsonObject { ["code"] = String() }, "code");
            case "anthropic.code_execution_20250825":
            case "anthropic.code_execution_20260120":
                return CodeExecutionSchema();
            case "anthropic.computer_20241022":
                return Computer(
                    [
                        "key",
                        "type",
                        "mouse_move",
                        "left_click",
                        "left_click_drag",
                        "right_click",
                        "middle_click",
                        "double_click",
                        "screenshot",
                        "cursor_position",
                    ],
                    false
                );
            case "anthropic.computer_20250124":
                return Computer(ComputerActions(false), false);
            case "anthropic.computer_20251124":
                return Computer(ComputerActions(true), true);
            case "anthropic.memory_20250818":
                return MemorySchema();
            case "anthropic.text_editor_20241022":
            case "anthropic.text_editor_20250124":
                return TextEditor("view", "create", "str_replace", "insert", "undo_edit");
            case "anthropic.text_editor_20250429":
            case "anthropic.text_editor_20250728":
                return TextEditor("view", "create", "str_replace", "insert");
            case "anthropic.tool_search_bm25_20251119":
                return Object(
                    new JsonObject { ["query"] = String(), ["limit"] = Number() },
                    "query"
                );
            case "anthropic.tool_search_regex_20251119":
                return Object(
                    new JsonObject { ["pattern"] = String(), ["limit"] = Number() },
                    "pattern"
                );
            case "anthropic.web_fetch_20250910":
            case "anthropic.web_fetch_20260209":
            case "anthropic.web_fetch_20260318":
                return Object(new JsonObject { ["url"] = String() }, "url");
            case "anthropic.web_search_20250305":
            case "anthropic.web_search_20260209":
            case "anthropic.web_search_20260318":
                return Object(new JsonObject { ["query"] = String() }, "query");
            default:
                return null;
        }
    }

    private static string[] ComputerActions(bool zoom)
    {
        var actions = new List<string>
        {
            "key",
            "hold_key",
            "type",
            "cursor_position",
            "mouse_move",
            "left_mouse_down",
            "left_mouse_up",
            "left_click",
            "left_click_drag",
            "right_click",
            "middle_click",
            "double_click",
            "triple_click",
            "scroll",
            "wait",
            "screenshot",
        };
        if (zoom)
        {
            actions.Add("zoom");
        }

        return actions.ToArray();
    }

    private static JsonObject Computer(string[] actions, bool region)
    {
        var properties = new JsonObject { ["action"] = Enum(actions), ["coordinate"] = Tuple(2) };
        if (actions.Length > 10)
        {
            properties["duration"] = Number();
            if (region)
            {
                properties["region"] = Tuple(4);
            }

            properties["scroll_amount"] = Number();
            properties["scroll_direction"] = Enum(["up", "down", "left", "right"]);
            properties["start_coordinate"] = Tuple(2);
        }

        properties["text"] = String();
        return Object(properties, "action");
    }

    private static JsonObject TextEditor(params string[] commands) =>
        Object(
            new JsonObject
            {
                ["command"] = Enum(commands),
                ["path"] = String(),
                ["file_text"] = String(),
                ["insert_line"] = new JsonObject { ["type"] = "integer" },
                ["new_str"] = String(),
                ["insert_text"] = String(),
                ["old_str"] = String(),
                ["view_range"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["type"] = "integer" },
                },
            },
            "command",
            "path"
        );

    private static JsonObject CodeExecutionSchema() =>
        new()
        {
            ["anyOf"] = new JsonArray(
                Object(
                    new JsonObject
                    {
                        ["type"] = Const("programmatic-tool-call"),
                        ["code"] = String(),
                    },
                    "type",
                    "code"
                ),
                Object(
                    new JsonObject
                    {
                        ["type"] = Const("bash_code_execution"),
                        ["command"] = String(),
                    },
                    "type",
                    "command"
                ),
                Object(
                    new JsonObject
                    {
                        ["type"] = Const("text_editor_code_execution"),
                        ["command"] = Const("view"),
                        ["path"] = String(),
                    },
                    "type",
                    "command",
                    "path"
                ),
                Object(
                    new JsonObject
                    {
                        ["type"] = Const("text_editor_code_execution"),
                        ["command"] = Const("create"),
                        ["path"] = String(),
                        ["file_text"] = String(),
                    },
                    "type",
                    "command",
                    "path"
                ),
                Object(
                    new JsonObject
                    {
                        ["type"] = Const("text_editor_code_execution"),
                        ["command"] = Const("str_replace"),
                        ["path"] = String(),
                        ["old_str"] = String(),
                        ["new_str"] = String(),
                    },
                    "type",
                    "command",
                    "path",
                    "old_str",
                    "new_str"
                )
            ),
        };

    private static JsonObject MemorySchema()
    {
        static JsonObject Command(JsonObject properties, params string[] required) =>
            Object(properties, required);

        return new JsonObject
        {
            ["anyOf"] = new JsonArray(
                Command(
                    new JsonObject
                    {
                        ["command"] = Const("view"),
                        ["path"] = String(),
                        ["view_range"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["prefixItems"] = new JsonArray(Number(), Number()),
                            ["minItems"] = 2,
                            ["maxItems"] = 2,
                        },
                    },
                    "command",
                    "path"
                ),
                Command(
                    new JsonObject
                    {
                        ["command"] = Const("create"),
                        ["path"] = String(),
                        ["file_text"] = String(),
                    },
                    "command",
                    "path",
                    "file_text"
                ),
                Command(
                    new JsonObject
                    {
                        ["command"] = Const("str_replace"),
                        ["path"] = String(),
                        ["old_str"] = String(),
                        ["new_str"] = String(),
                    },
                    "command",
                    "path",
                    "old_str",
                    "new_str"
                ),
                Command(
                    new JsonObject
                    {
                        ["command"] = Const("insert"),
                        ["path"] = String(),
                        ["insert_line"] = Number(),
                        ["insert_text"] = String(),
                    },
                    "command",
                    "path",
                    "insert_line",
                    "insert_text"
                ),
                Command(
                    new JsonObject { ["command"] = Const("delete"), ["path"] = String() },
                    "command",
                    "path"
                ),
                Command(
                    new JsonObject
                    {
                        ["command"] = Const("rename"),
                        ["old_path"] = String(),
                        ["new_path"] = String(),
                    },
                    "command",
                    "old_path",
                    "new_path"
                )
            ),
        };
    }

    private static JsonObject Object(JsonObject properties, params string[] required)
    {
        var requiredArray = new JsonArray();
        foreach (var name in required)
        {
            requiredArray.Add(name);
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = requiredArray,
            ["additionalProperties"] = false,
        };
    }

    private static JsonObject String() => new() { ["type"] = "string" };

    private static JsonObject Number() => new() { ["type"] = "number" };

    private static JsonObject Boolean() => new() { ["type"] = "boolean" };

    private static JsonObject Const(string value) =>
        new() { ["type"] = "string", ["const"] = value };

    private static JsonObject Enum(string[] values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return new JsonObject { ["type"] = "string", ["enum"] = array };
    }

    private static JsonObject Tuple(int length) =>
        new()
        {
            ["type"] = "array",
            ["items"] = new JsonObject { ["type"] = "integer" },
            ["minItems"] = length,
            ["maxItems"] = length,
        };
}
