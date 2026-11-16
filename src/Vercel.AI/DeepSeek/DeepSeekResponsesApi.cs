// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.DeepSeek;

/// <summary>
/// Structural check of a DeepSeek Responses API response body and stream chunk.
/// Mirrors <c>deepseekResponsesResponseSchema</c> and <c>deepseekResponsesChunkSchema</c> in <c>deepseek-responses-api.ts</c>.
/// A rule receives <see langword="null"/> when the key is absent. Unknown keys are accepted, as in zod.
/// </summary>
internal static class DeepSeekResponsesApi
{
    private delegate bool Rule(JsonElement? value);

    private static readonly Rule String = value => value is { ValueKind: JsonValueKind.String };

    private static readonly Rule Number = value => value is { ValueKind: JsonValueKind.Number };

    private static readonly Rule Any = _ => true;

    private static readonly Rule NullishString = Nullish(String);

    private static readonly Rule NullishNumber = Nullish(Number);

    private static readonly Rule Usage = Object(
        ("input_tokens", NullishNumber),
        ("input_tokens_details", Nullish(Object(("cached_tokens", NullishNumber)))),
        ("output_tokens", NullishNumber),
        ("output_tokens_details", Nullish(Object(("reasoning_tokens", NullishNumber)))),
        ("total_tokens", NullishNumber),
        ("server_tool_use", Nullish(Object(("tool_search", NullishNumber), ("tool_fetch", NullishNumber)))));

    private static readonly Rule Logprob = Object(
        ("token", String),
        ("logprob", Number),
        ("bytes", Nullish(Array(Number))),
        ("top_logprobs", Nullish(Array(Object(("token", String), ("logprob", Number), ("bytes", Nullish(Array(Number))))))));

    private static readonly Rule OutputItem = Either(
        Object(
            ("type", Literal("message")),
            ("id", NullishString),
            ("content", Nullish(Array(Object(("type", String), ("text", NullishString), ("logprobs", Nullish(Array(Logprob)))))))),
        Object(
            ("type", Literal("reasoning")),
            ("id", NullishString),
            ("content", Nullish(Array(Object(("type", String), ("text", NullishString)))))),
        Object(
            ("type", Literal("function_call")),
            ("id", NullishString),
            ("call_id", String),
            ("name", String),
            ("arguments", NullishString)),
        Object(
            ("type", Literal("web_search_call")),
            ("id", String),
            ("status", NullishString),
            ("action", Nullish(Record(Any)))),
        Object(("type", String)));

    private static readonly Rule ResponseObject = Object(
        ("id", NullishString),
        ("created_at", NullishNumber),
        ("model", NullishString),
        ("status", NullishString),
        ("error", Nullish(Object(("code", NullishString), ("message", NullishString)))),
        ("incomplete_details", Nullish(Object(("reason", NullishString)))),
        ("output", Nullish(Array(OutputItem))),
        ("usage", Nullish(Usage)));

    private static readonly Rule Chunk = Either(
        Object(("type", Enum("response.created", "response.in_progress")), ("response", ResponseObject)),
        Object(("type", Enum("response.completed", "response.incomplete", "response.failed")), ("response", ResponseObject)),
        Object(("type", Enum("response.output_item.added", "response.output_item.done")), ("output_index", NullishNumber), ("item", OutputItem)),
        Object(("type", Literal("response.output_text.delta")), ("item_id", String), ("delta", String), ("logprobs", Nullish(Array(Logprob)))),
        Object(("type", Literal("response.reasoning_text.delta")), ("item_id", String), ("delta", String)),
        Object(("type", Literal("response.function_call_arguments.delta")), ("item_id", String), ("delta", String)),
        Object(("type", Literal("error")), ("code", NullishString), ("message", NullishString)),
        Object(("type", String)));

    /// <summary>Checks a Responses API response body.</summary>
    public static bool IsValidResponse(JsonElement body) => ResponseObject(body);

    /// <summary>Checks one stream event.</summary>
    public static bool IsValidChunk(JsonElement chunk) => Chunk(chunk);

    private static Rule Nullish(Rule rule) => value => value is null || value.Value.ValueKind == JsonValueKind.Null || rule(value);

    private static Rule Either(params Rule[] rules) => value => rules.Any(rule => rule(value));

    private static Rule Literal(string expected) => value => value is { ValueKind: JsonValueKind.String } && value.Value.GetString() == expected;

    private static Rule Enum(params string[] allowed) => value => value is { ValueKind: JsonValueKind.String } && allowed.Contains(value.Value.GetString());

    private static Rule Array(Rule item) => value => value is { ValueKind: JsonValueKind.Array } && value.Value.EnumerateArray().All(element => item(element));

    private static Rule Record(Rule item) => value => value is { ValueKind: JsonValueKind.Object } && value.Value.EnumerateObject().All(property => item(property.Value));

    private static Rule Object(params (string Name, Rule Rule)[] properties) => value =>
        value is { ValueKind: JsonValueKind.Object } element
        && properties.All(property => property.Rule(element.TryGetProperty(property.Name, out var child) ? child : null));
}
