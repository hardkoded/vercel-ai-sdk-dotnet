// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

public static partial class AnthropicTools
{
    private const string Bash =
        "{\"type\":\"object\",\"properties\":{\"command\":{\"type\":\"string\"},\"restart\":{\"type\":\"boolean\"}},\"required\":[\"command\"],\"additionalProperties\":false}";

    private const string TextEditor =
        "{\"type\":\"object\",\"properties\":{\"command\":{\"type\":\"string\",\"enum\":[%COMMANDS%]},\"path\":{\"type\":\"string\"},\"file_text\":{\"type\":\"string\"},\"insert_line\":{\"type\":\"integer\"},\"new_str\":{\"type\":\"string\"},\"insert_text\":{\"type\":\"string\"},\"old_str\":{\"type\":\"string\"},\"view_range\":{\"type\":\"array\",\"items\":{\"type\":\"integer\"}}},\"required\":[\"command\",\"path\"],\"additionalProperties\":false}";

    private const string Query =
        "{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\"}},\"required\":[\"query\"],\"additionalProperties\":false}";

    private const string Url =
        "{\"type\":\"object\",\"properties\":{\"url\":{\"type\":\"string\"}},\"required\":[\"url\"],\"additionalProperties\":false}";

    private const string CodeExecutionCode =
        "{\"type\":\"object\",\"properties\":{\"code\":{\"type\":\"string\"}},\"required\":[\"code\"],\"additionalProperties\":false}";

    private const string CodeExecutionUnion =
        "{\"anyOf\":["
        + "{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"programmatic-tool-call\"},\"code\":{\"type\":\"string\"}},\"required\":[\"type\",\"code\"],\"additionalProperties\":false},"
        + "{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"bash_code_execution\"},\"command\":{\"type\":\"string\"}},\"required\":[\"type\",\"command\"],\"additionalProperties\":false},"
        + "{\"anyOf\":["
        + "{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"text_editor_code_execution\"},\"command\":{\"type\":\"string\",\"const\":\"view\"},\"path\":{\"type\":\"string\"}},\"required\":[\"type\",\"command\",\"path\"],\"additionalProperties\":false},"
        + "{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"text_editor_code_execution\"},\"command\":{\"type\":\"string\",\"const\":\"create\"},\"path\":{\"type\":\"string\"},\"file_text\":{\"type\":[\"string\",\"null\"]}},\"required\":[\"type\",\"command\",\"path\"],\"additionalProperties\":false},"
        + "{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"text_editor_code_execution\"},\"command\":{\"type\":\"string\",\"const\":\"str_replace\"},\"path\":{\"type\":\"string\"},\"old_str\":{\"type\":\"string\"},\"new_str\":{\"type\":\"string\"}},\"required\":[\"type\",\"command\",\"path\",\"old_str\",\"new_str\"],\"additionalProperties\":false}"
        + "]}]}";

    private const string Memory =
        "{\"anyOf\":["
        + "{\"type\":\"object\",\"properties\":{\"command\":{\"type\":\"string\",\"const\":\"view\"},\"path\":{\"type\":\"string\"},\"view_range\":{\"type\":\"array\",\"items\":[{\"type\":\"number\"},{\"type\":\"number\"}],\"minItems\":2,\"maxItems\":2}},\"required\":[\"command\",\"path\"],\"additionalProperties\":false},"
        + "{\"type\":\"object\",\"properties\":{\"command\":{\"type\":\"string\",\"const\":\"create\"},\"path\":{\"type\":\"string\"},\"file_text\":{\"type\":\"string\"}},\"required\":[\"command\",\"path\",\"file_text\"],\"additionalProperties\":false},"
        + "{\"type\":\"object\",\"properties\":{\"command\":{\"type\":\"string\",\"const\":\"str_replace\"},\"path\":{\"type\":\"string\"},\"old_str\":{\"type\":\"string\"},\"new_str\":{\"type\":\"string\"}},\"required\":[\"command\",\"path\",\"old_str\",\"new_str\"],\"additionalProperties\":false},"
        + "{\"type\":\"object\",\"properties\":{\"command\":{\"type\":\"string\",\"const\":\"insert\"},\"path\":{\"type\":\"string\"},\"insert_line\":{\"type\":\"number\"},\"insert_text\":{\"type\":\"string\"}},\"required\":[\"command\",\"path\",\"insert_line\",\"insert_text\"],\"additionalProperties\":false},"
        + "{\"type\":\"object\",\"properties\":{\"command\":{\"type\":\"string\",\"const\":\"delete\"},\"path\":{\"type\":\"string\"}},\"required\":[\"command\",\"path\"],\"additionalProperties\":false},"
        + "{\"type\":\"object\",\"properties\":{\"command\":{\"type\":\"string\",\"const\":\"rename\"},\"old_path\":{\"type\":\"string\"},\"new_path\":{\"type\":\"string\"}},\"required\":[\"command\",\"old_path\",\"new_path\"],\"additionalProperties\":false}"
        + "]}";

