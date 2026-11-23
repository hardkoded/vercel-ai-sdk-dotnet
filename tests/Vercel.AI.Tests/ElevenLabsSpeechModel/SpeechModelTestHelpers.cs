// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ElevenLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.ElevenLabsSpeechModel;

/// <summary>Maps to <c>prepareGenerationResponse</c> and the module-level provider of <c>elevenlabs-speech-model.test.ts</c>.</summary>
internal static class SpeechModelTestHelpers
{
    /// <summary>Maps to the audio returned by <c>prepareGenerationResponse</c>.</summary>
    internal static readonly byte[] Audio = [0xff, 0xfb, 0x90, 0x64, 1, 2, 3, 4];

    internal static (
        ElevenLabsProvider.ElevenLabsSpeechModel Model,
        ParityHandler Handler
    ) CreateModel(string modelId)
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(Audio, "audio/mpeg"));
        var provider = ElevenLabsProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        return ((ElevenLabsProvider.ElevenLabsSpeechModel)provider.SpeechModel(modelId), handler);
    }

    internal static JsonElement ElevenLabsOptions(string json) =>
        JsonDocument.Parse("{\"elevenlabs\":" + json + "}").RootElement.Clone();

    internal static void AssertJsonEqual(string expected, string actual) =>
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual)),
            "Expected " + expected + " but was " + actual
        );

    internal static void AssertMetadata(string expected, ElevenLabsSpeechResult result) =>
        AssertJsonEqual(expected, result.ProviderMetadata!.Value.GetRawText());

    /// <summary>Maps to <code>result.warnings.map(warning =&gt; 'feature' in warning &amp;&amp; warning.feature)</code>.</summary>
    internal static List<string?> Features(IEnumerable<ModelWarning> warnings) =>
        warnings.Select(warning => (warning as UnsupportedWarning)?.Feature).ToList();
}
