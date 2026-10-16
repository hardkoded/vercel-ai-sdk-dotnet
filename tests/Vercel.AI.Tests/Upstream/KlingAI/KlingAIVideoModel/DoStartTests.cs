// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel.KlingAIVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Port of <c>klingai-video-model.test.ts</c> &gt; <c>KlingAIVideoModel</c> &gt; <c>doStart</c>.</summary>
public sealed class DoStartTests
{
    private const string Prefix = "packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > doStart::";

    [Fact]
    [UpstreamTest(Prefix + "should return operation with taskId and endpointPath for t2v", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_operation_with_taskId_and_endpointPath_for_t2v()
    {
        var server = new Server();
        var result = await CreateModel(server.Handler, "kling-v2.6-t2v").DoStartAsync(StartT2V(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal((System.Text.Json.JsonElement)result.Operation!, "{\"taskId\":\"task-abc-123\",\"endpointPath\":\"/v1/videos/text2video\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should return operation with endpointPath for i2v", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_operation_with_endpointPath_for_i2v()
    {
        var server = new Server();
        var result = await CreateModel(server.Handler, "kling-v2.6-i2v").DoStartAsync(StartI2V(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal((System.Text.Json.JsonElement)result.Operation!, "{\"taskId\":\"task-abc-123\",\"endpointPath\":\"/v1/videos/image2video\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should return operation with endpointPath for motion-control", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_operation_with_endpointPath_for_motion_control()
    {
        var server = new Server();
        var result = await CreateModel(server.Handler).DoStartAsync(Start(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal((System.Text.Json.JsonElement)result.Operation!, "{\"taskId\":\"task-abc-123\",\"endpointPath\":\"/v1/videos/motion-control\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_correct_request_body()
    {
        var server = new Server();
        var call = Start(prompt: "A sunset over the ocean", aspectRatio: "16:9", duration: 5, providerOptions: StdOptions);
        await CreateModel(server.Handler, "kling-v2.6-t2v").DoStartAsync(call, CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(server.RequestBody(), "{\"model_name\":\"kling-v2-6\",\"prompt\":\"A sunset over the ocean\",\"mode\":\"std\",\"aspect_ratio\":\"16:9\",\"duration\":\"5\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var server = new Server();
        var model = CreateModel(
            server.Handler,
            "kling-v2.6-t2v",
            configure: options =>
            {
                options.ApiKey = "custom-token";
                options.Headers["X-Custom"] = "value";
            });
        await model.DoStartAsync(StartT2V(headers: new Dictionary<string, string> { ["X-Request-Header"] = "request-value" }), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Bearer custom-token", server.Calls[0].Header("authorization"));
        Assert.Equal("value", server.Calls[0].Header("x-custom"));
        Assert.Equal("request-value", server.Calls[0].Header("x-request-header"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when no task_id returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_when_no_task_id_returned()
    {
        var server = new Server();
        server.OnCreate("/v1/videos/text2video", "{\"code\":0,\"message\":\"success\",\"request_id\":\"req-004\",\"data\":null}");
        var failure = await Assert.ThrowsAnyAsync<Exception>(() => CreateModel(server.Handler, "kling-v2.6-t2v").DoStartAsync(StartT2V(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("No task_id", failure.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_response_metadata()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, "kling-v2.6-t2v", currentDate: () => TestDate);
        var result = await model.DoStartAsync(StartT2V(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(TestDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("kling-v2.6-t2v", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include warnings for unsupported options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_warnings_for_unsupported_options()
    {
        var server = new Server();
        var call = Start(resolution: "1920x1080", seed: 42, fps: 30, n: 3, providerOptions: StdOptions);
        var result = await CreateModel(server.Handler, "kling-v2.6-t2v").DoStartAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "resolution");
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "seed");
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "fps");
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "n");
    }
}
