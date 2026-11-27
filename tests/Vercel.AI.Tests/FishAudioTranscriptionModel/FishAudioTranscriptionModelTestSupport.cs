// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using FishAudioProviderType = Vercel.AI.FishAudio.FishAudioProvider;

namespace Vercel.AI.Tests.FishAudioTranscriptionModel;

/// <summary>Maps to the module-level provider, <c>transcriptionResponse</c> and <c>prepareJsonResponse</c> of <c>fish-audio-transcription-model.test.ts</c>.</summary>
internal static class FishAudioTranscriptionModelTestSupport
{
    internal const string TranscriptionJson =
        "{\"language\":\"English\",\"language_code\":\"en\",\"text\":\"Hello, world!\",\"duration\":2.5,\"segments\":[{\"text\":\"Hello,\",\"start\":0,\"end\":1.2},{\"text\":\"world!\",\"start\":1.2,\"end\":2.5}]}";

    internal static readonly byte[] AudioData = { 0, 1, 2, 3, 4 };

    internal static (
        FishAudioProviderType.FishTranscriptionModel Model,
        ParityHandler Handler
    ) TranscriptionModel(string response = TranscriptionJson)
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(response));
        var provider = FishAudioProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        return (
            new FishAudioProviderType.FishTranscriptionModel(provider, "transcribe-1"),
            handler
        );
    }

    internal static TranscriptionModelCall TranscriptionCall(string providerOptions = "{}")
    {
        return new TranscriptionModelCall(
            AudioData,
            "audio/mpeg",
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

    internal static string? FormValue(ParityCall call, string name)
    {
        var match = Regex.Match(
            call.Text,
            "name=" + Regex.Escape(name) + "\r\n(?:[^\r\n]+\r\n)*\r\n([^\r\n]*)\r\n",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1)
        );
        return match.Success ? match.Groups[1].Value : null;
    }
}
