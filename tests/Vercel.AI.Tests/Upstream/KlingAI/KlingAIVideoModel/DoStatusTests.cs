// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel.KlingAIVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Port of <c>klingai-video-model.test.ts</c> &gt; <c>KlingAIVideoModel</c> &gt; <c>doStatus</c>.</summary>
public sealed class DoStatusTests
{
    private const string Prefix = "packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > doStatus::";
    private const string T2VStatusPath = "/v1/videos/text2video/task-abc-123";

    [Fact]
    [UpstreamTest(Prefix + "should return completed with video data when succeed", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_completed_with_video_data_when_succeed()
    {
        var server = new Server();
        var result = await CreateModel(server.Handler, "kling-v2.6-t2v").DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal(("url", "https://p1.a.kwimgs.com/output/video-001.mp4", "video/mp4"), (video.Type, video.Url, video.MediaType));
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"klingai\":{\"taskId\":\"task-abc-123\",\"videos\":[{\"id\":\"video-001\",\"url\":\"https://p1.a.kwimgs.com/output/video-001.mp4\",\"watermarkUrl\":\"https://p1.a.kwimgs.com/output/video-001-watermark.mp4\",\"duration\":\"5.0\"}]}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should return pending when status is submitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_pending_when_status_is_submitted()
    {
        var server = new Server();
        server.OnStatus(T2VStatusPath, "{\"code\":0,\"message\":\"success\",\"request_id\":\"req-006\",\"data\":{\"task_id\":\"task-abc-123\",\"task_status\":\"submitted\",\"task_status_msg\":\"\",\"task_info\":{},\"created_at\":1722769557708,\"updated_at\":1722769557708}}");
        var result = await CreateModel(server.Handler, "kling-v2.6-t2v").DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return pending when status is processing", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_pending_when_status_is_processing()
    {
        var server = new Server();
        server.OnStatus(T2VStatusPath, "{\"code\":0,\"message\":\"success\",\"request_id\":\"req-007\",\"data\":{\"task_id\":\"task-abc-123\",\"task_status\":\"processing\",\"task_status_msg\":\"\",\"task_info\":{},\"created_at\":1722769557708,\"updated_at\":1722769558000}}");
        var result = await CreateModel(server.Handler, "kling-v2.6-t2v").DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return error status on failed task", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_error_status_on_failed_task()
    {
        var server = new Server();
        server.OnStatus(T2VStatusPath, "{\"code\":0,\"message\":\"success\",\"request_id\":\"req-008\",\"data\":{\"task_id\":\"task-abc-123\",\"task_status\":\"failed\",\"task_status_msg\":\"Content policy violation\",\"task_info\":{},\"created_at\":1722769557708,\"updated_at\":1722769560000}}");
        var result = await CreateModel(server.Handler, "kling-v2.6-t2v").DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("error", result.Status);
        Assert.Contains("Content policy violation", result.Error);
        Assert.NotNull(result.Response.Timestamp);
        Assert.Equal("kling-v2.6-t2v", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use correct endpointPath from operation", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_correct_endpointPath_from_operation()
    {
        var server = new Server();
        await CreateModel(server.Handler, "kling-v2.6-i2v").DoStatusAsync(Status("/v1/videos/image2video"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(TestBaseUrl + "/v1/videos/image2video/task-abc-123", server.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers to status request", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_to_status_request()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, "kling-v2.6-t2v", configure: options => options.ApiKey = "custom-token");
        await model.DoStatusAsync(Status(headers: new Dictionary<string, string> { ["X-Request-Header"] = "request-value" }), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Bearer custom-token", server.Calls[0].Header("authorization"));
        Assert.Equal("request-value", server.Calls[0].Header("x-request-header"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should validate redirects after trusting the configured origin", Coverage = UpstreamCoverage.Covered, Note = "Upstream asserts redirect: 'manual' on the request init. A .NET handler cannot see that flag. The port follows redirects by hand, so a single recorded call to the trusted URL and the DownloadError show the redirect was not followed.")]
    public async Task Should_validate_redirects_after_trusting_the_configured_origin()
    {
        var handler = new ParityHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Found);
            response.Headers.TryAddWithoutValidation("Location", "http://169.254.169.254/latest/meta-data/");
            return response;
        });
        var model = CreateModel(handler, "kling-v2.6-t2v", baseUrl: "http://localhost:3000");

        await Assert.ThrowsAsync<DownloadError>(() => model.DoStatusAsync(Status(), CancellationToken.None)).ConfigureAwait(false);

        var call = Assert.Single(handler.Calls);
        Assert.Equal("http://localhost:3000/v1/videos/text2video/task-abc-123", call.Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_response_metadata()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, "kling-v2.6-t2v", currentDate: () => TestDate);
        var result = await model.DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(TestDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("kling-v2.6-t2v", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }
}
