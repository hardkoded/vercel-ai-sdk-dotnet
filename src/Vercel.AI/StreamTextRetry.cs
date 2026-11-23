// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;

namespace Vercel.AI;

// The model call of one step that retries after a provider error part. It maps to the stream retry of streamText: the parts of the
// failed attempt that were already streamed stay in the consumer-facing stream, a boundary part separates the attempts, and the tool
// parts of an attempt wait until the attempt ends, so a failed attempt starts no tool.
internal sealed class StreamTextRetry
{
    private readonly StreamTextOptions _options;
    private readonly StreamAbort _abort;
    private readonly int _streamRetries;
    private readonly bool _canRetryViaOnError;
    private readonly bool _bufferToolParts;
    private readonly Func<CancellationToken, Task<StreamLanguageModelCallResult>> _callAgain;
    private readonly StreamTextErrorHandling _errors;
    private int _automaticRetries;
    private int _callbackRetries;

    public StreamTextRetry(
        StreamTextOptions options,
        StreamAbort abort,
        StreamLanguageModelCallResult initial,
        int streamRetries,
        StreamTextErrorHandling errors,
        Func<CancellationToken, Task<StreamLanguageModelCallResult>> callAgain
    )
    {
        _options = options;
        _abort = abort;
        _streamRetries = streamRetries;
        _errors = errors;
        _callAgain = callAgain;
        _canRetryViaOnError = options.StreamRetries != null && options.OnErrorRetry != null;
        _bufferToolParts = streamRetries > 0 || _canRetryViaOnError;
        Current = initial;
    }

    // The model call that is being read. Its request and response describe the step.
    public StreamLanguageModelCallResult Current { get; private set; }

