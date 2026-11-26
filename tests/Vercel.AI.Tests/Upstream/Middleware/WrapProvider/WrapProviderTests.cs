// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Wrap = global::Vercel.AI.WrapProvider;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapProvider;

/// <summary>Port of <c>wrap-provider.test.ts</c> &gt; <c>wrapProvider</c>.</summary>
public sealed class WrapProviderTests
{
    private const string Id = "packages/ai/src/middleware/wrap-provider.test.ts::wrapProvider::";

    [Fact]
    [UpstreamTest(
        Id + "should wrap all language models in the provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_wrap_all_language_models_in_the_provider()
    {
        var provider = new MockProvider
        {
            LanguageModels =
            {
                ["model-1"] = new TestLanguageModel("model-1"),
                ["model-2"] = new TestLanguageModel("model-2"),
                ["model-3"] = new TestLanguageModel("model-3"),
            },
        };
        var seen = new List<string>();
        var overrideModelId = new RecordingLanguageMiddleware(model =>
        {
            seen.Add(model.ModelId);
            return $"override-{model.ModelId}";
        });

        var wrappedProvider = Wrap.Wrap(provider, [overrideModelId]);

        Assert.Equal("override-model-1", wrappedProvider.LanguageModel("model-1").ModelId);
        Assert.Equal("override-model-2", wrappedProvider.LanguageModel("model-2").ModelId);
        Assert.Equal("override-model-3", wrappedProvider.LanguageModel("model-3").ModelId);

        Assert.Equal(["model-1", "model-2", "model-3"], seen);
    }

    [Fact]
    [UpstreamTest(
        Id + "should work when the provider is a ProviderV2",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no V2 provider or V2 language model type (ProviderBase and ILanguageModel are always v4), so this wraps a plain provider. The V2-to-V4 adaptation is not asserted."
    )]
    public void Should_work_when_the_provider_is_a_ProviderV2()
    {
        var provider = new MockProvider
        {
            LanguageModels =
            {
                ["model-1"] = new TestLanguageModel("model-1"),
                ["model-2"] = new TestLanguageModel("model-2"),
            },
        };

        var wrapped = Wrap.Wrap(
            provider,
            [new RecordingLanguageMiddleware(model => $"override-{model.ModelId}")]
        );

        Assert.Equal("override-model-1", wrapped.LanguageModel("model-1").ModelId);
        Assert.Equal("override-model-2", wrapped.LanguageModel("model-2").ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should wrap all image models in the provider when image middleware is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_wrap_all_image_models_in_the_provider_when_image_middleware_is_provided()
    {
        var provider = new MockProvider
        {
            LanguageModels = { ["language-model"] = new TestLanguageModel("language-model") },
            ImageModels =
            {
                ["model-1"] = new MockImageModel { ModelId = "model-1" },
                ["model-2"] = new MockImageModel { ModelId = "model-2" },
                ["model-3"] = new MockImageModel { ModelId = "model-3" },
            },
        };
        var seen = new List<string>();
        var overrideModelId = new RecordingImageMiddleware(model =>
        {
            seen.Add(model.ModelId);
            return $"override-{model.ModelId}";
        });

        var wrappedProvider = Wrap.Wrap(
            provider,
            [new RecordingLanguageMiddleware(_ => null)],
            [overrideModelId]
        );

        Assert.Equal("override-model-1", wrappedProvider.ImageModel("model-1").ModelId);
        Assert.Equal("override-model-2", wrappedProvider.ImageModel("model-2").ModelId);
        Assert.Equal("override-model-3", wrappedProvider.ImageModel("model-3").ModelId);

        Assert.Equal(["model-1", "model-2", "model-3"], seen);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve file and skill upload interfaces",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_file_and_skill_upload_interfaces()
    {
        var files = new StubFileStore();
        var skills = new StubSkillStore();
        var provider = new MockProvider { Files = files, Skills = skills };

        var wrappedProvider = Wrap.Wrap(provider, [new RecordingLanguageMiddleware(_ => null)]);

        Assert.Same(files, wrappedProvider.FileStore());
        Assert.Same(skills, wrappedProvider.SkillStore());
    }

    [Fact]
    [UpstreamTest(
        Id + "should not add file and skill upload interfaces when unsupported",
        Coverage = UpstreamCoverage.Partial,
        Note = "ProviderBase has FileStore() and SkillStore() methods that throw when unsupported, not optional members. The port asserts the wrapper throws the same AiSdkException as the provider instead of toBeUndefined."
    )]
    public void Should_not_add_file_and_skill_upload_interfaces_when_unsupported()
    {
        var provider = new MockProvider();

        var wrappedProvider = Wrap.Wrap(provider, [new RecordingLanguageMiddleware(_ => null)]);

        var fileError = Assert.Throws<AiSdkException>(() => wrappedProvider.FileStore());
        Assert.Equal(
            Assert.Throws<AiSdkException>(() => provider.FileStore()).Message,
            fileError.Message
        );
        var skillError = Assert.Throws<AiSdkException>(() => wrappedProvider.SkillStore());
        Assert.Equal(
            Assert.Throws<AiSdkException>(() => provider.SkillStore()).Message,
            skillError.Message
        );
    }

    private sealed class RecordingLanguageMiddleware(Func<ILanguageModel, string?> overrideModelId)
        : LanguageModelMiddleware
    {
        public override string? OverrideModelId(ILanguageModel model) => overrideModelId(model);
    }

    private sealed class RecordingImageMiddleware(
        Func<global::Vercel.AI.Operations.IImageCaller, string?> overrideModelId
    ) : ImageModelMiddleware
    {
        public override string? OverrideModelId(global::Vercel.AI.Operations.IImageCaller model) =>
            overrideModelId(model);
    }

    private sealed class StubFileStore : IFileStore
    {
        public Task<UploadedFile> UploadFileAsync(
            string fileName,
            byte[] data,
            string mediaType,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();
    }

    private sealed class StubSkillStore : ISkillStore
    {
        public Task<UploadedSkill> UploadSkillAsync(
            string name,
            string instructions,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();
    }

    /// <summary>Maps to <c>MockProviderV4</c>.</summary>
    private sealed class MockProvider() : ProviderBase("mock-provider")
    {
        public Dictionary<string, ILanguageModel> LanguageModels { get; } = [];

        public Dictionary<string, IImageModel> ImageModels { get; } = [];

        public IFileStore? Files { get; init; }

        public ISkillStore? Skills { get; init; }

        public override ILanguageModel LanguageModel(string modelId) => LanguageModels[modelId];

        public override IImageModel ImageModel(string modelId) => ImageModels[modelId];

        public override IFileStore FileStore() => Files ?? base.FileStore();

        public override ISkillStore SkillStore() => Skills ?? base.SkillStore();
    }
}
