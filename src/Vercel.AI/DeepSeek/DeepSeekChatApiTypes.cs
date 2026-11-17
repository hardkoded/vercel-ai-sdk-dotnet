// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.DeepSeek;

/// <summary>
/// Structural check of a DeepSeek chat response body and stream chunk.
/// Mirrors <c>deepseekChatResponseSchema</c>, <c>deepseekChatChunkSchema</c> and <c>deepSeekErrorSchema</c> in
/// <c>deepseek-chat-api-types.ts</c>. Unknown keys are accepted, as in zod.
/// </summary>
internal static class DeepSeekChatApiTypes
{
    /// <summary>True when <paramref name="root"/> matches <c>deepseekChatResponseSchema</c>.</summary>
    public static bool IsValidResponse(JsonElement root)
    {
        if (
            root.ValueKind != JsonValueKind.Object
            || !Nullish(root, "id", JsonValueKind.String)
            || !Nullish(root, "created", JsonValueKind.Number)
            || !Nullish(root, "model", JsonValueKind.String)
            || !Nullish(root, "system_fingerprint", JsonValueKind.String)
            || !IsValidUsage(root)
            || !root.TryGetProperty("choices", out var choices)
            || choices.ValueKind != JsonValueKind.Array
        )
        {
            return false;
        }

        foreach (var choice in choices.EnumerateArray())
        {
            if (
                choice.ValueKind != JsonValueKind.Object
                || !Nullish(choice, "index", JsonValueKind.Number)
                || !Nullish(choice, "finish_reason", JsonValueKind.String)
                || !IsValidLogprobs(choice)
                || !choice.TryGetProperty("message", out var message)
                || message.ValueKind != JsonValueKind.Object
                || !Nullish(message, "content", JsonValueKind.String)
                || !Nullish(message, "reasoning_content", JsonValueKind.String)
                || !IsValidToolCalls(message, requireIndex: false)
            )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>True when <paramref name="root"/> matches <c>deepSeekErrorSchema</c>.</summary>
    public static bool IsError(JsonElement root)
    {
        return root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("error", out var error)
            && error.ValueKind == JsonValueKind.Object
            && error.TryGetProperty("message", out var message)
            && message.ValueKind == JsonValueKind.String
            && Nullish(error, "type", JsonValueKind.String);
    }

    /// <summary>True when <paramref name="root"/> matches <c>deepseekChatChunkSchema</c>.</summary>
    public static bool IsValidChunk(JsonElement root)
    {
        if (IsError(root))
        {
            return true;
        }

        if (
            root.ValueKind != JsonValueKind.Object
            || !Nullish(root, "id", JsonValueKind.String)
            || !Nullish(root, "created", JsonValueKind.Number)
            || !Nullish(root, "model", JsonValueKind.String)
            || !Nullish(root, "system_fingerprint", JsonValueKind.String)
            || !IsValidUsage(root)
            || !root.TryGetProperty("choices", out var choices)
            || choices.ValueKind != JsonValueKind.Array
        )
        {
            return false;
        }

        foreach (var choice in choices.EnumerateArray())
        {
            if (
                choice.ValueKind != JsonValueKind.Object
                || !Nullish(choice, "index", JsonValueKind.Number)
                || !Nullish(choice, "finish_reason", JsonValueKind.String)
                || !IsValidLogprobs(choice)
            )
            {
                return false;
            }

            if (
                choice.TryGetProperty("delta", out var delta)
                && delta.ValueKind != JsonValueKind.Null
                && (
                    delta.ValueKind != JsonValueKind.Object
                    || !Nullish(delta, "role", JsonValueKind.String)
                    || !Nullish(delta, "content", JsonValueKind.String)
                    || !Nullish(delta, "reasoning_content", JsonValueKind.String)
                    || !IsValidToolCalls(delta, requireIndex: true)
                )
            )
            {
                return false;
            }
        }

        return true;
    }

    private static bool Nullish(JsonElement element, string name, JsonValueKind kind)
    {
        return !element.TryGetProperty(name, out var value)
            || value.ValueKind == JsonValueKind.Null
            || value.ValueKind == kind;
    }

    private static bool IsValidUsage(JsonElement root)
    {
        if (!root.TryGetProperty("usage", out var usage) || usage.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        return usage.ValueKind == JsonValueKind.Object
            && Nullish(usage, "prompt_tokens", JsonValueKind.Number)
            && Nullish(usage, "completion_tokens", JsonValueKind.Number)
            && Nullish(usage, "prompt_cache_hit_tokens", JsonValueKind.Number)
            && Nullish(usage, "prompt_cache_miss_tokens", JsonValueKind.Number)
            && Nullish(usage, "total_tokens", JsonValueKind.Number)
            && IsValidDetails(usage, "prompt_tokens_details", "cached_tokens")
            && IsValidDetails(usage, "completion_tokens_details", "reasoning_tokens");
    }

    private static bool IsValidDetails(JsonElement usage, string name, string field)
    {
        if (!usage.TryGetProperty(name, out var details) || details.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        return details.ValueKind == JsonValueKind.Object
            && Nullish(details, field, JsonValueKind.Number);
    }

    private static bool IsValidLogprobs(JsonElement choice)
    {
        if (
            !choice.TryGetProperty("logprobs", out var logprobs)
            || logprobs.ValueKind == JsonValueKind.Null
        )
        {
            return true;
        }

        return logprobs.ValueKind == JsonValueKind.Object
            && Nullish(logprobs, "content", JsonValueKind.Array)
            && Nullish(logprobs, "reasoning_content", JsonValueKind.Array);
    }

    private static bool IsValidToolCalls(JsonElement owner, bool requireIndex)
    {
        if (
            !owner.TryGetProperty("tool_calls", out var toolCalls)
            || toolCalls.ValueKind == JsonValueKind.Null
        )
        {
            return true;
        }

        if (toolCalls.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var toolCall in toolCalls.EnumerateArray())
        {
            if (
                toolCall.ValueKind != JsonValueKind.Object
                || !Nullish(toolCall, "id", JsonValueKind.String)
                || !Nullish(toolCall, "type", JsonValueKind.String)
                || !toolCall.TryGetProperty("function", out var function)
                || function.ValueKind != JsonValueKind.Object
            )
            {
                return false;
            }

            if (requireIndex)
            {
                if (
                    !toolCall.TryGetProperty("index", out var index)
                    || index.ValueKind != JsonValueKind.Number
                    || !Nullish(function, "name", JsonValueKind.String)
                    || !Nullish(function, "arguments", JsonValueKind.String)
                )
                {
                    return false;
                }
            }
            else if (
                !function.TryGetProperty("name", out var name)
                || name.ValueKind != JsonValueKind.String
                || !function.TryGetProperty("arguments", out var arguments)
                || arguments.ValueKind != JsonValueKind.String
            )
            {
                return false;
            }
        }

        return true;
    }
}
