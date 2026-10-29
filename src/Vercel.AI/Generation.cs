// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateObject;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI;

internal static class Generation
{
    private const int HardStepCap = 50;

    public static List<ModelMessage> BuildPrompt(GenerateTextOptions options)
    {
        var messages = new List<ModelMessage>();
        messages.AddRange(InstructionMessages(options));

        if (options.Messages != null)
        {
            messages.AddRange(options.Messages);
        }

        if (!string.IsNullOrEmpty(options.Prompt))
        {
            messages.Add(new UserModelMessage(options.Prompt!));
        }

        if (messages.Count == 0)
        {
            throw new AiSdkException("Prompt or messages are required.");
        }

        return messages;
    }

    private static Instructions? ResolveInstructions(GenerateTextOptions options)
    {
        return options.Instructions ?? (options.System != null ? Instructions.From(options.System) : null);
    }

    /// <summary>The leading system messages for the call's instructions. An empty string adds none.</summary>
    private static IReadOnlyList<SystemModelMessage> InstructionMessages(GenerateTextOptions options)
    {
        return InstructionMessages(ResolveInstructions(options));
    }

    private static IReadOnlyList<SystemModelMessage> InstructionMessages(Instructions? instructions)
    {
        if (instructions?.Messages is { } systemMessages)
        {
            return systemMessages;
        }

        return string.IsNullOrEmpty(instructions?.Text) ? Array.Empty<SystemModelMessage>() : new[] { new SystemModelMessage(instructions!.Text!) };
    }

    /// <summary>
    /// Runs <c>prepareStep</c> and applies its instructions and messages overrides, which carry forward to later steps.
    /// The prompt keeps the instructions as leading system messages, so <paramref name="instructionCount"/> marks where the messages start.
    /// </summary>
    private static PrepareStepUpdate? PrepareStep(
        GenerateTextOptions options,
        IReadOnlyList<StepResult> steps,
        ILanguageModel model,
        Instructions? initialInstructions,
        IReadOnlyList<ModelMessage> initialMessages,
        IReadOnlyList<ModelMessage> responseMessages,
        ref Instructions? instructions,
        ref List<ModelMessage> messages,
        ref int instructionCount)
    {
        var update = options.PrepareStep?.Invoke(new PrepareStepContext(steps.Count, steps, messages.Skip(instructionCount).ToArray(), model, instructions, initialInstructions, initialMessages, responseMessages.ToArray()));
        if (update?.Instructions != null || update?.Messages != null)
        {
            instructions = update.Instructions ?? instructions;
            var instructionMessages = InstructionMessages(instructions);
            var stepMessages = update.Messages ?? messages.Skip(instructionCount).ToArray();
            messages = new List<ModelMessage>(instructionMessages.Count + stepMessages.Count);
            messages.AddRange(instructionMessages);
            messages.AddRange(stepMessages);
            instructionCount = instructionMessages.Count;
        }

        return update;
    }

    public static LanguageModelCallOptions CallOptions(GenerateTextOptions options, IReadOnlyList<ModelMessage> prompt)
    {
        return new LanguageModelCallOptions
        {
            Prompt = PrepareProviderPrompt(prompt),
            MaxOutputTokens = options.MaxOutputTokens,
            Temperature = options.Temperature,
            TopP = options.TopP,
            TopK = options.TopK,
            PresencePenalty = options.PresencePenalty,
            FrequencyPenalty = options.FrequencyPenalty,
            StopSequences = options.StopSequences,
            Seed = options.Seed,
            JsonSchema = options.Output?.Schema,
            JsonSchemaName = options.Output?.SchemaName,
            JsonSchemaDescription = options.Output?.SchemaDescription,
            Tools = SelectTools(options),
            ToolChoice = options.ToolChoice ?? ToolChoice.Auto,
            Headers = options.Headers,
            ProviderOptions = options.ProviderOptions,
            Reasoning = options.Reasoning,
        };
    }

