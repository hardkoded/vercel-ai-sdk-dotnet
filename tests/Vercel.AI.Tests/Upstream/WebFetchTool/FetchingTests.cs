// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text;
using Vercel.AI.Prompt;
using Vercel.AI.Util;
using Vercel.AI.WebFetch;
using static Vercel.AI.Tests.Upstream.WebFetchTool.WebFetchToolFixture;

namespace Vercel.AI.Tests.Upstream.WebFetchTool;

/// <summary>Port of <c>web-fetch-tool.test.ts</c> &gt; <c>createWebFetchTool</c> &gt; <c>fetching</c>.</summary>
public sealed class FetchingTests
{
    private const string Prefix = FilePath + "::createWebFetchTool > fetching::";

    private static Tool Tool(Func<string, DownloadRequest, DownloadResponse> handler, WebFetchToolOptions? options = null)
    {
        options ??= new WebFetchToolOptions();
        options.Fetch = new RecordingFetch(handler).Fetch;
        return WebFetchTools.Create(options);
    }

    [Fact]
    [UpstreamTest(Prefix + "converts HTML to markdown with the main content", Coverage = UpstreamCoverage.Covered, Note = "The HTML is converted by AngleSharp and a built-in extractor, not by defuddle.")]
    public async Task ConvertsHtmlToMarkdownWithTheMainContent()
    {
        var webFetch = Tool((_, _) => HtmlResponse(ArticleHtml, "text/html; charset=utf-8"));

        var output = await ExecuteAsync(webFetch, "https://example.com/blog/post#section");

        Assert.Equal("document", output.GetProperty("type").GetString());
        Assert.Equal("https://example.com/blog/post#section", output.GetProperty("url").GetString());
        Assert.Equal("https://example.com/blog/post", output.GetProperty("finalUrl").GetString());
        Assert.Equal("text/html", output.GetProperty("contentType").GetString());
        Assert.Equal("Test Page", output.GetProperty("title").GetString());
        Assert.Equal(0, output.GetProperty("offset").GetInt32());
        Assert.Contains("The quick brown fox", Content(output));
        // Relative links are resolved against the page URL.
        Assert.Contains("[guide](https://example.com/docs/guide)", Content(output));
        Assert.DoesNotContain("Copyright", Content(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "sends GET requests preferring markdown with an identifying user agent", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsGetRequestsPreferringMarkdownWithAnIdentifyingUserAgent()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("# Hello", "text/markdown"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("document", output.GetProperty("type").GetString());
        Assert.Equal("text/markdown", output.GetProperty("contentType").GetString());
        Assert.Equal("# Hello", Content(output));
        var request = fetch.Calls[0].Request;
        Assert.StartsWith("text/markdown, ", request.Headers.Get("accept"));
        // Each product token has a single `/` (RFC 9110).
        Assert.Matches(@"^ai-sdk-tool-web-fetch/[^\s/]+( |$)", request.Headers.Get("user-agent"));
        Assert.Equal("GET", request.Method);
    }

    [Fact]
    [UpstreamTest(Prefix + "passes defuddle options to the content extractor", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesDefuddleOptionsToTheContentExtractor()
    {
        var webFetch = Tool(
            (_, _) => HtmlResponse(Html("""
                <main><p>Main content that is long enough to be selected by the extractor as the primary content of this page.</p></main>
                <aside id="sidebar"><p>Sidebar content that should be selected.</p></aside>
                """)),
            new WebFetchToolOptions { DefuddleOptions = new WebFetchDefuddleOptions { ContentSelector = "#sidebar" } });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("Sidebar content that should be selected.", Content(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "extracts HTML without an <html> element", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsHtmlWithoutAnHtmlElement()
    {
        var paragraph = string.Concat(Enumerable.Repeat("The quick brown fox jumps over the lazy dog in the meadow. ", 3));
        var webFetch = Tool((_, _) => HtmlResponse("<!doctype html><title>No Root</title><h1>Heading</h1><p>" + paragraph + "</p><p>" + paragraph + "</p>"));

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("document", output.GetProperty("type").GetString());
        Assert.Equal("No Root", output.GetProperty("title").GetString());
        Assert.Contains("The quick brown fox", Content(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "returns empty content for pages that require JavaScript", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsEmptyContentForPagesThatRequireJavaScript()
    {
        var webFetch = Tool((_, _) => HtmlResponse(Html("<div id=\"root\"></div><script src=\"/app.js\"></script>", "<title>My App</title>")));

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("document", output.GetProperty("type").GetString());
        Assert.Equal(string.Empty, Content(output));
        var modelOutput = Assert.IsType<TextToolModelOutput>(ToModelOutput(webFetch, output));
        Assert.Contains("may require JavaScript", modelOutput.Value);
    }

    [Fact]
    [UpstreamTest(Prefix + "normalizes whitespace in linear time", Coverage = UpstreamCoverage.Covered)]
    public async Task NormalizesWhitespaceInLinearTime()
    {
        // Deep nesting selects the plain-text fallback. A long run of non-breaking spaces without a newline made the
        // previous whitespace pattern backtrack quadratically (about 10 seconds for this input).
        var webFetch = Tool((_, _) => HtmlResponse(Html(
            string.Concat(Enumerable.Repeat("<div>", 600)) + "start" + new string(' ', 160_000) + "end" + string.Concat(Enumerable.Repeat("</div>", 600)))));

        var stopwatch = Stopwatch.StartNew();
        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("document", output.GetProperty("type").GetString());
        Assert.Equal("start end", Content(output));
        Assert.True(stopwatch.ElapsedMilliseconds < 2000);
    }

    [Fact]
    [UpstreamTest(Prefix + "falls back to plain text for deeply nested HTML", Coverage = UpstreamCoverage.Covered)]
    public async Task FallsBackToPlainTextForDeeplyNestedHtml()
    {
        const int depth = 1000;
        var webFetch = Tool((_, _) => HtmlResponse(Html(string.Concat(Enumerable.Repeat("<div>", depth)) + "Deep text" + string.Concat(Enumerable.Repeat("</div>", depth)))));

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("document", output.GetProperty("type").GetString());
        Assert.Equal("Deep text", Content(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "returns text formats as is", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsTextFormatsAsIs()
    {
        var webFetch = Tool((_, _) => DownloadResponse.Text("{\"name\":\"ai\"}", "application/json"));

        var output = await ExecuteAsync(webFetch, "https://example.com/data.json");

        Assert.Equal("document", output.GetProperty("type").GetString());
        Assert.Equal("application/json", output.GetProperty("contentType").GetString());
        Assert.Equal("{\"name\":\"ai\"}", Content(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "decodes the charset from the Content-Type header", Coverage = UpstreamCoverage.Covered)]
    public async Task DecodesTheCharsetFromTheContentTypeHeader()
    {
        var webFetch = Tool((_, _) => DownloadResponse.Bytes(new byte[] { 0x63, 0x61, 0x66, 0xe9 }, "text/plain; charset=iso-8859-1"));

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("café", Content(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "decodes the charset from a meta tag in HTML", Coverage = UpstreamCoverage.Covered)]
    public async Task DecodesTheCharsetFromAMetaTagInHtml()
    {
        var page = Html(
            "<main><p>Un café crème, s’il vous plaît. Merci beaucoup pour votre accueil chaleureux.</p></main>",
            "<meta charset=\"windows-1252\"><title>Café</title>");
        var body = page.Select(character => character == '’' ? (byte)0x92 : (byte)character).ToArray();
        var webFetch = Tool((_, _) => DownloadResponse.Bytes(body, "text/html"));

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("Café", output.GetProperty("title").GetString());
        Assert.Contains("Un café crème, s’il vous plaît.", Content(output));
    }

    [Fact]
    [UpstreamTest(Prefix + "sniffs HTML when the content type is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task SniffsHtmlWhenTheContentTypeIsMissing()
    {
        var webFetch = Tool((_, _) => DownloadResponse.Bytes(Encoding.UTF8.GetBytes(ArticleHtml), null));

        var output = await ExecuteAsync(webFetch, "https://example.com/page");

        Assert.Equal("document", output.GetProperty("type").GetString());
        Assert.Equal("text/html", output.GetProperty("contentType").GetString());
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("application/pdf")]
    [InlineData("application/octet-stream")]
    [UpstreamTest(Prefix + "rejects unsupported content type %s", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsUnsupportedContentType(string contentType)
    {
        var webFetch = Tool((_, _) => DownloadResponse.Bytes(Encoding.UTF8.GetBytes("%PDF-1.4"), contentType));

        var output = await ExecuteAsync(webFetch, "https://example.com/file");

        Assert.Equal("unsupported_content_type", ErrorCode(output));
    }
}
