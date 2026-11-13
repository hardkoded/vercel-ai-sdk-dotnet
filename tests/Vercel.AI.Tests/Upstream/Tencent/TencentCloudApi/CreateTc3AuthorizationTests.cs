// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tencent;
using Vercel.AI.Tests.MoreProviders;
using TencentCloudApiType = Vercel.AI.Tencent.TencentCloudApi;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentCloudApi;

/// <summary>Port of <c>tencent-cloud-api.test.ts</c> &gt; <c>createTc3Authorization</c>.</summary>
public sealed class CreateTc3AuthorizationTests
{
    private const string Prefix = "packages/tencent/src/tencent-cloud-api.test.ts::createTc3Authorization::";
    private const string Commit = "1c7a311d5384a6741daade550e4d3a7f9253d71e";

    [Fact]
    [UpstreamTest(Prefix + "signs a VOD request", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Signs_a_VOD_request()
    {
        // Reference value computed independently from the documented algorithm.
        var authorization = TencentCloudApiType.CreateTc3Authorization(
            "AKIDEXAMPLEEXAMPLEEXAMPLE",
            "EXAMPLESECRETKEYEXAMPLESECRETKEY",
            "vod",
            1794120000,
            """{"SubAppId":1500000000,"ModelName":"VS","ModelVersion":"2.0","Prompt":"A cat"}""",
            new Dictionary<string, string>
            {
                ["content-type"] = "application/json; charset=utf-8",
                ["host"] = "vod.intl.tencentcloudapi.com",
                ["x-tc-action"] = "createaigcvideotask",
            });

        Assert.Equal(
            "TC3-HMAC-SHA256 Credential=AKIDEXAMPLEEXAMPLEEXAMPLE/2026-11-08/vod/tc3_request, SignedHeaders=content-type;host;x-tc-action, Signature=20bd5603a92b091b44b53c95c55b2e5f7687cf779aff39688d83229b7252409f",
            authorization);
    }
}
