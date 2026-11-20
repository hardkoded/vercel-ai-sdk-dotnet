// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiImageModel.XaiImageTestSupport;
using Model = Vercel.AI.Xai.XaiImageModel;

namespace Vercel.AI.Tests.Upstream.Xai.XaiImageModel;

/// <summary>Port of <c>xai-image-model.test.ts</c> &gt; <c>XaiImageModel</c> &gt; <c>capabilities</c>.</summary>
public sealed class CapabilitiesTests
{
    private const string Id =
        "packages/xai/src/xai-image-model.test.ts::XaiImageModel > capabilities::";

    [Theory]
    [InlineData("grok-imagine-image", true, false)]
    [InlineData("grok-imagine-image-2.0", true, false)]
    [InlineData("custom-image-model", null, null)]
    [UpstreamTest(
        Id + "advertises file=$supportsFileInputs and mask=$supportsMaskInputs for $modelId",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Advertises_file_and_mask_support_per_model(
        string modelId,
        bool? supportsFileInputs,
        bool? supportsMaskInputs
    )
    {
        var provider = Vercel.AI.Xai.XaiProvider.Create(
            new OpenAICompatibleOptions { BaseUrl = BaseUrl, ApiKey = "test-key" }
        );
        var model = provider.Image(modelId);

        Assert.Equal(supportsFileInputs, model.SupportsFileInputs);
        Assert.Equal(supportsMaskInputs, model.SupportsMaskInputs);
    }
}
