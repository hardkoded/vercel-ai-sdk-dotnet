// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>Tool call execution. Maps to <c>execute-tool-call.ts</c>.</summary>
internal static class ToolCallExecution
{
    /// <summary>Executes a single tool call and manages its lifecycle callbacks. Maps to <c>executeToolCall</c>.</summary>
    internal static async Task<(ExecutedTool Executed, double ElapsedMs)> ExecuteToolCall(
        GeneratedToolCall call,
        GenerateTextOptions options,
        IReadOnlyList<ModelMessage> stepMessages,
        Func<double> now,
        CancellationToken cancellationToken,
        Action<ExecutedTool>? onPreliminaryToolResult = null
    )
    {
        var tool = Generation.FindTool(options, call.ToolName);

        if (tool == null || !Generation.HasExecutor(tool))
        {
            return (
                new ExecutedTool(
                    call.ToolCallId,
                    call.ToolName,
                    "{\"error\":\"Tool has no execute function.\"}",
                    true
                )
                {
                    InputJson = call.ArgumentsJson,
                    Dynamic = call.Dynamic,
                    Title = call.Title,
                    ToolMetadata = call.ToolMetadata,
                    ModelOutputType = "error-text",
                    ModelOutput = "Tool has no execute function.",
                },
                0
            );
        }

        if (options.OnToolExecutionStart != null)
        {
            await Generation
                .NotifyAsync(
                    () =>
                        options.OnToolExecutionStart(
                            new ToolExecutionStartEvent(call, stepMessages),
                            cancellationToken
                        ),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        // Like upstream, the tool timeout starts after onToolExecutionStart and cancels only this tool, not the call.
        var toolTimeoutMs = TimeoutConfiguration.GetToolTimeoutMs(options.Timeout, call.ToolName);
        using var toolTimeout =
            toolTimeoutMs != null
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : null;
        toolTimeout?.CancelAfter(toolTimeoutMs!.Value);
        var toolToken = toolTimeout?.Token ?? cancellationToken;

        var startMs = now();
        ExecutedTool executed;
        try
        {
            using var arguments = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson
            );
            var input = arguments.RootElement.Clone();
            string output;
            if (tool.ExecuteStream != null)
            {
                // Like upstream's executeTool, each streamed value is a preliminary result and the last one is the final result.
                var last = default(string);
                await foreach (
                    var part in Vercel
                        .AI.ProviderUtils.ToolExecution.ExecuteStreamAsync(
                            tool.ExecuteStream(
                                input,
                                new ToolExecutionOptions(call.ToolCallId, stepMessages, toolToken)
                                {
                                    ExperimentalSandbox = options.ExperimentalSandbox,
                                }
                            ),
                            toolToken
                        )
                        .ConfigureAwait(false)
                )
                {
                    if (part.Type == "preliminary")
                    {
                        onPreliminaryToolResult?.Invoke(
                            new ExecutedTool(
                                call.ToolCallId,
                                call.ToolName,
                                string.IsNullOrEmpty(part.Output) ? "null" : part.Output,
                                false
                            )
                            {
                                InputJson = call.ArgumentsJson,
                                Dynamic = call.Dynamic,
                                Title = call.Title,
                                ToolMetadata = call.ToolMetadata,
                                Preliminary = true,
                            }
                        );
                    }
                    else
                    {
                        last = part.Output;
                    }
                }

                output = last!;
            }
            else
            {
                output =
                    tool.ExecuteWithOptions != null
                        ? await tool.ExecuteWithOptions(
                                input,
                                new ToolExecutionOptions(call.ToolCallId, stepMessages, toolToken)
                                {
                                    ExperimentalSandbox = options.ExperimentalSandbox,
                                }
                            )
                            .ConfigureAwait(false)
                        : await tool.Execute!(input, toolToken).ConfigureAwait(false);
            }

            output = string.IsNullOrEmpty(output) ? "null" : output;
            var modelOutput = tool.ToModelOutput?.Invoke(
                new ToolModelOutputCall(call.ToolCallId, input, output)
            );
            executed = new ExecutedTool(call.ToolCallId, call.ToolName, output, false)
            {
                InputJson = call.ArgumentsJson,
                Dynamic = call.Dynamic,
                Title = call.Title,
                ToolMetadata = call.ToolMetadata,
                ModelOutputType = modelOutput is ContentToolModelOutput ? null : modelOutput?.Type,
                ModelOutput =
                    modelOutput is ContentToolModelOutput
                        ? null
                        : Generation.ModelOutputText(modelOutput),
                ModelOutputContent = modelOutput is ContentToolModelOutput content
                    ? Generation.ToUserContent(content.Value)
                    : null,
            };
        }
        catch (Exception thrown) when (!cancellationToken.IsCancellationRequested)
        {
            // A tool that stops on its token reports the timeout, as upstream reports the abort reason of the tool signal.
            var exception =
                thrown is OperationCanceledException cancelled
                && toolTimeout != null
                && cancelled.CancellationToken == toolToken
                && toolToken.IsCancellationRequested
                    ? new TimeoutError("The operation was aborted due to timeout")
                    : thrown;
            var error = JsonSerializer.Serialize(new { error = exception.Message });
            executed = new ExecutedTool(call.ToolCallId, call.ToolName, error, true)
            {
                InputJson = call.ArgumentsJson,
                Dynamic = call.Dynamic,
                Title = call.Title,
                ToolMetadata = call.ToolMetadata,
                ModelOutputType = "error-text",
                ModelOutput = ErrorMessage.ToErrorString(exception),
                Error = exception,
            };
        }

        var elapsedMs = now() - startMs;
        if (options.OnToolExecutionEnd != null)
        {
            var elapsed = TimeSpan.FromMilliseconds(elapsedMs);
            await Generation
                .NotifyAsync(
                    () =>
                        options.OnToolExecutionEnd(
                            new ToolExecutionEndEvent(call, stepMessages, executed, elapsed),
                            cancellationToken
                        ),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        return (executed, elapsedMs);
    }
}
