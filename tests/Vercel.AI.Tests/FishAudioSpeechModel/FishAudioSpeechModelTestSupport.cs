// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using FishAudioProviderType = Vercel.AI.FishAudio.FishAudioProvider;

namespace Vercel.AI.Tests.FishAudioSpeechModel;

/// <summary>Maps to the module-level provider, <c>prepareAudioResponse</c> and the test server of <c>fish-audio-speech-model.test.ts</c>.</summary>
internal static class FishAudioSpeechModelTestSupport
{
    internal static (
        FishAudioProviderType.FishSpeechModel Model,
        ParityHandler Handler
    ) SpeechModel(string modelId = "s1")
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3"));
        var provider = FishAudioProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        return (new FishAudioProviderType.FishSpeechModel(provider, modelId), handler);
    }

    internal static SpeechModelCall SpeechCall(
        string? voice = null,
        string? outputFormat = null,
        string? instructions = null,
        double? speed = null,
        string? language = null,
        string providerOptions = "{}"
    )
    {
        return new SpeechModelCall(
            "Hello, world!",
            voice,
            outputFormat,
            instructions,
            speed,
            language,
            Json(providerOptions),
            new Dictionary<string, string>(),
            CancellationToken.None
        );
    }

    internal static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    internal static JsonElement Body(ParityHandler handler)
    {
        return Json(handler.Calls[0].Text);
    }

    internal static void AssertWarnings(
        IReadOnlyList<OperationWarning> warnings,
        params (string Feature, string Details)[] expected
    )
    {
        Assert.Equal(expected.Length, warnings.Count);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal("unsupported", warnings[i].Type);
            Assert.Equal(expected[i].Feature, warnings[i].Feature);
            Assert.Equal(expected[i].Details, warnings[i].Details);
        }
    }
}
