// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayImageModel;

/// <summary>Port of <c>gateway-image-model.test.ts</c> &gt; <c>GatewayImageModel &gt; constructor</c>.</summary>
public sealed class GatewayImageModelConstructorTests
{
    private const string Prefix = "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > constructor::";

    [Fact]
    [UpstreamTest(Prefix + "should accept custom provider name", Coverage = UpstreamCoverage.Covered)]
    public void Should_accept_custom_provider_name()
    {
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" });
        var model = new Vercel.AI.Gateway.GatewayImageModel(provider, "openai/dall-e-3", "custom-gateway");

        Assert.Equal("custom-gateway", model.Provider);
        Assert.Equal("openai/dall-e-3", model.ModelId);
    }
}
