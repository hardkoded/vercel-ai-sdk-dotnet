// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.AspNetCore;
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
    [UpstreamTest(Id + "emits downloaded HTTPS file content as a usable UI data URL", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Emits_downloaded_https_file_content_as_a_usable_ui_data_url()
    {
        var fetches = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                fetches++;
                return Task.FromResult(DownloadResponse.Text("Hello World"));
            };
            var result = Client().StreamTextAsync(new StreamTextOptions { Model = FileModel("https://example.com/generated.txt"), Prompt = "image" });
            var chunks = new List<System.Text.Json.Nodes.JsonObject>();
            await foreach (var chunk in UIMessageStreamConverter.ToUIMessageStream(result.Stream()))
            {
                chunks.Add(chunk);
            }

            var file = Assert.Single(chunks, chunk => (string?)chunk["type"] == "file");
            Assert.Equal("text/plain", (string?)file["mediaType"]);
            Assert.Equal("data:text/plain;base64,SGVsbG8gV29ybGQ=", (string?)file["url"]);
            Assert.Equal(Encoding.UTF8.GetBytes("Hello World"), (await result.Files)[0].Data);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(1, fetches);
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest(Id + "surfaces HTTP download failures during %s", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Surfaces_http_download_failures(string mode)
    {
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) => Task.FromResult(new DownloadResponse(503, "Service Unavailable", null, null));
            var model = FileModel("https://example.com/generated.txt");
            DownloadError exception;
            if (mode == "generate")
            {
                exception = await Assert.ThrowsAsync<DownloadError>(() => Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "image" }));
            }
            else
            {
                var result = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "image", OnError = (_, _) => Task.CompletedTask });
                exception = await Assert.ThrowsAsync<DownloadError>(async () =>
                {
                    await foreach (var _ in result.Stream())
                    {
                    }
                });
            }

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
    [InlineData("generate", false)]
    [InlineData("stream", false)]
    [InlineData("generate", true)]
    [InlineData("stream", true)]
    [UpstreamTest(Id + "cancels a %s file download when aborted (custom download: %s)", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Cancels_a_file_download_when_aborted(string mode, bool customDownload)
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
            var model = FileModel("https://example.com/generated.txt");
            var download = customDownload ? Download.CreateDownload(fetch: fetch) : null;
            Task operation;
            if (mode == "generate")
            {
                operation = Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "image", ExperimentalDownload = download }, abort.Token);
            }
            else
            {
                var result = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "image", ExperimentalDownload = download, OnError = (_, _) => Task.CompletedTask }, abort.Token);
                operation = Task.Run(async () =>
                {
                    await foreach (var _ in result.Stream(abort.Token))
                    {
                    }
                });
            }

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
        var uri = new Uri(url);
        return new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedFileUrl(uri, "text/plain") },
                FinishReason.Stop,
                LanguageModelUsage.Empty,
                "stop"),
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new FileStreamPart(uri, "text/plain"),
                new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty, "stop"),
            },
        };
    }

    internal static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
