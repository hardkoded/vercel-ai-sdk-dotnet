// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

/// <summary>Provider registry and custom provider lookups matched to the upstream catalog.</summary>
public sealed class RegistryUpstreamTests
{
    private const string Registry = "packages/ai/src/registry/provider-registry.test.ts::";
    private const string Custom = "packages/ai/src/registry/custom-provider.test.ts::";
    private const string Evaluation = "packages/ai/src/registry/evaluation-model.test.ts::";

    [Theory]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [InlineData("transcriptionModel")]
    [InlineData("speechModel")]
    [InlineData("rerankingModel")]
    [InlineData("videoModel")]
    [UpstreamTest(Registry + "embeddingModel::should return embedding model from provider using embeddingModel", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "imageModel::should return image model from provider", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "transcriptionModel::should return transcription model from provider", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "speechModel::should return speech model from provider", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "rerankingModel::should return reranking model from provider using rerankingModel", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "videoModel::should return video model from provider", Coverage = UpstreamCoverage.Covered)]
    public void Registry_returns_the_provider_model(string modelType)
    {
        var model = new FakeModel("model");
        var provider = new StubProvider(model);
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        Assert.Same(model, Resolve(registry, modelType, "provider:model"));
        Assert.Equal(new[] { "model" }, provider.Requests);
    }

    [Fact]
    [UpstreamTest(Registry + "languageModel::should return language model with additional colon from provider", Coverage = UpstreamCoverage.Covered)]
    public void Registry_keeps_colons_after_the_first_in_the_model_id()
    {
        var model = new TestLanguageModel();
        var provider = new StubProvider(model);
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        Assert.Same(model, registry.LanguageModel("provider:model:part2"));
        Assert.Equal(new[] { "model:part2" }, provider.Requests);
    }

    [Theory]
    [InlineData("languageModel")]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [InlineData("transcriptionModel")]
    [InlineData("speechModel")]
    [InlineData("rerankingModel")]
    [InlineData("videoModel")]
    [UpstreamTest(Registry + "languageModel::should throw NoSuchProviderError if provider does not exist", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "embeddingModel::should throw NoSuchProviderError if provider does not exist", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "imageModel::should throw NoSuchProviderError if provider does not exist", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "transcriptionModel::should throw NoSuchProviderError if provider does not exist", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "speechModel::should throw NoSuchProviderError if provider does not exist", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "rerankingModel::should throw NoSuchProviderError if provider does not exist", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "videoModel::should throw NoSuchProviderError if provider does not exist", Coverage = UpstreamCoverage.Covered)]
    public void Registry_rejects_unknown_providers(string modelType)
    {
        var error = Assert.Throws<NoSuchProviderError>(() => Resolve(new ProviderRegistry(), modelType, "provider:model:part2"));
        Assert.Equal("provider", error.ProviderId);
        Assert.Equal(modelType, error.ModelType);
    }

    [Theory]
    [InlineData("languageModel")]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [InlineData("transcriptionModel")]
    [InlineData("speechModel")]
    [InlineData("rerankingModel")]
    [InlineData("videoModel")]
    [UpstreamTest(Registry + "languageModel::should throw NoSuchModelError if provider does not return a model", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "embeddingModel::should throw NoSuchModelError if provider does not return a model", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "imageModel::should throw NoSuchModelError if provider does not return a model", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "transcriptionModel::should throw NoSuchModelError if provider does not return a model", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "speechModel::should throw NoSuchModelError if provider does not return a model", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "rerankingModel::should throw NoSuchModelError if provider does not return a model", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "videoModel::should throw NoSuchModelError if provider does not return a model", Coverage = UpstreamCoverage.Covered)]
    public void Registry_reports_a_provider_without_the_model(string modelType)
    {
        var registry = new ProviderRegistry();
        registry.Register("provider", new StubProvider(null));
        var error = Assert.Throws<NoSuchModelError>(() => Resolve(registry, modelType, "provider:model"));
        Assert.Equal("provider:model", error.ModelId);
        Assert.Equal("No such " + modelType + ": provider:model", error.Message);
    }

