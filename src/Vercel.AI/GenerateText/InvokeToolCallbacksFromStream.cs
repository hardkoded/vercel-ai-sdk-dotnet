// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>
/// The transformer that calls the tool input callbacks from the model stream. Maps to <c>invoke-tool-callbacks-from-stream.ts</c>.
/// </summary>
internal static class InvokeToolCallbacksFromStream
{
    /// <summary>
    /// Forwards every part of <paramref name="stream"/> and calls <see cref="Tool.OnInputStart"/>, <see cref="Tool.OnInputDelta"/>
    /// and <see cref="Tool.OnInputAvailable"/> as the parts arrive. The context of a tool is validated once per tool call and shared by
    /// its callbacks. A stream retry attempt boundary drops the ongoing tool calls. Maps to <c>invokeToolCallbacksFromStream</c>.
    /// </summary>
    internal static async IAsyncEnumerable<TextStreamPart> Invoke(
        IAsyncEnumerable<TextStreamPart> stream,
        IReadOnlyList<Tool>? tools,
        IReadOnlyList<ModelMessage> stepInputMessages,
        CancellationToken abortSignal,
        IReadOnlyDictionary<string, JsonElement>? toolsContext,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (tools == null)
        {
            await foreach (
                var part in stream.WithCancellation(cancellationToken).ConfigureAwait(false)
            )
            {
                yield return part;
            }

            yield break;
        }

        var ongoingToolCalls = new Dictionary<string, OngoingToolCall>(StringComparer.Ordinal);

        JsonElement? GetValidatedContext(string toolCallId, string toolName)
        {
            ongoingToolCalls.TryGetValue(toolCallId, out var ongoingToolCall);
            if (ongoingToolCall?.ValidatedContexts.TryGetValue(toolName, out var validated) == true)
            {
                return validated;
            }

            JsonElement? context = null;
            if (toolsContext != null && toolsContext.TryGetValue(toolName, out var rawContext))
            {
                context = rawContext;
            }

            var newValidatedContext = ToolContexts.ValidateToolContext(
                toolName,
                context,
                Generation.FindTool(tools, toolName)?.ContextSchema
            );
            if (ongoingToolCall != null)
            {
                ongoingToolCall.ValidatedContexts[toolName] = newValidatedContext;
            }

            return newValidatedContext;
        }

        await foreach (
            var chunk in stream.WithCancellation(cancellationToken).ConfigureAwait(false)
        )
        {
            yield return chunk;

            if (StreamRetryAttemptBoundaryPart.IsStreamRetryAttemptBoundaryPart(chunk))
            {
                ongoingToolCalls = new Dictionary<string, OngoingToolCall>(StringComparer.Ordinal);
                continue;
            }

            switch (chunk)
            {
                case ToolInputStartPart inputStart:
                {
                    ongoingToolCalls[inputStart.Id] = new OngoingToolCall(inputStart.ToolName);

                    var tool = Generation.FindTool(tools, inputStart.ToolName);
                    if (tool?.OnInputStart != null)
                    {
                        await tool.OnInputStart(
                                new ToolExecutionOptions(
                                    inputStart.Id,
                                    stepInputMessages,
                                    abortSignal
                                )
                                {
                                    Context = GetValidatedContext(
                                        inputStart.Id,
                                        inputStart.ToolName
                                    ),
                                }
                            )
                            .ConfigureAwait(false);
                    }

                    break;
                }

                case ToolInputDeltaPart inputDelta:
                {
                    var toolName = ongoingToolCalls.TryGetValue(inputDelta.Id, out var ongoing)
                        ? ongoing.ToolName
                        : null;
                    var tool = toolName == null ? null : Generation.FindTool(tools, toolName);
                    if (tool?.OnInputDelta != null)
                    {
                        await tool.OnInputDelta(
                                new ToolInputDeltaOptions(
                                    inputDelta.Id,
                                    stepInputMessages,
                                    abortSignal,
                                    inputDelta.Delta
                                )
                                {
                                    Context = GetValidatedContext(inputDelta.Id, toolName!),
                                }
                            )
                            .ConfigureAwait(false);
                    }

                    break;
                }

                case ToolCallPart toolCallPart:
                {
                    var toolCall = toolCallPart.ToolCall;
                    var tool = Generation.FindTool(tools, toolCall.ToolName);
                    if (!toolCall.Invalid && tool?.OnInputAvailable != null)
                    {
                        var validatedContext = GetValidatedContext(
                            toolCall.ToolCallId,
                            toolCall.ToolName
                        );
                        ongoingToolCalls.Remove(toolCall.ToolCallId);

                        using var arguments = JsonDocument.Parse(
                            string.IsNullOrWhiteSpace(toolCall.ArgumentsJson)
                                ? "{}"
                                : toolCall.ArgumentsJson
                        );
                        await tool.OnInputAvailable(
                                new ToolInputAvailableOptions(
                                    toolCall.ToolCallId,
                                    stepInputMessages,
                                    abortSignal,
                                    arguments.RootElement.Clone()
                                )
                                {
                                    Context = validatedContext,
                                }
                            )
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        ongoingToolCalls.Remove(toolCall.ToolCallId);
                    }

                    break;
                }
            }
        }
    }

    // .NET-only: upstream's ongoing tool call record, with the validated contexts as values instead of promises.
    private sealed class OngoingToolCall(string toolName)
    {
        internal string ToolName { get; } = toolName;

        internal Dictionary<string, JsonElement?> ValidatedContexts { get; } =
            new(StringComparer.Ordinal);
    }
}
