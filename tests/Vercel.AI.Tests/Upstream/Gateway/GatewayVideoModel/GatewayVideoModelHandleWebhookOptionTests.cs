// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel.GatewayVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel;

/// <summary>Port of <c>gateway-video-model.test.ts</c> &gt; <c>GatewayVideoModel &gt; handleWebhookOption</c>.</summary>
public sealed class GatewayVideoModelHandleWebhookOptionTests
{
    private const string Prefix = "packages/gateway/src/gateway-video-model.test.ts::GatewayVideoModel > handleWebhookOption::";

    [Fact]
    [UpstreamTest(Prefix + "should pass the factory URL and received promise straight through", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_factory_URL_and_received_promise_straight_through()
    {
        var received = Task.CompletedTask;
        var calls = 0;
        Task<VideoWebhook> Webhook()
        {
            calls++;
            return Task.FromResult(new VideoWebhook("https://example.com/hook", received));
        }

        var result = await CreateTestModel(new UpstreamRecordingHandler("{}")).HandleWebhookAsync(Webhook, CancellationToken.None);

        Assert.Equal(1, calls);
        Assert.Equal("https://example.com/hook", result.Url);
        Assert.Same(received, result.Received);
    }
}
