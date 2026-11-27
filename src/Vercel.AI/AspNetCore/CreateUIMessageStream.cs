// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Options for <see cref="UIMessageStreams.CreateUIMessageStream"/>. Maps to the options of <c>createUIMessageStream</c>.</summary>
public sealed class CreateUIMessageStreamOptions
{
    /// <summary>Called with a writer to write UI message chunks to the stream. Maps to <c>execute</c>.</summary>
    public required Func<IUIMessageStreamWriterWithOutcome, Task?> Execute { get; init; }

    /// <summary>
    /// Extracts an error message from an error. Defaults to <c>An error occurred.</c>, so server-side error details are not leaked to the client.
    /// Maps to <c>onError</c>.
    /// </summary>
    public Func<Exception, string>? OnError { get; init; }

    /// <summary>
    /// The original messages. When they are provided, persistence mode is assumed and a message id is provided for the response message.
    /// Maps to <c>originalMessages</c>.
    /// </summary>
    public IReadOnlyList<UIMessage>? OriginalMessages { get; init; }

    /// <summary>Called when each step ends during multi-step agent runs. Maps to <c>onStepEnd</c>.</summary>
    public Func<UIMessageStreamOnStepEndEvent, Task>? OnStepEnd { get; init; }

    /// <summary>Called when each step ends during multi-step agent runs. Maps to the deprecated <c>onStepFinish</c>. Use <see cref="OnStepEnd"/> instead.</summary>
    [Obsolete("Use OnStepEnd instead.")]
    public Func<UIMessageStreamOnStepEndEvent, Task>? OnStepFinish { get; init; }

    /// <summary>Called when the stream ends. Maps to <c>onEnd</c>.</summary>
    public Func<UIMessageStreamOnEndEvent, Task>? OnEnd { get; init; }

    /// <summary>Called when the stream ends. Maps to the deprecated <c>onFinish</c>. Use <see cref="OnEnd"/> instead.</summary>
    [Obsolete("Use OnEnd instead.")]
    public Func<UIMessageStreamOnEndEvent, Task>? OnFinish { get; init; }

    /// <summary>A function that generates a unique id. Defaults to the built-in id generator. Maps to <c>generateId</c>.</summary>
    public Func<string>? GenerateId { get; init; }
}

/// <summary>Creates UI message streams. Maps to <c>createUIMessageStream</c>.</summary>
public static class UIMessageStreams
{
    /// <summary>Creates a UI message stream that can be used to send messages to the client.</summary>
    /// <param name="options">The execute function, error handler, persistence and callback options.</param>
    /// <returns>A stream of UI message chunks.</returns>
    public static ReadableStream<JsonObject> CreateUIMessageStream(
        CreateUIMessageStreamOptions options
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var onError = options.OnError ?? (_ => "An error occurred."); // prevent leaking server error details to the client by default
        var generateId = options.GenerateId ?? IdGenerator.Generate;

        var gate = new object();
        var ongoingStreamPromises = new Queue<Task>();
        var activeReaders = new List<ReadableStreamReader<JsonObject>>();
        var isCancelled = false;
        var outcome = new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Unknown);
        ReadableStreamController<JsonObject> controller = null!;

        var stream = new ReadableStream<JsonObject>(
            start: controllerArg => controller = controllerArg,
            cancel: async () =>
            {
                ReadableStreamReader<JsonObject>[] readers;
                lock (gate)
                {
                    isCancelled = true;
                    readers = [.. activeReaders];
                }

                await Task.WhenAll(readers.Select(reader => reader.CancelAsync()))
                    .ConfigureAwait(false);
            }
        );

        void SafeEnqueue(JsonObject data)
        {
            try
            {
                controller.Enqueue(data);
            }
            catch (Exception)
            {
                // suppress errors when the stream has been closed
            }
        }

        void SetOutcome(UIMessageStreamOutcome newOutcome)
        {
            lock (gate)
            {
                if (
                    outcome.Status == UIMessageStreamOutcomeStatus.Unknown
                    && newOutcome.Status != UIMessageStreamOutcomeStatus.Unknown
                )
                {
                    outcome = newOutcome;
                }
            }
        }

