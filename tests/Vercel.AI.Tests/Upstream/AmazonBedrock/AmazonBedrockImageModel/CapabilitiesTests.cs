// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockImageModel.BedrockImageTestSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockImageModel;

/// <summary>Port of <c>amazon-bedrock-image-model.test.ts</c> &gt; <c>capabilities</c>.</summary>
public sealed class CapabilitiesTests
{
    [Theory]
    [InlineData("amazon.nova-canvas-v1:0", true, true)]
    [InlineData("custom-image-model", null, null)]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-image-model.test.ts::capabilities::advertises file=$supportsFileInputs and mask=$supportsMaskInputs for $modelId",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Advertises_file_and_mask_support_per_model(
        string modelId,
        bool? supportsFileInputs,
        bool? supportsMaskInputs
    )
    {
        var model = Model(CreateProvider(new UpstreamRecordingHandler(ImagesBody)), modelId);

        Assert.Equal(supportsFileInputs, model.SupportsFileInputs);
        Assert.Equal(supportsMaskInputs, model.SupportsMaskInputs);
    }
}
