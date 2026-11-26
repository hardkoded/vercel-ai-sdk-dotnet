// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.FetchUntrustedUrl;

/// <summary>Port of <c>fetch-untrusted-url.test.ts</c> &gt; <c>fetchUntrustedUrl</c>.</summary>
[Collection("DownloadFetch")]
public sealed class FetchUntrustedUrlTests
{
    private const string Prefix =
        "packages/provider-utils/src/fetch-untrusted-url.test.ts::fetchUntrustedUrl::";

    [Theory]
    [InlineData(null)]
    [InlineData("https://provider.example.com")]
    [InlineData("http://example.com")]
    [InlineData("https://example.com:8443")]
    [UpstreamTest(
        Prefix
            + "withholds credentials and unknown headers without a matching credentialedOrigin (%s)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Withholds_credentials_and_unknown_headers_without_a_matching_credentialedOrigin(
        string? credentialedOrigin
    )
    {
        var fetch = new FetchMock(OkResponse());

        await Download.FetchUntrustedUrlAsync(
            "https://example.com/file",
            Headers(
                ("authorization", "Bearer secret"),
                ("x-key", "provider-secret"),
                ("x-jfrog-art-api", "vendor-secret"),
                ("x-access-token", "access-token"),
                ("x-client-secret", "client-secret"),
                ("x-goog-api-key", "google-api-key"),
                ("custom-vendor-header", "another-secret"),
                ("accept", "image/*"),
                ("range", "bytes=0-1023"),
                ("idempotency-key", "operation-id"),
                ("x-request-id", "request-id"),
                ("user-agent", "ai-sdk/test")
            ),
            credentialedOrigin,
            fetch: fetch.Fetch
        );

        Assert.Equal(
            Map(
                ("accept", "image/*"),
                ("range", "bytes=0-1023"),
                ("idempotency-key", "operation-id"),
                ("x-request-id", "request-id"),
                ("user-agent", "ai-sdk/test")
            ),
            Sent(fetch.Calls[0])
        );
    }

    [Theory]
    [InlineData("credentialedOrigin")]
    [InlineData("trustedOrigin")]
    [UpstreamTest(
        Prefix + "preserves sanitized headers for a matching origin: %j",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_sanitized_headers_for_a_matching_origin(string option)
    {
        var fetch = new FetchMock(OkResponse());
        var headers = Headers(
            ("authorization", "Bearer secret"),
            ("x-jfrog-art-api", "vendor-secret"),
            ("x-custom-metadata", "custom-value"),
            ("cookie", "session=abc"),
            ("metadata-flavor", "Google"),
            ("x-forwarded-for", "10.0.0.1")
        );

        await Download.FetchUntrustedUrlAsync(
            "https://example.com/file",
            headers,
            credentialedOrigin: option == "credentialedOrigin" ? "https://example.com/api" : null,
            fetch: fetch.Fetch,
            trustedOrigin: option == "trustedOrigin" ? "https://example.com" : null
        );

        Assert.Equal(
            Map(
                ("authorization", "Bearer secret"),
                ("x-jfrog-art-api", "vendor-secret"),
                ("x-custom-metadata", "custom-value")
            ),
            Sent(fetch.Calls[0])
        );
        Assert.Equal("session=abc", headers.Get("cookie"));
        Assert.Equal("Google", headers.Get("metadata-flavor"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "withholds credentials when only a non-matching trustedOrigin is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Withholds_credentials_when_only_a_non_matching_trustedOrigin_is_provided()
    {
        var fetch = new FetchMock(OkResponse());

        await Download.FetchUntrustedUrlAsync(
            "https://cdn.example.com/file",
            Headers(("authorization", "Bearer secret"), ("x-key", "provider-secret")),
            fetch: fetch.Fetch,
            trustedOrigin: "https://api.example.com"
        );

        Assert.Empty(Sent(fetch.Calls[0]));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses credentialedOrigin to narrow a matching trustedOrigin",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_credentialedOrigin_to_narrow_a_matching_trustedOrigin()
    {
        var fetch = new FetchMock(OkResponse());

        await Download.FetchUntrustedUrlAsync(
            "http://localhost:5000/predictions/123",
            Headers(("authorization", "Bearer secret"), ("accept", "application/json")),
            credentialedOrigin: "https://api.example.com",
            fetch: fetch.Fetch,
            trustedOrigin: "http://localhost:5000"
        );

        Assert.Equal(Map(("accept", "application/json")), Sent(fetch.Calls[0]));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves caller-approved metadata but cannot reintroduce sanitized headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_caller_approved_metadata_but_cannot_reintroduce_sanitized_headers()
    {
        var fetch = new FetchMock(OkResponse());

        await Download.FetchUntrustedUrlAsync(
            "https://example.com/file",
            Headers(
                ("Authorization", "Bearer secret"),
                ("Proxy-Authorization", "Basic secret"),
                ("X-Protocol-Version", "2026-09-24")
            ),
            untrustedFirstHopHeaders: ["X-Protocol-Version", "Proxy-Authorization"],
            fetch: fetch.Fetch
        );

        Assert.Equal(Map(("x-protocol-version", "2026-09-24")), Sent(fetch.Calls[0]));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "preserves headers on same-origin redirects, drops them across origins, and never reattaches them",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_headers_on_same_origin_redirects_drops_them_across_origins_and_never_reattaches_them()
    {
        var fetch = new FetchMock(
            RedirectResponse("https://example.com/next"),
            RedirectResponse("https://cdn.example.net/file"),
            RedirectResponse("https://example.com/back"),
            OkResponse()
        );
        var all = Map(
            ("authorization", "Bearer secret"),
            ("x-key", "provider-secret"),
            ("x-protocol-version", "2026-09-24"),
            ("user-agent", "ai-sdk/test")
        );

        await Download.FetchUntrustedUrlAsync(
            "https://example.com/file",
            Headers(all.Select(pair => (pair.Key, pair.Value)).ToArray()),
            credentialedOrigin: "https://example.com",
            untrustedFirstHopHeaders: ["x-protocol-version"],
            fetch: fetch.Fetch
        );

        var userAgent = Map(("user-agent", "ai-sdk/test"));
        Assert.Equal(new[] { all, all, userAgent, userAgent }, fetch.Calls.Select(Sent).ToArray());
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "never attaches withheld credentials when an untrusted first hop redirects to the credentialed origin",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Never_attaches_withheld_credentials_when_an_untrusted_first_hop_redirects_to_the_credentialed_origin()
    {
        var fetch = new FetchMock(RedirectResponse("https://api.example.com/file"), OkResponse());

        await Download.FetchUntrustedUrlAsync(
            "https://untrusted.example/file",
            Headers(("authorization", "Bearer secret"), ("x-key", "provider-secret")),
            credentialedOrigin: "https://api.example.com",
            fetch: fetch.Fetch
        );

        Assert.Equal(2, fetch.Calls.Count);
        foreach (var call in fetch.Calls)
        {
            Assert.Empty(Sent(call));
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects private URLs even when credentialedOrigin matches",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_private_URLs_even_when_credentialedOrigin_matches()
    {
        var fetch = new FetchMock();

        await Assert.ThrowsAsync<DownloadError>(() =>
            Download.FetchUntrustedUrlAsync(
                "http://127.0.0.1/file",
                Headers(("authorization", "Bearer secret")),
                credentialedOrigin: "http://127.0.0.1",
                fetch: fetch.Fetch
            )
        );
        Assert.Empty(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "allows a configured private origin but rejects a redirect to another private origin",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Allows_a_configured_private_origin_but_rejects_a_redirect_to_another_private_origin()
    {
        var fetch = new FetchMock(RedirectResponse("http://10.0.0.1/file"));

        await Assert.ThrowsAsync<DownloadError>(() =>
            Download.FetchUntrustedUrlAsync(
                "http://localhost:5000/file",
                Headers(("authorization", "Bearer secret")),
                fetch: fetch.Fetch,
                trustedOrigin: "http://localhost:5000"
            )
        );

        Assert.Single(fetch.Calls);
        Assert.Equal("Bearer secret", fetch.Calls[0].Request.Headers.Get("authorization"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "passes abort signals and redirect limits through without adding headers to a bare request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_abort_signals_and_redirect_limits_through_without_adding_headers_to_a_bare_request()
    {
        var fetch = new FetchMock(RedirectResponse("https://example.com/next"));
        using var source = new CancellationTokenSource();

        var error = await Assert.ThrowsAsync<DownloadError>(() =>
            Download.FetchUntrustedUrlAsync(
                "https://example.com/file",
                abortSignal: source.Token,
                maxRedirects: 0,
                fetch: fetch.Fetch
            )
        );

        Assert.Contains("Too many redirects (max 0)", error.Message);
        var call = Assert.Single(fetch.Calls);
        Assert.Equal("https://example.com/file", call.Url);
        Assert.Equal(source.Token, call.Request.AbortSignal);
        Assert.Equal("manual", call.Request.Redirect);
        Assert.Empty(call.Request.Headers.Entries());
    }

    private static HeaderCollection Headers(params (string Name, string Value)[] headers)
    {
        var collection = new HeaderCollection();
        foreach (var (name, value) in headers)
        {
            collection.Add(name, value);
        }

        return collection;
    }

    private static Dictionary<string, string> Map(params (string Name, string Value)[] headers)
    {
        return headers.ToDictionary(header => header.Name, header => header.Value);
    }

    // A JS Headers object lowercases names, so the comparison does too.
    private static Dictionary<string, string> Sent(FetchCall call)
    {
        return call
            .Request.Headers.Entries()
            .ToDictionary(header => header.Key.ToLowerInvariant(), header => header.Value);
    }

    private static DownloadResponse OkResponse()
    {
        return DownloadResponse.Bytes(null, "text/plain");
    }

    private static DownloadResponse RedirectResponse(string location)
    {
        var headers = new HeaderCollection();
        headers.Add("location", location);
        return new DownloadResponse(302, string.Empty, headers, null);
    }

    private sealed record FetchCall(string Url, DownloadRequest Request);

    private sealed class FetchMock(params DownloadResponse[] responses)
    {
        private readonly Queue<DownloadResponse> _responses = new(responses);

        public List<FetchCall> Calls { get; } = [];

        public Task<DownloadResponse> Fetch(string url, DownloadRequest request)
        {
            Calls.Add(new FetchCall(url, request));
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
