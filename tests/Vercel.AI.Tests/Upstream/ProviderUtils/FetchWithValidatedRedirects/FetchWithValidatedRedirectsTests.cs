// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.FetchWithValidatedRedirects;

/// <summary>Port of <c>fetch-with-validated-redirects.test.ts</c> &gt; <c>fetchWithValidatedRedirects</c>.</summary>
[Collection("DownloadFetch")]
public sealed class FetchWithValidatedRedirectsTests
{
    private const string Prefix = "packages/provider-utils/src/fetch-with-validated-redirects.test.ts::fetchWithValidatedRedirects::";

    [Theory]
    [InlineData(null)]
    [InlineData("https://example.com")]
    [InlineData("https://provider.example.com")]
    [UpstreamTest(Prefix + "preserves legacy first-hop credentials and custom headers with trustedOrigin %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_legacy_first_hop_credentials_and_custom_headers_with_trustedOrigin(string? trustedOrigin)
    {
        var fetch = new FetchMock(OkResponse());

        await Download.FetchWithValidatedRedirectsAsync(
            "https://example.com/file",
            Headers(("authorization", "Bearer secret"), ("x-jfrog-art-api", "vendor-secret"), ("x-custom-metadata", "custom-value")),
            fetch: fetch.Fetch,
            trustedOrigin: trustedOrigin);

        Assert.Equal(
            new[]
            {
                new KeyValuePair<string, string>("authorization", "Bearer secret"),
                new KeyValuePair<string, string>("x-jfrog-art-api", "vendor-secret"),
                new KeyValuePair<string, string>("x-custom-metadata", "custom-value"),
            },
            fetch.Calls[0].Request.Headers.Entries());
    }

    [Fact]
    [UpstreamTest(Prefix + "validates the initial URL before requesting it", Coverage = UpstreamCoverage.Covered)]
    public async Task Validates_the_initial_URL_before_requesting_it()
    {
        var fetch = new FetchMock();

        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.FetchWithValidatedRedirectsAsync("http://localhost/file", fetch: fetch.Fetch);
        });
        Assert.Empty(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "uses redirect: manual and omits headers when none are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_redirect_manual_and_omits_headers_when_none_are_provided()
    {
        var fetch = new FetchMock(OkResponse());

        await Download.FetchWithValidatedRedirectsAsync("https://example.com/file", fetch: fetch.Fetch);

        var call = Assert.Single(fetch.Calls);
        Assert.Equal("https://example.com/file", call.Url);
        Assert.Equal("manual", call.Request.Redirect);
        Assert.Empty(call.Request.Headers.Entries());
    }

    [Fact]
    [UpstreamTest(Prefix + "follows a redirect to a safe URL, validating the hop", Coverage = UpstreamCoverage.Covered)]
    public async Task Follows_a_redirect_to_a_safe_URL_validating_the_hop()
    {
        var fetch = new FetchMock(RedirectResponse("https://cdn.example.com/file"), OkResponse());

        var response = await Download.FetchWithValidatedRedirectsAsync("https://example.com/file", fetch: fetch.Fetch);

        Assert.True(response.Ok);
        Assert.Equal(2, fetch.Calls.Count);
        Assert.Equal("https://cdn.example.com/file", fetch.Calls[1].Url);
        Assert.Equal("manual", fetch.Calls[1].Request.Redirect);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects a redirect to a private address without requesting it", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_redirect_to_a_private_address_without_requesting_it()
    {
        var fetch = new FetchMock(RedirectResponse("http://169.254.169.254/"));

        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.FetchWithValidatedRedirectsAsync("https://evil.com/redirect", fetch: fetch.Fetch);
        });
        Assert.Single(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "starts cancelling redirect response bodies to prevent socket leaks", Coverage = UpstreamCoverage.Covered)]
    public async Task Starts_cancelling_redirect_response_bodies_to_prevent_socket_leaks()
    {
        var cancelled = 0;
        var fetch = new FetchMock(
            RedirectResponse("https://cdn.example.com/file", onCancel: delegate { cancelled++; }),
            RedirectResponse("http://169.254.169.254/", onCancel: delegate { cancelled++; }));

        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.FetchWithValidatedRedirectsAsync("https://example.com/file", fetch: fetch.Fetch);
        });

