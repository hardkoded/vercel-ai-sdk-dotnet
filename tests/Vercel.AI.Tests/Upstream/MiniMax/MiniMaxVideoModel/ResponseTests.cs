// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.MiniMax;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; response</c>.</summary>
public sealed class ResponseTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > response::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include the timestamp, modelId, and poll response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_timestamp_modelId_and_poll_response_headers()
    {
        var testDate = new DateTime(2024, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        var handler = new ParityHandler(call =>
            call.Method == HttpMethod.Post
                ? ParityHandler.Json("{\"task_id\":\"task-123\"}")
                : ParityHandler.Json(SucceededStatusResponse, ("x-minimax-request-id", "req-abc"))
        );
        var model = CreateModel(handler, currentDate: () => testDate);

        var result = await model.DoGenerateAsync(
            new MiniMaxCall().ToCall(),
            CancellationToken.None
        );

        Assert.Equal(testDate, result.Response.Timestamp);
        Assert.Equal("MiniMax-H3", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
        Assert.Equal("req-abc", result.Response.Headers!["x-minimax-request-id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the full provider metadata from the completed task",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_the_full_provider_metadata_from_the_completed_task()
    {
        var result = await CreateModel(Server())
            .DoGenerateAsync(new MiniMaxCall().ToCall(), CancellationToken.None);

        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"minimax\":{\"taskId\":\"task-123\",\"videoUrl\":\"https://cdn.minimax.io/output/video-001.mp4\",\"resolvedInputs\":{\"imageCount\":0,\"referenceVideoUrls\":[]},\"duration\":5,\"ratio\":\"16:9\",\"resolution\":\"2K\",\"usage\":{\"totalSeconds\":5,\"inputSeconds\":0,\"outputSeconds\":5}}}"
        );
    }
}
