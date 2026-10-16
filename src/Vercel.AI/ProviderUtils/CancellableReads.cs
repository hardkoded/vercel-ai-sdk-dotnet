// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// Reads from a response body and stops waiting when the caller cancels.
/// <c>netstandard2.0</c> readers take no token, so the wait is abandoned and the caller disposes the stream.
/// </summary>
internal static class CancellableReads
{
    internal static Task<string?> ReadLineAsync(StreamReader reader, CancellationToken cancellationToken)
    {
#if NET
        return reader.ReadLineAsync(cancellationToken).AsTask();
#else
        return WaitAsync(reader.ReadLineAsync(), cancellationToken);
#endif
    }

    internal static Task<int> ReadAsync(StreamReader reader, char[] buffer, CancellationToken cancellationToken)
    {
#if NET
        return reader.ReadAsync(buffer.AsMemory(), cancellationToken).AsTask();
#else
        return WaitAsync(reader.ReadAsync(buffer, 0, buffer.Length), cancellationToken);
#endif
    }

    internal static Task<string> ReadAsStringAsync(HttpContent content, CancellationToken cancellationToken)
    {
#if NET
        return content.ReadAsStringAsync(cancellationToken);
#else
        return WaitAsync(content.ReadAsStringAsync(), cancellationToken);
#endif
    }

    internal static Task<byte[]> ReadAsByteArrayAsync(HttpContent content, CancellationToken cancellationToken)
    {
#if NET
        return content.ReadAsByteArrayAsync(cancellationToken);
#else
        return WaitAsync(content.ReadAsByteArrayAsync(), cancellationToken);
#endif
    }

#if !NET
    private static async Task<T> WaitAsync<T>(Task<T> task, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled || task.IsCompleted)
        {
            return await task.ConfigureAwait(false);
        }

        var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
        {
            if (task != await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false))
            {
                // Observe the abandoned read so a later fault is not unobserved.
                _ = task.ContinueWith(static t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                throw new OperationCanceledException(cancellationToken);
            }
        }

        return await task.ConfigureAwait(false);
    }
#endif
}
