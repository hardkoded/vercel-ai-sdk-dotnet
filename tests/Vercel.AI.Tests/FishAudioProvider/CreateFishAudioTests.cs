// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using FishAudioProviderType = Vercel.AI.FishAudio.FishAudioProvider;

namespace Vercel.AI.Tests.FishAudioProvider;

public sealed class CreateFishAudioTests
{
    private const string Prefix =
        "packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::";

    [Fact]
    [UpstreamTest(
        Prefix + "should create speech models",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks Speech and SpeechModel. The callable form provider('s1').speech has no .NET counterpart."
    )]
    public void Should_create_speech_models()
    {
        var provider = FishAudioProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" }
        );
        Assert.IsType<FishAudioProviderType.FishSpeechModel>(provider.Speech("s1"));
        Assert.IsType<FishAudioProviderType.FishSpeechModel>(provider.SpeechModel("s1"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create transcription models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_transcription_models()
    {
        var provider = FishAudioProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" }
        );
        Assert.IsType<FishAudioProviderType.FishTranscriptionModel>(provider.Transcription());
        Assert.IsType<FishAudioProviderType.FishTranscriptionModel>(
            provider.TranscriptionModel("transcribe-1")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default the transcription model id to `transcribe-1`",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_default_the_transcription_model_id_to_transcribe_1()
    {
        var provider = FishAudioProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" }
        );
        Assert.Equal("transcribe-1", provider.Transcription().ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the speech model id and provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_expose_the_speech_model_id_and_provider()
    {
        var model = (FishAudioProviderType.FishSpeechModel)
            FishAudioProviderType
                .Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" })
                .Speech("s2-pro");
        Assert.Equal("s2-pro", model.ModelId);
        Assert.Equal("fish-audio.speech", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the transcription model provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_expose_the_transcription_model_provider()
    {
        var model = (FishAudioProviderType.FishTranscriptionModel)
            FishAudioProviderType
                .Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" })
                .Transcription();
        Assert.Equal("fish-audio.transcription", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for unsupported model types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_for_unsupported_model_types()
    {
        var provider = FishAudioProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" }
        );
        Assert.Throws<NoSuchModelError>(() => provider.LanguageModel("s1"));
        Assert.Throws<NoSuchModelError>(() => provider.EmbeddingModel("s1"));
        Assert.Throws<NoSuchModelError>(() => provider.ImageModel("s1"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report specification version v4",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_report_specification_version_v4()
    {
        Assert.Equal(
            "v4",
            FishAudioProviderType
                .Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" })
                .SpecificationVersion
        );
    }
}
