// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

public static partial class UIMessageStreamConverter
{
    /// <summary>
    /// Converts a stream of <see cref="TextStreamPart"/> parts (as emitted by <c>streamText</c>) into a stream of UI message chunks.
    /// It adds the text and reasoning start and end chunks, which the .NET stream parts do not carry, and the response message id.
    /// </summary>
    /// <param name="stream">The parts to convert.</param>
    /// <param name="options">Conversion options.</param>
    /// <param name="cancellationToken">Cancels the read of <paramref name="stream"/>.</param>
    public static async IAsyncEnumerable<JsonObject> ToUIMessageStream(
        IAsyncEnumerable<TextStreamPart> stream,
        ToUIMessageChunkOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        options ??= new ToUIMessageChunkOptions();
        var chunkOptions = new ToUIMessageChunkOptions
        {
            SendReasoning = options.SendReasoning,
            SendStart = options.SendStart,
            SendFinish = options.SendFinish,
            OnError = options.OnError,
            RedactToolErrors = options.RedactToolErrors,
            ResponseMessageId = options.ResponseMessageId ?? JsonValues.GenerateId("msg_"),
        };

        string? textId = null;
        string? reasoningId = null;
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
            if (part is ToolCallPart { ToolCall.Invalid: true } invalidCall)
            {
                invalidToolCallIds.Add(invalidCall.ToolCall.ToolCallId);
            }

            if (part is ToolErrorPart { Result.ProviderExecuted: false } invalidToolError && invalidToolCallIds.Remove(invalidToolError.Result.ToolCallId))
            {
                continue;
            }

            if (part is ToolCallPart { ToolCall: { Invalid: true, Error: not NoSuchToolError } } knownToolInvalid)
            {
                knownToolInvalidIds.Add(knownToolInvalid.ToolCall.ToolCallId);
            }

            if (part is StartStepPart)
            {
                // the model receives the tool errors of a step with the next step
                mismatchedToolCallIdInEarlierStep ??= mismatchedToolCallIdInCurrentStep;
                mismatchedToolCallIdInCurrentStep = null;
            }

            if (part is TextDeltaPart && textId == null)
            {
                textId = TextId;
                yield return new JsonObject { ["type"] = "text-start", ["id"] = textId };
            }

            var chunk = ToUIMessageChunk(part, chunkOptions);

            if (part is ReasoningDeltaPart && chunk != null && reasoningId == null)
            {
                reasoningId = ReasoningId;
                if (mismatchedToolCallIdInEarlierStep != null && !hasWarnedAboutToolErrorMismatch)
                {
                    hasWarnedAboutToolErrorMismatch = true;
                    LogToolErrorMismatchWarning(mismatchedToolCallIdInEarlierStep);
                }

                yield return new JsonObject { ["type"] = "reasoning-start", ["id"] = reasoningId };
            }

            if (part is ToolCallPart { ToolCall: { Invalid: true, ProviderExecuted: false } } mismatchedCall
                && chunk != null
                && (string?)chunk["errorText"] != ErrorMessage.ToErrorString(mismatchedCall.ToolCall.Error!))
            {
                mismatchedToolCallIdInCurrentStep ??= mismatchedCall.ToolCall.ToolCallId;
            }
            else if (part is ToolErrorPart { Result.ProviderExecuted: false } mismatchedError
                && chunk != null
                && (string?)chunk["errorText"] != ModelToolErrorText(mismatchedError.Result))
            {
                mismatchedToolCallIdInCurrentStep ??= mismatchedError.Result.ToolCallId;
            }

            if (chunk != null && part is ToolErrorPart knownError && knownToolInvalidIds.Contains(knownError.Result.ToolCallId))
            {
                chunk.Remove("dynamic");
            }

            if (part is StepFinishPart stepFinish)
            {
                if (textId != null)
                {
                    var textEnd = new JsonObject { ["type"] = "text-end", ["id"] = textId };
                    var citations = new List<Citation>();
                    foreach (var content in stepFinish.Step.Content)
                    {
                        if (content is GeneratedText { Citations: { } textCitations })
                        {
                            citations.AddRange(textCitations);
                        }
                    }

                    if (citations.Count > 0)
                    {
                        textEnd["citations"] = CitationJson.ToJson(citations);
                    }

                    yield return textEnd;
                    textId = null;
                }

                if (reasoningId != null)
                {
                    yield return new JsonObject { ["type"] = "reasoning-end", ["id"] = reasoningId };
                    reasoningId = null;
                }
            }
            else if (part is FinishPart)
            {
                if (textId != null)
                {
                    yield return new JsonObject { ["type"] = "text-end", ["id"] = textId };
                    textId = null;
                }

                if (reasoningId != null)
                {
                    yield return new JsonObject { ["type"] = "reasoning-end", ["id"] = reasoningId };
                    reasoningId = null;
                }
            }

            if (chunk != null)
            {
                yield return chunk;
            }
        }
    }

    private static void LogToolErrorMismatchWarning(string toolCallId)
        => LogWarnings.Log(new LogWarningsOptions(new ModelWarning[]
        {
            new OtherWarning(
                $"The error text sent to the UI for tool call \"{toolCallId}\" differs from the error text the model received, " +
                "and reasoning from a later step was sent to the UI. " +
                "Converting these UI messages back to model messages changes the tool result. " +
                "Models that bind reasoning to the preceding messages (for example Anthropic models with preserved thinking) reject that history. " +
                "See https://ai-sdk.dev/docs/ai-sdk-ui/chatbot-tool-usage#tool-error-text-and-message-history"),
        }));
}
