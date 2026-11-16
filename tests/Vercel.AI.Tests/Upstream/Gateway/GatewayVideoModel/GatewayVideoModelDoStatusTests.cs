// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel.GatewayVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel;

/// <summary>Port of <c>gateway-video-model.test.ts</c> &gt; <c>GatewayVideoModel &gt; doStatus</c>.</summary>
public sealed class GatewayVideoModelDoStatusTests
{
    private const string Prefix = "packages/gateway/src/gateway-video-model.test.ts::GatewayVideoModel > doStatus::";

    private static Vercel.AI.Operations.VideoModelCall StatusCall() => Call(operation: "{\"gatewayJobId\":\"job_123\"}");

    [Fact]
    [UpstreamTest(Prefix + "should return pending status", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_pending_status()
    {
        var handler = new UpstreamRecordingHandler("{\"status\":\"pending\",\"providerMetadata\":{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"running\"}}}}");

        var result = await CreateTestModel(handler).DoStatusAsync(StatusCall(), CancellationToken.None);

        Assert.Equal("pending", result.Status);
        Assert.True(Equal("{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"running\"}}}", Node(result.ProviderMetadata)));
        Assert.Equal(ModelId, result.Response.ModelId);
        Assert.Equal("https://api.test.com/video-model/status", handler.Uri);
        Assert.True(Equal("{\"operation\":{\"gatewayJobId\":\"job_123\"}}", Body(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts uploaded outcomes in completed status responses", Coverage = UpstreamCoverage.Covered, Commit = "2e4414481225ed70af7c39326a5f0ef5052a90ca")]
    public async Task Accepts_uploaded_outcomes_in_completed_status_responses()
    {
        var handler = new UpstreamRecordingHandler("{\"status\":\"completed\",\"videos\":[{\"type\":\"uploaded\",\"mediaType\":\"video/mp4\"}],\"warnings\":[]}");

        var result = await CreateTestModel(handler).DoStatusAsync(StatusCall(), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        Assert.Equal(
            new[] { ("uploaded", (string?)null, (string?)null, (string?)"video/mp4") },
            result.Videos.Select(Video).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "should return completed videos (provider-hosted URLs)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_completed_videos_provider_hosted_URLs()
    {
        var handler = new UpstreamRecordingHandler("{\"status\":\"completed\",\"videos\":[{\"type\":\"url\",\"url\":\"https://cdn.example.com/v.mp4\",\"mediaType\":\"video/mp4\"}],\"warnings\":[{\"type\":\"other\",\"message\":\"complete\"}],\"providerMetadata\":{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"result\":{\"expiresAt\":1234},\"status\":\"completed\"}}}}");

        var result = await CreateTestModel(handler).DoStatusAsync(StatusCall(), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        Assert.Equal(
            new[] { ("url", (string?)"https://cdn.example.com/v.mp4", (string?)null, (string?)"video/mp4") },
            result.Videos.Select(Video).ToArray());
        Assert.Equal(
            new[] { ("other", (string?)null, (string?)null, (string?)"complete") },
            result.Warnings.Select(Warning).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "should return error status", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_error_status()
    {
        var handler = new UpstreamRecordingHandler("{\"status\":\"error\",\"error\":\"Async video job failed\"}");

        var result = await CreateTestModel(handler).DoStatusAsync(StatusCall(), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Async video job failed", result.Error);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps the gateway cancelled status to a terminal error", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_the_gateway_cancelled_status_to_a_terminal_error()
    {
        var handler = new UpstreamRecordingHandler("{\"status\":\"cancelled\",\"providerMetadata\":{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"cancelled\"}}}}");

        var result = await CreateTestModel(handler).DoStatusAsync(StatusCall(), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation was cancelled.", result.Error);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map gateway errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_gateway_errors()
    {
        var handler = new UpstreamRecordingHandler("{\"error\":{\"message\":\"Unauthorized\",\"code\":\"unauthorized\"}}", HttpStatusCode.Unauthorized);

        await Assert.ThrowsAnyAsync<Exception>(() => CreateTestModel(handler).DoStatusAsync(StatusCall(), CancellationToken.None));
    }
}
