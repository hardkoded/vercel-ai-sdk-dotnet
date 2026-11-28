// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>abort signal</c>.
/// </summary>
public sealed class DoGenerateAbortSignalTests
{
    private const string Prefix = DoGenerateSupport.Id + "abort signal::";

    [Fact]
    [UpstreamTest(
        Prefix + "rejects when the abort signal is aborted before the request is dispatched",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_when_the_abort_signal_is_aborted_before_the_request_is_dispatched()
    {
        // The signal and the token of a call are cancelled together.
        var abortController = new AbortController();
        abortController.Abort();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.AbortSignal = abortController.Signal;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            GoogleInteractionsStreamSupport
                .Model(
                    new SequencedHandler().Json(
                        GoogleInteractionsStreamSupport.JsonFixture("basic")
                    )
                )
                .DoGenerateAsync(options, cancellation.Token)
        );
    }
}
