// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Mistral.MistralSpeechModel.MistralSpeechTestSupport;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralSpeechModel;

/// <summary>Port of <c>mistral-speech-model.test.ts</c> &gt; <c>MistralSpeechModel</c>.</summary>
public sealed class MistralSpeechModelTests
{
    private const string Id =
        "packages/mistral/src/mistral-speech-model.test.ts::MistralSpeechModel::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_expose_correct_provider_and_model_information()
    {
        var model = (Vercel.AI.Operations.ISpeechCaller)
            CreateProvider(AudioHandler()).SpeechModel(ModelId);

        Assert.Equal("mistral.speech", model.Provider);
        Assert.Equal(ModelId, model.ModelId);
        Assert.Equal("v4", ((Vercel.AI.Mistral.MistralSpeechModel)model).SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        Id + "should create speech models through both provider factories",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The port has one factory, SpeechModel; speech and speechModel are one method in C#."
    )]
    public void Should_create_speech_models_through_both_provider_factories()
    {
        var provider = CreateProvider(AudioHandler());

        Assert.IsType<Vercel.AI.Mistral.MistralSpeechModel>(provider.SpeechModel(ModelId));
    }
}
