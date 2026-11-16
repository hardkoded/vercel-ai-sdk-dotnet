// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Anthropic;

/// <summary>
/// Structural check of an Anthropic Message body. Mirrors <c>anthropicResponseSchema</c>: required keys, value kinds, and the <c>type</c> discriminators, down to the nested result bodies.
/// A rule receives <see langword="null"/> when the key is absent.
/// </summary>
internal static class AnthropicResponseSchema
{
    private delegate bool Rule(JsonElement? value);

    private static readonly Rule String = value => value is { ValueKind: JsonValueKind.String };

    private static readonly Rule Number = value => value is { ValueKind: JsonValueKind.Number };

    private static readonly Rule Boolean = value => value is { ValueKind: JsonValueKind.True or JsonValueKind.False };

    private static readonly Rule Any = _ => true;

    private static readonly Rule NullableString = Nullable(String);

    private static readonly Rule NullishString = Nullish(String);

    private static readonly Rule NullableNumber = Nullable(Number);

    private static readonly Rule ToolCallCaller = Union(
        ("code_execution_20250825", Object(("tool_id", String))),
        ("code_execution_20260120", Object(("tool_id", String))),
        ("direct", Object()));

    private static readonly Rule Citation = Union(
        ("web_search_result_location", Object(("cited_text", String), ("url", String), ("title", NullableString), ("encrypted_index", String))),
        ("page_location", Object(("cited_text", String), ("document_index", Number), ("document_title", NullableString), ("start_page_number", Number), ("end_page_number", Number), ("file_id", NullishString))),
        ("char_location", Object(("cited_text", String), ("document_index", Number), ("document_title", NullableString), ("start_char_index", Number), ("end_char_index", Number), ("file_id", NullishString))),
        ("content_block_location", Object(("cited_text", String), ("document_index", Number), ("document_title", NullableString), ("start_block_index", Number), ("end_block_index", Number), ("file_id", NullableString))),
        ("search_result_location", Object(("cited_text", String), ("search_result_index", Number), ("source", String), ("title", NullableString), ("start_block_index", Number), ("end_block_index", Number))));

    // MCP tool-result citations are opaque pass-through data.
    private static readonly Rule McpToolResultContent = Either(
        String,
        Array(Either(String, Object(("type", Literal("text")), ("text", String), ("citations", Nullish(Array(Any)))))));

    private static readonly Rule CodeExecutionOutputs = Optional(Array(Object(("type", Literal("code_execution_output")), ("file_id", String))));

    private static readonly Rule BashCodeExecutionOutputs = Array(Object(("type", Literal("bash_code_execution_output")), ("file_id", String)));

    private static readonly Rule WebFetchSource = Either(
        Object(("type", Literal("base64")), ("media_type", Literal("application/pdf")), ("data", String)),
        Object(("type", Literal("text")), ("media_type", Literal("text/plain")), ("data", String)));

    private static readonly Rule StopDetails = Object(("type", String), ("category", NullishString), ("explanation", NullishString), ("recommended_model", NullishString));

    private static readonly Rule SafeguardResult = Object(
        ("type", String),
        ("status", Object(
            ("type", String),
            ("tool_uses", Nullish(Record(Object(("type", String), ("outcome", NullishString), ("explanation", NullishString))))))));

    private static readonly Rule InputTransformation = Object(("type", String), ("path", String), ("reason", String));

    private static readonly Rule UsageIteration = Object(
        ("type", Enum("compaction", "message", "advisor_message", "fallback_message")),
        ("model", NullishString),
        ("input_tokens", Number),
        ("output_tokens", Number),
        ("cache_creation_input_tokens", Nullish(Number)),
        ("cache_read_input_tokens", Nullish(Number)));

    private static readonly Rule Usage = Object(
        ("service_tier", NullishString),
        ("input_tokens", Number),
        ("output_tokens", Number),
        ("output_tokens_details", Nullish(Object(("thinking_tokens", Nullish(Number))))),
        ("cache_creation_input_tokens", Nullish(Number)),
        ("cache_read_input_tokens", Nullish(Number)),
        ("iterations", Nullish(Array(UsageIteration))));

    private static readonly Rule Container = Nullish(Object(
        ("expires_at", String),
        ("id", String),
        ("skills", Nullish(Array(Object(("type", Enum("anthropic", "custom")), ("skill_id", String), ("version", String)))))));

    private static readonly Rule ContextManagement = Nullish(Object(
        ("applied_edits", Array(Union(
            ("clear_tool_uses_20250919", Object(("cleared_tool_uses", Number), ("cleared_input_tokens", Number))),
            ("clear_thinking_20251015", Object(("cleared_thinking_turns", Number), ("cleared_input_tokens", Number))),
            ("compact_20260112", Object()))))));

