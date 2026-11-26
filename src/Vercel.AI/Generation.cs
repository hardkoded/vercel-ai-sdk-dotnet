// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateObject;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
#if NET
using Microsoft.AspNetCore.Http;
using Vercel.AI.AspNetCore;
#endif

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
            if (
                !options.AllowSystemInMessages
                && options.Messages.Any(message => message is SystemModelMessage)
            )
            {
                throw new InvalidPromptException(
                    "System messages are not allowed in the prompt or messages fields. Use the instructions option instead."
                );
            }

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

    internal static Instructions? ResolveInstructions(GenerateTextOptions options)
    {
        return options.Instructions
            ?? (options.System != null ? Instructions.From(options.System) : null);
    }

    /// <summary>The leading system messages for the call's instructions. An empty string adds none.</summary>
    internal static IReadOnlyList<SystemModelMessage> InstructionMessages(
        GenerateTextOptions options
    )
    {
        return InstructionMessages(ResolveInstructions(options));
    }

    internal static IReadOnlyList<SystemModelMessage> InstructionMessages(
        Instructions? instructions
    )
    {
        if (instructions?.Messages is { } systemMessages)
        {
            return systemMessages;
        }

        return string.IsNullOrEmpty(instructions?.Text)
            ? Array.Empty<SystemModelMessage>()
            : new[] { new SystemModelMessage(instructions!.Text!) };
    }

    /// <summary>
    /// Runs <c>prepareStep</c> and applies its instructions and messages overrides, which carry forward to later steps.
    /// The prompt keeps the instructions as leading system messages, so <paramref name="instructionCount"/> marks where the messages start.
    /// </summary>
    private static PrepareStepUpdate? PrepareStep<TOptions>(
        ref TOptions options,
        IReadOnlyList<StepResult> steps,
        ILanguageModel model,
        Instructions? initialInstructions,
        IReadOnlyList<ModelMessage> initialMessages,
        IReadOnlyList<ModelMessage> responseMessages,
        IReadOnlyList<UserModelMessage> steeringMessages,
        ref Instructions? instructions,
        ref List<ModelMessage> messages,
        ref int instructionCount
    )
        where TOptions : GenerateTextOptions
    {
        var update = options.PrepareStep?.Invoke(
            new PrepareStepContext(
                steps.Count,
                steps,
                messages.Skip(instructionCount).ToArray(),
                model,
                instructions,
                initialInstructions,
                initialMessages,
                responseMessages.ToArray()
            )
            {
                ExperimentalSteeringMessages = steeringMessages.Count > 0 ? steeringMessages : null,
                ExperimentalSandbox = options.ExperimentalSandbox,
                ToolsContext = options.ToolsContext ?? StepResult.EmptyToolsContext,
                RuntimeContext = options.RuntimeContext ?? StepResult.EmptyRuntimeContext,
            }
        );
        // The contexts from prepareStep apply to this step and all later steps:
        if (update?.ToolsContext != null || update?.RuntimeContext != null)
        {
            options = (TOptions)options.ShallowCopy();
            options.ToolsContext = update.ToolsContext ?? options.ToolsContext;
            options.RuntimeContext = update.RuntimeContext ?? options.RuntimeContext;
        }

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

    // Call settings from prepareStep apply to this step only. Maps to `maxOutputTokens: prepareStepResult?.maxOutputTokens ?? maxOutputTokens` and the other call settings.
    private static void ApplyCallSettings(
        LanguageModelCallOptions callOptions,
        PrepareStepUpdate? update
    )
    {
        if (update == null)
        {
            return;
        }

        callOptions.MaxOutputTokens = update.MaxOutputTokens ?? callOptions.MaxOutputTokens;
        callOptions.Temperature = update.Temperature ?? callOptions.Temperature;
        callOptions.TopP = update.TopP ?? callOptions.TopP;
        callOptions.TopK = update.TopK ?? callOptions.TopK;
        callOptions.PresencePenalty = update.PresencePenalty ?? callOptions.PresencePenalty;
        callOptions.FrequencyPenalty = update.FrequencyPenalty ?? callOptions.FrequencyPenalty;
        callOptions.StopSequences = update.StopSequences ?? callOptions.StopSequences;
        callOptions.Seed = update.Seed ?? callOptions.Seed;
        callOptions.Reasoning = update.Reasoning ?? callOptions.Reasoning;
    }

    private static GenerateTextOptions WithActiveTools(
        GenerateTextOptions options,
        PrepareStepUpdate? update
    )
    {
        if (update?.ActiveTools == null)
        {
            return options;
        }

        var stepOptions = options.ShallowCopy();
        stepOptions.ActiveTools = update.ActiveTools;
        return stepOptions;
    }

    // Maps to `include = experimental_include` of streamText.
    private static StreamTextInclude ResolveStreamInclude(StreamTextOptions options)
    {
#pragma warning disable CS0618 // ExperimentalInclude is the deprecated alias of Include
        return options.Include ?? options.ExperimentalInclude ?? new StreamTextInclude();
#pragma warning restore CS0618
    }

    // Maps to `stepSandbox = prepareStepResult?.experimental_sandbox ?? sandbox`. The sandbox from prepareStep applies to tool execution in this step only.
    private static GenerateTextOptions StepOptions(
        GenerateTextOptions options,
        PrepareStepUpdate? update
    )
    {
        options = WithActiveTools(options, update);
        if (update?.ExperimentalSandbox == null)
        {
            return options;
        }

        var stepOptions = options.ShallowCopy();
        stepOptions.ExperimentalSandbox = update.ExperimentalSandbox;
        return stepOptions;
    }

    /// <summary>
    /// What a step that a steering interrupt cancelled keeps for its redo. The redo is the same step: it keeps its number, its timeout, and the
    /// steering messages that the step already delivered. Maps to the <c>redo</c> parameter of <c>streamStep</c> and to the state of
    /// <c>redoInterruptedStepAttempts</c>.
    /// </summary>
    private sealed record StepRedo(
        Instructions? Instructions,
        List<ModelMessage> Messages,
        int InstructionCount,
        ILanguageModel Model,
        IReadOnlyList<UserModelMessage> SteeringMessages,
        IReadOnlyList<UserModelMessage> DeliveredSteeringMessages,
        IReadOnlyDictionary<string, JsonElement>? ToolsContext,
        IReadOnlyDictionary<string, object?>? RuntimeContext
    )
    {
        // The start-step part of the step was streamed by a cancelled model call.
        public bool StartStepStreamed { get; set; }

        // onStepStart was notified for the step by a cancelled model call, and it is notified once per step.
        public bool StepStartNotified { get; set; }
    }

    // Maps to the shouldStartNextStep of generateText and streamText. A step with no client tool calls continues
    // only for a waiting steering message, and `endReason` says why the waiting messages are not delivered when the loop ends.
    // Tracks provider-executed tool calls that support deferred results (e.g. code_execution in programmatic tool calling).
    // Such a call may get its result in a later step, and the loop continues until it does.
    private static void TrackPendingDeferredToolCalls(
        StepResult step,
        IReadOnlyList<Tool>? tools,
        Dictionary<string, string> pendingDeferredToolCalls
    )
    {
        foreach (var call in step.ToolCalls)
        {
            if (!call.ProviderExecuted)
            {
                continue;
            }

            var tool = tools?.FirstOrDefault(t =>
                t.IsProviderDefined
                && string.Equals(t.Name, call.ToolName, StringComparison.Ordinal)
            );
            if (tool is not { SupportsDeferredResults: true })
            {
                continue;
            }

            if (!step.ToolResults.Any(r => r.ToolCallId == call.ToolCallId))
            {
                pendingDeferredToolCalls[call.ToolCallId] = call.ToolName;
            }
        }

        foreach (var result in step.ToolResults)
        {
            pendingDeferredToolCalls.Remove(result.ToolCallId);
        }
    }

    private static bool ShouldStartNextStep(
        StepResult step,
        List<StepResult> steps,
        StopCondition stop,
        SteeringRun? steeringRun,
        Dictionary<string, string> pendingDeferredToolCalls,
        ref SteeringNotDeliveredReason endReason
    )
    {
        if (step.FinishReason == FinishReason.Interrupted)
        {
            return false;
        }

        if (HasClientToolCall(step))
        {
            // Continue only after all client tool calls have been executed or denied:
            if (
                !ToolExecution.IsToolExecutionAllowedFinishReason(step.FinishReason)
                || HasUnsettledToolCall(step)
            )
            {
                endReason = SteeringNotDeliveredReason.WaitingForToolResults;
                return false;
            }
        }
        else if (pendingDeferredToolCalls.Count == 0)
        {
            // queued messages wait for a task that was not cut off:
            var isTruncated =
                step.FinishReason is FinishReason.Length or FinishReason.ContentFilter;
            if (
                steeringRun?.HasPendingMessages() != true
                && (isTruncated || steeringRun?.PromoteQueuedMessage() != true)
            )
            {
                return false;
            }

            // steering messages do not continue a failed step:
            if (step.FinishReason == FinishReason.Error)
            {
                endReason = SteeringNotDeliveredReason.Error;
                return false;
            }
        }

        // continue until a stop condition is met:
        if (stop.ShouldStop(steps))
        {
            endReason = SteeringNotDeliveredReason.StopCondition;
            return false;
        }

        return true;
    }

    public static LanguageModelCallOptions CallOptions(
        GenerateTextOptions options,
        IReadOnlyList<ModelMessage> prompt,
        IReadOnlyList<string>? stepToolOrder = null
    )
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
            JsonResponse = options.Output is { IsJson: true, Schema: null },
            JsonSchema = options.Output?.Schema,
            JsonSchemaName = options.Output?.SchemaName,
            JsonSchemaDescription = options.Output?.SchemaDescription,
            Tools = SelectTools(options, stepToolOrder ?? options.ToolOrder),
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
        CancellationToken callerToken
    )
    {
        options = options.ShallowCopy();
        options.Headers = WithUserAgentHeader(options.Headers);
        options.TelemetrySnapshot = TelemetryDispatcher.Create(options);
        // Maps to `include = experimental_include` and the defaults of `include`: nothing is retained unless it is asked for.
#pragma warning disable CS0618 // ExperimentalInclude is the deprecated alias of Include
        var include = options.Include ?? options.ExperimentalInclude ?? new GenerateTextInclude();
#pragma warning restore CS0618
        // With a timeout, the model and the tools get a token that also cancels when the timeout expires.
        using var timeouts = CallTimeouts.Create(options.Timeout, null, false, callerToken);
        var cancellationToken = timeouts?.Token ?? callerToken;
        var steeringRun = Steering.StartSteeringRun(options.ExperimentalSteering);
        // the step that has started but not ended, reported to onStepError when the call fails before the step produces a result:
        GenerateTextStepStartEvent? openStep = null;
        // The generateText span and the step span end when these complete. The port's step loop is flat, so a step that a steering
        // interrupt redoes stays in one span without upstream's callback around the step.
        TaskCompletionSource<object?>? generateTextSpan = null;
        TaskCompletionSource<object?>? stepSpan = null;
        string? failedCallId = null;
        try
        {
            var resolvedMaxRetries = Vercel.AI.Operations.OperationRetry.ResolveMaxRetries(
                options.MaxRetries
            );
            ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(options.ToolCallConcurrency);
            WarnUnsupportedTimeouts(options, model);
            using var scope = telemetry?.Begin("generateText", model.ModelId);
            var messages = BuildPrompt(options);
            var initialInstructions = ResolveInstructions(options);
            var instructions = initialInstructions;
            var instructionCount = InstructionMessages(instructions).Count;
            var initialMessages = messages.Skip(instructionCount).ToArray();
            var responseMessages = new List<ModelMessage>();
            var callId = (options.GenerateCallId ?? NewCallId)();
            failedCallId = callId;
            var telemetryDispatcher =
                GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                    options
                );
            generateTextSpan = new TaskCompletionSource<object?>();
            telemetryDispatcher.StartTracingChannelContext(
                TelemetryTracingEventType.GenerateText,
                CreateStartEvent(
                    options,
                    callId,
                    model,
                    initialInstructions,
                    initialMessages,
                    resolvedMaxRetries
                ),
                generateTextSpan.Task
            );
            if (options.OnStart != null)
            {
                await NotifyAsync(
                        () =>
                            options.OnStart(
                                CreateStartEvent(
                                    options,
                                    callId,
                                    model,
                                    initialInstructions,
                                    initialMessages,
                                    resolvedMaxRetries
                                ),
                                cancellationToken
                            ),
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }

            if (telemetryDispatcher.OnStart != null)
            {
                await telemetryDispatcher
                    .OnStart(
                        CreateStartEvent(
                            options,
                            callId,
                            model,
                            initialInstructions,
                            initialMessages,
                            resolvedMaxRetries
                        ),
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }

            await ProcessInitialApprovalsAsync(
                    options,
                    messages,
                    instructionCount,
                    responseMessages,
                    options.NowMs ?? DefaultNowMs,
                    cancellationToken,
                    callId: callId
                )
                .ConfigureAwait(false);
            var steps = new List<StepResult>();
            var stop = options.StopWhen ?? StopWhen.IsStepCount(1);
            var current = model;
            // why waiting steering messages are not delivered when the loop ends:
            var pendingDeferredToolCalls = new Dictionary<string, string>();
            var steeringEndReason = SteeringNotDeliveredReason.Finished;
            // A step that a steering interrupt cancelled is made again as the same step:
            StepRedo? redo = null;
            while (steps.Count < HardStepCap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (redo == null)
                {
                    timeouts?.StartStep();
                    stepSpan?.TrySetResult(null);
                    stepSpan = new TaskCompletionSource<object?>();
                    telemetryDispatcher.StartTracingChannelContext(
                        TelemetryTracingEventType.Step,
                        new TracingChannelStepEvent(callId, steps.Count),
                        stepSpan.Task
                    );
                }

                // A model from prepareStep applies to one step only. Maps to `prepareStepResult?.model ?? model`.
                current = model;
                var takenMessages = steeringRun?.TakeMessages() ?? Array.Empty<UserModelMessage>();
                IReadOnlyList<UserModelMessage> steeringMessages = takenMessages;
                if (redo != null)
                {
                    // prepareStep runs again for the redo. It gets the instructions and messages that the step started with,
                    // not the ones that it returned for the cancelled model call:
                    instructions = redo.Instructions;
                    messages = [.. redo.Messages];
                    instructionCount = redo.InstructionCount;
                    current = redo.Model;
                    options = options.ShallowCopy();
                    options.ToolsContext = redo.ToolsContext;
                    options.RuntimeContext = redo.RuntimeContext;
                    steeringMessages = [.. redo.SteeringMessages, .. takenMessages];
                }

                messages.AddRange(takenMessages);
                var stepStart = new StepRedo(
                    instructions,
                    [.. messages],
                    instructionCount,
                    current,
                    steeringMessages,
                    [],
                    options.ToolsContext,
                    options.RuntimeContext
                );
                var update = PrepareStep(
                    ref options,
                    steps,
                    current,
                    initialInstructions,
                    initialMessages,
                    responseMessages,
                    steeringMessages,
                    ref instructions,
                    ref messages,
                    ref instructionCount
                );
                if (update?.Model != null)
                {
                    current = update.Model;
                }

                // steering messages are delivered when they are in the messages that are sent to the model after prepareStep:
                var deliveredSteeringMessages = new List<UserModelMessage>(
                    redo?.DeliveredSteeringMessages ?? []
                );
                if (steeringRun != null)
                {
                    deliveredSteeringMessages.AddRange(
                        steeringRun
                            .ResolveTakenMessages(steps.Count, messages)
                            .Select(delivered => delivered.Message)
                    );
                }

                // The model gets a snapshot, because the step appends its own response to `messages` afterwards.
                // activeTools from prepareStep applies to this step only. Maps to `activeTools: prepareStepResult?.activeTools ?? activeTools`.
                var stepActiveOptions = WithActiveTools(options, update);
                var callOptions = CallOptions(
                    stepActiveOptions,
                    messages.ToArray(),
                    update?.ToolOrder
                );
                callOptions.AbortSignal = timeouts?.Signal;
                ApplyCallSettings(callOptions, update);
                if (update?.ToolChoice != null)
                {
                    callOptions.ToolChoice = update.ToolChoice;
                }

                callOptions.ProviderOptions = MergeProviderOptions(
                    callOptions.ProviderOptions,
                    update?.ProviderOptions
                );
                callOptions.Prompt = await LanguageModelPrompts
                    .ConvertToLanguageModelPromptAsync(
                        callOptions.Prompt,
                        current.SupportedUrls,
                        options.ExperimentalDownload,
                        cancellationToken,
                        current.Provider.Split('.')[0]
                    )
                    .ConfigureAwait(false);

                var stepStartNotified = redo?.StepStartNotified == true;
                if (
                    (
                        options.OnStepStart != null
                        || telemetryDispatcher.OnStepStart != null
                        || options.OnStepError != null
                        || telemetryDispatcher.OnStepError != null
                    ) && !stepStartNotified
                )
                {
                    stepStartNotified = true;
                    var stepStartEvent = new GenerateTextStepStartEvent
                    {
                        CallId = callId,
                        Provider = current.Provider,
                        ModelId = current.ModelId,
                        StepNumber = steps.Count,
                        Instructions = instructions,
                        Messages = messages.Skip(instructionCount).ToArray(),
                        Tools = options.Tools,
                        ToolChoice = update?.ToolChoice ?? options.ToolChoice,
                        ActiveTools = stepActiveOptions.ActiveTools,
                        ToolOrder = update?.ToolOrder ?? options.ToolOrder,
                        Steps = steps.ToArray(),
                        ProviderOptions = callOptions.ProviderOptions,
                        Output = options.Output,
                        ToolsContext = options.ToolsContext,
                        RuntimeContext = options.RuntimeContext,
                    };
                    openStep = stepStartEvent;
                    if (options.OnStepStart != null)
                    {
                        await NotifyAsync(
                                () => options.OnStepStart(stepStartEvent, cancellationToken),
                                cancellationToken
                            )
                            .ConfigureAwait(false);
                    }

                    if (telemetryDispatcher.OnStepStart != null)
                    {
                        await telemetryDispatcher
                            .OnStepStart(stepStartEvent, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                if (options.OnLanguageModelCallStart != null)
                {
                    await NotifyAsync(
                            () =>
                                options.OnLanguageModelCallStart(
                                    new LanguageModelCallStartEvent
                                    {
                                        CallId = callId,
                                        Provider = current.Provider,
                                        ModelId = current.ModelId,
                                        Instructions = instructions,
                                        Messages = messages.Skip(instructionCount).ToArray(),
                                        Tools = callOptions.Tools,
                                        CallOptions = callOptions,
                                    },
                                    cancellationToken
                                ),
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                }

                var modelCallStartEvent = new LanguageModelCallStartEvent
                {
                    CallId = callId,
                    Provider = current.Provider,
                    ModelId = current.ModelId,
                    Instructions = instructions,
                    Messages = messages.Skip(instructionCount).ToArray(),
                    Tools = callOptions.Tools,
                    CallOptions = callOptions,
                };
                var modelCallDispatcher =
                    GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                        options
                    );
                if (modelCallDispatcher.OnLanguageModelCallStart is { } telemetryCallStart)
                {
                    await telemetryCallStart(modelCallStartEvent, cancellationToken)
                        .ConfigureAwait(false);
                }

                var now = options.NowMs ?? DefaultNowMs;
                var stepStartMs = now();
                LanguageModelGenerateResult modelResponse;
                var interrupt =
                    steeringRun == null
                        ? null
                        : new StepInterrupt(steeringRun, timeouts?.Signal, cancellationToken);
                try
                {
                    if (interrupt != null)
                    {
                        callOptions.AbortSignal = interrupt.Signal;
                    }

                    var modelCallToken = interrupt?.Token ?? cancellationToken;
                    modelResponse = await modelCallDispatcher
                        .ExecuteLanguageModelCallAsync(
                            modelCallStartEvent,
                            () => current.DoGenerateAsync(callOptions, modelCallToken),
                            modelCallToken
                        )
                        .ConfigureAwait(false);
                }
                catch (Exception)
                    when (interrupt is { IsRequested: true }
                        // A caller abort or a timeout wins over an interrupt: the run ends instead of making the model call again.
                        && !cancellationToken.IsCancellationRequested
                        && timeouts?.Signal.Aborted != true
                    )
                {
                    redo = stepStart with
                    {
                        DeliveredSteeringMessages = deliveredSteeringMessages,
                        StepStartNotified = stepStartNotified,
                    };
                    continue;
                }
                finally
                {
                    interrupt?.Dispose();
                }

                redo = null;
                var responseTimeMs = now() - stepStartMs;
                var generated = await ParseToolCallsAsync(
                        modelResponse,
                        options,
                        messages,
                        instructions,
                        instructionCount,
                        callOptions.ToolChoice?.Type == "none",
                        cancellationToken
                    )
                    .ConfigureAwait(false);
                if (
                    options.OnLanguageModelCallEnd != null
                    || telemetryDispatcher.OnLanguageModelCallEnd != null
                )
                {
                    var callEndEvent = new LanguageModelCallEndEvent(
                        current.Provider,
                        generated.ResponseModelId ?? current.ModelId,
                        generated.FinishReason,
                        generated.Usage,
                        generated.Content,
                        generated.ResponseId,
                        generated.ProviderMetadata,
                        TimeSpan.FromMilliseconds(responseTimeMs)
                    )
                    {
                        CallId = callId,
                        Request = RequestOrigin.GetRequestOrigin(modelResponse.Request?.Url)
                            is { } origin
                            ? new LanguageModelStreamRequest { Url = origin }
                            : null,
                    };
                    if (options.OnLanguageModelCallEnd != null)
                    {
                        await NotifyAsync(
                                () =>
                                    options.OnLanguageModelCallEnd(callEndEvent, cancellationToken),
                                cancellationToken
                            )
                            .ConfigureAwait(false);
                    }

                    if (telemetryDispatcher.OnLanguageModelCallEnd != null)
                    {
                        await telemetryDispatcher
                            .OnLanguageModelCallEnd(callEndEvent, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                EnforceToolChoice(callOptions.ToolChoice, generated, current);
                var step = await FinishStepAsync(
                        generated,
                        StepOptions(options, update),
                        messages,
                        instructionCount,
                        responseMessages,
                        current,
                        cancellationToken,
                        new StepTiming(now, stepStartMs, responseTimeMs, Array.Empty<double>()),
                        // Maps to `stepRequest`: the body and the messages are kept only when `include` asks for them.
                        new LanguageModelRequestMetadata
                        {
                            Url = RequestOrigin.SanitizeRequestMetadata(modelResponse.Request)?.Url,
                            Body = include.RequestBody ? modelResponse.Request?.Body : null,
                            RedactBody = modelResponse.Request?.RedactBody,
                            Messages = include.RequestMessages ? messages.ToArray() : null,
                        },
                        GeneratedResponseInfo(generated, current),
                        include.ResponseBody ? ParseResponseBody(generated.RawResponse) : null,
                        steeringMessages: deliveredSteeringMessages,
                        callId: callId,
                        stepNumber: steps.Count
                    )
                    .ConfigureAwait(false);
                steps.Add(step);
                openStep = null;
                if (ResolveOnStepEnd(options) is { } onStepEnd)
                {
                    await onStepEnd(step, cancellationToken).ConfigureAwait(false);
                }

                if (telemetryDispatcher.OnStepEnd != null)
                {
                    await telemetryDispatcher
                        .OnStepEnd(step, cancellationToken)
                        .ConfigureAwait(false);
                }

                LogWarnings.Log(
                    new LogWarningsOptions(
                        (generated.Warnings ?? []).Select(ToModelWarning).ToArray(),
                        current.Provider,
                        current.ModelId
                    )
                );

                TrackPendingDeferredToolCalls(step, options.Tools, pendingDeferredToolCalls);
                if (
                    !ShouldStartNextStep(
                        step,
                        steps,
                        stop,
                        steeringRun,
                        pendingDeferredToolCalls,
                        ref steeringEndReason
                    )
                )
                {
                    break;
                }
            }

            steeringRun?.End(steeringEndReason);
            timeouts?.EndStep();
            stepSpan?.TrySetResult(null);
            // Upstream notifies onEnd before it parses the output, so onEnd still fires when parsing throws.
            Exception? outputError = null;
            GenerateTextResult result;
            try
            {
                result = await ToResultAsync(steps, responseMessages, options, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (NoObjectGeneratedException error)
            {
                outputError = error;
                result = await ToResultAsync(
                        steps,
                        responseMessages,
                        options,
                        cancellationToken,
                        parseOutput: false
                    )
                    .ConfigureAwait(false);
            }

            if (telemetry != null && scope != null)
            {
                try
                {
                    telemetry.OnUsage(scope, result.Usage);
                    telemetry.OnFinish(scope, result.FinishReason);
                }
                catch (Exception)
                    when (outputError != null && !cancellationToken.IsCancellationRequested)
                {
                    // Like upstream notify, a telemetry failure must not replace the saved parse error or skip onEnd.
                }
            }

#pragma warning disable CS0618 // OnFinish is the deprecated alias of OnEnd
            var onEnd = options.OnEnd ?? options.OnFinish;
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

            if (
                GenerateTextRestrictedTelemetryDispatcher
                    .CreateRestrictedTelemetryDispatcher(options)
                    .OnEnd is
                { } telemetryOnEnd
            )
            {
                await telemetryOnEnd(result, cancellationToken).ConfigureAwait(false);
            }

            if (outputError != null)
            {
                ExceptionDispatchInfo.Capture(outputError).Throw();
            }

            generateTextSpan?.TrySetResult(result);
            return result;
        }
        catch (ApiUserAbortException abortException)
        {
            steeringRun?.End(SteeringNotDeliveredReason.Aborted);
            // generateText has no onAbort, so aborts also end the step here:
            await NotifyStepErrorAsync(options, openStep, abortException).ConfigureAwait(false);
            stepSpan?.TrySetException(abortException);
            generateTextSpan?.TrySetException(abortException);
            throw;
        }
        catch (Exception exception)
        {
            steeringRun?.End(
                callerToken.IsCancellationRequested
                || timeouts?.Token.IsCancellationRequested == true
                    ? SteeringNotDeliveredReason.Aborted
                    : SteeringNotDeliveredReason.Error
            );

            // An expired timeout aborts the call. Like upstream, the call fails with the timeout reason.
            var error =
                exception is OperationCanceledException or ApiTimeoutException
                && !callerToken.IsCancellationRequested
                && timeouts?.Expired is { } expired
                    ? expired
                    : exception;
            await NotifyStepErrorAsync(options, openStep, error).ConfigureAwait(false);
            if (options.OnError != null)
            {
                await options.OnError(error, callerToken).ConfigureAwait(false);
            }

            if (
                GenerateTextRestrictedTelemetryDispatcher
                    .CreateRestrictedTelemetryDispatcher(options)
                    .OnError is
                { } telemetryOnError
            )
            {
                await telemetryOnError(new TelemetryErrorEvent(failedCallId, error), callerToken)
                    .ConfigureAwait(false);
            }

            stepSpan?.TrySetException(error);
            generateTextSpan?.TrySetException(error);
            if (!ReferenceEquals(error, exception))
            {
                throw error;
            }

            throw;
        }
    }

    // Maps to the onStepError notification of generateText and the notifyStepError of streamText. Notifies the callback of the
    // options, then telemetry. An exception of either is ignored, like upstream notify.
    internal static async Task NotifyStepErrorAsync(
        GenerateTextOptions options,
        GenerateTextStepStartEvent? openStep,
        Exception error
    )
    {
        if (openStep == null)
        {
            return;
        }

        var stepError = new GenerateTextStepErrorEvent(openStep, error);
        if (options.OnStepError is { } onStepError)
        {
            await NotifyAsync(
                    () => onStepError(stepError, CancellationToken.None),
                    CancellationToken.None
                )
                .ConfigureAwait(false);
        }

        if (
            GenerateTextRestrictedTelemetryDispatcher
                .CreateRestrictedTelemetryDispatcher(options)
                .OnStepError is
            { } telemetryOnStepError
        )
        {
            await NotifyAsync(
                    () => telemetryOnStepError(stepError, CancellationToken.None),
                    CancellationToken.None
                )
                .ConfigureAwait(false);
        }
    }

    // Maps to `onStepEnd ?? onStepFinish`.
    internal static Func<StepResult, CancellationToken, Task>? ResolveOnStepEnd(
        GenerateTextOptions options
    )
    {
#pragma warning disable CS0618 // OnStepFinish is the deprecated alias of OnStepEnd
        return options.OnStepEnd ?? options.OnStepFinish;
#pragma warning restore CS0618
    }

    internal static ModelWarning ToModelWarning(CallWarning warning) =>
        warning.Type switch
        {
            "unsupported" => new UnsupportedWarning(
                warning.Feature ?? warning.Message,
                warning.Feature == null ? null : warning.Message
            ),
            "compatibility" => new CompatibilityWarning(
                warning.Feature ?? warning.Message,
                warning.Feature == null ? null : warning.Message
            ),
            "deprecated" => new DeprecatedWarning(warning.Setting ?? string.Empty, warning.Message),
            _ => new OtherWarning(warning.Message),
        };

    // generateText does not stream, so a firstChunkMs or chunkMs timeout has no effect.
    private static void WarnUnsupportedTimeouts(GenerateTextOptions options, ILanguageModel model)
    {
        var warnings = new List<ModelWarning>();
        if (options.Timeout?.FirstChunkMs != null)
        {
            warnings.Add(
                new UnsupportedWarning(
                    "timeout.firstChunkMs",
                    "The firstChunkMs timeout is only supported by streaming functions."
                )
            );
        }

        if (options.Timeout?.ChunkMs != null)
        {
            warnings.Add(
                new UnsupportedWarning(
                    "timeout.chunkMs",
                    "The chunkMs timeout is only supported by streaming functions."
                )
            );
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
        CancellationToken cancellationToken,
        bool singleConsumer = false
    )
    {
        var steeringRun = Steering.StartSteeringRun(options.ExperimentalSteering);
        try
        {
            return StartStream(
                model,
                options,
                telemetry,
                steeringRun,
                cancellationToken,
                singleConsumer
            );
        }
        catch
        {
            // the call can fail before the stream starts, e.g. for invalid settings:
            steeringRun?.End(SteeringNotDeliveredReason.Error);
            throw;
        }
    }

    private static StreamTextResult StartStream(
        ILanguageModel model,
        StreamTextOptions options,
        IAiTelemetry? telemetry,
        SteeringRun? steeringRun,
        CancellationToken cancellationToken,
        bool singleConsumer
    )
    {
        options = (StreamTextOptions)options.ShallowCopy();
        options.TelemetrySnapshot = TelemetryDispatcher.Create(options);
        Vercel.AI.Operations.OperationRetry.ResolveMaxRetries(options.MaxRetries);
        var streamRetries = Vercel.AI.Operations.OperationRetry.ResolveMaxRetries(
            options.StreamRetries,
            "streamRetries",
            0
        );
        var errors = new StreamTextErrorHandling(options);
        ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(options.ToolCallConcurrency);
        // The consumers read this buffer. The event processor feeds it.
        var buffer = new PartBuffer
        {
            SingleConsumer = singleConsumer,
            HighWaterMark = singleConsumer ? 1 : null,
        };
        // The event processor reads this buffer: the stream after the transforms and the output transform.
        var recordedBuffer = new PartBuffer
        {
            SingleConsumer = true,
            HighWaterMark = singleConsumer ? 1 : null,
        };
        if (options.Output is { IsJson: true } outputSpecification)
        {
            recordedBuffer.Transform = new OutputTransformStream(outputSpecification).Transform;
        }

        var textSource = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var finishSource = new TaskCompletionSource<FinishReason>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var usageSource = new TaskCompletionSource<LanguageModelUsage>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var stepsSource = new TaskCompletionSource<IReadOnlyList<StepResult>>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var outputSource = new TaskCompletionSource<JsonElement?>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var initialResponseMessagesSource = new TaskCompletionSource<IReadOnlyList<ModelMessage>>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var abort = new StreamAbort { SteeringRun = steeringRun };
        var timeouts = CallTimeouts.Create(
            options.Timeout,
            options.AbortSignal,
            true,
            cancellationToken
        );
        var signal = timeouts?.Signal ?? options.AbortSignal;
        if (signal != null)
        {
            Action onSignal = () =>
                abort.Fire(
                    signal.Reason as Exception
                        ?? new OperationCanceledException(signal.Reason?.ToString()),
                    textSource,
                    finishSource,
                    usageSource,
                    stepsSource
                );
            signal.AddAbortHandler(onSignal);
            abort.SignalCleanup = () => signal.RemoveAbortHandler(onSignal);
        }

        if (cancellationToken.CanBeCanceled)
        {
            abort.Registration = cancellationToken.Register(() =>
                abort.Fire(
                    new OperationCanceledException(cancellationToken),
                    textSource,
                    finishSource,
                    usageSource,
                    stepsSource
                )
            );
        }

        // Decide before execution so a getter access cannot change collection. Maps to the collectContent of streamTextLowMemory.
        var collectContent = !singleConsumer || NeedsContentCollection(options);
        var pipeline = new CancellationTokenSource();
        recordedBuffer.OnConsumerStop = abort.Stop;
        buffer.OnConsumerStop = () =>
        {
            abort.Stop();
            try
            {
                pipeline.Cancel();
            }
            catch (Exception exception)
                when (exception is AggregateException or ObjectDisposedException) { }
        };
        var producerBuffer = recordedBuffer;
        if (options.ExperimentalTransform is { Count: > 0 } transforms)
        {
            // The producer writes to its own buffer. The transforms read it and feed the buffer that the event processor reads.
            producerBuffer = new PartBuffer
            {
                SingleConsumer = true,
                HighWaterMark = singleConsumer ? 1 : null,
                OnConsumerStop = abort.Stop,
            };
            ApplyStreamTextTransforms(
                producerBuffer,
                recordedBuffer,
                transforms,
                options.Tools,
                abort,
                pipeline.Token
            );
        }

        var eventProcessor = new StreamTextEventProcessor(
            options,
            abort,
            steeringRun,
            textSource,
            finishSource,
            usageSource,
            stepsSource,
            outputSource,
            errors,
            collectContent
        );
        _ = Task.Run(async () =>
        {
            try
            {
                await eventProcessor
                    .RunAsync(recordedBuffer, buffer, pipeline.Token)
                    .ConfigureAwait(false);
            }
            finally
            {
                pipeline.Dispose();
            }
        });
        _ = Task.Run(() =>
            ProduceAsync(
                model,
                options,
                telemetry,
                steeringRun,
                producerBuffer,
                abort,
                timeouts,
                collectContent,
                eventProcessor,
                errors,
                streamRetries,
                initialResponseMessagesSource
            )
        );
        return new StreamTextResult(
            buffer,
            textSource.Task,
            finishSource.Task,
            usageSource.Task,
            stepsSource.Task,
            outputSource.Task,
            initialResponseMessagesSource.Task
        )
        {
            Tools = options.Tools,
            OutputSpecification = options.Output,
        };
    }

    // Maps to the collectContent decision of streamTextLowMemory. Tools, an output, steps, or a callback need the complete content.
    // Enabled telemetry with at least one per-call or global integration also needs the content.
    private static bool NeedsContentCollection(StreamTextOptions options)
    {
#pragma warning disable CS0618 // the deprecated aliases collect content like their replacements
        return options.Tools is { Count: > 0 }
            || options.ToolChoice != null
            || options.Output != null
            || options.PrepareStep != null
            || options.StopWhen != null
            || options.OnEnd != null
            || options.OnFinish != null
            || options.OnStepEnd != null
            || options.OnAbort != null
            || options.OnLanguageModelCallEnd != null
            || GenerateTextRestrictedTelemetryDispatcher
                .CreateRestrictedTelemetryDispatcher(options)
                .HasIntegrations;
#pragma warning restore CS0618
    }

    // Maps to applyStreamTextTransforms.
    private static void ApplyStreamTextTransforms(
        PartBuffer source,
        PartBuffer target,
        IReadOnlyList<StreamTextTransform> transforms,
        IReadOnlyList<Tool>? tools,
        StreamAbort abort,
        CancellationToken cancellation
    )
    {
        var stopped = 0;
        StreamRetryBoundaryPart? pendingBoundary = null;
        var sourceReader = source.Read(cancellation).GetAsyncEnumerator(cancellation);

        void StopStream()
        {
            Volatile.Write(ref stopped, 1);
            // The stream ends, and the results settle with what the transforms let through, like upstream's terminate.
            abort.StoppedByTransform = true;
            abort.Stop();
        }

        // The gate prevents further parts from being emitted after a transform calls stopStream.
        async IAsyncEnumerable<TextStreamPart> Gate(
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            // One reader serves all segments, because the buffer releases the parts that were read.
            while (await sourceReader.MoveNextAsync().ConfigureAwait(false))
            {
                var part = sourceReader.Current;
                if (Volatile.Read(ref stopped) == 1)
                {
                    yield break;
                }

                if (part is StreamRetryBoundaryPart boundary)
                {
                    pendingBoundary = boundary;
                    yield break;
                }

                yield return part;
            }
        }

        _ = Task.Run(async () =>
        {
            try
            {
                // Each segment between two retry boundaries is transformed on its own, with fresh transforms. The boundary is
                // intercepted outside the user transforms, so they may filter or reconstruct any public part.
                var transformOptions = new StreamTextTransformOptions(tools, StopStream);
                while (true)
                {
                    pendingBoundary = null;
                    var stream = Gate(cancellation);
                    foreach (var transform in transforms)
                    {
                        stream = transform(transformOptions)(stream, cancellation);
                    }

                    await foreach (
                        var part in stream.WithCancellation(cancellation).ConfigureAwait(false)
                    )
                    {
                        target.Add(part);
                    }

                    if (pendingBoundary != null)
                    {
                        target.Add(pendingBoundary);
                        continue;
                    }

                    break;
                }

                target.Complete();
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                target.Complete();
            }
            catch (Exception exception)
            {
                abort.Stop();
                target.Fail(exception);
            }
            finally
            {
                await sourceReader.DisposeAsync().ConfigureAwait(false);
            }
        });
    }

    private static async Task ProduceAsync(
        ILanguageModel model,
        StreamTextOptions options,
        IAiTelemetry? telemetry,
        SteeringRun? steeringRun,
        PartBuffer buffer,
        StreamAbort abort,
        CallTimeouts? timeouts,
        bool collectContent,
        StreamTextEventProcessor recorder,
        StreamTextErrorHandling errors,
        int streamRetries,
        TaskCompletionSource<IReadOnlyList<ModelMessage>> initialResponseMessagesSource
    )
    {
        // The step loop sees the steps that the event processor recorded from the transformed stream, as upstream.
        var steps = recorder.Steps;
        var abortDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(options);
        if (options.OnAbort is { } || abortDispatcher.OnAbort != null)
        {
            // An abort that follows the consumer's cancellation still reaches onAbort, like upstream's same-tick cancel-then-abort.
            abort.OnLateAbort = async error =>
            {
                var context = new StreamAbortContext(
                    recorder.SnapshotSteps(),
                    error,
                    recorder.CallId
                );
                if (options.OnAbort is { } onAbort)
                {
                    await onAbort(context, CancellationToken.None).ConfigureAwait(false);
                }

                if (abortDispatcher.OnAbort != null)
                {
                    await abortDispatcher
                        .OnAbort(context, CancellationToken.None)
                        .ConfigureAwait(false);
                }
            };
        }
        var cancellationToken = abort.Token;
        // The streamText span and the step span end when these complete.
        TaskCompletionSource<object?>? streamTextSpan = null;
        TaskCompletionSource<object?>? stepSpan = null;
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
            var callId = (options.GenerateCallId ?? NewCallId)();
            var telemetryDispatcher =
                GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                    options
                );
            recorder.CallId = callId;
            streamTextSpan = new TaskCompletionSource<object?>();
            telemetryDispatcher.StartTracingChannelContext(
                TelemetryTracingEventType.StreamText,
                CreateStartEvent(
                    options,
                    callId,
                    model,
                    initialInstructions,
                    initialMessages,
                    Vercel.AI.Operations.OperationRetry.ResolveMaxRetries(options.MaxRetries),
                    "ai.streamText"
                ),
                streamTextSpan.Task
            );
            if (options.OnStart != null)
            {
                await NotifyAsync(
                        () =>
                            options.OnStart(
                                CreateStartEvent(
                                    options,
                                    callId,
                                    model,
                                    initialInstructions,
                                    initialMessages,
                                    Vercel.AI.Operations.OperationRetry.ResolveMaxRetries(
                                        options.MaxRetries
                                    ),
                                    "ai.streamText"
                                ),
                                cancellationToken
                            ),
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }

            if (telemetryDispatcher.OnStart != null)
            {
                await telemetryDispatcher
                    .OnStart(
                        CreateStartEvent(
                            options,
                            callId,
                            model,
                            initialInstructions,
                            initialMessages,
                            Vercel.AI.Operations.OperationRetry.ResolveMaxRetries(
                                options.MaxRetries
                            ),
                            "ai.streamText"
                        ),
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }
            await ProcessInitialApprovalsAsync(
                    options,
                    messages,
                    instructionCount,
                    responseMessages,
                    options.NowMs ?? DefaultNowMs,
                    cancellationToken,
                    buffer.Add,
                    callId
                )
                .ConfigureAwait(false);
            recorder.InitialResponseMessages = responseMessages.ToArray();
            initialResponseMessagesSource.TrySetResult(responseMessages.ToArray());
            var textPartIds = new PartIdReserver();
            var reasoningPartIds = new PartIdReserver();
            // why waiting steering messages are not delivered when the stream ends:
            var pendingDeferredToolCalls = new Dictionary<string, string>();
            var steeringEndReason = SteeringNotDeliveredReason.Finished;
            // A step that a steering interrupt cancelled is made again as the same step:
            StepRedo? redo = null;
            while (steps.Count < HardStepCap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (redo == null)
                {
                    timeouts?.StartStep();
                    stepSpan?.TrySetResult(null);
                    stepSpan = new TaskCompletionSource<object?>();
                    telemetryDispatcher.StartTracingChannelContext(
                        TelemetryTracingEventType.Step,
                        new TracingChannelStepEvent(callId, steps.Count),
                        stepSpan.Task
                    );
                }

                // A model from prepareStep applies to one step only. Maps to `prepareStepResult?.model ?? model`.
                current = model;
                var takenMessages = steeringRun?.TakeMessages() ?? Array.Empty<UserModelMessage>();
                IReadOnlyList<UserModelMessage> steeringMessages = takenMessages;
                if (redo != null)
                {
                    // prepareStep runs again for the redo. It gets the instructions and messages that the step started with,
                    // not the ones that it returned for the cancelled model call:
                    instructions = redo.Instructions;
                    messages = [.. redo.Messages];
                    instructionCount = redo.InstructionCount;
                    current = redo.Model;
                    options = (StreamTextOptions)options.ShallowCopy();
                    options.ToolsContext = redo.ToolsContext;
                    options.RuntimeContext = redo.RuntimeContext;
                    steeringMessages = [.. redo.SteeringMessages, .. takenMessages];
                }

                messages.AddRange(takenMessages);
                var stepStart = new StepRedo(
                    instructions,
                    [.. messages],
                    instructionCount,
                    current,
                    steeringMessages,
                    [],
                    options.ToolsContext,
                    options.RuntimeContext
                );
                var update = PrepareStep(
                    ref options,
                    steps,
                    current,
                    initialInstructions,
                    initialMessages,
                    responseMessages,
                    steeringMessages,
                    ref instructions,
                    ref messages,
                    ref instructionCount
                );
                if (update?.Model != null)
                {
                    current = update.Model;
                }

                // steering messages are delivered when they are in the messages that are sent to the model after prepareStep,
                // and they are streamed before the step output:
                var deliveredSteeringMessages = steeringRun?.ResolveTakenMessages(
                    steps.Count,
                    messages
                ) ?? Array.Empty<DeliveredSteeringMessage>();
                var stepDeliveredMessages = new List<UserModelMessage>(
                    redo?.DeliveredSteeringMessages ?? []
                );
                stepDeliveredMessages.AddRange(
                    deliveredSteeringMessages.Select(delivered => delivered.Message)
                );
                foreach (var delivered in deliveredSteeringMessages)
                {
                    buffer.Add(
                        new SteeringMessagePart(delivered.Id, delivered.Message, steps.Count)
                    );
                }

                // The model gets a snapshot, because the step appends its own response to `messages` afterwards.
                var stepMessages = messages.ToArray();
                var now = options.NowMs ?? DefaultNowMs;
                var stepStartMs = now();
                // The start-step part of a step is streamed once, even when a cancelled model call already streamed it:
                var startStepStreamed = redo?.StartStepStreamed == true;
                var stepStartNotified = redo?.StepStartNotified == true;
                var stepStarted = false;
                var stepResponseId = NewStepResponseId(options);
                var stepResponseTimestamp = (options.CurrentDate ?? DefaultCurrentDate)();
                var stepResponseModelId = current.ModelId;
                var emittedResultIds = new HashSet<string>(StringComparer.Ordinal);
                var text = new StringBuilder();
                var textBlocks = new List<TextBlock>();
                var reasoningBlocks = new List<ReasoningBlock>();
                var contentOrder = new List<object>();
                var toolCalls = new List<GeneratedToolCall>();
                // Everything but text and reasoning blocks goes into contentOrder as it arrives, so the step content keeps the stream order.
                var streamedApprovals = new StreamedApprovals();
                FinishReason? finish = null;
                string? rawFinish = null;
                JsonElement? providerMetadata = null;
                IReadOnlyList<CallWarning>? warnings = null;
                var receivedOutput = false;
                // A model stream that ended with an error or a finish part is complete. Maps to hasReceivedTerminalChunk.
                var receivedTerminal = false;
                var usage = LanguageModelUsage.Null;
                ModelCallPerformance? modelCallPerformance = null;

                var streamOptions = StreamLanguageModelCallOptions.From(options);
                streamOptions.Model = current;
                // The step draws its response id once from options.GenerateId (stepResponseId); the model call must not draw another:
                streamOptions.GenerateId = null;
                streamOptions.PromptMessages = stepMessages;
                streamOptions.PromptInstructions = instructions;
                streamOptions.NowMs = now;
                streamOptions.CallId = callId;
                streamOptions.ToolChoice = update?.ToolChoice ?? options.ToolChoice;
                streamOptions.ToolOrder = update?.ToolOrder ?? options.ToolOrder;
                streamOptions.ActiveTools = update?.ActiveTools ?? options.ActiveTools;
                streamOptions.MaxOutputTokens = update?.MaxOutputTokens ?? options.MaxOutputTokens;
                streamOptions.Temperature = update?.Temperature ?? options.Temperature;
                streamOptions.TopP = update?.TopP ?? options.TopP;
                streamOptions.TopK = update?.TopK ?? options.TopK;
                streamOptions.PresencePenalty = update?.PresencePenalty ?? options.PresencePenalty;
                streamOptions.FrequencyPenalty = update?.FrequencyPenalty
                    ?? options.FrequencyPenalty;
                streamOptions.StopSequences = update?.StopSequences ?? options.StopSequences;
                streamOptions.Seed = update?.Seed ?? options.Seed;
                streamOptions.Reasoning = update?.Reasoning ?? options.Reasoning;
                streamOptions.ProviderOptions = MergeProviderOptions(
                    options.ProviderOptions,
                    update?.ProviderOptions
                );
                streamOptions.IncludeRawChunks = ResolveStreamInclude(options).RawChunks;
                // onStepStart runs once per step with the converted prompt, even when a steering interrupt redoes the model call:
                streamOptions.OnStart = async (_, token) =>
                {
                    if (
                        stepStartNotified
                        || (
                            options.OnStepStart == null
                            && telemetryDispatcher.OnStepStart == null
                            && options.OnStepError == null
                            && telemetryDispatcher.OnStepError == null
                        )
                    )
                    {
                        return;
                    }

                    stepStartNotified = true;
                    var stepStartEvent = new GenerateTextStepStartEvent
                    {
                        CallId = callId,
                        Provider = current.Provider,
                        ModelId = current.ModelId,
                        StepNumber = steps.Count,
                        Instructions = instructions,
                        Messages = stepMessages.Skip(instructionCount).ToArray(),
                        Tools = options.Tools,
                        ToolChoice = streamOptions.ToolChoice,
                        ActiveTools = streamOptions.ActiveTools,
                        ToolOrder = streamOptions.ToolOrder,
                        Steps = steps.ToArray(),
                        ProviderOptions = streamOptions.ProviderOptions,
                        Output = options.Output,
                        ToolsContext = options.ToolsContext,
                        RuntimeContext = options.RuntimeContext,
                    };
                    recorder.OpenStep = stepStartEvent;
                    if (options.OnStepStart != null)
                    {
                        await NotifyAsync(() => options.OnStepStart(stepStartEvent, token), token)
                            .ConfigureAwait(false);
                    }

                    if (telemetryDispatcher.OnStepStart != null)
                    {
                        await telemetryDispatcher
                            .OnStepStart(stepStartEvent, token)
                            .ConfigureAwait(false);
                    }
                };
                using var interrupt =
                    steeringRun == null
                        ? null
                        : new StepInterrupt(
                            steeringRun,
                            timeouts?.Signal ?? options.AbortSignal,
                            cancellationToken
                        );
                streamOptions.CallAbortSignal = interrupt?.Signal
                    ?? timeouts?.Signal
                    ?? options.AbortSignal;
                var attemptToken = interrupt?.Token ?? cancellationToken;

                // A caller abort or a timeout wins over an interrupt: the run ends instead of making the model call again.
                bool IsInterrupted() =>
                    interrupt is { IsRequested: true }
                    && !cancellationToken.IsCancellationRequested
                    && !abort.Stopped
                    && abort.Error == null
                    && timeouts?.Signal.Aborted != true
                    && options.AbortSignal?.Aborted != true;

                StreamLanguageModelCallResult streamResult;
                try
                {
                    streamResult = await LanguageModelCallStreaming
                        .StreamLanguageModelCall(streamOptions, attemptToken)
                        .ConfigureAwait(false);
                }
                catch (Exception) when (IsInterrupted())
                {
                    redo = stepStart with
                    {
                        DeliveredSteeringMessages = stepDeliveredMessages,
                        StartStepStreamed = startStepStreamed || stepStarted,
                        StepStartNotified = stepStartNotified,
                    };
                    continue;
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    // The event processor reports the error to onError.
                    buffer.Add(new ErrorPart(exception.Message) { Error = exception });
                    if (steps.Count == 0)
                    {
                        // The event processor rejects the results, because no step was recorded.
                        if (!abort.TryFinish())
                        {
                            throw abort.Error!;
                        }

                        buffer.Complete();
                        return;
                    }

                    break;
                }

                LanguageModelRequestMetadata RequestMetadata(
                    LanguageModelStreamRequest? modelRequest
                ) =>
                    new()
                    {
                        Url = RequestOrigin.SanitizeRequestMetadata(modelRequest)?.Url,
                        Body = ResolveStreamInclude(options).RequestBody
                            ? modelRequest?.Body
                            : null,
                        RedactBody = modelRequest?.RedactBody,
                        Messages = ResolveStreamInclude(options).RequestMessages
                            ? stepMessages
                            : null,
                    };

                var stepRequest = RequestMetadata(streamResult.Request);
                // The start-step part of a step is streamed once. A model call that follows a cancelled one sends a boundary instead,
                // which replaces what the step recorded for the cancelled call, such as its request and warnings.
                TextStreamPart StepStart(IReadOnlyList<CallWarning>? stepWarnings) =>
                    startStepStreamed
                        ? new StreamRetryBoundaryPart(stepRequest, stepWarnings)
                        : new StartStepPart(stepWarnings, stepRequest);

                // executeToolsFromStream decides the approvals as the calls arrive and runs the tools at the end of the model call.
                // With streamRetries, a provider error part reruns the model call of this step. The tool parts of an attempt
                // wait until it ends.
                var retry =
                    options.StreamRetries == null
                        ? null
                        : new StreamTextRetry(
                            options,
                            abort,
                            streamResult,
                            streamRetries,
                            errors,
                            retryToken =>
                                LanguageModelCallStreaming.StreamLanguageModelCall(
                                    streamOptions,
                                    retryToken
                                )
                        );
                var enumerator = ExecuteToolsFromStream
                    .Execute(
                        InvokeToolCallbacksFromStream.Invoke(
                            retry == null ? streamResult.Stream : retry.Read(attemptToken),
                            StepOptions(options, update).Tools,
                            stepMessages.Skip(instructionCount).ToArray(),
                            cancellationToken,
                            options.ToolsContext,
                            attemptToken
                        ),
                        StepOptions(options, update),
                        callId,
                        stepMessages,
                        now,
                        instructionCount,
                        cancellationToken
                    )
                    .GetAsyncEnumerator(attemptToken);
                var disposal = abort.Track(streamResult.Disposal);
                var nextPending = false;
                // The model call can only be interrupted until it produces output:
                var interruptWindowClosed = interrupt == null;
                var attemptInterrupted = false;
                try
                {
                    timeouts?.StartFirstChunk();
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // A slow consumer pauses the pull of the model stream, like upstream's highWaterMark 0 pull model.
                        await buffer.WaitForRoomAsync(cancellationToken).ConfigureAwait(false);

                        // A model stream can ignore the token, so race each read against the abort and the interrupt.
                        var next = enumerator.MoveNextAsync().AsTask();
                        nextPending = true;
                        var raced = interruptWindowClosed
                            ? await Task.WhenAny(next, abort.Signal).ConfigureAwait(false)
                            : await Task.WhenAny(next, abort.Signal, interrupt!.Requested)
                                .ConfigureAwait(false);
                        if (raced != next)
                        {
                            _ = next.ContinueWith(
                                task => _ = task.Exception,
                                TaskContinuationOptions.OnlyOnFaulted
                            );
                            if (raced == interrupt?.Requested && IsInterrupted())
                            {
                                // The interrupt cancelled the read. A stream cannot be disposed while it is read, so the read gets
                                // a moment to end:
                                if (
                                    await Task.WhenAny(next, Task.Delay(InterruptReadTimeout))
                                        .ConfigureAwait(false) == next
                                )
                                {
                                    nextPending = false;
                                }

                                attemptInterrupted = true;
                                break;
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        var hasNext = await next.ConfigureAwait(false);
                        nextPending = false;
                        if (!hasNext)
                        {
                            break;
                        }

                        if (!interruptWindowClosed && IsInterrupted())
                        {
                            attemptInterrupted = true;
                            break;
                        }

                        var part = enumerator.Current;
                        // A tool input start is not output, but it ends the window as well, because a UI already shows a pending tool call for it.
                        if (
                            !interruptWindowClosed
                            && (
                                IsOutputPart(part)
                                || part is ToolInputStartPart
                                || part is ModelCallEndPart
                            )
                        )
                        {
                            interruptWindowClosed = true;
                            if (!interrupt!.TryClose())
                            {
                                // An interrupt cancelled the model call as the first output arrived. The output is discarded.
                                attemptInterrupted = true;
                                break;
                            }
                        }

                        if (timeouts != null)
                        {
                            ObserveChunkTimeout(timeouts, part);
                        }

                        if (IsOutputPart(part))
                        {
                            receivedOutput = true;
                        }

                        if (
                            !stepStarted
                            && part is not (ModelCallStartPart or StreamRetryAttemptBoundaryPart)
                        )
                        {
                            stepStarted = true;
                            buffer.Add(StepStart(null));
                        }

                        switch (part)
                        {
                            case TextStartPart textStart:
                                var startedText = TextBlock.Start(
                                    textBlocks,
                                    textStart.Id,
                                    textPartIds,
                                    contentOrder
                                );
                                startedText.ProviderMetadata = textStart.ProviderMetadata;
                                buffer.Add(
                                    new TextStartPart(
                                        startedText.PartId,
                                        textStart.ProviderMetadata
                                    )
                                );
                                break;
                            case TextDeltaPart delta:
                                var activeText = TextBlock.Find(
                                    textBlocks,
                                    delta.Id,
                                    textPartIds,
                                    buffer,
                                    contentOrder
                                );
                                if (collectContent)
                                {
                                    text.Append(delta.Text);
                                    activeText.Text.Append(delta.Text);
                                }

                                if (delta.Text.Length > 0 || delta.ProviderMetadata != null)
                                {
                                    buffer.Add(
                                        new TextDeltaPart(
                                            activeText.PartId,
                                            delta.Text,
                                            delta.ProviderMetadata
                                        )
                                    );
                                }

                                break;
                            case TextEndPart textEnd:
                                var ended = TextBlock.Find(
                                    textBlocks,
                                    textEnd.Id,
                                    textPartIds,
                                    buffer,
                                    contentOrder
                                );
                                ended.Citations = textEnd.Citations;
                                ended.ProviderMetadata = textEnd.ProviderMetadata
                                    ?? ended.ProviderMetadata;
                                ended.Open = false;
                                buffer.Add(
                                    new TextEndPart(
                                        ended.PartId,
                                        textEnd.Citations,
                                        textEnd.ProviderMetadata
                                    )
                                );
                                break;
                            case DocumentSourcePart documentSource:
                                contentOrder.Add(documentSource.Source);
                                buffer.Add(documentSource);
                                break;
                            case ReasoningStartPart reasoningStart:
                                var startedReasoning = ReasoningBlock.Start(
                                    reasoningBlocks,
                                    reasoningStart.Id,
                                    reasoningPartIds,
                                    contentOrder
                                );
                                startedReasoning.ProviderMetadata = reasoningStart.ProviderMetadata;
                                buffer.Add(
                                    new ReasoningStartPart(
                                        startedReasoning.PartId,
                                        reasoningStart.ProviderMetadata
                                    )
                                );
                                break;
                            case ReasoningEndPart reasoningEnd:
                                var endedReasoning = ReasoningBlock.Find(
                                    reasoningBlocks,
                                    reasoningEnd.Id,
                                    reasoningPartIds,
                                    buffer,
                                    contentOrder
                                );
                                endedReasoning.ProviderMetadata = reasoningEnd.ProviderMetadata
                                    ?? endedReasoning.ProviderMetadata;
                                endedReasoning.Open = false;
                                buffer.Add(
                                    new ReasoningEndPart(
                                        endedReasoning.PartId,
                                        reasoningEnd.ProviderMetadata
                                    )
                                );
                                break;
                            case ReasoningDeltaPart reasoningDelta:
                                var activeReasoning = ReasoningBlock.Find(
                                    reasoningBlocks,
                                    reasoningDelta.Id,
                                    reasoningPartIds,
                                    buffer,
                                    contentOrder
                                );
                                if (collectContent)
                                {
                                    activeReasoning.Text.Append(reasoningDelta.Text);
                                }

                                activeReasoning.ProviderMetadata = reasoningDelta.ProviderMetadata
                                    ?? activeReasoning.ProviderMetadata;
                                buffer.Add(
                                    new ReasoningDeltaPart(
                                        activeReasoning.PartId,
                                        reasoningDelta.Text,
                                        reasoningDelta.ProviderMetadata
                                    )
                                );
                                break;
                            case ToolCallPart toolCallPart:
                                toolCalls.Add(toolCallPart.ToolCall);
                                contentOrder.Add(toolCallPart.ToolCall);
                                buffer.Add(toolCallPart);
                                break;
                            case ToolApprovalRequestPart approvalRequest:
                                contentOrder.Add(
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
                                buffer.Add(approvalRequest);
                                break;
                            case ToolApprovalResponsePart approvalResponse:
                                contentOrder.Add(
                                    new GeneratedToolApprovalResponse(
                                        approvalResponse.ApprovalId,
                                        approvalResponse.ToolCall,
                                        approvalResponse.Approved,
                                        approvalResponse.Reason,
                                        approvalResponse.ProviderExecuted
                                    )
                                );
                                buffer.Add(approvalResponse);
                                break;
                            case ToolOutputDeniedPart outputDenied:
                                buffer.Add(outputDenied);
                                break;
                            case ExecuteToolsFromStream.ToolExecutionEndPart executionEnd:
                                streamedApprovals.ToolExecutionMs[executionEnd.ToolCallId] =
                                    executionEnd.ToolExecutionMs;
                                break;
                            case ToolResultPart { Result.Preliminary: true } preliminaryResult:
                                buffer.Add(preliminaryResult);
                                break;
                            case ToolResultPart executedResult
                                when !executedResult.Result.ProviderExecuted:
                                streamedApprovals.Executed.Add(executedResult.Result);
                                emittedResultIds.Add(executedResult.Result.ToolCallId);
                                buffer.Add(executedResult);
                                break;
                            case ToolResultPart { Result.ProviderExecuted: true } providerResult:
                                if (providerResult.Result.Preliminary != true)
                                {
                                    RecordProviderResult(providerResult.Result);
                                }

                                buffer.Add(providerResult);
                                break;
                            case ToolErrorPart { Result.ProviderExecuted: true } providerError:
                                RecordProviderResult(providerError.Result);
                                buffer.Add(providerError);
                                break;
                            case ToolErrorPart executedError
                                when streamedApprovals.ToolExecutionMs.ContainsKey(
                                    executedError.Result.ToolCallId
                                ):
                                streamedApprovals.Executed.Add(executedError.Result);
                                emittedResultIds.Add(executedError.Result.ToolCallId);
                                buffer.Add(executedError);
                                break;
                            case ToolErrorPart invalidCallError:
                                emittedResultIds.Add(invalidCallError.Result.ToolCallId);
                                streamedApprovals.InlineResultIds.Add(
                                    invalidCallError.Result.ToolCallId
                                );
                                contentOrder.Add(ToGeneratedToolResult(invalidCallError.Result));
                                buffer.Add(invalidCallError);
                                break;
                            case ToolInputStartPart
                            or ToolInputDeltaPart
                            or ToolInputEndPart
                            or SourcePart
                            or RawPart
                            or PingPart
                            or CustomPart:
                                if (part is SourcePart sourcePart)
                                {
                                    contentOrder.Add(sourcePart.Source);
                                }

                                buffer.Add(part);
                                break;
                            case FilePart filePart:
                                contentOrder.Add(filePart.File);
                                buffer.Add(filePart);
                                break;
                            case ReasoningFilePart reasoningFilePart:
                                contentOrder.Add(
                                    new GeneratedReasoningFile(
                                        reasoningFilePart.File.Data,
                                        reasoningFilePart.File.MediaType,
                                        reasoningFilePart.ProviderMetadata
                                    )
                                );
                                buffer.Add(reasoningFilePart);
                                break;
                            case ModelCallEndPart modelCallEnd:
                                receivedTerminal = true;
                                finish = modelCallEnd.FinishReason;
                                rawFinish = modelCallEnd.RawFinishReason;
                                usage = modelCallEnd.Usage;
                                providerMetadata = modelCallEnd.ProviderMetadata;
                                modelCallPerformance = modelCallEnd.Performance;
                                break;
                            case ErrorPart { Error: ToolChoiceViolationException } violationPart:
                                // Like upstream, the violation is an error part of the stream. The step finishes with the error finish reason and its usage.
                                receivedTerminal = true;
                                buffer.Add(violationPart);
                                break;
                            case ErrorPart error:
                                receivedTerminal = true;
                                finish = FinishReason.Error;
                                buffer.Add(error);
                                break;
                            case ModelCallResponseMetadataPart responseMetadata:
                                stepResponseId = responseMetadata.Id ?? stepResponseId;
                                stepResponseTimestamp = responseMetadata.Timestamp
                                    ?? stepResponseTimestamp;
                                stepResponseModelId = responseMetadata.ModelId
                                    ?? stepResponseModelId;
                                break;
                            case StreamRetryAttemptBoundaryPart attemptBoundary:
                                // The model call was rerun. What the failed attempt produced does not belong to the step.
                                text.Clear();
                                textBlocks.Clear();
                                reasoningBlocks.Clear();
                                contentOrder.Clear();
                                toolCalls.Clear();
                                emittedResultIds.Clear();
                                streamedApprovals = new StreamedApprovals();
                                finish = null;
                                rawFinish = null;
                                providerMetadata = null;
                                receivedOutput = false;
                                receivedTerminal = false;
                                usage = LanguageModelUsage.Null;
                                modelCallPerformance = null;
                                stepResponseId = NewStepResponseId(options);
                                stepResponseTimestamp = (
                                    options.CurrentDate ?? DefaultCurrentDate
                                )();
                                stepResponseModelId = current.ModelId;
                                warnings = attemptBoundary.Warnings;
                                // The retried attempt gets its own timeout windows.
                                timeouts?.ClearModelOutput();
                                timeouts?.StartFirstChunk();
                                stepRequest = RequestMetadata(retry?.Current.Request);
                                buffer.Add(new StreamRetryBoundaryPart(stepRequest, warnings));
                                break;
                            case ModelCallStartPart start:
                                warnings = start.Warnings;
                                if (!stepStarted)
                                {
                                    stepStarted = true;
                                    buffer.Add(StepStart(start.Warnings));
                                }

                                break;
                        }

                        void RecordProviderResult(ExecutedTool result)
                        {
                            contentOrder.Add(
                                new GeneratedToolResult(
                                    result.ToolCallId,
                                    result.ToolName,
                                    result.OutputJson,
                                    result.IsError
                                )
                                {
                                    ProviderMetadata = result.ProviderMetadata,
                                }
                            );
                            emittedResultIds.Add(result.ToolCallId);
                        }
                    }
                }
                catch (Exception) when (!interruptWindowClosed && IsInterrupted())
                {
                    attemptInterrupted = true;
                }
                finally
                {
                    timeouts?.ClearModelOutput();
                    try
                    {
                        await abort
                            .DisposeAsync(retry?.Current.Disposal ?? disposal)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        // A read an abort left pending cannot be disposed; the provider disposal above already ended it.
                        if (!nextPending)
                        {
                            await enumerator.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                var responseTimeMs = modelCallPerformance?.ResponseTimeMs ?? now() - stepStartMs;

                // Providers that skip text-end or reasoning-end still produce a closed block in the stream.
                foreach (var openText in textBlocks)
                {
                    if (openText.Open)
                    {
                        openText.Open = false;
                        buffer.Add(
                            new TextEndPart(
                                openText.PartId,
                                openText.Citations,
                                openText.ProviderMetadata
                            )
                        );
                    }
                }

                foreach (var openReasoning in reasoningBlocks)
                {
                    if (openReasoning.Open)
                    {
                        openReasoning.Open = false;
                        buffer.Add(
                            new ReasoningEndPart(
                                openReasoning.PartId,
                                openReasoning.ProviderMetadata
                            )
                        );
                    }
                }

                // The step is not finished, its model call is made again:
                if (attemptInterrupted)
                {
                    redo = stepStart with
                    {
                        DeliveredSteeringMessages = stepDeliveredMessages,
                        StartStepStreamed = startStepStreamed || stepStarted,
                        StepStartNotified = stepStartNotified,
                    };
                    continue;
                }

                redo = null;
                if (!stepStarted)
                {
                    buffer.Add(StepStart(null));
                }

                // A stream that ends without a finish part and without output is rejected instead of recorded as an empty step.
                // An incomplete stream with partial output keeps the partial result.
                if (!receivedTerminal && !receivedOutput)
                {
                    const string noOutputMessage =
                        "No output generated. The model stream ended without a finish chunk.";
                    // The event processor reports the error and rejects the results.
                    var noOutputError = new NoOutputGeneratedException(noOutputMessage);
                    buffer.Add(new ErrorPart(noOutputMessage) { Error = noOutputError });
                    steeringRun?.End(SteeringNotDeliveredReason.Error);
                    if (!abort.TryFinish())
                    {
                        throw abort.Error!;
                    }

                    buffer.Complete();
                    return;
                }

                var generated = new LanguageModelGenerateResult(
                    BuildContent(contentOrder),
                    finish ?? FinishReason.Other,
                    usage,
                    rawFinish,
                    warnings,
                    providerMetadata: providerMetadata
                );
                var stepResponse = new LanguageModelResponseMetadata(
                    stepResponseId,
                    stepResponseTimestamp,
                    stepResponseModelId,
                    (retry?.Current ?? streamResult).Response?.Headers
                );
                // The step that the event processor records from the transformed parts replaces this one. This one carries
                // the metadata of the model call, and its messages are not used.
                var step = await FinishStepAsync(
                        generated,
                        StepOptions(options, update),
                        [.. messages],
                        instructionCount,
                        [],
                        current,
                        cancellationToken,
                        new StepTiming(now, stepStartMs, responseTimeMs, Array.Empty<double>())
                        {
                            ModelCallPerformance = modelCallPerformance,
                        },
                        stepRequest,
                        stepResponse,
                        null,
                        streamedApprovals,
                        stepDeliveredMessages,
                        null,
                        callId,
                        steps.Count,
                        resolveFiles: false
                    )
                    .ConfigureAwait(false);
                foreach (var toolResult in step.ToolResults)
                {
                    if (emittedResultIds.Contains(toolResult.ToolCallId))
                    {
                        continue;
                    }

                    buffer.Add(
                        toolResult.IsError
                            ? new ToolErrorPart(toolResult)
                            : new ToolResultPart(toolResult)
                    );
                }

                buffer.Add(new StepFinishPart(step, stepResponse, rawFinish));

                // The next step needs the step as the event processor recorded it from the transformed stream.
                if (!await recorder.WaitForStepAsync(steps.Count + 1).ConfigureAwait(false))
                {
                    // A transform stopped the stream before the step finished.
                    cancellationToken.ThrowIfCancellationRequested();
                    break;
                }

                step = steps[steps.Count - 1];
                messages.AddRange(step.Response.Messages);
                responseMessages.AddRange(step.Response.Messages);
                TrackPendingDeferredToolCalls(step, options.Tools, pendingDeferredToolCalls);
                if (
                    !ShouldStartNextStep(
                        step,
                        steps,
                        stop,
                        steeringRun,
                        pendingDeferredToolCalls,
                        ref steeringEndReason
                    )
                )
                {
                    break;
                }
            }

            steeringRun?.End(steeringEndReason);
            timeouts?.EndStep();
            // The finish part carries the total usage and the finish reason of the last step. The results are settled
            // by the event processor, from the part as the transforms left it.
            var totalUsage = LanguageModelUsage.Null;
            foreach (var recordedStep in steps)
            {
                totalUsage = LanguageModelUsage.Add(totalUsage, recordedStep.Usage);
            }

            var finalFinishReason =
                steps.Count == 0 ? FinishReason.Other : steps[steps.Count - 1].FinishReason;
            if (telemetry != null && scope != null)
            {
                telemetry.OnUsage(scope, totalUsage);
                telemetry.OnFinish(scope, finalFinishReason);
            }

            if (!abort.TryFinish())
            {
                throw abort.Error!;
            }

            buffer.Add(new FinishPart(finalFinishReason, totalUsage));
            buffer.Complete();
        }
        catch (Exception exception)
        {
            stepSpan?.TrySetException(exception);
            streamTextSpan?.TrySetException(exception);
            abort.TryFinish();
            steeringRun?.End(
                abort.Stopped || abort.Error != null
                    ? SteeringNotDeliveredReason.Aborted
                    : SteeringNotDeliveredReason.Error
            );
            if (abort.Stopped)
            {
                // The consumer stopped reading. The event processor cancels the results.
                buffer.Complete();
                return;
            }

            if (abort.Error is { } abortError)
            {
                await FinishAbortAsync(
                        options,
                        buffer,
                        recorder.SnapshotSteps(),
                        abortError,
                        recorder.CallId
                    )
                    .ConfigureAwait(false);
                return;
            }

            // The event processor settles the results and reports the error to onError.
            buffer.Fail(exception);
        }
        finally
        {
            stepSpan?.TrySetResult(null);
            streamTextSpan?.TrySetResult(null);
            // A stream that ends before the initial approvals settle has no initial response messages.
            initialResponseMessagesSource.TrySetResult(Array.Empty<ModelMessage>());
            timeouts?.Dispose();
            abort.Registration.Dispose();
            abort.CleanupSignal();
            abort.Dispose();
        }
    }

    // How long an interrupt waits for the read of the cancelled model stream to end before it disposes the stream.
    private static readonly TimeSpan InterruptReadTimeout = TimeSpan.FromSeconds(5);

    // Parts that carry model output. Tells an empty incomplete stream from an incomplete stream with a partial result.
    private static bool IsOutputPart(TextStreamPart part)
    {
        return part switch
        {
            TextDeltaPart { Text.Length: > 0 } => true,
            ReasoningDeltaPart { Text.Length: > 0 } => true,
            ToolInputDeltaPart { Delta.Length: > 0 } => true,
            ToolCallPart => true,
            FilePart => true,
            ReasoningFilePart => true,
            _ => false,
        };
    }

    // A chunk of model output starts the chunkMs timeout again. A chunk without output leaves it as it is.
    // The timeout ends with the model response, so a finish part ends it before the next step or a slow consumer can trip it.
    private static void ObserveChunkTimeout(CallTimeouts timeouts, TextStreamPart part)
    {
        switch (part)
        {
            case TextDeltaPart { Text.Length: > 0 }:
            case ReasoningDeltaPart { Text.Length: > 0 }:
            case ToolInputDeltaPart { Delta.Length: > 0 }:
            case ToolCallPart:
                timeouts.OnOutputChunk();
                break;
            case ModelCallEndPart:
            case ErrorPart:
                timeouts.ClearModelOutput();
                break;
        }
    }

    // The result tasks are already faulted when the abort fires. This reports the abort, then closes the stream.
    // Like upstream, the stream ends only after OnAbort returns, so a reader that sees the end knows OnAbort ran.
    private static async Task FinishAbortAsync(
        StreamTextOptions options,
        PartBuffer buffer,
        IReadOnlyList<StepResult> steps,
        Exception error,
        string callId
    )
    {
        Exception? callbackError = null;
        var telemetryOnAbort = GenerateTextRestrictedTelemetryDispatcher
            .CreateRestrictedTelemetryDispatcher(options)
            .OnAbort;
        if (options.OnAbort != null || telemetryOnAbort != null)
        {
            try
            {
                var context = new StreamAbortContext(steps.ToArray(), error, callId);
                if (options.OnAbort != null)
                {
                    await options.OnAbort(context, CancellationToken.None).ConfigureAwait(false);
                }

                if (telemetryOnAbort != null)
                {
                    await telemetryOnAbort(context, CancellationToken.None).ConfigureAwait(false);
                }
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

    private static List<GeneratedContent> BuildContent(List<object> contentOrder)
    {
        var content = new List<GeneratedContent>();
        foreach (var entry in contentOrder)
        {
            switch (entry)
            {
                case ReasoningBlock block:
                    content.Add(
                        new GeneratedReasoning(block.Text.ToString(), block.ProviderMetadata)
                    );
                    break;
                case TextBlock block
                    when block.Text.Length > 0 || (block.Citations?.Count ?? 0) > 0:
                    content.Add(
                        new GeneratedText(
                            block.Text.ToString(),
                            block.Citations,
                            block.ProviderMetadata
                        )
                    );
                    break;
                case GeneratedContent part:
                    content.Add(part);
                    break;
            }
        }

        return content;
    }

    // One streamed text block. Citations arrive with its text-end part.
    private sealed class TextBlock
    {
        private TextBlock(string id, string partId)
        {
            Id = id;
            PartId = partId;
        }

        public string Id { get; }

        // The id of the block in the stream parts. It is unique across the steps of a stream.
        public string PartId { get; }

        public bool Open { get; set; } = true;

        public StringBuilder Text { get; } = new();

        public IReadOnlyList<Citation>? Citations { get; set; }

        public JsonElement? ProviderMetadata { get; set; }

        public static TextBlock Start(
            List<TextBlock> blocks,
            string id,
            PartIdReserver reserver,
            List<object> contentOrder
        )
        {
            var block = new TextBlock(id, reserver.Reserve(id));
            blocks.Add(block);
            contentOrder.Add(block);
            return block;
        }

        // Deltas and ends belong to the latest block with their id. Providers that skip text-start get one here, with its text-start part.
        public static TextBlock Find(
            List<TextBlock> blocks,
            string id,
            PartIdReserver reserver,
            PartBuffer buffer,
            List<object> contentOrder
        )
        {
            for (var i = blocks.Count - 1; i >= 0; i--)
            {
                if (blocks[i].Id == id)
                {
                    return blocks[i];
                }
            }

            var block = Start(blocks, id, reserver, contentOrder);
            buffer.Add(new TextStartPart(block.PartId));
            return block;
        }
    }

    // One streamed reasoning block. The latest provider metadata of its start, delta, and end parts wins, as upstream.
    private sealed class ReasoningBlock
    {
        private ReasoningBlock(string id, string partId)
        {
            Id = id;
            PartId = partId;
        }

        public string Id { get; }

        // The id of the block in the stream parts. It is unique across the steps of a stream.
        public string PartId { get; }

        public bool Open { get; set; } = true;

        public StringBuilder Text { get; } = new();

        public JsonElement? ProviderMetadata { get; set; }

        public static ReasoningBlock Start(
            List<ReasoningBlock> blocks,
            string id,
            PartIdReserver reserver,
            List<object> contentOrder
        )
        {
            var block = new ReasoningBlock(id, reserver.Reserve(id));
            blocks.Add(block);
            contentOrder.Add(block);
            return block;
        }

        // Deltas and ends belong to the latest block with their id. Providers that skip reasoning-start get one here, with its reasoning-start part.
        public static ReasoningBlock Find(
            List<ReasoningBlock> blocks,
            string id,
            PartIdReserver reserver,
            PartBuffer buffer,
            List<object> contentOrder
        )
        {
            for (var i = blocks.Count - 1; i >= 0; i--)
            {
                if (blocks[i].Id == id)
                {
                    return blocks[i];
                }
            }

            var block = Start(blocks, id, reserver, contentOrder);
            buffer.Add(new ReasoningStartPart(block.PartId));
            return block;
        }
    }

    // Maps the block ids of a stream to ids that are unique across its steps. Maps to createPartIdReserver.
    private sealed class PartIdReserver
    {
        private readonly HashSet<string> _usedIds = new(StringComparer.Ordinal);

        public string Reserve(string id)
        {
            if (_usedIds.Add(id))
            {
                return id;
            }

            var generatedId = Vercel.AI.ProviderUtils.JsonValues.GenerateId("id-");
            var uniqueId = generatedId;
            var suffix = 0;
            while (!_usedIds.Add(uniqueId))
            {
                uniqueId =
                    generatedId
                    + "-"
                    + (++suffix).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return uniqueId;
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
    internal static IReadOnlyDictionary<string, JsonElement>? MergeProviderOptions(
        IReadOnlyDictionary<string, JsonElement>? options,
        IReadOnlyDictionary<string, JsonElement>? step
    )
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
            merged[pair.Key] = merged.TryGetValue(pair.Key, out var current)
                ? MergeJson(current, pair.Value)
                : pair.Value;
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
            merged[property.Name] = merged.TryGetValue(property.Name, out var value)
                ? MergeJson(value, property.Value)
                : property.Value;
        }

        return JsonSerializer.SerializeToElement(merged);
    }

    internal static ExecutedTool ProviderExecutedResult(
        GeneratedToolResult result,
        IReadOnlyList<GeneratedToolCall> toolCalls,
        bool? preliminary = null
    )
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

        return new ExecutedTool(
            result.ToolCallId,
            result.ToolName,
            result.ResultJson,
            result.IsError
        )
        {
            ProviderExecuted = true,
            ProviderMetadata = result.ProviderMetadata,
            InputJson = input,
            Dynamic = matched?.Dynamic ?? false,
            Title = matched?.Title,
            ToolMetadata = matched?.ToolMetadata,
            Preliminary = preliminary,
        };
    }

    internal static IReadOnlyDictionary<string, JsonElement>? ToProviderOptions(
        JsonElement? metadata
    )
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

    // Maps to resolveGeneratedFileData. A generated file has no model to pass the URL through to, so a null result from a custom
    // download function cannot be honored.
    internal static async Task<GeneratedFile> ResolveGeneratedFileDataAsync(
        GeneratedFileUrl remote,
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? download,
        CancellationToken cancellationToken
    )
    {
        if (download is null)
        {
            DownloadUrls.ValidateDownloadUrl(remote.Url);
            var downloaded = await Download
                .GetAsync(remote.Url, null, cancellationToken)
                .ConfigureAwait(false);
            return new GeneratedFile(downloaded.Data, remote.MediaType);
        }

        var results = await download(
                new[]
                {
                    new DownloadRequestItem(remote.Url, false) { AbortSignal = cancellationToken },
                }
            )
            .ConfigureAwait(false);
        var result = results.Count == 0 ? null : results[0];
        if (result is null)
        {
            throw new DownloadException(
                remote.Url.ToString(),
                "Generated file URL "
                    + remote.Url
                    + " must be downloaded, but the download function returned null"
            );
        }

        return new GeneratedFile(result.Data, remote.MediaType);
    }

    private static async Task<StepResult> FinishStepAsync(
        LanguageModelGenerateResult generated,
        GenerateTextOptions options,
        List<ModelMessage> messages,
        int instructionCount,
        List<ModelMessage> responseMessages,
        ILanguageModel model,
        CancellationToken cancellationToken,
        StepTiming timing,
        LanguageModelRequestMetadata stepRequest,
        LanguageModelResponseMetadata responseInfo,
        JsonElement? responseBody,
        StreamedApprovals? streamed = null,
        IReadOnlyList<UserModelMessage>? steeringMessages = null,
        Action<ExecutedTool>? onPreliminaryToolResult = null,
        string? callId = null,
        int stepNumber = 0,
        bool resolveFiles = true
    )
    {
        var toolExecutionMs = new Dictionary<string, double>(StringComparer.Ordinal);
        var toolCalls = new List<GeneratedToolCall>();
        var content = new List<GeneratedContent>();
        var sources = new List<GeneratedSource>();
        var files = new List<GeneratedFile>();
        var reasoningParts = new List<string>();
        var reasoningMetadata = new List<JsonElement?>();
        foreach (var part in generated.Content)
        {
            var resolved = part switch
            {
                // A request that streamText decided as its call arrived carries its call already.
                GeneratedToolApprovalRequest { ToolCall: null } request => ResolveApprovalRequest(
                    request,
                    generated.Content
                ),
                GeneratedToolCall toolCall => ParseProviderExecutedInput(options, toolCall),
                GeneratedFileUrl remote when resolveFiles => await ResolveGeneratedFileDataAsync(
                        remote,
                        options.ExperimentalDownload,
                        cancellationToken
                    )
                    .ConfigureAwait(false),
                _ => part,
            };
            content.Add(resolved);
            switch (resolved)
            {
                case GeneratedToolCall call:
                    toolCalls.Add(call);
                    break;
                case GeneratedToolResult:
                    break;
                case GeneratedSource source:
                    sources.Add(source);
                    break;
                case GeneratedReasoning generatedReasoning:
                    reasoningParts.Add(generatedReasoning.Text);
                    reasoningMetadata.Add(generatedReasoning.ProviderMetadata);
                    break;
                case GeneratedFile file:
                    files.Add(file);
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
        var toolCallsCallId = callId ?? NewCallId();

        // Each valid call to a known tool gets its approval status. A call that needs a user decision waits for it, and
        // an automatic denial settles the call without running it. A streamed step decided them as the calls arrived.
        var (approvalRequests, approvalResponses, blockedToolCallIds) =
            streamed != null
                ? (
                    new List<GeneratedToolApprovalRequest>(),
                    new List<GeneratedToolApprovalResponse>(),
                    new HashSet<string>(StringComparer.Ordinal)
                )
                : await ResolveStepApprovalsAsync(
                        options,
                        toolCalls,
                        stepInputMessages.Skip(instructionCount).ToArray(),
                        stepInputMessages,
                        cancellationToken
                    )
                    .ConfigureAwait(false);

        if (ToolExecution.IsToolExecutionAllowedFinishReason(generated.FinishReason))
        {
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
                    }

                    continue;
                }

                // A streamed step ran its tools when the model call ended.
                if (streamed != null)
                {
                    continue;
                }

                var tool = FindTool(options, call.ToolName);

                // A tool without execute is for the caller to run: the step has the call and no result.
                if (
                    call.ProviderExecuted
                    || blockedToolCallIds.Contains(call.ToolCallId)
                    || (tool != null && !HasExecutor(tool))
                )
                {
                    continue;
                }

                callsToExecute.Add(call);
            }

            var executions = await ToolCallConcurrencyScheduler
                .MapWithConcurrency(
                    callsToExecute,
                    ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(
                        options.ToolCallConcurrency
                    ),
                    async (call, _) =>
                    {
                        var result = await ToolCallExecution
                            .ExecuteToolCall(
                                call,
                                options.Tools,
                                options.ToolsContext,
                                toolCallsCallId,
                                stepInputMessages.Skip(instructionCount).ToArray(),
                                cancellationToken,
                                timing.Now,
                                options.Timeout,
                                options.ExperimentalSandbox,
                                onPreliminaryToolResult,
                                ToolExecutionCallbacks.Start(options),
                                ToolExecutionCallbacks.End(options),
                                ToolExecutionCallbacks.ExecuteInTelemetryContext(
                                    options,
                                    cancellationToken
                                ),
                                ToolExecutionCallbacks.RunInTracingChannelSpan(options)
                            )
                            .ConfigureAwait(false);
                        return result;
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
            foreach (
                var (executed, elapsedMs) in executions
                    .Where(result => result != null)
                    .Select(result => result!.Value)
            )
            {
                toolExecutionMs[executed.ToolCallId] = elapsedMs;

                toolResults.Add(executed);
            }
        }

        if (streamed != null)
        {
            foreach (var elapsed in streamed.ToolExecutionMs)
            {
                toolExecutionMs[elapsed.Key] = elapsed.Value;
            }

            toolResults.AddRange(streamed.Executed);

            // A streamed step keeps the stream order: the approvals and the results of the provider are in the content already,
            // and the results of the tools this loop ran follow it.
            foreach (var executed in toolResults)
            {
                if (
                    !executed.ProviderExecuted
                    && !streamed.InlineResultIds.Contains(executed.ToolCallId)
                )
                {
                    content.Add(ToGeneratedToolResult(executed));
                }
            }
        }
        else
        {
            // The results of the tools this loop ran follow the model's own content, as upstream lists them: first the results of
            // calls without an approval response, then the approval requests and responses, then the results of the others.
            var answeredToolCallIds = new HashSet<string>(
                approvalResponses.Select(response => response.ToolCall.ToolCallId),
                StringComparer.Ordinal
            );
            foreach (var answered in new[] { false, true })
            {
                foreach (var executed in toolResults)
                {
                    if (
                        !executed.ProviderExecuted
                        && answeredToolCallIds.Contains(executed.ToolCallId) == answered
                    )
                    {
                        content.Add(ToGeneratedToolResult(executed));
                    }
                }

                if (!answered)
                {
                    content.AddRange(approvalRequests);
                    content.AddRange(approvalResponses);
                }
            }
        }

        // The assistant message and the tool message are built from the content, as upstream does.
        var stepResponseStart = responseMessages.Count;
        var stepMessages = ToResponseMessages.Convert(content, options.Tools);
        if (!ToolExecution.IsToolExecutionAllowedFinishReason(generated.FinishReason))
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

        foreach (var stepMessage in stepMessages)
        {
            messages.Add(stepMessage);
            responseMessages.Add(stepMessage);
        }

        var response = new StepResponse(
            responseInfo.Id,
            responseInfo.Timestamp,
            responseInfo.ModelId,
            responseInfo.Headers,
            responseMessages.Skip(stepResponseStart).ToArray(),
            responseBody
        )
        {
            RedactBody = generated.RedactRawResponse,
        };
        var performance =
            timing.ModelCallPerformance != null
                ? StepPerformanceCalculator.Create(
                    timing.Now() - timing.StartMs,
                    timing.ModelCallPerformance,
                    toolExecutionMs
                )
                : StepPerformanceCalculator.Create(
                    timing.Now() - timing.StartMs,
                    timing.ResponseTimeMs,
                    timing.OutputChunkTimesMs,
                    toolExecutionMs,
                    generated.Usage
                );
        return new StepResult(
            generated.Text,
            reasoning,
            toolCalls,
            toolResults,
            generated.FinishReason,
            generated.Usage,
            sources,
            generated.ProviderMetadata,
            generated.Warnings,
            files,
            model.Provider,
            model.ModelId,
            content,
            performance,
            stepRequest,
            response,
            generated.RawFinishReason
        )
        {
            CallId = callId ?? string.Empty,
            StepNumber = stepNumber,
            ToolsContext = options.ToolsContext ?? StepResult.EmptyToolsContext,
            RuntimeContext = options.RuntimeContext ?? StepResult.EmptyRuntimeContext,
            StructuredOutputText = generated.StructuredOutputText,
            ExperimentalSteeringMessages = steeringMessages is { Count: > 0 }
                ? steeringMessages
                : null,
        };
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
        CancellationToken cancellationToken
    )
    {
        var parses = new List<Task<GeneratedToolCall>>();
        foreach (var part in generated.Content)
        {
            if (part is GeneratedToolCall call)
            {
                parses.Add(
                    ParseToolCallAsync(
                        call,
                        options,
                        messages,
                        instructions,
                        instructionCount,
                        toolChoiceNone,
                        cancellationToken
                    )
                );
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

    internal static async Task<GeneratedToolCall> ParseToolCallAsync(
        GeneratedToolCall call,
        GenerateTextOptions options,
        IReadOnlyList<ModelMessage> messages,
        Instructions? instructions,
        int instructionCount,
        bool toolChoiceNone,
        CancellationToken cancellationToken
    )
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

        var parsed = await ToolCallParser
            .ParseToolCallAsync(
                new ParseToolCallOptions(call)
                {
                    Tools = tools,
                    RepairToolCall = repairToolCall,
                    RefineToolInput = toolChoiceNone ? null : options.RefineToolInput,
                    Messages = history,
                    Instructions = instructions,
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        return new GeneratedToolCall(
            parsed.ToolCallId,
            parsed.ToolName,
            parsed.Input.GetRawText(),
            parsed.ProviderMetadata
        )
        {
            ProviderExecuted = parsed.ProviderExecuted,
            Dynamic = parsed.Dynamic,
            Invalid = parsed.Invalid,
            Error = parsed.Error,
            InputSchemaInput = parsed.InputSchemaInput,
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

    // .NET-only: the port keeps ExecutedTool apart from GeneratedToolResult, where upstream has one tool-result part. This maps the executed
    // tool to the step content tool-result that upstream builds in runToolsTransformation/generateText. ToResponseMessages builds its model output.
    internal static GeneratedToolResult ToGeneratedToolResult(ExecutedTool executed) =>
        new(
            executed.ToolCallId,
            executed.ToolName,
            executed.IsError && executed.Error != null
                ? JsonSerializer.Serialize(ErrorMessage.ToErrorString(executed.Error))
                : executed.OutputJson,
            executed.IsError
        )
        {
            Error = executed.Error,
            Dynamic = executed.Dynamic,
            ToolMetadata = executed.ToolMetadata,
            ProviderExecuted = false,
            InputJson = executed.InputJson,
        };

    // The tool result of a call that failed to parse. The model reads the error the way upstream sends it: "Name: message".
    internal static ExecutedTool InvalidToolResult(GeneratedToolCall call)
    {
        var message = ErrorMessage.ToErrorString(call.Error!);
        return new ExecutedTool(
            call.ToolCallId,
            call.ToolName,
            JsonSerializer.Serialize(new { error = message }),
            true
        )
        {
            InputJson = call.ArgumentsJson,
            Dynamic = true,
            Title = call.Title,
            ToolMetadata = call.ToolMetadata,
            Error = call.Error,
        };
    }

    internal static double DefaultNowMs()
    {
        return Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency;
    }

    // The approvals that streamText decided while the model stream was read, as executeToolsFromStream does.
    private sealed class StreamedApprovals
    {
        // The results of the tools that ran when the model call ended, and how long each took.
        public List<ExecutedTool> Executed { get; } = [];

        public Dictionary<string, double> ToolExecutionMs { get; } = new(StringComparer.Ordinal);

        // Calls whose result is in the step content already, like a call that failed to parse.
        public HashSet<string> InlineResultIds { get; } = new(StringComparer.Ordinal);
    }

    // What FinishStepAsync needs to compute the performance of one step.
    private sealed class StepTiming(
        Func<double> now,
        double startMs,
        double responseTimeMs,
        IReadOnlyList<double> outputChunkTimesMs
    )
    {
        public Func<double> Now { get; } = now;

        public double StartMs { get; } = startMs;

        public double ResponseTimeMs { get; } = responseTimeMs;

        public IReadOnlyList<double> OutputChunkTimesMs { get; } = outputChunkTimesMs;

        // The performance the model call measured. Set for streamed steps.
        public ModelCallPerformance? ModelCallPerformance { get; init; }
    }

    // A provider-executed tool asks for approval by tool call id; the step carries the call itself, as upstream does.
    private static GeneratedToolApprovalRequest ResolveApprovalRequest(
        GeneratedToolApprovalRequest request,
        IReadOnlyList<GeneratedContent> content
    )
    {
        foreach (var part in content)
        {
            if (
                part is GeneratedToolCall call
                && string.Equals(call.ToolCallId, request.ToolCallId, StringComparison.Ordinal)
            )
            {
                return new GeneratedToolApprovalRequest(request.ApprovalId, request.ToolCallId)
                {
                    ToolCall = call,
                };
            }
        }

        throw new AiSdkException(
            "Tool call \""
                + request.ToolCallId
                + "\" not found for approval request \""
                + request.ApprovalId
                + "\"."
        );
    }

    // The model sees no approval requests, and only the approval responses of provider-executed calls.
    // A denied one is followed by an execution-denied result that names the approval for the provider.
    private static IReadOnlyList<ModelMessage> PrepareProviderPrompt(
        IReadOnlyList<ModelMessage> prompt
    )
    {
        prompt = ToolModelMessage.ExpandApprovalResponses(prompt);
        var needsWork = false;
        foreach (var message in prompt)
        {
            if (
                message is ToolApprovalResponseModelMessage
                || (
                    message is AssistantModelMessage assistant && ContainsApprovalRequest(assistant)
                )
            )
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

        var deniedToolCallIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var message in prompt)
        {
            if (
                message
                    is ToolApprovalResponseModelMessage
                    {
                        ProviderExecuted: true,
                        Approved: false
                    } denial
                && callIds.TryGetValue(denial.ApprovalId, out var deniedCallId)
            )
            {
                deniedToolCallIds.Add(deniedCallId);
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

                    prepared.Add(
                        new AssistantModelMessage(
                            assistant.Text,
                            assistant.ToolCalls,
                            assistant.Reasoning,
                            assistant.ReasoningProviderOptions,
                            parts
                        )
                    );
                    break;
                case ToolApprovalResponseModelMessage response:
                    if (!response.ProviderExecuted)
                    {
                        break;
                    }

                    if (
                        response.Approved
                        || !callIds.TryGetValue(response.ApprovalId, out var toolCallId)
                    )
                    {
                        prepared.Add(response);
                        break;
                    }

                    using (
                        var approvalOptions = JsonDocument.Parse(
                            JsonSerializer.Serialize(
                                new { openai = new { approvalId = response.ApprovalId } }
                            )
                        )
                    )
                    {
                        var denied = new ToolModelMessage(
                            toolCallId,
                            toolNames.TryGetValue(toolCallId, out var toolName)
                                ? toolName
                                : string.Empty,
                            response.Reason,
                            false,
                            "execution-denied"
                        )
                        {
                            OutputProviderOptions = approvalOptions.RootElement.Clone(),
                        };
                        prepared.Add(
                            new ToolApprovalResponseModelMessage(
                                response.ApprovalId,
                                false,
                                response.Reason,
                                true
                            )
                            {
                                DeniedToolResult = denied,
                            }
                        );
                    }

                    break;
                case ToolModelMessage tool when deniedToolCallIds.Count > 0:
                    // The result of a denied provider-executed call is sent with its approval response.
                    var kept = tool
                        .Results.Where(result =>
                            !(
                                result.OutputType == "execution-denied"
                                && deniedToolCallIds.Contains(result.ToolCallId)
                            )
                        )
                        .ToArray();
                    if (kept.Length == tool.Results.Count)
                    {
                        prepared.Add(message);
                    }
                    else if (kept.Length > 0)
                    {
                        prepared.Add(new ToolModelMessage(kept));
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

    private static void EnforceToolChoice(
        ToolChoice? toolChoice,
        LanguageModelGenerateResult generated,
        ILanguageModel model
    )
    {
        var violation = ToolChoiceViolation(
            toolChoice,
            generated.FinishReason,
            generated.Content,
            model.Provider,
            model.ModelId
        );
        if (violation != null)
        {
            throw violation;
        }
    }

    // Returns the violation instead of throwing it, so streamLanguageModelCall can report it as an error part.
    internal static ToolChoiceViolationException? ToolChoiceViolation(
        ToolChoice? toolChoice,
        FinishReason finishReason,
        IReadOnlyList<GeneratedContent> content,
        string provider,
        string modelId
    )
    {
        if (toolChoice is null || toolChoice.Type == "auto")
        {
            return null;
        }

        if (toolChoice.Type == "none")
        {
            return content.Any(part => part is GeneratedToolCall)
                ? new ToolChoiceViolationException(
                    "Model response contained a tool call even though tool choice was none.",
                    toolChoice,
                    finishReason,
                    provider,
                    modelId,
                    content
                )
                : null;
        }

        if (finishReason == FinishReason.Interrupted)
        {
            return null;
        }

        var calls = new List<GeneratedToolCall>();
        foreach (var part in content)
        {
            if (part is GeneratedToolCall call)
            {
                calls.Add(call);
            }
        }

        if (toolChoice.Type == "required")
        {
            return calls.Count == 0
                ? new ToolChoiceViolationException(
                    "Model response did not contain a tool call even though tool choice was required.",
                    toolChoice,
                    finishReason,
                    provider,
                    modelId,
                    content
                )
                : null;
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            foreach (var call in calls)
            {
                if (string.Equals(call.ToolName, named.ToolName, StringComparison.Ordinal))
                {
                    return null;
                }
            }

            return new ToolChoiceViolationException(
                "Model response did not contain a call to the required tool '"
                    + named.ToolName
                    + "'.",
                toolChoice,
                finishReason,
                provider,
                modelId,
                content
            );
        }

        return null;
    }

    // Upstream parses the input of a tool call with the tool's input schema. Only a provider-defined tool declared with a
    // schema is parsed here, so the input of any other call is kept as the model sent it.
    internal static GeneratedToolCall ParseProviderExecutedInput(
        GenerateTextOptions options,
        GeneratedToolCall call
    )
    {
        if (!call.ProviderExecuted || call.Invalid)
        {
            return call;
        }

        var tool = FindTool(options, call.ToolName);
        if (
            tool is not { IsProviderDefined: true }
            || tool.InputSchema.ValueKind != JsonValueKind.Object
        )
        {
            return call;
        }

        JsonNode? input;
        try
        {
            input = JsonNode.Parse(
                string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson
            );
        }
        catch (JsonException)
        {
            return call;
        }

        var parsed = JsonSchemaProjection.Project(tool.InputSchema, input);
        return new GeneratedToolCall(
            call.ToolCallId,
            call.ToolName,
            parsed?.ToJsonString() ?? "null",
            call.ProviderMetadata
        )
        {
            ProviderExecuted = true,
            Dynamic = call.Dynamic,
            InputSchemaInput = call.InputSchemaInput,
            Title = call.Title,
            ToolMetadata = call.ToolMetadata,
        };
    }

    internal static Tool? FindTool(GenerateTextOptions options, string name) =>
        FindTool(options.Tools, name);

    internal static Tool? FindTool(IReadOnlyList<Tool>? tools, string name)
    {
        if (tools != null)
        {
            foreach (var candidate in tools)
            {
                if (string.Equals(candidate.Name, name, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }
        }

        return null;
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

    // The approval responses at the end of the history settle the calls they answer before the first step: the approved calls
    // run, the denied calls get an execution-denied result, and an approved call whose input no longer validates gets its error.
    // The results are appended to the history and to the response messages as one tool message.
    private static async Task ProcessInitialApprovalsAsync(
        GenerateTextOptions options,
        List<ModelMessage> messages,
        int instructionCount,
        List<ModelMessage> responseMessages,
        Func<double> now,
        CancellationToken cancellationToken,
        Action<TextStreamPart>? emit = null,
        string? callId = null
    )
    {
        var history = messages.Skip(instructionCount).ToArray();
        var collected = ToolApprovalCollection.CollectToolApprovals(ToApprovalHistory(history));

        // A provider-executed call is answered by the provider, with the response in the prompt.
        var approvedToolApprovals = collected
            .ApprovedToolApprovals.Where(approval => !approval.ToolCall.ProviderExecuted)
            .ToList();
        var validated = await ToolApprovalValidation
            .ValidateApprovedToolApprovalsAsync(
                new ValidateApprovedToolApprovalsOptions(approvedToolApprovals)
                {
                    Tools = options.Tools,
                    ToolApproval = options.ToolApproval,
                    ToolsContext = options.ToolsContext,
                    RuntimeContext = options.RuntimeContext,
                    Messages = history,
                    ToolApprovalSecret = options.ExperimentalToolApprovalSecret,
                    ToolApprovalSecretBytes = options.ExperimentalToolApprovalSecretBytes,
                    RefineToolInput = options.RefineToolInput,
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // streamText reports every denied call, local or provider-executed, including one that already has its result.
        if (emit != null)
        {
            foreach (
                var denied in collected
                    .DeniedToolApprovals.Where(approval => !approval.ToolCall.ProviderExecuted)
                    .Concat(validated.DeniedToolApprovals)
                    .Concat(collected.DeniedToolApprovals.Where(a => a.ToolCall.ProviderExecuted))
            )
            {
                emit(
                    new ToolOutputDeniedPart(denied.ToolCall.ToolCallId, denied.ToolCall.ToolName)
                );
            }

            foreach (var invalid in validated.InvalidToolApprovals)
            {
                var message = ErrorMessage.ToErrorString(invalid.Error);
                emit(
                    new ToolErrorPart(
                        new ExecutedTool(
                            invalid.Approval.ToolCall.ToolCallId,
                            invalid.Approval.ToolCall.ToolName,
                            JsonSerializer.Serialize(new { error = message }),
                            true
                        )
                        {
                            Error = invalid.Error,
                            InputJson = invalid.Approval.ToolCall.Input.GetRawText(),
                        }
                    )
                );
            }
        }

        var deniedWithoutResults = collected
            .DeniedToolApprovals.Concat(validated.DeniedToolApprovals)
            .Where(approval =>
                approval.ExistingToolResult == null && !approval.ToolCall.ProviderExecuted
            )
            .ToList();
        if (
            deniedWithoutResults.Count == 0
            && validated.ApprovedToolApprovals.Count == 0
            && validated.InvalidToolApprovals.Count == 0
        )
        {
            return;
        }

        var stepMessages = messages.ToArray();
        var approvalCallId = callId ?? NewCallId();
        var executions = await ToolCallConcurrencyScheduler
            .MapWithConcurrency(
                validated.ApprovedToolApprovals,
                ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(
                    options.ToolCallConcurrency
                ),
                async (approval, _) =>
                {
                    var call = new GeneratedToolCall(
                        approval.ToolCall.ToolCallId,
                        approval.ToolCall.ToolName,
                        approval.ToolCall.Input.GetRawText()
                    );
                    var result = await ToolCallExecution
                        .ExecuteToolCall(
                            call,
                            options.Tools,
                            options.ToolsContext,
                            approvalCallId,
                            stepMessages,
                            cancellationToken,
                            now,
                            options.Timeout,
                            options.ExperimentalSandbox,
                            emit == null
                                ? null
                                : preliminary => emit(new ToolResultPart(preliminary)),
                            ToolExecutionCallbacks.Start(options),
                            ToolExecutionCallbacks.End(options),
                            ToolExecutionCallbacks.ExecuteInTelemetryContext(
                                options,
                                cancellationToken
                            ),
                            ToolExecutionCallbacks.RunInTracingChannelSpan(options)
                        )
                        .ConfigureAwait(false);
                    if (result == null)
                    {
                        return null;
                    }

                    var executed = result.Value.Output;

                    emit?.Invoke(
                        executed.IsError
                            ? new ToolErrorPart(executed)
                            : new ToolResultPart(executed)
                    );
                    return executed;
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        var toolMessages = new List<ToolModelMessage>();
        foreach (var executed in executions.OfType<ExecutedTool>())
        {
            toolMessages.Add(
                ToResponseMessages.CreateToolMessage(ToGeneratedToolResult(executed), options.Tools)
            );
        }

        // Repairing the input after approval would change the operation that was approved, so the model gets the error.
        foreach (var invalid in validated.InvalidToolApprovals)
        {
            toolMessages.Add(
                new ToolModelMessage(
                    invalid.Approval.ToolCall.ToolCallId,
                    invalid.Approval.ToolCall.ToolName,
                    ErrorMessage.ToErrorString(invalid.Error),
                    true,
                    "error-text"
                )
            );
        }

        foreach (var denied in deniedWithoutResults)
        {
            toolMessages.Add(
                new ToolModelMessage(
                    denied.ToolCall.ToolCallId,
                    denied.ToolCall.ToolName,
                    denied.ApprovalResponse.Reason,
                    false,
                    "execution-denied"
                )
            );
        }

        var toolMessage =
            toolMessages.Count == 1 ? toolMessages[0] : new ToolModelMessage(toolMessages);
        messages.Add(toolMessage);
        responseMessages.Add(toolMessage);
    }

    // The history in the shape that collectToolApprovals reads. Only the trailing tool messages count as the last tool message.
    private static IReadOnlyList<ApprovalHistoryMessage> ToApprovalHistory(
        IReadOnlyList<ModelMessage> history
    )
    {
        history = ToolModelMessage.ExpandApprovalResponses(history);
        var trailingStart = history.Count;
        while (
            trailingStart > 0
            && string.Equals(history[trailingStart - 1].Role, "tool", StringComparison.Ordinal)
        )
        {
            trailingStart--;
        }

        var result = new List<ApprovalHistoryMessage>();
        var trailingParts = new List<ApprovalHistoryPart>();
        for (var index = 0; index < history.Count; index++)
        {
            switch (history[index])
            {
                case AssistantModelMessage assistant:
                    var parts = new List<ApprovalHistoryPart>();
                    foreach (var part in assistant.Parts)
                    {
                        if (part is AssistantToolCallPart call)
                        {
                            using var input = JsonDocument.Parse(
                                string.IsNullOrWhiteSpace(call.ToolCall.ArgumentsJson)
                                    ? "{}"
                                    : call.ToolCall.ArgumentsJson
                            );
                            parts.Add(
                                new ApprovalToolCallPart(
                                    call.ToolCall.ToolCallId,
                                    call.ToolCall.ToolName,
                                    input.RootElement.Clone()
                                )
                                {
                                    ProviderExecuted = call.ToolCall.ProviderExecuted,
                                }
                            );
                        }
                        else if (part is AssistantToolApprovalRequestPart request)
                        {
                            parts.Add(
                                new ApprovalRequestPart(request.ApprovalId, request.ToolCallId)
                                {
                                    Signature = request.Signature,
                                    Reason = request.Reason,
                                    InputSchemaInput = request.InputSchemaInput,
                                }
                            );
                        }
                    }

                    result.Add(new ApprovalHistoryMessage("assistant", null, parts));
                    break;
                case ToolApprovalResponseModelMessage response when index >= trailingStart:
                    trailingParts.Add(
                        new ApprovalResponsePart(
                            response.ApprovalId,
                            response.Approved,
                            response.Reason
                        )
                    );
                    break;
                case ToolModelMessage tool when index >= trailingStart:
                    foreach (var toolResult in tool.Results)
                    {
                        var isDenied = string.Equals(
                            toolResult.OutputType,
                            "execution-denied",
                            StringComparison.Ordinal
                        );
                        trailingParts.Add(
                            new ApprovalToolResultPart(
                                toolResult.ToolCallId,
                                toolResult.ToolName,
                                toolResult.OutputType,
                                null,
                                isDenied ? toolResult.OutputJson : null
                            )
                        );
                    }

                    break;
                case ModelMessage other when index < trailingStart:
                    result.Add(new ApprovalHistoryMessage(other.Role));
                    break;
            }
        }

        if (history.Count > trailingStart)
        {
            result.Add(new ApprovalHistoryMessage("tool", null, trailingParts));
        }

        return result;
    }

    internal static DateTimeOffset DefaultCurrentDate() => DateTimeOffset.UtcNow;

    internal static string NewStepResponseId(GenerateTextOptions options) =>
        options.GenerateId?.Invoke() ?? Vercel.AI.ProviderUtils.JsonValues.GenerateId("aitxt-");

    internal static string NewApprovalId(GenerateTextOptions options) =>
        options.GenerateId?.Invoke() ?? Vercel.AI.ProviderUtils.JsonValues.GenerateId(string.Empty);

    internal static string? SignApproval(
        GenerateTextOptions options,
        string approvalId,
        GeneratedToolCall call
    )
    {
        if (
            options.ExperimentalToolApprovalSecret == null
            && options.ExperimentalToolApprovalSecretBytes == null
        )
        {
            return null;
        }

        using var input = JsonDocument.Parse(
            string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson
        );
        return options.ExperimentalToolApprovalSecret != null
            ? ToolApprovalSignatures.SignToolApproval(
                options.ExperimentalToolApprovalSecret,
                approvalId,
                call.ToolCallId,
                call.ToolName,
                input.RootElement
            )
            : ToolApprovalSignatures.SignToolApproval(
                options.ExperimentalToolApprovalSecretBytes!,
                approvalId,
                call.ToolCallId,
                call.ToolName,
                input.RootElement
            );
    }

    // Notifies the tool that its call is available, with the validated tool context, before the approval is resolved.
    // A call that is invalid or has no known tool is ignored.
    private static async Task InvokeToolInputCallbacksAsync(
        GenerateTextOptions options,
        GeneratedToolCall call,
        IReadOnlyList<ModelMessage> messages,
        CancellationToken cancellationToken
    )
    {
        if (call.Invalid)
        {
            return;
        }

        var tool = FindTool(options, call.ToolName);
        if (tool == null || (tool.OnInputStart == null && tool.OnInputAvailable == null))
        {
            return;
        }

        JsonElement? context = null;
        if (
            options.ToolsContext != null
            && options.ToolsContext.TryGetValue(call.ToolName, out var rawContext)
        )
        {
            context = rawContext;
        }

        context = ToolContexts.ValidateToolContext(call.ToolName, context, tool.ContextSchema);

        if (tool.OnInputStart != null)
        {
            await tool.OnInputStart(
                    new ToolExecutionOptions(call.ToolCallId, messages, cancellationToken)
                    {
                        Context = context,
                    }
                )
                .ConfigureAwait(false);
        }

        if (tool.OnInputAvailable != null)
        {
            using var arguments = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson
            );
            await tool.OnInputAvailable(
                    new ToolInputAvailableOptions(
                        call.ToolCallId,
                        messages,
                        cancellationToken,
                        arguments.RootElement.Clone()
                    )
                    {
                        Context = context,
                    }
                )
                .ConfigureAwait(false);
        }
    }

    // Resolves the approval of each valid call to a known tool. A call that does not need approval does not use an approval id.
    private static async Task<(
        List<GeneratedToolApprovalRequest> Requests,
        List<GeneratedToolApprovalResponse> Responses,
        HashSet<string> Blocked
    )> ResolveStepApprovalsAsync(
        GenerateTextOptions options,
        IReadOnlyList<GeneratedToolCall> toolCalls,
        IReadOnlyList<ModelMessage> messages,
        IReadOnlyList<ModelMessage> stepInputMessages,
        CancellationToken cancellationToken
    )
    {
        var requests = new List<GeneratedToolApprovalRequest>();
        var responses = new List<GeneratedToolApprovalResponse>();
        var blocked = new HashSet<string>(StringComparer.Ordinal);
        foreach (var call in toolCalls)
        {
            await InvokeToolInputCallbacksAsync(options, call, stepInputMessages, cancellationToken)
                .ConfigureAwait(false);
            var approval = await ExecuteToolsFromStream
                .ResolveToolCallApprovalAsync(options, call, messages)
                .ConfigureAwait(false);
            if (approval.Request != null)
            {
                requests.Add(approval.Request);
            }

            if (approval.Response != null)
            {
                responses.Add(approval.Response);
            }

            if (approval.Blocked)
            {
                blocked.Add(call.ToolCallId);
            }
        }

        return (requests, responses, blocked);
    }

    // A tool without an execute function, or one awaiting approval, leaves its call for the caller, so the loop ends without asking the stop condition.
    internal static bool HasExecutor(Tool? tool) =>
        tool != null
        && (tool.Execute != null || tool.ExecuteWithOptions != null || tool.ExecuteStream != null);

    // A call is settled when the step answers it: with a tool result, or with an automatic denial. Like upstream, the loop goes on
    // only when every client call is settled.
    private static bool HasUnsettledToolCall(StepResult step)
    {
        foreach (var call in step.ToolCalls)
        {
            if (call.ProviderExecuted || call.Invalid)
            {
                continue;
            }

            var settled = false;
            foreach (var result in step.ToolResults)
            {
                if (string.Equals(result.ToolCallId, call.ToolCallId, StringComparison.Ordinal))
                {
                    settled = true;
                    break;
                }
            }

            foreach (var part in step.Content)
            {
                if (
                    part is GeneratedToolApprovalResponse { Approved: false } response
                    && string.Equals(
                        response.ToolCall.ToolCallId,
                        call.ToolCallId,
                        StringComparison.Ordinal
                    )
                )
                {
                    settled = true;
                    break;
                }
            }

            if (!settled)
            {
                return true;
            }
        }

        return false;
    }

    // The model-facing result of a tool's toModelOutput. Only text and error-text results travel as plain text.
    internal static string? ModelOutputText(ToolModelOutput? output)
    {
        return output switch
        {
            TextToolModelOutput text => text.Value,
            ErrorTextToolModelOutput errorText => errorText.Value,
            JsonToolModelOutput json => json.Value.GetRawText(),
            ErrorJsonToolModelOutput errorJson => errorJson.Value.GetRawText(),
            _ => null,
        };
    }

    internal static IReadOnlyList<UserContentPart> ToUserContent(
        IReadOnlyList<ToolModelContentPart> content
    )
    {
        var parts = new List<UserContentPart>(content.Count);
        foreach (var part in content)
        {
            if (part is not FileToolModelContentPart file)
            {
                parts.Add(new TextContentPart(part.Text));
                continue;
            }

            parts.Add(
                new FileContentPart(
                    file.MediaType,
                    null,
                    Convert.FromBase64String(file.Data.TextData ?? string.Empty),
                    file.FileName
                )
            );
        }

        return parts;
    }

    // Maps to `withUserAgentSuffix(headers, `ai/${VERSION}`)`.
    internal static IReadOnlyDictionary<string, string?> WithUserAgentHeader(
        IReadOnlyDictionary<string, string?>? headers
    ) =>
        Vercel
            .AI.Operations.OperationHeaders.WithUserAgent(
                headers
                    ?.Where(pair => pair.Value != null)
                    .ToDictionary(pair => pair.Key, pair => pair.Value!),
                Vercel.AI.Operations.AiSdkVersion.UserAgent
            )
            .ToDictionary(pair => pair.Key, pair => (string?)pair.Value);

    // Stands for the call id that upstream generates once for each generateText or streamText call.
    private static string NewCallId() => "call-" + Guid.NewGuid().ToString("N");

    // Upstream's `notify` ignores an error a callback throws, so a faulty listener cannot break generation.
    private static GenerateTextStartEvent CreateStartEvent(
        GenerateTextOptions options,
        string callId,
        ILanguageModel model,
        Instructions? initialInstructions,
        IReadOnlyList<ModelMessage> initialMessages,
        int resolvedMaxRetries,
        string operationId = "ai.generateText"
    ) =>
        new GenerateTextStartEvent
        {
            CallId = callId,
            OperationId = operationId,
            Provider = model.Provider,
            ModelId = model.ModelId,
            Instructions = initialInstructions,
            Messages = initialMessages,
            Tools = options.Tools,
            ToolChoice = options.ToolChoice,
            ActiveTools = options.ActiveTools,
            ToolOrder = options.ToolOrder,
            MaxOutputTokens = options.MaxOutputTokens,
            Temperature = options.Temperature,
            TopP = options.TopP,
            TopK = options.TopK,
            PresencePenalty = options.PresencePenalty,
            FrequencyPenalty = options.FrequencyPenalty,
            StopSequences = options.StopSequences,
            Seed = options.Seed,
            Reasoning = options.Reasoning,
            MaxRetries = resolvedMaxRetries,
            Timeout = options.Timeout,
            Headers = options.Headers,
            ProviderOptions = options.ProviderOptions,
            Output = options.Output,
            ToolsContext = options.ToolsContext ?? StepResult.EmptyToolsContext,
            RuntimeContext = options.RuntimeContext ?? StepResult.EmptyRuntimeContext,
        };

    internal static async Task NotifyAsync(Func<Task> callback, CancellationToken cancellationToken)
    {
        try
        {
            await callback().ConfigureAwait(false);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested) { }
    }

    private static IReadOnlyList<LanguageModelTool>? SelectTools(
        GenerateTextOptions options,
        IReadOnlyList<string>? toolOrder
    )
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

            tools.Add(
                tool.IsProviderDefined
                    ? LanguageModelTool.ProviderDefined(
                        tool.ProviderToolId!,
                        tool.Name,
                        tool.ProviderToolArgs,
                        tool.RedactArgs
                    )
                    : new LanguageModelTool(
                        tool.Name,
                        ResolveToolDescription(tool, options),
                        tool.InputSchema,
                        tool.Strict
                    )
                    {
                        ProviderOptions = tool.ProviderOptions,
                    }
            );
        }

        // Upstream filters the active tools first, then prepareTools returns undefined for an empty set.
        return tools.Count == 0 ? null : OrderTools(tools, toolOrder);
    }

    // Tools named in toolOrder come first, in that order. The rest follow, sorted by name. Maps to orderToolEntries.
    private static IReadOnlyList<LanguageModelTool> OrderTools(
        List<LanguageModelTool> tools,
        IReadOnlyList<string>? toolOrder
    )
    {
        if (toolOrder == null)
        {
            return tools;
        }

        var listed = tools
            .Where(tool => Contains(toolOrder, tool.Name))
            .OrderBy(tool => IndexOfName(toolOrder, tool.Name));
        var rest = tools
            .Where(tool => !Contains(toolOrder, tool.Name))
            .OrderBy(tool => tool.Name, StringComparer.Ordinal);
        return listed.Concat(rest).ToList();
    }

    private static int IndexOfName(IReadOnlyList<string> names, string name)
    {
        for (var i = 0; i < names.Count; i++)
        {
            if (string.Equals(names[i], name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    // Maps to resolveToolDescription in prepare-tools.ts.
    private static string? ResolveToolDescription(Tool tool, GenerateTextOptions options)
    {
        if (tool.DescriptionFactory == null)
        {
            return tool.Description;
        }

        object? context = null;
        if (
            options.ToolsContext != null
            && options.ToolsContext.TryGetValue(tool.Name, out var value)
        )
        {
            context = value;
        }

        return tool.DescriptionFactory(
            new Vercel.AI.Prompt.ToolDescriptionContext(context, options.ExperimentalSandbox)
        );
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

    private static LanguageModelResponseMetadata GeneratedResponseInfo(
        LanguageModelGenerateResult generated,
        ILanguageModel model
    )
    {
        return new LanguageModelResponseMetadata(
            generated.ResponseId ?? Vercel.AI.ProviderUtils.JsonValues.GenerateId("aitxt-"),
            generated.ResponseTimestamp ?? DateTimeOffset.UtcNow,
            generated.ResponseModelId ?? model.ModelId,
            generated.ResponseHeaders
        );
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

    internal static async Task<GenerateTextResult> ToResultAsync(
        List<StepResult> steps,
        List<ModelMessage> responseMessages,
        GenerateTextOptions options,
        CancellationToken cancellationToken,
        bool parseOutput = true,
        LanguageModelUsage? totalUsage = null
    )
    {
        var outputSpec = parseOutput ? options.Output : null;
        var last =
            steps.Count == 0
                ? new StepResult(
                    string.Empty,
                    null,
                    Array.Empty<GeneratedToolCall>(),
                    Array.Empty<ExecutedTool>(),
                    FinishReason.Other,
                    LanguageModelUsage.Empty,
                    Array.Empty<GeneratedSource>()
                )
                : steps[steps.Count - 1];
        var usage = LanguageModelUsage.Null;
        var sources = new List<GeneratedSource>();
        foreach (var step in steps)
        {
            usage = LanguageModelUsage.Add(usage, step.Usage);
            sources.AddRange(step.Sources);
        }

        usage = totalUsage ?? usage;

        // A provider-selected structured output text replaces the step text for JSON output only. An empty selection
        // is explicit, so it is parsed (and rejected) instead of falling back to the step text.
        var outputText =
            outputSpec?.IsJson == true ? last.StructuredOutputText ?? last.Text : last.Text;
        JsonElement? output = null;
        if (outputSpec is { IsJson: true })
        {
            if (options.GenerateObjectMode && !last.Content.OfType<GeneratedText>().Any())
            {
                throw new NoObjectGeneratedException(
                    "No object generated: the model did not return a response.",
                    null,
                    null,
                    last.Response,
                    last.Usage,
                    last.FinishReason
                );
            }

            if (
                !options.GenerateObjectMode
                && last.FinishReason != FinishReason.Stop
                && (last.FinishReason == FinishReason.ToolCalls || last.Text.Length == 0)
            )
            {
                return options.WrapResult(
                    new GenerateTextResult(
                        last.Text,
                        last.ReasoningText,
                        steps,
                        last.FinishReason,
                        usage,
                        null,
                        sources,
                        last.ProviderMetadata
                    )
                    {
                        ResponseMessages = responseMessages.ToArray(),
                    }
                );
            }

            try
            {
                output = outputSpec.ParseCompleteOutput(outputText);
            }
            catch (Exception error)
                when (error
                        is ProviderUtils.JsonParseException
                            or ProviderUtils.TypeValidationException
                )
            {
                var initialError = new NoObjectGeneratedException(
                    error is ProviderUtils.TypeValidationException
                        ? "No object generated: response did not match schema."
                        : "No object generated: could not parse the response.",
                    error,
                    outputText,
                    last.Response,
                    last.Usage,
                    last.FinishReason
                );
                if (options.RepairOutput == null)
                {
                    throw initialError;
                }

                var repairedText = await options
                    .RepairOutput(outputText, error, cancellationToken)
                    .ConfigureAwait(false);
                if (repairedText == null)
                {
                    throw initialError;
                }

                try
                {
                    output = outputSpec.ParseCompleteOutput(repairedText);
                }
                catch (Exception repairedError)
                    when (repairedError
                            is ProviderUtils.JsonParseException
                                or ProviderUtils.TypeValidationException
                    )
                {
                    throw new NoObjectGeneratedException(
                        repairedError is ProviderUtils.TypeValidationException
                            ? "No object generated: response did not match schema."
                            : "No object generated: could not parse the response.",
                        repairedError,
                        repairedText,
                        last.Response,
                        last.Usage,
                        last.FinishReason
                    );
                }
            }
        }

        // Without a JSON output, the output is the text of a stop response or a non-empty response that is not a tool call.
        // Maps to `output ?? text()`.
        if (
            parseOutput
            && outputSpec is not { IsJson: true }
            && (
                last.FinishReason == FinishReason.Stop
                || (last.FinishReason != FinishReason.ToolCalls && last.Text.Length > 0)
            )
        )
        {
            output = JsonSerializer.SerializeToElement(last.Text);
        }

        return options.WrapResult(
            new GenerateTextResult(
                last.Text,
                last.ReasoningText,
                steps,
                last.FinishReason,
                usage,
                output,
                sources,
                last.ProviderMetadata
            )
            {
                ResponseMessages = responseMessages.ToArray(),
            }
        );
    }
}

// Settles a streamText call exactly once: as aborted, or as finished. Whichever comes first wins.
internal sealed class StreamAbort
{
    // Set when a stream transform called stopStream. The event processor then finishes the results instead of cancelling them.
    public volatile bool StoppedByTransform;

    private static readonly object Done = new();
    private static readonly TimeSpan StopAbortGrace = TimeSpan.FromSeconds(1);
    private readonly CancellationTokenSource _cancellation = new();
    private readonly TaskCompletionSource<bool> _signal = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );
    private object? _outcome;
    private EnumeratorDisposal? _disposal;

    public CancellationToken Token => _cancellation.Token;

    public Task Signal => _signal.Task;

    public CancellationTokenRegistration Registration { get; set; }

    public Action? SignalCleanup { get; set; }

    // Reports an abort that arrives after the consumer stopped reading.
    public Func<Exception, Task>? OnLateAbort
    {
        get => Volatile.Read(ref _lateAbort);
        set => Volatile.Write(ref _lateAbort, value);
    }

    private Func<Exception, Task>? _lateAbort;

    // The steering messages that are still waiting cannot be delivered once the stream is aborted or stopped.
    public SteeringRun? SteeringRun { get; set; }

    public Exception? Error => _outcome as Exception;

    public bool Stopped
    {
        get => Volatile.Read(ref _stopped);
        private set => Volatile.Write(ref _stopped, value);
    }

    private bool _stopped;

    public void Dispose() => _cancellation.Dispose();

    // After the consumer stopped, an abort that follows right away must still reach onAbort. Upstream's cancel reaches the run asynchronously,
    // so an abort in the same tick wins. The signal handler therefore stays registered for a short grace period.
    public void CleanupSignal()
    {
        var cleanup = SignalCleanup;
        if (cleanup is null)
        {
            return;
        }

        if (Stopped && OnLateAbort is not null)
        {
            _ = Task.Delay(StopAbortGrace)
                .ContinueWith(
                    _ => cleanup(),
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default
                );
            return;
        }

        cleanup();
    }

    // The consumer stopped reading. Cancels the provider call and the tool loop, like upstream cancelling the source reader.
    public void Stop()
    {
        if (
            Interlocked.CompareExchange(
                ref _outcome,
                new OperationCanceledException("The stream consumer stopped reading."),
                null
            ) != null
        )
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
            SteeringRun?.End(SteeringNotDeliveredReason.Aborted);
            Volatile.Read(ref _disposal)?.Abandon();
            _signal.TrySetResult(true);
        }
    }

    public bool TryFinish() => Interlocked.CompareExchange(ref _outcome, Done, null) == null;

    public EnumeratorDisposal Track(IAsyncDisposable enumerator)
    {
        return Track(new EnumeratorDisposal(enumerator));
    }

    public EnumeratorDisposal Track(EnumeratorDisposal disposal)
    {
        Volatile.Write(ref _disposal, disposal);
        return disposal;
    }

    // Rejects the results first, so no callback or provider cleanup can delay them.
    public void Fire(
        Exception error,
        TaskCompletionSource<string> text,
        TaskCompletionSource<FinishReason> finishReason,
        TaskCompletionSource<LanguageModelUsage> usage,
        TaskCompletionSource<IReadOnlyList<StepResult>> steps
    )
    {
        if (Interlocked.CompareExchange(ref _outcome, error, null) != null)
        {
            ReportLateAbort(error);
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
        catch (Exception exception)
            when (exception is AggregateException or ObjectDisposedException)
        {
            // A token callback threw, or the call already ended. The call is aborted either way.
        }
        finally
        {
            SteeringRun?.End(SteeringNotDeliveredReason.Aborted);
            Volatile.Read(ref _disposal)?.Abandon();
            _signal.TrySetResult(true);
        }
    }

    private void ReportLateAbort(Exception error)
    {
        if (!Stopped)
        {
            return;
        }

        var late = Interlocked.Exchange(ref _lateAbort, null);
        if (late is null)
        {
            return;
        }

        try
        {
            _ = late(error)
                .ContinueWith(
                    static task => _ = task.Exception,
                    TaskContinuationOptions.OnlyOnFaulted
                );
        }
        catch (Exception)
        {
            // The callback threw before it returned a task. The consumer is gone, so nobody can observe the failure.
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
            _ = DisposeAsync()
                .ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        }
        catch (Exception)
        {
            // The enumerator threw while starting to dispose. The call is already aborted.
        }
    }
}

// Maps to createOutputTransformStream. Text deltas are held back until the partial output changes, so that each published
// delta carries the partial output that its text produces. The array output parses only the new text of each delta.
internal sealed class OutputTransformStream
{
    private readonly OutputSpec _output;
    private string? _firstTextChunkId;
    private readonly StringBuilder _text = new();
    private readonly StringBuilder _textChunk = new();
    private JsonElement? _textProviderMetadata;
    private Util.ArrayOutputStreamParser? _arrayStreamParser;
    private JsonElement? _lastArrayPartial;
    private string? _lastPublishedValue;
    private bool _hasPublishedValue;

    public OutputTransformStream(OutputSpec output)
    {
        _output = output;
        _arrayStreamParser = Util.ArrayOutputStream.CreateArrayStreamParser(output);
    }

    public IEnumerable<TextStreamPart> Transform(TextStreamPart chunk)
    {
        if (chunk is StartStepPart or StreamRetryBoundaryPart)
        {
            ResetOutputState();
        }

        // Ensure that the last text chunk is published before the step finish.
        if (chunk is StepFinishPart && _textChunk.Length > 0)
        {
            yield return PublishTextChunk(null);
        }

        if (chunk is not (TextDeltaPart or TextStartPart or TextEndPart))
        {
            yield return chunk;
            yield break;
        }

        // The partial output follows the first text block of the step.
        var id = chunk switch
        {
            TextDeltaPart delta => delta.Id,
            TextStartPart start => start.Id,
            _ => ((TextEndPart)chunk).Id,
        };
        if (_firstTextChunkId == null)
        {
            _firstTextChunkId = id;
        }
        else if (id != _firstTextChunkId)
        {
            yield return chunk;
            yield break;
        }

        if (chunk is TextStartPart)
        {
            yield return chunk;
            yield break;
        }

        if (chunk is TextEndPart)
        {
            if (_textChunk.Length > 0)
            {
                yield return PublishTextChunk(null);
            }

            yield return chunk;
            yield break;
        }

        var textDelta = (TextDeltaPart)chunk;
        _text.Append(textDelta.Text);
        _textChunk.Append(textDelta.Text);
        _textProviderMetadata = textDelta.ProviderMetadata ?? _textProviderMetadata;

        if (textDelta.Text.Length == 0 && textDelta.ProviderMetadata != null)
        {
            yield return chunk;
            yield break;
        }

        // Built-in arrays publish only changed snapshots, so growing arrays do not need
        // to be reparsed, revalidated, or serialized on every chunk.
        if (_arrayStreamParser != null)
        {
            var parsed = _arrayStreamParser(textDelta.Text);
            if (!ReferenceEquals(parsed, Util.ArrayOutputStreamParseResult.Fallback))
            {
                if (parsed != null)
                {
                    _lastArrayPartial = JsonSerializer.SerializeToElement(parsed.Partial);
                    yield return PublishTextChunk(_lastArrayPartial);
                }

                yield break;
            }

            _arrayStreamParser = null;
            if (_lastArrayPartial != null)
            {
                _lastPublishedValue = _lastArrayPartial.Value.GetRawText();
                _hasPublishedValue = true;
            }
        }

        // Only publish if the partial output can be parsed, and only if it changed.
        if (_output.ParsePartialOutput(_text.ToString()) is { } partial)
        {
            // For string partials (text output), compare directly to avoid unnecessary serialization overhead.
            var currentValue =
                partial.ValueKind == JsonValueKind.String
                    ? partial.GetString()
                    : partial.GetRawText();
            if (!_hasPublishedValue || currentValue != _lastPublishedValue)
            {
                _lastPublishedValue = currentValue;
                _hasPublishedValue = true;
                yield return PublishTextChunk(partial);
            }
        }
    }

    private TextDeltaPart PublishTextChunk(JsonElement? partialOutput)
    {
        var part = new TextDeltaPart(
            _firstTextChunkId!,
            _textChunk.ToString(),
            _textProviderMetadata
        )
        {
            PartialOutput = partialOutput,
        };
        _textChunk.Clear();
        return part;
    }

    private void ResetOutputState()
    {
        _arrayStreamParser = Util.ArrayOutputStream.CreateArrayStreamParser(_output);
        _lastArrayPartial = null;
        _firstTextChunkId = null;
        _text.Clear();
        _textChunk.Clear();
        _textProviderMetadata = null;
        _lastPublishedValue = null;
        _hasPublishedValue = false;
    }
}

internal sealed class PartBuffer
{
    private readonly List<TextStreamPart> _parts = new();
    private readonly object _gate = new();
    private bool _claimed;
    private readonly SemaphoreSlim _signal = new(0);
    private Exception? _error;
    private bool _completed;
    private int _readers;

    // Called when the last active reader stops before the stream ends.
    public Action? OnConsumerStop { get; set; }

    // Maps to retention.replay === false. The buffer then serves one consumer and drops each part once it is read.
    public bool SingleConsumer { get; set; }

    // Maps to the pull model of the single-consumer stream. The producer waits while this many parts are unread.
    public int? HighWaterMark { get; set; }

    private readonly SemaphoreSlim _room = new(0);

    // Waits until the consumer has read enough parts for the producer to pull the next one.
    public async Task WaitForRoomAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            lock (_gate)
            {
                if (HighWaterMark is not { } limit || _parts.Count < limit || _completed)
                {
                    return;
                }
            }

            await _room.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    // Maps to teeStream's AI_StreamAlreadyConsumedError.
    public void Claim()
    {
        if (!SingleConsumer)
        {
            return;
        }

        lock (_gate)
        {
            if (_claimed)
            {
                throw new AiSdkException(
                    "This result supports one stream consumer. Choose one stream projection or response helper."
                );
            }

            _claimed = true;
        }
    }

    // Maps to createOutputTransformStream. Rewrites the text deltas of a structured output so that each one carries a partial output.
    public Func<TextStreamPart, IEnumerable<TextStreamPart>>? Transform { get; set; }

    public void Add(TextStreamPart part)
    {
        var added = 0;
        lock (_gate)
        {
            if (Transform == null)
            {
                _parts.Add(part);
                added = 1;
            }
            else
            {
                foreach (var transformed in Transform(part))
                {
                    _parts.Add(transformed);
                    added++;
                }
            }
        }

        if (added > 0)
        {
            _signal.Release(added);
        }
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

    public async IAsyncEnumerable<TextStreamPart> Read(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var index = 0;
        var finished = false;
        Interlocked.Increment(ref _readers);
        try
        {
            while (true)
            {
                // A reader that stopped does not receive another part, even when one arrived at the same time.
                cancellationToken.ThrowIfCancellationRequested();
                TextStreamPart? part = null;
                var ready = false;
                Exception? error = null;
                var done = false;
                lock (_gate)
                {
                    if (index < _parts.Count)
                    {
                        part = _parts[index];
                        if (SingleConsumer)
                        {
                            // Nothing replays, so a part that was read is released.
                            _parts[index] = null!;
                            if (++index == _parts.Count)
                            {
                                _parts.Clear();
                                index = 0;
                            }

                            if (_room.CurrentCount == 0)
                            {
                                _room.Release();
                            }
                        }
                        else
                        {
                            index++;
                        }

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
                if (_room.CurrentCount == 0)
                {
                    _room.Release();
                }
            }
        }
    }
}

/// <summary>Result of <c>streamText</c>. Enumerating <see cref="Stream"/> or awaiting <see cref="Text"/> consumes the same buffered events.</summary>
public class StreamTextResult
{
    private readonly PartBuffer _buffer;

    // The tools of the call. The UI message stream marks the parts of dynamic tools with them.
    internal IReadOnlyList<Tool>? Tools { get; init; }

    // The output specification of the call. Null means text output.
    internal OutputSpec? OutputSpecification { get; init; }

    // Copies a result. StreamTextResult<TOutput> uses it to give the same stream a typed output.
    private protected StreamTextResult(StreamTextResult result)
    {
        _buffer = result._buffer;
        Tools = result.Tools;
        OutputSpecification = result.OutputSpecification;
        Text = result.Text;
        FinishReason = result.FinishReason;
        Usage = result.Usage;
        Steps = result.Steps;
        RawFinishReason = result.RawFinishReason;
        Files = result.Files;
        FinalStep = result.FinalStep;
        Content = result.Content;
        Warnings = result.Warnings;
        ReasoningText = result.ReasoningText;
        Reasoning = result.Reasoning;
        Sources = result.Sources;
        ToolCalls = result.ToolCalls;
        StaticToolCalls = result.StaticToolCalls;
        DynamicToolCalls = result.DynamicToolCalls;
        ToolResults = result.ToolResults;
        StaticToolResults = result.StaticToolResults;
        DynamicToolResults = result.DynamicToolResults;
        Request = result.Request;
        Response = result.Response;
        ResponseMessages = result.ResponseMessages;
        ProviderMetadata = result.ProviderMetadata;
        Output = result.Output;
    }

    internal StreamTextResult(
        PartBuffer buffer,
        Task<string> text,
        Task<FinishReason> finishReason,
        Task<LanguageModelUsage> usage,
        Task<IReadOnlyList<StepResult>> steps,
        Task<JsonElement?> output,
        Task<IReadOnlyList<ModelMessage>> initialResponseMessages
    )
    {
        _buffer = buffer;
        Text = text;
        FinishReason = finishReason;
        Usage = usage;
        Steps = steps;
        RawFinishReason = FromFinalStep(steps, step => step.RawFinishReason);
        Files = FromAllSteps(steps, step => step.Files);
        FinalStep = FromFinalStep(steps, step => step);
        Content = FromAllSteps(steps, step => step.Content);
        Warnings = FromAllSteps(steps, step => step.Warnings);
        ReasoningText = FromFinalStep(steps, step => step.ReasoningText);
        Reasoning = FromFinalStep(steps, step => step.Reasoning);
        Sources = FromAllSteps(steps, step => step.Sources);
        ToolCalls = FromAllSteps(steps, step => step.ToolCalls);
        StaticToolCalls = FromAllSteps(steps, step => step.StaticToolCalls);
        DynamicToolCalls = FromAllSteps(steps, step => step.DynamicToolCalls);
        ToolResults = FromAllSteps(steps, step => step.ToolResults);
        StaticToolResults = FromAllSteps(steps, step => step.StaticToolResults);
        DynamicToolResults = FromAllSteps(steps, step => step.DynamicToolResults);
        Request = FromFinalStep(steps, step => step.Request);
        Response = FromFinalStep(steps, step => step.Response);
        ResponseMessages = AllResponseMessages(initialResponseMessages, steps);
        ProviderMetadata = FromFinalStep(steps, step => step.ProviderMetadata);
        Output = FromSteps(steps, output);

        // Steps carries the failure. These tasks fail with it, so a caller that never awaits them leaves no unobserved exception.
        _ = RawFinishReason.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = Files.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        _ = Request.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        _ = Response.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = ResponseMessages.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = ProviderMetadata.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = Output.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        _ = FinalStep.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = Content.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        _ = Warnings.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = ReasoningText.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = Reasoning.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = Sources.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        _ = ToolCalls.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = StaticToolCalls.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = DynamicToolCalls.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = ToolResults.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = StaticToolResults.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
        _ = DynamicToolResults.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        );
    }

    // The results of the approvals that settled before the first step come first, as upstream's initialResponseMessages do.
    private static async Task<IReadOnlyList<ModelMessage>> AllResponseMessages(
        Task<IReadOnlyList<ModelMessage>> initialResponseMessages,
        Task<IReadOnlyList<StepResult>> steps
    )
    {
        var completed = await steps.ConfigureAwait(false);
        var initial = await initialResponseMessages.ConfigureAwait(false);
        return initial.Concat(completed.SelectMany(step => step.Response.Messages)).ToArray();
    }

    private static async Task<JsonElement?> FromSteps(
        Task<IReadOnlyList<StepResult>> steps,
        Task<JsonElement?> output
    )
    {
        // Steps carries the failure, so a failed stream throws its error here.
        await steps.ConfigureAwait(false);
        return await output.ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<T>> FromAllSteps<T>(
        Task<IReadOnlyList<StepResult>> steps,
        Func<StepResult, IEnumerable<T>> select
    )
    {
        var completed = await steps.ConfigureAwait(false);
        return completed.SelectMany(select).ToArray();
    }

    private static async Task<T> FromFinalStep<T>(
        Task<IReadOnlyList<StepResult>> steps,
        Func<StepResult, T> select
    )
    {
        var completed = await steps.ConfigureAwait(false);
        return select(completed[completed.Count - 1]);
    }

    /// <summary>Consumes the stream without processing the parts. Use it when only the final result or the callbacks matter. Maps to <c>consumeStream</c>.</summary>
    public async Task ConsumeStreamAsync(CancellationToken cancellationToken = default)
    {
        _buffer.Claim();
        await foreach (var _ in _buffer.Read(cancellationToken).ConfigureAwait(false)) { }
    }

    /// <summary>
    /// Builds a <c>text/plain</c> response of the text deltas. Each delta is its own UTF-8 body chunk. Maps to <c>toTextStreamResponse</c>.
    /// </summary>
    public TextStreamResponse ToTextStreamResponse(
        int? status = null,
        string? statusText = null,
        HeaderCollection? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        return TextStreams.CreateTextStreamResponse(
            ReadableStream<string>.From(TextStream(cancellationToken)),
            status,
            statusText,
            headers
        );
    }

    /// <summary>Writes the text deltas to <paramref name="response"/> as <c>text/plain</c>. Maps to <c>pipeTextStreamToResponse</c>.</summary>
    public Task PipeTextStreamToResponseAsync(
        ServerResponse response,
        int? status = null,
        string? statusText = null,
        HeaderCollection? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        return TextStreams.PipeTextStreamToResponseAsync(
            response,
            ReadableStream<string>.From(TextStream(cancellationToken)),
            status,
            statusText,
            headers
        );
    }

    /// <summary>Text deltas only.</summary>
    public IAsyncEnumerable<string> TextStream(CancellationToken cancellationToken = default)
    {
        _buffer.Claim();
        return ReadTextStream(cancellationToken);
    }

    private async IAsyncEnumerable<string> ReadTextStream(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var part in _buffer.Read(cancellationToken).ConfigureAwait(false))
        {
            if (part is TextDeltaPart delta)
            {
                yield return delta.Text;
            }
        }
    }

    /// <summary>
    /// The partial structured output after each text delta that changes it: the text so far for text output, the repaired partial
    /// object for object output, the complete elements for array output, the choice for choice output, and the parsed JSON
    /// so far for JSON output. Each step starts over. Maps to <c>partialOutputStream</c>.
    /// </summary>
    public IAsyncEnumerable<JsonElement> PartialOutputStream(
        CancellationToken cancellationToken = default
    )
    {
        if (_buffer.SingleConsumer && OutputSpecification == null)
        {
            throw new Vercel.AI.Operations.UnsupportedFunctionalityException(
                "partialOutputStream without an explicit output specification with streamTextLowMemory",
                "'partialOutputStream without an explicit output specification with streamTextLowMemory' functionality not supported."
            );
        }

        _buffer.Claim();
        return ReadPartialOutputStream(cancellationToken);
    }

    /// <summary>The deprecated alias of <see cref="PartialOutputStream"/>. Maps to <c>experimental_partialOutputStream</c>.</summary>
    [Obsolete("Use PartialOutputStream instead.")]
    public IAsyncEnumerable<JsonElement> ExperimentalPartialOutputStream(
        CancellationToken cancellationToken = default
    )
    {
        return PartialOutputStream(cancellationToken);
    }

    private async IAsyncEnumerable<JsonElement> ReadPartialOutputStream(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var (partial, isNewStep) in ReadPartialOutputs(cancellationToken))
        {
            if (!isNewStep)
            {
                yield return partial!.Value;
            }
        }
    }

    /// <summary>
    /// The elements of an array output as each one completes. Empty for any other output. The stream fails with a
    /// type validation error when the model exceeds <c>maxItems</c>. Maps to <c>elementStream</c>.
    /// </summary>
    public IAsyncEnumerable<JsonElement> ElementStream(
        CancellationToken cancellationToken = default
    )
    {
        _buffer.Claim();
        return ReadElementStream(cancellationToken);
    }

    private async IAsyncEnumerable<JsonElement> ReadElementStream(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var transform = OutputSpecification?.CreateElementStreamTransform();
        if (transform == null)
        {
            yield break;
        }

        // A failure of the element transform ends the element stream, but not the generation: the stream is read to its end
        // first, so the producer finishes and the output reports its own error, as in upstream's pipeline.
        Exception? failure = null;
        await foreach (var (partial, isNewStep) in ReadPartialOutputs(cancellationToken))
        {
            if (isNewStep || failure != null)
            {
                continue;
            }

            using var elements = transform(partial!.Value).GetEnumerator();
            while (true)
            {
                JsonElement element;
                try
                {
                    if (!elements.MoveNext())
                    {
                        break;
                    }

                    element = elements.Current;
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    failure = error;
                    break;
                }

                yield return element;
            }
        }

        if (failure != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    // The partial output after each text delta of the first text block of a step that changes it. A step start is reported with isNewStep.
    // The partial outputs of a structured output are on the text deltas. Text output has none, so the text so far is the partial output.
    private async IAsyncEnumerable<(JsonElement? Partial, bool IsNewStep)> ReadPartialOutputs(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var isText = OutputSpecification is not { IsJson: true };
        string? firstTextChunkId = null;
        var text = new StringBuilder();
        await foreach (var part in _buffer.Read(cancellationToken).ConfigureAwait(false))
        {
            if (part is StartStepPart)
            {
                firstTextChunkId = null;
                text.Clear();
                yield return (null, true);
                continue;
            }

            if (part is not TextDeltaPart delta)
            {
                continue;
            }

            if (!isText)
            {
                if (delta.PartialOutput is { } partial)
                {
                    yield return (partial, false);
                }

                continue;
            }

            firstTextChunkId ??= delta.Id;
            if (delta.Id != firstTextChunkId)
            {
                continue;
            }

            if (delta.Text.Length == 0)
            {
                continue;
            }

            text.Append(delta.Text);
            yield return (JsonSerializer.SerializeToElement(text.ToString()), false);
        }
    }

    /// <summary>Every stream part, including tool calls, tool results, and finish.</summary>
    public IAsyncEnumerable<TextStreamPart> Stream(CancellationToken cancellationToken = default)
    {
        _buffer.Claim();
        return _buffer.Read(cancellationToken);
    }

    /// <summary>Full text from the last step. Completes when the stream finishes.</summary>
    public Task<string> Text { get; }

    /// <summary>The parsed structured output, or null when no output is set. Completes when the stream finishes. Maps to <c>output</c>.</summary>
    public Task<JsonElement?> Output { get; }

    /// <summary>Provider metadata of the last step. Completes when the stream finishes. Maps to <c>providerMetadata</c>.</summary>
    public Task<JsonElement?> ProviderMetadata { get; }

    /// <summary>Finish reason. Completes when the stream finishes.</summary>
    public Task<FinishReason> FinishReason { get; }

    /// <summary>The finish reason exactly as the provider reported it, or <see langword="null"/> when it reported none. Completes when the stream finishes. Maps to <c>rawFinishReason</c>.</summary>
    public Task<string?> RawFinishReason { get; }

    /// <summary>Total usage. Completes when the stream finishes.</summary>
    public Task<LanguageModelUsage> Usage { get; }

    /// <summary>Completed steps. Completes when the stream finishes.</summary>
    public Task<IReadOnlyList<StepResult>> Steps { get; }

    /// <summary>Files generated in every step. Completes when the stream finishes. Maps to <c>files</c>.</summary>
    public Task<IReadOnlyList<GeneratedFile>> Files { get; }

    /// <summary>The last completed step. Completes when the stream finishes. Maps to <c>finalStep</c>.</summary>
    public Task<StepResult> FinalStep { get; }

    /// <summary>Content from every step. See <see cref="StepResult.Content"/>. Completes when the stream finishes. Maps to <c>content</c>.</summary>
    public Task<IReadOnlyList<GeneratedContent>> Content { get; }

    /// <summary>Warnings from every step. Completes when the stream finishes. Maps to <c>warnings</c>.</summary>
    public Task<IReadOnlyList<CallWarning>> Warnings { get; }

    /// <summary>Reasoning text of the last step. Completes when the stream finishes. Maps to <c>reasoningText</c>.</summary>
    public Task<string?> ReasoningText { get; }

    /// <summary>The reasoning parts and reasoning files of the last step. Completes when the stream finishes. Maps to <c>reasoning</c>.</summary>
    public Task<IReadOnlyList<GeneratedContent>> Reasoning { get; }

    /// <summary>Sources from every step. Completes when the stream finishes. Maps to <c>sources</c>.</summary>
    public Task<IReadOnlyList<GeneratedSource>> Sources { get; }

    /// <summary>Tool calls from every step. Completes when the stream finishes. Maps to <c>toolCalls</c>.</summary>
    public Task<IReadOnlyList<GeneratedToolCall>> ToolCalls { get; }

    /// <summary>Static tool calls from every step. Completes when the stream finishes. Maps to <c>staticToolCalls</c>.</summary>
    public Task<IReadOnlyList<GeneratedToolCall>> StaticToolCalls { get; }

    /// <summary>Dynamic tool calls from every step. Completes when the stream finishes. Maps to <c>dynamicToolCalls</c>.</summary>
    public Task<IReadOnlyList<GeneratedToolCall>> DynamicToolCalls { get; }

    /// <summary>Tool results from every step. Completes when the stream finishes. Maps to <c>toolResults</c>.</summary>
    public Task<IReadOnlyList<ExecutedTool>> ToolResults { get; }

    /// <summary>Static tool results from every step. Completes when the stream finishes. Maps to <c>staticToolResults</c>.</summary>
    public Task<IReadOnlyList<ExecutedTool>> StaticToolResults { get; }

    /// <summary>Dynamic tool results from every step. Completes when the stream finishes. Maps to <c>dynamicToolResults</c>.</summary>
    public Task<IReadOnlyList<ExecutedTool>> DynamicToolResults { get; }

    /// <summary>
    /// Additional request information of the last step. <see cref="LanguageModelRequestMetadata.Body"/> and
    /// <see cref="LanguageModelRequestMetadata.Messages"/> are set only when <see cref="GenerateTextOptions.Include"/> asks for them.
    /// Completes when the stream finishes. Maps to <c>request</c>.
    /// </summary>
    public Task<LanguageModelRequestMetadata> Request { get; }

    /// <summary>Additional response information of the last step, with the messages that step added. Completes when the stream finishes. Maps to <c>response</c>.</summary>
    public Task<StepResponse> Response { get; }

    /// <summary>
    /// Messages this call added to the conversation: the assistant message of each step, with any tool approval request, and the
    /// tool results the client ran. Completes when the stream finishes. Maps to <c>responseMessages</c>.
    /// </summary>
    public Task<IReadOnlyList<ModelMessage>> ResponseMessages { get; }
}

/// <summary>
/// Result of <c>streamText</c> whose parsed output has the type of the <see cref="OutputSpec{TOutput}"/> of the call.
/// Maps to <c>StreamTextResult&lt;TOOLS, OUTPUT&gt;</c>.
/// </summary>
/// <typeparam name="TOutput">The type of the parsed output.</typeparam>
public sealed class StreamTextResult<TOutput> : StreamTextResult
{
    internal StreamTextResult(StreamTextResult result, OutputSpec<TOutput> spec)
        : base(result)
    {
        Output = Typed(result.Output, spec);

        // As the base class does for its tasks: a caller that never awaits Output leaves no unobserved exception.
        _ = Output.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// The parsed output once the stream finished, or <see langword="default"/> when none was generated. A mismatch with
    /// <typeparamref name="TOutput"/> faults the task with <see cref="JsonException"/>. Maps to <c>output</c>.
    /// </summary>
    public new Task<TOutput?> Output { get; }

    private static async Task<TOutput?> Typed(Task<JsonElement?> output, OutputSpec<TOutput> spec)
    {
        return await output.ConfigureAwait(false) is { } json ? spec.Deserialize(json) : default;
    }
}

/// <summary>
/// A single-consumer view of a text generation, without replay or final content members. Maps to <c>StreamTextSingleConsumerResult</c>.
/// Choose one stream projection. A second one throws.
/// </summary>
public sealed class StreamTextSingleConsumerResult
{
    private readonly StreamTextResult _result;

    internal StreamTextSingleConsumerResult(StreamTextResult result)
    {
        _result = result;
    }

    /// <summary>Total usage. Completes when the stream finishes. Maps to <c>totalUsage</c> and <c>usage</c>.</summary>
    public Task<LanguageModelUsage> Usage => _result.Usage;

    /// <summary>Finish reason. Completes when the stream finishes. Maps to <c>finishReason</c>.</summary>
    public Task<FinishReason> FinishReason => _result.FinishReason;

    /// <summary>The finish reason as the provider reported it. Completes when the stream finishes. Maps to <c>rawFinishReason</c>.</summary>
    public Task<string?> RawFinishReason => _result.RawFinishReason;

    /// <summary>Consumes the stream without processing the parts. Maps to <c>consumeStream</c>.</summary>
    public Task ConsumeStreamAsync(CancellationToken cancellationToken = default)
    {
        return _result.ConsumeStreamAsync(cancellationToken);
    }

#if NET
    /// <summary>Converts the stream parts into UI message chunks. Maps to <c>toUIMessageStream</c>.</summary>
    public IAsyncEnumerable<JsonObject> ToUIMessageStream(
        UIMessageStreamOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return _result.ToUIMessageStream(options, cancellationToken);
    }

    /// <summary>Returns a <c>text/event-stream</c> result of the UI message stream. Maps to <c>toUIMessageStreamResponse</c>.</summary>
    public IResult ToUIMessageStreamResult(
        int? keepAliveMs = null,
        Func<Exception, string>? onError = null,
        bool? redactToolErrors = null,
        bool sendSources = false
    )
    {
        return _result.ToUIMessageStreamResult(keepAliveMs, onError, redactToolErrors, sendSources);
    }
#endif

    /// <summary>Builds a <c>text/plain</c> response of the text deltas. Maps to <c>toTextStreamResponse</c>.</summary>
    public TextStreamResponse ToTextStreamResponse(
        int? status = null,
        string? statusText = null,
        HeaderCollection? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        return _result.ToTextStreamResponse(status, statusText, headers, cancellationToken);
    }

#if NET
    /// <summary>Pipes the UI message stream to <paramref name="response"/>. Maps to <c>pipeUIMessageStreamToResponse</c>.</summary>
    public Task PipeUIMessageStreamToResponseAsync(
        ServerResponse response,
        int? status = null,
        string? statusText = null,
        HeaderCollection? headers = null,
        int? keepAliveMs = null,
        Func<Exception, string>? onError = null,
        bool? redactToolErrors = null,
        CancellationToken cancellationToken = default,
        bool sendSources = false
    )
    {
        return _result.PipeUIMessageStreamToResponseAsync(
            response,
            status,
            statusText,
            headers,
            keepAliveMs,
            onError,
            redactToolErrors,
            cancellationToken,
            sendSources
        );
    }
#endif

    /// <summary>Pipes the text deltas to <paramref name="response"/>. Maps to <c>pipeTextStreamToResponse</c>.</summary>
    public Task PipeTextStreamToResponseAsync(
        ServerResponse response,
        int? status = null,
        string? statusText = null,
        HeaderCollection? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        return _result.PipeTextStreamToResponseAsync(
            response,
            status,
            statusText,
            headers,
            cancellationToken
        );
    }

    /// <summary>Text deltas only. Maps to <c>textStream</c>.</summary>
    public IAsyncEnumerable<string> TextStream(CancellationToken cancellationToken = default)
    {
        return _result.TextStream(cancellationToken);
    }

    /// <summary>Every stream part. Maps to <c>stream</c>.</summary>
    public IAsyncEnumerable<TextStreamPart> Stream(CancellationToken cancellationToken = default)
    {
        return _result.Stream(cancellationToken);
    }

    /// <summary>
    /// The partial structured output. It requires an explicit output specification, otherwise it throws
    /// <see cref="Vercel.AI.Operations.UnsupportedFunctionalityException"/>. Maps to <c>partialOutputStream</c>.
    /// </summary>
    public IAsyncEnumerable<JsonElement> PartialOutputStream(
        CancellationToken cancellationToken = default
    )
    {
        return _result.PartialOutputStream(cancellationToken);
    }

    /// <summary>The elements of an array output as each one completes. Maps to <c>elementStream</c>.</summary>
    public IAsyncEnumerable<JsonElement> ElementStream(
        CancellationToken cancellationToken = default
    )
    {
        return _result.ElementStream(cancellationToken);
    }
}
