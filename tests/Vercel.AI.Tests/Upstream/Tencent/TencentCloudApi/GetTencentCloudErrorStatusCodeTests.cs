// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tencent;
using Vercel.AI.Tests.MoreProviders;
using TencentCloudApiType = Vercel.AI.Tencent.TencentCloudApi;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentCloudApi;

/// <summary>Port of <c>tencent-cloud-api.test.ts</c> &gt; <c>getTencentCloudErrorStatusCode</c>.</summary>
public sealed class GetTencentCloudErrorStatusCodeTests
{
    private const string Prefix = "packages/tencent/src/tencent-cloud-api.test.ts::getTencentCloudErrorStatusCode::";
    private const string Commit = "1c7a311d5384a6741daade550e4d3a7f9253d71e";

    [Theory]
    [InlineData("AuthFailure.SignatureFailure", 401)]
    [InlineData("AuthFailure.SecretIdNotFound", 401)]
    [InlineData("UnauthorizedOperation", 403)]
    [InlineData("FailedOperation.InvalidVodUser", 403)]
    [InlineData("ResourceNotFound", 404)]
    [InlineData("InvalidParameterValue.SubAppId", 400)]
    [InlineData("MissingParameter", 400)]
    [InlineData("RequestLimitExceeded", 429)]
    [InlineData("LimitExceeded", 429)]
    [InlineData("ResourceUnavailable", 503)]
    [InlineData("InternalError", 500)]
    [InlineData("FailedOperation", 500)]
    [InlineData("SomethingNew", 500)]
    [UpstreamTest(Prefix + "maps %s to %i", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Maps_code_to_status(string code, int status)
    {
        Assert.Equal(status, TencentCloudApiType.GetTencentCloudErrorStatusCode(code));
    }
}
