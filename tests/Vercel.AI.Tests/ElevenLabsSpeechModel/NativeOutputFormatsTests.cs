// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.ElevenLabsSpeechModel;

public sealed class NativeOutputFormatsTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-speech-model.test.ts::ElevenLabsSpeechModel > native output formats::";

    private static readonly byte[] Audio = [0xff, 0xfb, 0x90, 0x64, 1, 2, 3, 4];

    [Theory]
    [InlineData("music_v2_5", "https://api.elevenlabs.io/v1/music", "{}")]
    [InlineData("eleven_text_to_sound_v2", "https://api.elevenlabs.io/v1/sound-generation", "{}")]
    [InlineData(
        "eleven_v3",
        "https://api.elevenlabs.io/v1/text-to-dialogue",
        "{\"inputs\":[{\"text\":\"Hello\",\"voiceId\":\"voice-a\"}]}"
    )]
    [UpstreamTest(
        Prefix + "should normalize WAV for $modelId",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_normalize_WAV_for_modelId(
        string modelId,
        string url,
        string providerOptions
    )
    {
        var (model, handler) = CreateModel(modelId);

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Hello")
            {
                OutputFormat = "wav",
                ProviderOptions = JsonDocument
                    .Parse("{\"elevenlabs\":" + providerOptions + "}")
                    .RootElement.Clone(),
            },
            CancellationToken.None
        );

        Assert.Equal(url, handler.Calls[0].Uri.GetLeftPart(UriPartial.Path));
        Assert.Equal("output_format=wav_44100", handler.Calls[0].Uri.Query.TrimStart('?'));
        Assert.Equal(Audio, result.Audio);
        Assert.Equal(
            "wav_44100",
            result
                .ProviderMetadata!.Value.GetProperty("elevenlabs")
                .GetProperty("outputFormat")
                .GetString()
        );
    }

    [Theory]
    [InlineData("music_v2_5", "https://api.elevenlabs.io/v1/music", "auto", "{}")]
    [InlineData(
        "eleven_text_to_sound_v2",
        "https://api.elevenlabs.io/v1/sound-generation",
        "wav_24000",
        "{}"
    )]
    [InlineData(
        "eleven_v3",
        "https://api.elevenlabs.io/v1/text-to-dialogue",
        "wav_22050",
        "{\"inputs\":[{\"text\":\"Knock knock\",\"voiceId\":\"voice-a\"}]}"
    )]
    [UpstreamTest(
        Prefix + "should pass $outputFormat through to $modelId",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_pass_outputFormat_through_to_modelId(
        string modelId,
        string url,
        string outputFormat,
        string providerOptions
    )
    {
        var (model, handler) = CreateModel(modelId);

        var result = await model.GenerateAsync(
            new ElevenLabsSpeechRequest("Hello")
            {
                OutputFormat = outputFormat,
                ProviderOptions = JsonDocument
                    .Parse("{\"elevenlabs\":" + providerOptions + "}")
                    .RootElement.Clone(),
            },
            CancellationToken.None
        );

        Assert.Equal($"{url}?output_format={outputFormat}", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(
            outputFormat,
            result
                .ProviderMetadata!.Value.GetProperty("elevenlabs")
                .GetProperty("outputFormat")
                .GetString()
        );
    }

    private static (
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
}