    [Theory]
    [InlineData("languageModel")]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [InlineData("transcriptionModel")]
    [InlineData("speechModel")]
    [InlineData("rerankingModel")]
    [InlineData("videoModel")]
    [UpstreamTest(Registry + "languageModel::should throw NoSuchModelError if model id doesn't contain a colon", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "embeddingModel::should throw NoSuchModelError if model id doesn't contain a colon", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "imageModel::should throw NoSuchModelError if model id doesn't contain a colon", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "transcriptionModel::should throw NoSuchModelError if model id doesn't contain a colon", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "speechModel::should throw NoSuchModelError if model id doesn't contain a colon", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "rerankingModel::should throw NoSuchModelError if model id doesn't contain a colon", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "videoModel::should throw NoSuchModelError if model id doesn't contain a colon", Coverage = UpstreamCoverage.Covered)]
    public void Registry_rejects_ids_without_a_separator(string modelType)
    {
        var error = Assert.Throws<NoSuchModelError>(() => Resolve(new ProviderRegistry(), modelType, "model"));
        Assert.False(NoSuchProviderError.IsInstance(error));
        Assert.Equal("Invalid " + modelType + " id for registry: model (must be in the format \"providerId:modelId\")", error.Message);
    }

    [Theory]
    [InlineData("languageModel", "|")]
    [InlineData("languageModel", " > ")]
    [InlineData("embeddingModel", "|")]
    [InlineData("imageModel", "|")]
    [InlineData("rerankingModel", "|")]
    [InlineData("videoModel", "|")]
    [UpstreamTest(Registry + "languageModel::should support custom separator", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "languageModel::should support custom separator with multiple characters", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "embeddingModel::should support custom separator", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "imageModel::should support custom separator", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "rerankingModel::should support custom separator", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Registry + "videoModel::should support custom separator", Coverage = UpstreamCoverage.Covered)]
    public void Registry_splits_on_a_custom_separator(string modelType, string separator)
    {
        object model = modelType == "languageModel" ? new TestLanguageModel() : new FakeModel("model");
        var provider = new StubProvider(model);
        var registry = new ProviderRegistry(separator);
        registry.Register("provider", provider);
        Assert.Same(model, Resolve(registry, modelType, "provider" + separator + "model"));
        Assert.Equal(new[] { "model" }, provider.Requests);
    }

    [Fact]
    [UpstreamTest(Registry + "files and skills::should return files interface from provider", Coverage = UpstreamCoverage.Covered)]
    public void Registry_returns_the_provider_file_store()
    {
        var files = new FakeStore();
        var registry = new ProviderRegistry();
        registry.Register("provider", new StubProvider(null) { Files = files });
        Assert.Same(files, registry.FileStore("provider"));
    }

    [Fact]
    [UpstreamTest(Registry + "files and skills::should return skills interface from provider", Coverage = UpstreamCoverage.Covered)]
    public void Registry_returns_the_provider_skill_store()
    {
        var skills = new FakeStore();
        var registry = new ProviderRegistry();
        registry.Register("provider", new StubProvider(null) { Skills = skills });
        Assert.Same(skills, registry.SkillStore("provider"));
    }

