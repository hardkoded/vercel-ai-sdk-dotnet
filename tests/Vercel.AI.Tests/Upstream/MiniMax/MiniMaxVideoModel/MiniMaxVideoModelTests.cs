// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel</c>.</summary>
public sealed class MiniMaxVideoModelTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel::";

    [Fact]
    [UpstreamTest(
        Prefix + "should leave the generic webhook hook undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_leave_the_generic_webhook_hook_undefined()
    {
        IVideoCaller model = CreateModel(Server());

        Assert.False(model.CanWebhook);
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            model.HandleWebhookAsync(
                () => Task.FromResult(new VideoWebhook("https://example.com", Task.CompletedTask)),
                CancellationToken.None
            )
        );
    }
}
