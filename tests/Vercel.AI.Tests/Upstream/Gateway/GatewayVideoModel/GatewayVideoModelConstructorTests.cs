// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel.GatewayVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel;

/// <summary>Port of <c>gateway-video-model.test.ts</c> &gt; <c>GatewayVideoModel &gt; constructor</c>.</summary>
public sealed class GatewayVideoModelConstructorTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-video-model.test.ts::GatewayVideoModel > constructor::";

    [Fact]
    [UpstreamTest(
        Prefix + "should create instance with correct properties",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_instance_with_correct_properties()
    {
        var model = CreateTestModel(new UpstreamRecordingHandler("{}"));

        Assert.Equal(ModelId, model.ModelId);
        Assert.Equal("gateway", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(int.MaxValue, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should avoid client-side splitting for video models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_avoid_client_side_splitting_for_video_models()
    {
        var model = CreateTestModel(new UpstreamRecordingHandler("{}"), "fal/luma-ray-2");

        Assert.Equal(int.MaxValue, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept custom provider name",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_custom_provider_name()
    {
        var model = CreateTestModel(
            new UpstreamRecordingHandler("{}"),
            providerName: "custom-gateway"
        );

        Assert.Equal("custom-gateway", model.Provider);
    }
}
