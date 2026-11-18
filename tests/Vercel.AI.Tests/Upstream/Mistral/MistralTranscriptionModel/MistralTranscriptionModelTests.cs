// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Mistral.MistralTranscriptionModel.MistralTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralTranscriptionModel;

/// <summary>Port of <c>mistral-transcription-model.test.ts</c> &gt; <c>MistralTranscriptionModel</c>.</summary>
public sealed class MistralTranscriptionModelTests
{
    private const string Id =
        "packages/mistral/src/mistral-transcription-model.test.ts::MistralTranscriptionModel::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_expose_correct_provider_and_model_information()
    {
        var model = Transcription(CreateProvider(JsonHandler()));

        Assert.Equal("mistral.transcription", model.Provider);
        Assert.Equal(ModelId, model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        Id + "should create transcription models through both provider factories",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The port has one factory, TranscriptionModel; transcription and transcriptionModel are one method in C#."
    )]
    public void Should_create_transcription_models_through_both_provider_factories()
    {
        var provider = CreateProvider(JsonHandler());

        Assert.IsType<Vercel.AI.Mistral.MistralTranscriptionModel>(
            provider.TranscriptionModel(ModelId)
        );
    }
}
