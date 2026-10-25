// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Util;
using Vercel.AI.WebFetch;
using static Vercel.AI.Tests.Upstream.WebFetchTool.WebFetchToolFixture;

namespace Vercel.AI.Tests.Upstream.WebFetchTool;

/// <summary>Port of <c>web-fetch-tool.test.ts</c> &gt; <c>createWebFetchTool</c> &gt; <c>truncation</c>.</summary>
public sealed class TruncationTests
{
    private const string Prefix = FilePath + "::createWebFetchTool > truncation::";

    [Fact]
    [UpstreamTest(Prefix + "truncates long content and continues at an offset from the cache", Coverage = UpstreamCoverage.Covered)]
    public async Task TruncatesLongContentAndContinuesAtAnOffsetFromTheCache()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text(new string('a', 25)));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, MaxLength = 10 });

        var first = await ExecuteAsync(webFetch, "https://example.com/");
        Assert.Equal(new string('a', 10), Content(first));
        Assert.Equal(0, first.GetProperty("offset").GetInt32());
        Assert.Equal(25, first.GetProperty("totalLength").GetInt32());
        Assert.Equal(10, first.GetProperty("nextOffset").GetInt32());
        var modelOutput = Assert.IsType<TextToolModelOutput>(ToModelOutput(webFetch, first));
        Assert.Contains("Call the tool again with offset 10 to continue reading.", modelOutput.Value);

        var last = await ExecuteAsync(webFetch, "https://example.com/", offset: 20);
        Assert.Equal(new string('a', 5), Content(last));
        Assert.Equal(20, last.GetProperty("offset").GetInt32());
        Assert.Equal(25, last.GetProperty("totalLength").GetInt32());
        Assert.False(last.TryGetProperty("nextOffset", out _));
        Assert.Single(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "refetches at offset 0", Coverage = UpstreamCoverage.Covered)]
    public async Task RefetchesAtOffset0()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text(new string('a', 25)));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, MaxLength = 10 });

        await ExecuteAsync(webFetch, "https://example.com/");
        await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal(2, fetch.Calls.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not split surrogate pairs", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotSplitSurrogatePairs()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("abc😀def"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, MaxLength = 4 });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("abc", Content(output));
        Assert.Equal(3, output.GetProperty("nextOffset").GetInt32());
    }

    [Fact]
    [UpstreamTest(Prefix + "returns empty content past the end", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsEmptyContentPastTheEnd()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("short"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, "https://example.com/", offset: 100);

        Assert.Equal(string.Empty, Content(output));
        Assert.Equal(5, output.GetProperty("totalLength").GetInt32());
        var modelOutput = Assert.IsType<TextToolModelOutput>(ToModelOutput(webFetch, output));
        Assert.Contains("No content at offset 100", modelOutput.Value);
    }
}
