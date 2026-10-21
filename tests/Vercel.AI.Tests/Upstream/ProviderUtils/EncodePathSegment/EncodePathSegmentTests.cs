// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.EncodePathSegment;

/// <summary>Port of <c>encode-path-segment.test.ts</c> &gt; <c>encodePathSegment</c>.</summary>
public sealed class EncodePathSegmentTests
{
    private const string Prefix = "packages/provider-utils/src/encode-path-segment.test.ts::encodePathSegment::";

    [Theory]
    [InlineData("job-123", "job-123")]
    [InlineData("abc/../../internal", "abc%2F..%2F..%2Finternal")]
    [InlineData(".", "%252E")]
    [InlineData("..", "%252E%252E")]
    [InlineData("%2e%2e", "%252e%252e")]
    [InlineData("%2F", "%252F")]
    [InlineData("a?admin=true#fragment", "a%3Fadmin%3Dtrue%23fragment")]
    [InlineData("a\\..\\internal", "a%5C..%5Cinternal")]
    [InlineData("café", "caf%C3%A9")]
    [InlineData("a b", "a%20b")]
    [InlineData("", "")]
    [UpstreamTest(Prefix + "encodes %j without changing URL structure", Coverage = UpstreamCoverage.Covered)]
    public void Encodes_value_without_changing_URL_structure(string value, string expected)
    {
        var encoded = PathSegments.Encode(value);
        Assert.Equal(expected, encoded);

        var url = new Uri($"https://example.com/v1/jobs/{encoded}/result");
        Assert.Equal($"/v1/jobs/{expected}/result", url.AbsolutePath);
        Assert.Equal(string.Empty, url.Query);
        Assert.Equal(string.Empty, url.Fragment);
    }

    [Fact]
    [UpstreamTest(Prefix + "can encode resource paths one segment at a time", Coverage = UpstreamCoverage.Covered)]
    public void Can_encode_resource_paths_one_segment_at_a_time()
    {
        var path = string.Join("/", "operations/../internal?x#y".Split('/').Select(PathSegments.Encode));

        Assert.Equal("/v1/operations/%252E%252E/internal%3Fx%23y", new Uri($"https://example.com/v1/{path}").AbsolutePath);
    }
}
