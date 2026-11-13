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

    private static readonly Rule WebSearchAction = Union(
        ("search", Object(
            ("query", Nullish(String)),
            ("queries", Nullish(StringArray)),
            ("sources", Nullish(Array(Union(("url", Object(("url", String))), ("api", Object(("name", String))))))))),
        ("open_page", Object(("url", Nullish(String)))),
        ("find_in_page", Object(("url", Nullish(String)), ("pattern", Nullish(String)))));

    private static readonly Rule FileSearchResults = Array(Object(
        ("attributes", Record(Either(String, Number, Boolean))),
        ("file_id", String),
        ("filename", String),
        ("score", Number),
        ("text", String)));

    private static readonly Rule OutputItem = Union(
        ("message", Object(
            ("role", Literal("assistant")),
            ("id", String),
            ("phase", Nullish(Enum("commentary", "final_answer"))),
            // The API returns either text or a refusal as content parts.
            ("content", Array(Union(
                ("output_text", Object(
                    ("text", String),
                    ("logprobs", Nullish(Array(Logprob))),
                    ("annotations", Array(Annotation)))),
                ("refusal", Object(("refusal", String)))))))),
        ("web_search_call", Object(
            ("id", String),
            ("status", String),
            ("action", Nullish(WebSearchAction)))),
        ("file_search_call", Object(
            ("id", String),
            ("queries", StringArray),
            ("results", Nullish(FileSearchResults)))),
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
        ("input_tokens_details", Nullish(Object(("cached_tokens", Nullish(Number)), ("cache_write_tokens", Nullish(Number)), ("cache_write_tokens_details", Nullish(Object(("ttl_30m_tokens", Nullish(Number)), ("ttl_12h_tokens", Nullish(Number))))), ("orchestration_input_tokens", Nullish(Number)), ("orchestration_input_cached_tokens", Nullish(Number))))),
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

    private static readonly Rule PromptCacheDiagnostics = Object(
        ("type", String),
        ("reason", Nullish(String)),
        ("cache_missed_tokens", Nullish(Number)),
        ("comparison_reusable_tokens", Nullish(Number)));

    private static readonly Rule CodeInterpreterOutputs = Nullable(Array(Union(("logs", Object(("logs", String))), ("image", Object(("url", String))))));

    private static readonly Rule ApplyPatchOperation = Union(
        ("create_file", Object(("path", String), ("diff", String))),
        ("delete_file", Object(("path", String))),
        ("update_file", Object(("path", String), ("diff", String))));

    private static readonly Rule ShellCallAction = Object(("commands", StringArray), ("timeout_ms", Nullish(Number)), ("max_output_length", Nullish(Number)));

    private static readonly Rule ShellCallOutput = Array(Object(
        ("stdout", String),
        ("stderr", String),
        ("outcome", Union(("timeout", Object()), ("exit", Object(("exit_code", Number)))))));

    private static readonly Rule LocalShellCall = Object(
        ("id", String),
        ("call_id", String),
        ("action", Object(
            ("type", Literal("exec")),
            ("command", StringArray),
            ("timeout_ms", Optional(Number)),
            ("user", Optional(String)),
            ("working_directory", Optional(String)),
            ("env", Optional(Record(String))))));

    private static readonly Rule ComputerCall = Object(
        ("id", String),
        ("call_id", Nullish(String)),
        ("status", Enum("in_progress", "completed", "incomplete")),
        ("action", Nullish(ComputerAction)),
        ("actions", Nullish(Array(ComputerAction))),
        ("pending_safety_checks", Nullish(Array(Object(("id", String), ("code", Nullish(String)), ("message", Nullish(String)))))));

    private static readonly Rule Program = Object(("id", String), ("call_id", String), ("code", String), ("fingerprint", String));

    private static readonly Rule ProgramOutput = Object(("id", String), ("call_id", String), ("result", String), ("status", Enum("completed", "incomplete")));

    private static readonly Rule ToolSearchCall = Object(
        ("id", String),
        ("execution", Enum("server", "client")),
        ("call_id", NullableString),
        ("status", Enum("in_progress", "completed", "incomplete")),
        ("arguments", Any));

    private static readonly Rule ToolSearchOutput = Object(
        ("id", String),
        ("execution", Enum("server", "client")),
        ("call_id", NullableString),
        ("status", Enum("in_progress", "completed", "incomplete")),
        ("tools", Array(Record(Any))));

    private static readonly Rule ChunkItemAdded = Union(
        ("message", Object(("id", String), ("phase", Nullish(Enum("commentary", "final_answer"))))),
        ("reasoning", Object(("id", String), ("encrypted_content", Nullish(String)))),
        ("function_call", Object(
            ("id", String),
            ("call_id", String),
            ("name", String),
            ("arguments", String),
            ("async", Nullish(Boolean)),
            ("namespace", Nullish(String)),
            ("caller", Nullish(ToolCaller)))),
        ("program", Program),
        ("program_output", ProgramOutput),
        ("web_search_call", Object(("id", String), ("status", String))),
        ("computer_call", ComputerCall),
        ("file_search_call", Object(("id", String))),
        ("local_shell_call", LocalShellCall),
        ("image_generation_call", Object(("id", String))),
        ("code_interpreter_call", Object(("id", String), ("container_id", String), ("code", NullableString), ("outputs", CodeInterpreterOutputs), ("status", String))),
        ("mcp_call", Object(("id", String), ("status", String), ("approval_request_id", Nullish(String)))),
        ("mcp_list_tools", Object(("id", String))),
        ("mcp_approval_request", Object(("id", String))),
        ("apply_patch_call", Object(("id", String), ("call_id", String), ("status", Enum("in_progress", "completed")), ("operation", ApplyPatchOperation))),
        ("custom_tool_call", Object(("id", String), ("call_id", String), ("name", String), ("input", String), ("async", Nullish(Boolean)))),
        ("shell_call", Object(("id", String), ("call_id", String), ("status", Enum("in_progress", "completed", "incomplete")), ("action", ShellCallAction))),
        ("compaction", Object(("id", String), ("encrypted_content", Nullish(String)))),
        ("shell_call_output", Object(
            ("id", String),
            ("call_id", String),
            ("status", Enum("in_progress", "completed", "incomplete")),
            ("max_output_length", Nullish(Number)),
            ("output", ShellCallOutput))),
        ("tool_search_call", ToolSearchCall),
        ("tool_search_output", ToolSearchOutput));

    private static readonly Rule ChunkItemDone = Union(
        ("message", Object(("id", String), ("phase", Nullish(Enum("commentary", "final_answer"))))),
        ("reasoning", Object(("id", String), ("encrypted_content", Nullish(String)))),
        ("function_call", Object(
            ("id", String),
            ("call_id", String),
            ("name", String),
            ("arguments", String),
            ("async", Nullish(Boolean)),
            ("status", Enum("in_progress", "completed", "incomplete")),
            ("namespace", Nullish(String)),
            ("caller", Nullish(ToolCaller)))),
        ("program", Program),
        ("program_output", ProgramOutput),
        ("custom_tool_call", Object(
            ("id", String),
            ("call_id", String),
            ("name", String),
            ("input", String),
            ("async", Nullish(Boolean)),
            ("status", Literal("completed")))),
        ("code_interpreter_call", Object(("id", String), ("code", NullableString), ("container_id", String), ("outputs", CodeInterpreterOutputs))),
        ("image_generation_call", Object(("id", String), ("result", String))),
        ("web_search_call", Object(("id", String), ("status", String), ("action", Nullish(WebSearchAction)))),
        ("file_search_call", Object(("id", String), ("queries", StringArray), ("results", Nullish(FileSearchResults)))),
        ("local_shell_call", LocalShellCall),
        ("computer_call", ComputerCall),
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
        ("mcp_approval_request", Object(("id", String), ("server_label", String), ("name", String), ("arguments", String), ("approval_request_id", Optional(String)))),
        ("apply_patch_call", Object(("id", String), ("call_id", String), ("status", Enum("in_progress", "completed")), ("operation", ApplyPatchOperation))),
        ("shell_call", Object(("id", String), ("call_id", String), ("status", Enum("in_progress", "completed", "incomplete")), ("action", ShellCallAction))),
        ("compaction", Object(("id", String), ("encrypted_content", String))),
        ("shell_call_output", Object(
            ("id", String),
            ("call_id", String),
            ("status", Enum("in_progress", "completed", "incomplete")),
            ("max_output_length", Nullish(Number)),
            ("output", ShellCallOutput))),
        ("tool_search_call", ToolSearchCall),
        ("tool_search_output", ToolSearchOutput));

    private static readonly HashSet<string> ModeledChunkTypes =
    [
        "error",
        "response.apply_patch_call_operation_diff.delta",
        "response.apply_patch_call_operation_diff.done",
        "response.code_interpreter_call_code.delta",
        "response.code_interpreter_call_code.done",
        "response.completed",
        "response.created",
        "response.custom_tool_call_input.delta",
        "response.failed",
        "response.function_call_arguments.delta",
        "response.function_call_arguments.done",
        "response.image_generation_call.partial_image",
        "response.in_progress",
        "response.incomplete",
        "response.output_item.added",
        "response.output_item.done",
        "response.output_text.annotation.added",
        "response.output_text.delta",
        "response.refusal.delta",
        "response.refusal.done",
        "response.reasoning_summary_part.added",
        "response.reasoning_summary_part.done",
        "response.reasoning_summary_text.delta",
    ];

    private static readonly HashSet<string> ModeledOutputItemTypes =
    [
        "apply_patch_call",
        "code_interpreter_call",
        "compaction",
        "computer_call",
        "custom_tool_call",
        "file_search_call",
        "function_call",
        "image_generation_call",
        "local_shell_call",
        "mcp_approval_request",
        "mcp_call",
        "mcp_list_tools",
        "message",
        "program",
        "program_output",
        "reasoning",
        "shell_call",
        "shell_call_output",
        "tool_search_call",
        "tool_search_output",
        "web_search_call",
    ];

    private static readonly Rule ResponseHeader = Object(("id", String), ("created_at", Number), ("model", String), ("service_tier", Nullish(String)));

    private static readonly Rule ResponseReasoning = Nullish(Object(("context", Nullish(String))));

    private static readonly Rule IncompleteDetails = Nullish(Object(("reason", String)));

    private static readonly Rule CompletedChunk = Object(
        ("response", Object(
            ("prompt_cache_diagnostics", Nullish(PromptCacheDiagnostics)),
            ("incomplete_details", IncompleteDetails),
            ("usage", Nullish(Usage)),
            ("reasoning", ResponseReasoning),
            ("service_tier", Nullish(String)))));

    private static readonly Rule Chunk = Union(
        ("response.output_text.delta", Object(("item_id", String), ("output_index", Nullish(Number)), ("delta", String), ("logprobs", Nullish(Array(Logprob))))),
        ("response.refusal.delta", Object(("item_id", String), ("output_index", Nullish(Number)), ("content_index", Nullish(Number)), ("delta", String))),
        ("response.refusal.done", Object(("item_id", String), ("output_index", Nullish(Number)), ("content_index", Nullish(Number)), ("refusal", String))),
        ("response.completed", CompletedChunk),
        ("response.incomplete", CompletedChunk),
        ("response.failed", Object(
            ("sequence_number", Number),
            ("response", Object(
                ("prompt_cache_diagnostics", Nullish(PromptCacheDiagnostics)),
                ("error", Nullish(Object(("code", Nullish(String)), ("message", String)))),
                ("incomplete_details", IncompleteDetails),
                ("usage", Nullish(Usage)),
                ("reasoning", ResponseReasoning),
                ("service_tier", Nullish(String)))))),
        ("response.created", Object(("response", ResponseHeader))),
        ("response.in_progress", Object(("response", ResponseHeader))),
        ("response.output_item.added", Object(("output_index", Number), ("item", ChunkItemAdded))),
        ("response.output_item.done", Object(("output_index", Number), ("item", ChunkItemDone))),
        ("response.function_call_arguments.delta", Object(("item_id", String), ("output_index", Number), ("delta", String))),
        // `name` is documented as required but omitted from live API events: https://github.com/openai/openai-openapi/issues/545
        ("response.function_call_arguments.done", Object(("item_id", String), ("output_index", Number), ("arguments", String))),
        ("response.custom_tool_call_input.delta", Object(("item_id", String), ("output_index", Number), ("delta", String))),
        ("response.image_generation_call.partial_image", Object(("item_id", String), ("output_index", Number), ("partial_image_b64", String))),
        ("response.code_interpreter_call_code.delta", Object(("item_id", String), ("output_index", Number), ("delta", String))),
        ("response.code_interpreter_call_code.done", Object(("item_id", String), ("output_index", Number), ("code", String))),
        ("response.output_text.annotation.added", Object(("annotation", Annotation))),
        ("response.reasoning_summary_part.added", Object(("item_id", String), ("output_index", Nullish(Number)), ("summary_index", Number))),
        ("response.reasoning_summary_text.delta", Object(("item_id", String), ("output_index", Nullish(Number)), ("summary_index", Number), ("delta", String))),
        ("response.reasoning_summary_part.done", Object(("item_id", String), ("output_index", Nullish(Number)), ("summary_index", Number))),
        ("response.apply_patch_call_operation_diff.delta", Object(("item_id", String), ("output_index", Number), ("delta", String), ("obfuscation", Nullish(String)))),
        ("response.apply_patch_call_operation_diff.done", Object(("item_id", String), ("output_index", Number), ("diff", String))),
        // Two shapes of the error chunk: the nested one captured from an early insufficient_quota stream error, and the documented ResponseErrorEvent.
        ("error", Either(
            Object(("sequence_number", Number), ("error", Object(("type", String), ("code", Nullish(String)), ("message", String), ("param", Nullish(String))))),
            Object(("sequence_number", Number), ("code", Nullish(String)), ("message", String), ("param", Nullish(String))))));

    private static readonly Rule ResultLine = Object(
        ("custom_id", String),
        ("response", Nullish(Object(("status_code", Number), ("request_id", Nullish(String)), ("body", Any)))),
        ("error", Nullish(Object(("code", String), ("message", String)))));

    /// <summary>Checks a Responses API response body.</summary>
    public static bool IsValidResponse(JsonElement body) => Response(body);

    /// <summary>Checks one stream event: a modeled chunk, or an unknown chunk type that falls back to <c>unknown_chunk</c>.</summary>
    public static bool IsValidChunk(JsonElement chunk) => Chunk(chunk) || IsUnknownChunk(chunk);

    /// <summary>Checks that a stream event is an unknown chunk, or a modeled chunk whose output item is of an unknown type. Mirrors the <c>unknown_chunk</c> fallback.</summary>
    public static bool IsUnknownChunk(JsonElement chunk) => !Chunk(chunk) && chunk is { ValueKind: JsonValueKind.Object } && !IsModeledChunk(chunk);

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

    private static bool IsModeledChunk(JsonElement value)
    {
        if (!value.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String)
        {
            // The fallback needs a string type, so a value without one is not modeled and not valid either.
            return true;
        }

        var typeName = type.GetString()!;

        if (!ModeledChunkTypes.Contains(typeName))
        {
            return false;
        }

        if (typeName is not ("response.output_item.added" or "response.output_item.done"))
        {
            return true;
        }

        if (!value.TryGetProperty("item", out var item)
            || item.ValueKind != JsonValueKind.Object
            || !item.TryGetProperty("type", out var itemType)
            || itemType.ValueKind != JsonValueKind.String)
        {
            return true;
        }

        return ModeledOutputItemTypes.Contains(itemType.GetString()!);
    }
}
