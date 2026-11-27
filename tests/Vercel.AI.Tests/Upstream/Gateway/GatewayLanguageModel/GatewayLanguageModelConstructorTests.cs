// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; constructor</c>.</summary>
public sealed class GatewayLanguageModelConstructorTests
{
    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > constructor::should set basic properties",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_set_basic_properties()
    {
        var model = CreateTestModel(new GatewayLanguageModelServer());

        Assert.Equal("test-model", model.ModelId);
        Assert.Equal("test-provider", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
    }
}
