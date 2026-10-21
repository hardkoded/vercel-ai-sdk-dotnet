// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Fal;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.Upstream.Fal.FalImageModel;

/// <summary>Port of <c>fal-image-model.test.ts</c> &gt; <c>FalImageModel &gt; capabilities</c>.</summary>
public sealed class CapabilitiesTests
{
    public static TheoryData<string, bool?, bool?> Cases()
    {
        return new TheoryData<string, bool?, bool?>
        {
            { "fal-ai/flux/dev/image-to-image", true, false },
            { "fal-ai/flux-2/edit", true, false },
            { "fal-ai/flux-pro/kontext", true, false },
            { "fal-ai/flux-pro/kontext/max", true, false },
            { "fal-ai/flux-general/inpainting", true, true },
            { "fal-ai/recraft/v3/text-to-image", false, false },
            { "custom/image-model", null, null },
            { "custom/image-to-image", null, null },
            { "custom/inpainting", null, null },
            { "custom/text-to-image", null, null },
            { "fal-ai/custom/image-to-image", null, null },
            { "fal-ai/custom/inpainting", null, null },
            { "fal-ai/custom/text-to-image", null, null },
        };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/fal/src/fal-image-model.test.ts::FalImageModel > capabilities::advertises file=$supportsFileInputs and mask=$supportsMaskInputs for $modelId", Coverage = UpstreamCoverage.Covered)]
    public void Advertises_file_and_mask_support(string modelId, bool? supportsFileInputs, bool? supportsMaskInputs)
    {
        var model = new Vercel.AI.Fal.FalImageModel(FalProvider.Create(new OpenAICompatibleOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" }), modelId);

        Assert.Equal(supportsFileInputs, model.SupportsFileInputs);
        Assert.Equal(supportsMaskInputs, model.SupportsMaskInputs);
    }
}
