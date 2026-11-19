// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel.GatewayExtractionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel;

/// <summary>Port of <c>gateway-extraction-model.test.ts</c> &gt; <c>GatewayExtractionModel</c>.</summary>
public sealed class GatewayExtractionModelTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-extraction-model.test.ts::GatewayExtractionModel::";

    [Fact]
    [UpstreamTest(
        Prefix + "should report every field type as supported",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_report_every_field_type_as_supported()
    {
        Assert.Equal(
            new[] { "text", "number", "date", "time", "datetime", "duration", "list" },
            Setup().Model.SupportedFieldTypes
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create the model from both provider factories",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_the_model_from_both_provider_factories()
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }
        );
        Assert.IsType<Vercel.AI.Gateway.GatewayExtractionModel>(
            provider.Extraction("scaledown/extract-1")
        );
        var model = provider.ExtractionModel("scaledown/extract-1");
        Assert.IsType<Vercel.AI.Gateway.GatewayExtractionModel>(model);
        Assert.Equal("gateway", model.Provider);
        Assert.Equal("scaledown/extract-1", model.ModelId);
    }
}
