// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.MiniMax;

/// <summary>Port of <c>minimaxVideoFailedResponseHandler</c> in <c>minimax-video-model.ts</c>.</summary>
internal static class MiniMaxVideoError
{
    private const string DefaultMessage = "MiniMax video generation error";

    /// <summary>Reduces the MiniMax error envelope to <c>error.message</c>, like upstream's <c>errorToMessage</c>.</summary>
    public static ApiException FailedResponseHandler(int statusCode, string body, string url)
    {
        return ProviderHttp.MapStatusWithHeaders(statusCode, ErrorToMessage(body), body, null, url);
    }

    private static string ErrorToMessage(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (
                document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String
                && message.GetString() is { } text
            )
            {
                return text;
            }
        }
        catch (JsonException)
        {
            // An unparsable body falls back to the generic message.
        }

        return DefaultMessage;
    }
}

/// <summary>An <c>AISDKError</c> of <c>minimax-video-model.ts</c>. <see cref="Name"/> is upstream's <c>name</c> (for example <c>MINIMAX_VIDEO_GENERATION_FAILED</c>).</summary>
internal sealed class MiniMaxVideoGenerationException : AiSdkException
{
    public MiniMaxVideoGenerationException(string name, string message)
        : base(message)
    {
        Name = name;
    }

    public string Name { get; }
}
