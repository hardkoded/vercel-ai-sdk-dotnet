// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI;

// Maps to the eventProcessor TransformStream of streamText. It reads the parts that the stream transforms produced, forwards them to
// the consumers, and records the steps, the results, and the callbacks from them. The step loop reads the recorded steps.
internal sealed class StreamTextEventProcessor
{
    private readonly StreamTextOptions _options;
    private readonly StreamAbort _abort;
    private readonly CancellationToken _abortToken;
    private readonly SteeringRun? _steeringRun;
    private readonly TaskCompletionSource<string> _textSource;
    private readonly TaskCompletionSource<FinishReason> _finishSource;
    private readonly TaskCompletionSource<LanguageModelUsage> _usageSource;
    private readonly TaskCompletionSource<IReadOnlyList<StepResult>> _stepsSource;
    private readonly TaskCompletionSource<JsonElement?> _outputSource;
    private readonly StreamTextErrorHandling _errors;
    private readonly bool _collectContent;
    private readonly object _gate = new();
    private readonly List<StepResult> _steps = [];
    private readonly Dictionary<int, TaskCompletionSource<bool>> _stepWaiters = [];
    private readonly TaskCompletionSource<bool> _ended = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    // the data of the step that is being recorded:
    private List<object> _contentOrder = [];
    private Dictionary<string, TextRecord> _activeText = [];
    private Dictionary<string, ReasoningRecord> _activeReasoning = [];
    private List<ExecutedTool> _executedResults = [];
    private LanguageModelRequestMetadata _recordedRequest = LanguageModelRequestMetadata.Empty;
    private IReadOnlyList<CallWarning> _recordedWarnings = [];
    private FinishReason? _recordedFinishReason;
    private LanguageModelUsage? _recordedTotalUsage;
    private NoOutputGeneratedException? _recordedNoOutputError;

    // the step that has started but not ended and its last error that a stream retry did not handle, reported to onStepError
    // when the stream ends before the step produces a result:
    private GenerateTextStepStartEvent? _openStep;
    private Exception? _openStepError;

    public StreamTextEventProcessor(
        StreamTextOptions options,
        StreamAbort abort,
        SteeringRun? steeringRun,
        TaskCompletionSource<string> textSource,
        TaskCompletionSource<FinishReason> finishSource,
        TaskCompletionSource<LanguageModelUsage> usageSource,
        TaskCompletionSource<IReadOnlyList<StepResult>> stepsSource,
        TaskCompletionSource<JsonElement?> outputSource,
        StreamTextErrorHandling errors,
        bool collectContent
    )
    {
        _options = options;
        _abort = abort;
        _abortToken = abort.Token;
        _steeringRun = steeringRun;
        _textSource = textSource;
        _finishSource = finishSource;
        _usageSource = usageSource;
        _stepsSource = stepsSource;
        _outputSource = outputSource;
        _errors = errors;
        _collectContent = collectContent;
    }

    // A transform that stopped the stream cancelled the abort token, but the results still settle, so the callbacks get no token then.
    private CancellationToken Token =>
        _abort.StoppedByTransform ? CancellationToken.None : _abortToken;

    // The id of the call, set by the step loop. It is the id of the steps the processor records.
    public string CallId { get; set; } = string.Empty;

    // The response messages of the initial approvals, set by the step loop. Maps to initialResponseMessages.
    public IReadOnlyList<ModelMessage> InitialResponseMessages { get; set; } = [];

    // The steps that the processor recorded. The step loop reads them after WaitForStepAsync returned.
    public List<StepResult> Steps => _steps;

    // Set by the step loop when a step starts. Maps to `openStep = { event: stepStartEvent }`.
    public GenerateTextStepStartEvent? OpenStep
    {
        get => Volatile.Read(ref _openStep);
        set => Volatile.Write(ref _openStep, value);
    }

    public StepResult[] SnapshotSteps()
    {
        lock (_gate)
        {
            return _steps.ToArray();
        }
    }

