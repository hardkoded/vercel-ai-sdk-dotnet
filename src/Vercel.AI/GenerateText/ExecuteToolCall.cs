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
    /// <summary>
    /// Executes a single tool call and manages its lifecycle callbacks. Returns null when the tool is missing or has no execute function.
    /// Maps to <c>executeToolCall</c>. <paramref name="now"/> stands for upstream's module-level <c>now</c> and is the same clock
    /// the loop uses. <c>runInTracingChannelSpan</c> is not ported, because the port has no telemetry dispatcher (#1380).
    /// </summary>
    internal static async Task<(ExecutedTool Output, double ToolExecutionMs)?> ExecuteToolCall(
        GeneratedToolCall toolCall,
        IReadOnlyList<Tool>? tools,
        IReadOnlyDictionary<string, JsonElement>? toolsContext,
        string callId,
        IReadOnlyList<ModelMessage> messages,
        CancellationToken abortSignal,
        Func<double> now,
        TimeoutConfiguration? timeout = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Action<ExecutedTool>? onPreliminaryToolResult = null,
        IReadOnlyList<
            Func<ToolExecutionStartEvent, CancellationToken, Task>
        >? onToolExecutionStart = null,
        IReadOnlyList<Func<ToolExecutionEndEvent, CancellationToken, Task>>? onToolExecutionEnd =
            null,
        Func<ExecuteToolInTelemetryContextParams, Task>? executeToolInTelemetryContext = null
    )
    {
        var tool = Generation.FindTool(tools, toolCall.ToolName);

        if (tool == null || !Generation.HasExecutor(tool))
        {
            return null;
        }

        JsonElement? toolContext = null;
        if (toolsContext != null && toolsContext.TryGetValue(toolCall.ToolName, out var rawContext))
        {
            toolContext = rawContext;
        }

        toolContext = ToolContexts.ValidateToolContext(
            toolCall.ToolName,
            toolContext,
            tool.ContextSchema
        );

        var startEvent = new ToolExecutionStartEvent(toolCall, messages)
        {
            CallId = callId,
            ToolContext = toolContext,
        };

        await NotifyAsync(onToolExecutionStart, startEvent, abortSignal).ConfigureAwait(false);

        // Like upstream, the tool timeout starts after onToolExecutionStart and cancels only this tool, not the call.
        var toolTimeoutMs = TimeoutConfiguration.GetToolTimeoutMs(timeout, toolCall.ToolName);
        using var toolTimeout =
            toolTimeoutMs != null
                ? CancellationTokenSource.CreateLinkedTokenSource(abortSignal)
                : null;
        toolTimeout?.CancelAfter(toolTimeoutMs!.Value);
        var toolToken = toolTimeout?.Token ?? abortSignal;

        var toolExecutionMs = 0d;
        ExecutedTool executed;
        try
        {
            using var arguments = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(toolCall.ArgumentsJson) ? "{}" : toolCall.ArgumentsJson
            );
            var input = arguments.RootElement.Clone();
            var output = default(string);

            async Task Execute()
            {
                var startMs = now();
                try
                {
                    output = await RunToolAsync(
                            tool,
                            toolCall,
                            input,
                            messages,
                            toolToken,
                            toolContext,
                            experimentalSandbox,
                            onPreliminaryToolResult
                        )
                        .ConfigureAwait(false);
                }
                finally
                {
                    toolExecutionMs = now() - startMs;
                }
            }

            // Integration wrappers keep nested AI SDK calls associated with this tool execution.
            if (executeToolInTelemetryContext != null)
            {
                await executeToolInTelemetryContext(
                        new ExecuteToolInTelemetryContextParams(
                            callId,
                            toolCall.ToolCallId,
                            toolCall,
                            messages,
                            toolContext,
                            Execute
                        )
                    )
                    .ConfigureAwait(false);
            }
            else
            {
                await Execute().ConfigureAwait(false);
            }

            output = string.IsNullOrEmpty(output) ? "null" : output;
            executed = new ExecutedTool(toolCall.ToolCallId, toolCall.ToolName, output, false)
            {
                InputJson = toolCall.ArgumentsJson,
                Dynamic = toolCall.Dynamic,
                Title = toolCall.Title,
                ToolMetadata = toolCall.ToolMetadata,
                ProviderMetadata = toolCall.ProviderMetadata,
            };
        }
        catch (Exception thrown) when (!abortSignal.IsCancellationRequested)
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
            executed = new ExecutedTool(toolCall.ToolCallId, toolCall.ToolName, error, true)
            {
                InputJson = toolCall.ArgumentsJson,
                Dynamic = toolCall.Dynamic,
                Title = toolCall.Title,
                ToolMetadata = toolCall.ToolMetadata,
                ProviderMetadata = toolCall.ProviderMetadata,
                Error = exception,
            };
        }

        await NotifyAsync(
                onToolExecutionEnd,
                new ToolExecutionEndEvent(
                    toolCall,
                    messages,
                    executed,
                    TimeSpan.FromMilliseconds(toolExecutionMs)
                )
                {
                    CallId = callId,
                    ToolContext = toolContext,
                },
                abortSignal
            )
            .ConfigureAwait(false);

        return (executed, toolExecutionMs);
    }

    // Maps to the `executeTool` call of upstream: every streamed value is a preliminary result and the last one is the final result.
    private static async Task<string?> RunToolAsync(
        Tool tool,
        GeneratedToolCall toolCall,
        JsonElement input,
        IReadOnlyList<ModelMessage> messages,
        CancellationToken toolToken,
        JsonElement? toolContext,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox,
        Action<ExecutedTool>? onPreliminaryToolResult
    )
    {
        var executionOptions = new ToolExecutionOptions(toolCall.ToolCallId, messages, toolToken)
        {
            Context = toolContext,
            ExperimentalSandbox = experimentalSandbox,
        };

        if (tool.ExecuteStream == null)
        {
            return tool.ExecuteWithOptions != null
                ? await tool.ExecuteWithOptions(input, executionOptions).ConfigureAwait(false)
                : await tool.Execute!(input, toolToken).ConfigureAwait(false);
        }

        var last = default(string);
        await foreach (
            var part in Vercel
                .AI.ProviderUtils.ToolExecution.ExecuteStreamAsync(
                    tool.ExecuteStream(input, executionOptions),
                    toolToken
                )
                .ConfigureAwait(false)
        )
        {
            if (part.Type == "preliminary")
            {
                onPreliminaryToolResult?.Invoke(
                    new ExecutedTool(
                        toolCall.ToolCallId,
                        toolCall.ToolName,
                        string.IsNullOrEmpty(part.Output) ? "null" : part.Output,
                        false
                    )
                    {
                        InputJson = toolCall.ArgumentsJson,
                        Dynamic = toolCall.Dynamic,
                        Title = toolCall.Title,
                        ToolMetadata = toolCall.ToolMetadata,
                        ProviderMetadata = toolCall.ProviderMetadata,
                        Preliminary = true,
                    }
                );
            }
            else
            {
                last = part.Output;
            }
        }

        return last;
    }

    // Maps to upstream's `notify`: it calls every listener and ignores an error a listener throws.
    private static async Task NotifyAsync<TEvent>(
        IReadOnlyList<Func<TEvent, CancellationToken, Task>>? callbacks,
        TEvent @event,
        CancellationToken cancellationToken
    )
    {
        if (callbacks == null)
        {
            return;
        }

        foreach (var callback in callbacks)
        {
            await Generation
                .NotifyAsync(() => callback(@event, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

/// <summary>The <see cref="GenerateTextOptions"/> tool execution callbacks as the lists that <c>executeToolCall</c> takes (<c>Arrayable</c>).</summary>
internal static class ToolExecutionCallbacks
{
    internal static IReadOnlyList<Func<ToolExecutionStartEvent, CancellationToken, Task>>? Start(
        GenerateTextOptions options
    ) => options.OnToolExecutionStart == null ? null : [options.OnToolExecutionStart];

    internal static IReadOnlyList<Func<ToolExecutionEndEvent, CancellationToken, Task>>? End(
        GenerateTextOptions options
    ) => options.OnToolExecutionEnd == null ? null : [options.OnToolExecutionEnd];
}

/// <summary>
/// The parameters of the <c>executeToolInTelemetryContext</c> wrapper of <c>executeToolCall</c>.
/// Maps to <c>Partial&lt;ToolExecutionStartEvent&gt; &amp; { callId, toolCallId, execute }</c>.
/// </summary>
internal sealed record ExecuteToolInTelemetryContextParams(
    string CallId,
    string ToolCallId,
    GeneratedToolCall ToolCall,
    IReadOnlyList<ModelMessage> Messages,
    JsonElement? ToolContext,
    Func<Task> Execute
);
