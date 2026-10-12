// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Registry.DecisionModel;

/// <summary>Port of <c>describe('custom decision models')</c> in <c>decision-model.test.ts</c>.</summary>
[Collection("DecisionDefaultProvider")]
public sealed class CustomDecisionModelsTests : DecisionDefaultProviderFixture
{
    private const string Prefix = "packages/ai/src/registry/decision-model.test.ts::custom decision models::";

    private readonly DecisionMockModel _model = new();

    [Fact]
    [UpstreamTest(Prefix + "resolves an alias before consulting the fallback", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_an_alias_before_consulting_the_fallback()
    {
        var calls = 0;
        var provider = new CustomProvider
        {
            FallbackProvider = new FactoryProvider(_ =>
            {
                calls++;
                return null;
            }),
        }.AddDecisionModel("alias", _model);
        Assert.Same(_model, provider.DecisionModel("alias"));
        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "resolves string aliases through the configured default provider", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_string_aliases_through_the_configured_default_provider()
    {
        DecisionModels.DefaultProvider = new DecisionTestProvider(_model);
        var provider = new CustomProvider().AddDecisionModel("alias", "model:version");
        Assert.Same(_model, provider.DecisionModel("alias"));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves the fallback receiver", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_fallback_receiver()
    {
        var provider = new CustomProvider { FallbackProvider = new DecisionTestProvider(_model) };
        Assert.Same(_model, provider.DecisionModel("model:version"));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("toString")]
    [InlineData("constructor")]
    [InlineData("__proto__")]
    [UpstreamTest(Prefix + "reports missing alias %s without treating inherited properties as models", Coverage = UpstreamCoverage.Covered)]
    public void Reports_missing_alias_without_treating_inherited_properties_as_models(string modelId)
    {
        var provider = new CustomProvider().AddDecisionModel("known", _model);
        var error = Assert.Throws<NoSuchModelError>(() => provider.DecisionModel(modelId));
        Assert.Equal(modelId, error.ModelId);
        Assert.Equal("decisionModel", error.ModelType);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports an unavailable fallback model", Coverage = UpstreamCoverage.Covered)]
    public void Reports_an_unavailable_fallback_model()
    {
        var provider = new CustomProvider { FallbackProvider = new FactoryProvider(_ => null) };
        var error = Assert.Throws<NoSuchModelError>(() => provider.DecisionModel("missing"));
        Assert.Equal("missing", error.ModelId);
        Assert.Equal("decisionModel", error.ModelType);
    }
}