    private const string ToolSearchBm25 =
        "{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\"},\"limit\":{\"type\":\"number\"}},\"required\":[\"query\"],\"additionalProperties\":false}";

    private const string ToolSearchRegex =
        "{\"type\":\"object\",\"properties\":{\"pattern\":{\"type\":\"string\"},\"limit\":{\"type\":\"number\"}},\"required\":[\"pattern\"],\"additionalProperties\":false}";

    private const string Advisor =
        "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}";

    private const string Int = "{\"type\":\"integer\"}";

    private const string WebFetchOutput =
        "{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"web_fetch_result\"},\"url\":{\"type\":\"string\"},"
        + "\"content\":{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"document\"},\"title\":{\"type\":[\"string\",\"null\"]},"
        + "\"citations\":{\"type\":\"object\",\"properties\":{\"enabled\":{\"type\":\"boolean\"}},\"required\":[\"enabled\"]},"
        + "\"source\":{\"anyOf\":["
        + "{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"base64\"},\"mediaType\":{\"type\":\"string\",\"const\":\"application/pdf\"},\"data\":{\"type\":\"string\"}},\"required\":[\"type\",\"mediaType\",\"data\"]},"
        + "{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"text\"},\"mediaType\":{\"type\":\"string\",\"const\":\"text/plain\"},\"data\":{\"type\":\"string\"}},\"required\":[\"type\",\"mediaType\",\"data\"]}"
        + "]}},\"required\":[\"type\",\"title\",\"source\"]},"
        + "\"retrievedAt\":{\"type\":[\"string\",\"null\"]}},\"required\":[\"type\",\"url\",\"content\",\"retrievedAt\"]}";

    private const string WebSearchOutput =
        "{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"url\":{\"type\":\"string\"},\"title\":{\"type\":[\"string\",\"null\"]},"
        + "\"pageAge\":{\"type\":[\"string\",\"null\"]},\"encryptedContent\":{\"type\":\"string\"},\"type\":{\"type\":\"string\",\"const\":\"web_search_result\"}},"
        + "\"required\":[\"url\",\"title\",\"pageAge\",\"encryptedContent\",\"type\"]}}";

    /// <summary>JSON Schema of the output of <c>web_fetch_20250910</c>. Maps to <c>webFetch_20250910OutputSchema</c>.</summary>
    public static JsonElement WebFetch20250910OutputSchema() => Parse(WebFetchOutput);

    /// <summary>JSON Schema of the output of <c>web_fetch_20260209</c>. Maps to <c>webFetch_20260209OutputSchema</c>.</summary>
    public static JsonElement WebFetch20260209OutputSchema() => Parse(WebFetchOutput);

    /// <summary>JSON Schema of the output of <c>web_fetch_20260318</c>. Maps to <c>webFetch_20260318OutputSchema</c>.</summary>
    public static JsonElement WebFetch20260318OutputSchema() => Parse(WebFetchOutput);

    /// <summary>JSON Schema of the output of <c>web_search_20250305</c>. Maps to <c>webSearch_20250305OutputSchema</c>.</summary>
    public static JsonElement WebSearch20250305OutputSchema() => Parse(WebSearchOutput);

    /// <summary>JSON Schema of the output of <c>web_search_20260209</c>. Maps to <c>webSearch_20260209OutputSchema</c>.</summary>
    public static JsonElement WebSearch20260209OutputSchema() => Parse(WebSearchOutput);

    /// <summary>JSON Schema of the output of <c>web_search_20260318</c>. Maps to <c>webSearch_20260318OutputSchema</c>.</summary>
    public static JsonElement WebSearch20260318OutputSchema() => Parse(WebSearchOutput);

    /// <summary>
    /// Factory of each Anthropic provider tool, keyed by tool id. Calling a factory returns the tool with its default
    /// arguments, and its <see cref="Tool.InputSchema"/>. Maps to <c>anthropicTools</c>.
    /// </summary>
    public static IReadOnlyDictionary<string, Func<Tool>> Factories { get; } =
        AnthropicProvider.Tools.ToDictionary(
            id => id,
            id =>
                id == "anthropic.computer_toolset_20260801"
                    ? (Func<Tool>)(() => ComputerToolset20260801())
                    : () => Factory(id),
            StringComparer.Ordinal
        );

