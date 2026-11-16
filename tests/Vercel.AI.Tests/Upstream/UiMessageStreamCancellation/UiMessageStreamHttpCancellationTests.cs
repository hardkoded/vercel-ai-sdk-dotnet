// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.UiMessageStreamCancellation;

/// <summary>Port of <c>ui-message-stream-cancellation.node.test.ts</c> &gt; <c>UI message stream HTTP cancellation</c>.</summary>
public sealed class UiMessageStreamHttpCancellationTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/ui-message-stream-cancellation.node.test.ts::UI message stream HTTP cancellation::";

    [Theory]
    [InlineData("abort-first")]
    [InlineData("cancel-first")]
    [UpstreamTest(Prefix + "reports consumer cancellation for the same-tick streamText race ($s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Reports_consumer_cancellation_for_the_same_tick_streamText_race(string order)
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

        if (order == "abort-first")
        {
            context.Abort.Abort(new OperationCanceledException("The operation was aborted."));
            await reader.DisposeAsync();
        }
        else
        {
            var cancelled = reader.DisposeAsync();
            context.Abort.Abort(new OperationCanceledException("The operation was aborted."));
            await cancelled;
        }

        var ended = await WaitForEnd(context.Ended.Task);
        await WaitFor(() => context.OnAbortFired);

        Assert.False(ended.IsAborted);
        Assert.True(ended.IsCancelled);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, ended.Outcome.Status);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports consumer cancellation when abort and response cancellation happen in the same tick", Coverage = UpstreamCoverage.Covered)]
    public async Task Reports_consumer_cancellation_when_abort_and_response_cancellation_happen_in_the_same_tick()
    {
        var ended = await RunDisconnectScenario(consumeSseStream: false);

        Assert.False(ended.IsAborted);
        Assert.True(ended.IsCancelled);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, ended.Outcome.Status);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports an observed abort when consumeSseStream keeps the source flowing", Coverage = UpstreamCoverage.Covered, Note = "isCancelled is a bool in .NET, so 'undefined' is false.")]
    public async Task Reports_an_observed_abort_when_consumeSseStream_keeps_the_source_flowing()
    {
        var ended = await RunDisconnectScenario(consumeSseStream: true);

        Assert.True(ended.IsAborted);
        Assert.Equal(UIMessageStreamOutcomeStatus.Aborted, ended.Outcome.Status);
        Assert.False(ended.IsCancelled);
    }

    private static async Task<UIMessageStreamOnEndEvent> RunDisconnectScenario(bool consumeSseStream)
    {
        StreamContext? context = null;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        await using var app = builder.Build();
        app.Run(async http =>
        {
            context = SetupStream();
            started.TrySetResult();

            var response = UIMessageStreamResponses.CreateUIMessageStreamResponse(
                ReadableStream<JsonObject>.From(context.Stream),
                new UIMessageStreamResponseInit
                {
                    ConsumeSseStream = consumeSseStream ? stream => ConsumeStream.ConsumeAsync(stream) : null,
                });
            http.Response.StatusCode = response.Status;
            foreach (var header in response.Headers.Entries())
            {
                http.Response.Headers.Append(header.Key, header.Value);
            }

            var reader = response.Body.GetReader();
            var finished = false;
            using var closed = http.RequestAborted.Register(() =>
            {
                if (!finished)
                {
                    // Upstream aborts and then cancels. In JavaScript nothing runs between the two calls (the same tick). Here the abort reaches
                    // the run on other threads right away, so the cancellation goes first to keep nothing observable between the two.
                    _ = reader.CancelAsync();
                    context.Abort.Abort(new OperationCanceledException("The operation was aborted."));
                }
            });

            try
            {
                while (true)
                {
                    var read = await reader.ReadAsync();
                    if (read.Done || read.Rejected)
                    {
                        break;
                    }

                    await http.Response.Body.WriteAsync(read.Value);
                    await http.Response.Body.FlushAsync();
                }

                finished = true;
            }
            catch (Exception)
            {
                // The response reader is cancelled when the client disconnects.
            }
        });

        await app.StartAsync();
        try
        {
            var address = new Uri(app.Urls.First());
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(address.Host, address.Port);
            var stream = client.GetStream();
            await stream.WriteAsync(Encoding.ASCII.GetBytes($"GET / HTTP/1.1\r\nHost: {address.Authority}\r\n\r\n"));
            await ReadUntilText(stream);

            // The client abort: an immediate reset of the connection, so the server sees the disconnect right away.
            client.LingerState = new System.Net.Sockets.LingerOption(true, 0);
            client.Close();

            await started.Task;
            var ended = await WaitForEnd(context!.Ended.Task);
            await WaitFor(() => context.OnAbortFired);
            return ended;
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static async Task ReadUntilText(Stream body)
    {
        var buffer = new byte[4096];
        var received = new StringBuilder();
        while (true)
        {
            var count = await body.ReadAsync(buffer);
            Assert.NotEqual(0, count); // Response ended before a text delta was received.
            received.Append(Encoding.UTF8.GetString(buffer, 0, count));
            if (received.ToString().Contains("text-delta", StringComparison.Ordinal))
            {
                return;
            }
        }
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
