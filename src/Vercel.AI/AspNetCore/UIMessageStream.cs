// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Writes a <see cref="StreamTextResult"/> as an AI SDK UI message stream.</summary>
public static class UIMessageStreamExtensions
{
    /// <summary>
    /// Returns a <c>text/event-stream</c> result a JavaScript <c>useChat</c> client can consume.
    /// Emitted chunks are start, text, reasoning, tool input/output, tool approval request, source-url, step boundaries, finish, abort, and error.
    /// Finish reasons use the protocol values <c>stop</c>, <c>length</c>, <c>content-filter</c>, <c>tool-calls</c>, <c>error</c>, and <c>other</c>.
    /// </summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="keepAliveMs">
    /// Optional idle interval, in milliseconds. Null sends no SSE comments.
    /// A positive value up to 2147483647 writes <c>: stream-open</c> before the start event and <c>: keep-alive</c> while the next part is still pending.
    /// </param>
    /// <param name="onError">
    /// Optional mapper for <c>error</c> chunk text. Null writes <c>An error occurred.</c>. It also formats failed tool calls when
    /// <c>redactToolErrors</c> is enabled, which is the default when this callback is provided. Errors of provider-executed tools are not masked.
    /// </param>
    /// <param name="redactToolErrors">
    /// Replaces the error text of failed tool calls (tool execution errors and invalid tool calls) with the result of <c>onError</c>.
    /// By default, the client receives the same error text as the model. The UI messages are sent back to the model on the next request,
    /// so a different text changes the message history that the model's reasoning and answers are based on. Some models reject such a history,
    /// for example Anthropic models with preserved thinking. To keep error details away from the model and the client, throw a safe error from the tool.
    /// Defaults to <c>true</c> when <c>onError</c> is provided, so that an existing <c>onError</c> keeps handling tool errors, and to <c>false</c> otherwise.
    /// </param>
    public static IResult ToUIMessageStreamResult(
        this StreamTextResult result,
        int? keepAliveMs = null,
        Func<string, string>? onError = null,
        bool? redactToolErrors = null)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return new UIMessageStreamResult(result, keepAliveMs, onError, redactToolErrors);
    }
}

/// <summary>SSE result for the AI SDK UI message protocol.</summary>
public sealed class UIMessageStreamResult : IResult
{
    private const int MaxKeepAliveMilliseconds = 2147483647;
    private const string StreamOpenComment = ": stream-open\n\n";
    private const string KeepAliveComment = ": keep-alive\n\n";

    private readonly StreamTextResult _result;
    private readonly int? _keepAliveMs;
    private readonly Func<string, string>? _onError;
    private readonly bool? _redactToolErrors;

