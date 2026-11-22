// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.GoogleVertexVideoModel.GoogleVertexVideoModelFixture;

namespace Vercel.AI.Tests.GoogleVertexVideoModel;

/// <summary>Port of <c>google-vertex-video-model.test.ts</c> &gt; <c>GoogleVertexVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Id =
        "packages/google-vertex/src/google-vertex-video-model.test.ts::GoogleVertexVideoModel > constructor::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldExposeCorrectProviderAndModelInformation()
    {
        var model = CreateModel(new VertexVideoServer());

        Assert.Equal("google.vertex.video", model.Provider);
        Assert.Equal("veo-2.0-generate-001", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(4, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(Id + "should support different model IDs", Coverage = UpstreamCoverage.Covered)]
    public void ShouldSupportDifferentModelIDs()
    {
        var model = CreateModel(new VertexVideoServer(), modelId: "veo-3.0-generate-001");

        Assert.Equal("veo-3.0-generate-001", model.ModelId);
    }
}