    public async IAsyncEnumerable<TextStreamPart> Read(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var enumerator = Current.Stream.GetAsyncEnumerator(cancellationToken);
        var buffered = new List<TextStreamPart>();
        var openText = new HashSet<string>(StringComparer.Ordinal);
        var openReasoning = new HashSet<string>(StringComparer.Ordinal);
        var boundaryPending = false;
        try
        {
            while (true)
            {
                var hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                var part = hasNext ? enumerator.Current : null;
                if (boundaryPending)
                {
                    yield return StreamRetryAttemptBoundaryPart.CreateStreamRetryAttemptBoundaryPart(
                        part is ModelCallStartPart start ? start.Warnings : []
                    );
                    boundaryPending = false;
                }

                if (part == null)
                {
                    foreach (var held in Flush(buffered, openText, openReasoning))
                    {
                        yield return held;
                    }

                    yield break;
                }

                if (part is ModelCallEndPart)
                {
                    foreach (var held in Flush(buffered, openText, openReasoning))
                    {
                        yield return held;
                    }

                    yield return part;
                    continue;
                }

                var isToolPart =
                    part
                    is ToolInputStartPart
                        or ToolInputDeltaPart
                        or ToolInputEndPart
                        or ToolCallPart
                        or ToolApprovalRequestPart
                        or ToolApprovalResponsePart
                        or ToolResultPart
                        or ToolErrorPart;
                if (
                    _bufferToolParts
                    && part is not (ErrorPart or PingPart)
                    && (isToolPart || buffered.Count > 0)
                )
                {
                    buffered.Add(part);
                    continue;
                }

                if (part is not ErrorPart error)
                {
                    Track(part, openText, openReasoning);
                    yield return part;
                    continue;
                }

                var failure = error.Error ?? new AiSdkException(error.Message);
                // The part carries its error, so the event processor can tell that the error was reported.
                var forwarded =
                    error.Error == null ? new ErrorPart(error.Message) { Error = failure } : error;
                var isToolChoiceViolation = failure is ToolChoiceViolationException;
                var result = await _errors
                    .NotifyAsync(error, failure, cancellationToken)
                    .ConfigureAwait(false);
                var callbackRequestedRetry = _canRetryViaOnError && result?.Retry == true;
                var automaticRetry = !isToolChoiceViolation && _automaticRetries < _streamRetries;
                var callbackRetry =
                    !isToolChoiceViolation
                    && !automaticRetry
                    && callbackRequestedRetry
                    && _callbackRetries < 1;
                if (!automaticRetry && !callbackRetry)
                {
                    foreach (var held in Flush(buffered, openText, openReasoning))
                    {
                        yield return held;
                    }

                    _errors.MarkHandled(failure);
                    yield return forwarded;
                    continue;
                }

                if (automaticRetry)
                {
                    _automaticRetries++;
                }
                else
                {
                    _callbackRetries++;
                }

                await DisposeAttemptAsync(enumerator).ConfigureAwait(false);
                buffered.Clear();
                foreach (var id in openText)
                {
                    yield return new TextEndPart(id);
                }

                openText.Clear();
                foreach (var id in openReasoning)
                {
                    yield return new ReasoningEndPart(id);
                }

                openReasoning.Clear();
                StreamLanguageModelCallResult? retried = null;
                Exception? retryFailure = null;
                try
                {
                    retried = await _callAgain(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception retryError) when (!cancellationToken.IsCancellationRequested)
                {
                    retryFailure = retryError;
                }

                if (retried == null)
                {
                    yield return new ErrorPart(retryFailure!.Message) { Error = retryFailure };
                    yield break;
                }

                Current = retried;
                _abort.Track(retried.Disposal);
                enumerator = retried.Stream.GetAsyncEnumerator(cancellationToken);
                boundaryPending = true;
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task DisposeAttemptAsync(IAsyncEnumerator<TextStreamPart> enumerator)
    {
        // The provider disposal ends the read of the failed attempt and releases its response.
        await _abort.DisposeAsync(Current.Disposal).ConfigureAwait(false);
        await enumerator.DisposeAsync().ConfigureAwait(false);
    }

    private static IEnumerable<TextStreamPart> Flush(
        List<TextStreamPart> buffered,
        HashSet<string> openText,
        HashSet<string> openReasoning
    )
    {
        var parts = buffered.ToArray();
        buffered.Clear();
        foreach (var part in parts)
        {
            Track(part, openText, openReasoning);
            yield return part;
        }
    }

    private static void Track(
        TextStreamPart part,
        HashSet<string> openText,
        HashSet<string> openReasoning
    )
    {
        switch (part)
        {
            case TextStartPart start:
                openText.Add(start.Id);
                break;
            case TextEndPart end:
                openText.Remove(end.Id);
                break;
            case ReasoningStartPart start:
                openReasoning.Add(start.Id);
                break;
            case ReasoningEndPart end:
                openReasoning.Remove(end.Id);
                break;
        }
    }
}

// Reports the error parts of a stream to onChunk and onError, once each. The retry reports the error parts it sees before it decides
// to retry, and the event processor skips them. Maps to errorsHandledForStreamRetry.
internal sealed class StreamTextErrorHandling
{
    private readonly StreamTextOptions _options;
    private readonly HashSet<Exception> _handled = new();

    public StreamTextErrorHandling(StreamTextOptions options)
    {
        _options = options;
    }

    public bool TakeHandled(ErrorPart part)
    {
        if (part.Error == null)
        {
            return false;
        }

        lock (_handled)
        {
            return _handled.Remove(part.Error);
        }
    }

    public void MarkHandled(Exception error)
    {
        lock (_handled)
        {
            _handled.Add(error);
        }
    }

    // Calls onChunk and onError for the error. The result of onError can request a retry.
    public async Task<StreamTextOnErrorResult?> NotifyAsync(
        ErrorPart part,
        Exception error,
        CancellationToken cancellationToken
    )
    {
        await NotifyChunkAsync(_options, part, cancellationToken).ConfigureAwait(false);
        return await InvokeOnErrorAsync(_options, error, cancellationToken, swallow: true)
            .ConfigureAwait(false);
    }

    // Maps to notify of onChunk. Errors in the callback do not break the stream.
    public static async Task NotifyChunkAsync(
        StreamTextOptions options,
        TextStreamPart part,
        CancellationToken cancellationToken
    )
    {
        if (options.OnChunk == null)
        {
            return;
        }

        try
        {
            await options
                .OnChunk(new StreamTextChunkEvent(part), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Errors in callbacks do not break the generation flow (upstream notify).
        }
    }

    // Maps to onError: the callback that may request a retry, or the plain one.
    public static async Task<StreamTextOnErrorResult?> InvokeOnErrorAsync(
        StreamTextOptions options,
        Exception error,
        CancellationToken cancellationToken,
        bool swallow,
        bool reportToTelemetry = true
    )
    {
        try
        {
            StreamTextOnErrorResult? result = null;
            if (options.OnErrorRetry != null)
            {
                result = await options.OnErrorRetry(error, cancellationToken).ConfigureAwait(false);
            }
            else if (options.OnError != null)
            {
                await options.OnError(error, cancellationToken).ConfigureAwait(false);
            }

            if (
                reportToTelemetry
                && TelemetryDispatcher.Create(options).OnError is { } telemetryOnError
            )
            {
                await telemetryOnError(new TelemetryErrorEvent(null, error), cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception) when (swallow && !cancellationToken.IsCancellationRequested)
        {
            // An error in onError does not retry and does not break the stream.
        }

        return null;
    }
}