    private static Tool Factory(string id)
    {
        var json = id switch
        {
            "anthropic.advisor_20260301" => Advisor,
            "anthropic.bash_20241022" or "anthropic.bash_20250124" => Bash,
            "anthropic.code_execution_20250522" => CodeExecutionCode,
            "anthropic.code_execution_20250825" or "anthropic.code_execution_20260120" =>
                CodeExecutionUnion,
            "anthropic.computer_20241022" => Computer(
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
            ),
            "anthropic.computer_20250124" => Computer(
                [
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
                ],
                true
            ),
            "anthropic.computer_20251124" => Computer(
                [
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
                    "zoom",
                ],
                true,
                true
            ),
            "anthropic.memory_20250818" => Memory,
            "anthropic.text_editor_20241022" or "anthropic.text_editor_20250124" =>
                TextEditor.Replace(
                    "%COMMANDS%",
                    "\"view\",\"create\",\"str_replace\",\"insert\",\"undo_edit\""
                ),
            "anthropic.text_editor_20250429" or "anthropic.text_editor_20250728" =>
                TextEditor.Replace("%COMMANDS%", "\"view\",\"create\",\"str_replace\",\"insert\""),
            "anthropic.tool_search_bm25_20251119" => ToolSearchBm25,
            "anthropic.tool_search_regex_20251119" => ToolSearchRegex,
            "anthropic.web_fetch_20250910"
            or "anthropic.web_fetch_20260209"
            or "anthropic.web_fetch_20260318" => Url,
            "anthropic.web_search_20250305"
            or "anthropic.web_search_20260209"
            or "anthropic.web_search_20260318" => Query,
            _ => throw new ArgumentException("Unknown Anthropic tool: " + id, nameof(id)),
        };
        using var schema = JsonDocument.Parse(json);
        var tool = Tool.ProviderDefined(
            id,
            id.Substring("anthropic.".Length),
            JsonSerializer.SerializeToElement(new JsonObject()),
            schema.RootElement
        );
        tool.OutputSchema = id switch
        {
            "anthropic.web_fetch_20250910" => WebFetch20250910OutputSchema(),
            "anthropic.web_fetch_20260209" => WebFetch20260209OutputSchema(),
            "anthropic.web_fetch_20260318" => WebFetch20260318OutputSchema(),
            "anthropic.web_search_20250305" => WebSearch20250305OutputSchema(),
            "anthropic.web_search_20260209" => WebSearch20260209OutputSchema(),
            "anthropic.web_search_20260318" => WebSearch20260318OutputSchema(),
            _ => null,
        };
        return tool;
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    // The computer tools share one object shape. Newer versions add a tuple coordinate, duration,
    // scroll and zoom fields.
    private static string Computer(
        string[] actions,
        bool tuple,
        bool region = false,
        bool repeat = false
    )
    {
        var coordinate = tuple
            ? "{\"type\":\"array\",\"items\":["
                + Int
                + ","
                + Int
                + "],\"minItems\":2,\"maxItems\":2}"
            : "{\"type\":\"array\",\"items\":" + Int + "}";
        var properties =
            "\"action\":{\"type\":\"string\",\"enum\":["
            + string.Join(",", actions.Select(action => "\"" + action + "\""))
            + "]},\"coordinate\":"
            + coordinate;
        if (tuple)
        {
            properties += ",\"duration\":{\"type\":\"number\"}";
        }

        if (region)
        {
            properties +=
                ",\"region\":{\"type\":\"array\",\"items\":["
                + string.Join(",", Int, Int, Int, Int)
                + "],\"minItems\":4,\"maxItems\":4}";
        }

        if (repeat)
        {
            properties += ",\"repeat\":" + Int;
        }

        if (tuple)
        {
            properties +=
                ",\"scroll_amount\":{\"type\":\"number\"},\"scroll_direction\":{\"type\":\"string\",\"enum\":[\"up\",\"down\",\"left\",\"right\"]},\"start_coordinate\":"
                + coordinate;
        }

        properties += ",\"text\":{\"type\":\"string\"}";
        return "{\"type\":\"object\",\"properties\":{"
            + properties
            + "},\"required\":[\"action\"],\"additionalProperties\":false}";
    }
}
