// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.GenerateText;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GeneratedFileDownload;

/// <summary>Port of <c>generated-file-download.test.ts</c> &gt; <c>generated file downloads &gt; experimental_download</c>.</summary>
[Collection("DownloadFetch")]
public sealed class ExperimentalDownloadTests
{
    private const string Id = "packages/ai/src/generate-text/generated-file-download.test.ts::generated file downloads > experimental_download::";

    [Fact]
    [UpstreamTest(Id + "downloads %s file URLs through the configured download function instead of global fetch", Coverage = UpstreamCoverage.Partial, Note = "generate mode only: the port has no stream part that carries a file URL", Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Downloads_generate_file_urls_through_the_configured_download_function_instead_of_global_fetch()
    {
        var globalFetches = 0;
        var calls = new List<IReadOnlyList<DownloadRequestItem>>();
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                globalFetches++;
                return Task.FromResult(new DownloadResponse(500, "Unexpected", null, null));
            };
            var result = await GeneratedFileDownloadTests.Client().GenerateTextAsync(new GenerateTextOptions
            {
                Model = GeneratedFileDownloadTests.FileModel("https://example.com/generated.txt"),
                Prompt = "image",
                ExperimentalDownload = requests =>
                {
                    calls.Add(requests);
                    return Task.FromResult<IReadOnlyList<DownloadResult>>(requests.Select(_ => new DownloadResult(Encoding.UTF8.GetBytes("Hello World"), "text/plain")).ToArray());
                },
            });

            Assert.Equal(Encoding.UTF8.GetBytes("Hello World"), result.Files[0].Data);
        }
        finally
        {
            Download.Fetch = previous;
        }

        // The prompt has no URLs, so the port does not call the download function for it.
        var call = Assert.Single(calls);
        var request = Assert.Single(call);
        Assert.Equal(new Uri("https://example.com/generated.txt"), request.Url);
        Assert.False(request.IsUrlSupportedByModel);
        Assert.Equal(0, globalFetches);
    }

    [Fact]
    [UpstreamTest(Id + "uses the fetch transport of createDownload for generated file URLs", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Uses_the_fetch_transport_of_createDownload_for_generated_file_urls()
    {
        var globalFetches = 0;
        var fetched = new List<(string Url, string Redirect)>();
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                globalFetches++;
                return Task.FromResult(new DownloadResponse(500, "Unexpected", null, null));
            };
            var result = await GeneratedFileDownloadTests.Client().GenerateTextAsync(new GenerateTextOptions
            {
                Model = GeneratedFileDownloadTests.FileModel("https://example.com/generated.txt"),
                Prompt = "image",
                ExperimentalDownload = Download.CreateDownload(fetch: (url, request) =>
                {
                    fetched.Add((url, request.Redirect));
                    return Task.FromResult(DownloadResponse.Text("Hello World"));
                }),
            });

            Assert.Equal(Encoding.UTF8.GetBytes("Hello World"), result.Files[0].Data);
        }
        finally
        {
            Download.Fetch = previous;
        }

        var single = Assert.Single(fetched);
        Assert.Equal("https://example.com/generated.txt", single.Url);
        Assert.Equal("manual", single.Redirect);
        Assert.Equal(0, globalFetches);
    }

    [Fact]
    [UpstreamTest(Id + "rejects a null result for a generated file URL", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Rejects_a_null_result_for_a_generated_file_url()
    {
        var exception = await Assert.ThrowsAsync<DownloadException>(() => GeneratedFileDownloadTests.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = GeneratedFileDownloadTests.FileModel("https://example.com/generated.txt"),
            Prompt = "image",
            ExperimentalDownload = requests => Task.FromResult<IReadOnlyList<DownloadResult>>(requests.Select(_ => (DownloadResult)null!).ToArray()),
        }));

        Assert.Equal("AI_DownloadError", exception.Name);
        Assert.Equal("https://example.com/generated.txt", exception.Url);
        Assert.Contains("returned null", exception.Message);
    }
}