    public static async Task<GenerateTextResult> GenerateAsync(
        ILanguageModel model,
        GenerateTextOptions options,
        IAiTelemetry? telemetry,
        CancellationToken callerToken)
    {
        // With a timeout, the model and the tools get a token that also cancels when the timeout expires.
        using var timeouts = CallTimeouts.Create(options.Timeout, null, false, callerToken);
        var cancellationToken = timeouts?.Token ?? callerToken;
        try
        {
            ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(options.ToolCallConcurrency);
            WarnUnsupportedTimeouts(options, model);
            using var scope = telemetry?.Begin("generateText", model.ModelId);
            var messages = BuildPrompt(options);
            var initialInstructions = ResolveInstructions(options);
            var instructions = initialInstructions;
            var instructionCount = InstructionMessages(instructions).Count;
            var initialMessages = messages.Skip(instructionCount).ToArray();
            var responseMessages = new List<ModelMessage>();
            var steps = new List<StepResult>();
            var stop = options.StopWhen ?? StopWhen.IsStepCount(1);
            var current = model;
            while (steps.Count < HardStepCap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                timeouts?.StartStep();
                var update = PrepareStep(options, steps, current, initialInstructions, initialMessages, responseMessages, ref instructions, ref messages, ref instructionCount);
                if (update?.Model != null)
                {
                    current = update.Model;
                }

                // The model gets a snapshot, because the step appends its own response to `messages` afterwards.
                var callOptions = CallOptions(options, messages.ToArray());
                callOptions.AbortSignal = timeouts?.Signal;
                if (update?.ToolChoice != null)
                {
                    callOptions.ToolChoice = update.ToolChoice;
                }

                callOptions.ProviderOptions = MergeProviderOptions(callOptions.ProviderOptions, update?.ProviderOptions);

                var now = options.NowMs ?? DefaultNowMs;
                var stepStartMs = now();
                var modelResponse = await current.DoGenerateAsync(callOptions, cancellationToken).ConfigureAwait(false);
                var responseTimeMs = now() - stepStartMs;
                var generated = await ParseToolCallsAsync(modelResponse, options, messages, instructions, instructionCount, callOptions.ToolChoice?.Type == "none", cancellationToken).ConfigureAwait(false);
                if (options.OnLanguageModelCallEnd != null)
                {
                    await NotifyAsync(
                        () => options.OnLanguageModelCallEnd(
                            new LanguageModelCallEndEvent(
                                current.Provider,
                                generated.ResponseModelId ?? current.ModelId,
                                generated.FinishReason,
                                generated.Usage,
                                generated.Content,
                                generated.ResponseId,
                                generated.ProviderMetadata,
                                TimeSpan.FromMilliseconds(responseTimeMs)),
                            cancellationToken),
                        cancellationToken).ConfigureAwait(false);
                }

                EnforceToolChoice(callOptions.ToolChoice, generated, current);
                var step = await FinishStepAsync(generated, options, messages, responseMessages, current, cancellationToken, new StepTiming(now, stepStartMs, responseTimeMs, Array.Empty<double>()), LanguageModelRequestMetadata.Empty, GeneratedResponseInfo(generated, current), ParseResponseBody(generated.RawResponse)).ConfigureAwait(false);
                steps.Add(step);
                if (options.OnStepEnd != null)
                {
                    await options.OnStepEnd(step, cancellationToken).ConfigureAwait(false);
                }

                if (!ToolExecution.IsToolExecutionAllowedFinishReason(generated.FinishReason) || !HasClientToolCall(step) || HasToolCallWithoutExecute(step, options) || stop.ShouldStop(steps))
                {
                    break;
                }
            }

            timeouts?.EndStep();
            var result = await ToResultAsync(steps, responseMessages, options, cancellationToken).ConfigureAwait(false);
            if (telemetry != null && scope != null)
            {
                telemetry.OnUsage(scope, result.Usage);
                telemetry.OnFinish(scope, result.FinishReason);
            }

            if (options.OnFinish != null)
            {
                await options.OnFinish(result, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception exception) when (exception is not ApiUserAbortException)
        {
            // An expired timeout aborts the call. Like upstream, the call fails with the timeout reason.
            var error = exception is OperationCanceledException or ApiTimeoutException && !callerToken.IsCancellationRequested && timeouts?.Expired is { } expired ? expired : exception;
            if (options.OnError != null)
            {
                await options.OnError(error, callerToken).ConfigureAwait(false);
            }

            if (!ReferenceEquals(error, exception))
            {
                throw error;
            }

            throw;
        }
    }

    // generateText does not stream, so a firstChunkMs or chunkMs timeout has no effect.
    private static void WarnUnsupportedTimeouts(GenerateTextOptions options, ILanguageModel model)
    {
        var warnings = new List<ModelWarning>();
        if (options.Timeout?.FirstChunkMs != null)
        {
            warnings.Add(new UnsupportedWarning("timeout.firstChunkMs", "The firstChunkMs timeout is only supported by streaming functions."));
        }

        if (options.Timeout?.ChunkMs != null)
        {
            warnings.Add(new UnsupportedWarning("timeout.chunkMs", "The chunkMs timeout is only supported by streaming functions."));
        }

        if (warnings.Count == 0)
        {
            return;
        }

        LogWarnings.Log(new LogWarningsOptions(warnings, model.Provider, model.ModelId));
    }

    public static StreamTextResult Stream(
        ILanguageModel model,
        StreamTextOptions options,
        IAiTelemetry? telemetry,
        CancellationToken cancellationToken)
    {
        ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(options.ToolCallConcurrency);
        var buffer = new PartBuffer();
        var textSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishSource = new TaskCompletionSource<FinishReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        var usageSource = new TaskCompletionSource<LanguageModelUsage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stepsSource = new TaskCompletionSource<IReadOnlyList<StepResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var abort = new StreamAbort();
        var timeouts = CallTimeouts.Create(options.Timeout, options.AbortSignal, true, cancellationToken);
        var signal = timeouts?.Signal ?? options.AbortSignal;
        if (signal != null)
        {
            Action onSignal = () => abort.Fire(signal.Reason as Exception ?? new OperationCanceledException(signal.Reason?.ToString()), textSource, finishSource, usageSource, stepsSource);
            signal.AddAbortHandler(onSignal);
            abort.SignalCleanup = () => signal.RemoveAbortHandler(onSignal);
        }

        if (cancellationToken.CanBeCanceled)
        {
            abort.Registration = cancellationToken.Register(() => abort.Fire(new OperationCanceledException(cancellationToken), textSource, finishSource, usageSource, stepsSource));
        }

        buffer.OnConsumerStop = abort.Stop;
        _ = Task.Run(() => ProduceAsync(model, options, telemetry, buffer, abort, timeouts, textSource, finishSource, usageSource, stepsSource));
        return new StreamTextResult(buffer, textSource.Task, finishSource.Task, usageSource.Task, stepsSource.Task);
    }

    private static async Task ProduceAsync(
        ILanguageModel model,
        StreamTextOptions options,
        IAiTelemetry? telemetry,
        PartBuffer buffer,
        StreamAbort abort,
        CallTimeouts? timeouts,
        TaskCompletionSource<string> textSource,
        TaskCompletionSource<FinishReason> finishSource,
        TaskCompletionSource<LanguageModelUsage> usageSource,
        TaskCompletionSource<IReadOnlyList<StepResult>> stepsSource)
    {
        var steps = new List<StepResult>();
        var cancellationToken = abort.Token;
        try
        {
            using var scope = telemetry?.Begin("streamText", model.ModelId);
            var messages = BuildPrompt(options);
            var initialInstructions = ResolveInstructions(options);
            var instructions = initialInstructions;
            var instructionCount = InstructionMessages(instructions).Count;
            var initialMessages = messages.Skip(instructionCount).ToArray();
            var responseMessages = new List<ModelMessage>();
            var stop = options.StopWhen ?? StopWhen.IsStepCount(1);
            var current = model;
            buffer.Add(new StartPart());
            while (steps.Count < HardStepCap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                timeouts?.StartStep();
                var update = PrepareStep(options, steps, current, initialInstructions, initialMessages, responseMessages, ref instructions, ref messages, ref instructionCount);
                if (update?.Model != null)
                {
                    current = update.Model;
                }

                // The model gets a snapshot, because the step appends its own response to `messages` afterwards.
                var stepMessages = messages.ToArray();
                var callOptions = CallOptions(options, stepMessages);
                callOptions.AbortSignal = timeouts?.Signal ?? options.AbortSignal;
                if (update?.ToolChoice != null)
                {
                    callOptions.ToolChoice = update.ToolChoice;
                }

                callOptions.ProviderOptions = MergeProviderOptions(callOptions.ProviderOptions, update?.ProviderOptions);

                var now = options.NowMs ?? DefaultNowMs;
                var stepStartMs = now();
                double ElapsedMs() => now() - stepStartMs;
                var stepStarted = false;
                var stepResponseId = Vercel.AI.ProviderUtils.JsonValues.GenerateId("aitxt-");
                var stepResponseTimestamp = DateTimeOffset.UtcNow;
                var stepResponseModelId = current.ModelId;
                var outputChunkTimes = new List<double>();
                var emittedResultIds = new HashSet<string>(StringComparer.Ordinal);
                var text = new StringBuilder();
                var textBlocks = new List<TextBlock>();
                var reasoning = new StringBuilder();
                var reasoningMetadata = new List<JsonElement?>();
                var toolCalls = new List<GeneratedToolCall>();
                var toolContent = new List<GeneratedContent>();
                var sources = new List<GeneratedSource>();
                var documentSources = new List<GeneratedDocumentSource>();
                FinishReason? finish = null;
                string? rawFinish = null;
                JsonElement? providerMetadata = null;
                IReadOnlyList<CallWarning>? warnings = null;
                var receivedOutput = false;
                var usage = LanguageModelUsage.Null;
                callOptions.IncludeRawChunks = options.Include?.RawChunks == true;
                // Tool choice none rejects tool calls when the model call finishes. Withhold tool parts so that consumers
                // cannot act on them before the rejection, and skip the callbacks that would repair or refine them.
                var withholdToolParts = callOptions.ToolChoice?.Type == "none";
                var streamResult = await current.DoStreamAsync(callOptions, cancellationToken).ConfigureAwait(false);
                var stepRequest = new LanguageModelRequestMetadata
                {
                    Body = options.Include?.RequestBody == true ? streamResult.Request?.Body : null,
                    Messages = options.Include?.RequestMessages == true ? stepMessages : null,
                };
                IAsyncEnumerator<LanguageModelStreamPart> enumerator;
                try
                {
                    enumerator = streamResult.Stream.GetAsyncEnumerator(cancellationToken);
                }
                catch
                {
                    await streamResult.DisposeAsync().ConfigureAwait(false);
                    throw;
                }

                var disposal = abort.Track(new StreamResultDisposal(enumerator, streamResult));
                try
                {
                    timeouts?.StartFirstChunk();
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // A model stream can ignore the token, so race each read against the abort.
                        var next = enumerator.MoveNextAsync().AsTask();
                        if (await Task.WhenAny(next, abort.Signal).ConfigureAwait(false) != next)
                        {
                            _ = next.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        if (!await next.ConfigureAwait(false))
                        {
                            break;
                        }

                        var part = enumerator.Current;
                        if (timeouts != null)
                        {
                            ObserveChunkTimeout(timeouts, part);
                        }

                        if (IsOutputChunk(part))
                        {
                            receivedOutput = true;
                        }

                        if (!stepStarted && part is not StreamStartStreamPart)
                        {
                            stepStarted = true;
                            buffer.Add(new StartStepPart(null, stepRequest));
                        }

                        switch (part)
                        {
                            case TextStartStreamPart textStart:
                                TextBlock.Start(textBlocks, textStart.Id).ProviderMetadata = textStart.ProviderMetadata;
                                break;
                            case TextDeltaStreamPart delta:
                                text.Append(delta.Delta);
                                TextBlock.Find(textBlocks, delta.Id).Text.Append(delta.Delta);
                                outputChunkTimes.Add(ElapsedMs());
                                buffer.Add(new TextDeltaPart(delta.Delta));
                                break;
                            case TextEndStreamPart textEnd:
                                var ended = TextBlock.Find(textBlocks, textEnd.Id);
                                ended.Citations = textEnd.Citations;
                                ended.ProviderMetadata = textEnd.ProviderMetadata ?? ended.ProviderMetadata;
                                break;
                            case DocumentSourceStreamPart documentSource:
                                documentSources.Add(new GeneratedDocumentSource(
                                    documentSource.Id,
                                    documentSource.MediaType,
                                    documentSource.Title,
                                    documentSource.Filename,
                                    documentSource.ProviderMetadata));
                                break;
                            case ReasoningStartStreamPart reasoningStart:
                                reasoningMetadata.Add(reasoningStart.ProviderMetadata);
                                break;
                            case ReasoningEndStreamPart reasoningEnd:
                                reasoningMetadata.Add(reasoningEnd.ProviderMetadata);
                                break;
                            case ReasoningDeltaStreamPart reasoningDelta:
                                reasoning.Append(reasoningDelta.Delta);
                                reasoningMetadata.Add(reasoningDelta.ProviderMetadata);
                                outputChunkTimes.Add(ElapsedMs());
                                buffer.Add(new ReasoningDeltaPart(reasoningDelta.Delta));
                                break;
                            case ToolCallStreamPart toolCall:
                                var call = ParseProviderExecutedInput(
                                    options,
                                    await ParseToolCallAsync(
                                        new GeneratedToolCall(toolCall.ToolCallId, toolCall.ToolName, toolCall.ArgumentsJson, toolCall.ProviderMetadata) { ProviderExecuted = toolCall.ProviderExecuted, Dynamic = toolCall.Dynamic },
                                        options,
                                        stepMessages,
                                        instructions,
                                        instructionCount,
                                        withholdToolParts,
                                        cancellationToken).ConfigureAwait(false));
                                toolCalls.Add(call);
                                toolContent.Add(call);
                                if (withholdToolParts)
                                {
                                    break;
                                }

                                outputChunkTimes.Add(ElapsedMs());
                                buffer.Add(new ToolCallPart(call));
                                if (call.Invalid && !call.ProviderExecuted)
                                {
                                    emittedResultIds.Add(call.ToolCallId);
                                    buffer.Add(new ToolErrorPart(InvalidToolResult(call)));
                                }

                                break;
                            case ToolApprovalRequestStreamPart when withholdToolParts:
                                break;
                            case ToolApprovalRequestStreamPart approvalRequest:
                                var resolvedRequest = ResolveApprovalRequest(new GeneratedToolApprovalRequest(approvalRequest.ApprovalId, approvalRequest.ToolCallId), toolContent);
                                toolContent.Add(resolvedRequest);
                                buffer.Add(new ToolApprovalRequestPart(resolvedRequest));
                                break;
                            case ToolResultStreamPart streamedResult:
                                var providerResult = new GeneratedToolResult(streamedResult.ToolCallId, streamedResult.ToolName, streamedResult.Result.GetRawText(), streamedResult.IsError) { ProviderMetadata = streamedResult.ProviderMetadata };
                                toolContent.Add(providerResult);
                                var providerExecuted = ProviderExecutedResult(providerResult, toolCalls);
                                emittedResultIds.Add(providerResult.ToolCallId);
                                if (withholdToolParts)
                                {
                                    break;
                                }

                                buffer.Add(providerResult.IsError ? new ToolErrorPart(providerExecuted) : new ToolResultPart(providerExecuted));
                                break;
                            case ToolInputStartStreamPart or ToolInputDeltaStreamPart or ToolInputEndStreamPart when withholdToolParts:
                                break;
                            case ToolInputStartStreamPart inputStart:
                                outputChunkTimes.Add(ElapsedMs());
                                var inputStartTool = FindTool(options, inputStart.ToolName);
                                buffer.Add(new ToolInputStartPart(
                                    inputStart.Id,
                                    inputStart.ToolName,
                                    inputStart.ProviderMetadata,
                                    inputStart.ProviderExecuted,
                                    inputStartTool?.IsDynamic ?? inputStart.Dynamic ?? false,
                                    inputStart.InputEncoding,
                                    inputStartTool?.Title)
                                {
                                    ToolMetadata = inputStartTool?.Metadata,
                                });
                                break;
                            case ToolInputDeltaStreamPart inputDelta:
                                outputChunkTimes.Add(ElapsedMs());
                                buffer.Add(new ToolInputDeltaPart(inputDelta.Id, inputDelta.Delta, inputDelta.ProviderMetadata));
                                break;
                            case ToolInputEndStreamPart inputEnd:
                                buffer.Add(new ToolInputEndPart(inputEnd.Id, inputEnd.ProviderMetadata));
                                break;
                            case SourceStreamPart source:
                                var generatedSource = new GeneratedSource(source.Id, source.Url, source.Title, source.ProviderMetadata);
                                sources.Add(generatedSource);
                                buffer.Add(new SourcePart(generatedSource));
                                break;
                            case FinishStreamPart done:
                                finish = done.FinishReason;
                                rawFinish = done.RawFinishReason;
                                usage = done.Usage;
                                providerMetadata = done.ProviderMetadata;
                                break;
                            case RawStreamPart raw:
                                if (options.Include?.RawChunks == true)
                                {
                                    using var rawDocument = JsonDocument.Parse(raw.RawJson.Length == 0 ? "null" : raw.RawJson);
                                    buffer.Add(new RawPart(rawDocument.RootElement.Clone()));
                                }

                                break;
                            case ErrorStreamPart error:
                                buffer.Add(new ErrorPart(error.Message));
                                throw new AiSdkException(error.Message);
                            case ResponseMetadataStreamPart responseMetadata:
                                stepResponseId = responseMetadata.Id ?? stepResponseId;
                                stepResponseTimestamp = responseMetadata.Timestamp ?? stepResponseTimestamp;
                                stepResponseModelId = responseMetadata.ModelId ?? stepResponseModelId;
                                break;
                            case StreamStartStreamPart start:
                                warnings = start.Warnings;
                                if (!stepStarted)
                                {
                                    stepStarted = true;
                                    buffer.Add(new StartStepPart(start.Warnings, stepRequest));
                                }

                                break;
                        }
                    }
                }
                finally
                {
                    timeouts?.ClearModelOutput();
                    await abort.DisposeAsync(disposal).ConfigureAwait(false);
                }

                var responseTimeMs = ElapsedMs();
                if (!stepStarted)
                {
                    buffer.Add(new StartStepPart(null, stepRequest));
                }

                // A stream that ends without a finish part and without output is rejected instead of recorded as an empty step.
                // An incomplete stream with partial output keeps the partial result.
                if (finish == null && !receivedOutput)
                {
                    const string noOutputMessage = "No output generated. The model stream ended without a finish chunk.";
                    buffer.Add(new ErrorPart(noOutputMessage));
                    throw new NoOutputGeneratedException(noOutputMessage);
                }

                var generated = new LanguageModelGenerateResult(
                    BuildContent(textBlocks, reasoning.ToString(), MergeProviderMetadata(reasoningMetadata), toolContent, sources, documentSources),
                    finish ?? FinishReason.Other,
                    usage,
                    rawFinish,
                    warnings,
                    providerMetadata: providerMetadata);
                if (options.OnLanguageModelCallEnd != null)
                {
                    await NotifyAsync(
                        () => options.OnLanguageModelCallEnd(
                            new LanguageModelCallEndEvent(
                                current.Provider,
                                stepResponseModelId,
                                generated.FinishReason,
                                generated.Usage,
                                generated.Content,
                                stepResponseId,
                                generated.ProviderMetadata,
                                TimeSpan.FromMilliseconds(responseTimeMs)),
                            cancellationToken),
                        cancellationToken).ConfigureAwait(false);
                }

                EnforceToolChoice(callOptions.ToolChoice, generated, current);
                var stepResponse = new LanguageModelResponseMetadata(stepResponseId, stepResponseTimestamp, stepResponseModelId, streamResult.Response?.Headers);
                var step = await FinishStepAsync(generated, options, messages, responseMessages, current, cancellationToken, new StepTiming(now, stepStartMs, responseTimeMs, outputChunkTimes), stepRequest, stepResponse, null).ConfigureAwait(false);
                foreach (var toolResult in step.ToolResults)
                {
                    if (emittedResultIds.Contains(toolResult.ToolCallId))
                    {
                        continue;
                    }

                    buffer.Add(toolResult.IsError ? new ToolErrorPart(toolResult) : new ToolResultPart(toolResult));
                }

                buffer.Add(new StepFinishPart(step, stepResponse, rawFinish));
                steps.Add(step);
                if (options.OnStepEnd != null)
                {
                    await options.OnStepEnd(step, cancellationToken).ConfigureAwait(false);
                }

                if (!ToolExecution.IsToolExecutionAllowedFinishReason(generated.FinishReason) || !HasClientToolCall(step) || HasToolCallWithoutExecute(step, options) || stop.ShouldStop(steps))
                {
                    break;
                }
            }

            timeouts?.EndStep();
            var result = await ToResultAsync(steps, responseMessages, options, cancellationToken).ConfigureAwait(false);
            if (telemetry != null && scope != null)
            {
                telemetry.OnUsage(scope, result.Usage);
                telemetry.OnFinish(scope, result.FinishReason);
            }

            if (!abort.TryFinish())
            {
                throw abort.Error!;
            }

            buffer.Add(new FinishPart(result.FinishReason, result.Usage));
            buffer.Complete();
            textSource.TrySetResult(result.Text);
            finishSource.TrySetResult(result.FinishReason);
            usageSource.TrySetResult(result.Usage);
            stepsSource.TrySetResult(result.Steps);
            if (options.OnFinish != null)
            {
                await options.OnFinish(result, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            abort.TryFinish();
            if (abort.Stopped)
            {
                // The consumer stopped reading. Nobody waits for the stream, so there is nothing to report.
                textSource.TrySetCanceled();
                finishSource.TrySetCanceled();
                usageSource.TrySetCanceled();
                stepsSource.TrySetCanceled();
                buffer.Complete();
                return;
            }

            if (abort.Error is { } abortError)
            {
                await FinishAbortAsync(options, buffer, steps, abortError).ConfigureAwait(false);
                return;
            }

            buffer.Fail(exception);
            textSource.TrySetException(exception);
            finishSource.TrySetException(exception);
            usageSource.TrySetException(exception);
            stepsSource.TrySetException(exception);
            if (options.OnError != null)
            {
                try
                {
                    await options.OnError(exception, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception callback)
                {
                    buffer.Fail(callback);
                }
            }
        }
        finally
        {
            timeouts?.Dispose();
            abort.Registration.Dispose();
            abort.SignalCleanup?.Invoke();
            abort.Dispose();
        }
    }

    // Chunks that carry model output. Tells an empty incomplete stream from an incomplete stream with a partial result.
    private static bool IsOutputChunk(LanguageModelStreamPart part)
    {
        return part switch
        {
            TextDeltaStreamPart { Delta.Length: > 0 } => true,
            ReasoningDeltaStreamPart { Delta.Length: > 0 } => true,
            ToolInputDeltaStreamPart { Delta.Length: > 0 } => true,
            ToolCallStreamPart => true,
            _ => false,
        };
    }

    // A chunk of model output starts the chunkMs timeout again. A chunk without output leaves it as it is.
    // The timeout ends with the model response, so a finish part ends it before the next step or a slow consumer can trip it.
    private static void ObserveChunkTimeout(CallTimeouts timeouts, LanguageModelStreamPart part)
    {
        switch (part)
        {
            case TextDeltaStreamPart { Delta.Length: > 0 }:
            case ReasoningDeltaStreamPart { Delta.Length: > 0 }:
            case ToolInputDeltaStreamPart { Delta.Length: > 0 }:
            case ToolCallStreamPart:
                timeouts.OnOutputChunk();
                break;
            case FinishStreamPart:
            case ErrorStreamPart:
                timeouts.ClearModelOutput();
                break;
        }
    }

    // The result tasks are already faulted when the abort fires. This reports the abort, then closes the stream.
    // Like upstream, the stream ends only after OnAbort returns, so a reader that sees the end knows OnAbort ran.
    private static async Task FinishAbortAsync(StreamTextOptions options, PartBuffer buffer, List<StepResult> steps, Exception error)
    {
        Exception? callbackError = null;
        if (options.OnAbort != null)
        {
            try
            {
                await options.OnAbort(new StreamAbortContext(steps.ToArray(), error), CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception callback)
            {
                callbackError = callback;
            }
        }

        buffer.Add(new AbortPart(error is JsError ? error.ToString() : error.Message));
        if (callbackError != null)
        {
            buffer.Fail(callbackError);
        }
        else
        {
            buffer.Complete();
        }
    }

    private static List<GeneratedContent> BuildContent(
        List<TextBlock> textBlocks,
        string reasoning,
        JsonElement? reasoningMetadata,
        List<GeneratedContent> toolContent,
        List<GeneratedSource> sources,
        List<GeneratedDocumentSource> documentSources)
    {
        var content = new List<GeneratedContent>();
        if (reasoning.Length > 0)
        {
            content.Add(new GeneratedReasoning(reasoning, reasoningMetadata));
        }

        foreach (var block in textBlocks)
        {
            if (block.Text.Length > 0 || (block.Citations?.Count ?? 0) > 0)
            {
                content.Add(new GeneratedText(block.Text.ToString(), block.Citations, block.ProviderMetadata));
            }
        }

        content.AddRange(toolContent);
        content.AddRange(sources);
        content.AddRange(documentSources);
        return content;
    }

    // One streamed text block. Citations arrive with its text-end part.
    private sealed class TextBlock
    {
        private TextBlock(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public StringBuilder Text { get; } = new();

        public IReadOnlyList<Citation>? Citations { get; set; }

        public JsonElement? ProviderMetadata { get; set; }

        public static TextBlock Start(List<TextBlock> blocks, string id)
        {
            var block = new TextBlock(id);
            blocks.Add(block);
            return block;
        }

        // Deltas and ends belong to the latest block with their id. Providers that skip text-start get one here.
        public static TextBlock Find(List<TextBlock> blocks, string id)
        {
            for (var i = blocks.Count - 1; i >= 0; i--)
            {
                if (blocks[i].Id == id)
                {
                    return blocks[i];
                }
            }

            return Start(blocks, id);
        }
    }

    // Later entries override earlier ones, property by property, inside each provider's object.
    private static JsonElement? MergeProviderMetadata(List<JsonElement?> metadata)
    {
        var merged = new JsonObject();
        foreach (var item in metadata)
        {
            if (item is not { ValueKind: JsonValueKind.Object } root)
            {
                continue;
            }

            foreach (var provider in root.EnumerateObject())
            {
                if (provider.Value.ValueKind != JsonValueKind.Object)
                {
                    merged[provider.Name] = JsonNode.Parse(provider.Value.GetRawText());
                    continue;
                }

                if (merged[provider.Name] is not JsonObject target)
                {
                    target = new JsonObject();
                    merged[provider.Name] = target;
                }

                foreach (var property in provider.Value.EnumerateObject())
                {
                    target[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                }
            }
        }

        if (merged.Count == 0)
        {
            return null;
        }

        using var document = JsonDocument.Parse(merged.ToJsonString());
        return document.RootElement.Clone();
    }

    // Deep merge, as upstream mergeObjects: nested objects merge, other values from the step replace the call's.
    private static IReadOnlyDictionary<string, JsonElement>? MergeProviderOptions(
        IReadOnlyDictionary<string, JsonElement>? options,
        IReadOnlyDictionary<string, JsonElement>? step)
    {
        if (step == null || options == null)
        {
            return step ?? options;
        }

        var merged = new Dictionary<string, JsonElement>();
        foreach (var pair in options)
        {
            merged[pair.Key] = pair.Value;
        }

        foreach (var pair in step)
        {
            merged[pair.Key] = merged.TryGetValue(pair.Key, out var current) ? MergeJson(current, pair.Value) : pair.Value;
        }

        return merged;
    }

    private static JsonElement MergeJson(JsonElement current, JsonElement step)
    {
        if (current.ValueKind != JsonValueKind.Object || step.ValueKind != JsonValueKind.Object)
        {
            return step;
        }

        var merged = new Dictionary<string, JsonElement>();
        foreach (var property in current.EnumerateObject())
        {
            merged[property.Name] = property.Value;
        }

        foreach (var property in step.EnumerateObject())
        {
            merged[property.Name] = merged.TryGetValue(property.Name, out var value) ? MergeJson(value, property.Value) : property.Value;
        }

        return JsonSerializer.SerializeToElement(merged);
    }

    private static ExecutedTool ProviderExecutedResult(GeneratedToolResult result, IReadOnlyList<GeneratedToolCall> toolCalls)
    {
        string? input = null;
        GeneratedToolCall? matched = null;
        foreach (var call in toolCalls)
        {
            if (string.Equals(call.ToolCallId, result.ToolCallId, StringComparison.Ordinal))
            {
                input = call.ArgumentsJson;
                matched = call;
            }
        }

        return new ExecutedTool(result.ToolCallId, result.ToolName, result.ResultJson, result.IsError)
        {
            ProviderExecuted = true,
            ProviderMetadata = result.ProviderMetadata,
            InputJson = input,
            Dynamic = matched?.Dynamic ?? false,
            Title = matched?.Title,
            ToolMetadata = matched?.ToolMetadata,
        };
    }

    private static IReadOnlyDictionary<string, JsonElement>? ToProviderOptions(JsonElement? metadata)
    {
        if (metadata is not { ValueKind: JsonValueKind.Object } root)
        {
            return null;
        }

        var options = new Dictionary<string, JsonElement>();
        foreach (var provider in root.EnumerateObject())
        {
            options[provider.Name] = provider.Value.Clone();
        }

        return options;
    }

    private static async Task<StepResult> FinishStepAsync(
        LanguageModelGenerateResult generated,
        GenerateTextOptions options,
        List<ModelMessage> messages,
        List<ModelMessage> responseMessages,
        ILanguageModel model,
        CancellationToken cancellationToken,
        StepTiming timing,
        LanguageModelRequestMetadata stepRequest,
        LanguageModelResponseMetadata responseInfo,
        JsonElement? responseBody)
    {
        var toolExecutionMs = new Dictionary<string, double>(StringComparer.Ordinal);
        var toolCalls = new List<GeneratedToolCall>();
        var content = new List<GeneratedContent>();
        var sources = new List<GeneratedSource>();
        var files = new List<GeneratedFile>();
        var reasoningParts = new List<string>();
        var reasoningMetadata = new List<JsonElement?>();
        var assistantParts = new List<AssistantContentPart>();
        foreach (var part in generated.Content)
        {
            var resolved = part switch
            {
                GeneratedToolApprovalRequest request => ResolveApprovalRequest(request, generated.Content),
                GeneratedToolCall toolCall => ParseProviderExecutedInput(options, toolCall),
                _ => part,
            };
            content.Add(resolved);
            switch (resolved)
            {
                case GeneratedToolApprovalRequest approvalRequest:
                    assistantParts.Add(new AssistantToolApprovalRequestPart(approvalRequest.ApprovalId, approvalRequest.ToolCallId));
                    break;
                case GeneratedToolCall call:
                    toolCalls.Add(call);
                    assistantParts.Add(new AssistantToolCallPart(call));
                    break;
                case GeneratedText generatedText:
                    assistantParts.Add(new AssistantTextPart(generatedText.Text, ToProviderOptions(generatedText.ProviderMetadata)));
                    break;
                case GeneratedToolResult providerResult:
                    // The provider ran the tool, so the next step replays the result in the same assistant message.
                    var output = new ToolModelMessage(providerResult.ToolCallId, providerResult.ToolName, providerResult.ResultJson, providerResult.IsError);
                    assistantParts.Add(new AssistantToolResultPart(providerResult.ToolCallId, providerResult.ToolName, output, ToProviderOptions(providerResult.ProviderMetadata)));
                    break;
                case GeneratedSource source:
                    sources.Add(source);
                    break;
                case GeneratedReasoning generatedReasoning:
                    reasoningParts.Add(generatedReasoning.Text);
                    reasoningMetadata.Add(generatedReasoning.ProviderMetadata);
                    assistantParts.Add(new AssistantReasoningPart(generatedReasoning.Text, ToProviderOptions(generatedReasoning.ProviderMetadata)));
                    break;
                case GeneratedFile file:
                    files.Add(file);
                    break;
                case GeneratedFileUrl remote:
                    DownloadUrls.ValidateDownloadUrl(remote.Url);
                    break;
            }
        }

        var reasoning = reasoningParts.Count == 0 ? null : string.Concat(reasoningParts);
        if (reasoning != null && reasoning.Length == 0)
        {
            reasoning = null;
        }
        // Upstream lists the provider's own results beside the client's; an error result is left out.
        var toolResults = new List<ExecutedTool>();
        foreach (var part in generated.Content)
        {
            if (part is GeneratedToolResult { IsError: false } providerResult)
            {
                toolResults.Add(ProviderExecutedResult(providerResult, toolCalls));
            }
        }

        var stepInputMessages = messages.ToArray();
        var stepResponseStart = responseMessages.Count;
        var assistantMessage = new AssistantModelMessage(generated.Text, toolCalls, reasoning, ToProviderOptions(MergeProviderMetadata(reasoningMetadata)), assistantParts);
        messages.Add(assistantMessage);
        responseMessages.Add(assistantMessage);
        if (ToolExecution.IsToolExecutionAllowedFinishReason(generated.FinishReason))
        {
            var toolMessages = new List<ToolModelMessage>();
            var callsToExecute = new List<GeneratedToolCall>();
            foreach (var call in toolCalls)
            {
                // A call that failed to parse is answered with its error, and nothing runs. A provider reports its own error.
                if (call.Invalid)
                {
                    if (!call.ProviderExecuted)
                    {
                        var invalidResult = InvalidToolResult(call);
                        toolResults.Add(invalidResult);
                        toolMessages.Add(new ToolModelMessage(invalidResult.ToolCallId, invalidResult.ToolName, invalidResult.ModelOutput ?? invalidResult.OutputJson, invalidResult.IsError, invalidResult.ModelOutputType));
                    }

                    continue;
                }

                var tool = FindTool(options, call.ToolName);

                // A tool without execute is for the caller to run: the step has the call and no result.
                if (call.ProviderExecuted || IsAwaitingApproval(tool, options) || (tool != null && !HasExecutor(tool)))
                {
                    continue;
                }

                callsToExecute.Add(call);
            }

            var executions = await ToolCallConcurrencyScheduler.MapWithConcurrency(
                callsToExecute,
                ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(options.ToolCallConcurrency),
                async (call, _) =>
                {
                    var (executed, executionMs) = await ExecuteToolAsync(call, options, stepInputMessages, timing.Now, cancellationToken).ConfigureAwait(false);
                    return (Executed: executed, ElapsedMs: executionMs);
                },
                cancellationToken).ConfigureAwait(false);
            foreach (var (executed, elapsedMs) in executions)
            {
                toolExecutionMs[executed.ToolCallId] = elapsedMs;

                toolResults.Add(executed);
                toolMessages.Add(new ToolModelMessage(executed.ToolCallId, executed.ToolName, executed.ModelOutput ?? executed.OutputJson, executed.IsError, executed.ModelOutputType));
            }

            // Upstream sends the results of one step as a single tool message.
            if (toolMessages.Count > 0)
            {
                var toolMessage = toolMessages.Count == 1 ? toolMessages[0] : new ToolModelMessage(toolMessages);
                messages.Add(toolMessage);
                responseMessages.Add(toolMessage);
            }
        }

        // The results of the tools this loop ran follow the model's own content, as upstream lists them.
        foreach (var executed in toolResults)
        {
            if (!executed.ProviderExecuted)
            {
                content.Add(new GeneratedToolResult(executed.ToolCallId, executed.ToolName, executed.IsError && executed.ModelOutput != null ? JsonSerializer.Serialize(executed.ModelOutput) : executed.OutputJson, executed.IsError) { Error = executed.Error, Dynamic = executed.Dynamic, ToolMetadata = executed.ToolMetadata });
            }
        }

        var response = new StepResponse(
            responseInfo.Id,
            responseInfo.Timestamp,
            responseInfo.ModelId,
            responseInfo.Headers,
            responseMessages.Skip(stepResponseStart).ToArray(),
            responseBody);
        var performance = StepPerformanceCalculator.Create(timing.Now() - timing.StartMs, timing.ResponseTimeMs, timing.OutputChunkTimesMs, toolExecutionMs, generated.Usage);
        return new StepResult(generated.Text, reasoning, toolCalls, toolResults, generated.FinishReason, generated.Usage, sources, generated.ProviderMetadata, generated.Warnings, files, model.Provider, model.ModelId, content, performance, stepRequest, response, generated.RawFinishReason) { StructuredOutputText = generated.StructuredOutputText };
    }

    // Parses the tool calls of a model response with the call's tools, repair function, and input refinements. A call that
    // cannot be parsed stays in the content, flagged invalid, so the model can see its error.
    private static async Task<LanguageModelGenerateResult> ParseToolCallsAsync(
        LanguageModelGenerateResult generated,
        GenerateTextOptions options,
        IReadOnlyList<ModelMessage> messages,
        Instructions? instructions,
        int instructionCount,
        bool toolChoiceNone,
        CancellationToken cancellationToken)
    {
        var parses = new List<Task<GeneratedToolCall>>();
        foreach (var part in generated.Content)
        {
            if (part is GeneratedToolCall call)
            {
                parses.Add(ParseToolCallAsync(call, options, messages, instructions, instructionCount, toolChoiceNone, cancellationToken));
            }
        }

        if (parses.Count == 0)
        {
            return generated;
        }

        var parsed = await Task.WhenAll(parses).ConfigureAwait(false);
        var content = new List<GeneratedContent>(generated.Content.Count);
        var next = 0;
        foreach (var part in generated.Content)
        {
            content.Add(part is GeneratedToolCall ? parsed[next++] : part);
        }

        return generated.WithContent(content);
    }

    private static async Task<GeneratedToolCall> ParseToolCallAsync(
        GeneratedToolCall call,
        GenerateTextOptions options,
        IReadOnlyList<ModelMessage> messages,
        Instructions? instructions,
        int instructionCount,
        bool toolChoiceNone,
        CancellationToken cancellationToken)
    {
        // The messages are the history without the leading instructions, which the repair function gets apart.
        var history = messages.Skip(instructionCount).ToArray();

        var tools = ActiveTools(options);

#pragma warning disable CS0618
        var repairToolCall = options.RepairToolCall ?? options.ExperimentalRepairToolCall;
#pragma warning restore CS0618
        // Tool calls are rejected with tool choice none, so user callbacks must not process them.
        if (toolChoiceNone)
        {
            repairToolCall = null;
        }

        var parsed = await ToolCallParser.ParseToolCallAsync(
            new ParseToolCallOptions(call)
            {
                Tools = tools,
                RepairToolCall = repairToolCall,
                RefineToolInput = toolChoiceNone ? null : options.RefineToolInput,
                Messages = history,
                Instructions = instructions,
            },
            cancellationToken).ConfigureAwait(false);
        return new GeneratedToolCall(parsed.ToolCallId, parsed.ToolName, parsed.Input.GetRawText(), parsed.ProviderMetadata)
        {
            ProviderExecuted = parsed.ProviderExecuted,
            Dynamic = parsed.Dynamic,
            Invalid = parsed.Invalid,
            Error = parsed.Error,
            Title = parsed.Title,
            ToolMetadata = parsed.ToolMetadata,
        };
    }

    private static bool Contains(IReadOnlyList<Tool>? tools, string name)
    {
        if (tools != null)
        {
            foreach (var tool in tools)
            {
                if (string.Equals(tool.Name, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // The tools the model was offered: the call's tools narrowed to its active tools.
    private static IReadOnlyList<Tool>? ActiveTools(GenerateTextOptions options)
    {
        if (options.Tools == null || options.ActiveTools == null)
        {
            return options.Tools;
        }

        var tools = new List<Tool>();
        foreach (var tool in options.Tools)
        {
            if (Contains(options.ActiveTools, tool.Name))
            {
                tools.Add(tool);
            }
        }

        return tools;
    }

    // The tool result of a call that failed to parse. The model reads the error the way upstream sends it: "Name: message".
    private static ExecutedTool InvalidToolResult(GeneratedToolCall call)
    {
        var message = ErrorMessage.ToErrorString(call.Error!);
        return new ExecutedTool(call.ToolCallId, call.ToolName, JsonSerializer.Serialize(new { error = message }), true)
        {
            InputJson = call.ArgumentsJson,
            Dynamic = true,
            Title = call.Title,
            ToolMetadata = call.ToolMetadata,
            ModelOutputType = "error-text",
            ModelOutput = message,
        };
    }

    private static double DefaultNowMs()
    {
        return Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency;
    }

    // What FinishStepAsync needs to compute the performance of one step.
    private sealed class StepTiming(Func<double> now, double startMs, double responseTimeMs, IReadOnlyList<double> outputChunkTimesMs)
    {
        public Func<double> Now { get; } = now;

        public double StartMs { get; } = startMs;

        public double ResponseTimeMs { get; } = responseTimeMs;

        public IReadOnlyList<double> OutputChunkTimesMs { get; } = outputChunkTimesMs;
    }

    // A provider-executed tool asks for approval by tool call id; the step carries the call itself, as upstream does.
    private static GeneratedToolApprovalRequest ResolveApprovalRequest(GeneratedToolApprovalRequest request, IReadOnlyList<GeneratedContent> content)
    {
        foreach (var part in content)
        {
            if (part is GeneratedToolCall call && string.Equals(call.ToolCallId, request.ToolCallId, StringComparison.Ordinal))
            {
                return new GeneratedToolApprovalRequest(request.ApprovalId, request.ToolCallId) { ToolCall = call };
            }
        }

        throw new AiSdkException("Tool call \"" + request.ToolCallId + "\" not found for approval request \"" + request.ApprovalId + "\".");
    }

    // The model sees no approval requests, and only the approval responses of provider-executed calls.
    // A denied one is followed by an execution-denied result that names the approval for the provider.
    private static IReadOnlyList<ModelMessage> PrepareProviderPrompt(IReadOnlyList<ModelMessage> prompt)
    {
        var needsWork = false;
        foreach (var message in prompt)
        {
            if (message is ToolApprovalResponseModelMessage || (message is AssistantModelMessage assistant && ContainsApprovalRequest(assistant)))
            {
                needsWork = true;
                break;
            }
        }

        if (!needsWork)
        {
            return prompt;
        }

        var toolNames = new Dictionary<string, string>(StringComparer.Ordinal);
        var callIds = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var message in prompt)
        {
            if (message is not AssistantModelMessage assistant)
            {
                continue;
            }

            foreach (var part in assistant.Parts)
            {
                if (part is AssistantToolCallPart call)
                {
                    toolNames[call.ToolCall.ToolCallId] = call.ToolCall.ToolName;
                }
                else if (part is AssistantToolApprovalRequestPart request)
                {
                    callIds[request.ApprovalId] = request.ToolCallId;
                }
            }
        }

        var prepared = new List<ModelMessage>();
        foreach (var message in prompt)
        {
            switch (message)
            {
                case AssistantModelMessage assistant when ContainsApprovalRequest(assistant):
                    var parts = new List<AssistantContentPart>();
                    foreach (var part in assistant.Parts)
                    {
                        if (part is not AssistantToolApprovalRequestPart)
                        {
                            parts.Add(part);
                        }
                    }

                    prepared.Add(new AssistantModelMessage(assistant.Text, assistant.ToolCalls, assistant.Reasoning, assistant.ReasoningProviderOptions, parts));
                    break;
                case ToolApprovalResponseModelMessage response:
                    if (!response.ProviderExecuted)
                    {
                        break;
                    }

                    if (response.Approved || !callIds.TryGetValue(response.ApprovalId, out var toolCallId))
                    {
                        prepared.Add(response);
                        break;
                    }

                    using (var approvalOptions = JsonDocument.Parse(JsonSerializer.Serialize(new { openai = new { approvalId = response.ApprovalId } })))
                    {
                        var denied = new ToolModelMessage(toolCallId, toolNames.TryGetValue(toolCallId, out var toolName) ? toolName : string.Empty, response.Reason, false, "execution-denied")
                        {
                            OutputProviderOptions = approvalOptions.RootElement.Clone(),
                        };
                        prepared.Add(new ToolApprovalResponseModelMessage(response.ApprovalId, false, response.Reason, true) { DeniedToolResult = denied });
                    }

                    break;
                default:
                    prepared.Add(message);
                    break;
            }
        }

        return prepared;
    }

    private static bool ContainsApprovalRequest(AssistantModelMessage assistant)
    {
        foreach (var part in assistant.Parts)
        {
            if (part is AssistantToolApprovalRequestPart)
            {
                return true;
            }
        }

        return false;
    }

    private static void EnforceToolChoice(ToolChoice? toolChoice, LanguageModelGenerateResult generated, ILanguageModel model)
    {
        if (toolChoice is null || toolChoice.Type == "auto")
        {
            return;
        }

        if (toolChoice.Type == "none")
        {
            if (generated.Content.Any(part => part is GeneratedToolCall))
            {
                throw new ToolChoiceViolationException(
                    "Model response contained a tool call even though tool choice was none.",
                    toolChoice,
                    generated.FinishReason,
                    model.Provider,
                    model.ModelId,
                    generated.Content);
            }

            return;
        }

        if (generated.FinishReason == FinishReason.Interrupted)
        {
            return;
        }

        var calls = new List<GeneratedToolCall>();
        foreach (var part in generated.Content)
        {
            if (part is GeneratedToolCall call)
            {
                calls.Add(call);
            }
        }

        if (toolChoice.Type == "required")
        {
            if (calls.Count == 0)
            {
                throw new ToolChoiceViolationException(
                    "Model response did not contain a tool call even though tool choice was required.",
                    toolChoice,
                    generated.FinishReason,
                    model.Provider,
                    model.ModelId,
                    generated.Content);
            }

            return;
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            foreach (var call in calls)
            {
                if (string.Equals(call.ToolName, named.ToolName, StringComparison.Ordinal))
                {
                    return;
                }
            }

            throw new ToolChoiceViolationException(
                "Model response did not contain a call to the required tool '" + named.ToolName + "'.",
                toolChoice,
                generated.FinishReason,
                model.Provider,
                model.ModelId,
                generated.Content);
        }
    }

    // Upstream parses the input of a tool call with the tool's input schema. Only a provider-defined tool declared with a
    // schema is parsed here, so the input of any other call is kept as the model sent it.
    private static GeneratedToolCall ParseProviderExecutedInput(GenerateTextOptions options, GeneratedToolCall call)
    {
        if (!call.ProviderExecuted || call.Invalid)
        {
            return call;
        }

        var tool = FindTool(options, call.ToolName);
        if (tool is not { IsProviderDefined: true } || tool.InputSchema.ValueKind != JsonValueKind.Object)
        {
            return call;
        }

        JsonNode? input;
        try
        {
            input = JsonNode.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson);
        }
        catch (JsonException)
        {
            return call;
        }

        var parsed = JsonSchemaProjection.Project(tool.InputSchema, input);
        return new GeneratedToolCall(call.ToolCallId, call.ToolName, parsed?.ToJsonString() ?? "null", call.ProviderMetadata) { ProviderExecuted = true, Dynamic = call.Dynamic, Title = call.Title, ToolMetadata = call.ToolMetadata };
    }

    private static Tool? FindTool(GenerateTextOptions options, string name)
    {
        if (options.Tools != null)
        {
            foreach (var candidate in options.Tools)
            {
                if (string.Equals(candidate.Name, name, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    // A tool that needs approval, with nobody to ask, leaves its call for the caller.
    private static bool IsAwaitingApproval(Tool? tool, GenerateTextOptions options)
    {
        return tool != null && tool.NeedsApproval && options.ApproveTool == null;
    }

    // A provider-executed call is already done, so only the calls the caller must run keep the loop going.
    private static bool HasClientToolCall(StepResult step)
    {
        foreach (var call in step.ToolCalls)
        {
            if (!call.ProviderExecuted)
            {
                return true;
            }
        }

        return false;
    }

    // A tool without an execute function, or one awaiting approval, leaves its call for the caller, so the loop ends without asking the stop condition.
    private static bool HasExecutor(Tool? tool) => tool != null && (tool.Execute != null || tool.ExecuteWithOptions != null);

    private static bool HasToolCallWithoutExecute(StepResult step, GenerateTextOptions options)
    {
        foreach (var call in step.ToolCalls)
        {
            if (call.ProviderExecuted || call.Invalid)
            {
                continue;
            }

            var tool = FindTool(options, call.ToolName);
            if ((tool != null && !HasExecutor(tool)) || IsAwaitingApproval(tool, options))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<(ExecutedTool Executed, double ElapsedMs)> ExecuteToolAsync(GeneratedToolCall call, GenerateTextOptions options, IReadOnlyList<ModelMessage> stepMessages, Func<double> now, CancellationToken cancellationToken)
    {
        var tool = FindTool(options, call.ToolName);

        if (tool == null || !HasExecutor(tool))
        {
            return (new ExecutedTool(call.ToolCallId, call.ToolName, "{\"error\":\"Tool has no execute function.\"}", true) { InputJson = call.ArgumentsJson, Dynamic = call.Dynamic, Title = call.Title, ToolMetadata = call.ToolMetadata, ModelOutputType = "error-text", ModelOutput = "Tool has no execute function." }, 0);
        }

        if (options.ApproveTool != null)
        {
            var approved = await options.ApproveTool(new ToolApprovalRequest(call.ToolCallId, call.ToolName, call.ArgumentsJson), cancellationToken).ConfigureAwait(false);
            if (!approved)
            {
                return (new ExecutedTool(call.ToolCallId, call.ToolName, "{\"error\":\"Tool call was not approved.\"}", true) { InputJson = call.ArgumentsJson, Dynamic = call.Dynamic, Title = call.Title, ToolMetadata = call.ToolMetadata, ModelOutputType = "execution-denied", ModelOutput = "Tool call was not approved." }, 0);
            }
        }

        if (options.OnToolExecutionStart != null)
        {
            await NotifyAsync(() => options.OnToolExecutionStart(new ToolExecutionStartEvent(call, stepMessages), cancellationToken), cancellationToken).ConfigureAwait(false);
        }

        // Like upstream, the tool timeout starts after onToolExecutionStart and cancels only this tool, not the call.
        var toolTimeoutMs = TimeoutConfiguration.GetToolTimeoutMs(options.Timeout, call.ToolName);
        using var toolTimeout = toolTimeoutMs != null ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : null;
        toolTimeout?.CancelAfter(toolTimeoutMs!.Value);
        var toolToken = toolTimeout?.Token ?? cancellationToken;

        var startMs = now();
        ExecutedTool executed;
        try
        {
            using var arguments = JsonDocument.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson);
            var input = arguments.RootElement.Clone();
            var output = tool.ExecuteWithOptions != null
                ? await tool.ExecuteWithOptions(input, new ToolExecutionOptions(call.ToolCallId, stepMessages, toolToken)).ConfigureAwait(false)
                : await tool.Execute!(input, toolToken).ConfigureAwait(false);
            output = string.IsNullOrEmpty(output) ? "null" : output;
            var modelOutput = tool.ToModelOutput?.Invoke(new ToolModelOutputCall(call.ToolCallId, input, output));
            executed = new ExecutedTool(call.ToolCallId, call.ToolName, output, false) { InputJson = call.ArgumentsJson, Dynamic = call.Dynamic, Title = call.Title, ToolMetadata = call.ToolMetadata, ModelOutputType = modelOutput?.Type, ModelOutput = ModelOutputText(modelOutput) };
        }
        catch (Exception thrown) when (!cancellationToken.IsCancellationRequested)
        {
            // A tool that stops on its token reports the timeout, as upstream reports the abort reason of the tool signal.
            var exception = thrown is OperationCanceledException cancelled && toolTimeout != null && cancelled.CancellationToken == toolToken && toolToken.IsCancellationRequested ? new TimeoutError("The operation was aborted due to timeout") : thrown;
            var error = JsonSerializer.Serialize(new { error = exception.Message });
            executed = new ExecutedTool(call.ToolCallId, call.ToolName, error, true) { InputJson = call.ArgumentsJson, Dynamic = call.Dynamic, Title = call.Title, ToolMetadata = call.ToolMetadata, ModelOutputType = "error-text", ModelOutput = ErrorMessage.ToErrorString(exception), Error = exception };
        }

        var elapsedMs = now() - startMs;
        if (options.OnToolExecutionEnd != null)
        {
            var elapsed = TimeSpan.FromMilliseconds(elapsedMs);
            await NotifyAsync(() => options.OnToolExecutionEnd(new ToolExecutionEndEvent(call, stepMessages, executed, elapsed), cancellationToken), cancellationToken).ConfigureAwait(false);
        }

        return (executed, elapsedMs);
    }

    // The model-facing result of a tool's toModelOutput. Only text and error-text results travel as plain text.
    private static string? ModelOutputText(ToolModelOutput? output)
    {
        return output switch
        {
            TextToolModelOutput text => text.Value,
            ErrorTextToolModelOutput errorText => errorText.Value,
            JsonToolModelOutput json => json.Value.GetRawText(),
            ErrorJsonToolModelOutput errorJson => errorJson.Value.GetRawText(),
            ContentToolModelOutput content => JsonSerializer.Serialize(content.Value.Select(part => new { type = part.Type, text = part.Text })),
            _ => null,
        };
    }

    // Upstream's `notify` ignores an error a callback throws, so a faulty listener cannot break generation.
    private static async Task NotifyAsync(Func<Task> callback, CancellationToken cancellationToken)
    {
        try
        {
            await callback().ConfigureAwait(false);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static IReadOnlyList<LanguageModelTool>? SelectTools(GenerateTextOptions options)
    {
        if (options.Tools == null || options.Tools.Count == 0)
        {
            return null;
        }

        var tools = new List<LanguageModelTool>();
        foreach (var tool in options.Tools)
        {
            if (options.ActiveTools != null && !Contains(options.ActiveTools, tool.Name))
            {
                continue;
            }

            tools.Add(tool.IsProviderDefined
                ? LanguageModelTool.ProviderDefined(tool.ProviderToolId!, tool.Name, tool.ProviderToolArgs)
                : new LanguageModelTool(tool.Name, tool.Description, tool.InputSchema, tool.Strict) { ProviderOptions = tool.ProviderOptions });
        }

        return tools;
    }

    private static bool Contains(IReadOnlyList<string> names, string name)
    {
        foreach (var candidate in names)
        {
            if (string.Equals(candidate, name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static LanguageModelResponseMetadata GeneratedResponseInfo(LanguageModelGenerateResult generated, ILanguageModel model)
    {
        return new LanguageModelResponseMetadata(
            generated.ResponseId ?? Vercel.AI.ProviderUtils.JsonValues.GenerateId("aitxt-"),
            generated.ResponseTimestamp ?? DateTimeOffset.UtcNow,
            generated.ResponseModelId ?? model.ModelId,
            generated.ResponseHeaders);
    }

    // The raw response body, as a JSON value when the provider sent JSON and as a string otherwise.
    private static JsonElement? ParseResponseBody(string? rawResponse)
    {
        if (rawResponse == null)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(rawResponse);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return JsonSerializer.SerializeToElement(rawResponse);
        }
    }

    private static async Task<GenerateTextResult> ToResultAsync(
        List<StepResult> steps,
        List<ModelMessage> responseMessages,
        GenerateTextOptions options,
        CancellationToken cancellationToken)
    {
        var last = steps.Count == 0
            ? new StepResult(string.Empty, null, Array.Empty<GeneratedToolCall>(), Array.Empty<ExecutedTool>(), FinishReason.Other, LanguageModelUsage.Empty, Array.Empty<GeneratedSource>())
            : steps[steps.Count - 1];
        var usage = LanguageModelUsage.Null;
        var sources = new List<GeneratedSource>();
        foreach (var step in steps)
        {
            usage = LanguageModelUsage.Add(usage, step.Usage);
            sources.AddRange(step.Sources);
        }

        // A provider-selected structured output text replaces the step text for JSON output only. An empty selection
        // is explicit, so it is parsed (and rejected) instead of falling back to the step text.
        var outputText = options.Output?.Schema != null ? last.StructuredOutputText ?? last.Text : last.Text;
        JsonElement? output = null;
        if (options.Output?.Schema is { } outputSchema)
        {
            if (options.GenerateObjectMode && !last.Content.OfType<GeneratedText>().Any())
            {
                throw new NoObjectGeneratedException(
                    "No object generated: the model did not return a response.",
                    null,
                    null,
                    last.Response,
                    last.Usage,
                    last.FinishReason);
            }

            if (!options.GenerateObjectMode
                && last.FinishReason != FinishReason.Stop
                && (last.FinishReason == FinishReason.ToolCalls || last.Text.Length == 0))
            {
                return new GenerateTextResult(last.Text, last.ReasoningText, steps, last.FinishReason, usage, null, sources, last.ProviderMetadata) { ResponseMessages = responseMessages.ToArray() };
            }

            try
            {
                output = ProviderUtils.JsonParsing.Parse(outputText, JsonNode.Parse(outputSchema.GetRawText()));
            }
            catch (Exception error) when (error is ProviderUtils.JsonParseException or ProviderUtils.TypeValidationException)
            {
                var initialError = new NoObjectGeneratedException(
                    error is ProviderUtils.TypeValidationException
                        ? "No object generated: response did not match schema."
                        : "No object generated: could not parse the response.",
                    error,
                    outputText,
                    last.Response,
                    last.Usage,
                    last.FinishReason);
                if (options.RepairOutput == null)
                {
                    throw initialError;
                }

                var repairedText = await options.RepairOutput(outputText, error, cancellationToken).ConfigureAwait(false);
                if (repairedText == null)
                {
                    throw initialError;
                }

                try
                {
                    output = ProviderUtils.JsonParsing.Parse(repairedText, JsonNode.Parse(outputSchema.GetRawText()));
                }
                catch (Exception repairedError) when (repairedError is ProviderUtils.JsonParseException or ProviderUtils.TypeValidationException)
                {
                    throw new NoObjectGeneratedException(
                        repairedError is ProviderUtils.TypeValidationException
                            ? "No object generated: response did not match schema."
                            : "No object generated: could not parse the response.",
                        repairedError,
                        repairedText,
                        last.Response,
                        last.Usage,
                        last.FinishReason);
                }
            }
        }

        return new GenerateTextResult(last.Text, last.ReasoningText, steps, last.FinishReason, usage, output, sources, last.ProviderMetadata) { ResponseMessages = responseMessages.ToArray() };
    }
}

// Settles a streamText call exactly once: as aborted, or as finished. Whichever comes first wins.
internal sealed class StreamAbort
{
    private static readonly object Done = new();
    private readonly CancellationTokenSource _cancellation = new();
    private readonly TaskCompletionSource<bool> _signal = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private object? _outcome;
    private EnumeratorDisposal? _disposal;

    public CancellationToken Token => _cancellation.Token;

    public Task Signal => _signal.Task;

    public CancellationTokenRegistration Registration { get; set; }

    public Action? SignalCleanup { get; set; }

    public Exception? Error => _outcome as Exception;

    public bool Stopped { get; private set; }

    public void Dispose() => _cancellation.Dispose();

    // The consumer stopped reading. Cancels the provider call and the tool loop, like upstream cancelling the source reader.
    public void Stop()
    {
        if (Interlocked.CompareExchange(ref _outcome, new OperationCanceledException("The stream consumer stopped reading."), null) != null)
        {
            return;
        }

        Stopped = true;
        try
        {
            _cancellation.Cancel();
        }
        catch (Exception)
        {
            // A token callback threw. The call is stopped either way.
        }
        finally
        {
            Volatile.Read(ref _disposal)?.Abandon();
            _signal.TrySetResult(true);
        }
    }

    public bool TryFinish() => Interlocked.CompareExchange(ref _outcome, Done, null) == null;

    public EnumeratorDisposal Track(IAsyncDisposable enumerator)
    {
        var disposal = new EnumeratorDisposal(enumerator);
        Volatile.Write(ref _disposal, disposal);
        return disposal;
    }

    // Rejects the results first, so no callback or provider cleanup can delay them.
    public void Fire(
        Exception error,
        TaskCompletionSource<string> text,
        TaskCompletionSource<FinishReason> finishReason,
        TaskCompletionSource<LanguageModelUsage> usage,
        TaskCompletionSource<IReadOnlyList<StepResult>> steps)
    {
        if (Interlocked.CompareExchange(ref _outcome, error, null) != null)
        {
            return;
        }

        text.TrySetException(error);
        finishReason.TrySetException(error);
        usage.TrySetException(error);
        steps.TrySetException(error);
        try
        {
            _cancellation.Cancel();
        }
        catch (Exception exception) when (exception is AggregateException or ObjectDisposedException)
        {
            // A token callback threw, or the call already ended. The call is aborted either way.
        }
        finally
        {
            Volatile.Read(ref _disposal)?.Abandon();
            _signal.TrySetResult(true);
        }
    }

    // Waits for the provider to dispose, unless an abort arrives first. A model stream may never finish disposing.
    public async Task DisposeAsync(EnumeratorDisposal disposal)
    {
        var dispose = disposal.DisposeAsync();
        if (await Task.WhenAny(dispose, Signal).ConfigureAwait(false) == dispose)
        {
            await dispose.ConfigureAwait(false);
        }
        else
        {
            disposal.Abandon();
        }
    }
}

// Disposes the enumerator, then the stream result that owns the provider response. The result is released even when the enumerator throws.
internal sealed class StreamResultDisposal : IAsyncDisposable
{
    private readonly IAsyncDisposable _enumerator;
    private readonly IAsyncDisposable _result;

    public StreamResultDisposal(IAsyncDisposable enumerator, IAsyncDisposable result)
    {
        _enumerator = enumerator;
        _result = result;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _enumerator.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            await _result.DisposeAsync().ConfigureAwait(false);
        }
    }
}

internal sealed class EnumeratorDisposal
{
    private readonly IAsyncDisposable _enumerator;
    private readonly object _gate = new();
    private Task? _task;

    public EnumeratorDisposal(IAsyncDisposable enumerator)
    {
        _enumerator = enumerator;
    }

    public Task DisposeAsync()
    {
        lock (_gate)
        {
            return _task ??= _enumerator.DisposeAsync().AsTask();
        }
    }

    // Starts the disposal without waiting. Nobody observes the outcome, so a failure is dropped.
    public void Abandon()
    {
        try
        {
            _ = DisposeAsync().ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        }
        catch (Exception)
        {
            // The enumerator threw while starting to dispose. The call is already aborted.
        }
    }
}

internal sealed class PartBuffer
{
    private readonly List<TextStreamPart> _parts = new();
    private readonly object _gate = new();
    private readonly SemaphoreSlim _signal = new(0);
    private Exception? _error;
    private bool _completed;
    private int _readers;

    // Called when the last active reader stops before the stream ends.
    public Action? OnConsumerStop { get; set; }

    public void Add(TextStreamPart part)
    {
        lock (_gate)
        {
            _parts.Add(part);
        }

        _signal.Release();
    }

    public void Complete()
    {
        lock (_gate)
        {
            _completed = true;
        }

        _signal.Release();
    }

    public void Fail(Exception exception)
    {
        lock (_gate)
        {
            _error ??= exception;
            _completed = true;
        }

        _signal.Release();
    }

    public async IAsyncEnumerable<TextStreamPart> Read([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var index = 0;
        var finished = false;
        Interlocked.Increment(ref _readers);
        try
        {
            while (true)
            {
                TextStreamPart? part = null;
                var ready = false;
                Exception? error = null;
                var done = false;
                lock (_gate)
                {
                    if (index < _parts.Count)
                    {
                        part = _parts[index++];
                        ready = true;
                    }
                    else if (_completed)
                    {
                        done = true;
                        error = _error;
                    }
                }

                if (ready)
                {
                    yield return part!;
                    continue;
                }

                if (done)
                {
                    finished = true;
                    if (error != null)
                    {
                        throw error;
                    }

                    yield break;
                }

                await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            if (Interlocked.Decrement(ref _readers) == 0 && !finished)
            {
                OnConsumerStop?.Invoke();
            }
        }
    }
}

/// <summary>Result of <c>streamText</c>. Enumerating <see cref="Stream"/> or awaiting <see cref="Text"/> consumes the same buffered events.</summary>
public sealed class StreamTextResult
{
    private readonly PartBuffer _buffer;

    internal StreamTextResult(
        PartBuffer buffer,
        Task<string> text,
        Task<FinishReason> finishReason,
        Task<LanguageModelUsage> usage,
        Task<IReadOnlyList<StepResult>> steps)
    {
        _buffer = buffer;
        Text = text;
        FinishReason = finishReason;
        Usage = usage;
        Steps = steps;
        Request = FromFinalStep(steps, step => step.Request);
        Response = FromFinalStep(steps, step => step.Response);

        // Steps carries the failure. These tasks fail with it, so a caller that never awaits them leaves no unobserved exception.
        _ = Request.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        _ = Response.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
    }

    private static async Task<T> FromFinalStep<T>(Task<IReadOnlyList<StepResult>> steps, Func<StepResult, T> select)
    {
        var completed = await steps.ConfigureAwait(false);
        return select(completed[completed.Count - 1]);
    }

    /// <summary>Text deltas only.</summary>
    public async IAsyncEnumerable<string> TextStream([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var part in _buffer.Read(cancellationToken).ConfigureAwait(false))
        {
            if (part is TextDeltaPart delta)
            {
                yield return delta.Text;
            }
        }
    }

    /// <summary>Every stream part, including tool calls, tool results, and finish.</summary>
    public IAsyncEnumerable<TextStreamPart> Stream(CancellationToken cancellationToken = default)
    {
        return _buffer.Read(cancellationToken);
    }

    /// <summary>Full text from the last step. Completes when the stream finishes.</summary>
    public Task<string> Text { get; }

    /// <summary>Finish reason. Completes when the stream finishes.</summary>
    public Task<FinishReason> FinishReason { get; }

    /// <summary>Total usage. Completes when the stream finishes.</summary>
    public Task<LanguageModelUsage> Usage { get; }

    /// <summary>Completed steps. Completes when the stream finishes.</summary>
    public Task<IReadOnlyList<StepResult>> Steps { get; }

    /// <summary>
    /// Additional request information of the last step. <see cref="LanguageModelRequestMetadata.Body"/> and
    /// <see cref="LanguageModelRequestMetadata.Messages"/> are set only when <see cref="StreamTextOptions.Include"/> asks for them.
    /// Completes when the stream finishes. Maps to <c>request</c>.
    /// </summary>
    public Task<LanguageModelRequestMetadata> Request { get; }

    /// <summary>Additional response information of the last step, with the messages that step added. Completes when the stream finishes. Maps to <c>response</c>.</summary>
    public Task<StepResponse> Response { get; }
}
