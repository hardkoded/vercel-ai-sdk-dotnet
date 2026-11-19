// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Operations.Extract;
using Vercel.AI.Tests.Upstream.Registry.DecisionModel;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Registry.ExtractionModel;

/// <summary>Port of <c>describe('extraction registry')</c> in <c>extraction-model.test.ts</c>.</summary>
public sealed class ExtractionRegistryTests
{
    private const string Prefix =
        "packages/ai/src/registry/extraction-model.test.ts::extraction registry::";

    private readonly ExtractionMockModel _model = new();

    [Fact]
    [UpstreamTest(
        Prefix + "preserves model identity, receiver, and colons inside model IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_model_identity_receiver_and_colons_inside_model_IDs()
    {
        var registry = new ProviderRegistry();
        registry.Register("provider", new ExtractionTestProvider(_model));
        Assert.Same(_model, registry.ExtractionModel("provider:model:version"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves legacy provider extensions and custom separators",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the custom separator. .NET has no ProviderV3 to adapt. Issue #391."
    )]
    public void Preserves_legacy_provider_extensions_and_custom_separators()
    {
        var provider = new ExtractionFactoryProvider(modelId =>
        {
            Assert.Equal("model::version", modelId);
            return _model;
        });
        var registry = new ProviderRegistry("::");
        registry.Register("provider", provider);
        Assert.Same(_model, registry.ExtractionModel("provider::model::version"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves the receiver when adapting video models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_the_receiver_when_adapting_video_models()
    {
        var videoModel = new MockVideoModel();
        var provider = new ExtractionFactoryProvider(_ => _model) { Video = videoModel };
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        Assert.Same(_model, registry.ExtractionModel("provider:model"));
        Assert.Same(videoModel, registry.VideoModel("provider:video"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "identifies unknown providers with the existing marker-based error",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Identifies_unknown_providers_with_the_existing_marker_based_error()
    {
        var registry = new ProviderRegistry();
        registry.Register("known", new MockProvider());
        var error = Assert.Throws<NoSuchProviderError>(() =>
            registry.ExtractionModel("missing:model")
        );
        Assert.True(NoSuchProviderError.IsInstance(error));
        Assert.True(NoSuchModelError.IsInstance(error));
        Assert.Equal("missing", error.ProviderId);
        Assert.Equal("extractionModel", error.ModelType);
        Assert.Equal(new[] { "known" }, error.AvailableProviders);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports malformed IDs", Coverage = UpstreamCoverage.Covered)]
    public void Reports_malformed_IDs()
    {
        var error = Assert.Throws<NoSuchModelError>(() =>
            new ProviderRegistry().ExtractionModel("model")
        );
        Assert.Contains(
            "must be in the format \"providerId:modelId\"",
            error.Message,
            StringComparison.Ordinal
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "reports missing extraction capabilities or models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Reports_missing_extraction_capabilities_or_models(bool hasFactory)
    {
        var registry = new ProviderRegistry();
        registry.Register(
            "provider",
            hasFactory ? new ExtractionFactoryProvider(_ => null) : new MockProvider()
        );
        var error = Assert.Throws<NoSuchModelError>(() =>
            registry.ExtractionModel("provider:model")
        );
        Assert.Equal("provider:model", error.ModelId);
        Assert.Equal("extractionModel", error.ModelType);
        Assert.Equal("No such extractionModel: provider:model", error.Message);
    }
}
