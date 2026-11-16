// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Azure.AzureImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureImageDownloadAuth;

/// <summary>Port of <c>azure-image-download-auth.test.ts</c> &gt; <c>%s fetch</c> &gt; <c>%s authentication</c>.</summary>
[Collection("DownloadFetch")]
public sealed class FetchTests
{
    private const string Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d";

    [Theory]
    [InlineData("custom", "token", "MAI-Image-2.6")]
    [InlineData("custom", "token", "gpt-image-1")]
    [InlineData("custom", "api-key", "MAI-Image-2.6")]
    [InlineData("custom", "api-key", "gpt-image-1")]
    [InlineData("global", "token", "MAI-Image-2.6")]
    [InlineData("global", "token", "gpt-image-1")]
    [InlineData("global", "api-key", "MAI-Image-2.6")]
    [InlineData("global", "api-key", "gpt-image-1")]
    [UpstreamTest("packages/azure/src/azure-image-download-auth.test.ts::%s fetch > %s authentication::keeps credentials off %s image downloads and redirects", Coverage = UpstreamCoverage.Covered, Commit = Commit,
        Note = "The global transport variant replaces Download.Fetch. A redirect is an observable second request in the custom variant, and the redirect mode is asserted in the global variant.")]
    public async Task KeepsCredentialsOffImageDownloadsAndRedirects(string transport, string auth, string modelId)
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
        var redirectModes = new List<string>();
        var previousFetch = Download.Fetch;
        using var invoker = new HttpMessageInvoker(handler, disposeHandler: false);
        try
        {
            if (transport == "global")
            {
                Download.Fetch = async (url, init) =>
                {
                    lock (redirectModes)
                    {
                        redirectModes.Add(init.Redirect);
                    }

                    using var message = new HttpRequestMessage(new HttpMethod(init.Method), url);
                    foreach (var header in init.Headers.Entries())
                    {
                        message.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    if (init.Body != null)
                    {
                        message.Content = new ByteArrayContent(init.Body);
                    }

                    using var response = await invoker.SendAsync(message, init.AbortSignal).ConfigureAwait(false);
                    var headers = new HeaderCollection();
                    foreach (var header in response.Headers)
                    {
                        headers.Add(header.Key, string.Join(",", header.Value));
                    }

                    foreach (var header in response.Content.Headers)
                    {
                        headers.Add(header.Key, string.Join(",", header.Value));
                    }

                    var body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    return new DownloadResponse((int)response.StatusCode, response.ReasonPhrase, headers, body);
                };
            }

            await RunAsync(transport, auth, modelId, handler, redirectModes).ConfigureAwait(false);
        }
        finally
        {
            Download.Fetch = previousFetch;
        }
    }

    private static async Task RunAsync(string transport, string auth, string modelId, AzureImageHandler handler, List<string> redirectModes)
    {
        var tokenCalls = 0;
        var provider = CreateProvider(transport == "custom" ? handler : null, options =>
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

        if (transport == "global")
        {
            Assert.Equal(downloads.Count + 1, redirectModes.Count);
            Assert.All(redirectModes.Take(downloads.Count), mode => Assert.Equal("manual", mode));
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
