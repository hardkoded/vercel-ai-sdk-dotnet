// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Options of <see cref="UIMessageStreamConverter.ToUIMessageStream"/>. Maps to <c>UIMessageStreamOptions</c>.</summary>
public sealed class UIMessageStreamOptions
{
    /// <summary>The tools of the call. A call, input, result or error of a dynamic tool is marked as dynamic. Maps to the <c>tools</c> argument of <c>toUIMessageStream</c>.</summary>
    public IReadOnlyList<Tool>? Tools { get; init; }

    /// <summary>Whether reasoning parts are sent. Defaults to <c>true</c>. Maps to <c>sendReasoning</c>.</summary>
    public bool SendReasoning { get; init; } = true;

    /// <summary>Whether source parts are sent. Defaults to <c>false</c>. Maps to <c>sendSources</c>.</summary>
    public bool SendSources { get; init; }

    /// <summary>Whether the <c>start</c> chunk is sent. Defaults to <c>true</c>. Maps to <c>sendStart</c>.</summary>
    public bool SendStart { get; init; } = true;

    /// <summary>Whether the <c>finish</c> chunk is sent. Defaults to <c>true</c>. Maps to <c>sendFinish</c>.</summary>
    public bool SendFinish { get; init; } = true;

    /// <summary>
    /// Formats stream errors. Also formats invalid tool calls and application-executed tool errors when <see cref="RedactToolErrors"/> is enabled.
    /// Provider-executed tool execution errors bypass this callback so their original error data survives model-message round trips.
    /// Null writes <c>An error occurred.</c>. Maps to <c>onError</c>.
    /// </summary>
    public Func<Exception, string>? OnError { get; init; }

    /// <summary>
    /// Replaces the error text of failed tool calls with the result of <see cref="OnError"/>.
    /// Defaults to <c>true</c> when <see cref="OnError"/> is provided and to <c>false</c> otherwise. Maps to <c>redactToolErrors</c>.
    /// </summary>
    public bool? RedactToolErrors { get; init; }

    /// <summary>
    /// Called for each stream part. Its value is sent with the <c>start</c> and <c>finish</c> chunks, and with a separate <c>message-metadata</c> chunk for any other part.
    /// Maps to the function form of <c>messageMetadata</c>.
    /// </summary>
    public Func<TextStreamPart, JsonElement?>? MessageMetadata { get; init; }

    /// <summary>
    /// The messages of the conversation. When the last one is an assistant message, the response continues it and reuses its id.
    /// Maps to <c>originalMessages</c>.
    /// </summary>
    public IReadOnlyList<UIMessage>? OriginalMessages { get; init; }

    /// <summary>
    /// Generates the id of the response message, unless the last original message is an assistant message, whose id is reused.
    /// Without it, the <c>start</c> chunk has no <c>messageId</c>. Maps to <c>generateMessageId</c>.
    /// </summary>
    public Func<string>? GenerateMessageId { get; init; }

    /// <summary>Called when each step ends during multi-step agent runs. Maps to <c>onStepEnd</c>.</summary>
    public Func<UIMessageStreamOnStepEndEvent, Task>? OnStepEnd { get; init; }

    /// <summary>Called when each step ends during multi-step agent runs. Maps to the deprecated <c>onStepFinish</c>. Use <see cref="OnStepEnd"/> instead.</summary>
    [Obsolete("Use OnStepEnd instead.")]
    public Func<UIMessageStreamOnStepEndEvent, Task>? OnStepFinish { get; init; }

    /// <summary>
    /// Called when the stream ends, whether it completed, failed, was aborted, or the consumer cancelled it. Maps to <c>onEnd</c>.
    /// </summary>
    public Func<UIMessageStreamOnEndEvent, Task>? OnEnd { get; init; }

    /// <summary>Called when the stream ends. Maps to the deprecated <c>onFinish</c>. Use <see cref="OnEnd"/> instead.</summary>
    [Obsolete("Use OnEnd instead.")]
    public Func<UIMessageStreamOnEndEvent, Task>? OnFinish { get; init; }
}

public static partial class UIMessageStreamConverter
{
    /// <summary>
    /// Converts a stream of <see cref="TextStreamPart"/> parts (as emitted by <c>streamText</c>) into a stream of UI message chunks.
    /// It adds the response message id when <see cref="UIMessageStreamOptions.GenerateMessageId"/> is set.
    /// It also calls <see cref="UIMessageStreamOptions.OnStepEnd"/> and <see cref="UIMessageStreamOptions.OnEnd"/>.
    /// </summary>
    /// <param name="stream">The parts to convert.</param>
    /// <param name="options">Conversion options.</param>
    /// <param name="cancellationToken">Cancels the read of <paramref name="stream"/>.</param>
    public static IAsyncEnumerable<JsonObject> ToUIMessageStream(
        IAsyncEnumerable<TextStreamPart> stream,
        UIMessageStreamOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return ToUIMessageStreamWithDefaultTools(stream, options, null, cancellationToken);
    }

