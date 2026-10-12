// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

#pragma warning disable CS0618

using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Registry.DecisionModel;

/// <summary>Port of <c>decision-model.test.ts</c> <c>describe('decision registry')</c>.</summary>
public sealed class DecisionRegistryTests
{
    private const string Prefix = "packages/ai/src/registry/decision-model.test.ts::decision registry::";

    private readonly DecisionMockModel _model = new DecisionMockModel();

    [Fact]
    [UpstreamTest(Prefix + "preserves model identity, receiver, and colons inside model IDs", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_model_identity_receiver_and_colons_inside_model_IDs()
    {
        var provider = new DecisionTestProvider(_model);
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        Assert.Same(_model, registry.DecisionModel("provider:model:version"));
        Assert.Same(provider.LanguageModel("language"), registry.LanguageModel("provider:language"));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves legacy provider extensions and custom separators", Coverage = UpstreamCoverage.Partial, Note = "Checks the custom separator. .NET has no ProviderV3 to adapt. Issue #391.")]
    public void Preserves_legacy_provider_extensions_and_custom_separators()
    {
        var provider = new DecisionFactoryProvider(modelId =>
        {
            Assert.Equal("model::version", modelId);
            return _model;
        });
        var registry = new ProviderRegistry("::");
        registry.Register("provider", provider);
        Assert.Same(_model, registry.DecisionModel("provider::model::version"));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves deprecated factories when adapting a v3 provider", Coverage = UpstreamCoverage.Partial, Note = "Checks that DecisionModel and the deprecated EvaluationModel resolve through the deprecated factory. .NET has no ProviderV3 to adapt. Issue #391.")]
    public void Preserves_deprecated_factories_when_adapting_a_v3_provider()
    {
        var provider = new EvaluationFactoryProvider(modelId =>
        {
            Assert.Equal("model::version", modelId);
            return _model;
        });
        var registry = new ProviderRegistry("::");
        registry.Register("provider", provider);
        Assert.Same(_model, registry.DecisionModel("provider::model::version"));
        Assert.Same(_model, registry.EvaluationModel("provider::model::version"));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves deprecated factory receivers when adapting video models", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_deprecated_factory_receivers_when_adapting_video_models()
    {
        var videoModel = new MockVideoModel();
        var provider = new EvaluationFactoryProvider(modelId =>
        {
            Assert.Equal("model", modelId);
            return _model;
        })
        {
            Video = videoModel,
        };
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        Assert.Same(_model, registry.DecisionModel("provider:model"));
        Assert.Same(_model, registry.EvaluationModel("provider:model"));
        Assert.Same(videoModel, registry.VideoModel("provider:video"));
    }

    [Fact]
    [UpstreamTest(Prefix + "identifies unknown providers with the existing marker-based error", Coverage = UpstreamCoverage.Covered)]
    public void Identifies_unknown_providers_with_the_existing_marker_based_error()
    {
        var registry = new ProviderRegistry();
        registry.Register("known", new MockProvider());
        var error = Assert.Throws<NoSuchProviderError>(() => registry.DecisionModel("missing:model"));
        Assert.True(NoSuchProviderError.IsInstance(error));
        Assert.True(NoSuchModelError.IsInstance(error));
        Assert.Equal("missing", error.ProviderId);
        Assert.Equal("decisionModel", error.ModelType);
        Assert.Equal(new[] { "known" }, error.AvailableProviders);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports malformed IDs", Coverage = UpstreamCoverage.Covered)]
    public void Reports_malformed_IDs()
    {
        var error = Assert.Throws<NoSuchModelError>(() => new ProviderRegistry().DecisionModel("model"));
        Assert.Contains("must be in the format \"providerId:modelId\"", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Prefix + "reports missing decision capabilities or models", Coverage = UpstreamCoverage.Covered)]
    public void Reports_missing_decision_capabilities_or_models(bool hasFactory)
    {
        var registry = new ProviderRegistry();
        registry.Register("provider", hasFactory ? new DecisionFactoryProvider(_ => null) : new MockProvider());
        var error = Assert.Throws<NoSuchModelError>(() => registry.DecisionModel("provider:model"));
        Assert.Equal("provider:model", error.ModelId);
        Assert.Equal("decisionModel", error.ModelType);
        Assert.Equal("No such decisionModel: provider:model", error.Message);
    }
}
