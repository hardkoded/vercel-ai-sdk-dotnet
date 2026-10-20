// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.OpenAI;

/// <summary>
/// Structural check of an OpenAI Responses API response body and of a batch result line.
/// Mirrors <c>openaiResponsesResponseSchema</c> and <c>openaiBatchResultLineSchema</c>: required keys, value kinds, enums, and the <c>type</c> discriminators.
/// A rule receives <see langword="null"/> when the key is absent.
/// </summary>
internal static class OpenAIResponsesSchema
{
    private delegate bool Rule(JsonElement? value);

    private static readonly Rule String = value => value is { ValueKind: JsonValueKind.String };

    private static readonly Rule Number = value => value is { ValueKind: JsonValueKind.Number };

    private static readonly Rule Boolean = value => value is { ValueKind: JsonValueKind.True or JsonValueKind.False };

    private static readonly Rule Any = _ => true;

    private static readonly Rule NullableString = Nullable(String);

    private static readonly Rule StringArray = Array(String);

    private static readonly Rule OptionalKeys = Nullish(StringArray);

    private static readonly Rule ToolCaller = Union(
        ("direct", Object()),
        ("program", Object(("caller_id", String))));

    private static readonly Rule ErrorValue = Either(
        String,
        Object(("type", Optional(String)), ("code", Optional(Either(Number, String))), ("message", Optional(String))));

    private static readonly Rule ComputerAction = Union(
        ("click", Object(("button", Enum("left", "right", "wheel", "back", "forward")), ("x", Number), ("y", Number), ("keys", OptionalKeys))),
        ("double_click", Object(("x", Number), ("y", Number), ("keys", OptionalKeys))),
        ("drag", Object(("path", Array(Object(("x", Number), ("y", Number)))), ("keys", OptionalKeys))),
        ("keypress", Object(("keys", StringArray))),
        ("move", Object(("x", Number), ("y", Number), ("keys", OptionalKeys))),
        ("screenshot", Object()),
        ("scroll", Object(("x", Number), ("y", Number), ("scroll_x", Number), ("scroll_y", Number), ("keys", OptionalKeys))),
        ("type", Object(("text", String))),
        ("wait", Object()));

    private static readonly Rule Annotation = Union(
        ("url_citation", Object(("start_index", Number), ("end_index", Number), ("url", String), ("title", String))),
        ("file_citation", Object(("file_id", String), ("filename", String), ("index", Number))),
        ("container_file_citation", Object(("container_id", String), ("file_id", String), ("filename", String), ("start_index", Number), ("end_index", Number))),
        ("file_path", Object(("file_id", String), ("index", Number))));

    private static readonly Rule Logprob = Object(
        ("token", String),
        ("logprob", Number),
        ("top_logprobs", Array(Object(("token", String), ("logprob", Number)))));

    private static readonly Rule OutputItem = Union(
        ("message", Object(
            ("role", Literal("assistant")),
            ("id", String),
            ("phase", Nullish(Enum("commentary", "final_answer"))),
            ("content", Array(Object(
                ("type", Literal("output_text")),
                ("text", String),
                ("logprobs", Nullish(Array(Logprob))),
                ("annotations", Array(Annotation))))))),
        ("web_search_call", Object(
            ("id", String),
            ("status", String),
            ("action", Nullish(Union(
                ("search", Object(
                    ("query", Nullish(String)),
                    ("queries", Nullish(StringArray)),
                    ("sources", Nullish(Array(Union(("url", Object(("url", String))), ("api", Object(("name", String))))))))),
                ("open_page", Object(("url", Nullish(String)))),
                ("find_in_page", Object(("url", Nullish(String)), ("pattern", Nullish(String))))))))),
        ("file_search_call", Object(
            ("id", String),
            ("queries", StringArray),
            ("results", Nullish(Array(Object(
                ("attributes", Record(Either(String, Number, Boolean))),
                ("file_id", String),
                ("filename", String),
                ("score", Number),
                ("text", String))))))),
        ("code_interpreter_call", Object(
            ("id", String),
            ("code", NullableString),
            ("container_id", String),
            ("outputs", Nullable(Array(Union(("logs", Object(("logs", String))), ("image", Object(("url", String))))))))),
        ("image_generation_call", Object(("id", String), ("result", String))),
        ("local_shell_call", Object(
            ("id", String),
            ("call_id", String),
            ("action", Object(
                ("type", Literal("exec")),
                ("command", StringArray),
                ("timeout_ms", Optional(Number)),
                ("user", Optional(String)),
                ("working_directory", Optional(String)),
                ("env", Optional(Record(String))))))),
        ("function_call", Object(
            ("call_id", String),
            ("name", String),
            ("arguments", String),
            ("id", String),
            ("async", Nullish(Boolean)),
            ("namespace", Nullish(String)),
            ("caller", Nullish(ToolCaller)))),
        ("program", Object(("id", String), ("call_id", String), ("code", String), ("fingerprint", String))),
        ("program_output", Object(("id", String), ("call_id", String), ("result", String), ("status", Enum("completed", "incomplete")))),
        ("custom_tool_call", Object(
            ("call_id", String),
            ("name", String),
            ("input", String),
            ("id", String),
            ("async", Nullish(Boolean)))),
        ("computer_call", Object(
            ("id", String),
            ("call_id", Nullish(String)),
            ("status", Enum("in_progress", "completed", "incomplete")),
            ("action", Nullish(ComputerAction)),
            ("actions", Nullish(Array(ComputerAction))),
            ("pending_safety_checks", Nullish(Array(Object(("id", String), ("code", Nullish(String)), ("message", Nullish(String)))))))),
        ("reasoning", Object(
            ("id", String),
            ("encrypted_content", Nullish(String)),
            ("summary", Array(Object(("type", Literal("summary_text")), ("text", String)))))),
        ("mcp_call", Object(
            ("id", String),
            ("status", String),
            ("arguments", String),
            ("name", String),
            ("server_label", String),
            ("output", Nullish(String)),
            ("error", Nullish(ErrorValue)),
            ("approval_request_id", Nullish(String)))),
        ("mcp_list_tools", Object(
            ("id", String),
            ("server_label", String),
            ("tools", Array(Object(
                ("name", String),
                ("description", Optional(String)),
                ("input_schema", Any),
                ("annotations", Optional(Record(Any)))))),
            ("error", Optional(ErrorValue)))),
        ("mcp_approval_request", Object(
            ("id", String),
            ("server_label", String),
            ("name", String),
            ("arguments", String),
            ("approval_request_id", Optional(String)))),
        ("apply_patch_call", Object(
            ("id", String),
            ("call_id", String),
            ("status", Enum("in_progress", "completed")),
            ("operation", Union(
                ("create_file", Object(("path", String), ("diff", String))),
                ("delete_file", Object(("path", String))),
                ("update_file", Object(("path", String), ("diff", String))))))),
        ("shell_call", Object(
            ("id", String),
            ("call_id", String),
            ("status", Enum("in_progress", "completed", "incomplete")),
            ("action", Object(("commands", StringArray), ("timeout_ms", Nullish(Number)), ("max_output_length", Nullish(Number)))))),
        ("compaction", Object(("id", String), ("encrypted_content", String))),
        ("shell_call_output", Object(
            ("id", String),
            ("call_id", String),
            ("status", Enum("in_progress", "completed", "incomplete")),
            ("max_output_length", Nullish(Number)),
            ("output", Array(Object(
                ("stdout", String),
                ("stderr", String),
                ("outcome", Union(("timeout", Object()), ("exit", Object(("exit_code", Number)))))))))),
        ("tool_search_call", Object(
            ("id", String),
            ("execution", Enum("server", "client")),
            ("call_id", NullableString),
            ("status", Enum("in_progress", "completed", "incomplete")),
            ("arguments", Any))),
        ("tool_search_output", Object(
            ("id", String),
            ("execution", Enum("server", "client")),
            ("call_id", NullableString),
            ("status", Enum("in_progress", "completed", "incomplete")),
            ("tools", Array(Record(Any))))));

