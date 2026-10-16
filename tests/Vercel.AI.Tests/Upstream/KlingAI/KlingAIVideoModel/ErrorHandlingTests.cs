// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel.KlingAIVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Port of <c>klingai-video-model.test.ts</c> &gt; <c>KlingAIVideoModel</c> &gt; <c>error handling</c>.</summary>
public sealed class ErrorHandlingTests
{
    private const string Prefix = "packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > error handling::";

    [Fact]
    [UpstreamTest(Prefix + "should throw when motion control provider options are missing required fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_when_motion_control_provider_options_are_missing_required_fields()
    {
        var server = new Server();
        // Missing videoUrl, characterOrientation, mode
        await Assert.ThrowsAnyAsync<Exception>(() => CreateModel(server.Handler).DoStartAsync(Start(providerOptions: "{\"klingai\":{}}"), CancellationToken.None)).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when klingai provider options are missing entirely for motion control", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_when_klingai_provider_options_are_missing_entirely_for_motion_control()
    {
        var server = new Server();
        var failure = await Assert.ThrowsAnyAsync<Exception>(() => CreateModel(server.Handler).DoStartAsync(Start(providerOptions: "{}"), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("providerOptions.klingai", failure.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when no task_id is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_when_no_task_id_is_returned()
    {
        var server = new Server();
        server.OnCreate("/v1/videos/motion-control", "{\"code\":0,\"message\":\"success\",\"request_id\":\"req-004\",\"data\":null}");
        var failure = await Assert.ThrowsAnyAsync<Exception>(() => CreateModel(server.Handler).DoStartAsync(Start(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("No task_id", failure.Message);
    }
}
