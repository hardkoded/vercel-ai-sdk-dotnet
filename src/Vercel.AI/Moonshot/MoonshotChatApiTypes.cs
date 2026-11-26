// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;

namespace Vercel.AI.Moonshot;

/// <summary>
/// Validation of the Moonshot AI chat response and stream chunks. Maps to the zod schemas of
/// <c>moonshotai-chat-api-types.ts</c>: <c>tokenUsageSchema</c>, <c>moonshotAIChatResponseSchema</c> and
/// <c>moonshotAIChatChunkSchema</c>. The schemas are limited versions focused on what the implementation needs,
/// so fields they do not list pass through.
/// </summary>
internal static class MoonshotChatApiTypes
{
    /// <summary>Validates a chat response body. Maps to <c>moonshotAIChatResponseSchema</c>.</summary>
    /// <exception cref="InvalidResponseDataException">The body does not match the schema.</exception>
    public static void ValidateChatResponse(JsonElement root)
    {
        var error = ChatResponseError(JsonNode.Parse(root.GetRawText()));
        if (error != null)
        {
            throw new InvalidResponseDataException(
                root.Clone(),
                "Type validation failed: " + error
            );
        }
    }

    /// <summary>Returns the error of a streamed chunk that does not match <c>moonshotAIChatChunkSchema</c>, or null.</summary>
    public static string? ChatChunkError(JsonObject root)
    {
        return Fields(
            root,
            "chunk",
            ("id", NullableString),
            ("created", NullableNumber),
            ("model", NullableString),
            ("object", Literal("chat.completion.chunk")),
            ("usage", TokenUsage)
        )
            ?? Array(root["choices"], "chunk.choices", (choice, path) =>
                Fields(
                    choice,
                    path,
                    ("index", NullableNumber),
                    ("delta", (delta, path) => Nullable(delta, Delta, path)),
                    ("logprobs", Logprobs),
                    ("finish_reason", NullableString),
                    ("usage", TokenUsage)
                )
            );
    }

    private static string? ChatResponseError(JsonNode? root)
    {
        return Fields(
            root,
            "response",
            ("id", NullableString),
            ("created", NullableNumber),
            ("model", NullableString),
            ("object", Literal("chat.completion")),
            ("usage", TokenUsage)
        )
            ?? Array((root as JsonObject)?["choices"], "response.choices", (choice, path) =>
                Fields(
                    choice,
                    path,
                    ("index", NullableNumber),
                    ("message", Required(Message)),
                    ("logprobs", Logprobs),
                    ("finish_reason", NullableString)
                )
            );
    }

    private static string? Message(JsonNode? message, string path)
    {
        return Fields(
            message,
            path,
            ("role", Literal("assistant")),
            ("content", NullableString),
            ("reasoning_content", NullableString),
            ("tool_calls", ToolCalls(chunk: false))
        );
    }

    private static string? Delta(JsonNode? delta, string path)
    {
        return Fields(
            delta,
            path,
            ("role", Literal("assistant")),
            ("content", NullableString),
            ("reasoning_content", NullableString),
            ("tool_calls", ToolCalls(chunk: true))
        );
    }

    private static Func<JsonNode?, string, string?> ToolCalls(bool chunk)
    {
        return (calls, path) =>
            Nullable(
                calls,
                (value, p) =>
                    Array(value, p, (call, callPath) =>
                        Fields(
                            call,
                            callPath,
                            ("index", chunk ? NullableNumber : Any),
                            ("id", NullableString),
                            ("type", Literal("function")),
                            (
                                "function",
                                Required(
                                    (function, functionPath) =>
                                        chunk
                                            ? Fields(
                                                function,
                                                functionPath,
                                                ("name", NullableString),
                                                ("arguments", NullableString)
                                            )
                                            : Fields(
                                                function,
                                                functionPath,
                                                ("name", RequiredString),
                                                ("arguments", RequiredString)
                                            )
                                )
                            )
                        )
                    ),
                path
            );
    }

    private static string? Logprobs(JsonNode? logprobs, string path)
    {
        return Nullable(
            logprobs,
            (value, p) =>
                Fields(
                    value,
                    p,
                    ("content", (content, contentPath) =>
                        Nullable(
                            content,
                            (items, itemsPath) => Array(items, itemsPath, Logprob),
                            contentPath
                        )
                    )
                ),
            path
        );
    }

