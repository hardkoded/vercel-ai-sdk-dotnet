// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureImageDownloadAuth;

/// <summary>Port of <c>azure-image-download-auth.test.ts</c> &gt; <c>%s fetch</c> &gt; <c>%s authentication</c>.</summary>
public sealed class FetchTests
{
    private const string Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d";

    [Theory]
    [InlineData("token", "MAI-Image-2.6")]
    [InlineData("token", "gpt-image-1")]
    [InlineData("api-key", "MAI-Image-2.6")]
    [InlineData("api-key", "gpt-image-1")]
    [UpstreamTest("packages/azure/src/azure-image-download-auth.test.ts::%s fetch > %s authentication::keeps credentials off %s image downloads and redirects", Coverage = UpstreamCoverage.Partial, Commit = Commit,
        Note = "Covers the custom fetch transport. The port has no global fetch to stub, so the global transport variant is not ported. A redirect is an observable second request, not a fetch init flag.")]
    public async Task KeepsCredentialsOffImageDownloadsAndRedirects(string auth, string modelId)
    {
        var handler = new AzureImageHandler((request, _) =>
        {
            switch (request.Url)
            {
                case "https://files.example.com/image.png":
                    var redirect = new HttpResponseMessage(HttpStatusCode.Found);
                    redirect.Headers.Location = new Uri("https://cdn.example.com/image.png");
                    return Task.FromResult(redirect);
                case "https://cdn.example.com/image.png":
                case "https://files.example.com/mask.png":
                    var image = new HttpResponseMessage { Content = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4e, 0x47 }) };
                    image.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    return Task.FromResult(image);
                default:
                    return Task.FromResult(AzureImageHandler.JsonResponse(new { data = new[] { new { b64_json = "iVBORw0KGgo=" } } }));
            }
        });
        var tokenCalls = 0;
        var provider = CreateProvider(handler, options =>
        {
            if (auth == "token")
            {
                options.ApiKey = null;
                options.TokenProvider = () =>
                {
                    tokenCalls++;
                    return "entra-token";
                };
            }
            else
            {
                options.ApiKey = "azure-api-key";
            }

            options.Headers["x-provider-secret"] = "provider-secret";
        });
        var isMai = modelId == "MAI-Image-2.6";
        var call = new ImageCallOptions("Edit this image")
        {
            Count = 1,
            Files = new[] { ImageModelFile.FromUrl("https://files.example.com/image.png") },
            Mask = isMai ? null : ImageModelFile.FromUrl("https://files.example.com/mask.png"),
        };

        await provider.Image(modelId).DoGenerateAsync(call, CancellationToken.None);

        var downloads = handler.Requests.Take(handler.Requests.Count - 1).ToList();
        var expectedDownloads = new List<string> { "https://files.example.com/image.png", "https://cdn.example.com/image.png" };
        if (!isMai)
        {
            expectedDownloads.Add("https://files.example.com/mask.png");
        }

        Assert.Equal(expectedDownloads.OrderBy(url => url), downloads.Select(download => download.Url).OrderBy(url => url));
        foreach (var download in downloads)
        {
            Assert.False(download.Headers.ContainsKey("authorization"));
            Assert.False(download.Headers.ContainsKey("api-key"));
            Assert.False(download.Headers.ContainsKey("x-provider-secret"));
        }

        var api = handler.Last;
        Assert.Equal(
            isMai
                ? "https://test-resource.services.ai.azure.com/mai/v1/images/edits"
                : "https://test-resource.openai.azure.com/openai/v1/images/edits?api-version=v1",
            api.Url);
        Assert.Equal("provider-secret", api.Headers["x-provider-secret"]);
        if (auth == "token")
        {
            Assert.Equal("Bearer entra-token", api.Headers["authorization"]);
            Assert.Equal(1, tokenCalls);
        }
        else
        {
            Assert.Equal("azure-api-key", api.Headers["api-key"]);
            Assert.Equal(0, tokenCalls);
        }
    }
}
