// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.FishAudio;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Fish Audio factory behavior matched to the upstream catalog.</summary>
public sealed class FishAudioParityTests
{
    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should create speech models", Coverage = UpstreamCoverage.Covered)]
    public void Speech_and_speech_model_return_speech_models()
    {
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.IsType<FishAudioProvider.FishSpeechModel>(provider.Speech("s1"));
        Assert.IsType<FishAudioProvider.FishSpeechModel>(provider.SpeechModel("s1"));
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should create transcription models", Coverage = UpstreamCoverage.Covered)]
    public void Transcription_factories_return_transcription_models()
    {
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.IsType<FishAudioProvider.FishTranscriptionModel>(provider.Transcription());
        Assert.IsType<FishAudioProvider.FishTranscriptionModel>(provider.TranscriptionModel("transcribe-1"));
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should default the transcription model id to `transcribe-1`", Coverage = UpstreamCoverage.Covered)]
    public void Transcription_defaults_the_model_id()
    {
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.Equal("transcribe-1", provider.Transcription().ModelId);
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should expose the speech model id and provider", Coverage = UpstreamCoverage.Covered)]
    public void Speech_model_exposes_its_id_provider_and_version()
    {
        var model = (FishAudioProvider.FishSpeechModel)FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }).Speech("s2-pro");
        Assert.Equal("s2-pro", model.ModelId);
        Assert.Equal("fish-audio.speech", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should expose the transcription model provider", Coverage = UpstreamCoverage.Covered)]
    public void Transcription_model_exposes_its_provider_and_version()
    {
        var model = (FishAudioProvider.FishTranscriptionModel)FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }).Transcription();
        Assert.Equal("fish-audio.transcription", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should throw for unsupported model types", Coverage = UpstreamCoverage.Covered)]
    public void Unsupported_model_types_throw()
    {
        var provider = FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.Throws<AiSdkException>(() => provider.LanguageModel("s1"));
        Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("s1"));
        Assert.Throws<AiSdkException>(() => provider.ImageModel("s1"));
    }

    [Fact]
    [UpstreamTest("packages/fish-audio/src/fish-audio-provider.test.ts::createFishAudio::should report specification version v4", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reports_specification_version_v4()
    {
        Assert.Equal("v4", FishAudioProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }).SpecificationVersion);
    }
}
