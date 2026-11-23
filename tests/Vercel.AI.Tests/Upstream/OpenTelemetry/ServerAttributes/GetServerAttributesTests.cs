// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using OtelServerAttributes = Vercel.AI.OpenTelemetry.ServerAttributes;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.ServerAttributes;

/// <summary>Port of <c>server-attributes.test.ts</c> &gt; <c>getServerAttributes</c>.</summary>
public sealed class GetServerAttributesTests
{
    private const string Test =
        "packages/otel/src/server-attributes.test.ts::getServerAttributes::";

    [Theory]
    [InlineData(
        "https://model.example.test:8443/private/v1?key=secret#fragment",
        "model.example.test",
        8443
    )]
    [InlineData("https://user:password@model.example.test/v1", "model.example.test", 443)]
    [InlineData("http://model.example.test/v1", "model.example.test", 80)]
    [InlineData("https://[2001:db8::1]:8443/v1", "2001:db8::1", 8443)]
    [UpstreamTest(
        Test + "records only host and port from $0",
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Records_only_host_and_port(string url, string address, int port)
    {
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["server.address"] = address,
                ["server.port"] = port,
            },
            OtelServerAttributes.GetServerAttributes(url)
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/relative/path")]
    [InlineData("not a URL")]
    [InlineData("file:///etc/passwd")]
    [InlineData("data:text/plain,secret")]
    [UpstreamTest(
        Test + "omits unknown or non-HTTP endpoints ($0)",
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Omits_unknown_or_non_HTTP_endpoints(string? url)
    {
        Assert.Empty(OtelServerAttributes.GetServerAttributes(url));
    }
}
