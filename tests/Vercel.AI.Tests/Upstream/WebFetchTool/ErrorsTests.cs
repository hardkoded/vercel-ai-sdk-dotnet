// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;
using Vercel.AI.WebFetch;
using static Vercel.AI.Tests.Upstream.WebFetchTool.WebFetchToolFixture;

namespace Vercel.AI.Tests.Upstream.WebFetchTool;

/// <summary>Port of <c>web-fetch-tool.test.ts</c> &gt; <c>createWebFetchTool</c> &gt; <c>errors</c>.</summary>
public sealed class ErrorsTests
{
    private const string Prefix = FilePath + "::createWebFetchTool > errors::";

    [Fact]
    [UpstreamTest(Prefix + "returns HTTP errors", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsHttpErrors()
    {
        var fetch = new RecordingFetch((_, _) => new DownloadResponse(404, "Not Found", null, System.Text.Encoding.UTF8.GetBytes("missing")));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal(
            "{\"type\":\"error\",\"url\":\"https://example.com/\",\"errorCode\":\"http_error\",\"message\":\"The request failed with HTTP 404 Not Found.\",\"statusCode\":404}",
            output.GetRawText());
        var modelOutput = Assert.IsType<ErrorTextToolModelOutput>(ToModelOutput(webFetch, output));
        Assert.Equal("The request failed with HTTP 404 Not Found.", modelOutput.Value);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports bot protection challenges", Coverage = UpstreamCoverage.Covered)]
    public async Task ReportsBotProtectionChallenges()
    {
        var headers = new HeaderCollection();
        headers.Add("cf-mitigated", "challenge");
        var fetch = new RecordingFetch((_, _) => new DownloadResponse(403, "Forbidden", headers, System.Text.Encoding.UTF8.GetBytes("challenge")));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("http_error", ErrorCode(output));
        Assert.Contains("blocked by bot protection", output.GetProperty("message").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "returns network errors", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsNetworkErrors()
    {
        var fetch = new RecordingFetch((Func<string, DownloadRequest, Task<DownloadResponse>>)((_, _) => throw new HttpRequestException("fetch failed")));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("network_error", ErrorCode(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "limits the response size", Coverage = UpstreamCoverage.Covered)]
    public async Task LimitsTheResponseSize()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text(new string('a', 100)));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, MaxBytes = 10 });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("too_large", ErrorCode(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "times out slow requests", Coverage = UpstreamCoverage.Covered)]
    public async Task TimesOutSlowRequests()
    {
        var fetch = new RecordingFetch(async (_, request) =>
        {
            await Task.Delay(Timeout.Infinite, request.AbortSignal);
            return DownloadResponse.Text("never");
        });
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, Timeout = TimeSpan.FromMilliseconds(20) });
        using var controller = new CancellationTokenSource();

        var output = await ExecuteAsync(webFetch, "https://example.com/", abortSignal: controller.Token);

        Assert.Equal("timeout", ErrorCode(output));
        Assert.False(controller.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "propagates caller cancellation", Coverage = UpstreamCoverage.Covered, Note = "A CancellationToken has no reason, so the test checks for the cancellation instead of the 'Cancelled' message.")]
    public async Task PropagatesCallerCancellation()
    {
        var fetch = new RecordingFetch(async (_, request) =>
        {
            await Task.Delay(Timeout.Infinite, request.AbortSignal);
            return DownloadResponse.Text("never");
        });
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });
        using var controller = new CancellationTokenSource();

        var result = ExecuteAsync(webFetch, "https://example.com/", abortSignal: controller.Token);
        controller.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result);
    }
}
