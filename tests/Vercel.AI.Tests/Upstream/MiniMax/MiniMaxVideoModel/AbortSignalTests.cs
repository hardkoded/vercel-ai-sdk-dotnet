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

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; abort signal</c>.</summary>
public sealed class AbortSignalTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > abort signal::";

    [Fact]
    [UpstreamTest(
        Prefix + "should forward the abort signal to the create and poll calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_the_abort_signal_to_the_create_and_poll_calls()
    {
        using var source = new CancellationTokenSource();
        // The call's token is linked to the caller's token; a never-cancellable token would mean it was dropped.
        var signals = new List<bool>();
        var handler = new ParityHandler(call =>
        {
            signals.Add(call.CancellationToken.CanBeCanceled);
            return ParityHandler.Json(
                call.Uri.AbsoluteUri.Contains("/query/")
                    ? SucceededStatusResponse
                    : "{\"task_id\":\"task-123\"}"
            );
        });

        await CreateModel(handler)
            .DoGenerateAsync(
                new MiniMaxCall { CancellationToken = source.Token }.ToCall(),
                source.Token
            );

        Assert.Equal(new[] { true, true }, signals);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stop polling once the signal is aborted",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stop_polling_once_the_signal_is_aborted()
    {
        using var source = new CancellationTokenSource();
        var pollCount = 0;
        var handler = new ParityHandler(call =>
        {
            if (call.Uri.AbsoluteUri.Contains("/query/"))
            {
                pollCount++;
                // Abort while the first poll is in flight.
                source.Cancel();
                return ParityHandler.Json(PendingStatusResponse("running"));
            }

            return ParityHandler.Json("{\"task_id\":\"task-123\"}");
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateModel(handler)
                .DoGenerateAsync(
                    new MiniMaxCall { CancellationToken = source.Token }.ToCall(),
                    source.Token
                )
        );
        Assert.Equal(1, pollCount);
    }
}
