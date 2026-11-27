// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.ResolveModel;

namespace Vercel.AI.Tests;

/// <summary>Maps to <c>describe('string model ids')</c> in <c>custom-provider.test.ts</c>.</summary>
[Collection(GlobalProviderCollection.Name)]
public sealed class StringModelIdsTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/registry/custom-provider.test.ts::string model ids::should resolve string model ids through the global default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_resolve_string_model_ids_through_the_global_default_provider()
    {
        var languageModel = new TestLanguageModel("language");
        var embeddingModel = new FakeEmbeddingModel("fake", "embedding");
        var imageModel = new FakeImageModel("fake", "image");
        var transcriptionModel = new FakeTranscriptionModel("fake", "transcription");
        var speechModel = new FakeSpeechModel("fake", "speech");
        var rerankingModel = new FakeRerankingModel("fake", "reranking");
        var videoModel = new FakeVideoModel("fake", "video");
        using var scope = new GlobalProviderScope(
            new CustomProvider()
                .AddLanguageModel("language", languageModel)
                .AddEmbeddingModel("embedding", embeddingModel)
                .AddImageModel("image", imageModel)
                .AddTranscriptionModel("transcription", transcriptionModel)
                .AddSpeechModel("speech", speechModel)
                .AddRerankingModel("reranking", rerankingModel)
                .AddVideoModel("video", videoModel)
        );

        var provider = new CustomProvider()
            .AddLanguageModel("alias", "language")
            .AddEmbeddingModel("alias", "embedding")
            .AddImageModel("alias", "image")
            .AddTranscriptionModel("alias", "transcription")
            .AddSpeechModel("alias", "speech")
            .AddRerankingModel("alias", "reranking")
            .AddVideoModel("alias", "video");

        Assert.Same(languageModel, provider.LanguageModel("alias"));
        Assert.Same(embeddingModel, provider.EmbeddingModel("alias"));
        Assert.Same(imageModel, provider.ImageModel("alias"));
        Assert.Same(transcriptionModel, provider.TranscriptionModel("alias"));
        Assert.Same(speechModel, provider.SpeechModel("alias"));
        Assert.Same(rerankingModel, provider.RerankingModel("alias"));
        Assert.Same(videoModel, provider.VideoModel("alias"));
    }
}
