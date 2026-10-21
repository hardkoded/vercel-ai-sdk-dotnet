// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Luma;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Luma.LumaImageModel;

/// <summary>Port of <c>luma-image-model.test.ts</c> &gt; <c>LumaImageModel</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix = "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate::";
    private const string Prompt = "A cute baby sea otter";

    [Fact]
    [UpstreamTest(Prefix + "should encode the generation ID as a single URL path segment", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_the_generation_ID_as_a_single_URL_path_segment()
    {
        var generationId = "abc/../../internal";
        var queued = "{\"id\":\"" + generationId + "\",\"generation_type\":\"image\",\"state\":\"queued\",\"created_at\":\"2024-01-01T00:00:00Z\",\"model\":\"test-model\",\"request\":{\"generation_type\":\"image\",\"model\":\"test-model\",\"prompt\":\"" + Prompt + "\"}}";
        var completed = "{\"id\":\"" + generationId + "\",\"generation_type\":\"image\",\"state\":\"completed\",\"created_at\":\"2024-01-01T00:00:00Z\",\"assets\":{\"image\":\"https://api.example.com/image.png\"},\"model\":\"test-model\",\"request\":{\"generation_type\":\"image\",\"model\":\"test-model\",\"prompt\":\"" + Prompt + "\"}}";
        var handler = new ParityHandler(call => call.Uri.AbsoluteUri switch
        {
            "https://api.example.com/dream-machine/v1/generations/image" => ParityHandler.Json(queued),
            "https://api.example.com/dream-machine/v1/generations/abc%2F..%2F..%2Finternal" => ParityHandler.Json(completed),
            "https://api.example.com/image.png" => ParityHandler.Bytes(Encoding.UTF8.GetBytes("test-binary-content"), null),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound),
        });
        var options = new OpenAICompatibleOptions { BaseUrl = "https://api.example.com" };
        options.Headers["api-key"] = "test-key";

        await LumaProvider.Create(options, handler).GenerateImageAsync("test-model", new LumaImageRequest(Prompt) { AspectRatio = "16:9" }, CancellationToken.None);

        Assert.Equal("https://api.example.com/dream-machine/v1/generations/" + Uri.EscapeDataString(generationId), handler.Calls[1].Uri.AbsoluteUri);
    }
}
