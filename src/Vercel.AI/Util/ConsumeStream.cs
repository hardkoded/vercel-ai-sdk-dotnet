// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Reads a stream to the end. Maps to <c>consumeStream</c>.</summary>
public static class ConsumeStream
{
    /// <summary>
    /// Consumes <paramref name="stream"/> until it is fully read. It does not process or return the data.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="onError">Called when reading fails. The failure is not rethrown.</param>
    /// <param name="abortSignal">Cancels the stream when it aborts.</param>
    /// <returns>A task that completes when the stream is fully consumed.</returns>
    public static async Task ConsumeAsync<T>(ReadableStream<T> stream, Action<Exception>? onError = null, AbortSignal? abortSignal = null)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var reader = stream.GetReader();
        void CancelOnAbort() => _ = reader.CancelAsync().ContinueWith(static task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);

        if (abortSignal?.Aborted == true)
        {
            CancelOnAbort();
        }
        else
        {
            abortSignal?.AddAbortHandler(CancelOnAbort);
        }

        try
        {
            while (true)
            {
                var read = await reader.ReadAsync().ConfigureAwait(false);
                if (read.Rejected)
                {
                    onError?.Invoke(read.Reason as Exception ?? new UndefinedStreamError());
                    break;
                }

                if (read.Done)
                {
                    break;
                }
            }
        }
        finally
        {
            abortSignal?.RemoveAbortHandler(CancelOnAbort);
            reader.ReleaseLock();
        }
    }
}
