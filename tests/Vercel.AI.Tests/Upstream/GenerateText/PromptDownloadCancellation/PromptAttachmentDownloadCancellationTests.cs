// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText.PromptDownloadCancellation;

/// <summary>Port of <c>prompt-download-cancellation.test.ts</c> &gt; <c>prompt attachment download cancellation</c>.</summary>
[Collection("DownloadFetch")]
public sealed class PromptAttachmentDownloadCancellationTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/prompt-download-cancellation.test.ts::prompt attachment download cancellation::";

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel generateText downloads when the caller aborts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cancel_generateText_downloads_when_the_caller_aborts()
    {
        using var download = new PendingDownload();
        using var abort = new CancellationTokenSource();
        var model = new TestLanguageModel();

        var operation = Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Messages = CreatePrompt(),
                    MaxRetries = 0,
                },
                abort.Token
            );

        await download.Started.WaitAsync(TimeSpan.FromSeconds(10));
        abort.Cancel();

        Assert.Equal(Settlement.Rejected, await SettleWithin(operation));
        Assert.True(download.Signal.IsCancellationRequested);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel streamText downloads when the caller aborts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cancel_streamText_downloads_when_the_caller_aborts()
    {
        using var download = new PendingDownload();
        using var abort = new CancellationTokenSource();
        var model = new TestLanguageModel();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Messages = CreatePrompt(),
                    MaxRetries = 0,
                    OnError = (_, _) => Task.CompletedTask,
                },
                abort.Token
            );
        var operation = Task.Run(async () =>
        {
            await foreach (var _ in result.Stream(abort.Token)) { }
        });

        await download.Started.WaitAsync(TimeSpan.FromSeconds(10));
        abort.Cancel();

        Assert.NotEqual(Settlement.Pending, await SettleWithin(operation));
        Assert.True(download.Signal.IsCancellationRequested);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel downloads when the total timeout expires",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cancel_downloads_when_the_total_timeout_expires()
    {
        using var download = new PendingDownload();
        var model = new TestLanguageModel();

        var operation = Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Messages = CreatePrompt(),
                    Timeout = new TimeoutConfiguration { TotalMs = 100 },
                    MaxRetries = 0,
                }
            );

        await download.Started.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(Settlement.Rejected, await SettleWithin(operation));
        Assert.True(download.Signal.IsCancellationRequested);
        Assert.Empty(model.Calls);
    }

    private enum Settlement
    {
        Fulfilled,
        Rejected,
        Pending,
    }

    private static async Task<Settlement> SettleWithin(Task operation, int timeoutMs = 1_000)
    {
        var finished = await Task.WhenAny(operation, Task.Delay(timeoutMs));
        if (finished != operation)
        {
            return Settlement.Pending;
        }

        return operation.IsFaulted || operation.IsCanceled
            ? Settlement.Rejected
            : Settlement.Fulfilled;
    }

    private static ModelMessage[] CreatePrompt()
    {
        return new ModelMessage[]
        {
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart("text/plain", "https://example.com/file.txt", null, null),
                }
            ),
        };
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    // Replaces Download.Fetch with a fetch that never completes until its signal is cancelled.
    private sealed class PendingDownload : IDisposable
    {
        private readonly DownloadFetch? _previous = Download.Fetch;
        private readonly TaskCompletionSource<CancellationToken> _started = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        internal PendingDownload()
        {
            Download.Fetch = async (url, request) =>
            {
                _started.TrySetResult(request.AbortSignal);
                await Task.Delay(Timeout.Infinite, request.AbortSignal).ConfigureAwait(false);
                return DownloadResponse.Text("unreachable");
            };
        }

        internal Task Started => _started.Task;

        internal CancellationToken Signal =>
            _started.Task.IsCompleted ? _started.Task.Result : CancellationToken.None;

        public void Dispose()
        {
            Download.Fetch = _previous;
        }
    }
}
