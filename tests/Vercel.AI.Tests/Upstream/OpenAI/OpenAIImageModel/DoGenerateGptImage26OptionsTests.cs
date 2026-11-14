// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIImageModel;

/// <summary>Port of <c>openai-image-model.test.ts</c> &gt; <c>doGenerate - GPT Image 2.6 options</c>.</summary>
public sealed class DoGenerateGptImage26OptionsTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate - GPT Image 2.6 options::should throw when markup and mask are both provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_when_markup_and_mask_are_both_provided()
    {
        var capture = new OpenAICapture();
        var file = new OpenAIImageFile(new byte[] { 137, 80, 78, 71 }, "image/png", "image.png");
        var model = new Vercel.AI.OpenAI.OpenAIImageModel(OpenAIUpstream.Provider(capture), "gpt-image-2.6-flare");

        var error = await Assert.ThrowsAnyAsync<Exception>(() => model.GenerateAsync(
            new OpenAIImageCall("edit")
            {
                Files = new[] { file },
                Mask = file,
                Size = "1024x1024",
                ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"markup\":\"data:image/png;base64,iVBORw0KGgo=\"}}"),
            },
            CancellationToken.None));

        Assert.Contains("`markup` cannot be combined with `mask`.", error.Message);
        Assert.Equal(0, capture.Calls);
    }
}