    // Maps to awaiting stepFinish. It is false when the stream ended before the step was recorded.
    public async Task<bool> WaitForStepAsync(int stepCount)
    {
        TaskCompletionSource<bool> waiter;
        lock (_gate)
        {
            if (_steps.Count >= stepCount)
            {
                return true;
            }

            if (!_stepWaiters.TryGetValue(stepCount, out waiter!))
            {
                waiter = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                _stepWaiters[stepCount] = waiter;
            }
        }

        var raced = await Task.WhenAny(waiter.Task, _ended.Task).ConfigureAwait(false);
        lock (_gate)
        {
            return _steps.Count >= stepCount;
        }
    }

    public async Task RunAsync(
        PartBuffer source,
        PartBuffer target,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await foreach (var part in source.Read(cancellationToken).ConfigureAwait(false))
            {
                if (part is StreamRetryBoundaryPart boundary)
                {
                    // The boundary replaces what the step recorded for the cancelled model call. It is not sent on.
                    _contentOrder = [];
                    _activeText = [];
                    _activeReasoning = [];
                    _executedResults = [];
                    _recordedRequest = boundary.Request;
                    _recordedWarnings = boundary.Warnings;
                    continue;
                }

                // A slow consumer pauses the processor, like upstream's pull model.
                await target.WaitForRoomAsync(cancellationToken).ConfigureAwait(false);
                target.Add(part);
                await ProcessAsync(part, target).ConfigureAwait(false);
            }

            await FlushAsync(target).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The consumers stopped reading. Nobody waits for the stream, so there is nothing to report.
            _textSource.TrySetCanceled();
            _finishSource.TrySetCanceled();
            _usageSource.TrySetCanceled();
            _stepsSource.TrySetCanceled();
            target.Complete();
        }
        catch (Exception exception)
        {
            _steeringRun?.End(SteeringNotDeliveredReason.Error);
            _textSource.TrySetException(exception);
            _finishSource.TrySetException(exception);
            _usageSource.TrySetException(exception);
            _stepsSource.TrySetException(exception);
            var streamError = exception;
            // an erroring stream skips the flush: report the failed step, then the error to telemetry:
            if (!_abort.Stopped && _abort.Error == null)
            {
                await Generation
                    .NotifyStepErrorAsync(_options, _openStep, exception)
                    .ConfigureAwait(false);
            }

            try
            {
                await StreamTextErrorHandling
                    .InvokeOnErrorAsync(
                        _options,
                        exception,
                        CancellationToken.None,
                        swallow: false
                    )
                    .ConfigureAwait(false);
            }
            catch (Exception callback)
            {
                streamError = callback;
            }

            target.Fail(streamError);
        }
        finally
        {
            _ended.TrySetResult(true);
        }
    }

    private async Task ProcessAsync(TextStreamPart part, PartBuffer target)
    {
        var cancellationToken = Token;
        // The stream retry reported the error parts it saw already.
        var handledForRetry = part is ErrorPart errorPart && _errors.TakeHandled(errorPart);
        if (!handledForRetry)
        {
            await StreamTextErrorHandling
                .NotifyChunkAsync(_options, part, cancellationToken)
                .ConfigureAwait(false);
        }

        switch (part)
        {
            case ErrorPart error:
                if (error.Error is NoOutputGeneratedException noOutput)
                {
                    _recordedNoOutputError = noOutput;
                }

                if (!handledForRetry)
                {
                    _openStepError = error.Error ?? new AiSdkException(error.Message);

                    await NotifyErrorAsync(error).ConfigureAwait(false);
                }

                break;
            case StartStepPart start:
                // reset the recorded data when a new step starts:
                _openStepError = null;
                _contentOrder = [];
                _activeText = [];
                _activeReasoning = [];
                _executedResults = [];
                _recordedRequest = start.Request;
                _recordedWarnings = start.Warnings;
                break;
            case TextStartPart textStart:
                var text = new TextRecord { ProviderMetadata = textStart.ProviderMetadata };
                _activeText[textStart.Id] = text;
                Record(_contentOrder, text);
                break;
            case TextDeltaPart delta:
                var activeText = FindText(delta.Id);
                if (_collectContent)
                {
                    activeText.Text.Append(delta.Text);
                }

                activeText.ProviderMetadata = delta.ProviderMetadata ?? activeText.ProviderMetadata;
                break;
            case TextEndPart textEnd:
                var ended = FindText(textEnd.Id);
                ended.ProviderMetadata = textEnd.ProviderMetadata ?? ended.ProviderMetadata;
                ended.Citations = textEnd.Citations;
                _activeText.Remove(textEnd.Id);
                break;
            case ReasoningStartPart reasoningStart:
                var reasoning = new ReasoningRecord
                {
                    ProviderMetadata = reasoningStart.ProviderMetadata,
                };
                _activeReasoning[reasoningStart.Id] = reasoning;
                Record(_contentOrder, reasoning);
                break;
            case ReasoningDeltaPart reasoningDelta:
                var activeReasoning = FindReasoning(reasoningDelta.Id);
                if (_collectContent)
                {
                    activeReasoning.Text.Append(reasoningDelta.Text);
                }

                activeReasoning.ProviderMetadata = reasoningDelta.ProviderMetadata
                    ?? activeReasoning.ProviderMetadata;
                break;
            case ReasoningEndPart reasoningEnd:
                var endedReasoning = FindReasoning(reasoningEnd.Id);
                endedReasoning.ProviderMetadata = reasoningEnd.ProviderMetadata
                    ?? endedReasoning.ProviderMetadata;
                _activeReasoning.Remove(reasoningEnd.Id);
                break;
            case DocumentSourcePart documentSource:
                Record(_contentOrder, documentSource.Source);
                break;
            case SourcePart sourcePart:
                Record(_contentOrder, sourcePart.Source);
                break;
            case FilePart filePart:
                Record(_contentOrder, filePart.File);
                break;
            case ReasoningFilePart reasoningFilePart:
                Record(
                    _contentOrder,
                    new GeneratedReasoningFile(
                        reasoningFilePart.File.Data,
                        reasoningFilePart.File.MediaType,
                        reasoningFilePart.ProviderMetadata
                    )
                );
                break;
            case ToolCallPart toolCallPart:
                Record(_contentOrder, toolCallPart.ToolCall);
                break;
            case ToolApprovalRequestPart approvalRequest:
                Record(
                    _contentOrder,
                    new GeneratedToolApprovalRequest(
                        approvalRequest.ApprovalId,
                        approvalRequest.ToolCall.ToolCallId
                    )
                    {
                        ToolCall = approvalRequest.ToolCall,
                        Reason = approvalRequest.Reason,
                        IsAutomatic = approvalRequest.IsAutomatic,
                        Signature = approvalRequest.Signature,
                    }
                );
                break;
            case ToolApprovalResponsePart approvalResponse:
                Record(
                    _contentOrder,
                    new GeneratedToolApprovalResponse(
                        approvalResponse.ApprovalId,
                        approvalResponse.ToolCall,
                        approvalResponse.Approved,
                        approvalResponse.Reason,
                        approvalResponse.ProviderExecuted
                    )
                );
                break;
            case ToolResultPart { Result.Preliminary: true }:
                break;
            case ToolResultPart { Result.ProviderExecuted: true } providerResult:
                Record(_contentOrder, ProviderResultContent(providerResult.Result));
                break;
            case ToolErrorPart { Result.ProviderExecuted: true } providerError:
                Record(_contentOrder, ProviderResultContent(providerError.Result));
                break;
            case ToolResultPart clientResult:
                _executedResults.Add(clientResult.Result);
                Record(_contentOrder, Generation.ToGeneratedToolResult(clientResult.Result));
                break;
            case ToolErrorPart clientError:
                _executedResults.Add(clientError.Result);
                Record(_contentOrder, Generation.ToGeneratedToolResult(clientError.Result));
                break;
            case StepFinishPart stepFinish:
                await FinishStepAsync(stepFinish).ConfigureAwait(false);
                break;
            case FinishPart finish:
                _recordedTotalUsage = finish.Usage;
                _recordedFinishReason = finish.FinishReason;
                break;
        }
    }

    // Maps to retention.collectContent: a low-memory stream keeps no content.
    private void Record(List<object> content, object item)
    {
        if (_collectContent)
        {
            content.Add(item);
        }
    }

    private async Task NotifyErrorAsync(ErrorPart error)
    {
        await StreamTextErrorHandling
            .InvokeOnErrorAsync(
                _options,
                error.Error ?? new AiSdkException(error.Message),
                Token,
                swallow: false,
                reportToTelemetry: false
            )
            .ConfigureAwait(false);
    }

    private TextRecord FindText(string id)
    {
        if (!_activeText.TryGetValue(id, out var record))
        {
            // A transform dropped the text-start part. The text still belongs to the step.
            record = new TextRecord();
            _activeText[id] = record;
            Record(_contentOrder, record);
        }

        return record;
    }

    private ReasoningRecord FindReasoning(string id)
    {
        if (!_activeReasoning.TryGetValue(id, out var record))
        {
            record = new ReasoningRecord();
            _activeReasoning[id] = record;
            Record(_contentOrder, record);
        }

        return record;
    }

    private static GeneratedToolResult ProviderResultContent(ExecutedTool result) =>
        new(result.ToolCallId, result.ToolName, result.OutputJson, result.IsError)
        {
            ProviderMetadata = result.ProviderMetadata,
        };

    // Maps to the finish-step branch of the event processor.
    private async Task FinishStepAsync(StepFinishPart part)
    {
        var baseStep = part.Step;
        var content = new List<GeneratedContent>();
        foreach (var entry in _contentOrder)
        {
            switch (entry)
            {
                case ReasoningRecord block:
                    content.Add(
                        new GeneratedReasoning(block.Text.ToString(), block.ProviderMetadata)
                    );
                    break;
                case TextRecord block:
                    content.Add(
                        new GeneratedText(
                            block.Text.ToString(),
                            block.Citations,
                            block.ProviderMetadata
                        )
                    );
                    break;
                case GeneratedToolCall call:
                    content.Add(Generation.ParseProviderExecutedInput(_options, call));
                    break;
                case GeneratedFileUrl remote:
                    content.Add(
                        await Generation
                            .ResolveGeneratedFileDataAsync(
                                remote,
                                _options.ExperimentalDownload,
                                Token
                            )
                            .ConfigureAwait(false)
                    );
                    break;
                case GeneratedContent other:
                    content.Add(other);
                    break;
            }
        }

        var toolCalls = new List<GeneratedToolCall>();
        var sources = new List<GeneratedSource>();
        var files = new List<GeneratedFile>();
        var text = new StringBuilder();
        var reasoningText = new StringBuilder();
        var toolResults = new List<ExecutedTool>();
        foreach (var item in content)
        {
            switch (item)
            {
                case GeneratedText generatedText:
                    text.Append(generatedText.Text);
                    break;
                case GeneratedToolCall call:
                    toolCalls.Add(call);
                    break;
                case GeneratedSource source:
                    sources.Add(source);
                    break;
                case GeneratedReasoning generatedReasoning:
                    reasoningText.Append(generatedReasoning.Text);
                    break;
                case GeneratedFile file:
                    files.Add(file);
                    break;
            }
        }

        // Upstream lists the provider's own results beside the client's; an error result is left out.
        foreach (var item in content)
        {
            if (
                item is GeneratedToolResult
                {
                    IsError: false,
                    ProviderExecuted: true
                } providerResult
            )
            {
                toolResults.Add(Generation.ProviderExecutedResult(providerResult, toolCalls));
            }
        }

        toolResults.AddRange(_executedResults);

        // The assistant message and the tool message are built from the recorded content, as upstream does.
        var stepMessages = ToResponseMessages.Convert(content, _options.Tools);
        if (!ToolExecution.IsToolExecutionAllowedFinishReason(baseStep.FinishReason))
        {
            // The results are dropped, but the approval responses stay.
            for (var index = stepMessages.Count - 1; index >= 0; index--)
            {
                if (stepMessages[index] is not ToolModelMessage tool)
                {
                    continue;
                }

                if (tool.ApprovalResponses.Count > 0)
                {
                    stepMessages[index] = new ToolModelMessage(tool.ApprovalResponses);
                }
                else
                {
                    stepMessages.RemoveAt(index);
                }
            }
        }

        if (stepMessages.Count == 0 || stepMessages[0] is not AssistantModelMessage)
        {
            stepMessages.Insert(0, new AssistantModelMessage(Array.Empty<AssistantContentPart>()));
        }

        var response = new StepResponse(
            part.Response.Id,
            part.Response.Timestamp,
            part.Response.ModelId,
            part.Response.Headers,
            stepMessages.ToArray(),
            baseStep.Response.Body
        );
        var step = new StepResult(
            text.ToString(),
            reasoningText.Length == 0 ? null : reasoningText.ToString(),
            toolCalls,
            toolResults,
            baseStep.FinishReason,
            baseStep.Usage,
            sources,
            baseStep.ProviderMetadata,
            baseStep.Warnings,
            files,
            baseStep.Provider,
            baseStep.ModelId,
            content,
            baseStep.Performance,
            baseStep.Request,
            response,
            part.RawFinishReason
        )
        {
            CallId = CallId,
            StepNumber = SnapshotCount(),
            ToolsContext = _options.ToolsContext ?? StepResult.EmptyToolsContext,
            RuntimeContext = _options.RuntimeContext ?? StepResult.EmptyRuntimeContext,
            StructuredOutputText = baseStep.StructuredOutputText,
            ExperimentalSteeringMessages = baseStep.ExperimentalSteeringMessages,
        };

        OpenStep = null;
        _openStepError = null;

        if (Generation.ResolveOnStepEnd(_options) is { } onStepEnd)
        {
            await onStepEnd(step, Token).ConfigureAwait(false);
        }

        if (TelemetryDispatcher.Create(_options).OnStepEnd is { } telemetryOnStepEnd)
        {
            await telemetryOnStepEnd(step, Token).ConfigureAwait(false);
        }

        LogWarnings.Log(
            new LogWarningsOptions(
                _recordedWarnings.Select(Generation.ToModelWarning).ToArray(),
                baseStep.Provider,
                baseStep.ModelId
            )
        );

        TaskCompletionSource<bool>? waiter;
        lock (_gate)
        {
            _steps.Add(step);
            _stepWaiters.TryGetValue(_steps.Count, out waiter);
        }

        // resolve the promise to signal that the step has been fully processed by the event processor:
        waiter?.TrySetResult(true);
    }

    private int SnapshotCount()
    {
        lock (_gate)
        {
            return _steps.Count;
        }
    }

    // Maps to notifyStepError of streamText. An abort or a timeout ends the step with onAbort, so the flush never reaches it then.
    private async Task<Exception> NotifyStepErrorAsync()
    {
        var error = _openStepError
            ?? new NoOutputGeneratedException("No output generated. Check the stream for errors.");
        var openStep = _openStep;
        _openStep = null;
        await Generation.NotifyStepErrorAsync(_options, openStep, error).ConfigureAwait(false);
        return error;
    }

    // Maps to the flush of the event processor.
    private async Task FlushAsync(PartBuffer target)
    {
        var steps = SnapshotSteps();
        if (_abort.Stopped && !_abort.StoppedByTransform)
        {
            _textSource.TrySetCanceled();
            _finishSource.TrySetCanceled();
            _usageSource.TrySetCanceled();
            _stepsSource.TrySetCanceled();
            target.Complete();
            return;
        }

        if (_abort.Error != null)
        {
            // The abort settled the results already.
            target.Complete();
            return;
        }

        // steering messages that are still waiting cannot be delivered anymore:
        _steeringRun?.End(
            steps.Length == 0 || _recordedNoOutputError != null
                ? SteeringNotDeliveredReason.Error
                : SteeringNotDeliveredReason.Finished
        );

        // a step that started without producing a result has failed:
        var stepError = await NotifyStepErrorAsync().ConfigureAwait(false);

        // reject when no output was generated or an incomplete model stream ended a continuation step:
        if (steps.Length == 0 || _recordedNoOutputError != null)
        {
            var noOutput = _recordedNoOutputError
                ?? new NoOutputGeneratedException(
                    "No output generated. Check the stream for errors."
                );
            // The stream itself does not report an error part to telemetry, so report here to close open spans:
            if (TelemetryDispatcher.Create(_options).OnError is { } telemetryOnError)
            {
                await Generation
                    .NotifyAsync(
                        () =>
                            telemetryOnError(
                                new TelemetryErrorEvent(null, _recordedNoOutputError ?? stepError),
                                Token
                            ),
                        Token
                    )
                    .ConfigureAwait(false);
            }

            _textSource.TrySetException(noOutput);
            _finishSource.TrySetException(noOutput);
            _usageSource.TrySetException(noOutput);
            _stepsSource.TrySetException(noOutput);
            target.Complete();
            return;
        }

        var cancellationToken = Token;
        var responseMessages = new List<ModelMessage>(InitialResponseMessages);
        foreach (var step in steps)
        {
            responseMessages.AddRange(step.Response.Messages);
        }

        // A model answer that does not parse into the output fails the output promise only, not the stream.
        Exception? outputError = null;
        GenerateTextResult result;
        var totalUsage = _recordedTotalUsage ?? LanguageModelUsage.Null;
        try
        {
            result = await Generation
                .ToResultAsync(
                    [.. steps],
                    responseMessages,
                    _options,
                    cancellationToken,
                    totalUsage: totalUsage
                )
                .ConfigureAwait(false);
        }
        catch (NoObjectGeneratedException error)
        {
            outputError = error;
            result = await Generation
                .ToResultAsync(
                    [.. steps],
                    responseMessages,
                    _options,
                    cancellationToken,
                    parseOutput: false,
                    totalUsage: totalUsage
                )
                .ConfigureAwait(false);
        }

        _textSource.TrySetResult(result.Text);
        _finishSource.TrySetResult(_recordedFinishReason ?? FinishReason.Other);
        _usageSource.TrySetResult(totalUsage);
        if (outputError != null)
        {
            _outputSource.TrySetException(outputError);
        }
        else
        {
            // Without a JSON output, the output is the text. Maps to `output ?? text()`.
            _outputSource.TrySetResult(
                result.Output
                    ?? (
                        _options.Output is { IsJson: true }
                            ? null
                            : JsonSerializer.SerializeToElement(result.Text)
                    )
            );
        }

        _stepsSource.TrySetResult(result.Steps);
#pragma warning disable CS0618 // OnFinish is the deprecated alias of OnEnd
        var onEnd = _options.OnEnd ?? _options.OnFinish;
#pragma warning restore CS0618
        if (onEnd != null)
        {
            try
            {
                await onEnd(result, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                // Errors in callbacks do not break the generation flow (upstream notify).
            }
        }

        if (TelemetryDispatcher.Create(_options).OnEnd is { } telemetryOnEnd)
        {
            await telemetryOnEnd(result, cancellationToken).ConfigureAwait(false);
        }

        target.Complete();
    }

    private sealed class TextRecord
    {
        public StringBuilder Text { get; } = new();

        public IReadOnlyList<Citation>? Citations { get; set; }

        public JsonElement? ProviderMetadata { get; set; }
    }

    private sealed class ReasoningRecord
    {
        public StringBuilder Text { get; } = new();

        public JsonElement? ProviderMetadata { get; set; }
    }
}

// Maps to StreamRetryBoundaryPart: a marker between two model calls of one step. It is internal and never reaches a consumer.
internal sealed class StreamRetryBoundaryPart : TextStreamPart
{
    public StreamRetryBoundaryPart(
        LanguageModelRequestMetadata request,
        IReadOnlyList<CallWarning>? warnings
    )
        : base("stream-retry-boundary")
    {
        Request = request;
        Warnings = warnings ?? [];
    }

    public LanguageModelRequestMetadata Request { get; }

    public IReadOnlyList<CallWarning> Warnings { get; }
}
