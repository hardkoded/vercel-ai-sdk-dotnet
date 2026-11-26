// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>Wraps the models of a provider with middleware. Maps to <c>wrapProvider</c>.</summary>
public static class WrapProvider
{
    /// <summary>
    /// Returns a provider whose language models are wrapped with <paramref name="languageModelMiddleware"/> and, when given,
    /// whose image models are wrapped with <paramref name="imageModelMiddleware"/>. The first middleware transforms the input first.
    /// Every other model and store comes from <paramref name="provider"/>.
    /// </summary>
    public static ProviderBase Wrap(
        ProviderBase provider,
        IEnumerable<ILanguageModelMiddleware> languageModelMiddleware,
        IEnumerable<IImageModelMiddleware>? imageModelMiddleware = null
    )
    {
        return new WrappedProvider(
            provider ?? throw new ArgumentNullException(nameof(provider)),
            (
                languageModelMiddleware
                ?? throw new ArgumentNullException(nameof(languageModelMiddleware))
            ).ToArray(),
            imageModelMiddleware?.ToArray()
        );
    }

    private sealed class WrappedProvider(
        ProviderBase inner,
        ILanguageModelMiddleware[] languageModelMiddleware,
        IImageModelMiddleware[]? imageModelMiddleware
    ) : ProviderBase(inner.Name)
    {
        public override string SpecificationVersion => inner.SpecificationVersion;

        public override ILanguageModel LanguageModel(string modelId) =>
            inner.LanguageModel(modelId).WrapLanguageModel(languageModelMiddleware);

        public override IImageModel ImageModel(string modelId)
        {
            var model = inner.ImageModel(modelId);
            if (imageModelMiddleware is null)
            {
                return model;
            }

            return model is IImageCaller caller
                ? (IImageModel)caller.WrapImageModel(imageModelMiddleware)
                : model;
        }

        public override IEmbeddingModel EmbeddingModel(string modelId) =>
            inner.EmbeddingModel(modelId);

        public override ISpeechModel SpeechModel(string modelId) => inner.SpeechModel(modelId);

        public override ITranscriptionModel TranscriptionModel(string modelId) =>
            inner.TranscriptionModel(modelId);

        public override ISpeechTranslationModel SpeechTranslationModel(string modelId) =>
            inner.SpeechTranslationModel(modelId);

        public override IVideoModel VideoModel(string modelId) => inner.VideoModel(modelId);

        public override IRerankingModel RerankingModel(string modelId) =>
            inner.RerankingModel(modelId);

        public override IRubricModel RubricModel(string modelId) => inner.RubricModel(modelId);

        public override IBatchModel BatchModel() => inner.BatchModel();

        public override Provider.IFileStore FileStore() => inner.FileStore();

        public override Provider.ISkillStore SkillStore() => inner.SkillStore();

        public override IRealtimeModel RealtimeModel(string modelId) =>
            inner.RealtimeModel(modelId);
    }
}
