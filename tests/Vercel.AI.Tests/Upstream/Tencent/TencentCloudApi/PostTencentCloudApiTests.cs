// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tencent;
using Vercel.AI.Tests.MoreProviders;
using TencentCloudApiType = Vercel.AI.Tencent.TencentCloudApi;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentCloudApi;

/// <summary>Port of <c>tencent-cloud-api.test.ts</c> &gt; <c>postTencentCloudApi</c>.</summary>
public sealed class PostTencentCloudApiTests
{
    private const string Prefix = "packages/tencent/src/tencent-cloud-api.test.ts::postTencentCloudApi::";
    private const string Commit = "1c7a311d5384a6741daade550e4d3a7f9253d71e";

    private const string Url = "https://vod.intl.tencentcloudapi.com";
    private const string SignedAuthorization = "TC3-HMAC-SHA256 Credential=AKIDEXAMPLEEXAMPLEEXAMPLE/2026-11-08/vod/tc3_request, SignedHeaders=content-type;host;x-tc-action, Signature=20bd5603a92b091b44b53c95c55b2e5f7687cf779aff39688d83229b7252409f";

    private static readonly TencentCloudCredentials Credentials = new() { SecretId = "AKIDEXAMPLEEXAMPLEEXAMPLE", SecretKey = "EXAMPLESECRETKEYEXAMPLESECRETKEY" };

    private static readonly JsonObject Body = new() { ["SubAppId"] = 1500000000, ["ModelName"] = "VS", ["ModelVersion"] = "2.0", ["Prompt"] = "A cat" };

    private static ParityHandler Handler(string body) => new(_ => ParityHandler.Json(body));

    private static Task<TencentCloudResponse> Post(ParityHandler handler, string action, TencentCloudCredentials credentials, JsonObject body, string? region = null, Dictionary<string, string?>? headers = null)
    {
        return TencentCloudApiType.PostTencentCloudApiAsync(new HttpClient(handler, disposeHandler: false), Url, "vod", action, "2018-07-17", region, credentials, body, headers, 1794120000, CancellationToken.None);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends the signed payload and protocol headers", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_the_signed_payload_and_protocol_headers()
    {
        var handler = Handler("""{"Response":{"TaskId":"task-1","RequestId":"req-1"}}""");

        var result = await Post(handler, "CreateAigcVideoTask", Credentials, Body, headers: new Dictionary<string, string?> { ["Content-Type"] = "text/plain", ["x-custom"] = "value" });

        Assert.Equal("task-1", result.Value.GetProperty("TaskId").GetString());
        var call = handler.Calls[0];
        Assert.Equal(Url + "/", call.Uri.AbsoluteUri);
        Assert.Equal(Body.ToJsonString(), call.Text);
        Assert.Equal("application/json; charset=utf-8", call.Header("content-type"));
        Assert.Equal("CreateAigcVideoTask", call.Header("x-tc-action"));
        Assert.Equal("2018-07-17", call.Header("x-tc-version"));
        Assert.Equal("1794120000", call.Header("x-tc-timestamp"));
        Assert.Null(call.Header("x-tc-region"));
        Assert.Equal("value", call.Header("x-custom"));
        Assert.Equal(SignedAuthorization, call.Header("authorization"));
    }

    [Fact]
    [UpstreamTest(Prefix + "sends region and session token headers when provided", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_region_and_session_token_headers_when_provided()
    {
        var handler = Handler("""{"Response":{"TaskId":"task-1"}}""");

        await Post(handler, "DescribeTaskDetail", new TencentCloudCredentials { SecretId = Credentials.SecretId, SecretKey = Credentials.SecretKey, SessionToken = "session-token" }, new JsonObject { ["TaskId"] = "task-1" }, "ap-singapore");

        Assert.Equal("ap-singapore", handler.Calls[0].Header("x-tc-region"));
        Assert.Equal("session-token", handler.Calls[0].Header("x-tc-token"));
    }

    [Fact]
    [UpstreamTest(Prefix + "throws an APICallError for a Response.Error returned with HTTP 200", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Throws_an_APICallError_for_a_Response_Error_returned_with_HTTP_200()
    {
        var handler = Handler("""{"Response":{"Error":{"Code":"AuthFailure.SignatureFailure","Message":"The provided credentials could not be validated."},"RequestId":"req-1"}}""");

        var error = await Assert.ThrowsAsync<AuthenticationException>(() => Post(handler, "CreateAigcVideoTask", Credentials, Body));

        Assert.Equal(401, error.StatusCode);
        Assert.False(error.IsRetryable);
        Assert.Equal("AuthFailure.SignatureFailure: The provided credentials could not be validated.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "marks rate limit errors as retryable", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Marks_rate_limit_errors_as_retryable()
    {
        var handler = Handler("""{"Response":{"Error":{"Code":"RequestLimitExceeded","Message":"Too many requests."}}}""");

        var error = await Assert.ThrowsAsync<RateLimitException>(() => Post(handler, "CreateAigcVideoTask", Credentials, Body));

        Assert.Equal(429, error.StatusCode);
        Assert.True(error.IsRetryable);
    }
}
