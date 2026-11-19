// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Operations.Extract;
using Vercel.AI.Tests.Upstream.Registry.DecisionModel;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Registry.ExtractionModel;

/// <summary>Port of <c>describe('custom extraction models')</c> in <c>extraction-model.test.ts</c>.</summary>
[Collection("DecisionDefaultProvider")]
public sealed class CustomExtractionModelsTests : DecisionDefaultProviderFixture
{
    private const string Prefix =
        "packages/ai/src/registry/extraction-model.test.ts::custom extraction models::";

    private readonly ExtractionMockModel _model = new();

    [Fact]
    [UpstreamTest(
        Prefix + "resolves an alias before consulting the fallback",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Resolves_an_alias_before_consulting_the_fallback()
    {
        var calls = 0;
        var provider = new CustomProvider
        {
            FallbackProvider = new ExtractionFactoryProvider(_ =>
            {
                calls++;
                return null;
            }),
        }.AddExtractionModel("alias", _model);
        Assert.Same(_model, provider.ExtractionModel("alias"));
        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "resolves string aliases through the configured default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Resolves_string_aliases_through_the_configured_default_provider()
    {
        DecisionModels.DefaultProvider = new ExtractionTestProvider(_model);
        var provider = new CustomProvider().AddExtractionModel("alias", "model:version");
        Assert.Same(_model, provider.ExtractionModel("alias"));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves the fallback receiver", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_fallback_receiver()
    {
        var provider = new CustomProvider { FallbackProvider = new ExtractionTestProvider(_model) };
        Assert.Same(_model, provider.ExtractionModel("model:version"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves extraction when adapting a legacy fallback",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks a fallback provider that has an extraction factory. .NET has no ProviderV2 or ProviderV3 to adapt. Issue #391."
    )]
    public void Preserves_extraction_when_adapting_a_legacy_fallback()
    {
        var provider = new CustomProvider
        {
            FallbackProvider = new ExtractionFactoryProvider(_ => _model),
        };
        Assert.Same(_model, provider.ExtractionModel("model"));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("toString")]
    [InlineData("constructor")]
    [InlineData("__proto__")]
    [UpstreamTest(
        Prefix + "reports missing alias %s without treating inherited properties as models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Reports_missing_alias_without_treating_inherited_properties_as_models(
        string modelId
    )
    {
        var provider = new CustomProvider().AddExtractionModel("known", _model);
        var error = Assert.Throws<NoSuchModelError>(() => provider.ExtractionModel(modelId));
        Assert.Equal(modelId, error.ModelId);
        Assert.Equal("extractionModel", error.ModelType);
        Assert.Equal("No such extractionModel: " + modelId, error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports an unavailable fallback model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Reports_an_unavailable_fallback_model()
    {
        var provider = new CustomProvider
        {
            FallbackProvider = new ExtractionFactoryProvider(_ => null),
        };
        var error = Assert.Throws<NoSuchModelError>(() => provider.ExtractionModel("missing"));
        Assert.Equal("missing", error.ModelId);
        Assert.Equal("extractionModel", error.ModelType);
        Assert.Equal("No such extractionModel: missing", error.Message);
    }
}
