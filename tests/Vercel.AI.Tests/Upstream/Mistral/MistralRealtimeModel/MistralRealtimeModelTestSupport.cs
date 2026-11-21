// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralRealtimeModel;

/// <summary>Shared fixtures for the Mistral realtime tests. Mirrors the top of <c>mistral-realtime-model.test.ts</c>.</summary>
internal static class MistralRealtimeModelTestSupport
{
    public const string Url = "https://api.mistral.ai/v1/audio/sessions";
    public const string SessionId = "session-1";
    public const string Commit = "0912e0b1b937418b438344ff2098a57961a83787";

    public static ParityHandler SessionHandler()
    {
        return new ParityHandler(call =>
            call.Method == HttpMethod.Delete
                ? new HttpResponseMessage(System.Net.HttpStatusCode.NoContent)
                : ParityHandler.Json(
                    "{\"id\":\""
                        + SessionId
                        + "\",\"participant_token\":\"participant-token\",\"server_url\":\"wss://voice.example.com\",\"unused\":\"not returned\"}"
                )
        );
    }

    public static MistralProvider CreateProvider(
        ParityHandler handler,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return MistralProvider.Create(options, handler);
    }
}
