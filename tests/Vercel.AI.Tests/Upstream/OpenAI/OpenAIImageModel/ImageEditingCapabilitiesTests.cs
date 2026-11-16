// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIImageModel;

/// <summary>Port of <c>openai-image-model.test.ts</c> &gt; <c>image editing capabilities</c>.</summary>
public sealed class ImageEditingCapabilitiesTests
{
    private const string Id = "packages/openai/src/image/openai-image-model.test.ts::image editing capabilities::";

    [Theory]
    [UpstreamTest(Id + "advertises file and mask support for %s", Coverage = UpstreamCoverage.Covered)]
    [InlineData("dall-e-2")]
    [InlineData("gpt-image-1")]
    [InlineData("gpt-image-1-mini")]
    [InlineData("gpt-image-1.5")]
    [InlineData("gpt-image-2")]
    [InlineData("gpt-image-2.6-flare")]
    [InlineData("gpt-image-2.6-sunburst")]
    [InlineData("gpt-image-2.5-flare")]
    [InlineData("gpt-image-2.5-flare-2026-09-08")]
    [InlineData("gpt-image-2.5-sunburst")]
    [InlineData("gpt-image-2.5-sunburst-2026-09-08")]
    [InlineData("chatgpt-image-latest")]
    public void Advertises_file_and_mask_support(string modelId)
    {
        var model = new Vercel.AI.OpenAI.OpenAIImageModel(OpenAIUpstream.Provider(new OpenAICapture()), modelId);

        Assert.True(model.SupportsFileInputs);
        Assert.True(model.SupportsMaskInputs);
    }

    [Fact]
    [UpstreamTest(Id + "advertises that dall-e-3 does not support editing inputs", Coverage = UpstreamCoverage.Covered)]
    public void Advertises_that_dall_e_3_does_not_support_editing_inputs()
    {
        var model = new Vercel.AI.OpenAI.OpenAIImageModel(OpenAIUpstream.Provider(new OpenAICapture()), "dall-e-3");

        Assert.False(model.SupportsFileInputs);
        Assert.False(model.SupportsMaskInputs);
    }

    [Theory]
    [UpstreamTest(Id + "leaves editing support unknown for %s", Coverage = UpstreamCoverage.Covered)]
    [InlineData("custom-image-model")]
    [InlineData("gpt-image-custom")]
    [InlineData("chatgpt-image-custom")]
    public void Leaves_editing_support_unknown(string modelId)
    {
        var model = new Vercel.AI.OpenAI.OpenAIImageModel(OpenAIUpstream.Provider(new OpenAICapture()), modelId);

        Assert.Null(model.SupportsFileInputs);
        Assert.Null(model.SupportsMaskInputs);
    }
}