    [Fact]
    [UpstreamTest(Registry + "files and skills::should throw when provider does not expose files", Coverage = UpstreamCoverage.Partial, Note = "The provider's own error is thrown. It names the provider but has no files() hint.")]
    public void Registry_file_store_fails_when_the_provider_has_none()
    {
        var registry = new ProviderRegistry();
        registry.Register("provider", new StubProvider(null));
        Assert.Contains("does not support file uploads", Assert.Throws<AiSdkException>(() => registry.FileStore("provider")).Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Registry + "files and skills::should throw when provider does not expose skills", Coverage = UpstreamCoverage.Partial, Note = "The provider's own error is thrown. It names the provider but has no skills() hint.")]
    public void Registry_skill_store_fails_when_the_provider_has_none()
    {
        var registry = new ProviderRegistry();
        registry.Register("provider", new StubProvider(null));
        Assert.Contains("does not support skill uploads", Assert.Throws<AiSdkException>(() => registry.SkillStore("provider")).Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Registry + "middleware functionality::should wrap all language models accessed through the provider registry", Coverage = UpstreamCoverage.Partial, Note = "Middleware has no overrideModelId. Checks that each model's generate call runs through the middleware.")]
    public async Task Registry_wraps_every_language_model_with_its_middleware()
    {
        var models = new[] { new TestLanguageModel("model-1"), new TestLanguageModel("model-2"), new TestLanguageModel("model-3") };
        var middleware = new RecordingMiddleware();
        var registry = new ProviderRegistry { LanguageModelMiddleware = new[] { middleware } };
        registry.Register("provider1", new CustomProvider().AddLanguageModel(models[0]).AddLanguageModel(models[1]));
        registry.Register("provider2", new CustomProvider().AddLanguageModel(models[2]));
        foreach (var id in new[] { "provider1:model-1", "provider1:model-2", "provider2:model-3" })
        {
            var wrapped = registry.LanguageModel(id);
            Assert.NotSame(models.Single(model => id.EndsWith(model.ModelId, StringComparison.Ordinal)), wrapped);
            await wrapped.DoGenerateAsync(new LanguageModelCallOptions(), CancellationToken.None).ConfigureAwait(false);
        }

        Assert.Equal(3, middleware.Calls);
        Assert.All(models, model => Assert.Single(model.Calls));
    }

