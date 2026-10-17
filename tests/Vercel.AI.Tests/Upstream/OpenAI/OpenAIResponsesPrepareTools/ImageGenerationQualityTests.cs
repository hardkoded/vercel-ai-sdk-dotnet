// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class ImageGenerationQualityTests
{
    [Fact]
    [UpstreamTest(Suite + " > image generation > %s quality::should pass %s quality", Coverage = UpstreamCoverage.Partial, Note = "The port has no imageGeneration tool factory (issue 549), so the test builds the tool args directly. It checks the prepared tool and the warnings for both models and both qualities.")]
    public void Should_pass_quality()
    {
        foreach (var model in new[] { "gpt-image-2.5-flare", "gpt-image-2.5-sunburst" })
        {
            foreach (var quality in new[] { "xhigh", "max" })
            {
                var result = Prepare(null, ProviderTool("openai.image_generation", "image_generation", $$"""{"model":"{{model}}","quality":"{{quality}}"}"""));

                AssertTools(result, $$"""[{"type":"image_generation","model":"{{model}}","quality":"{{quality}}"}]""");
            }
        }
    }
}
