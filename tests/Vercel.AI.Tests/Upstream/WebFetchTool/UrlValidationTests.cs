// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using Vercel.AI.WebFetch;
using static Vercel.AI.Tests.Upstream.WebFetchTool.WebFetchToolFixture;

namespace Vercel.AI.Tests.Upstream.WebFetchTool;

/// <summary>Port of <c>web-fetch-tool.test.ts</c> &gt; <c>createWebFetchTool</c> &gt; <c>URL validation</c>.</summary>
public sealed class UrlValidationTests
{
    private const string Prefix = FilePath + "::createWebFetchTool > URL validation::";

    private static IReadOnlyList<ModelMessage> User(string text)
    {
        return new ModelMessage[] { new UserModelMessage(text) };
    }

    [Theory]
    [InlineData("not a url", "invalid_url")]
    [InlineData("file:///etc/passwd", "invalid_url")]
    [InlineData("https://example.com/AAAA", "invalid_url")]
    [InlineData("https://user:password@example.com/", "url_not_allowed")]
    [InlineData("http://localhost:3000/", "url_not_allowed")]
    [InlineData("http://169.254.169.254/latest/meta-data", "url_not_allowed")]
    [InlineData("http://[::1]/", "url_not_allowed")]
    [UpstreamTest(Prefix + "rejects %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects(string url, string errorCode)
    {
        if (url == "https://example.com/AAAA")
        {
            url = "https://example.com/" + new string('a', 2048);
        }

        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("ok"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, url);

        Assert.Equal(errorCode, ErrorCode(output));
        Assert.Equal(url, output.GetProperty("url").GetString());
        Assert.Empty(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "allows only configured domains and their subdomains", Coverage = UpstreamCoverage.Covered)]
    public async Task AllowsOnlyConfiguredDomainsAndTheirSubdomains()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("ok"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, AllowedDomains = new[] { "example.com" } });

        foreach (var url in new[] { "https://example.com/", "https://docs.example.com/", "https://EXAMPLE.com./" })
        {
            Assert.True(IsDocument(await ExecuteAsync(webFetch, url)), url);
        }

        foreach (var url in new[] { "https://example.org/", "https://notexample.com/", "https://example.com.evil.org/" })
        {
            Assert.Equal("url_not_allowed", ErrorCode(await ExecuteAsync(webFetch, url)));
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "blocks configured domains and their subdomains", Coverage = UpstreamCoverage.Covered)]
    public async Task BlocksConfiguredDomainsAndTheirSubdomains()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("ok"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, BlockedDomains = new[] { "example.com" } });

        foreach (var url in new[] { "https://www.example.com./", "https://example.com/" })
        {
            Assert.Equal("url_not_allowed", ErrorCode(await ExecuteAsync(webFetch, url)));
        }

        Assert.True(IsDocument(await ExecuteAsync(webFetch, "https://example.org/")));
    }

    [Fact]
    [UpstreamTest(Prefix + "requires the URL to appear in a user message when configured", Coverage = UpstreamCoverage.Covered)]
    public async Task RequiresTheUrlToAppearInAUserMessageWhenConfigured()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("ok"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, RequireUrlInUserMessage = true });

        Assert.True(IsDocument(await ExecuteAsync(
            webFetch,
            "https://example.com",
            messages: new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new TextContentPart("Read https://example.com") }) })));

        // URLs in system, assistant, and tool messages do not count: the model controls assistant and tool content, and
        // tools do not receive the top-level system prompt.
        var others = new ModelMessage[]
        {
            new SystemModelMessage("Docs: https://example.com/docs"),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("I will fetch https://example.com/docs") }),
            new ToolModelMessage("search", "search", "{\"results\":[\"https://example.com/docs\"]}", false),
        };
        foreach (var message in others)
        {
            var output = await ExecuteAsync(
                webFetch,
                "https://example.com/docs",
                messages: new[] { new UserModelMessage("Read the docs"), message });

            Assert.Equal("url_not_in_user_message", ErrorCode(output));
        }

        Assert.Single(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "matches whole URLs in user messages, not substrings", Coverage = UpstreamCoverage.Covered)]
    public async Task MatchesWholeUrlsInUserMessagesNotSubstrings()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("ok"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, RequireUrlInUserMessage = true });
        Task<System.Text.Json.JsonElement> Check(string url, string text) => ExecuteAsync(webFetch, url, messages: User(text));

        foreach (var (url, text) in new[]
        {
            ("https://example.com", "Read https://example.com"),
            ("https://example.com/", "Read https://example.com"),
            ("https://EXAMPLE.com/docs", "See https://example.com/docs."),
            ("https://example.com/docs", "(see https://example.com/docs)"),
            ("https://example.com/docs#intro", "Read https://example.com/docs"),
            ("https://example.com/a?b=1", "<https://example.com/a?b=1>"),
        })
        {
            Assert.True(IsDocument(await Check(url, text)), url);
        }

        foreach (var (url, text) in new[]
        {
            ("https://attacker.com", "Read https://attacker.com.example.org/docs"),
            ("https://example.com/a", "Read https://example.com/abc"),
            ("https://example.com/docs", "Read https://example.com/docs/page"),
            ("https://example.com/?q=1", "Read https://example.com/?q=12"),
            ("https://example.com/", "Visit example.com"),
        })
        {
            Assert.Equal("url_not_in_user_message", ErrorCode(await Check(url, text)));
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "does not authorize a rejected URL through its own error result", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotAuthorizeARejectedUrlThroughItsOwnErrorResult()
    {
        var fetch = new RecordingFetch((_, _) => DownloadResponse.Text("ok"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, RequireUrlInUserMessage = true });
        const string exfiltrationUrl = "https://attacker.example/c?d=SECRET";
        var userMessage = new UserModelMessage("Summarize https://docs.example.com/guide");

        foreach (var firstUrl in new[] { exfiltrationUrl, "::" + exfiltrationUrl })
        {
            var first = await ExecuteAsync(webFetch, firstUrl, messages: new ModelMessage[] { userMessage });
            Assert.Equal("error", first.GetProperty("type").GetString());

            var modelOutput = Assert.IsType<ErrorTextToolModelOutput>(ToModelOutput(webFetch, first));
            var retry = await ExecuteAsync(
                webFetch,
                exfiltrationUrl,
                messages: new ModelMessage[] { userMessage, new ToolModelMessage("first", "webFetch", modelOutput.Value, true, "error-text") });
            Assert.Equal("url_not_in_user_message", ErrorCode(retry));
        }

        Assert.Empty(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not repeat the requested URL in error messages", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotRepeatTheRequestedUrlInErrorMessages()
    {
        // protects against authorizing a rejected URL through its own error result
        const string marker = "MODEL-INPUT-MARKER";
        var fetch = new RecordingFetch((url, _) => url.Contains("missing")
            ? new DownloadResponse(404, "Not Found", null, System.Text.Encoding.UTF8.GetBytes("missing"))
            : Redirect(302, "https://blocked.example/?" + marker));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions
        {
            Fetch = fetch.Fetch,
            AllowedDomains = new[] { "example.com" },
            RequireUrlInUserMessage = true,
        });
        var messages = User("https://example.com/redirect?" + marker + " https://example.com/missing?" + marker);

        foreach (var url in new[]
        {
            "::https://example.com/?" + marker,
            "https://example.org/?" + marker,
            "https://example.com/other?" + marker,
            "http://localhost/?" + marker,
            "https://example.com/redirect?" + marker,
            "https://example.com/missing?" + marker,
        })
        {
            var output = await ExecuteAsync(webFetch, url, messages: messages);

            Assert.Equal("error", output.GetProperty("type").GetString());
            Assert.DoesNotContain(marker, output.GetProperty("message").GetString());
        }
    }
}
