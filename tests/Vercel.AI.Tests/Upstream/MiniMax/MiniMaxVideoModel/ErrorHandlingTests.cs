// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; error handling</c>.</summary>
public sealed class ErrorHandlingTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > error handling::";

    private static ParityHandler Poll(string body)
    {
        return Server(pollBody: body);
    }

    private static Task<Vercel.AI.Operations.VideoModelResult> Generate(ParityHandler handler)
    {
        return CreateModel(handler)
            .DoGenerateAsync(new MiniMaxCall().ToCall(), CancellationToken.None);
    }

    private static async Task AssertFails(string pollBody, string name, string message)
    {
        var error = await Assert.ThrowsAsync<MiniMaxVideoGenerationException>(() =>
            Generate(Poll(pollBody))
        );
        Assert.Equal(name, error.Name);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the task failed, including the error message and code",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_the_task_failed_including_the_error_message_and_code()
    {
        await AssertFails(
            "{\"task\":{\"id\":\"task-123\",\"status\":\"failed\",\"error\":{\"code\":1027,\"message\":\"Content policy violation\"}}}",
            "MINIMAX_VIDEO_GENERATION_FAILED",
            "MiniMax video generation failed: Content policy violation (1027). Task ID: " + TaskId
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw a bare failure message when the task carries no error object",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_a_bare_failure_message_when_the_task_carries_no_error_object()
    {
        await AssertFails(
            "{\"task\":{\"id\":\"task-123\",\"status\":\"failed\"}}",
            "MINIMAX_VIDEO_GENERATION_FAILED",
            "MiniMax video generation failed. Task ID: " + TaskId
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the task was cancelled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_the_task_was_cancelled()
    {
        await AssertFails(
            "{\"task\":{\"id\":\"task-123\",\"status\":\"cancelled\"}}",
            "MINIMAX_VIDEO_GENERATION_CANCELLED",
            "MiniMax video generation was cancelled. Task ID: " + TaskId
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the task expired",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_the_task_expired()
    {
        await AssertFails(
            "{\"task\":{\"id\":\"task-123\",\"status\":\"expired\"}}",
            "MINIMAX_VIDEO_GENERATION_EXPIRED",
            "MiniMax video generation request expired. Task ID: " + TaskId
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when a succeeded task has no video URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_a_succeeded_task_has_no_video_URL()
    {
        await AssertFails(
            "{\"task\":{\"id\":\"task-123\",\"status\":\"succeeded\",\"content\":{}}}",
            "MINIMAX_VIDEO_GENERATION_ERROR",
            "MiniMax video generation completed but no video URL was returned. Task ID: " + TaskId
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when a succeeded task has no content at all",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_a_succeeded_task_has_no_content_at_all()
    {
        var error = await Assert.ThrowsAsync<MiniMaxVideoGenerationException>(() =>
            Generate(Poll("{\"task\":{\"id\":\"task-123\",\"status\":\"succeeded\"}}"))
        );
        Assert.Contains("no video URL was returned", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the create call returns no task_id",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_the_create_call_returns_no_task_id()
    {
        var handler = Server(createBody: "{}");

        var error = await Assert.ThrowsAsync<MiniMaxVideoGenerationException>(() =>
            Generate(handler)
        );

        Assert.Equal("MINIMAX_VIDEO_GENERATION_ERROR", error.Name);
        Assert.Equal("No task_id returned from the MiniMax API. Response: {}", error.Message);
        // The poll loop is never entered.
        Assert.Single(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw when the create call returns an empty task_id",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_the_create_call_returns_an_empty_task_id()
    {
        var error = await Assert.ThrowsAsync<MiniMaxVideoGenerationException>(() =>
            Generate(Server(createBody: "{\"task_id\":\"\"}"))
        );

        Assert.Contains("No task_id returned from the MiniMax API", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should surface the MiniMax error message from a failed create call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_surface_the_MiniMax_error_message_from_a_failed_create_call()
    {
        var handler = new ParityHandler(_ =>
            ErrorResponse(400, ErrorEnvelope("invalid duration", 400))
        );

        var error = await Assert.ThrowsAsync<BadRequestException>(() => Generate(handler));

        Assert.Equal("invalid duration", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal(CreateUrl, error.Url);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should surface the MiniMax error message from a failed poll call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_surface_the_MiniMax_error_message_from_a_failed_poll_call()
    {
        var handler = new ParityHandler(call =>
            call.Method == HttpMethod.Post
                ? ParityHandler.Json("{\"task_id\":\"task-123\"}")
                : ErrorResponse(500, ErrorEnvelope("internal server error", 500))
        );

        var error = await Assert.ThrowsAsync<InternalServerException>(() => Generate(handler));

        Assert.Equal("internal server error", error.Message);
        Assert.Equal(500, error.StatusCode);
        Assert.Equal(PollUrl, error.Url);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to a generic message when the error envelope has no message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fall_back_to_a_generic_message_when_the_error_envelope_has_no_message()
    {
        var handler = new ParityHandler(_ =>
            ErrorResponse(401, "{\"type\":\"error\",\"request_id\":\"req-err-002\"}")
        );

        var error = await Assert.ThrowsAsync<AuthenticationException>(() => Generate(handler));

        Assert.Equal("MiniMax video generation error", error.Message);
        Assert.Equal(401, error.StatusCode);
    }
}
