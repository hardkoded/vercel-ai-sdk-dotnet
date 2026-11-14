// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Util.CreateDownload;

/// <summary>Port of <c>create-download.test.ts</c>.</summary>
[Collection("DownloadFetch")]
public sealed class CreateDownloadTests
{
    private const string Id = "packages/ai/src/util/download/create-download.test.ts::";

    [Fact]
    [UpstreamTest(Id + "forwards the fetch transport, abort signal, and download size limit", Coverage = UpstreamCoverage.Partial, Note = "called with a one-item batch: the port's CreateDownload has no single-request shape", Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Forwards_the_fetch_transport_abort_signal_and_download_size_limit()
    {
        using var abort = new CancellationTokenSource();
        var seen = new List<(string Url, CancellationToken Signal, string Redirect)>();
        var download = Download.CreateDownload(
            maxBytes: 3,
            fetch: (url, request) =>
            {
                seen.Add((url, request.AbortSignal, request.Redirect));
                return Task.FromResult(DownloadResponse.Bytes(Encoding.UTF8.GetBytes("too large"), "text/plain"));
            });

        await Assert.ThrowsAsync<DownloadError>(() => download(new[] { new DownloadRequestItem(new Uri("https://files.example.com/file"), false) { AbortSignal = abort.Token } }));

        var call = Assert.Single(seen);
        Assert.Equal("https://files.example.com/file", call.Url);
        Assert.Equal(abort.Token, call.Signal);
        Assert.Equal("manual", call.Redirect);
    }

    [Fact]
    [UpstreamTest(Id + "acts as an experimental_download function when called with a batch", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Acts_as_an_experimental_download_function_when_called_with_a_batch()
    {
        var urls = new List<string>();
        var download = Download.CreateDownload(fetch: (url, request) =>
        {
            urls.Add(url);
            return Task.FromResult(DownloadResponse.Bytes(Encoding.UTF8.GetBytes("Hello World"), "text/plain"));
        });

        var results = await download(new[]
        {
            new DownloadRequestItem(new Uri("https://files.example.com/supported"), true),
            new DownloadRequestItem(new Uri("https://files.example.com/unsupported"), false),
        });

        Assert.Null(results[0]);
        Assert.Equal(Encoding.UTF8.GetBytes("Hello World"), results[1].Data);
        Assert.Equal("text/plain", results[1].MediaType);
        Assert.Equal(new[] { "https://files.example.com/unsupported" }, urls);
    }
}
