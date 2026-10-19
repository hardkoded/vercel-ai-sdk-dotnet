// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.GoogleImageModel;

public sealed class MaxImagesPerCallTests
{
    private const string Prefix = "packages/google/src/google-image-model.test.ts::GoogleImageModel > maxImagesPerCall::";

    [Fact]
    [UpstreamTest(Prefix + "should default to a supported per-call limit", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_to_a_supported_per_call_limit()
    {
        var model = new Vercel.AI.Google.GoogleImageModel(GoogleProvider.Create(GoogleImageModelFixture.Key(), new RecordingHandler()), "gemini-2.5-flash-image");
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(Prefix + "should respect a custom setting", Coverage = UpstreamCoverage.Covered)]
    public void Respects_a_custom_setting()
    {
        var model = new Vercel.AI.Google.GoogleImageModel(GoogleProvider.Create(GoogleImageModelFixture.Key(), new RecordingHandler()), "gemini-2.5-flash-image", 5);
        Assert.Equal(5, model.MaxImagesPerCall);
    }
}