    private static string? Logprob(JsonNode? logprob, string path)
    {
        return Fields(
            logprob,
            path,
            ("token", RequiredString),
            ("logprob", RequiredNumber),
            ("bytes", Bytes),
            (
                "top_logprobs",
                Required(
                    (top, topPath) =>
                        Array(top, topPath, (entry, entryPath) =>
                            Fields(
                                entry,
                                entryPath,
                                ("token", RequiredString),
                                ("logprob", RequiredNumber),
                                ("bytes", Bytes)
                            )
                        )
                )
            )
        );
    }

    private static string? Bytes(JsonNode? bytes, string path)
    {
        return bytes == null ? null : Array(bytes, path, RequiredNumber);
    }

    /// <summary>Maps to <c>tokenUsageSchema</c>: numbers or null, nested objects or null, loose.</summary>
    private static string? TokenUsage(JsonNode? usage, string path)
    {
        return Nullable(
            usage,
            (value, p) =>
                Fields(
                    value,
                    p,
                    ("prompt_tokens", NullableNumber),
                    ("completion_tokens", NullableNumber),
                    ("cached_tokens", NullableNumber),
                    ("total_tokens", NullableNumber),
                    ("prompt_tokens_details", (details, detailsPath) =>
                        Nullable(
                            details,
                            (d, dp) =>
                                Fields(
                                    d,
                                    dp,
                                    ("cached_tokens", NullableNumber),
                                    ("cache_write_tokens", NullableNumber)
                                ),
                            detailsPath
                        )
                    ),
                    ("completion_tokens_details", (details, detailsPath) =>
                        Nullable(
                            details,
                            (d, dp) => Fields(d, dp, ("reasoning_tokens", NullableNumber)),
                            detailsPath
                        )
                    )
                ),
            path
        );
    }

    private static string? Nullable(
        JsonNode? value,
        Func<JsonNode?, string, string?> inner,
        string path
    )
    {
        return value == null ? null : inner(value, path);
    }

    private static Func<JsonNode?, string, string?> Required(Func<JsonNode?, string, string?> inner)
    {
        return (value, path) => value == null ? path + " is required." : inner(value, path);
    }

    private static string? Any(JsonNode? value, string path)
    {
        return null;
    }

    private static string? NullableString(JsonNode? value, string path)
    {
        return value == null || IsKind(value, JsonValueKind.String)
            ? null
            : path + " must be a string.";
    }

    private static string? RequiredString(JsonNode? value, string path)
    {
        return IsKind(value, JsonValueKind.String) ? null : path + " must be a string.";
    }

    private static string? NullableNumber(JsonNode? value, string path)
    {
        return value == null || IsKind(value, JsonValueKind.Number)
            ? null
            : path + " must be a number.";
    }

    private static string? RequiredNumber(JsonNode? value, string path)
    {
        return IsKind(value, JsonValueKind.Number) ? null : path + " must be a number.";
    }

    private static Func<JsonNode?, string, string?> Literal(string expected)
    {
        return (value, path) =>
            value == null
            || (IsKind(value, JsonValueKind.String) && value.GetValue<string>() == expected)
                ? null
                : path + " must be \"" + expected + "\".";
    }

    private static bool IsKind(JsonNode? value, JsonValueKind kind)
    {
        return value is JsonValue json && json.GetValueKind() == kind;
    }

    private static string? Array(
        JsonNode? value,
        string path,
        Func<JsonNode?, string, string?> item
    )
    {
        if (value is not JsonArray array)
        {
            return path + " must be an array.";
        }

        for (var i = 0; i < array.Count; i++)
        {
            var error = item(array[i], path + "[" + i + "]");
            if (error != null)
            {
                return error;
            }
        }

        return null;
    }

    private static string? Fields(
        JsonNode? value,
        string path,
        params (string Name, Func<JsonNode?, string, string?> Check)[] fields
    )
    {
        if (value is not JsonObject obj)
        {
            return path + " must be an object.";
        }

        foreach (var (name, check) in fields)
        {
            var error = check(obj[name], path + "." + name);
            if (error != null)
            {
                return error;
            }
        }

        return null;
    }
}
