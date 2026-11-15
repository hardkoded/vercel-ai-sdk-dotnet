// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http;
using System.Text;
using Vercel.AI.OpenAI;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.DownloadBlob;

/// <summary>Port of <c>download-blob.test.ts</c> &gt; <c>downloadBlob()</c>. The port's <c>downloadBlob</c> is <c>ImageFileDownload.DownloadBlobAsync</c>.</summary>
public sealed class DownloadBlobTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/download-blob.test.ts::downloadBlob()::uses an injected transport while retaining URL and redirect checks",
        Coverage = UpstreamCoverage.Partial,
        Note = "The injected transport is OpenAICompatibleOptions.DownloadHttpClient. It does not take a per-request redirect flag, so the request init is not checked.",
        Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Uses_an_injected_transport_while_retaining_url_and_redirect_checks()
    {
        var requested = new List<string>();
        var transport = new Handler(request =>
        {
            requested.Add(request.RequestUri!.AbsoluteUri);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Encoding.UTF8.GetBytes("content")) };
        });
        var runtime = new OpenAICapture();
        var provider = OpenAIUpstream.Provider(runtime, options => options.DownloadHttpClient = new HttpClient(transport, disposeHandler: false));

        var blob = await ImageFileDownload.DownloadBlobAsync(provider, new Dictionary<string, string?>(), "https://files.example.com/file", CancellationToken.None);
        Assert.Equal("content", Encoding.UTF8.GetString(blob.Data));

        await Assert.ThrowsAsync<Vercel.AI.GenerateText.DownloadException>(() => ImageFileDownload.DownloadBlobAsync(provider, new Dictionary<string, string?>(), "http://127.0.0.1/file", CancellationToken.None));

        Assert.Equal(new[] { "https://files.example.com/file" }, requested);
        Assert.Equal(0, runtime.Calls);
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(respond(request));
        }
    }
}
