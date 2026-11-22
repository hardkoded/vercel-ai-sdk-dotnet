// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>
/// The transformer that decides the approval of each tool call as it arrives in the model stream and runs the approved tools
/// at <c>model-call-end</c>. Maps to <c>execute-tools-from-stream.ts</c>.
/// </summary>
internal static class ExecuteToolsFromStream
{
    /// <summary>
    /// Forwards every part of <paramref name="stream"/>. A tool call gets its approval parts as it arrives. At the end of a model call
    /// that allows tool execution, the approved tools run and the stream carries <c>tool-execution-end</c> and the result of
    /// each call as its execution completes, in completion order. A failing execution is an <see cref="ErrorPart"/>.
    /// Maps to <c>executeToolsFromStream</c>. <paramref name="now"/> stands for upstream's module-level <c>now</c>.
    /// <paramref name="instructionCount"/> is .NET-only: the number of leading instruction messages that
    /// <paramref name="messages"/> holds and the approval does not get.
    /// </summary>
    internal static async IAsyncEnumerable<TextStreamPart> Execute(
        IAsyncEnumerable<TextStreamPart> stream,
        GenerateTextOptions options,
        string callId,
        IReadOnlyList<ModelMessage> messages,
        Func<double> now,
        int instructionCount = 0,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var toolCallsToExecute = new List<GeneratedToolCall>();
        var approvalMessages = messages.Skip(instructionCount).ToArray();

        await foreach (
            var chunk in stream.WithCancellation(cancellationToken).ConfigureAwait(false)
        )
        {
            // immediately forward all chunks
            yield return chunk;

            if (StreamRetryAttemptBoundaryPart.IsStreamRetryAttemptBoundaryPart(chunk))
            {
                toolCallsToExecute.Clear();
                continue;
            }

            switch (chunk)
            {
                case ToolCallPart toolCallPart:
                    var toolCall = toolCallPart.ToolCall;
                    var approval = await ResolveToolCallApprovalAsync(
                            options,
                            toolCall,
                            approvalMessages
                        )
                        .ConfigureAwait(false);
                    if (approval.Request != null)
                    {
                        yield return new ToolApprovalRequestPart(approval.Request);
                    }

                    if (approval.Response != null)
                    {
                        yield return new ToolApprovalResponsePart(approval.Response);
                        if (!approval.Response.Approved)
                        {
                            yield return new ToolOutputDeniedPart(
                                toolCall.ToolCallId,
                                toolCall.ToolName
                            );
                        }
                    }

                    // An invalid call or a call to an unknown tool is ignored, e.g. a provider-executed dynamic tool.
                    // An approved call and a call that needs no approval run unless the provider executes it.
                    var tool = toolCall.Invalid
                        ? null
                        : Generation.FindTool(options, toolCall.ToolName);
                    if (
                        tool != null
                        && !approval.Blocked
                        && !toolCall.ProviderExecuted
                        && Generation.HasExecutor(tool)
                    )
                    {
                        toolCallsToExecute.Add(toolCall);
                    }

                    break;
                case ModelCallEndPart modelCallEnd
                    when ToolExecution.IsToolExecutionAllowedFinishReason(
                        modelCallEnd.FinishReason
                    ):
                    if (toolCallsToExecute.Count == 0)
                    {
                        // The setting is validated when the model call ends, as before.
                        ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(
                            options.ToolCallConcurrency
                        );
                        break;
                    }

                    await foreach (
                        var part in RunToolCalls(
                                toolCallsToExecute,
                                options,
                                callId,
                                messages,
                                now,
                                cancellationToken
                            )
                            .ConfigureAwait(false)
                    )
                    {
                        yield return part;
                    }

                    break;
            }
        }
    }