    /// <summary>Creates a result that writes <paramref name="result"/>.</summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="keepAliveMs">Optional idle interval, in milliseconds. Null sends no SSE comments.</param>
    /// <param name="onError">Optional mapper for <c>error</c> chunk text. Null writes <c>An error occurred.</c>. It also formats failed tool calls when <c>redactToolErrors</c> is enabled.</param>
    /// <param name="redactToolErrors">Replaces the error text of failed tool calls with the result of <c>onError</c>. Defaults to <c>true</c> when <c>onError</c> is provided and to <c>false</c> otherwise.</param>
    public UIMessageStreamResult(StreamTextResult result, int? keepAliveMs = null, Func<string, string>? onError = null, bool? redactToolErrors = null)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        ValidateKeepAlive(keepAliveMs);
        _keepAliveMs = keepAliveMs;
        _onError = onError;
        _redactToolErrors = redactToolErrors;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        ApplyProtocolHeaders(httpContext.Response);
        await WriteAsync(_result, httpContext.Response.Body, _keepAliveMs, httpContext.RequestAborted, _onError, _redactToolErrors).ConfigureAwait(false);
    }

    /// <summary>Writes the UI message stream to <paramref name="destination"/>.</summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="destination">The response body or another writable stream.</param>
    /// <param name="keepAliveMs">
    /// Optional idle interval, in milliseconds. Null sends no SSE comments.
    /// A positive value up to 2147483647 writes <c>: stream-open</c> before the start event and <c>: keep-alive</c> while the next part is still pending.
    /// </param>
    /// <param name="cancellationToken">Cancels the write and stops keep-alive comments. ASP.NET Core passes <c>HttpContext.RequestAborted</c>.</param>
    /// <param name="onError">Optional mapper for <c>error</c> chunk text. Null writes <c>An error occurred.</c>. It also formats failed tool calls when <c>redactToolErrors</c> is enabled.</param>
    /// <param name="redactToolErrors">Replaces the error text of failed tool calls with the result of <c>onError</c>. Defaults to <c>true</c> when <c>onError</c> is provided and to <c>false</c> otherwise.</param>
    public static async Task WriteAsync(
        StreamTextResult result,
        Stream destination,
        int? keepAliveMs = null,
        CancellationToken cancellationToken = default,
        Func<string, string>? onError = null,
        bool? redactToolErrors = null)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        ValidateKeepAlive(keepAliveMs);

        // Not disposed on purpose: every chunk is flushed asynchronously, and StreamWriter.Dispose flushes the
        // destination synchronously, which Kestrel rejects. The writer owns no other resources (leaveOpen).
        var writer = new StreamWriter(destination, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);
        var chunks = CreateSseStreamWithKeepAlive(ReadSseChunks(result, onError, redactToolErrors, cancellationToken), keepAliveMs, cancellationToken);
        await foreach (var chunk in chunks.ConfigureAwait(false))
        {
            await writer.WriteAsync(chunk.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Wraps an SSE chunk stream with optional idle comments.
    /// A null interval returns <paramref name="source"/> unchanged.
    /// </summary>
    internal static IAsyncEnumerable<string> CreateSseStreamWithKeepAlive(
        IAsyncEnumerable<string> source,
        int? keepAliveMs,
        CancellationToken cancellationToken = default)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (keepAliveMs is null)
        {
            return source;
        }

        ValidateKeepAlive(keepAliveMs);
        return KeepAlive(source, keepAliveMs.Value, cancellationToken);
    }

    /// <summary>Writes protocol headers for an AI SDK UI message response.</summary>
    /// <param name="response">The HTTP response.</param>
    internal static void ApplyProtocolHeaders(HttpResponse response)
    {
        response.ContentType = "text/event-stream";
        response.Headers["Cache-Control"] = "no-cache";
        response.Headers["x-vercel-ai-ui-message-stream"] = "v1";
        response.Headers["x-accel-buffering"] = "no";
        try
        {
            response.Headers["Connection"] = "keep-alive";
        }
        catch (InvalidOperationException)
        {
            // Kestrel owns the hop-by-hop Connection header.
        }
    }

    /// <summary>Writes already-built UI message chunks as SSE, then <c>data: [DONE]</c>.</summary>
    /// <param name="chunks">Chunk objects. Each one is one <c>data:</c> frame.</param>
    /// <param name="destination">The response body or another writable stream.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    public static async Task WriteChunksAsync(
        IReadOnlyList<JsonObject> chunks,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        if (chunks is null)
        {
            throw new ArgumentNullException(nameof(chunks));
        }

        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        using var writer = new StreamWriter(destination, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);
        foreach (var chunk in chunks)
        {
            if (chunk is null)
            {
                throw new ArgumentException("Chunks cannot contain null.", nameof(chunks));
            }

            await writer.WriteAsync(Frame(chunk).AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        await writer.WriteAsync("data: [DONE]\n\n".AsMemory(), cancellationToken).ConfigureAwait(false);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async IAsyncEnumerable<string> ReadSseChunks(
        StreamTextResult result,
        Func<string, string>? onError,
        bool? redactToolErrors,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var options = new ToUIMessageChunkOptions { OnError = onError, RedactToolErrors = redactToolErrors };
        await foreach (var chunk in UIMessageStreamConverter.ToUIMessageStream(result.Stream(cancellationToken), options, cancellationToken).ConfigureAwait(false))
        {
            yield return Frame(chunk);
        }

        yield return "data: [DONE]\n\n";
    }

    private static readonly TimeSpan SourceStopGrace = TimeSpan.FromSeconds(1);

    private static async IAsyncEnumerable<string> KeepAlive(
        IAsyncEnumerable<string> source,
        int keepAliveMs,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var readCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var enumerator = source.GetAsyncEnumerator(readCancellation.Token);
        Task<bool>? pending = null;
        CancellationTokenSource? timer = null;
        try
        {
            yield return StreamOpenComment;
            pending = enumerator.MoveNextAsync().AsTask();
            timer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            while (true)
            {
                while (!pending.IsCompleted)
                {
                    var delay = Task.Delay(keepAliveMs, timer!.Token);
                    if (await Task.WhenAny(pending, delay).ConfigureAwait(false) == pending)
                    {
                        break;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    yield return KeepAliveComment;
                }

                var finished = timer;
                timer = null;
                finished!.Cancel();
                finished.Dispose();

                if (!await pending.ConfigureAwait(false))
                {
                    yield break;
                }

                var chunk = enumerator.Current;
                yield return chunk;
                pending = enumerator.MoveNextAsync().AsTask();
                timer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }
        }
        finally
        {
            if (timer != null)
            {
                timer.Cancel();
                timer.Dispose();
            }

            if (pending != null && !pending.IsCompleted)
            {
                // The consumer stopped while a read is in flight. Stop the read and wait for it before the enumerator is disposed.
                // A source that ignores cancellation gets a short grace period, so a stalled source cannot hold up the response.
                readCancellation.Cancel();
                try
                {
                    await Task.WhenAny(pending, Task.Delay(SourceStopGrace)).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // The in-flight read is observed before the source enumerator is disposed.
                }
            }

            await enumerator.DisposeAsync().ConfigureAwait(false);
            Observe(pending);
        }
    }

    private static void ValidateKeepAlive(int? keepAliveMs)
    {
        if (keepAliveMs is null)
        {
            return;
        }

        var milliseconds = keepAliveMs.Value;
        if (milliseconds <= 0 || (long)milliseconds > MaxKeepAliveMilliseconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(keepAliveMs),
                milliseconds,
                "keepAliveMs must be a positive duration no greater than 2147483647 ms.");
        }
    }

    private static void Observe(Task? pending)
    {
        if (pending is null)
        {
            return;
        }

        if (pending.IsCompleted)
        {
            _ = pending.Exception;
            return;
        }

        _ = pending.ContinueWith(
            static completed => _ = completed.Exception,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static string Frame(JsonObject payload)
    {
        return "data: " + payload.ToJsonString() + "\n\n";
    }
}
