// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.SafeNodeFetch;

/// <summary>Port of <c>safe-node-fetch.test.ts</c> &gt; <c>createSafeLookup</c>.</summary>
public sealed class CreateSafeLookupTests
{
    private const string Prefix = "packages/provider-utils/src/safe-node-fetch.test.ts::createSafeLookup::";

    private static Task<IPAddress[]> RunLookup(string[] addresses, out List<string> lookedUp)
    {
        var calls = new List<string>();
        lookedUp = calls;
        var resolved = Array.ConvertAll(addresses, IPAddress.Parse);
        return SafeLookup.ResolveValidatedAsync(
            "files.example.com",
            (hostname, _) =>
            {
                calls.Add(hostname);
                return Task.FromResult(resolved);
            },
            CancellationToken.None);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns the validated addresses to the connector", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_validated_addresses_to_the_connector()
    {
        var addresses = new[] { "8.8.8.8", "2606:4700:4700::1111" };

        var result = await RunLookup(addresses, out var lookedUp);

        Assert.Equal(addresses, Array.ConvertAll(result, address => address.ToString()));
        Assert.Equal(new[] { "files.example.com" }, lookedUp);
    }

    [Fact]
    [UpstreamTest(Prefix + "blocks a hostname that resolves to a private address", Coverage = UpstreamCoverage.Covered)]
    public async Task Blocks_a_hostname_that_resolves_to_a_private_address()
    {
        await Assert.ThrowsAsync<DownloadError>(() => RunLookup(new[] { "127.0.0.1" }, out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "blocks mixed DNS results when any address is private", Coverage = UpstreamCoverage.Covered)]
    public async Task Blocks_mixed_DNS_results_when_any_address_is_private()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(() => RunLookup(new[] { "8.8.8.8", "169.254.169.254" }, out _));

        Assert.Contains("resolved to disallowed IP address 169.254.169.254", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "blocks private IPv6 DNS results", Coverage = UpstreamCoverage.Covered)]
    public async Task Blocks_private_IPv6_DNS_results()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(() => RunLookup(new[] { "::1" }, out _));

        Assert.Contains("resolved to disallowed IP address ::1", error.Message);
    }
}
