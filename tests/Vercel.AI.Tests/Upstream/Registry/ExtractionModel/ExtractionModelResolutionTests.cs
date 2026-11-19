// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Operations.Extract;
using Vercel.AI.Tests.Upstream.Registry.DecisionModel;
using Vercel.AI.Util;
using ExtractOperation = Vercel.AI.Operations.Extract;

namespace Vercel.AI.Tests.Upstream.Registry.ExtractionModel;

/// <summary>Port of <c>describe('extraction model resolution')</c> in <c>extraction-model.test.ts</c>.</summary>
[Collection("DecisionDefaultProvider")]
public sealed class ExtractionModelResolutionTests : DecisionDefaultProviderFixture
{
    private const string Prefix =
        "packages/ai/src/registry/extraction-model.test.ts::extraction model resolution::";

    private const string NotSupported = "The default provider does not support extraction models.";

    private readonly ExtractionMockModel _model = new();

    [Fact]
    [UpstreamTest(
        Prefix + "returns model instances unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Returns_model_instances_unchanged()
    {
        Assert.Same(_model, DecisionModels.ResolveExtractionModel(_model));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports that Gateway has no extraction models yet",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Reports_that_Gateway_has_no_extraction_models_yet()
    {
        DecisionModels.DefaultProvider = null;
        var error = Assert.Throws<NoSuchModelError>(() =>
            DecisionModels.ResolveExtractionModel(null, "scaledown/extract")
        );
        Assert.Contains(NotSupported, error.Message, StringComparison.Ordinal);
        Assert.Equal("extractionModel", error.ModelType);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "requires an explicitly configured default provider to support extraction",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Requires_an_explicitly_configured_default_provider_to_support_extraction()
    {
        DecisionModels.DefaultProvider = new MockProvider();
        var error = Assert.Throws<NoSuchModelError>(() =>
            DecisionModels.ResolveExtractionModel(null, "model")
        );
        Assert.Contains(NotSupported, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "validates versions returned by model instances, aliases, registries, and defaults",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Validates_versions_returned_by_model_instances_aliases_registries_and_defaults()
    {
        var invalid = new ExtractionMockModel { SpecificationVersion = "v3" };
        var provider = new CustomProvider().AddExtractionModel("invalid", invalid);
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        DecisionModels.DefaultProvider = provider;
        foreach (
            var resolve in new Func<IExtractionCaller>[]
            {
                () => DecisionModels.ResolveExtractionModel(invalid),
                () => provider.ExtractionModel("invalid"),
                () => registry.ExtractionModel("provider:invalid"),
                () => DecisionModels.ResolveExtractionModel(null, "invalid"),
            }
        )
        {
            Assert.Throws<UnsupportedModelVersionException>(resolve);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves errors thrown by provider factories",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_errors_thrown_by_provider_factories()
    {
        var error = new NoSuchModelError("missing", "extractionModel");
        DecisionModels.DefaultProvider = new ExtractionFactoryProvider(_ => throw error);
        Assert.Same(
            error,
            Assert.Throws<NoSuchModelError>(() =>
                DecisionModels.ResolveExtractionModel(null, "missing")
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports missing models returned by the default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Reports_missing_models_returned_by_the_default_provider()
    {
        DecisionModels.DefaultProvider = new ExtractionFactoryProvider(_ => null);
        var error = Assert.Throws<NoSuchModelError>(() =>
            DecisionModels.ResolveExtractionModel(null, "missing")
        );
        Assert.Equal("missing", error.ModelId);
        Assert.Equal("extractionModel", error.ModelType);
        Assert.Equal("No such extractionModel: missing", error.Message);
    }

    [Theory]
    [InlineData("string")]
    [InlineData("registry")]
    [UpstreamTest(
        Prefix + "extracts with %s model resolution and forwards options in one call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Extracts_with_model_resolution_and_forwards_options_in_one_call(
        string resolution
    )
    {
        var mock = new ExtractionMockModel
        {
            OnExtract = _ =>
                Task.FromResult(
                    new ExtractionModelResult(
                        new Dictionary<string, ExtractionAnswer>
                        {
                            ["total"] = new ExtractionAnswer("number", number: 12),
                        },
                        [],
                        new OperationUsage(10, 2, 12)
                    )
                ),
        };
        var provider = new CustomProvider().AddExtractionModel("invoice", mock);
        DecisionModels.DefaultProvider = provider;
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        var fields = new Dictionary<string, ExtractionField>
        {
            ["total"] = new ExtractionField("number"),
        };

        var result = await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model =
                    resolution == "string" ? null : registry.ExtractionModel("provider:invoice"),
                ModelId = resolution == "string" ? "invoice" : null,
                State = "Total: 12",
                Fields = fields,
                Headers = new Dictionary<string, string> { ["x-test"] = "header" },
                ProviderOptions = DecisionModelSupport.Json("""{"test":{"flag":true}}"""),
            }
        );

        Assert.Equal(12, result.Answers["total"].Number);
        Assert.Equal(12, result.Usage.TotalTokens);
        var call = Assert.Single(mock.Calls);
        Assert.Same(fields, call.Fields);
        Assert.Equal("header", call.Headers["x-test"]);
        Assert.Equal("""{"test":{"flag":true}}""", call.ProviderOptions.GetRawText());
    }
}
