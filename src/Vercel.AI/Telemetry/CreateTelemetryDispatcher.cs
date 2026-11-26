// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;

namespace Vercel.AI;

/// <summary>
/// Sends telemetry events to the resolved integrations. Per-call integrations take precedence over the global ones. Maps to
/// <c>TelemetryDispatcher</c> in <c>telemetry/telemetry.ts</c>.
/// </summary>
internal sealed class TelemetryDispatcher
{
    /// <summary>Whether the call has an enabled integration, which needs the content of the generation.</summary>
    public bool HasIntegrations { get; init; }

    /// <summary>Maps to <c>onStart</c>. Null when no integration has it.</summary>
    public Func<IOperationStartEvent, CancellationToken, Task>? OnStart { get; init; }

    /// <summary>Maps to <c>onStepStart</c>. Null when no integration has it.</summary>
    public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; init; }

    /// <summary>Maps to <c>onToolExecutionStart</c>. Null when no integration has it.</summary>
    public Func<
        ToolExecutionStartEvent,
        CancellationToken,
        Task
    >? OnToolExecutionStart { get; init; }

    /// <summary>Maps to <c>onToolExecutionEnd</c>. Null when no integration has it.</summary>
    public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; init; }

    /// <summary>Maps to <c>onStepEnd</c>, fanned out to the deprecated <c>onStepFinish</c> too. Null when no integration has either.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; init; }

    /// <summary>Maps to <c>onAbort</c>. Null when no integration has it.</summary>
    public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; init; }

    /// <summary>Maps to <c>onObjectStepStart</c>. Null when no integration has it.</summary>
    public Func<
        GenerateObjectStepStartEvent,
        CancellationToken,
        Task
    >? OnObjectStepStart { get; init; }

    /// <summary>Maps to <c>onObjectStepEnd</c>. Null when no integration has it.</summary>
    public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnObjectStepEnd { get; init; }

    /// <summary>Maps to <c>onEnd</c>. Null when no integration has it.</summary>
    public Func<IOperationEndEvent, CancellationToken, Task>? OnEnd { get; init; }

    /// <summary>Maps to <c>onLanguageModelCallStart</c>. Null when no integration has it.</summary>
    public Func<
        LanguageModelCallStartEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallStart { get; init; }

    /// <summary>Maps to <c>onLanguageModelCallEnd</c>. Null when no integration has it.</summary>
    public Func<
        LanguageModelCallEndEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallEnd { get; init; }

    /// <summary>Maps to <c>onError</c>. Null when no integration has it.</summary>
    public Func<TelemetryErrorEvent, CancellationToken, Task>? OnError { get; init; }

    /// <summary>Maps to <c>onStepError</c>. Null when no integration has it.</summary>
    public Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError { get; init; }

    /// <summary>Maps to <c>onEmbedStart</c>. Null when no integration has it.</summary>
    public Func<EmbeddingModelCallStartEvent, CancellationToken, Task>? OnEmbedStart { get; init; }

    /// <summary>Maps to <c>onEmbedEnd</c>. Null when no integration has it.</summary>
    public Func<EmbeddingModelCallEndEvent, CancellationToken, Task>? OnEmbedEnd { get; init; }

    /// <summary>Maps to <c>onRerankStart</c>. Null when no integration has it.</summary>
    public Func<RerankingModelCallStartEvent, CancellationToken, Task>? OnRerankStart { get; init; }

    /// <summary>Maps to <c>onRerankEnd</c>. Null when no integration has it.</summary>
    public Func<RerankingModelCallEndEvent, CancellationToken, Task>? OnRerankEnd { get; init; }

    /// <summary>Maps to <c>experimental_onStreamTranscriptionStart</c>. Null when no integration has it.</summary>
    public Func<
        StreamTranscriptionStartEvent,
        CancellationToken,
        Task
    >? OnStreamTranscriptionStart { get; init; }

    /// <summary>Maps to <c>experimental_onStreamTranscriptionEnd</c>. Null when no integration has it.</summary>
    public Func<
        StreamTranscriptionEndEvent,
        CancellationToken,
        Task
    >? OnStreamTranscriptionEnd { get; init; }

    internal IReadOnlyList<IExecuteLanguageModelCall> ExecuteLanguageModelCallWrappers { get; init; } =
    [];

    internal IReadOnlyList<IExecuteTool> ExecuteToolWrappers { get; init; } = [];

    internal TelemetryOptions? Options { get; init; }

    /// <summary>
    /// Runs the provider call inside the <c>executeLanguageModelCall</c> wrappers of the integrations, the first integration outermost.
    /// Without a wrapper it only runs the call. Maps to <c>executeLanguageModelCall</c>.
    /// </summary>
    internal Task<T> ExecuteLanguageModelCallAsync<T>(
        LanguageModelCallStartEvent @event,
        Func<Task<T>> execute,
        CancellationToken cancellationToken
    )
    {
        var wrapped = execute;
        if (ExecuteLanguageModelCallWrappers.Count > 0)
        {
            var augmented = AugmentEvent(@event, Options);
            foreach (var wrapper in ExecuteLanguageModelCallWrappers)
            {
                var inner = wrapped;
                var current = wrapper;
                wrapped = () => RunRedactedAsync(current, augmented, inner, cancellationToken);
            }
        }

        return RunInTracingChannelSpan(
            TelemetryTracingEventType.LanguageModelCall,
            @event,
            wrapped
        );
    }

    // Each integration sees the result and the error through their provider-owned telemetry views. The caller gets the originals back.
    private static async Task<T> RunRedactedAsync<T>(
        IExecuteLanguageModelCall wrapper,
        LanguageModelCallStartEvent augmented,
        Func<Task<T>> inner,
        CancellationToken cancellationToken
    )
    {
        var originals = new List<(object Redacted, object Original)>();
        T Redact(T original)
        {
            var redacted = RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(original);
            if (!ReferenceEquals(redacted, original))
            {
                originals.Add((redacted!, original!));
            }

            return redacted;
        }

        T Restore(T value)
        {
            foreach (var (redacted, original) in originals)
            {
                if (ReferenceEquals(redacted, value))
                {
                    return (T)original;
                }
            }

            return value;
        }

        try
        {
            return Restore(
                await wrapper
                    .ExecuteAsync(new ExecuteLanguageModelCallOptions<T>(augmented, async () =>
                        {
                            try
                            {
                                return Redact(await inner().ConfigureAwait(false));
                            }
                            catch (Exception error) when (error is not OperationCanceledException)
                            {
                                var redacted =
                                    RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(
                                        error
                                    );
                                if (!ReferenceEquals(redacted, error))
                                {
                                    originals.Add((redacted, error));
                                    throw redacted;
                                }

                                throw;
                            }
                        }), cancellationToken)
                    .ConfigureAwait(false)
            );
        }
        catch (Exception error)
        {
            foreach (var (redacted, original) in originals)
            {
                if (ReferenceEquals(redacted, error) && original is Exception originalError)
                {
                    System
                        .Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(originalError)
                        .Throw();
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Runs the tool inside the <c>executeTool</c> wrappers of the integrations, the first integration outermost. Without a wrapper it
    /// only runs the tool. Maps to <c>executeTool</c>.
    /// </summary>
    internal Task<T> ExecuteToolAsync<T>(
        ToolExecutionStartEvent @event,
        Func<Task<T>> execute,
        CancellationToken cancellationToken
    )
    {
        if (ExecuteToolWrappers.Count == 0)
        {
            return execute();
        }

        var augmented = new ToolExecutionStartEvent(@event.ToolCall, @event.Messages)
        {
            CallId = @event.CallId,
            ToolContext = FilterToolContext(@event, Options),
            RecordInputs = Options?.RecordInputs,
            RecordOutputs = Options?.RecordOutputs,
            FunctionId = Options?.FunctionId,
            AiVersion = Operations.AiSdkVersion.Version,
            Timestamp = @event.Timestamp,
        };
        var wrapped = execute;
        foreach (var wrapper in ExecuteToolWrappers)
        {
            var inner = wrapped;
            var current = wrapper;
            wrapped = () =>
                current.ExecuteAsync(
                    new ExecuteToolOptions<T>(augmented, inner),
                    cancellationToken
                );
        }

        return wrapped();
    }

    /// <summary>Maps to <c>experimental_onDecideStart</c>, or the deprecated <c>experimental_onEvaluateStart</c>. Null when no integration has it.</summary>
    public Func<DecideEvent, CancellationToken, Task>? OnDecideStart { get; init; }

    /// <summary>Maps to <c>experimental_onDecisionModelCallStart</c>, or the deprecated <c>experimental_onEvaluationModelCallStart</c>. Null when no integration has it.</summary>
    public Func<
        DecisionModelEvent,
        CancellationToken,
        Task
    >? OnDecisionModelCallStart { get; init; }

    /// <summary>Maps to <c>experimental_onDecisionModelCallEnd</c>, or the deprecated <c>experimental_onEvaluationModelCallEnd</c>. Null when no integration has it.</summary>
    public Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallEnd { get; init; }

    /// <summary>Maps to <c>experimental_onDecideEnd</c>, or the deprecated <c>experimental_onEvaluateEnd</c>. Null when no integration has it.</summary>
    public Func<DecideEvent, CancellationToken, Task>? OnDecideEnd { get; init; }

    /// <summary>Maps to <c>experimental_onExtractStart</c>. Null when no integration has it.</summary>
    public Func<ExtractStartEvent, CancellationToken, Task>? OnExtractStart { get; init; }

    /// <summary>Maps to <c>experimental_onExtractionModelCallStart</c>. Null when no integration has it.</summary>
    public Func<
        ExtractionModelCallStartEvent,
        CancellationToken,
        Task
    >? OnExtractionModelCallStart { get; init; }

    /// <summary>Maps to <c>experimental_onExtractionModelCallEnd</c>. Null when no integration has it.</summary>
    public Func<
        ExtractionModelCallEndEvent,
        CancellationToken,
        Task
    >? OnExtractionModelCallEnd { get; init; }

    /// <summary>Maps to <c>experimental_onExtractEnd</c>. Null when no integration has it.</summary>
    public Func<ExtractEndEvent, CancellationToken, Task>? OnExtractEnd { get; init; }

    /// <summary>The telemetry settings of the call. Null when tracing is off. Maps to <c>telemetryMetadata</c>.</summary>
    internal TelemetryOptions? TracingTelemetry { get; init; }

    /// <summary>Whether the dispatcher publishes tracing channel spans. False when telemetry is explicitly disabled.</summary>
    internal bool IsTracingEnabled { get; init; }

    /// <summary>
    /// Runs <paramref name="execute"/> inside a tracing channel span. Maps to <c>runInTracingChannelSpan</c>. A dispatcher without
    /// tracing runs <paramref name="execute"/> directly.
    /// </summary>
    internal async Task<T> RunInTracingChannelSpan<T>(
        string type,
        object @event,
        Func<Task<T>> execute
    )
    {
        if (!IsTracingEnabled)
        {
            return await execute().ConfigureAwait(false);
        }

        // Raw operation results may contain both inputs and outputs.
        var recordResult =
            TracingTelemetry?.RecordInputs != false && TracingTelemetry?.RecordOutputs != false;
        return await TracingChannelPublisher
            .RunWithTracingChannelSpan(
                new TelemetryTracingChannelMessage(type, AugmentTracingEvent(@event)),
                execute,
                recordResult
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a tracing channel span whose completion is observed separately. Streamed operations use it, so that the span lives as long
    /// as the stream. Maps to <c>startTracingChannelContext</c>. Null when tracing is off or nothing subscribes.
    /// </summary>
    internal TracingChannelContext? StartTracingChannelContext(
        string type,
        object @event,
        Task<object?> completion
    )
    {
        if (!IsTracingEnabled)
        {
            return null;
        }

        var recordResult =
            TracingTelemetry?.RecordInputs != false && TracingTelemetry?.RecordOutputs != false;
        return TracingChannelPublisher.OpenTelemetryChannelSpanContext(
            new TelemetryTracingChannelMessage(type, AugmentTracingEvent(@event)),
            completion,
            recordResult
        );
    }

    /// <summary>The integrations a call resolves: the per-call ones, or the global ones. Maps to the lookup in <c>createTelemetryDispatcher</c>.</summary>
    internal static IReadOnlyList<ITelemetry> ResolveIntegrations(TelemetryOptions? telemetry)
    {
        return telemetry?.Integrations ?? TelemetryRegistry.GetGlobalTelemetryIntegrations();
    }

    /// <summary>Creates the dispatcher of a text generation call.</summary>
    internal static TelemetryDispatcher Create(GenerateTextOptions options)
    {
#pragma warning disable CS0618 // the deprecated alias is read like its replacement
        return options.TelemetrySnapshot
            ?? Create(options.Telemetry ?? options.ExperimentalTelemetry);
#pragma warning restore CS0618
    }

    /// <summary>Creates the dispatcher of an object generation call.</summary>
    internal static TelemetryDispatcher Create<T>(GenerateObjectOptions<T> options)
    {
#pragma warning disable CS0618 // the deprecated alias is read like its replacement
        return Create(options.Telemetry ?? options.ExperimentalTelemetry);
#pragma warning restore CS0618
    }

    /// <summary>Creates the dispatcher of an object streaming call.</summary>
    internal static TelemetryDispatcher Create<T>(StreamObjectOptions<T> options)
    {
#pragma warning disable CS0618 // the deprecated alias is read like its replacement
        return Create(options.Telemetry ?? options.ExperimentalTelemetry);
#pragma warning restore CS0618
    }

    /// <summary>Creates the dispatcher of a call. Maps to <c>createTelemetryDispatcher</c>.</summary>
    internal static TelemetryDispatcher Create(TelemetryOptions? telemetry)
    {
        // When telemetry is explicitly disabled, the dispatcher performs no work.
        if (telemetry?.IsEnabled == false)
        {
            return new TelemetryDispatcher();
        }

        var integrations = ResolveIntegrations(telemetry);
#pragma warning disable CS0618 // the deprecated evaluate callbacks keep working like upstream
        return new TelemetryDispatcher
        {
            HasIntegrations = integrations.Count > 0,
            Options = telemetry,
            ExecuteLanguageModelCallWrappers = integrations
                .Select(i => i.ExecuteLanguageModelCall)
                .OfType<IExecuteLanguageModelCall>()
                .ToList(),
            ExecuteToolWrappers = integrations
                .Select(i => i.ExecuteTool)
                .OfType<IExecuteTool>()
                .ToList(),
            IsTracingEnabled = true,
            TracingTelemetry = telemetry,
            OnObjectStepStart = Filter<GenerateObjectStepStartEvent>(
                Merge(integrations.Select(i => i.OnObjectStepStart)),
                e => e.AugmentEvent(telemetry)
            ),
            OnObjectStepEnd = Filter<GenerateObjectStepEndEvent>(
                Merge(integrations.Select(i => i.OnObjectStepEnd)),
                e => e.AugmentEvent(telemetry)
            ),
            OnEnd = Filter<IOperationEndEvent>(Merge(integrations.Select(i => i.OnEnd)), e =>
                e switch
                {
                    IAugmentableEndEvent augmentable => augmentable.AugmentEvent(telemetry),
                    GenerateTextResult result => AugmentResult(result, telemetry),
                    EmbedEndEvent embed => FilterTelemetryEventHelper.FilterTelemetryEvent(
                        embed,
                        telemetry
                    ),
                    RerankEndEvent rerank => FilterTelemetryEventHelper.FilterTelemetryEvent(
                        rerank,
                        telemetry
                    ),
                    TranscriptionEndEvent transcription =>
                        FilterTelemetryEventHelper.FilterTelemetryEvent(transcription, telemetry),
                    GenerateSpeechEndEvent speech =>
                        FilterTelemetryEventHelper.FilterTelemetryEvent(speech, telemetry),
                    IRecordingFilterableEndEvent filterable
                        when telemetry?.RecordInputs == false
                            || telemetry?.RecordOutputs == false => filterable.FilterTelemetryEvent(
                        telemetry.RecordInputs == false,
                        telemetry.RecordOutputs == false
                    ),
                    _ => e,
                }
            ),
            OnLanguageModelCallStart = Filter<LanguageModelCallStartEvent>(
                Merge(integrations.Select(i => i.OnLanguageModelCallStart)),
                e => AugmentEvent(e, telemetry)
            ),
            OnLanguageModelCallEnd = Filter<LanguageModelCallEndEvent>(
                Merge(integrations.Select(i => i.OnLanguageModelCallEnd)),
                e => AugmentEvent(e, telemetry)
            ),
            OnStart = Filter<IOperationStartEvent>(Merge(integrations.Select(i => i.OnStart)), e =>
                e is GenerateTextStartEvent textStart
                    ? textStart
                        .WithContexts(
                            FilterIncludedContextHelper.FilterIncludedContext(
                                textStart.RuntimeContext,
                                telemetry?.IncludeRuntimeContext
                            ),
                            FilterIncludedContextHelper.FilterToolsContext(
                                textStart.ToolsContext,
                                telemetry?.IncludeToolsContext
                            )
                        )
                        .WithTelemetry(telemetry)
                    : e switch
                    {
                        GenerateObjectStartEvent objectStart => objectStart.AugmentEvent(telemetry),
                        EmbedStartEvent embed => FilterTelemetryEventHelper.FilterTelemetryEvent(
                            embed,
                            telemetry
                        ),
                        RerankStartEvent rerank => FilterTelemetryEventHelper.FilterTelemetryEvent(
                            rerank,
                            telemetry
                        ),
                        TranscriptionStartEvent transcription =>
                            FilterTelemetryEventHelper.FilterTelemetryEvent(
                                transcription,
                                telemetry
                            ),
                        GenerateSpeechStartEvent speech =>
                            FilterTelemetryEventHelper.FilterTelemetryEvent(speech, telemetry),
                        _ => e,
                    }
            ),
            OnStepStart = Filter<GenerateTextStepStartEvent>(
                Merge(integrations.Select(i => i.OnStepStart)),
                e =>
                    e.WithContexts(
                        FilterIncludedContextHelper.FilterIncludedContext(
                            e.RuntimeContext,
                            telemetry?.IncludeRuntimeContext
                        ),
                        FilterIncludedContextHelper.FilterToolsContext(
                            e.ToolsContext,
                            telemetry?.IncludeToolsContext
                        ),
                        e.Steps.Select(step => FilterStep(step, telemetry)).ToList()
                    )
            ),
            OnToolExecutionStart = Filter<ToolExecutionStartEvent>(
                Merge(integrations.Select(i => i.OnToolExecutionStart)),
                e =>
                    FilterTelemetryEventHelper.FilterToolEvent(
                        new ToolExecutionStartEvent(e.ToolCall, e.Messages)
                        {
                            CallId = e.CallId,
                            ToolContext = FilterToolContext(e, telemetry),
                        },
                        telemetry
                    )
            ),
            OnToolExecutionEnd = Filter<ToolExecutionEndEvent>(
                Merge(integrations.Select(i => i.OnToolExecutionEnd)),
                e =>
                    FilterTelemetryEventHelper.FilterToolEvent(
                        new ToolExecutionEndEvent(
                            e.ToolCall,
                            e.Messages,
                            e.ToolOutput,
                            e.ToolExecutionTime
                        )
                        {
                            CallId = e.CallId,
                            ToolContext = FilterToolContext(e, telemetry),
                        },
                        telemetry
                    )
            ),
            // Fans out to the deprecated onStepFinish too, so integrations that only implement it keep working.
            OnStepEnd = Filter<StepResult>(
                Merge(
                    integrations
                        .Select(i => i.OnStepEnd)
                        .Concat(integrations.Select(i => i.OnStepFinish))
                ),
                step => FilterStep(step, telemetry)
            ),
            OnAbort = Filter<StreamAbortContext>(
                Merge(integrations.Select(i => i.OnAbort)),
                context => new StreamAbortContext(
                    context.Steps.Select(step => FilterStep(step, telemetry)).ToList(),
                    context.Exception,
                    context.CallId
                )
            ),
            OnError = Filter(Merge(integrations.Select(i => i.OnError)), (TelemetryErrorEvent e) =>
                FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnStepError = Filter<GenerateTextStepErrorEvent>(
                Merge(integrations.Select(i => i.OnStepError)),
                e => FilterStepError(e, telemetry)
            ),
            OnEmbedStart = Filter(
                Merge(integrations.Select(i => i.OnEmbedStart)),
                (EmbeddingModelCallStartEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnEmbedEnd = Filter(
                Merge(integrations.Select(i => i.OnEmbedEnd)),
                (EmbeddingModelCallEndEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnRerankStart = Filter(
                Merge(integrations.Select(i => i.OnRerankStart)),
                (RerankingModelCallStartEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnRerankEnd = Filter(
                Merge(integrations.Select(i => i.OnRerankEnd)),
                (RerankingModelCallEndEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnStreamTranscriptionStart = Filter(
                Merge(integrations.Select(i => i.OnStreamTranscriptionStart)),
                (StreamTranscriptionStartEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnStreamTranscriptionEnd = Filter(
                Merge(integrations.Select(i => i.OnStreamTranscriptionEnd)),
                (StreamTranscriptionEndEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnDecideStart = Filter(
                Merge(
                    integrations.Select(i =>
                        i.OnDecideStart
                        ?? Adapt<DecideEvent, EvaluateEvent>(i.OnEvaluateStart, EvaluateEvent.From)
                    )
                ),
                (DecideEvent e) => FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnDecisionModelCallStart = Filter(
                Merge(
                    integrations.Select(i =>
                        i.OnDecisionModelCallStart
                        ?? Adapt<DecisionModelEvent, EvaluateModelEvent>(
                            i.OnEvaluationModelCallStart,
                            EvaluateModelEvent.From
                        )
                    )
                ),
                (DecisionModelEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnDecisionModelCallEnd = Filter(
                Merge(
                    integrations.Select(i =>
                        i.OnDecisionModelCallEnd
                        ?? Adapt<DecisionModelEvent, EvaluateModelEvent>(
                            i.OnEvaluationModelCallEnd,
                            EvaluateModelEvent.From
                        )
                    )
                ),
                (DecisionModelEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnDecideEnd = Filter(
                Merge(
                    integrations.Select(i =>
                        i.OnDecideEnd
                        ?? Adapt<DecideEvent, EvaluateEvent>(i.OnEvaluateEnd, EvaluateEvent.From)
                    )
                ),
                (DecideEvent e) => FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnExtractStart = Filter(
                Merge(integrations.Select(i => i.OnExtractStart)),
                (ExtractStartEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnExtractionModelCallStart = Filter(
                Merge(integrations.Select(i => i.OnExtractionModelCallStart)),
                (ExtractionModelCallStartEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnExtractionModelCallEnd = Filter(
                Merge(integrations.Select(i => i.OnExtractionModelCallEnd)),
                (ExtractionModelCallEndEvent e) =>
                    FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
            OnExtractEnd = Filter(
                Merge(integrations.Select(i => i.OnExtractEnd)),
                (ExtractEndEvent e) => FilterTelemetryEventHelper.FilterTelemetryEvent(e, telemetry)
            ),
        };
#pragma warning restore CS0618
    }

    // Maps to augmentEvent with filterContext: the settings and version, the runtime context filter, then the content filter.
    private object AugmentTracingEvent(object value)
    {
        var telemetry = TracingTelemetry;
        switch (value)
        {
            case ExtractStartEvent extract:
                return FilterTelemetryEventHelper.FilterTelemetryEvent(
                    new ExtractStartEvent(
                        extract.CallId,
                        FilterIncludedContextHelper.FilterIncludedContext(
                            extract.RuntimeContext,
                            telemetry?.IncludeRuntimeContext
                        ),
                        extract.Provider,
                        extract.ModelId,
                        extract.State,
                        extract.Fields,
                        extract.ReferenceDate,
                        extract.MaxOutputTokens,
                        extract.Reasoning,
                        extract.MaxRetries,
                        extract.Headers,
                        extract.ProviderOptions
                    ),
                    telemetry
                );
            case EmbedStartEvent embed:
                return FilterTelemetryEventHelper.FilterTelemetryEvent(
                    new EmbedStartEvent(
                        embed.CallId,
                        embed.OperationId,
                        FilterIncludedContextHelper.FilterIncludedContext(
                            embed.RuntimeContext,
                            telemetry?.IncludeRuntimeContext
                        ),
                        embed.Provider,
                        embed.ModelId,
                        embed.Value,
                        embed.MaxRetries,
                        embed.Headers,
                        embed.ProviderOptions,
                        embed.Dimensions
                    ),
                    telemetry
                );
            case RerankStartEvent rerank:
                return FilterTelemetryEventHelper.FilterTelemetryEvent(
                    new RerankStartEvent(
                        rerank.CallId,
                        rerank.OperationId,
                        FilterIncludedContextHelper.FilterIncludedContext(
                            rerank.RuntimeContext,
                            telemetry?.IncludeRuntimeContext
                        ),
                        rerank.Provider,
                        rerank.ModelId,
                        rerank.Documents,
                        rerank.Query,
                        rerank.TopN,
                        rerank.MaxRetries,
                        rerank.Headers,
                        rerank.ProviderOptions
                    ),
                    telemetry
                );
            case DecideEvent decide:
                return FilterTelemetryEventHelper.FilterTelemetryEvent(
                    new DecideEvent(
                        decide.CallId,
                        decide.OperationId,
                        FilterIncludedContextHelper.FilterIncludedContext(
                            decide.RuntimeContext,
                            telemetry?.IncludeRuntimeContext
                        ),
                        decide.Provider,
                        decide.ModelId,
                        decide.State,
                        decide.Questions,
                        decide.MaxRetries,
                        decide.Headers,
                        decide.ProviderOptions,
                        decide.Answers,
                        decide.Usage,
                        decide.Warnings,
                        maxOutputTokens: decide.MaxOutputTokens,
                        reasoning: decide.Reasoning
                    ),
                    telemetry
                );
            case GenerateTextStartEvent textStart:
                return textStart
                    .WithContexts(
                        FilterIncludedContextHelper.FilterIncludedContext(
                            textStart.RuntimeContext,
                            telemetry?.IncludeRuntimeContext
                        ),
                        FilterIncludedContextHelper.FilterToolsContext(
                            textStart.ToolsContext,
                            telemetry?.IncludeToolsContext
                        )
                    )
                    .WithTelemetry(telemetry);
            case LanguageModelCallStartEvent modelCall:
                return AugmentEvent(modelCall, telemetry);
            case ToolExecutionStartEvent toolExecution:
                return FilterTelemetryEventHelper.FilterToolEvent(
                    new ToolExecutionStartEvent(toolExecution.ToolCall, toolExecution.Messages)
                    {
                        CallId = toolExecution.CallId,
                        ToolContext = FilterToolContext(toolExecution, telemetry),
                        RecordInputs = telemetry?.RecordInputs,
                        RecordOutputs = telemetry?.RecordOutputs,
                        FunctionId = telemetry?.FunctionId,
                        AiVersion = Operations.AiSdkVersion.Version,
                    },
                    telemetry
                );
            case TracingChannelStepEvent step:
                return new TracingChannelStepEvent(step.CallId, step.StepNumber)
                {
                    RecordInputs = telemetry?.RecordInputs,
                    RecordOutputs = telemetry?.RecordOutputs,
                    FunctionId = telemetry?.FunctionId,
                    AiVersion = Operations.AiSdkVersion.Version,
                };
            default:
                return value;
        }
    }

    // Maps a deprecated callback, which takes a subclass of the event, onto the current event type.
    private static Func<TEvent, CancellationToken, Task>? Adapt<TEvent, TLegacy>(
        Func<TLegacy, CancellationToken, Task>? legacy,
        Func<TEvent, TLegacy> convert
    )
    {
        return legacy == null
            ? null
            : (e, cancellationToken) => legacy(convert(e), cancellationToken);
    }

    // Adds the telemetry settings and the SDK version to an event, then strips the inputs and outputs the call does not record. Maps to augmentEvent.
    private static LanguageModelCallStartEvent AugmentEvent(
        LanguageModelCallStartEvent value,
        TelemetryOptions? telemetry
    )
    {
        var augmented = new LanguageModelCallStartEvent
        {
            CallId = value.CallId,
            Provider = value.Provider,
            ModelId = value.ModelId,
            Instructions = value.Instructions,
            Messages = value.Messages,
            Tools = value.Tools,
            CallOptions = value.CallOptions,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
            AiVersion = Operations.AiSdkVersion.Version,
        };
        return FilterTelemetryEventHelper.FilterTelemetryEvent(augmented, telemetry);
    }

    // Maps to augmentEvent.
    private static LanguageModelCallEndEvent AugmentEvent(
        LanguageModelCallEndEvent value,
        TelemetryOptions? telemetry
    )
    {
        var augmented = new LanguageModelCallEndEvent(
            value.Provider,
            value.ModelId,
            value.FinishReason,
            value.Usage,
            value.Content,
            value.ResponseId,
            value.ProviderMetadata,
            value.ResponseTime
        )
        {
            CallId = value.CallId,
            Request = value.Request,
            Performance = value.Performance,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
            AiVersion = Operations.AiSdkVersion.Version,
        };
        return FilterTelemetryEventHelper.FilterTelemetryEvent(augmented, telemetry);
    }

    // Maps to augmentEvent: the settings and version, the context filter, the content filter, and the credential redaction.
    private static GenerateTextResult AugmentResult(
        GenerateTextResult result,
        TelemetryOptions? telemetry
    )
    {
        var augmented = FilterResult(result, telemetry);
        augmented = new GenerateTextResult(
            augmented.Text,
            augmented.ReasoningText,
            augmented.Steps,
            augmented.FinishReason,
            augmented.Usage,
            augmented.Output,
            augmented.Sources,
            augmented.ProviderMetadata
        )
        {
            ResponseMessages = augmented.ResponseMessages,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
            AiVersion = Operations.AiSdkVersion.Version,
        };
        return FilterTelemetryEventHelper.FilterTelemetryEvent(
            RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(augmented),
            telemetry
        );
    }

    // Integrations see only the runtime and tool context properties the call includes. Maps to the filterContext step of augmentEvent.
    private static GenerateTextResult FilterResult(
        GenerateTextResult result,
        TelemetryOptions? telemetry
    )
    {
        var steps = result.Steps.Select(step => FilterStep(step, telemetry)).ToList();
        return result.WithSteps(steps);
    }

    // Maps to augmentEvent for a step error: the step start event keeps only the included contexts and the error is redacted.
    private static GenerateTextStepErrorEvent FilterStepError(
        GenerateTextStepErrorEvent e,
        TelemetryOptions? telemetry
    )
    {
        var hidden = telemetry?.RecordInputs == false || telemetry?.RecordOutputs == false;
        var step = e.WithContexts(
            FilterIncludedContextHelper.FilterIncludedContext(
                e.RuntimeContext,
                telemetry?.IncludeRuntimeContext
            ),
            FilterIncludedContextHelper.FilterToolsContext(
                e.ToolsContext,
                telemetry?.IncludeToolsContext
            ),
            e.Steps.Select(s => FilterStep(s, telemetry)).ToList()
        );
        return new GenerateTextStepErrorEvent(
            step,
            hidden
                ? new InvalidOperationException("AI SDK operation failed")
                : RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(e.Error)
        );
    }

    private static StepResult FilterStep(StepResult step, TelemetryOptions? telemetry)
    {
        return step.WithContexts(
            FilterIncludedContextHelper.FilterIncludedContext(
                step.RuntimeContext,
                telemetry?.IncludeRuntimeContext
            ),
            FilterIncludedContextHelper.FilterToolsContext(
                step.ToolsContext,
                telemetry?.IncludeToolsContext
            )
        );
    }

    // Maps to the toolContext branch of augmentEvent: the context of the called tool is filtered by name.
    private static JsonElement? FilterToolContext(
        ToolExecutionStartEvent e,
        TelemetryOptions? telemetry
    )
    {
        return e.ToolContext is { } toolContext
            ? FilterIncludedContextHelper.FilterToolContext(
                e.ToolCall.ToolName,
                toolContext,
                telemetry?.IncludeToolsContext
            )
            : null;
    }

    private static Func<TEvent, CancellationToken, Task>? Filter<TEvent>(
        Func<TEvent, CancellationToken, Task>? callback,
        Func<TEvent, TEvent> filter
    )
    {
        return callback == null
            ? null
            : (e, cancellationToken) => callback(filter(e), cancellationToken);
    }

    private static Func<TEvent, CancellationToken, Task>? Merge<TEvent>(
        IEnumerable<Func<TEvent, CancellationToken, Task>?> callbacks
    )
    {
        var list = callbacks.Where(c => c != null).ToList();
        if (list.Count == 0)
        {
            return null;
        }

        return async (e, cancellationToken) =>
        {
            foreach (var callback in list)
            {
                // Like upstream notify, an integration failure does not break the generation.
                await Generation
                    .NotifyAsync(() => callback!(e, cancellationToken), cancellationToken)
                    .ConfigureAwait(false);
            }
        };
    }
}