    private static readonly Rule Usage = Object(
        ("input_tokens", Number),
        ("input_tokens_details", Nullish(Object(("cached_tokens", Nullish(Number)), ("cache_write_tokens", Nullish(Number)), ("orchestration_input_tokens", Nullish(Number)), ("orchestration_input_cached_tokens", Nullish(Number))))),
        ("output_tokens", Number),
        ("output_tokens_details", Nullish(Object(("reasoning_tokens", Nullish(Number)), ("orchestration_output_tokens", Nullish(Number))))),
        ("total_tokens", Optional(Number)));

    private static readonly Rule Response = Object(
        ("id", Optional(String)),
        ("created_at", Optional(Number)),
        ("error", Nullish(Object(("message", String), ("type", String), ("param", Nullish(String)), ("code", String)))),
        ("model", Optional(String)),
        ("output", Optional(Array(OutputItem))),
        ("service_tier", Nullish(String)),
        ("reasoning", Nullish(Object(("context", Nullish(String))))),
        ("incomplete_details", Nullish(Object(("reason", String)))),
        ("usage", Nullish(Usage)));

    private static readonly Rule ResultLine = Object(
        ("custom_id", String),
        ("response", Nullish(Object(("status_code", Number), ("request_id", Nullish(String)), ("body", Any)))),
        ("error", Nullish(Object(("code", String), ("message", String)))));

    /// <summary>Checks a Responses API response body.</summary>
    public static bool IsValidResponse(JsonElement body) => Response(body);

    /// <summary>Checks one line of a batch output or error file.</summary>
    public static bool IsValidResultLine(JsonElement line) => ResultLine(line);

    private static Rule Optional(Rule rule) => value => value is null || rule(value);

    private static Rule Nullable(Rule rule) => value => value is { ValueKind: JsonValueKind.Null } || rule(value);

    private static Rule Nullish(Rule rule) => value => value is null || value.Value.ValueKind == JsonValueKind.Null || rule(value);

    private static Rule Either(params Rule[] rules) => value => rules.Any(rule => rule(value));

    private static Rule Literal(string expected) => value => value is { ValueKind: JsonValueKind.String } && value.Value.GetString() == expected;

    private static Rule Enum(params string[] allowed) => value => value is { ValueKind: JsonValueKind.String } && allowed.Contains(value.Value.GetString());

    private static Rule Array(Rule item) => value => value is { ValueKind: JsonValueKind.Array } && value.Value.EnumerateArray().All(element => item(element));

    private static Rule Record(Rule item) => value => value is { ValueKind: JsonValueKind.Object } && value.Value.EnumerateObject().All(property => item(property.Value));

    private static Rule Object(params (string Name, Rule Rule)[] properties) => value =>
        value is { ValueKind: JsonValueKind.Object } element
        && properties.All(property => property.Rule(element.TryGetProperty(property.Name, out var child) ? child : null));

    /// <summary>A zod discriminated union on <c>type</c>. The variant rule also checks the other keys of the object.</summary>
    private static Rule Union(params (string Type, Rule Rule)[] variants) => value =>
        value is { ValueKind: JsonValueKind.Object } element
        && element.TryGetProperty("type", out var type)
        && type.ValueKind == JsonValueKind.String
        && variants.FirstOrDefault(variant => variant.Type == type.GetString()) is { Rule: { } rule }
        && rule(value);
}