    // The tools of the call are the default of options.Tools, as the result passes them to toUIMessageStream upstream.
    internal static async IAsyncEnumerable<JsonObject> ToUIMessageStreamWithDefaultTools(
        IAsyncEnumerable<TextStreamPart> stream,
        UIMessageStreamOptions? options,
        IReadOnlyList<Tool>? defaultTools,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        options ??= new UIMessageStreamOptions();

        var outcome = new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Unknown);
        var hasFatalFailure = false;

        void SetSourceOutcome(UIMessageStreamOutcome newOutcome)
        {
            if (
                !hasFatalFailure
                && outcome.Status != UIMessageStreamOutcomeStatus.Completed
                && outcome.Status != UIMessageStreamOutcomeStatus.Aborted
                && newOutcome.Status != UIMessageStreamOutcomeStatus.Unknown
                && (
                    outcome.Status == UIMessageStreamOutcomeStatus.Unknown
                    || newOutcome.Status != UIMessageStreamOutcomeStatus.Failed
                )
            )
            {
                outcome = newOutcome;
            }
        }

        void FailOutcome(Exception error)
        {
            hasFatalFailure = true;
            outcome = new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Failed, error);
        }

        // upstream getResponseUIMessageId: a continued assistant message keeps its id
        string? responseMessageId = null;
        if (options.GenerateMessageId != null && options.OriginalMessages is { } originals)
        {
            // when there are no original messages (no persistence), the client generates the message id
            var lastOriginal = originals.Count > 0 ? originals[originals.Count - 1] : null;
            responseMessageId =
                lastOriginal?.Role == UIMessageRole.Assistant
                    ? lastOriginal.Id
                    : options.GenerateMessageId();
        }

        var chunkOptions = new ToUIMessageChunkOptions
        {
            Tools = options.Tools ?? defaultTools,
            SendReasoning = options.SendReasoning,
            SendSources = options.SendSources,
            SendStart = options.SendStart,
            SendFinish = options.SendFinish,
            OnError = options.OnError,
            RedactToolErrors = options.RedactToolErrors,
            ResponseMessageId = responseMessageId,
        };

        var chunks = TrackOutcome(
            ConvertToUIMessageChunks(
                stream,
                chunkOptions,
                options.MessageMetadata,
                SetSourceOutcome,
                cancellationToken
            ),
            FailOutcome
        );

        await foreach (
            var chunk in HandleUIMessageStreamFinish(
                    chunks,
                    responseMessageId ?? options.GenerateMessageId?.Invoke(),
                    options.OriginalMessages,
                    options.OnStepEnd,
                    options.OnEnd,
                    error => (options.OnError ?? (_ => "An error occurred."))(error),
                    () => outcome,
#pragma warning disable CS0618 // the deprecated aliases are resolved by HandleUIMessageStreamFinish
                    options.OnStepFinish,
                    options.OnFinish,
#pragma warning restore CS0618
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
        {
            yield return chunk;
        }
    }

    // A failure of the source stream or of the conversion is fatal: it overrides every outcome that the stream declared.
    private static async IAsyncEnumerable<JsonObject> TrackOutcome(
        IAsyncEnumerable<JsonObject> chunks,
        Action<Exception> failOutcome
    )
    {
        await using var enumerator = chunks.GetAsyncEnumerator();
        while (true)
        {
            try
            {
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    yield break;
                }
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                failOutcome(error);
                throw;
            }

            yield return enumerator.Current;
        }
    }

    private static async IAsyncEnumerable<JsonObject> ConvertToUIMessageChunks(
        IAsyncEnumerable<TextStreamPart> stream,
        ToUIMessageChunkOptions chunkOptions,
        Func<TextStreamPart, JsonElement?>? messageMetadataSelector,
        Action<UIMessageStreamOutcome> setSourceOutcome,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var knownToolInvalidIds = new HashSet<string>(StringComparer.Ordinal);

        // streamText follows each invalid tool call with a tool-error part so the model receives a tool result. The
        // tool-input-error chunk already reports the failure, so that tool-error is skipped to avoid a second onError call.
        // TODO AI SDK 8: remove once invalid tool calls no longer inject tool-error parts.
        var invalidToolCallIds = new HashSet<string>(StringComparer.Ordinal);

        // When tool errors are redacted, a failed tool call has two error texts: the model receives the error text with the next step,
        // and the UI receives the onError result. Reasoning from a later step was produced after the model read its text, so UI messages
        // that contain both no longer reproduce the history that this reasoning belongs to.
        string? mismatchedToolCallIdInCurrentStep = null;
        string? mismatchedToolCallIdInEarlierStep = null;
        var hasWarnedAboutToolErrorMismatch = false;

        await foreach (var part in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            // the source declares the outcome of the stream
            switch (part)
            {
                case FinishPart:
                    setSourceOutcome(
                        new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
                    );
                    break;
                case AbortPart:
                    setSourceOutcome(
                        new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Aborted)
                    );
                    break;
                case ErrorPart errorPart:
                    setSourceOutcome(
                        new UIMessageStreamOutcome(
                            UIMessageStreamOutcomeStatus.Failed,
                            errorPart.Error ?? new Exception(errorPart.Message)
                        )
                    );
                    break;
            }

            if (part is ToolCallPart { ToolCall.Invalid: true } invalidCall)
            {
                invalidToolCallIds.Add(invalidCall.ToolCall.ToolCallId);
            }

            if (
                part is ToolErrorPart { Result.ProviderExecuted: false } invalidToolError
                && invalidToolCallIds.Remove(invalidToolError.Result.ToolCallId)
            )
            {
                continue;
            }

            if (
                part is ToolCallPart
                {
                    ToolCall: { Invalid: true, Error: not NoSuchToolError }
                } knownToolInvalid
            )
            {
                knownToolInvalidIds.Add(knownToolInvalid.ToolCall.ToolCallId);
            }

            if (part is StartStepPart)
            {
                // the model receives the tool errors of a step with the next step
                mismatchedToolCallIdInEarlierStep ??= mismatchedToolCallIdInCurrentStep;
                mismatchedToolCallIdInCurrentStep = null;
            }

            var messageMetadata = messageMetadataSelector?.Invoke(part);
            var hasMessageMetadata =
                messageMetadata is
                { ValueKind: not (JsonValueKind.Null or JsonValueKind.Undefined) };
            var chunk = ToUIMessageChunkWithMetadata(part, chunkOptions, messageMetadata);

            if (
                part is ReasoningStartPart
                && chunk != null
                && mismatchedToolCallIdInEarlierStep != null
                && !hasWarnedAboutToolErrorMismatch
            )
            {
                hasWarnedAboutToolErrorMismatch = true;
                LogToolErrorMismatchWarning(mismatchedToolCallIdInEarlierStep);
            }

            if (
                part
                    is ToolCallPart
                    {
                        ToolCall: { Invalid: true, ProviderExecuted: false }
                    } mismatchedCall
                && chunk != null
                && ModelToolErrorText(mismatchedCall.ToolCall.Error!) is { } invalidModelText
                && (string?)chunk["errorText"] != invalidModelText
            )
            {
                mismatchedToolCallIdInCurrentStep ??= mismatchedCall.ToolCall.ToolCallId;
            }
            else if (
                part is ToolErrorPart { Result.ProviderExecuted: false } mismatchedError
                && chunk != null
                && ModelToolErrorText(mismatchedError.Result) is { } modelErrorText
                && (string?)chunk["errorText"] != modelErrorText
            )
            {
                mismatchedToolCallIdInCurrentStep ??= mismatchedError.Result.ToolCallId;
            }

            if (
                chunk != null
                && part is ToolErrorPart knownError
                && knownToolInvalidIds.Contains(knownError.Result.ToolCallId)
            )
            {
                chunk.Remove("dynamic");
            }

            if (chunk != null && part is not StepFinishPart)
            {
                yield return chunk;
            }

            // start and finish chunks already include the metadata; any other part gets a separate message-metadata chunk
            if (hasMessageMetadata && part is not (StartPart or FinishPart))
            {
                yield return new JsonObject
                {
                    ["type"] = "message-metadata",
                    ["messageMetadata"] = JsonNode.Parse(messageMetadata!.Value.GetRawText()),
                };
            }

            // step metadata is applied before the boundary triggers a UI snapshot
            if (chunk != null && part is StepFinishPart)
            {
                yield return chunk;
            }
        }
    }

    private static void LogToolErrorMismatchWarning(string toolCallId) =>
        LogWarnings.Log(
            new LogWarningsOptions(
                new ModelWarning[]
                {
                    new OtherWarning(
                        $"The error text sent to the UI for tool call \"{toolCallId}\" differs from the error text the model received, "
                            + "and reasoning from a later step was sent to the UI. "
                            + "Converting these UI messages back to model messages changes the tool result. "
                            + "Models that bind reasoning to the preceding messages (for example Anthropic models with preserved thinking) reject that history. "
                            + "See https://ai-sdk.dev/docs/ai-sdk-ui/chatbot-tool-usage#tool-error-text-and-message-history"
                    ),
                }
            )
        );
}
