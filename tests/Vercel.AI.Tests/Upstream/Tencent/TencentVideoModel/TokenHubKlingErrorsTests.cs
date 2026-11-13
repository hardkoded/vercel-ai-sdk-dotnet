// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; TokenHub Kling &gt; errors</c>.</summary>
public sealed class TokenHubKlingErrorsTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > TokenHub Kling > errors::";

    private const string TextToVideoUrl = TokenHubBaseUrl + "/wand/kling/text-to-video";

    [Fact]
    [UpstreamTest(Prefix + "throws when the response has a non-zero code", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Throws_when_the_response_has_a_non_zero_code()
    {
        var server = new Server();
        server.Respond(TextToVideoUrl, """{"code":1201,"message":"Invalid model","request_id":"r"}""");

        var error = await Assert.ThrowsAsync<ApiException>(() => CreateModel(server, "kling-video-v3").DoStartAsync(Start(), CancellationToken.None));

        Assert.Equal("Invalid model", error.Message);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest(Prefix + "surfaces HTTP error messages", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Surfaces_HTTP_error_messages()
    {
        var server = new Server();
        server.Respond(TextToVideoUrl, """{"error":{"message":"Invalid API key","type":"auth_error"}}""", 401);

        var error = await Assert.ThrowsAsync<AuthenticationException>(() => CreateModel(server, "kling-video-v3").DoStartAsync(Start(), CancellationToken.None));

        Assert.Equal(401, error.StatusCode);
        Assert.Equal("Invalid API key", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "throws when no task ID is returned", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Throws_when_no_task_ID_is_returned()
    {
        var server = new Server();
        server.Respond(TextToVideoUrl, """{"code":0,"data":{}}""");

        var error = await Assert.ThrowsAsync<AiSdkException>(() => CreateModel(server, "kling-video-v3").DoStartAsync(Start(), CancellationToken.None));

        Assert.Equal("No task ID returned from TokenHub.", error.Message);
    }
}
