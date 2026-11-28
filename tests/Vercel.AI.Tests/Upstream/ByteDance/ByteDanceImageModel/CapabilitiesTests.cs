// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ByteDance;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.MoreProviders;
using ByteDanceImageModelType = Vercel.AI.ByteDance.ByteDanceImageModel;

namespace Vercel.AI.Tests.Upstream.ByteDance.ByteDanceImageModel;

/// <summary>Port of <c>bytedance-image-model.test.ts</c> &gt; <c>ByteDanceImageModel</c> &gt; <c>capabilities</c>.</summary>
public sealed class CapabilitiesTests
{
    private const string Prefix =
        "packages/bytedance/src/bytedance-image-model.test.ts::ByteDanceImageModel > capabilities::";

    [Theory]
    [InlineData("seedream-5-0-260128", true, false)]
    [InlineData("dola-seedream-5-0-pro-260628", true, false)]
    [InlineData("seedream-5-0-lite-260128", true, false)]
    [InlineData("seedream-4-5-251128", true, false)]
    [InlineData("seedream-4-0-250828", true, false)]
    [InlineData("not-seedream", null, null)]
    [InlineData("custom-seedream-model", null, null)]
    [InlineData("seedream-future", null, null)]
    [InlineData("custom-image-model", null, null)]
    [UpstreamTest(
        Prefix + "advertises file=$supportsFileInputs and mask=$supportsMaskInputs for $modelId",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Advertises_file_and_mask_support_for_modelId(
        string modelId,
        bool? supportsFileInputs,
        bool? supportsMaskInputs
    )
    {
        var provider = ByteDanceProvider.Create(
            new OpenAICompatibleOptions
            {
                ApiKey = "test-key",
                BaseUrl = "https://api.example.com",
            },
            new ParityHandler()
        );
        var model = new ByteDanceImageModelType(provider, modelId);

        Assert.Equal(supportsFileInputs, model.SupportsFileInputs);
        Assert.Equal(supportsMaskInputs, model.SupportsMaskInputs);
    }
}