        void FailOutcome(Exception error)
        {
            lock (gate)
            {
                outcome = new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Failed, error);
            }
        }

        void SafeError(Exception error)
        {
            try
            {
                controller.Error(error);
            }
            catch (Exception)
            {
                // suppress errors when the stream has been closed
            }
        }

        void HandleError(Exception error)
        {
            FailOutcome(error);

            string errorText;
            try
            {
                errorText = onError(error);
            }
            catch (Exception onErrorError)
            {
                FailOutcome(onErrorError);
                SafeError(onErrorError);
                return;
            }

            SafeEnqueue(new JsonObject { ["type"] = "error", ["errorText"] = errorText });
        }

        async Task MergeAsync(ReadableStream<JsonObject> streamArg)
        {
            try
            {
                var reader = streamArg.GetReader();
                lock (gate)
                {
                    activeReaders.Add(reader);
                }

                try
                {
                    while (true)
                    {
                        var read = await reader.ReadAsync().ConfigureAwait(false);
                        if (read.Rejected)
                        {
                            throw read.Reason as Exception
                                ?? new InvalidOperationException(
                                    read.Reason?.ToString() ?? "The merged stream failed."
                                );
                        }

                        if (read.Done)
                        {
                            break;
                        }

                        SafeEnqueue(read.Value);
                    }
                }
                finally
                {
                    lock (gate)
                    {
                        activeReaders.Remove(reader);
                    }

                    reader.ReleaseLock();
                }
            }
            catch (Exception error)
            {
                HandleError(error);
            }
        }

        async Task ObserveAsync(Task execution)
        {
            try
            {
                await execution.ConfigureAwait(false);
            }
            catch (Exception error)
            {
                HandleError(error);
            }
        }

        void Add(Task task)
        {
            lock (gate)
            {
                ongoingStreamPromises.Enqueue(task);
            }
        }

        var writer = new Writer(
            SafeEnqueue,
            streamArg =>
            {
                bool cancelled;
                lock (gate)
                {
                    cancelled = isCancelled;
                }

                if (cancelled)
                {
                    _ = streamArg.CancelAsync().ContinueWith(_ => { }, TaskScheduler.Default);
                    return;
                }

                Add(MergeAsync(streamArg));
            },
            SetOutcome,
            onError
        );

        try
        {
            var result = options.Execute(writer);
            if (result is not null)
            {
                Add(ObserveAsync(result));
            }
        }
        catch (Exception error)
        {
            HandleError(error);
        }

        // Wait until all ongoing streams are done. This approach enables merging
        // streams even after execute has returned, as long as there is still an
        // open merged stream. This is important to e.g. forward new streams and
        // from callbacks.
        async Task WaitForStreams()
        {
            try
            {
                while (true)
                {
                    Task next;
                    lock (gate)
                    {
                        if (ongoingStreamPromises.Count == 0)
                        {
                            break;
                        }

                        next = ongoingStreamPromises.Dequeue();
                    }

                    await next.ConfigureAwait(false);
                }
            }
            finally
            {
                try
                {
                    controller.Close();
                }
                catch (Exception)
                {
                    // suppress errors when the stream has been closed
                }
            }
        }

        _ = WaitForStreams();

#pragma warning disable CS0618 // the deprecated aliases are resolved like upstream: onStepEnd ?? onStepFinish
        var onStepEnd = options.OnStepEnd ?? options.OnStepFinish;
        var onEnd = options.OnEnd ?? options.OnFinish;
#pragma warning restore CS0618

        return ReadableStream<JsonObject>.From(
            UIMessageStreamConverter.HandleUIMessageStreamFinish(
                new AsyncIterableStream<JsonObject>(stream),
                generateId(),
                options.OriginalMessages,
                onStepEnd,
                onEnd,
                error => onError(error),
                () =>
                {
                    lock (gate)
                    {
                        return outcome;
                    }
                }
            )
        );
    }

    private sealed class Writer(
        Action<JsonObject> write,
        Action<ReadableStream<JsonObject>> merge,
        Action<UIMessageStreamOutcome> setOutcome,
        Func<Exception, string> onError
    ) : IUIMessageStreamWriterWithOutcome
    {
        public Func<Exception, string>? OnError => onError;

        public void Write(JsonObject part) => write(part);

        public void Merge(ReadableStream<JsonObject> stream) => merge(stream);

        public void SetOutcome(UIMessageStreamOutcome outcome) => setOutcome(outcome);
    }
}