    private static readonly Rule ContentPart = Union(
        ("text", Object(("text", String), ("citations", Nullish(Array(Citation))))),
        ("thinking", Object(("thinking", String), ("signature", String))),
        ("redacted_thinking", Object(("data", String))),
        ("compaction", Object(("content", NullishString), ("signature", NullishString))),
        ("tool_use", Object(("id", String), ("name", String), ("caller", Optional(ToolCallCaller)), ("toolset_name", NullishString))),
        ("server_tool_use", Object(("id", String), ("name", String), ("input", Nullish(Record(Any))), ("caller", Optional(ToolCallCaller)))),
        ("mcp_tool_use", Object(("id", String), ("name", String), ("server_name", String))),
        ("mcp_tool_result", Object(("tool_use_id", String), ("is_error", Boolean), ("content", McpToolResultContent))),
        ("web_fetch_tool_result", Object(
            ("tool_use_id", String),
            ("caller", Optional(ToolCallCaller)),
            ("content", Either(
                Object(
                    ("type", Literal("web_fetch_result")),
                    ("url", String),
                    ("retrieved_at", NullableString),
                    ("content", Object(
                        ("type", Literal("document")),
                        ("title", NullableString),
                        ("citations", Optional(Object(("enabled", Boolean)))),
                        ("source", WebFetchSource)))),
                Object(("type", Literal("web_fetch_tool_result_error")), ("error_code", String)))))),
        ("web_search_tool_result", Object(
            ("tool_use_id", String),
            ("caller", Optional(ToolCallCaller)),
            ("content", Either(
                Array(Object(("type", Literal("web_search_result")), ("url", String), ("title", NullableString), ("encrypted_content", String), ("page_age", NullishString))),
                Object(("type", Literal("web_search_tool_result_error")), ("error_code", String)))))),
        ("code_execution_tool_result", Object(
            ("tool_use_id", String),
            ("content", Union(
                ("code_execution_result", Object(("stdout", String), ("stderr", String), ("return_code", Number), ("content", CodeExecutionOutputs))),
                ("encrypted_code_execution_result", Object(("encrypted_stdout", String), ("stderr", String), ("return_code", Number), ("content", CodeExecutionOutputs))),
                ("code_execution_tool_result_error", Object(("error_code", String))))))),
        ("bash_code_execution_tool_result", Object(
            ("tool_use_id", String),
            ("content", Union(
                ("bash_code_execution_result", Object(("content", BashCodeExecutionOutputs), ("stdout", String), ("stderr", String), ("return_code", Number))),
                ("bash_code_execution_tool_result_error", Object(("error_code", String))))))),
        ("text_editor_code_execution_tool_result", Object(
            ("tool_use_id", String),
            ("content", Union(
                ("text_editor_code_execution_tool_result_error", Object(("error_code", String))),
                ("text_editor_code_execution_view_result", Object(("content", String), ("file_type", String), ("num_lines", NullableNumber), ("start_line", NullableNumber), ("total_lines", NullableNumber))),
                ("text_editor_code_execution_create_result", Object(("is_file_update", Boolean))),
                ("text_editor_code_execution_str_replace_result", Object(("lines", Nullable(Array(String))), ("new_lines", NullableNumber), ("new_start", NullableNumber), ("old_lines", NullableNumber), ("old_start", NullableNumber))))))),
        ("tool_search_tool_result", Object(
            ("tool_use_id", String),
            ("content", Union(
                ("tool_search_tool_search_result", Object(("tool_references", Array(Object(("type", Literal("tool_reference")), ("tool_name", String)))))),
                ("tool_search_tool_result_error", Object(("error_code", String))))))),
        ("container_upload", Object(("file_id", String))),
        ("advisor_tool_result", Object(
            ("tool_use_id", String),
            ("content", Union(
                ("advisor_result", Object(("text", String), ("stop_reason", NullishString))),
                ("advisor_redacted_result", Object(("encrypted_content", String), ("stop_reason", NullishString))),
                ("advisor_tool_result_error", Object(("error_code", String))))))),
        ("fallback", Object(("from", Object(("model", String))), ("to", Object(("model", String))))));

    // The content array is checked part by part, see IsValidContentPart.
    private static readonly Rule Message = Object(
        ("type", Literal("message")),
        ("id", NullishString),
        ("model", NullishString),
        ("content", Array(Any)),
        ("stop_reason", NullishString),
        ("stop_sequence", NullishString),
        ("service_tier", NullishString),
        ("stop_details", Nullish(StopDetails)),
        ("input_transformations", Nullish(Array(InputTransformation))),
        ("safeguard_results", Nullish(Array(SafeguardResult))),
        ("usage", Usage),
        ("container", Container),
        ("context_management", ContextManagement));

    /// <summary>Checks everything of a message except its content parts.</summary>
    public static bool IsValidMessage(JsonElement message) => Message(message);

    /// <summary>Checks one content part against the discriminated union of known blocks.</summary>
    public static bool IsValidContentPart(JsonElement part) => ContentPart(part);

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
