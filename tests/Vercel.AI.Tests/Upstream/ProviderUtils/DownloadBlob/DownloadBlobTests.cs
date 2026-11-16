// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http;
using System.Text;
using Vercel.AI.OpenAI;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.DownloadBlob;

/// <summary>Port of <c>download-blob.test.ts</c> &gt; <c>downloadBlob()</c>. The port's <c>downloadBlob</c> is <c>ImageFileDownload.DownloadBlobAsync</c>.</summary>
[Collection("DownloadFetch")]
public sealed class DownloadBlobTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/download-blob.test.ts::downloadBlob()::uses an injected transport while retaining URL and redirect checks",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream also checks signal is undefined. HttpClient always wraps the caller token with its own timeout token, so the port cannot assert an absent signal.",
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Uses_an_injected_transport_while_retaining_url_and_redirect_checks()
    {
        // The injected transport is the download client. Its request init is the DownloadRequest handed to Download.Fetch.
        var requests = new List<(string Url, string Redirect)>();
        var runtime = new Handler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                requests.Add((url, request.Redirect));
                return Task.FromResult(DownloadResponse.Text("content"));
            };
            var options = new OpenAIOptions
            {
                ApiKey = "test-api-key",
                DownloadHttpClient = new HttpClient(
                    new GlobalFetchHandler(allowAutoRedirect: false)
                ),
            };
            var provider = OpenAIProvider.Create(options, runtime);

            var blob = await ImageFileDownload.DownloadBlobAsync(
                provider,
                new Dictionary<string, string?>(),
                "https://files.example.com/file",
                CancellationToken.None
            );
            Assert.Equal("content", Encoding.UTF8.GetString(blob.Data));
            Assert.Equal(new[] { ("https://files.example.com/file", "manual") }, requests);

            await Assert.ThrowsAsync<Vercel.AI.GenerateText.DownloadException>(() =>
                ImageFileDownload.DownloadBlobAsync(
                    provider,
                    new Dictionary<string, string?>(),
                    "http://127.0.0.1/file",
                    CancellationToken.None
                )
            );
            Assert.Single(requests);
            Assert.Equal(0, runtime.Calls);
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            return Task.FromResult(respond(request));
        }
    }
}
