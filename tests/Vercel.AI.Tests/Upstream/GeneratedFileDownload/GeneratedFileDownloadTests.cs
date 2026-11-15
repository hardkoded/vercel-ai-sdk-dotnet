// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GeneratedFileDownload;

/// <summary>Port of <c>generated-file-download.test.ts</c> &gt; <c>generated file downloads</c>.</summary>
[Collection("DownloadFetch")]
public sealed class GeneratedFileDownloadTests
{
    private const string Id = "packages/ai/src/generate-text/generated-file-download.test.ts::generated file downloads::";

    [Fact]
    [UpstreamTest(Id + "surfaces HTTP download failures during %s", Coverage = UpstreamCoverage.Partial, Note = "generate mode only: the port has no stream part that carries a file URL")]
    public async Task Surfaces_http_download_failures_during_generate()
    {
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) => Task.FromResult(new DownloadResponse(503, "Service Unavailable", null, null));
            var exception = await Assert.ThrowsAsync<DownloadError>(() => Client().GenerateTextAsync(new GenerateTextOptions { Model = FileModel("https://example.com/generated.txt"), Prompt = "image" }));
            Assert.Equal(503, exception.StatusCode);
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    [Fact]
    [UpstreamTest(Id + "rejects an unsafe file URL before fetching it", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_unsafe_file_url_before_fetching()
    {
        var model = FileModel("http://127.0.0.1/file");
        var fetches = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                fetches++;
                return Task.FromResult(DownloadResponse.Text("unexpected"));
            };
            var exception = await Assert.ThrowsAsync<DownloadException>(() => Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "image" }));
            Assert.Equal("AI_DownloadError", exception.Name);
            Assert.Contains("127.0.0.1", exception.Message);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(0, fetches);
        Assert.Single(model.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Id + "cancels a %s file download when aborted (custom download: %s)", Coverage = UpstreamCoverage.Partial, Note = "generate mode only: the port has no stream part that carries a file URL", Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Cancels_a_generate_file_download_when_aborted(bool customDownload)
    {
        var started = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        DownloadFetch fetch = async (url, request) =>
        {
            started.TrySetResult(request.AbortSignal);
            await Task.Delay(Timeout.Infinite, request.AbortSignal).ConfigureAwait(false);
            return DownloadResponse.Text("unreachable");
        };
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = fetch;
            using var abort = new CancellationTokenSource();
            var options = new GenerateTextOptions
            {
                Model = FileModel("https://example.com/generated.txt"),
                Prompt = "image",
                ExperimentalDownload = customDownload ? Download.CreateDownload(fetch: fetch) : null,
            };
            var operation = Client().GenerateTextAsync(options, abort.Token);
            var signal = await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            abort.Cancel();
            Assert.True(signal.IsCancellationRequested);
            await Assert.ThrowsAnyAsync<Exception>(() => operation);
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    internal static TestLanguageModel FileModel(string url)
    {
        return new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedFileUrl(new Uri(url), "text/plain") },
                FinishReason.Stop,
                LanguageModelUsage.Empty,
                "stop"),
        };
    }

    internal static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
