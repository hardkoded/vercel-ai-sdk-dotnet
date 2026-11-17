// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.MiniMax;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; request headers</c>.</summary>
public sealed class RequestHeadersTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > request headers::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send the configured auth header on the create and poll calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_configured_auth_header_on_the_create_and_poll_calls()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(new MiniMaxCall().ToCall(), CancellationToken.None);

        Assert.Equal("Bearer test-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("application/json", handler.Calls[0].Header("content-type"));
        Assert.Equal("Bearer test-key", handler.Calls[1].Header("authorization"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge per-request headers into the create and poll calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_per_request_headers_into_the_create_and_poll_calls()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                new MiniMaxCall
                {
                    Headers = new Dictionary<string, string>
                    {
                        ["x-request-header"] = "request-value",
                    },
                }.ToCall(),
                CancellationToken.None
            );

        foreach (var call in handler.Calls.Take(2))
        {
            Assert.Equal("Bearer test-key", call.Header("authorization"));
            Assert.Equal("request-value", call.Header("x-request-header"));
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should let a per-request header override the configured header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_let_a_per_request_header_override_the_configured_header()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                new MiniMaxCall
                {
                    Headers = new Dictionary<string, string>
                    {
                        ["Authorization"] = "Bearer override-key",
                    },
                }.ToCall(),
                CancellationToken.None
            );

        Assert.Equal("Bearer override-key", handler.Calls[0].Header("authorization"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should poll with GET on the task status URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_poll_with_GET_on_the_task_status_URL()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(new MiniMaxCall().ToCall(), CancellationToken.None);

        Assert.Equal(HttpMethod.Get, handler.Calls[1].Method);
        Assert.Equal(PollUrl, handler.Calls[1].Uri.AbsoluteUri);
    }
}
