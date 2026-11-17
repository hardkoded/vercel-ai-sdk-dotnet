// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.MiniMax;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; polling</c>.</summary>
public sealed class PollingTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > polling::";

    [Fact]
    [UpstreamTest(
        Prefix + "should keep polling through queued and running until succeeded",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_polling_through_queued_and_running_until_succeeded()
    {
        // The call number counts every request: 0 is the create call, 1+ are polls.
        ParityHandler handler = null!;
        handler = new ParityHandler(call =>
            call.Method == HttpMethod.Post
                ? ParityHandler.Json("{\"task_id\":\"task-123\"}")
                : ParityHandler.Json(
                    handler.Calls.IndexOf(call) switch
                    {
                        1 => PendingStatusResponse("queued"),
                        2 => PendingStatusResponse("running"),
                        _ => SucceededStatusResponse,
                    }
                )
        );

        var result = await CreateModel(handler)
            .DoGenerateAsync(new MiniMaxCall().ToCall(), CancellationToken.None);

        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("https://cdn.minimax.io/output/video-001.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        // 1 create call + 3 polls.
        Assert.Equal(4, handler.Calls.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep polling on an unknown status",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_polling_on_an_unknown_status()
    {
        ParityHandler handler = null!;
        handler = new ParityHandler(call =>
            call.Method == HttpMethod.Post
                ? ParityHandler.Json("{\"task_id\":\"task-123\"}")
                : ParityHandler.Json(
                    handler.Calls.IndexOf(call) == 1
                        ? "{\"task\":{\"id\":\"task-123\",\"status\":\"something-new\"}}"
                        : SucceededStatusResponse
                )
        );

        var result = await CreateModel(handler)
            .DoGenerateAsync(new MiniMaxCall().ToCall(), CancellationToken.None);

        Assert.Single(result.Videos);
        Assert.Equal(3, handler.Calls.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when polling exceeds pollTimeoutMs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_polling_exceeds_pollTimeoutMs()
    {
        ParityHandler handler = null!;
        handler = new ParityHandler(call =>
            call.Method == HttpMethod.Post
                ? ParityHandler.Json("{\"task_id\":\"task-123\"}")
                : ParityHandler.Json(PendingStatusResponse("running"))
        );

        var error = await Assert.ThrowsAsync<MiniMaxVideoGenerationException>(() =>
            CreateModel(handler)
                .DoGenerateAsync(
                    new MiniMaxCall
                    {
                        ProviderOptions = MiniMaxOptions(
                            "{\"pollIntervalMs\":10,\"pollTimeoutMs\":30}"
                        ),
                    }.ToCall(),
                    CancellationToken.None
                )
        );

        Assert.Equal("MINIMAX_VIDEO_GENERATION_TIMEOUT", error.Name);
        Assert.Equal(
            "MiniMax video generation timed out after 30ms. Task ID: " + TaskId,
            error.Message
        );
    }
}
