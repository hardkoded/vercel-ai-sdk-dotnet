// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Prompt;

namespace Vercel.AI.Groq;

/// <summary>
/// The Groq chat language model helpers. The shared OpenAI-compatible chat model runs the call, and this class holds the
/// parts <c>groq-chat-language-model.ts</c> owns itself.
/// </summary>
internal static class GroqChatLanguageModel
{
    /// <summary>Maps a streamed <c>error</c> chunk to a provider stream error. Maps to <c>createGroqStreamError</c>.</summary>
    public static ProviderStreamError? CreateGroqStreamError(JsonObject data)
    {
        if (
            data["error"] is not JsonObject error
            || error["message"] is not JsonValue message
            || !message.TryGetValue<string>(out var text)
            || error["type"] is not JsonValue type
            || !type.TryGetValue<string>(out var typeText)
        )
        {
            return null;
        }

        var (statusCode, isRetryable) = GetGroqStreamErrorMetadata(typeText);
        return new ProviderStreamError(text, data, typeText, null, statusCode, isRetryable);
    }

    /// <summary>Maps to <c>getGroqStreamErrorMetadata</c>.</summary>
    public static (int? StatusCode, bool? IsRetryable) GetGroqStreamErrorMetadata(string type)
    {
        return type switch
        {
            "rate_limit_error" => (429, true),
            "api_error" or "internal_server_error" or "server_error" => (500, true),
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
