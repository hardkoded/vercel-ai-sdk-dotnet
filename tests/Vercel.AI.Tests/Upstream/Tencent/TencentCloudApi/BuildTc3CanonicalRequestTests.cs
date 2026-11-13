// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tencent;
using Vercel.AI.Tests.MoreProviders;
using TencentCloudApiType = Vercel.AI.Tencent.TencentCloudApi;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentCloudApi;

/// <summary>Port of <c>tencent-cloud-api.test.ts</c> &gt; <c>buildTc3CanonicalRequest</c>.</summary>
public sealed class BuildTc3CanonicalRequestTests
{
    private const string Prefix = "packages/tencent/src/tencent-cloud-api.test.ts::buildTc3CanonicalRequest::";
    private const string Commit = "1c7a311d5384a6741daade550e4d3a7f9253d71e";

    // Worked example from Tencent's "Signature v3" documentation (https://intl.cloud.tencent.com/document/product/377/30988).
    private const string Payload = """{"Limit": 1, "Filters": [{"Values": ["unnamed"], "Name": "instance-name"}]}""";

    [Fact]
    [UpstreamTest(Prefix + "hashes the payload as documented", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Hashes_the_payload_as_documented()
    {
        Assert.Equal("99d58dfbc6745f6747f36bfca17dee5e6881dc0428a0a36f96199342bc5b4907", TencentCloudApiType.Sha256Hex(Payload));
    }

    [Fact]
    [UpstreamTest(Prefix + "produces the documented canonical request hash", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Produces_the_documented_canonical_request_hash()
    {
        var (canonicalRequest, signedHeaders) = TencentCloudApiType.BuildTc3CanonicalRequest(
            Payload,
            new Dictionary<string, string> { ["Host"] = "cvm.tencentcloudapi.com", ["Content-Type"] = "application/json; charset=utf-8" });

        Assert.Equal("content-type;host", signedHeaders);
        Assert.Equal("2815843035062fffda5fd6f2a44ea8a34818b0dc46f024b8b3786976a3adda7a", TencentCloudApiType.Sha256Hex(canonicalRequest));
    }
}
