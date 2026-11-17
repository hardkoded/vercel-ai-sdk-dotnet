// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Vercel.AI.Ui;

namespace Vercel.AI.AspNetCore;

public static partial class UIMessageStreamConverter
{
    /// <summary>
    /// Injects the response message id into the <c>start</c> chunk, tracks the message state, and calls the step and stream end callbacks.
    /// Maps to <c>handleUIMessageStreamFinish</c>.
    /// </summary>
    internal static async IAsyncEnumerable<JsonObject> HandleUIMessageStreamFinish(
        IAsyncEnumerable<JsonObject> stream,
        string? messageId,
        IReadOnlyList<UIMessage>? originalMessages,
        Func<UIMessageStreamOnStepEndEvent, Task>? onStepEnd,
        Func<UIMessageStreamOnEndEvent, Task>? onEnd,
        Action<Exception> onError,
        Func<UIMessageStreamOutcome>? getOutcome,
        Func<UIMessageStreamOnStepEndEvent, Task>? onStepFinish = null,
        Func<UIMessageStreamOnEndEvent, Task>? onFinish = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        originalMessages ??= [];

        // onStepFinish and onFinish are the deprecated aliases of onStepEnd and onEnd
        onStepEnd ??= onStepFinish;
        onEnd ??= onFinish;

        // last message is only relevant for assistant messages
        UIMessage? lastMessage =
            originalMessages.Count > 0 ? originalMessages[originalMessages.Count - 1] : null;
        if (lastMessage?.Role != UIMessageRole.Assistant)
        {
            lastMessage = null;
        }
        else
        {
            // appending to the last message, so we need to use the same id
            messageId = lastMessage.Id;
        }

        var isAborted = false;
        var hasProcessingFailure = false;
        Exception? processingError = null;

        void RecordProcessingFailure(Exception error)
        {
            hasProcessingFailure = true;
            processingError = error;
        }

        async IAsyncEnumerable<JsonObject> InjectId(
            [EnumeratorCancellation] CancellationToken token
        )
        {
            await foreach (var chunk in stream.WithCancellation(token).ConfigureAwait(false))
            {
                var outputChunk = chunk;

                // when there is no messageId in the start chunk, but the user checked for persistence, inject the messageId into the chunk
                if (
                    (string?)chunk["type"] == "start"
                    && chunk["messageId"] == null
                    && messageId != null
                )
                {
                    outputChunk = (JsonObject)chunk.DeepClone();
                    outputChunk["messageId"] = messageId;
                }

                if ((string?)chunk["type"] == "abort")
                {
                    isAborted = true;
                }

                yield return outputChunk;
            }
        }

        var idInjectedStream = InjectId(cancellationToken);

        // Only process the stream if we need to track state for callbacks
        if (onEnd == null && onStepEnd == null)
        {
            await foreach (var chunk in idInjectedStream.ConfigureAwait(false))
            {
                yield return chunk;
            }

            yield break;
        }

        var state = UIMessageStreamProcessing.CreateStreamingUIMessageState(
            lastMessage != null ? UIMessageJson.ToJson(lastMessage) : null,
            messageId ?? string.Empty
        ); // will be overridden by the stream

        async Task RunUpdateMessageJob(Func<UpdateMessageJobContext, Task> job)
        {
            try
            {
                await job(new UpdateMessageJobContext(state, _ => { })).ConfigureAwait(false);
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                RecordProcessingFailure(error);
                throw;
            }
        }

        var endCalled = false;

        async Task CallOnEnd(bool isCancelled)
        {
            if (endCalled || onEnd == null)
            {
                return;
            }

            endCalled = true;

            var isContinuation = (string?)state.Message["id"] == lastMessage?.Id;
            var declaredOutcome = getOutcome?.Invoke()
                ?? new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Unknown);
            var outcome =
                hasProcessingFailure
                    ? new UIMessageStreamOutcome(
                        UIMessageStreamOutcomeStatus.Failed,
                        processingError
                    )
                : declaredOutcome.Status == UIMessageStreamOutcomeStatus.Unknown && isAborted
                    ? new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Aborted)
                : declaredOutcome;
            var isConsumerCancellation =
                isCancelled && outcome.Status == UIMessageStreamOutcomeStatus.Unknown;

            var responseMessage = UIMessageJson.FromJson(state.Message);
            await onEnd(
                    new UIMessageStreamOnEndEvent(
                        MessagesWith(originalMessages, isContinuation, responseMessage),
                        isContinuation,
                        isAborted || outcome.Status == UIMessageStreamOutcomeStatus.Aborted,
                        outcome,
                        responseMessage
                    )
                    {
                        IsCancelled = isConsumerCancellation,
                        FinishReason = state.FinishReason,
                    }
                )
                .ConfigureAwait(false);
        }

        async Task CallOnStepEnd()
        {
            if (onStepEnd == null)
            {
                return;
            }

            var isContinuation = (string?)state.Message["id"] == lastMessage?.Id;

            try
            {
                // a snapshot, because the state keeps changing
                var responseMessage = UIMessageJson.FromJson(state.Message);
                await onStepEnd(
                        new UIMessageStreamOnStepEndEvent(
                            MessagesWith(originalMessages, isContinuation, responseMessage),
                            isContinuation,
                            responseMessage
                        )
                    )
                    .ConfigureAwait(false);
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                onError(error);
            }
        }

        var completed = false;
        try
        {
            await foreach (
                var chunk in UIMessageStreamProcessing
                    .ProcessUIMessageStream(
                        idInjectedStream,
                        new ProcessUIMessageStreamOptions(RunUpdateMessageJob, onError),
                        cancellationToken
                    )
                    .ConfigureAwait(false)
            )
            {
                if ((string?)chunk["type"] == "finish-step")
                {
                    await CallOnStepEnd().ConfigureAwait(false);
                }

                yield return chunk;
            }

            completed = true;
        }
        finally
        {
            // a consumer that stops early, or a stream that fails, ends the stream without a flush
            await CallOnEnd(isCancelled: !completed).ConfigureAwait(false);
        }
    }

    private static List<UIMessage> MessagesWith(
        IReadOnlyList<UIMessage> originalMessages,
        bool isContinuation,
        UIMessage responseMessage
    )
    {
        var messages = new List<UIMessage>(originalMessages);
        if (isContinuation && messages.Count > 0)
        {
            messages.RemoveAt(messages.Count - 1);
        }

        messages.Add(responseMessage);
        return messages;
    }
}