    // .NET-only: a queue and a semaphore take the place of upstream's stream controller. Runs the calls with the tool call
    // concurrency. The parts of the executions are forwarded as they are produced.
    private static async IAsyncEnumerable<TextStreamPart> RunToolCalls(
        IReadOnlyList<GeneratedToolCall> toolCalls,
        GenerateTextOptions options,
        string callId,
        IReadOnlyList<ModelMessage> messages,
        Func<double> now,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var concurrency = ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(
            options.ToolCallConcurrency
        );
        // The executions add their parts, and the reader forwards them in order. The extra permit at the end wakes the reader.
        var parts = new ConcurrentQueue<TextStreamPart>();
        using var signal = new SemaphoreSlim(0);
        var finished = false;
        void Enqueue(TextStreamPart part)
        {
            parts.Enqueue(part);
            signal.Release();
        }

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var run = Task.Run(
            async () =>
            {
                try
                {
                    await ToolCallConcurrencyScheduler
                        .MapWithConcurrency(
                            toolCalls,
                            concurrency,
                            async (toolCall, _) =>
                            {
                                try
                                {
                                    var result = await ToolCallExecution
                                        .ExecuteToolCall(
                                            toolCall,
                                            options.Tools,
                                            options.ToolsContext,
                                            callId,
                                            messages,
                                            cancellation.Token,
                                            now,
                                            options.Timeout,
                                            options.ExperimentalSandbox,
                                            preliminary => Enqueue(new ToolResultPart(preliminary)),
                                            ToolExecutionCallbacks.Start(options),
                                            ToolExecutionCallbacks.End(options)
                                        )
                                        .ConfigureAwait(false);
                                    if (result is { } executed)
                                    {
                                        Enqueue(
                                            new ToolExecutionEndPart(
                                                executed.Output.ToolCallId,
                                                executed.ToolExecutionMs
                                            )
                                        );
                                        Enqueue(
                                            executed.Output.IsError
                                                ? new ToolErrorPart(executed.Output)
                                                : new ToolResultPart(executed.Output)
                                        );
                                    }
                                }
                                catch (Exception error) when (!cancellation.IsCancellationRequested)
                                {
                                    Enqueue(new ErrorPart(error.Message) { Error = error });
                                }

                                return true;
                            },
                            cancellation.Token
                        )
                        .ConfigureAwait(false);
                }
                finally
                {
                    Volatile.Write(ref finished, true);
                    signal.Release();
                }
            },
            CancellationToken.None
        );

        try
        {
            while (true)
            {
                await signal.WaitAsync(cancellationToken).ConfigureAwait(false);
                if (parts.TryDequeue(out var part))
                {
                    yield return part;
                }
                else if (Volatile.Read(ref finished))
                {
                    break;
                }
            }

            // An error of the scheduler, or the cancellation of the caller, surfaces here.
            await run.ConfigureAwait(false);
        }
        finally
        {
            // A consumer that stops early cancels the tools that still run.
            cancellation.Cancel();
            try
            {
                await run.ConfigureAwait(false);
            }
            catch
            {
                // The error is thrown to the reader when it still reads.
            }
        }
    }

    /// <summary>
    /// Resolves the approval of one tool call. A call that is invalid or has no known tool, and a call that does not need approval,
    /// has no request and does not use an approval id. A user approval or an automatic denial blocks the call.
    /// </summary>
    internal static async Task<ToolCallApproval> ResolveToolCallApprovalAsync(
        GenerateTextOptions options,
        GeneratedToolCall toolCall,
        IReadOnlyList<ModelMessage> messages
    )
    {
        if (toolCall.Invalid || Generation.FindTool(options, toolCall.ToolName) == null)
        {
            return ToolCallApproval.None;
        }

        var status = await ToolApprovalResolution
            .ResolveToolApprovalAsync(
                options.Tools,
                toolCall,
                options.ToolApproval,
                messages,
                options.ToolsContext,
                options.RuntimeContext
            )
            .ConfigureAwait(false);
        if (status.Type == ToolApprovalStatus.NotApplicableType)
        {
            return ToolCallApproval.None;
        }

        var approvalId = Generation.NewApprovalId(options);

        // A signature lets a later request execute this tool call by replaying the approval from
        // client-supplied history, so a denied tool call must not get one.
        var signature =
            status.Type == ToolApprovalStatus.DeniedType
                ? null
                : Generation.SignApproval(options, approvalId, toolCall);
        switch (status.Type)
        {
            case ToolApprovalStatus.UserApprovalType:
                return new ToolCallApproval(
                    new GeneratedToolApprovalRequest(approvalId, toolCall.ToolCallId)
                    {
                        ToolCall = toolCall,
                        Reason = status.Reason,
                        Signature = signature,
                    },
                    null,
                    true
                );
            case ToolApprovalStatus.ApprovedType:
            case ToolApprovalStatus.DeniedType:
                var approved = status.Type == ToolApprovalStatus.ApprovedType;
                return new ToolCallApproval(
                    new GeneratedToolApprovalRequest(approvalId, toolCall.ToolCallId)
                    {
                        ToolCall = toolCall,
                        IsAutomatic = true,
                        Signature = signature,
                    },
                    new GeneratedToolApprovalResponse(
                        approvalId,
                        toolCall,
                        approved,
                        status.Reason,
                        toolCall.ProviderExecuted
                    ),
                    !approved
                );
            default:
                return ToolCallApproval.None;
        }
    }

    /// <summary>The end of one tool execution. Maps to <c>ToolExecutionEndStreamPart</c>.</summary>
    internal sealed class ToolExecutionEndPart(string toolCallId, double toolExecutionMs)
        : TextStreamPart("tool-execution-end")
    {
        internal string ToolCallId { get; } = toolCallId;

        internal double ToolExecutionMs { get; } = toolExecutionMs;
    }

    /// <summary>The approval of one tool call. .NET-only: upstream enqueues these parts inside the transformer.</summary>
    internal sealed record ToolCallApproval(
        GeneratedToolApprovalRequest? Request,
        GeneratedToolApprovalResponse? Response,
        bool Blocked
    )
    {
        internal static readonly ToolCallApproval None = new(null, null, false);
    }
}
