// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Vercel.AI.Ui;

namespace Vercel.AI.AspNetCore;

/// <summary>Options of <see cref="UIMessageStreamConverter.ReadUIMessageStream"/>.</summary>
public sealed class ReadUIMessageStreamOptions
{
    /// <summary>Creates options that read <paramref name="stream"/>.</summary>
    public ReadUIMessageStreamOptions(IAsyncEnumerable<JsonObject> stream)
    {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <summary>The stream of UI message chunks to read. Maps to <c>stream</c>.</summary>
    public IAsyncEnumerable<JsonObject> Stream { get; }

    /// <summary>The last assistant message to use as a starting point when the conversation is resumed. Otherwise null. Maps to <c>message</c>.</summary>
    public UIMessage? Message { get; init; }

    /// <summary>Called when an error occurs. Maps to <c>onError</c>.</summary>
    public Action<Exception>? OnError { get; init; }

    /// <summary>Whether to terminate the stream if an error occurs. Maps to <c>terminateOnError</c>.</summary>
    public bool TerminateOnError { get; init; }
}

public static partial class UIMessageStreamConverter
{
    /// <summary>
    /// Transforms a stream of UI message chunks into a stream of <see cref="UIMessage"/>s.
    /// Each element is a different state of the same message as it is being completed. Maps to <c>readUIMessageStream</c>.
    /// </summary>
    /// <param name="options">The stream to read and how to handle errors.</param>
    /// <param name="cancellationToken">Cancels the read of the stream.</param>
    public static async IAsyncEnumerable<UIMessage> ReadUIMessageStream(
        ReadUIMessageStreamOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var snapshots = new Queue<UIMessage>();
        Exception? terminalError = null;

        var state = UIMessageStreamProcessing.CreateStreamingUIMessageState(
            options.Message != null ? UIMessageJson.ToJson(options.Message) : null,
            options.Message?.Id ?? string.Empty);

        void HandleError(Exception error)
        {
            options.OnError?.Invoke(error);

            if (terminalError == null && options.TerminateOnError)
            {
                terminalError = error;
            }
        }

        Task RunUpdateMessageJob(Func<UpdateMessageJobContext, Task> job)
            => job(new UpdateMessageJobContext(state, _ => snapshots.Enqueue(UIMessageJson.FromJson(state.Message))));

        await using var enumerator = UIMessageStreamProcessing.ProcessUIMessageStream(
            options.Stream,
            new ProcessUIMessageStreamOptions(RunUpdateMessageJob, HandleError) { ResetStateOnMessageIdChange = true },
            cancellationToken).GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            bool hasNext;
            try
            {
                hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                HandleError(error);
                if (terminalError != null)
                {
                    throw;
                }

                // a failed stream ends the output
                hasNext = false;
            }

            // erroring a stream discards what is queued
            if (terminalError != null)
            {
                throw terminalError;
            }

            while (snapshots.Count > 0)
            {
                yield return snapshots.Dequeue();
            }

            if (!hasNext)
            {
                yield break;
            }
        }
    }
}
