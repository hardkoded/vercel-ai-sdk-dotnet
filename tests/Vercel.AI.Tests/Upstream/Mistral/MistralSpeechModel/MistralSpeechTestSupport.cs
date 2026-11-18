// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralSpeechModel;

/// <summary>Shared fixtures for the Mistral speech tests. Mirrors the top of <c>mistral-speech-model.test.ts</c>.</summary>
internal static class MistralSpeechTestSupport
{
    public const string ModelId = "voxtral-mini-tts-2603";
    public const string Url = "https://api.mistral.ai/v1/audio/speech";
    public const string Commit = "486e08f0446a5986c2f6fd5bcf8c9b00770e51f7";

    public static ParityHandler AudioHandler(
        string audioData = "SUQzBAAAAAAA",
        params (string Name, string Value)[] headers
    )
    {
        return new ParityHandler(_ =>
            ParityHandler.Json("{\"audio_data\":\"" + audioData + "\"}", headers)
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

    public static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }
}
