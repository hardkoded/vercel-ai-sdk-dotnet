// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    [Fact]
    [UpstreamTest(
        "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > constructor::should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldExposeCorrectProviderAndModelInformation()
    {
        var model = CreateModel(CreateServer());

        Assert.Equal("black-forest-labs.video", model.Provider);
        Assert.Equal("flux-3-video", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(
        "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > constructor::should support workflow serialization",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldSupportWorkflowSerialization()
    {
        var model = CreateModel(CreateServer(), () => DateTimeOffset.FromUnixTimeMilliseconds(0));

        var serialized = model.Serialize();

        Assert.Equal("flux-3-video", serialized.ModelId);
        Assert.Equal("black-forest-labs.video", serialized.Config["provider"]);
        Assert.Equal(TestBaseUrl, serialized.Config["baseURL"]);
        Assert.Equal(1, serialized.Config["pollIntervalMillis"]);
        Assert.Equal(5000, serialized.Config["pollTimeoutMillis"]);
        Assert.DoesNotContain("test-key", System.Text.Json.JsonSerializer.Serialize(serialized));

        var restored = Vercel.AI.BlackForestLabs.BlackForestLabsVideoModel.Deserialize(
            System.Text.Json.JsonSerializer.Deserialize<SerializedModelOptions>(
                System.Text.Json.JsonSerializer.Serialize(serialized)
            )!,
            "test-key"
        );

        Assert.Equal("black-forest-labs.video", restored.Provider);
        Assert.Equal("flux-3-video", restored.ModelId);
    }
}
