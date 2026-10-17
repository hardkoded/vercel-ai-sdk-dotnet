// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class ImageGenerationTests
{
    [Theory]
    [InlineData("gpt-image-2.5-flare", "xhigh")]
    [InlineData("gpt-image-2.5-flare", "max")]
    [InlineData("gpt-image-2.5-sunburst", "xhigh")]
    [InlineData("gpt-image-2.5-sunburst", "max")]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > image generation > %s quality::should pass %s quality", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_quality(string model, string quality)
    {
        var tool = OpenAITools.ImageGeneration(new OpenAIImageGenerationToolOptions { Model = model, Quality = quality });
        var result = Prepare(null, LanguageModelTool.ProviderDefined("openai.image_generation", "image_generation", tool.Args));

        AssertTools(result, "[{\"type\":\"image_generation\",\"model\":\"" + model + "\",\"quality\":\"" + quality + "\"}]");
    }
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > image generation::should prepare image_generation tool with all options", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_image_generation_tool_with_all_options()
    {
        var result = Prepare(null, ProviderTool("openai.image_generation", "image_generation", "{\"background\":\"opaque\",\"size\":\"1536x1024\",\"quality\":\"high\",\"moderation\":\"auto\",\"outputFormat\":\"png\",\"outputCompression\":100}"));

        AssertTools(result, "[{\"type\":\"image_generation\",\"background\":\"opaque\",\"moderation\":\"auto\",\"output_compression\":100,\"output_format\":\"png\",\"quality\":\"high\",\"size\":\"1536x1024\"}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > image generation::should pass action, low moderation and a gpt-image-2 size", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_action_low_moderation_and_a_gpt_image_2_size()
    {
        var result = Prepare(null, ProviderTool("openai.image_generation", "image_generation", "{\"action\":\"edit\",\"model\":\"gpt-image-2\",\"moderation\":\"low\",\"size\":\"1536x864\"}"));

        AssertTools(result, "[{\"type\":\"image_generation\",\"action\":\"edit\",\"model\":\"gpt-image-2\",\"moderation\":\"low\",\"size\":\"1536x864\"}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > image generation::should reject a size that is not WIDTHxHEIGHT", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_a_size_that_is_not_WIDTHxHEIGHT()
    {
        Assert.Throws<TypeValidationException>(() => Prepare(null, ProviderTool("openai.image_generation", "image_generation", "{\"size\":\"large\"}")));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > image generation::should support tool choice selection for image_generation", Coverage = UpstreamCoverage.Covered)]
    public void Should_support_tool_choice_selection_for_image_generation()
    {
        var result = Prepare(ToolChoice.Tool("image_generation"), ProviderTool("openai.image_generation", "image_generation", "{}"));

        AssertTools(result, "[{\"type\":\"image_generation\"}]");
        AssertToolChoice(result, "{\"type\":\"image_generation\"}");
    }
}
