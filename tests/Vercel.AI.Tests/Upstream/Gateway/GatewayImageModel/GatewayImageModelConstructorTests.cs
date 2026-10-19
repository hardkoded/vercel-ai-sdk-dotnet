// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayImageModel;

/// <summary>Port of <c>gateway-image-model.test.ts</c> &gt; <c>GatewayImageModel &gt; constructor</c>.</summary>
public sealed class GatewayImageModelConstructorTests
{
    private const string Prefix = "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > constructor::";

    [Fact]
    [UpstreamTest(Prefix + "should create instance with correct properties", Coverage = UpstreamCoverage.Covered)]
    public void Should_create_instance_with_correct_properties()
    {
        var model = GatewayImageModelSupport.CreateTestModel(new UpstreamRecordingHandler("{}"));

        Assert.Equal(GatewayImageModelSupport.ModelId, model.ModelId);
        Assert.Equal("gateway", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(int.MaxValue, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(Prefix + "should avoid client-side splitting even for BFL models", Coverage = UpstreamCoverage.Covered)]
    public void Should_avoid_client_side_splitting_even_for_BFL_models()
    {
        var model = GatewayImageModelSupport.CreateTestModel(new UpstreamRecordingHandler("{}"), "bfl/flux-pro-1.1");

        Assert.Equal(int.MaxValue, model.MaxImagesPerCall);
    }
}
