// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureMaiImageModel;

/// <summary>Port of <c>azure-mai-image-model.test.ts</c> &gt; <c>edits</c>.</summary>
public sealed class EditsTests
{
    private const string File = "packages/azure/src/azure-mai-image-model.test.ts::edits::";

    [Fact]
    [UpstreamTest(File + "sends reference images as repeated multipart fields", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsReferenceImagesAsRepeatedMultipartFields()
    {
        var handler = new AzureImageHandler(() => AzureImageHandler.JsonResponse(MaiResponse));
        var options = new ImageCallOptions("Combine both apples")
        {
            Size = "1024x1024",
            Files = new[]
            {
                ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"),
                ImageModelFile.FromBase64("AQID", "image/jpeg"),
            },
            Mask = ImageModelFile.FromFile(new byte[] { 4 }, "image/png"),
            ProviderOptions = ProviderOptionsOf("{\"azure\":{\"webGrounding\":true}}"),
        };

        var result = await CreateProvider(handler).Image("MAI-Image-2.6").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(EditsUrl, handler.Last.Url);
        var parts = handler.Last.Parts();
        Assert.Equal("MAI-Image-2.6", parts.Single(part => part.Name == "model").Text);
        Assert.Equal("Combine both apples", parts.Single(part => part.Name == "prompt").Text);
        Assert.Equal("1024", parts.Single(part => part.Name == "width").Text);
        Assert.Equal("1024", parts.Single(part => part.Name == "height").Text);
        Assert.Equal("true", parts.Single(part => part.Name == "web_grounding").Text);
        Assert.DoesNotContain(parts, part => part.Name == "mask");
        var images = parts.Where(part => part.Name == "image").ToList();
        Assert.Equal(new[] { ("image-1.png", "image/png"), ("image-2.jpg", "image/jpeg") }, images.Select(image => (image.FileName!, image.ContentType!)));
        Assert.Contains(
            result.CallWarnings,
            warning => warning.Type == "unsupported" && warning.Feature == "mask" && warning.Message == "MAI image edits do not support masks.");
    }

    [Fact]
    [UpstreamTest(File + "sums text and image input tokens for edits", Coverage = UpstreamCoverage.Covered)]
    public async Task SumsTextAndImageInputTokensForEdits()
    {
        var handler = new AzureImageHandler(() => AzureImageHandler.JsonResponse(new
        {
            created = 1791229019,
            model = "MAI-Image-2.6-Flash",
            size = "1024x1024",
            usage = new { num_output_tokens = 1024, num_input_text_tokens = 252, num_input_image_tokens = 1600 },
            data = new[] { new { b64_json = "iVBORw0KGgo=" } },
        }));
        var options = CallOptions();
        options.Files = new[]
        {
            ImageModelFile.FromFile(new byte[] { 1 }, "image/png"),
            ImageModelFile.FromFile(new byte[] { 2 }, "image/png"),
        };

        var result = await CreateProvider(handler).Image("MAI-Image-2.6-Flash").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(1852, result.Usage!.InputTokens);
        Assert.Equal(1024, result.Usage.OutputTokens);
        Assert.Equal(2876, result.Usage.TotalTokens);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"azure\":{\"images\":[{\"created\":1791229019,\"size\":\"1024x1024\",\"textTokens\":252,\"imageTokens\":1600}]}}"),
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }

    [Fact]
    [UpstreamTest(File + "downloads URL reference images with the abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task DownloadsUrlReferenceImagesWithTheAbortSignal()
    {
        var downloadTokenCanCancel = false;
        var handler = new AzureImageHandler((request, token) =>
        {
            if (request.Url == "https://example.com/apple.png")
            {
                downloadTokenCanCancel = token.CanBeCanceled;
                var image = new HttpResponseMessage { Content = new ByteArrayContent(new byte[] { 1 }) };
                image.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                return Task.FromResult(image);
            }

            return Task.FromResult(AzureImageHandler.JsonResponse(MaiResponse));
        });
        using var abort = new CancellationTokenSource();
        var options = CallOptions();
        options.Files = new[] { ImageModelFile.FromUrl("https://example.com/apple.png") };

        await CreateProvider(handler).Image("MAI-Image-2.6").DoGenerateAsync(options, abort.Token);

        Assert.True(downloadTokenCanCancel);
        Assert.Single(handler.Last.Parts(), part => part.Name == "image");
    }

    [Fact]
    [UpstreamTest(File + "stops before the MAI request when a download is aborted", Coverage = UpstreamCoverage.Covered)]
    public async Task StopsBeforeTheMaiRequestWhenADownloadIsAborted()
    {
        var downloadStarted = new TaskCompletionSource();
        var handler = new AzureImageHandler(async (request, token) =>
        {
            if (request.Url == "https://example.com/apple.png")
            {
                downloadStarted.SetResult();
                await Task.Delay(Timeout.Infinite, token);
            }

            return AzureImageHandler.JsonResponse(MaiResponse);
        });
        using var abort = new CancellationTokenSource();
        var options = CallOptions();
        options.Files = new[] { ImageModelFile.FromUrl("https://example.com/apple.png") };

        var result = CreateProvider(handler).Image("MAI-Image-2.6").DoGenerateAsync(options, abort.Token);
        await downloadStarted.Task;
        abort.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result);
        Assert.DoesNotContain(handler.Requests, request => request.Url.StartsWith("https://test-resource.services.ai.azure.com", StringComparison.Ordinal));
    }
}
