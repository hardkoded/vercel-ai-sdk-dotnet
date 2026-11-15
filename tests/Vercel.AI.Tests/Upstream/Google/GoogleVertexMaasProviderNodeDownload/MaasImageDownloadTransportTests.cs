// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexMaasProviderNodeDownload;

/// <summary>Port of <c>google-vertex-maas-provider-node-download.test.ts</c> &gt; <c>MaaS %s image download transport</c>.</summary>
public sealed class MaasImageDownloadTransportTests
{
    private const string Commit = "2ebc78b65435926718879f85a1a85687b053c542";

    private static readonly byte[] Png = { 1, 2, 3 };

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider-node-download.test.ts::MaaS %s image download transport::downloads images and redirect hops through the %s fetch without credentials", Coverage = UpstreamCoverage.Partial, Commit = Commit,
        Note = "Covers the custom download client. A .NET provider sends API calls through its handler and downloads through DownloadHttpClient, so there is no single fetch for the default and forwarded-global variants, and node and edge are one build. A redirect is an observable second request, not a fetch init flag.")]
    public async Task DownloadsImagesAndRedirectHopsThroughTheCustomFetchWithoutCredentials()
    {
        var api = new StubHandler(request =>
            Task.FromResult(new HttpResponseMessage { Content = new StringContent("{\"data\":[{\"url\":\"https://images.example/image.png\"}]}", System.Text.Encoding.UTF8, "application/json") }));
        var downloads = new StubHandler(request =>
        {
            switch (request.RequestUri!.AbsoluteUri)
            {
                case "https://images.example/image.png":
                    var redirect = new HttpResponseMessage(HttpStatusCode.Found);
                    redirect.Headers.Location = new Uri("https://cdn.example/image.png");
                    return Task.FromResult(redirect);
                case "https://cdn.example/image.png":
                    return Task.FromResult(new HttpResponseMessage { Content = new ByteArrayContent(Png) });
                default:
                    throw new InvalidOperationException("Unexpected URL: " + request.RequestUri);
            }
        });
        var options = new GoogleVertexPartnerOptions
        {
            Project = "test-project",
            BaseUrl = "https://api.example/v1",
            GenerateAuthToken = _ => Task.FromResult("test-token"),
            DownloadHttpClient = new HttpClient(downloads),
        };
        options.Headers["x-provider-key"] = "test-key";
        var provider = new GoogleVertexMaasProvider(options, api);
        using var cancellation = new CancellationTokenSource();

        var result = await provider.ImageModel("test-model").DoGenerateAsync(new ImageCallOptions("A small blue square") { Count = 1 }, cancellation.Token);

        Assert.Equal(Png, result.Images.Single().Data);
        var apiRequest = Assert.Single(api.Requests);
        Assert.Equal("https://api.example/v1/images/generations", apiRequest.Url);
        Assert.Equal("Bearer test-token", apiRequest.Headers["Authorization"]);
        Assert.Equal("test-key", apiRequest.Headers["x-provider-key"]);
        Assert.Equal(
            new[] { "https://images.example/image.png", "https://cdn.example/image.png" },
            downloads.Requests.Select(request => request.Url));
        foreach (var download in downloads.Requests)
        {
            Assert.False(download.Headers.ContainsKey("Authorization"));
            Assert.False(download.Headers.ContainsKey("x-provider-key"));
        }
    }

    private sealed record Recorded(string Url, Dictionary<string, string> Headers);

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public List<Recorded> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new Recorded(request.RequestUri!.AbsoluteUri, request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value), StringComparer.OrdinalIgnoreCase)));
            return respond(request);
        }
    }
}
