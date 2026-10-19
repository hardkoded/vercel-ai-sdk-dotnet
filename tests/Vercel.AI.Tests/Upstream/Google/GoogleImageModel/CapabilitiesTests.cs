// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.GoogleImageModel;

/// <summary>Port of <c>google-image-model.test.ts</c> &gt; <c>GoogleImageModel &gt; capabilities</c>.</summary>
public sealed class CapabilitiesTests
{
    public static TheoryData<string, bool?, bool?> Cases()
    {
        return new TheoryData<string, bool?, bool?>
        {
            { "gemini-2.5-flash-image", true, false },
            { "gemini-3-pro-image-preview", true, false },
            { "gemini-3.1-flash-image-preview", true, false },
            { "gemini-nano-banana-2.1", true, false },
            { "gemini-2.5-pro", null, null },
            { "gemini-3-pro-preview", null, null },
            { "gemini-custom", null, null },
            { "legacy-image-model", null, null },
        };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > capabilities::advertises file=$supportsFileInputs and mask=$supportsMaskInputs for $modelId", Coverage = UpstreamCoverage.Covered)]
    public void Advertises_file_and_mask_support(string modelId, bool? supportsFileInputs, bool? supportsMaskInputs)
    {
        var capabilityModel = new Vercel.AI.Google.GoogleImageModel(GoogleImageModelTestSupport.CreateProvider(new RecordingHandler()), modelId);

        Assert.Equal(supportsFileInputs, capabilityModel.SupportsFileInputs);
        Assert.Equal(supportsMaskInputs, capabilityModel.SupportsMaskInputs);
    }
}