        // Both the followed hop and the hop rejected by the SSRF guard must have their bodies cancelled.
        Assert.Equal(2, cancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "resolves relative redirect targets against the current URL", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_relative_redirect_targets_against_the_current_URL()
    {
        var fetch = new FetchMock(RedirectResponse("/internal"), OkResponse());

        await Download.FetchWithValidatedRedirectsAsync("https://example.com/start", fetch: fetch.Fetch);

        Assert.Equal("https://example.com/internal", fetch.Calls[1].Url);
        Assert.Equal("manual", fetch.Calls[1].Request.Redirect);
    }

    [Theory]
    [InlineData(301)]
    [InlineData(302)]
    [InlineData(303)]
    [InlineData(307)]
    [InlineData(308)]
    [UpstreamTest(Prefix + "follows a %d redirect", Coverage = UpstreamCoverage.Covered)]
    public async Task Follows_a_redirect(int status)
    {
        var fetch = new FetchMock(RedirectResponse("https://cdn.example.com/file", status), OkResponse());

        await Download.FetchWithValidatedRedirectsAsync("https://example.com/file", fetch: fetch.Fetch);

        Assert.Equal(2, fetch.Calls.Count);
    }

    [Theory]
    [InlineData(300)]
    [InlineData(304)]
    [UpstreamTest(Prefix + "returns a %d response as final even when it carries a Location header", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_a_response_as_final_even_when_it_carries_a_Location_header(int status)
    {
        // Per the fetch spec, only 301/302/303/307/308 are redirect statuses.
        // A hostile Location on a non-redirect status must not be followed, not even to a private address.
        var fetch = new FetchMock(RedirectResponse("http://169.254.169.254/", status));

        var response = await Download.FetchWithValidatedRedirectsAsync("https://example.com/file", fetch: fetch.Fetch);

        Assert.Equal(status, response.Status);
        Assert.Single(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects once the redirect limit is exceeded", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_once_the_redirect_limit_is_exceeded()
    {
        var fetch = new FetchMock(() => RedirectResponse("https://example.com/next"));

        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.FetchWithValidatedRedirectsAsync("https://example.com/start", maxRedirects: 2, fetch: fetch.Fetch);
        });
    }

    [Fact]
    [UpstreamTest(Prefix + "fails closed on an opaque redirect outside the browser", Coverage = UpstreamCoverage.Covered)]
    public async Task Fails_closed_on_an_opaque_redirect_outside_the_browser()
    {
        var fetch = new FetchMock(OpaqueRedirectResponse());

        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.FetchWithValidatedRedirectsAsync("https://example.com/redirect", fetch: fetch.Fetch);
        });
        Assert.Single(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "follows redirects natively on an opaque redirect in the browser", Coverage = UpstreamCoverage.Covered)]
    public async Task Follows_redirects_natively_on_an_opaque_redirect_in_the_browser()
    {
        var previous = Download.IsBrowser;
        Download.IsBrowser = true;
        try
        {
            var fetch = new FetchMock(OpaqueRedirectResponse(), OkResponse());

            var response = await Download.FetchWithValidatedRedirectsAsync("https://example.com/file", fetch: fetch.Fetch);

            Assert.True(response.Ok);
            Assert.Equal("https://example.com/file", fetch.Calls[1].Url);
            Assert.Equal("follow", fetch.Calls[1].Request.Redirect);
        }
        finally
        {
            Download.IsBrowser = previous;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "passes each hop an independent header snapshot (later hops must not mutate earlier ones)", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_each_hop_an_independent_header_snapshot()
    {
        var fetch = new FetchMock(RedirectResponse("https://other.example.net/a"), OkResponse());

        await Download.FetchWithValidatedRedirectsAsync(
            "https://example.com/file",
            Headers(("authorization", "Bearer secret")),
            fetch: fetch.Fetch);

        var firstHop = fetch.Calls[0].Request.Headers;
        var secondHop = fetch.Calls[1].Request.Headers;
        Assert.NotSame(firstHop, secondHop);
        // The cross-origin credential drop on the second hop must not be visible in the header set the first hop was issued with.
        Assert.Equal("Bearer secret", firstHop.Get("authorization"));
        Assert.Null(secondHop.Get("authorization"));
    }

    [Theory]
    [InlineData("http://localhost:5000", "http://localhost:5000/predictions/123", "http://localhost:5000/final")]
    [InlineData("https://service.internal", "https://service.internal/predictions/123", "https://service.internal/final")]
    [UpstreamTest(Prefix + "skips validation for hops same-origin with trustedOrigin", Coverage = UpstreamCoverage.Covered)]
    public async Task Skips_validation_for_hops_same_origin_with_trustedOrigin(string trustedOrigin, string url, string redirectTarget)
    {
        // A self-hosted deployment: the configured origin is private, and its response URLs
        // (and same-origin redirects) point back at it.
        var fetch = new FetchMock(RedirectResponse(redirectTarget), OkResponse());

        var response = await Download.FetchWithValidatedRedirectsAsync(url, fetch: fetch.Fetch, trustedOrigin: trustedOrigin);

        Assert.True(response.Ok);
        Assert.Equal(2, fetch.Calls.Count);
    }

    [Theory]
    [InlineData("http://169.254.169.254/")]
    [InlineData("https://other.internal/file")]
    [UpstreamTest(Prefix + "validates hops on other origins even when trustedOrigin is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Validates_hops_on_other_origins_even_when_trustedOrigin_is_set(string redirectTarget)
    {
        var fetch = new FetchMock(RedirectResponse(redirectTarget));

        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.FetchWithValidatedRedirectsAsync(
                "http://localhost:5000/predictions/123",
                fetch: fetch.Fetch,
                trustedOrigin: "http://localhost:5000");
        });
        Assert.Single(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "uses the injected fetch instead of the global one", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_injected_fetch_instead_of_the_global_one()
    {
        var previous = Download.Fetch;
        var globalCalls = 0;
        Download.Fetch = delegate
        {
            globalCalls++;
            return Task.FromResult(OkResponse());
        };
        try
        {
            var injected = new FetchMock(OkResponse());

            await Download.FetchWithValidatedRedirectsAsync("https://example.com/file", fetch: injected.Fetch);

            Assert.Single(injected.Calls);
            Assert.Equal(0, globalCalls);
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "drops all caller headers except user-agent on a cross-origin redirect but keeps them same-origin", Coverage = UpstreamCoverage.Covered)]
    public async Task Drops_all_caller_headers_except_user_agent_on_a_cross_origin_redirect_but_keeps_them_same_origin()
    {
        // Cross-origin: no caller header, including custom API-key headers the fetch spec would let through,
        // may follow to a different host.
        var crossOrigin = new FetchMock(RedirectResponse("https://other.example.net/a"), OkResponse());

        await Download.FetchWithValidatedRedirectsAsync(
            "https://example.com/file",
            Headers(("authorization", "Bearer secret"), ("x-key", "provider-api-key"), ("user-agent", "ai-sdk/test")),
            fetch: crossOrigin.Fetch);

        var secondHop = crossOrigin.Calls[1].Request.Headers;
        Assert.Null(secondHop.Get("authorization"));
        Assert.Null(secondHop.Get("x-key"));
        Assert.Equal("ai-sdk/test", secondHop.Get("user-agent"));

        // Same-origin: caller headers are preserved across the hop.
        var sameOrigin = new FetchMock(RedirectResponse("https://example.com/next"), OkResponse());

        await Download.FetchWithValidatedRedirectsAsync(
            "https://example.com/file",
            Headers(("authorization", "Bearer secret"), ("x-key", "provider-api-key")),
            fetch: sameOrigin.Fetch);

        var sameOriginHop = sameOrigin.Calls[1].Request.Headers;
        Assert.Equal("Bearer secret", sameOriginHop.Get("authorization"));
        Assert.Equal("provider-api-key", sameOriginHop.Get("x-key"));
    }

    [Fact]
    [UpstreamTest(Prefix + "does not re-attach dropped headers when a later hop returns to the original origin", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_re_attach_dropped_headers_when_a_later_hop_returns_to_the_original_origin()
    {
        var fetch = new FetchMock(
            RedirectResponse("https://other.example.net/a"),
            RedirectResponse("https://example.com/back"),
            OkResponse());

        await Download.FetchWithValidatedRedirectsAsync(
            "https://example.com/file",
            Headers(("authorization", "Bearer secret"), ("x-key", "provider-api-key")),
            fetch: fetch.Fetch);

        var thirdHop = fetch.Calls[2].Request.Headers;
        Assert.Null(thirdHop.Get("authorization"));
        Assert.Null(thirdHop.Get("x-key"));
    }

    private static HeaderCollection Headers(params (string Name, string Value)[] headers)
    {
        var collection = new HeaderCollection();
        foreach (var header in headers)
        {
            collection.Add(header.Name, header.Value);
        }

        return collection;
    }

    private static DownloadResponse OkResponse()
    {
        return DownloadResponse.Bytes(null, "image/png");
    }

    private static DownloadResponse RedirectResponse(string location, int status = 302, Action? onCancel = null)
    {
        var headers = new HeaderCollection();
        headers.Add("location", location);
        return new DownloadResponse(status, string.Empty, headers, null) { OnCancel = onCancel };
    }

    private static DownloadResponse OpaqueRedirectResponse()
    {
        return new DownloadResponse(0, string.Empty, new HeaderCollection(), null, "opaqueredirect");
    }

    private sealed record FetchCall(string Url, DownloadRequest Request);

    /// <summary>Replays queued responses, or builds one per call, and records every request.</summary>
    private sealed class FetchMock
    {
        private readonly Queue<DownloadResponse> _responses;
        private readonly Func<DownloadResponse>? _factory;

        public FetchMock(params DownloadResponse[] responses)
        {
            _responses = new Queue<DownloadResponse>(responses);
        }

        public FetchMock(Func<DownloadResponse> factory)
        {
            _responses = new Queue<DownloadResponse>();
            _factory = factory;
        }

        public List<FetchCall> Calls { get; } = new List<FetchCall>();

        public Task<DownloadResponse> Fetch(string url, DownloadRequest request)
        {
            Calls.Add(new FetchCall(url, request));
            return Task.FromResult(_factory != null ? _factory() : _responses.Dequeue());
        }
    }
}
