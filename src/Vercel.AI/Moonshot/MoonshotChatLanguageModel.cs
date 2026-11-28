// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Prompt;

namespace Vercel.AI.Moonshot;

/// <summary>
/// Stream error helpers of the Moonshot AI chat model. Map to <c>createMoonshotAIStreamError</c> and
/// <c>getMoonshotAIStreamErrorMetadata</c> in <c>moonshotai-chat-language-model.ts</c>.
/// </summary>
internal static class MoonshotChatLanguageModel
{
    /// <summary>Maps to <c>createMoonshotAIStreamError</c>. Returns null when the chunk has no error message.</summary>
    public static ProviderStreamError? CreateMoonshotAIStreamError(JsonObject data)
    {
        if (
            data["error"] is not JsonObject error
            || error["message"] is not JsonValue message
            || !message.TryGetValue<string>(out var text)
        )
        {
            return null;
        }

        var type =
            error["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var t)
                ? t
                : null;
        var code =
            error["code"] is JsonValue codeValue && codeValue.TryGetValue<string>(out var c)
                ? c
                : null;
        var (statusCode, isRetryable) = GetMoonshotAIStreamErrorMetadata(type);
        return new ProviderStreamError(text, data, type, code, statusCode, isRetryable);
    }

    /// <summary>Maps to <c>getMoonshotAIStreamErrorMetadata</c>.</summary>
    public static (int? StatusCode, bool? IsRetryable) GetMoonshotAIStreamErrorMetadata(
        string? type
    )
    {
        return type switch
        {
            "rate_limit_exceeded" or "rate_limit_error" => (429, true),
            "server_error" or "api_error" or "internal_server_error" => (500, true),
            "overloaded_error" or "service_unavailable" => (503, true),
            "timeout" or "timeout_error" => (504, true),
            "authentication_error" or "invalid_api_key" => (401, false),
            "permission_error" => (403, false),
            "not_found_error" or "model_not_found" => (404, false),
            "bad_request" or "context_length_exceeded" or "invalid_request_error" => (400, false),
            _ => (null, null),
        };
    }
}
