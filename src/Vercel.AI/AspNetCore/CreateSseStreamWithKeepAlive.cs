// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

public sealed partial class UIMessageStreamResult
{
    private const int MaxKeepAliveMilliseconds = 2147483647;
    private const string StreamOpenComment = ": stream-open\n\n";
    private const string KeepAliveComment = ": keep-alive\n\n";

    /// <summary>
    /// Wraps an SSE chunk stream with optional idle comments.
    /// A null interval returns <paramref name="source"/> unchanged.
    /// </summary>
    internal static IAsyncEnumerable<string> CreateSseStreamWithKeepAlive(
        IAsyncEnumerable<string> source,
        int? keepAliveMs,
        CancellationToken cancellationToken = default
    )
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


    private static readonly TimeSpan SourceStopGrace = TimeSpan.FromSeconds(1);

    private static async IAsyncEnumerable<string> KeepAlive(
        IAsyncEnumerable<string> source,
        int keepAliveMs,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        using var readCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken
        );
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

    internal static void ValidateKeepAlive(int? keepAliveMs)
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
                "keepAliveMs must be a positive duration no greater than 2147483647 ms."
            );
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
            TaskScheduler.Default
        );
    }

    /// <summary>
    /// Wraps an SSE chunk stream with optional idle comments.
    /// A null interval returns <paramref name="stream"/> unchanged. Maps to <c>createSseStreamWithKeepAlive</c>.
    /// </summary>
    internal static ReadableStream<string> CreateSseStreamWithKeepAlive(
        ReadableStream<string> stream,
        int? keepAliveMs
    )
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (keepAliveMs is null)
        {
            return stream;
        }

        ValidateKeepAlive(keepAliveMs);
        return ReadableStream<string>.From(
            KeepAlive(new AsyncIterableStream<string>(stream), keepAliveMs.Value, default)
        );
    }
}
