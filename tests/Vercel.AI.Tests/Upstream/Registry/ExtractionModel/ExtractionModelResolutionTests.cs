// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.Operations.Extract;
using Vercel.AI.Tests.Upstream.Registry.DecisionModel;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Registry.DecisionModel.DecisionModelSupport;
using ExtractOperation = Vercel.AI.Operations.Extract;

namespace Vercel.AI.Tests.Upstream.Registry.ExtractionModel;

/// <summary>Port of <c>describe('extraction model resolution')</c> in <c>extraction-model.test.ts</c>.</summary>
[Collection("DecisionDefaultProvider")]
public sealed class ExtractionModelResolutionTests : DecisionDefaultProviderFixture
{
    private const string Prefix =
        "packages/ai/src/registry/extraction-model.test.ts::extraction model resolution::";

    private readonly ExtractionMockModel _model = new();

    [Fact]
    [UpstreamTest(
        Prefix + "returns model instances unchanged",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Returns_model_instances_unchanged()
    {
        Assert.Same(_model, DecisionModels.ResolveExtractionModel(_model));
    }

    [Theory]
    [InlineData("string")]
    [InlineData("alias")]
    [UpstreamTest(
        Prefix + "extracts through Gateway using a model %s when no default provider is configured",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Extracts_through_Gateway_using_a_model_when_no_default_provider_is_configured(
        string resolution
    )
    {
        DecisionModels.DefaultProvider = null;
        Environment.SetEnvironmentVariable("AI_GATEWAY_API_KEY", "test-api-key");
        var capture = new OpenAICapture
        {
            ResponseJson =
                """{"answers":{"vendor":{"type":"text","value":"Acme"}},"usage":{"inputTokens":10,"outputTokens":2},"warnings":[]}""",
        };
        DecisionModels.GatewayFactory = () => GatewayProvider.Create(handler: capture);
        const string modelId = "scaledown/extract-1";

        var result = await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model =
                    resolution == "string"
                        ? null
                        : new CustomProvider()
                            .AddExtractionModel("invoices", modelId)
                            .ExtractionModel("invoices"),
                ModelId = resolution == "string" ? modelId : null,
                State = "Invoice from Acme.",
                Fields = new Dictionary<string, ExtractionField> { ["vendor"] = new("text") },
            }
        );

        Assert.Equal("Acme", result.Answers["vendor"].Text);
        Assert.Equal(12, result.Usage.TotalTokens);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("Bearer test-api-key", capture.Headers["Authorization"]);
        Assert.Equal(modelId, capture.Headers["ai-model-id"]);
        Assert.Equal("4", capture.Headers["ai-extraction-model-specification-version"]);
        var expected = JsonNode.Parse(
            """{"stateParts":[{"type":"text","text":"Invoice from Acme."}],"fields":{"vendor":{"type":"text"}},"providerOptions":{}}"""
        );
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(capture.Body)), capture.Body);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "requires an explicitly configured default provider to support extraction",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Requires_an_explicitly_configured_default_provider_to_support_extraction()
    {
        var gatewayCalls = 0;
        DecisionModels.GatewayFactory = () =>
        {
            gatewayCalls++;
            return GatewayProvider.Create();
        };
        DecisionModels.DefaultProvider = new MockProvider();
        var error = Assert.Throws<InvalidOperationException>(() =>
            DecisionModels.ResolveExtractionModel(null, "model")
        );
        Assert.Equal("The default provider does not support extraction models.", error.Message);
        Assert.Equal(0, gatewayCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "validates versions returned by model instances, aliases, registries, and defaults",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Partial,
        Note = "The default provider is the custom provider. A ProviderRegistry is not a ProviderBase, so it cannot be the default provider as upstream does. Issue #1422."
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
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
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
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
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
    }

    [Theory]
    [InlineData("string")]
    [InlineData("registry")]
    [UpstreamTest(
        Prefix + "extracts with %s model resolution and forwards options in one call",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
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
                            ["total"] = new("number", number: 12),
                        },
                        usage: new OperationUsage(10, 2, 12)
                    )
                ),
        };
        var provider = new CustomProvider().AddExtractionModel("invoice", mock);
        DecisionModels.DefaultProvider = provider;
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        var fields = new Dictionary<string, ExtractionField> { ["total"] = new("number") };

        var result = await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model =
                    resolution == "string" ? null : registry.ExtractionModel("provider:invoice"),
                ModelId = resolution == "string" ? "invoice" : null,
                State = "Total: 12",
                Fields = fields,
                Headers = new Dictionary<string, string> { ["x-test"] = "header" },
                ProviderOptions = Json("""{"test":{"flag":true}}"""),
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
