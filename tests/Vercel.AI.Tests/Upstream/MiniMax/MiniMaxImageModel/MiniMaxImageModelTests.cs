// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.MiniMax;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.MiniMaxImageModel;

/// <summary>Port of <c>minimax-image-model.test.ts</c> &gt; <c>MiniMaxImageModel</c>.</summary>
public sealed class MiniMaxImageModelTests
{
    private const string Prefix = "packages/minimax/src/minimax-image-model.test.ts::MiniMaxImageModel::";
    private const string BaseUrl = "https://api.example.com/v1";
    private const string GenerationUrl = BaseUrl + "/images/generations";
    private const string EditUrl = BaseUrl + "/images/edits";
    private const string ImageUrl = "https://api.example.com/generated.png";
    private const string Image = "{\"url\":\"" + ImageUrl + "\",\"size\":\"1024x1024\",\"revised_prompt\":\"A red circle.\"}";
    private const string Usage = "{\"generated_images\":1,\"input_tokens\":30,\"output_tokens\":40,\"total_tokens\":70,\"input_tokens_details\":{\"text_tokens\":10,\"image_tokens\":20},\"output_tokens_details\":{\"image_tokens\":40}}";

    [Fact]
    [UpstreamTest(Prefix + "generates and downloads images while preserving usage", Coverage = UpstreamCoverage.Covered, Commit = "c4c8523cc3a8c00cede2e068901cbb136162f3fb")]
    public async Task Generates_and_downloads_images_while_preserving_usage()
    {
        var handler = Server("{\"data\":[" + Image + "],\"usage\":" + Usage + ",\"output_format\":\"png\",\"request_id\":\"request-1\"}");
        var options = Params();
        options.Size = "1024x1024";
        options.Seed = 1;
        options.Headers = new Dictionary<string, string?> { ["request-header"] = "request" };
        options.ProviderOptions = Options("{\"disablePromptEnhanced\":true,\"outputFormat\":\"png\",\"outputCompression\":90}");

        var result = await Provider(handler).Image("MiniMax-H3-Image").DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"model\":\"MiniMax-H3-Image\",\"prompt\":\"A red circle.\",\"n\":1,\"size\":\"1024x1024\",\"disable_prompt_enhanced\":true,\"output_format\":\"png\",\"output_compression\":90,\"response_format\":\"url\"}");
        Assert.Equal("Bearer test-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("value", handler.Calls[0].Header("custom-header"));
        Assert.Equal("request", handler.Calls[0].Header("request-header"));
        Assert.Null(handler.Calls[1].Header("authorization"));
        Assert.Null(handler.Calls[1].Header("custom-header"));
        Assert.Equal(new byte[] { 1, 2, 3 }, Assert.Single(result.Images).Data);
        Assert.Equal(30, result.Usage!.InputTokens);
        Assert.Equal(40, result.Usage.OutputTokens);
        Assert.Equal(70, result.Usage.TotalTokens);
        JsonAssert.Equal(JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["minimax"], "{\"images\":[{\"size\":\"1024x1024\",\"revisedPrompt\":\"A red circle.\",\"usage\":" + Usage + ",\"outputFormat\":\"png\",\"requestId\":\"request-1\"}]}");
        var warning = Assert.Single(result.CallWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("seed", warning.Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps aspect ratio, area, and batch size without supplying a pixel size", Coverage = UpstreamCoverage.Covered, Commit = "c4c8523cc3a8c00cede2e068901cbb136162f3fb")]
    public async Task Maps_aspect_ratio_area_and_batch_size_without_supplying_a_pixel_size()
    {
        var batchUsage = Usage.Replace("\"generated_images\":1", "\"generated_images\":2");
        var handler = Server("{\"data\":[" + Image + "," + Image + "],\"usage\":" + batchUsage + "}");
        var options = Params();
        options.Count = 2;
        options.AspectRatio = "16:9";
        options.ProviderOptions = Options("{\"area\":\"2K\"}");

        var result = await Provider(handler).Image("MiniMax-H3-Image").DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"model\":\"MiniMax-H3-Image\",\"prompt\":\"A red circle.\",\"n\":2,\"aspect_ratio\":\"16:9\",\"area\":\"2K\",\"response_format\":\"url\"}");
        JsonAssert.Equal(JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["minimax"]!["images"], "[{\"size\":\"1024x1024\",\"revisedPrompt\":\"A red circle.\",\"usage\":" + batchUsage + "},{\"size\":\"1024x1024\",\"revisedPrompt\":\"A red circle.\"}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "edits binary and URL references and omits the generation-only prompt option", Coverage = UpstreamCoverage.Covered, Commit = "c4c8523cc3a8c00cede2e068901cbb136162f3fb")]
    public async Task Edits_binary_and_URL_references_and_omits_the_generation_only_prompt_option()
    {
        var handler = Server("{\"data\":[" + Image + "],\"usage\":" + Usage + "}");
        var options = Params();
        options.Files = new[] { ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"), ImageModelFile.FromUrl("https://example.com/reference.png") };
        options.ProviderOptions = Options("{\"disablePromptEnhanced\":true}");

        var result = await Provider(handler).Image("MiniMax-H3-Image").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(EditUrl, handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"model\":\"MiniMax-H3-Image\",\"prompt\":\"A red circle.\",\"n\":1,\"images\":[{\"image_url\":\"data:image/png;base64,AQID\"},{\"image_url\":\"https://example.com/reference.png\"}],\"response_format\":\"url\"}");
        var warning = Assert.Single(result.CallWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("disablePromptEnhanced", warning.Feature);
        Assert.Equal("MiniMax image edits always apply prompt enhancement.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns successful images and reports per-image failures without losing request usage", Coverage = UpstreamCoverage.Covered, Commit = "c4c8523cc3a8c00cede2e068901cbb136162f3fb")]
    public async Task Returns_successful_images_and_reports_per_image_failures_without_losing_request_usage()
    {
        var handler = Server("{\"data\":[" + Image + ",{\"error\":{\"code\":\"1027\",\"message\":\"Content safety check failed.\"}}],\"usage\":" + Usage + "}");
        var options = Params();
        options.Count = 2;

        var result = await Provider(handler).Image("MiniMax-H3-Image").DoGenerateAsync(options, CancellationToken.None);

        Assert.Single(result.Images);
        var images = JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["minimax"]!["images"]!.AsArray();
        Assert.Single(images);
        JsonAssert.Equal(images[0]!["usage"], Usage);
        var warning = Assert.Single(result.CallWarnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("Image 2 failed: Content safety check failed.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects masks before sending an unmasked edit", Coverage = UpstreamCoverage.Covered, Commit = "c4c8523cc3a8c00cede2e068901cbb136162f3fb")]
    public async Task Rejects_masks_before_sending_an_unmasked_edit()
    {
        var handler = Server("{\"data\":[" + Image + "]}");
        var options = Params();
        options.Mask = ImageModelFile.FromFile(new byte[] { 1 }, "image/png");

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Provider(handler).Image("MiniMax-H3-Image").DoGenerateAsync(options, CancellationToken.None));

        Assert.Contains("MiniMax image masks", error.Message);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves the provider error message", Coverage = UpstreamCoverage.Covered, Commit = "c4c8523cc3a8c00cede2e068901cbb136162f3fb")]
    public async Task Preserves_the_provider_error_message()
    {
        var handler = new ParityHandler(_ => new HttpResponseMessage(HttpStatusCode.PaymentRequired)
        {
            Content = new StringContent("{\"error\":{\"message\":\"Insufficient balance (1008)\"}}", Encoding.UTF8, "application/json"),
        });

        var error = await Assert.ThrowsAsync<ApiException>(() => Provider(handler).Image("MiniMax-H3-Image").DoGenerateAsync(Params(), CancellationToken.None));

        Assert.Contains("Insufficient balance (1008)", error.Message);
    }

    private static ImageCallOptions Params()
    {
        return new ImageCallOptions("A red circle.");
    }

    private static IReadOnlyDictionary<string, JsonElement> Options(string minimax)
    {
        using var document = JsonDocument.Parse(minimax);
        return new Dictionary<string, JsonElement> { ["minimax"] = document.RootElement.Clone() };
    }

    private static ParityHandler Server(string generationBody)
    {
        return new ParityHandler(call => call.Uri.AbsoluteUri switch
        {
            GenerationUrl or EditUrl => ParityHandler.Json(generationBody),
            ImageUrl => ParityHandler.Bytes(new byte[] { 1, 2, 3 }, "image/png"),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound),
        });
    }

    private static Vercel.AI.MiniMax.MiniMaxProvider Provider(ParityHandler handler)
    {
        var provider = new MiniMaxOptions { ApiKey = "test-key", ImageBaseUrl = BaseUrl };
        provider.Headers["custom-header"] = "value";
        return Vercel.AI.MiniMax.MiniMaxProvider.Create(provider, handler);
    }
}
