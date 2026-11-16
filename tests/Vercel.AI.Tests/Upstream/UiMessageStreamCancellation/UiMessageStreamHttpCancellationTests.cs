// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.UiMessageStreamCancellation;

/// <summary>Port of <c>ui-message-stream-cancellation.node.test.ts</c> &gt; <c>UI message stream HTTP cancellation</c>.</summary>
public sealed class UiMessageStreamHttpCancellationTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/ui-message-stream-cancellation.node.test.ts::UI message stream HTTP cancellation::";

    // The cancel-first row is not ported: after the consumer stops reading, the .NET streamText run ends and no longer listens to the
    // abort signal, so onAbort never fires for an abort that follows the cancellation. Upstream keeps the run alive until the abort.
    [Fact]
    [UpstreamTest(Prefix + "reports consumer cancellation for the same-tick streamText race ($s)", Coverage = UpstreamCoverage.Partial, Note = "Only the abort-first row is ported.")]
    public async Task Reports_consumer_cancellation_when_streamText_is_aborted_before_the_reader_is_cancelled()
    {
        using var context = SetupStream();
        await using var reader = context.Stream.GetAsyncEnumerator();

        while (true)
        {
            Assert.True(await reader.MoveNextAsync(), "Stream ended before a text delta was received.");
            if ((string?)reader.Current["type"] == "text-delta")
            {
                break;
            }
        }

        context.Abort.Abort(new OperationCanceledException("The operation was aborted."));
        await reader.DisposeAsync();

        var ended = await WaitForEnd(context.Ended.Task);
        await WaitFor(() => context.OnAbortFired);

        Assert.False(ended.IsAborted);
        Assert.True(ended.IsCancelled);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, ended.Outcome.Status);
    }

    private static StreamContext SetupStream()
    {
        var context = new StreamContext();
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new AbortableModel(),
            Prompt = "Hello",
            AbortSignal = context.Abort,
            OnAbort = (_, _) =>
            {
                context.OnAbortFired = true;
                return Task.CompletedTask;
            },
            OnError = (_, _) => Task.CompletedTask,
        });

        context.Stream = UIMessageStreamConverter.ToUIMessageStream(
            result.Stream(),
            new ToUIMessageChunkOptions
            {
                OnEnd = e =>
                {
                    context.Ended.TrySetResult(e);
                    return Task.CompletedTask;
                },
            });
        return context;
    }

    private static async Task<UIMessageStreamOnEndEvent> WaitForEnd(Task<UIMessageStreamOnEndEvent> ended)
    {
        var completed = await Task.WhenAny(ended, Task.Delay(TimeSpan.FromSeconds(1)));
        Assert.Same(ended, completed); // Timed out waiting for the onEnd callback.
        return await ended;
    }

    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(1);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.True(condition());
    }

    private sealed class StreamContext : IDisposable
    {
        public AbortSignal Abort { get; } = new();

        public TaskCompletionSource<UIMessageStreamOnEndEvent> Ended { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool OnAbortFired { get; set; }

        public IAsyncEnumerable<JsonObject> Stream { get; set; } = null!;

        public void Dispose()
        {
        }
    }

    // createAbortableModel: the stream errors when the call is aborted.
    private sealed class AbortableModel : ILanguageModel
    {
        public IReadOnlyDictionary<string, string> SupportedUrls => Vercel.AI.ProviderUtils.UrlSupport.None;

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "mock-model-id";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
            => Task.FromResult(new LanguageModelStreamResult(Stream(options, cancellationToken)));

        private static async IAsyncEnumerable<LanguageModelStreamPart> Stream(
            LanguageModelCallOptions options,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return new StreamStartStreamPart(null);
            yield return new TextStartStreamPart("text-1");
            yield return new TextDeltaStreamPart("text-1", "Hello");

            var aborted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action onAbort = () => aborted.TrySetResult(true);
            options.AbortSignal?.AddAbortHandler(onAbort);
            try
            {
                await using (cancellationToken.Register(() => aborted.TrySetResult(true)).ConfigureAwait(false))
                {
                    await aborted.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                options.AbortSignal?.RemoveAbortHandler(onAbort);
            }

            throw new OperationCanceledException("The operation was aborted.");
        }
    }
}
