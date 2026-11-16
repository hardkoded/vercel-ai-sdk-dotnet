// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexMaasProviderNodeDownload;

/// <summary>Port of <c>google-vertex-maas-provider-node-download.test.ts</c> &gt; <c>MaaS %s image download transport</c>.</summary>
[Collection("DownloadFetch")]
public sealed class MaasImageDownloadTransportTests
{
    private const string Commit = "2ebc78b65435926718879f85a1a85687b053c542";

    private static readonly byte[] Png = { 1, 2, 3 };

    [Theory]
    [InlineData("default")]
    [InlineData("forwarded global")]
    [InlineData("custom")]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider-node-download.test.ts::MaaS %s image download transport::downloads images and redirect hops through the %s fetch without credentials", Coverage = UpstreamCoverage.Partial, Commit = Commit,
        Note = "Node and edge are one build. The custom variant uses DownloadHttpClient for downloads, where a redirect is an observable second request and not a fetch init flag, so its redirect mode and abort signal are not asserted.")]
    public async Task DownloadsImagesAndRedirectHopsThroughTheFetchWithoutCredentials(string transport)
    {
        var global = new Recording(Respond);
        var custom = new Recording(Respond);
        var previous = Download.Fetch;
        Download.Fetch = global.Fetch;
        try
        {
            var options = new GoogleVertexPartnerOptions
            {
                Project = "test-project",
                BaseUrl = "https://api.example/v1",
                GenerateAuthToken = _ => Task.FromResult("test-token"),
            };
            options.Headers["x-provider-key"] = "test-key";
            HttpMessageHandler? handler = null;
            if (transport == "forwarded global")
            {
                handler = new FetchHandler(global);
            }
            else if (transport == "custom")
            {
                handler = new FetchHandler(custom);
                options.DownloadHttpClient = new HttpClient(new FetchHandler(custom), disposeHandler: true);
            }

            var provider = new GoogleVertexMaasProvider(options, handler);
            using var cancellation = new CancellationTokenSource();

            var result = await provider.ImageModel("test-model").DoGenerateAsync(new ImageCallOptions("A small blue square") { Count = 1 }, cancellation.Token);

            Assert.Equal(Png, result.Images.Single().Data);
            var selected = transport == "custom" ? custom : global;
            Assert.Empty((transport == "custom" ? global : custom).Calls);
            Assert.Equal(
                new[] { "https://api.example/v1/images/generations", "https://images.example/image.png", "https://cdn.example/image.png" },
                selected.Calls.Select(call => call.Url));
            Assert.Equal("Bearer test-token", selected.Calls[0].Headers["Authorization"]);
            Assert.Equal("test-key", selected.Calls[0].Headers["x-provider-key"]);
            foreach (var download in selected.Calls.Skip(1))
            {
                if (transport != "custom")
                {
                    Assert.Equal("manual", download.Redirect);
                    Assert.Equal(cancellation.Token, download.Signal);
                }

                Assert.False(download.Headers.ContainsKey("Authorization"));
                Assert.False(download.Headers.ContainsKey("x-provider-key"));
            }
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider-node-download.test.ts::MaaS %s browser redirects::preserves the native follow retry after an opaque redirect", Coverage = UpstreamCoverage.Covered, Commit = Commit,
        Note = "Node and edge are one build. Download.IsBrowser stands in for the stubbed window global, and Download.Fetch for the custom fetch that serves the API call and the downloads.")]
    public async Task PreservesTheNativeFollowRetryAfterAnOpaqueRedirect()
    {
        var calls = new List<(string Url, string Redirect)>();
        var previousFetch = Download.Fetch;
        var previousBrowser = Download.IsBrowser;
        Download.IsBrowser = true;
        Download.Fetch = (url, init) =>
        {
            lock (calls)
            {
                calls.Add((url, init.Redirect));
            }

            if (url == "https://api.example/v1/images/generations")
            {
                return Task.FromResult(Json("{\"data\":[{\"url\":\"https://images.example/image.png\"}]}"));
            }

            return init.Redirect switch
            {
                "manual" => Task.FromResult(new DownloadResponse(0, string.Empty, null, null, "opaqueredirect")),
                "follow" => Task.FromResult(new DownloadResponse(200, "OK", null, Png)),
                _ => throw new InvalidOperationException("Unexpected redirect mode"),
            };
        };
        try
        {
            var provider = new GoogleVertexMaasProvider(new GoogleVertexPartnerOptions
            {
                Project = "test-project",
                BaseUrl = "https://api.example/v1",
                GenerateAuthToken = _ => Task.FromResult("test-token"),
            });

            var result = await provider.ImageModel("test-model").DoGenerateAsync(new ImageCallOptions("A small blue square") { Count = 1 }, CancellationToken.None);

            Assert.Equal(Png, result.Images.Single().Data);
            Assert.Equal(3, calls.Count);
            Assert.Equal(("https://images.example/image.png", "manual"), calls[1]);
            Assert.Equal(("https://images.example/image.png", "follow"), calls[2]);
        }
        finally
        {
            Download.Fetch = previousFetch;
            Download.IsBrowser = previousBrowser;
        }
    }

    private static DownloadResponse Json(string body)
    {
        var headers = new HeaderCollection();
        headers.Add("content-type", "application/json");
        return new DownloadResponse(200, "OK", headers, System.Text.Encoding.UTF8.GetBytes(body));
    }

    private static DownloadResponse Respond(string url)
    {
        switch (url)
        {
            case "https://api.example/v1/images/generations":
                return Json("{\"data\":[{\"url\":\"https://images.example/image.png\"}]}");
            case "https://images.example/image.png":
                var headers = new HeaderCollection();
                headers.Add("location", "https://cdn.example/image.png");
                return new DownloadResponse(302, "Found", headers, null);
            case "https://cdn.example/image.png":
                return new DownloadResponse(200, "OK", null, Png);
            default:
                throw new InvalidOperationException("Unexpected URL: " + url);
        }
    }

    private sealed record Call(string Url, string Redirect, CancellationToken Signal, Dictionary<string, string> Headers);

    private sealed class Recording(Func<string, DownloadResponse> respond)
    {
        public List<Call> Calls { get; } = [];

        public Task<DownloadResponse> Fetch(string url, DownloadRequest init)
        {
            lock (Calls)
            {
                Calls.Add(new Call(url, init.Redirect, init.AbortSignal, init.Headers.Entries().ToDictionary(h => h.Key, h => h.Value, StringComparer.OrdinalIgnoreCase)));
            }

            return Task.FromResult(respond(url));
        }
    }

    /// <summary>A transport that sends each request through a recording fetch.</summary>
    private sealed class FetchHandler(Recording fetch) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var init = new DownloadRequest(null, cancellationToken, "manual");
            foreach (var header in request.Headers)
            {
                init.Headers.Add(header.Key, string.Join(",", header.Value));
            }

            var url = request.RequestUri!.AbsoluteUri;
            var response = await fetch.Fetch(url, init).ConfigureAwait(false);
            var message = new HttpResponseMessage((HttpStatusCode)response.Status) { Content = new ByteArrayContent(response.Body ?? []) };
            var location = response.Headers.Get("location");
            if (location != null)
            {
                message.Headers.Location = new Uri(location);
            }

            return message;
        }
    }
}