    [Theory]
    [InlineData("languageModel")]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [InlineData("transcriptionModel")]
    [InlineData("speechModel")]
    [InlineData("rerankingModel")]
    [InlineData("videoModel")]
    [UpstreamTest(Custom + "languageModel::should return the language model if it exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "embeddingModel::should return the embedding model if it exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "imageModel::should return the image model if it exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "transcriptionModel::should return the transcription model if it exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "speechModel::should return the speech model if it exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "rerankingModel::should return the reranking model if it exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "videoModel::should return the video model if it exists", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_returns_a_registered_model(string modelType)
    {
        var provider = new CustomProvider();
        var model = Add(provider, modelType, "test-model");
        Assert.Same(model, Resolve(provider, modelType, "test-model"));
    }

    [Theory]
    [InlineData("languageModel")]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [InlineData("transcriptionModel")]
    [InlineData("speechModel")]
    [InlineData("rerankingModel")]
    [InlineData("videoModel")]
    [UpstreamTest(Custom + "languageModel::should use fallback provider if model not found and fallback exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "embeddingModel::should use fallback provider if model not found and fallback exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "imageModel::should use fallback provider if model not found and fallback exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "transcriptionModel::should use fallback provider if model not found and fallback exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "speechModel::should use fallback provider if model not found and fallback exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "rerankingModel::should use fallback provider if model not found and fallback exists", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "videoModel::should use fallback provider if model not found and fallback exists", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_asks_the_fallback_for_unknown_ids(string modelType)
    {
        object model = modelType == "languageModel" ? new TestLanguageModel() : new FakeModel("test-model");
        var fallback = new StubProvider(model);
        var provider = new CustomProvider { FallbackProvider = fallback };
        Assert.Same(model, Resolve(provider, modelType, "test-model"));
        Assert.Equal(new[] { "test-model" }, fallback.Requests);
    }

    [Theory]
    [InlineData("languageModel")]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [InlineData("transcriptionModel")]
    [InlineData("speechModel")]
    [InlineData("rerankingModel")]
    [InlineData("videoModel")]
    [UpstreamTest(Custom + "languageModel::should throw NoSuchModelError if model not found and no fallback", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "embeddingModel::should throw NoSuchModelError if model not found and no fallback", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "imageModel::should throw NoSuchModelError if model not found and no fallback", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "transcriptionModel::should throw NoSuchModelError if model not found and no fallback", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "speechModel::should throw NoSuchModelError if model not found and no fallback", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "rerankingModel::should throw NoSuchModelError if model not found and no fallback", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Custom + "videoModel::should throw NoSuchModelError if model not found and no fallback", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_without_fallback_reports_unknown_ids(string modelType)
    {
        var error = Assert.Throws<NoSuchModelError>(() => Resolve(new CustomProvider(), modelType, "test-model"));
        Assert.Equal("test-model", error.ModelId);
        Assert.Equal(modelType, error.ModelType);
    }

    [Fact]
    [UpstreamTest(Custom + "files::should return the files interface if it exists", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_returns_its_file_store()
    {
        var files = new FakeStore();
        Assert.Same(files, new CustomProvider { Files = files }.FileStore());
    }

    [Fact]
    [UpstreamTest(Custom + "files::should use fallback provider files if files is not configured and fallback exists", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_uses_the_fallback_file_store()
    {
        var files = new FakeStore();
        Assert.Same(files, new CustomProvider { FallbackProvider = new StubProvider(null) { Files = files } }.FileStore());
    }

    [Fact]
    [UpstreamTest(Custom + "files::should not expose files if files is not configured and fallback does not support files", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_without_files_has_no_file_store()
    {
        Assert.Throws<AiSdkException>(() => new CustomProvider().FileStore());
    }

    [Fact]
    [UpstreamTest(Custom + "skills::should return the skills interface if it exists", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_returns_its_skill_store()
    {
        var skills = new FakeStore();
        Assert.Same(skills, new CustomProvider { Skills = skills }.SkillStore());
    }

    [Fact]
    [UpstreamTest(Custom + "skills::should use fallback provider skills if skills is not configured and fallback exists", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_uses_the_fallback_skill_store()
    {
        var skills = new FakeStore();
        Assert.Same(skills, new CustomProvider { FallbackProvider = new StubProvider(null) { Skills = skills } }.SkillStore());
    }

    [Fact]
    [UpstreamTest(Custom + "skills::should not expose skills if skills is not configured and fallback does not support skills", Coverage = UpstreamCoverage.Covered)]
    public void Custom_provider_without_skills_has_no_skill_store()
    {
        Assert.Throws<AiSdkException>(() => new CustomProvider().SkillStore());
    }

    [Fact]
    [UpstreamTest(Evaluation + "custom evaluation models::resolves an alias before consulting the fallback", Coverage = UpstreamCoverage.Covered)]
    public void Custom_evaluation_alias_wins_over_the_fallback()
    {
        var model = new FakeModel("model");
        var fallback = new StubProvider(new FakeModel("other"));
        var provider = new CustomProvider { FallbackProvider = fallback }.AddEvaluationModel("alias", model);
        Assert.Same(model, provider.EvaluationModel("alias"));
        Assert.Empty(fallback.Requests);
    }

    [Fact]
    [UpstreamTest(Evaluation + "custom evaluation models::preserves the fallback receiver", Coverage = UpstreamCoverage.Covered)]
    public void Custom_evaluation_uses_the_fallback_provider()
    {
        var model = new FakeModel("model:version");
        var fallback = new StubProvider(model);
        Assert.Same(model, new CustomProvider { FallbackProvider = fallback }.EvaluationModel("model:version"));
        Assert.Equal(new[] { "model:version" }, fallback.Requests);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("toString")]
    [InlineData("constructor")]
    [InlineData("__proto__")]
    [UpstreamTest(Evaluation + "custom evaluation models::reports missing alias %s without treating inherited properties as models", Coverage = UpstreamCoverage.Covered)]
    public void Custom_evaluation_reports_missing_aliases(string modelId)
    {
        var provider = new CustomProvider().AddEvaluationModel("known", new FakeModel("known"));
        var error = Assert.Throws<NoSuchModelError>(() => provider.EvaluationModel(modelId));
        Assert.Equal("No such evaluationModel: " + modelId, error.Message);
    }

    [Fact]
    [UpstreamTest(Evaluation + "custom evaluation models::reports an unavailable fallback model", Coverage = UpstreamCoverage.Covered)]
    public void Custom_evaluation_reports_a_fallback_without_the_model()
    {
        var provider = new CustomProvider { FallbackProvider = new StubProvider(null) };
        var error = Assert.Throws<NoSuchModelError>(() => provider.EvaluationModel("missing"));
        Assert.Equal("No such evaluationModel: missing", error.Message);
    }

    [Fact]
    [UpstreamTest(Evaluation + "evaluation registry::preserves model identity, receiver, and colons inside model IDs", Coverage = UpstreamCoverage.Covered)]
    public void Registry_evaluation_keeps_model_identity_and_colons()
    {
        var model = new FakeModel("model:version");
        var provider = new StubProvider(model);
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        Assert.Same(model, registry.EvaluationModel("provider:model:version"));
        Assert.Equal(new[] { "model:version" }, provider.Requests);
    }

    [Fact]
    [UpstreamTest(Evaluation + "evaluation registry::preserves legacy provider extensions and custom separators", Coverage = UpstreamCoverage.Partial, Note = "Checks the multi-character separator. .NET has no ProviderV3 to adapt.")]
    public void Registry_evaluation_splits_on_a_custom_separator()
    {
        var model = new FakeModel("model::version");
        var provider = new StubProvider(model);
        var registry = new ProviderRegistry("::");
        registry.Register("provider", provider);
        Assert.Same(model, registry.EvaluationModel("provider::model::version"));
        Assert.Equal(new[] { "model::version" }, provider.Requests);
    }

    [Fact]
    [UpstreamTest(Evaluation + "evaluation registry::identifies unknown providers with the existing marker-based error", Coverage = UpstreamCoverage.Covered)]
    public void Registry_evaluation_marks_unknown_providers()
    {
        var registry = new ProviderRegistry();
        registry.Register("known", new StubProvider(null));
        var error = Assert.Throws<NoSuchProviderError>(() => registry.EvaluationModel("missing:model"));
        Assert.True(NoSuchProviderError.IsInstance(error));
        Assert.True(NoSuchModelError.IsInstance(error));
        Assert.Equal("missing", error.ProviderId);
        Assert.Equal("evaluationModel", error.ModelType);
        Assert.Equal(new[] { "known" }, error.AvailableProviders);
    }

    [Fact]
    [UpstreamTest(Evaluation + "evaluation registry::reports malformed IDs", Coverage = UpstreamCoverage.Covered)]
    public void Registry_evaluation_reports_malformed_ids()
    {
        Assert.Contains("must be in the format \"providerId:modelId\"", Assert.Throws<NoSuchModelError>(() => new ProviderRegistry().EvaluationModel("model")).Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Evaluation + "evaluation registry::reports missing evaluation capabilities or models", Coverage = UpstreamCoverage.Partial, Note = "Checks a provider that returns no model. A provider without evaluation support throws its own AiSdkException.")]
    public void Registry_evaluation_reports_a_provider_without_the_model()
    {
        var registry = new ProviderRegistry();
        registry.Register("provider", new StubProvider(null));
        var error = Assert.Throws<NoSuchModelError>(() => registry.EvaluationModel("provider:model"));
        Assert.Equal("No such evaluationModel: provider:model", error.Message);
    }

    private static object Resolve(ProviderRegistry registry, string modelType, string id)
    {
        return modelType switch
        {
            "languageModel" => registry.LanguageModel(id),
            "embeddingModel" => registry.EmbeddingModel(id),
            "imageModel" => registry.ImageModel(id),
            "transcriptionModel" => registry.TranscriptionModel(id),
            "speechModel" => registry.SpeechModel(id),
            "rerankingModel" => registry.RerankingModel(id),
            "videoModel" => registry.VideoModel(id),
            _ => throw new ArgumentOutOfRangeException(nameof(modelType)),
        };
    }

    private static object Resolve(ProviderBase provider, string modelType, string id)
    {
        return modelType switch
        {
            "languageModel" => provider.LanguageModel(id),
            "embeddingModel" => provider.EmbeddingModel(id),
            "imageModel" => provider.ImageModel(id),
            "transcriptionModel" => provider.TranscriptionModel(id),
            "speechModel" => provider.SpeechModel(id),
            "rerankingModel" => provider.RerankingModel(id),
            "videoModel" => provider.VideoModel(id),
            _ => throw new ArgumentOutOfRangeException(nameof(modelType)),
        };
    }

    private static object Add(CustomProvider provider, string modelType, string id)
    {
        var model = new FakeModel(id);
        switch (modelType)
        {
            case "languageModel":
                var language = new TestLanguageModel(id);
                provider.AddLanguageModel(id, language);
                return language;
            case "embeddingModel":
                provider.AddEmbeddingModel(id, model);
                break;
            case "imageModel":
                provider.AddImageModel(id, model);
                break;
            case "transcriptionModel":
                provider.AddTranscriptionModel(id, model);
                break;
            case "speechModel":
                provider.AddSpeechModel(id, model);
                break;
            case "rerankingModel":
                provider.AddRerankingModel(id, model);
                break;
            case "videoModel":
                provider.AddVideoModel(id, model);
                break;
        }

        return model;
    }

    // Returns Model for every modality it implements, or null when Model is null or of another kind.
    private sealed class StubProvider : ProviderBase
    {
        private readonly object? _model;

        public StubProvider(object? model)
            : base("stub")
        {
            _model = model;
        }

        public List<string> Requests { get; } = new();

        public IFileStore? Files { get; set; }

        public ISkillStore? Skills { get; set; }

        public override ILanguageModel LanguageModel(string modelId) => Get<ILanguageModel>(modelId)!;

        public override IEmbeddingModel EmbeddingModel(string modelId) => Get<IEmbeddingModel>(modelId)!;

        public override IImageModel ImageModel(string modelId) => Get<IImageModel>(modelId)!;

        public override ITranscriptionModel TranscriptionModel(string modelId) => Get<ITranscriptionModel>(modelId)!;

        public override ISpeechModel SpeechModel(string modelId) => Get<ISpeechModel>(modelId)!;

        public override IRerankingModel RerankingModel(string modelId) => Get<IRerankingModel>(modelId)!;

        public override IVideoModel VideoModel(string modelId) => Get<IVideoModel>(modelId)!;

        public override IEvaluationModel EvaluationModel(string modelId) => Get<IEvaluationModel>(modelId)!;

        public override IFileStore FileStore() => Files ?? base.FileStore();

        public override ISkillStore SkillStore() => Skills ?? base.SkillStore();

        private T? Get<T>(string modelId)
            where T : class
        {
            Requests.Add(modelId);
            return _model as T;
        }
    }

    private sealed class FakeModel : IEmbeddingModel, IImageModel, ITranscriptionModel, ISpeechModel, IRerankingModel, IVideoModel, IEvaluationModel
    {
        public FakeModel(string modelId)
        {
            ModelId = modelId;
        }

        public string SpecificationVersion => "V4";

        public string Provider => "fake";

        public string ModelId { get; }

        public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken, int? dimensions = null) => throw new NotSupportedException();

        public Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<EvaluationResult> DoEvaluateAsync(string rubric, string candidate, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class FakeStore : IFileStore, ISkillStore
    {
        public Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<UploadedSkill> UploadSkillAsync(string name, string instructions, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class RecordingMiddleware : LanguageModelMiddleware
    {
        public int Calls { get; private set; }

        public override Task<LanguageModelGenerateResult> WrapGenerateAsync(
            LanguageModelCallOptions options,
            Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
            CancellationToken cancellationToken)
        {
            Calls++;
            return next(options, cancellationToken);
        }
    }
}
