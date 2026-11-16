// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIImageModel;

/// <summary>Port of <c>openai-image-model.test.ts</c> &gt; <c>doGenerate - GPT Image 2.6 options</c>.</summary>
public sealed class DoGenerateGptImage26OptionsTests
{
    private const string Id = "packages/openai/src/image/openai-image-model.test.ts::doGenerate - GPT Image 2.6 options::";

    [Fact]
    [UpstreamTest(Id + "should map complexity, gif and safety options for /images/generations", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_complexity_gif_and_safety_options_for_images_generations()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"}]}" };
        var model = new Vercel.AI.OpenAI.OpenAIImageModel(OpenAIUpstream.Provider(capture), "gpt-image-2.6-flare");

        await model.GenerateAsync(
            new OpenAIImageCall("A cute baby sea otter")
            {
                Count = 1,
                Size = "1024x1024",
                ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"complexity\":\"low\",\"outputFormat\":\"gif\",\"frameCount\":12,\"frameDurationMs\":100,\"safetyIdentifier\":\"hashed-user\"}}"),
            },
            CancellationToken.None);

        OpenAIUpstream.Equal(
            System.Text.Json.Nodes.JsonNode.Parse(capture.Body),
            "{\"model\":\"gpt-image-2.6-flare\",\"prompt\":\"A cute baby sea otter\",\"n\":1,\"size\":\"1024x1024\",\"complexity\":\"low\",\"output_format\":\"gif\",\"frame_count\":12,\"frame_duration_ms\":100,\"safety_identifier\":\"hashed-user\"}");
    }

    [Fact]
    [UpstreamTest(Id + "should reject gif for edits", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_gif_for_edits()
    {
        var model = new Vercel.AI.OpenAI.OpenAIImageModel(OpenAIUpstream.Provider(new OpenAICapture()), "gpt-image-2.6-flare");

        await Assert.ThrowsAnyAsync<Exception>(() => model.GenerateAsync(
            new OpenAIImageCall("edit")
            {
                Files = new[] { new OpenAIImageFile(new byte[] { 137, 80, 78, 71 }, "image/png", "image.png") },
                Size = "1024x1024",
                ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"outputFormat\":\"gif\"}}"),
            },
            CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Id + "should send complexity, safety identifier and markup for /images/edits", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_complexity_safety_identifier_and_markup_for_images_edits()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"}]}" };
        var model = new Vercel.AI.OpenAI.OpenAIImageModel(OpenAIUpstream.Provider(capture), "gpt-image-2.6-flare");

        await model.GenerateAsync(
            new OpenAIImageCall("edit")
            {
                Files = new[] { new OpenAIImageFile(new byte[] { 137, 80, 78, 71 }, "image/png", "image.png") },
                Size = "1024x1024",
                ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"complexity\":\"medium\",\"safetyIdentifier\":\"hashed-user\",\"markup\":\"data:image/png;base64," + Convert.ToBase64String(new byte[] { 137, 80, 78, 71 }) + "\"}}"),
            },
            CancellationToken.None);

        var fields = capture.Parts.Where(part => part.FileName == null).ToDictionary(part => part.Name, part => System.Text.Encoding.UTF8.GetString(part.Data));
        Assert.Equal("medium", fields["complexity"]);
        Assert.Equal("hashed-user", fields["safety_identifier"]);
        var markup = Assert.Single(capture.Parts, part => part.Name == "markup");
        Assert.NotNull(markup.FileName);
        Assert.Equal(new byte[] { 137, 80, 78, 71 }, markup.Data);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate - GPT Image 2.6 options::should throw when markup and mask are both provided", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
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
