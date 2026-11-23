// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenTelemetry;

/// <summary>
/// Telemetry integration that records GenAI spans on an <see cref="ActivitySource"/>. Maps to <c>OpenTelemetry</c> in
/// <c>packages/otel/src/open-telemetry.ts</c>. Pass it in <see cref="TelemetryOptions.Integrations"/> or register it with
/// <see cref="TelemetryRegistry.RegisterTelemetry"/>.
/// </summary>
/// <remarks>
/// Only the callbacks <see cref="ITelemetry"/> has are ported: text generation (root, step, model call, and tool spans), object
/// generation, embed, rerank, speech, transcription, decide, and extract. The durable parts of upstream need events
/// the port does not emit yet.
/// </remarks>
public class OpenTelemetry : ITelemetry
{
    private static readonly Lazy<ActivitySource> DefaultTracer = new(() =>
        new ActivitySource("gen_ai")
    );

    private static readonly double[] InferenceDurationBucketBoundaries =
    [
        0.01,
        0.02,
        0.04,
        0.08,
        0.16,
        0.32,
        0.64,
        1.28,
        2.56,
        5.12,
        10.24,
        20.48,
        40.96,
        81.92,
    ];

    private static readonly Lazy<Histogram<double>> InferenceDuration = new(() =>
        new Meter("gen_ai").CreateHistogram<double>(
            "gen_ai.client.inference.duration",
            "s",
            "GenAI client inference operation duration."
#if NET9_0_OR_GREATER
            ,
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = InferenceDurationBucketBoundaries,
            }
#endif
        )
    );

    private readonly ConcurrentDictionary<string, CallState> _callStates = new(
        StringComparer.Ordinal
    );
    private readonly ActivitySource _tracer;
    private readonly AgentSpanName? _agentSpanName;
    private readonly SupplementalAttributeOptions _supplementalAttributes;
    private readonly EnrichSpan? _enrichSpan;

    /// <summary>Creates the integration.</summary>
    /// <param name="options">Options. Null uses the defaults.</param>
    public OpenTelemetry(OpenTelemetryOptions? options = null)
    {
        options ??= new OpenTelemetryOptions();
        _tracer = options.Tracer ?? DefaultTracer.Value;
        _agentSpanName = options.AgentSpanName;
        _supplementalAttributes = SupplementalAttributeOptions.Normalize(options);
        _enrichSpan = options.EnrichSpan;
    }

    /// <inheritdoc />
    public Func<IOperationStartEvent, CancellationToken, Task>? OnStart =>
        (e, _) =>
        {
            switch (e)
            {
                case GenerateSpeechStartEvent speech:
                    OnAudioOperationStart(
                        speech.CallId,
                        speech.OperationId,
                        speech.Provider,
                        speech.ModelId,
                        speech.RecordInputs,
                        speech.RecordOutputs,
                        speech.FunctionId,
                        speech.Headers,
                        audio: null,
                        speech.Text
                    );
                    break;
                case TranscriptionStartEvent transcription:
                    OnAudioOperationStart(
                        transcription.CallId,
                        transcription.OperationId,
                        transcription.Provider,
                        transcription.ModelId,
                        transcription.RecordInputs,
                        transcription.RecordOutputs,
                        transcription.FunctionId,
                        transcription.Headers,
                        transcription.Audio,
                        text: null
                    );
                    break;
                case EmbedStartEvent embed:
                    OnEmbedOperationStart(embed);
                    break;
                case RerankStartEvent rerank:
                    OnRerankOperationStart(rerank);
                    break;
                case GenerateTextStartEvent start:
                    OnGenerateStart(start);
                    break;
                case GenerateObjectStartEvent objectStart:
                    OnObjectOperationStart(objectStart);
                    break;
            }

            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnObjectStepStart =>
        (e, _) =>
        {
            OnObjectStepStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnObjectStepEnd =>
        (e, _) =>
        {
            OnObjectStepEndCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart =>
        (e, _) =>
        {
            OnStepStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<ToolExecutionStartEvent, CancellationToken, Task>? OnToolExecutionStart =>
        (e, _) =>
        {
            OnToolExecutionStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd =>
        (e, _) =>
        {
            OnToolExecutionEndCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<StepResult, CancellationToken, Task>? OnStepEnd =>
        (e, _) =>
        {
            OnStepEndCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<StepResult, CancellationToken, Task>? OnStepFinish => null;

    /// <inheritdoc />
    public Func<StreamAbortContext, CancellationToken, Task>? OnAbort =>
        (e, _) =>
        {
            OnAbortCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<TelemetryErrorEvent, CancellationToken, Task>? OnError =>
        (e, _) =>
        {
            OnErrorCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<IOperationEndEvent, CancellationToken, Task>? OnEnd =>
        (e, _) =>
        {
            if (!TryGetCallState(CallIdOf(e), out var state) || state.RootSpan is null)
            {
                return Task.CompletedTask;
            }

            switch (e)
            {
                case EmbedEndEvent embed:
                    OnEmbedOperationEnd(embed);
                    break;
                case RerankEndEvent rerank:
                    OnRerankOperationEnd(rerank);
                    break;
                case GenerateSpeechEndEvent speech:
                    OnAudioOperationEnd(
                        speech.CallId,
                        inputAudio: null,
                        outputAudio: speech.Audio,
                        transcript: null,
                        speech.Usage,
                        speech.ProviderMetadata
                    );
                    break;
                case TranscriptionEndEvent transcription:
                    OnAudioOperationEnd(
                        transcription.CallId,
                        inputAudio: transcription.Audio,
                        outputAudio: null,
                        transcription.Text,
                        transcription.Usage,
                        transcription.ProviderMetadata
                    );
                    break;
                case GenerateTextResult result:
                    OnGenerateEnd(result);
                    break;
                case IGenerateObjectEndEvent objectEnd:
                    OnObjectOperationEnd(objectEnd);
                    break;
            }

            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart =>
        (e, _) =>
        {
            OnLanguageModelCallStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd =>
        (e, _) =>
        {
            OnLanguageModelCallEndCore(e);
            return Task.CompletedTask;
        };

#pragma warning disable CS0618 // The current hooks call the deprecated ones on purpose.
    // The current hooks route through the deprecated ones, so a subclass that overrides a deprecated hook keeps working.
    /// <inheritdoc />
    public virtual Func<DecideEvent, CancellationToken, Task>? OnDecideStart =>
        (e, ct) => OnEvaluateStart?.Invoke(EvaluateEvent.From(e), ct) ?? Task.CompletedTask;

    /// <inheritdoc />
    public virtual Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallStart =>
        (e, ct) =>
            OnEvaluationModelCallStart?.Invoke(EvaluateModelEvent.From(e), ct)
            ?? Task.CompletedTask;

    /// <inheritdoc />
    public virtual Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallEnd =>
        (e, ct) =>
            OnEvaluationModelCallEnd?.Invoke(EvaluateModelEvent.From(e), ct) ?? Task.CompletedTask;

    /// <inheritdoc />
    public virtual Func<DecideEvent, CancellationToken, Task>? OnDecideEnd =>
        (e, ct) => OnEvaluateEnd?.Invoke(EvaluateEvent.From(e), ct) ?? Task.CompletedTask;

    /// <inheritdoc />
    [Obsolete("Use OnDecideStart instead.")]
    public virtual Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateStart =>
        (e, _) =>
        {
            OnDecideOperationStart(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    [Obsolete("Use OnDecisionModelCallStart instead.")]
    public virtual Func<EvaluateModelEvent, CancellationToken, Task>? OnEvaluationModelCallStart =>
        (e, _) =>
        {
            OnDecisionModelCallStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    [Obsolete("Use OnDecisionModelCallEnd instead.")]
    public virtual Func<EvaluateModelEvent, CancellationToken, Task>? OnEvaluationModelCallEnd =>
        (e, _) =>
        {
            OnDecisionModelCallEndCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    [Obsolete("Use OnDecideEnd instead.")]
    public virtual Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateEnd =>
        (e, _) =>
        {
            OnDecideOperationEnd(e);
            return Task.CompletedTask;
        };

#pragma warning restore CS0618

    /// <inheritdoc />
    public Func<ExtractStartEvent, CancellationToken, Task>? OnExtractStart =>
        (e, _) =>
        {
            OnExtractStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<
        ExtractionModelCallStartEvent,
        CancellationToken,
        Task
    >? OnExtractionModelCallStart =>
        (e, _) =>
        {
            OnExtractionModelCallStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<ExtractionModelCallEndEvent, CancellationToken, Task>? OnExtractionModelCallEnd =>
        (e, _) =>
        {
            OnExtractionModelCallEndCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<ExtractEndEvent, CancellationToken, Task>? OnExtractEnd =>
        (e, _) =>
        {
            OnExtractEndCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError =>
        (e, _) =>
        {
            OnStepErrorCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<EmbeddingModelCallStartEvent, CancellationToken, Task>? OnEmbedStart =>
        (e, _) =>
        {
            OnEmbedStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<EmbeddingModelCallEndEvent, CancellationToken, Task>? OnEmbedEnd =>
        (e, _) =>
        {
            OnEmbedEndCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<RerankingModelCallStartEvent, CancellationToken, Task>? OnRerankStart =>
        (e, _) =>
        {
            OnRerankStartCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<RerankingModelCallEndEvent, CancellationToken, Task>? OnRerankEnd =>
        (e, _) =>
        {
            OnRerankEndCore(e);
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<
        StreamTranscriptionStartEvent,
        CancellationToken,
        Task
    >? OnStreamTranscriptionStart =>
        (e, _) =>
        {
            OnAudioOperationStart(
                e.CallId,
                e.OperationId,
                e.Provider,
                e.ModelId,
                e.RecordInputs,
                e.RecordOutputs,
                e.FunctionId,
                e.Headers,
                e.Audio,
                text: null
            );
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public Func<StreamTranscriptionEndEvent, CancellationToken, Task>? OnStreamTranscriptionEnd =>
        (e, _) =>
        {
            OnAudioOperationEnd(
                e.CallId,
                inputAudio: e.Audio,
                outputAudio: null,
                e.Text,
                e.Usage,
                e.ProviderMetadata
            );
            return Task.CompletedTask;
        };

    /// <inheritdoc />
    public IExecuteLanguageModelCall? ExecuteLanguageModelCall => null;

    /// <inheritdoc />
    public IExecuteTool? ExecuteTool => null;

    private static ResolvableTelemetryAttribute In(Func<object?> resolve) =>
        ResolvableTelemetryAttribute.Input(resolve);

    private static ResolvableTelemetryAttribute Out(Func<object?> resolve) =>
        ResolvableTelemetryAttribute.Output(resolve);

    private static Dictionary<string, object?> Merge(
        params IEnumerable<KeyValuePair<string, object?>>[] maps
    )
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var map in maps)
        {
            foreach (var pair in map)
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }

    private static TelemetrySelection SelectionOf(bool? recordInputs, bool? recordOutputs) =>
        new() { RecordInputs = recordInputs, RecordOutputs = recordOutputs };

    private static object? Setting(CallState state, string key) =>
        state.Settings.TryGetValue(key, out var value) ? value : null;

    private static void End(Activity? span)
    {
        if (span is null)
        {
            return;
        }

        var current = Activity.Current;
        span.Stop();
        Activity.Current = current == span ? span.Parent : current;
    }

    private static double? MsToSeconds(double? durationMs) => durationMs / 1000;

    /// <summary>Maps to <c>getGenAIClientPerformanceAttributes</c>.</summary>
    private static Dictionary<string, object?> GetGenAIClientPerformanceAttributes(
        Vercel.AI.GenerateText.ModelCallPerformance? performance,
        TimeSpan responseTime
    ) =>
        new()
        {
            ["gen_ai.client.operation.duration"] = MsToSeconds(
                performance?.ResponseTimeMs ?? responseTime.TotalMilliseconds
            ),
            ["gen_ai.client.operation.time_to_first_chunk"] = MsToSeconds(
                performance?.TimeToFirstOutputMs
            ),
            ["gen_ai.client.operation.time_per_output_chunk"] = MsToSeconds(
                performance?.TimeBetweenOutputChunksMs?.Avg
            ),
        };

    private static string ToolChoiceJson(ToolChoice toolChoice) =>
        toolChoice is ToolChoice.NamedChoice named
            ? "{\"type\":\"tool\",\"toolName\":" + JsonSerializer.Serialize(named.ToolName) + "}"
            : "{\"type\":" + JsonSerializer.Serialize(toolChoice.Type) + "}";

    /// <summary>Maps to <c>recordInferenceDuration</c>.</summary>
    private static void RecordInferenceDuration(
        CallState state,
        double? responseTimeMs = null,
        string? provider = null,
        string? responseModelId = null,
        string? errorType = null
    )
    {
        var inference = state.InferenceMetrics;
        if (inference is null)
        {
            return;
        }

        state.InferenceMetrics = null;
        var durationMs = responseTimeMs
            ?? (Stopwatch.GetTimestamp() - inference.StartTimestamp) * 1000.0 / Stopwatch.Frequency;

        var attributes = TelemetryAttributes.SelectAttributes(
            state.Telemetry,
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "chat",
                ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(
                    provider ?? inference.Provider
                ),
                ["gen_ai.request.model"] = inference.ModelId,
                ["gen_ai.response.model"] = responseModelId,
                ["error.type"] = errorType,
            }
        );
        var tags = new TagList();
        foreach (var pair in attributes)
        {
            tags.Add(new KeyValuePair<string, object?>(pair.Key, pair.Value));
        }

        InferenceDuration.Value.Record(durationMs / 1000, tags);
    }

    private static string? CallIdOf(IOperationEndEvent e) =>
        e switch
        {
            GenerateTextResult result => result.CallId,
            EmbedEndEvent embed => embed.CallId,
            RerankEndEvent rerank => rerank.CallId,
            GenerateSpeechEndEvent speech => speech.CallId,
            TranscriptionEndEvent transcription => transcription.CallId,
            _ => null,
        };

    private bool TryGetCallState(string? callId, out CallState state)
    {
        state = null!;
        return callId is not null && _callStates.TryGetValue(callId, out state!);
    }

    private Activity? StartSpan(
        string name,
        ActivityKind kind,
        IReadOnlyDictionary<string, object?> attributes,
        ActivityContext parent
    )
    {
        var previous = Activity.Current;
        var span = _tracer.StartActivity(
            name,
            kind,
            parent,
            attributes,
            links: null,
            startTime: default
        );
        Activity.Current = previous;
        return span;
    }

    private Dictionary<string, object?> GetSpanAttributes(
        IReadOnlyDictionary<string, object?> attributes,
        OpenTelemetrySpanType spanType,
        string operationId,
        string callId,
        IReadOnlyDictionary<string, object?>? runtimeContext
    )
    {
        IReadOnlyDictionary<string, object?>? custom;
        try
        {
            custom = _enrichSpan?.Invoke(
                new EnrichSpanContext
                {
                    SpanType = spanType,
                    OperationId = operationId,
                    CallId = callId,
                    RuntimeContext = runtimeContext,
                }
            );
        }
        catch (Exception)
        {
            custom = null;
        }

        return Merge(TelemetryAttributes.Sanitize(custom), attributes);
    }

    /// <summary>Maps to <c>getAgentSpanName</c>.</summary>
    private string GetAgentSpanName(string operationName, string modelId, string? functionId)
    {
        var defaultName = operationName + " " + modelId;

        // Other operations, such as harness turns, share the generation handler.
        if (operationName != "invoke_agent" || _agentSpanName is null)
        {
            return defaultName;
        }

        if (_agentSpanName.IsFunctionId)
        {
            return "invoke_agent " + (functionId ?? modelId);
        }

        if (_agentSpanName.Callback is { } callback)
        {
            try
            {
                return callback(modelId, functionId) ?? defaultName;
            }
            catch (Exception)
            {
                return defaultName;
            }
        }

        return defaultName;
    }

    private void OnGenerateStart(GenerateTextStartEvent e)
    {
        var telemetry = SelectionOf(e.RecordInputs, e.RecordOutputs);
        var settings = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["maxOutputTokens"] = e.MaxOutputTokens,
            ["temperature"] = e.Temperature,
            ["topP"] = e.TopP,
            ["topK"] = e.TopK,
            ["presencePenalty"] = e.PresencePenalty,
            ["frequencyPenalty"] = e.FrequencyPenalty,
            ["stopSequences"] = e.StopSequences,
            ["seed"] = e.Seed,
            ["maxRetries"] = e.MaxRetries,
        };

        var providerName = GenAiConventions.MapProviderName(e.Provider);
        var operationName = GenAiConventions.MapOperationName(e.OperationId);
        var runtimeContext = e.RuntimeContext;
        var baseSupplemental = Merge(
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.RuntimeContext,
                SupplementalAttributeHelpers.GetRuntimeContextAttributes(runtimeContext)
            ),
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Headers,
                SupplementalAttributeHelpers.GetHeaderAttributes(e.Headers)
            )
        );

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = operationName,
                    ["gen_ai.provider.name"] = providerName,
                    ["gen_ai.request.model"] = e.ModelId,
                    ["gen_ai.agent.name"] = e.FunctionId,
                    ["gen_ai.request.frequency_penalty"] = e.FrequencyPenalty,
                    ["gen_ai.request.max_tokens"] = e.MaxOutputTokens,
                    ["gen_ai.request.presence_penalty"] = e.PresencePenalty,
                    ["gen_ai.request.temperature"] = e.Temperature,
                    ["gen_ai.request.top_k"] = e.TopK,
                    ["gen_ai.request.top_p"] = e.TopP,
                    ["gen_ai.request.stop_sequences"] = e.StopSequences?.ToArray(),
                    ["gen_ai.request.seed"] = e.Seed,
                }
            ),
            baseSupplemental
        );

        var spanName = GetAgentSpanName(operationName, e.ModelId, e.FunctionId);
        var parent = Activity.Current?.Context ?? default;
        var rootSpan = StartSpan(
            spanName,
            ActivityKind.Internal,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Operation,
                e.OperationId,
                e.CallId,
                runtimeContext
            ),
            parent
        );

        if (rootSpan is null)
        {
            return;
        }

        _callStates[e.CallId] = new CallState(e.OperationId, telemetry)
        {
            RootSpan = rootSpan,
            Settings = settings,
            Provider = e.Provider,
            ModelId = e.ModelId,
            RuntimeContext = runtimeContext,
            BaseSupplementalAttributes = baseSupplemental,
        };
    }

    private void OnObjectOperationStart(GenerateObjectStartEvent e)
    {
        var telemetry = SelectionOf(e.RecordInputs, e.RecordOutputs);
        var settings = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["maxOutputTokens"] = e.MaxOutputTokens,
            ["temperature"] = e.Temperature,
            ["topP"] = e.TopP,
            ["topK"] = e.TopK,
            ["presencePenalty"] = e.PresencePenalty,
            ["frequencyPenalty"] = e.FrequencyPenalty,
            ["seed"] = e.Seed,
            ["maxRetries"] = e.MaxRetries,
        };

        var providerName = GenAiConventions.MapProviderName(e.Provider);
        var operationName = GenAiConventions.MapOperationName(e.OperationId);
        var baseSupplemental = Merge(
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Headers,
                SupplementalAttributeHelpers.GetHeaderAttributes(e.Headers)
            )
        );

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = operationName,
                    ["gen_ai.provider.name"] = providerName,
                    ["gen_ai.request.model"] = e.ModelId,
                    ["gen_ai.agent.name"] = e.FunctionId,
                    ["gen_ai.output.type"] = "json",
                    ["gen_ai.request.frequency_penalty"] = e.FrequencyPenalty,
                    ["gen_ai.request.max_tokens"] = e.MaxOutputTokens,
                    ["gen_ai.request.presence_penalty"] = e.PresencePenalty,
                    ["gen_ai.request.temperature"] = e.Temperature,
                    ["gen_ai.request.top_k"] = e.TopK,
                    ["gen_ai.request.top_p"] = e.TopP,
                    ["gen_ai.request.seed"] = e.Seed,
                    ["gen_ai.system_instructions"] = e.System is null
                        ? null
                        : In(() => StringifyInstructions(e.System)),
                    ["gen_ai.input.messages"] = In(() =>
                        GenAiConventions.StringifySemConv(
                            GenAiConventions
                                .FormatModelMessages(e.Prompt, e.Messages)
                                .Select(message => message.ToJsonObject())
                        )
                    ),
                }
            ),
            baseSupplemental,
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Schema,
                new Dictionary<string, object?>
                {
                    ["ai.schema"] = e.Schema is { } schema ? In(() => schema.GetRawText()) : null,
                    ["ai.schema.name"] = e.SchemaName,
                    ["ai.schema.description"] = e.SchemaDescription,
                    ["ai.settings.output"] = e.Output,
                }
            )
        );

        var spanName = GetAgentSpanName(operationName, e.ModelId, e.FunctionId);
        var parent = Activity.Current?.Context ?? default;
        var rootSpan = StartSpan(
            spanName,
            ActivityKind.Internal,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Operation,
                e.OperationId,
                e.CallId,
                null
            ),
            parent
        );

        _callStates[e.CallId] = new CallState(e.OperationId, telemetry)
        {
            RootSpan = rootSpan,
            Settings = settings,
            Provider = e.Provider,
            ModelId = e.ModelId,
            BaseSupplementalAttributes = baseSupplemental,
            ObjectSystemInstructions = e.System,
        };
    }

    private static string StringifyInstructions(Instructions instructions) =>
        GenAiConventions.StringifySemConv(
            GenAiConventions
                .FormatSystemInstructions(instructions)
                .Select(part => part.ToJsonObject())
        );

    /// <summary>Maps to <c>onObjectStepStart</c>.</summary>
    private void OnObjectStepStartCore(GenerateObjectStepStartEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        var systemInstructionCount = state.ObjectSystemInstructions is null
            ? 0
            : GenAiConventions.FormatSystemInstructions(state.ObjectSystemInstructions).Count;
        var inputMessages = e.PromptMessages?.Skip(systemInstructionCount).ToList();

        var attributes = TelemetryAttributes.SelectAttributes(
            state.Telemetry,
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "chat",
                ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(e.Provider),
                ["gen_ai.request.model"] = e.ModelId,
                ["gen_ai.output.type"] = "json",
                ["gen_ai.request.frequency_penalty"] = Setting(state, "frequencyPenalty"),
                ["gen_ai.request.max_tokens"] = Setting(state, "maxOutputTokens"),
                ["gen_ai.request.presence_penalty"] = Setting(state, "presencePenalty"),
                ["gen_ai.request.temperature"] = Setting(state, "temperature"),
                ["gen_ai.request.top_k"] = Setting(state, "topK"),
                ["gen_ai.request.top_p"] = Setting(state, "topP"),
                ["gen_ai.system_instructions"] = state.ObjectSystemInstructions is null
                    ? null
                    : In(() => StringifyInstructions(state.ObjectSystemInstructions)),
                ["gen_ai.input.messages"] = In(() =>
                    inputMessages is null
                        ? null
                        : GenAiConventions.StringifySemConv(
                            GenAiConventions
                                .FormatInputMessages(inputMessages)
                                .Select(message => message.ToJsonObject())
                        )
                ),
            }
        );

        state.InferenceSpan = StartSpan(
            "chat " + e.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.LanguageModel,
                state.OperationId,
                e.CallId,
                state.RuntimeContext
            ),
            state.RootSpan.Context
        );
    }

    /// <summary>Maps to <c>onObjectStepEnd</c>.</summary>
    private void OnObjectStepEndCore(GenerateObjectStepEndEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.InferenceSpan is null)
        {
            return;
        }

        if (e.FinishReason == FinishReason.Error)
        {
            state.InferenceSpan.SetStatus(ActivityStatusCode.Error);
        }

        SetAttributes(
            state.InferenceSpan,
            Merge(
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    Merge(
                        ServerAttributes.GetServerAttributes(e.Request.Url),
                        new Dictionary<string, object?>
                        {
                            ["gen_ai.response.finish_reasons"] = new[]
                            {
                                GenAiConventions.FormatFinishReason(e.FinishReason),
                            },
                            ["gen_ai.response.id"] = e.Response.Id,
                            ["gen_ai.response.model"] = e.Response.ModelId,
                            ["gen_ai.usage.input_tokens"] = e.Usage.InputTokens,
                            ["gen_ai.usage.output_tokens"] = e.Usage.OutputTokens,
                            ["gen_ai.usage.cache_read.input_tokens"] = e.Usage.CacheReadTokens,
                            ["gen_ai.output.messages"] = Out(() =>
                                FormatObjectOutput(
                                    e.ObjectText,
                                    GenAiConventions.FormatFinishReason(e.FinishReason)
                                ) ?? e.ObjectText
                            ),
                        }
                    )
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ProviderMetadata,
                    ProviderMetadataAttributes(e.ProviderMetadata)
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.Usage,
                    DetailedUsageAttributes(e.Usage)
                )
            )
        );

        End(state.InferenceSpan);
        state.InferenceSpan = null;
    }

    private static string? FormatObjectOutput(string objectText, string finishReason) =>
        GenAiConventions.StringifySemConv(
            GenAiConventions
                .FormatObjectOutputMessages(objectText, finishReason)
                .Select(message => message.ToJsonObject())
        );

    /// <summary>Maps to <c>onObjectOperationEnd</c>.</summary>
    private void OnObjectOperationEnd(IGenerateObjectEndEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        if (e.FinishReason == FinishReason.Error)
        {
            state.RootSpan.SetStatus(ActivityStatusCode.Error);
        }

        SetAttributes(
            state.RootSpan,
            Merge(
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    new Dictionary<string, object?>
                    {
                        ["gen_ai.response.finish_reasons"] = new[]
                        {
                            GenAiConventions.FormatFinishReason(e.FinishReason),
                        },
                        ["gen_ai.usage.input_tokens"] = e.Usage.InputTokens,
                        ["gen_ai.usage.output_tokens"] = e.Usage.OutputTokens,
                        ["gen_ai.usage.cache_read.input_tokens"] = e.Usage.CacheReadTokens,
                        ["gen_ai.output.messages"] = Out(() =>
                            e.ObjectJson is { } json
                                ? FormatObjectOutput(
                                    json,
                                    GenAiConventions.FormatFinishReason(e.FinishReason)
                                )
                                : null
                        ),
                    }
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ProviderMetadata,
                    ProviderMetadataAttributes(e.ProviderMetadata)
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.Usage,
                    DetailedUsageAttributes(e.Usage)
                )
            )
        );

        End(state.RootSpan);
        _callStates.TryRemove(e.CallId, out _);
    }

    private void OnStepStartCore(GenerateTextStepStartEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        var stepAttributes = Merge(
            TelemetryAttributes.SelectAttributes(
                state.Telemetry,
                new Dictionary<string, object?> { ["gen_ai.operation.name"] = "agent_step" }
            ),
            state.BaseSupplementalAttributes,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.ToolChoice,
                new Dictionary<string, object?>
                {
                    ["ai.prompt.toolChoice"] = In(() =>
                        e.ToolChoice is { } toolChoice ? ToolChoiceJson(toolChoice) : null
                    ),
                }
            )
        );

        state.StepSpan = StartSpan(
            "step " + (e.StepNumber + 1),
            ActivityKind.Internal,
            GetSpanAttributes(
                stepAttributes,
                OpenTelemetrySpanType.Step,
                state.OperationId,
                e.CallId,
                state.RuntimeContext
            ),
            state.RootSpan.Context
        );
    }

    private void OnLanguageModelCallStartCore(LanguageModelCallStartEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.StepSpan is null)
        {
            return;
        }

        // A stream retry or a steering interrupt starts a new model call without ending the previous one.
        if (state.InferenceSpan is not null)
        {
            state.InferenceSpan.SetStatus(ActivityStatusCode.Error);
            End(state.InferenceSpan);
            state.InferenceSpan = null;
            state.InferenceContext = null;
        }

        RecordInferenceDuration(state, errorType: "_OTHER");
        state.InferenceMetrics = new InferenceMetrics(
            Stopwatch.GetTimestamp(),
            e.Provider,
            e.ModelId
        );

        var attributes = TelemetryAttributes.SelectAttributes(
            state.Telemetry,
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "chat",
                ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(e.Provider),
                ["gen_ai.request.model"] = e.ModelId,
                ["gen_ai.request.frequency_penalty"] = Setting(state, "frequencyPenalty"),
                ["gen_ai.request.max_tokens"] = Setting(state, "maxOutputTokens"),
                ["gen_ai.request.presence_penalty"] = Setting(state, "presencePenalty"),
                ["gen_ai.request.stop_sequences"] = (
                    Setting(state, "stopSequences") as IReadOnlyList<string>
                )?.ToArray(),
                ["gen_ai.request.temperature"] = Setting(state, "temperature"),
                ["gen_ai.request.top_k"] = Setting(state, "topK"),
                ["gen_ai.request.top_p"] = Setting(state, "topP"),
            }
        );

        state.InferenceSpan = StartSpan(
            "chat " + e.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.LanguageModel,
                state.OperationId,
                e.CallId,
                state.RuntimeContext
            ),
            state.StepSpan.Context
        );
        state.InferenceContext = state.InferenceSpan?.Context;
    }

    private void OnLanguageModelCallEndCore(LanguageModelCallEndEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.InferenceSpan is null)
        {
            return;
        }

        var span = state.InferenceSpan;
        RecordInferenceDuration(
            state,
            e.Performance?.ResponseTimeMs ?? e.ResponseTime.TotalMilliseconds,
            e.Provider,
            e.ModelId,
            e.FinishReason == FinishReason.Error ? "_OTHER" : null
        );
        if (e.FinishReason == FinishReason.Error)
        {
            span.SetStatus(ActivityStatusCode.Error);
        }

        SetAttributes(
            span,
            Merge(
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    Merge(
                        GetGenAIClientPerformanceAttributes(e.Performance, e.ResponseTime),
                        ServerAttributes.GetServerAttributes(e.Request?.Url)
                    )
                ),
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    new Dictionary<string, object?>
                    {
                        ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(e.Provider),
                        ["gen_ai.response.finish_reasons"] = new[]
                        {
                            GenAiConventions.FormatFinishReason(e.FinishReason),
                        },
                        ["gen_ai.response.id"] = e.ResponseId,
                        ["gen_ai.response.model"] = e.ModelId,
                        ["gen_ai.usage.input_tokens"] = e.Usage.InputTokens,
                        ["gen_ai.usage.output_tokens"] = e.Usage.OutputTokens,
                        ["gen_ai.usage.cache_read.input_tokens"] = e.Usage.CacheReadTokens,
                        ["gen_ai.usage.cache_creation.input_tokens"] = e.Usage.CacheWriteTokens,
                    }
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ProviderMetadata,
                    ProviderMetadataAttributes(e.ProviderMetadata)
                )
            )
        );

        RecordProviderExecutedToolSpans(state, e);

        End(span);
        state.InferenceSpan = null;
        state.InferenceContext = null;
    }

    /// <summary>Records the <c>execute_tool</c> spans of the provider-executed tools of a model call, which the model call reports itself.</summary>
    private void RecordProviderExecutedToolSpans(CallState state, LanguageModelCallEndEvent e)
    {
        if (state.InferenceContext is not { } inferenceContext)
        {
            return;
        }

        var finalToolOutputs = new Dictionary<string, GeneratedToolResult>(StringComparer.Ordinal);
        foreach (var part in e.Content)
        {
            // The port has no tool-error part, and no preliminary result: a failed result is a result with IsError.
            if (part is GeneratedToolResult result)
            {
                finalToolOutputs[result.ToolCallId] = result;
            }
        }

        var recordedToolCallIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var part in e.Content)
        {
            if (
                part is not GeneratedToolCall { ProviderExecuted: true } toolCall
                || !recordedToolCallIds.Add(toolCall.ToolCallId)
            )
            {
                continue;
            }

            var toolSpan = StartSpan(
                "execute_tool " + toolCall.ToolName,
                ActivityKind.Internal,
                GetSpanAttributes(
                    TelemetryAttributes.SelectAttributes(
                        state.Telemetry,
                        new Dictionary<string, object?>
                        {
                            ["gen_ai.operation.name"] = "execute_tool",
                            ["gen_ai.tool.name"] = toolCall.ToolName,
                            ["gen_ai.tool.call.id"] = toolCall.ToolCallId,
                            ["gen_ai.tool.type"] = "extension",
                            ["gen_ai.tool.call.arguments"] = In(() => toolCall.ArgumentsJson),
                        }
                    ),
                    OpenTelemetrySpanType.Tool,
                    state.OperationId,
                    e.CallId!,
                    state.RuntimeContext
                ),
                inferenceContext
            );
            if (toolSpan is null)
            {
                continue;
            }

            if (finalToolOutputs.TryGetValue(toolCall.ToolCallId, out var output))
            {
                if (!output.IsError)
                {
                    SetAttributes(
                        toolSpan,
                        TelemetryAttributes.SelectAttributes(
                            state.Telemetry,
                            new Dictionary<string, object?>
                            {
                                ["gen_ai.tool.call.result"] = Out(() => output.ResultJson),
                            }
                        )
                    );
                }
                else
                {
                    TelemetrySpan.RecordError(toolSpan, (object?)output.Error ?? "error");
                }
            }

            End(toolSpan);
        }
    }

    private void OnToolExecutionStartCore(ToolExecutionStartEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state))
        {
            return;
        }

        var parent = state.StepSpan ?? state.RootSpan;
        if (parent is null)
        {
            return;
        }

        var toolCall = e.ToolCall;
        var attributes = TelemetryAttributes.SelectAttributes(
            state.Telemetry,
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "execute_tool",
                ["gen_ai.tool.name"] = toolCall.ToolName,
                ["gen_ai.tool.call.id"] = toolCall.ToolCallId,
                ["gen_ai.tool.type"] = toolCall.ProviderExecuted ? "extension" : "function",
                ["gen_ai.tool.call.arguments"] = In(() => toolCall.ArgumentsJson),
            }
        );

        var span = StartSpan(
            "execute_tool " + toolCall.ToolName,
            ActivityKind.Internal,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Tool,
                state.OperationId,
                e.CallId!,
                state.RuntimeContext
            ),
            parent.Context
        );
        if (span is not null)
        {
            state.ToolSpans[toolCall.ToolCallId] = span;
        }
    }

    private void OnToolExecutionEndCore(ToolExecutionEndEvent e)
    {
        if (
            !TryGetCallState(e.CallId, out var state)
            || !state.ToolSpans.TryGetValue(e.ToolCall.ToolCallId, out var span)
        )
        {
            return;
        }

        SetAttributes(
            span,
            TelemetryAttributes.SelectAttributes(
                state.Telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.execute_tool.duration"] = e.ToolExecutionTime.TotalSeconds,
                }
            )
        );

        if (!e.ToolOutput.IsError)
        {
            SetAttributes(
                span,
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    new Dictionary<string, object?>
                    {
                        ["gen_ai.tool.call.result"] = Out(() => e.ToolOutput.OutputJson),
                    }
                )
            );
        }
        else
        {
            TelemetrySpan.RecordError(span, (object?)e.ToolOutput.Error ?? "error");
        }

        End(span);
        state.ToolSpans.Remove(e.ToolCall.ToolCallId);
    }

    private void OnStepEndCore(StepResult e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.StepSpan is null)
        {
            return;
        }

        // Streams can close after an error chunk without a model-call end event.
        RecordInferenceDuration(
            state,
            errorType: e.FinishReason == FinishReason.Error ? "_OTHER" : null
        );

        if (e.FinishReason == FinishReason.Error)
        {
            state.StepSpan.SetStatus(ActivityStatusCode.Error);
        }

        SetAttributes(
            state.StepSpan,
            Merge(
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ProviderMetadata,
                    ProviderMetadataAttributes(e.ProviderMetadata)
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.Usage,
                    DetailedUsageAttributes(e.Usage)
                )
            )
        );

        End(state.StepSpan);
        state.StepSpan = null;
    }

    private void OnStepErrorCore(GenerateTextStepErrorEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.StepSpan is null)
        {
            return;
        }

        if (state.InferenceSpan is not null)
        {
            TelemetrySpan.RecordError(state.InferenceSpan, e.Error);
            End(state.InferenceSpan);
            state.InferenceSpan = null;
        }

        TelemetrySpan.RecordError(state.StepSpan, e.Error);
        End(state.StepSpan);
        state.StepSpan = null;
    }

    private void OnGenerateEnd(GenerateTextResult e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        if (e.FinishReason == FinishReason.Error)
        {
            state.RootSpan.SetStatus(ActivityStatusCode.Error);
        }

        SetAttributes(
            state.RootSpan,
            Merge(
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    new Dictionary<string, object?>
                    {
                        ["gen_ai.response.finish_reasons"] = new[]
                        {
                            GenAiConventions.FormatFinishReason(e.FinishReason),
                        },
                        ["gen_ai.usage.input_tokens"] = e.TotalUsage.InputTokens,
                        ["gen_ai.usage.output_tokens"] = e.TotalUsage.OutputTokens,
                        ["gen_ai.usage.cache_read.input_tokens"] = e.TotalUsage.CacheReadTokens,
                        ["gen_ai.usage.cache_creation.input_tokens"] =
                            e.TotalUsage.CacheWriteTokens,
                    }
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ProviderMetadata,
                    ProviderMetadataAttributes(e.FinalStep.ProviderMetadata)
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.Usage,
                    DetailedUsageAttributes(e.TotalUsage)
                )
            )
        );

        End(state.RootSpan);
        _callStates.TryRemove(e.CallId, out _);
    }

    private void OnDecideOperationStart(DecideEvent e)
    {
        var telemetry = SelectionOf(e.RecordInputs, e.RecordOutputs);
        var baseSupplemental = Merge(
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.RuntimeContext,
                SupplementalAttributeHelpers.GetRuntimeContextAttributes(e.RuntimeContext)
            ),
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Headers,
                SupplementalAttributeHelpers.GetHeaderAttributes(
                    e.Headers?.Select(pair => new KeyValuePair<string, string?>(
                        pair.Key,
                        pair.Value
                    ))
                )
            )
        );
        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = "decide",
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(e.Provider),
                    ["gen_ai.request.model"] = e.ModelId,
                }
            ),
            baseSupplemental,
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.ExperimentalDecision,
                new Dictionary<string, object?>
                {
                    ["ai.decision.state"] = e.State is null
                        ? null
                        : In(() =>
                            StringifyForTelemetry.StringifyDecisionStateForTelemetry(e.State)
                        ),
                    ["ai.decision.questions"] = In(() =>
                        StringifyForTelemetry.StringifyQuestions(e.Questions)
                    ),
                }
            )
        );

        var rootSpan = StartSpan(
            "decide " + e.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Operation,
                e.OperationId,
                e.CallId,
                e.RuntimeContext
            ),
            Activity.Current?.Context ?? default
        );

        if (rootSpan is null)
        {
            return;
        }

        _callStates[e.CallId] = new CallState(e.OperationId, telemetry)
        {
            RootSpan = rootSpan,
            Settings = new Dictionary<string, object?> { ["maxRetries"] = e.MaxRetries },
            Provider = e.Provider,
            ModelId = e.ModelId,
            RuntimeContext = e.RuntimeContext,
            BaseSupplementalAttributes = baseSupplemental,
        };
    }

    private void OnDecideOperationEnd(DecideEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        SetAttributes(
            state.RootSpan,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.ExperimentalDecision,
                new Dictionary<string, object?>
                {
                    ["ai.decision.answers"] = e.Answers is null
                        ? null
                        : Out(() => StringifyForTelemetry.StringifyDecisionAnswers(e.Answers)),
                }
            )
        );
        End(state.RootSpan);
        _callStates.TryRemove(e.CallId, out _);
    }

    private void OnDecisionModelCallStartCore(DecisionModelEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                state.Telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = "decide",
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(state.Provider),
                    ["gen_ai.request.model"] = state.ModelId,
                }
            ),
            state.BaseSupplementalAttributes,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.ExperimentalDecision,
                new Dictionary<string, object?>
                {
                    ["ai.decision.state"] = e.State is null
                        ? null
                        : In(() => StringifyForTelemetry.StringifyDecisionModelState(e.State)),
                    ["ai.decision.questions"] = In(() =>
                        StringifyForTelemetry.StringifyQuestions(e.Questions)
                    ),
                }
            )
        );

        state.DecisionSpan = StartSpan(
            "decide " + state.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.ExperimentalDecision,
                e.OperationId,
                e.CallId,
                state.RuntimeContext
            ),
            state.RootSpan.Context
        );
    }

    private void OnDecisionModelCallEndCore(DecisionModelEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.DecisionSpan is null)
        {
            return;
        }

        SetAttributes(
            state.DecisionSpan,
            Merge(
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    new Dictionary<string, object?>
                    {
                        ["gen_ai.response.id"] = e.Response?.Id,
                        ["gen_ai.response.model"] = e.Response?.ModelId,
                        ["gen_ai.usage.input_tokens"] = e.Usage?.InputTokens,
                        ["gen_ai.usage.output_tokens"] = e.Usage?.OutputTokens,
                    }
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ProviderMetadata,
                    ProviderMetadataAttributes(e.ProviderMetadata)
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ExperimentalDecision,
                    new Dictionary<string, object?>
                    {
                        ["ai.decision.answers"] = e.Answers is null
                            ? null
                            : Out(() => StringifyForTelemetry.StringifyDecisionAnswers(e.Answers)),
                    }
                )
            )
        );
        End(state.DecisionSpan);
        state.DecisionSpan = null;
    }

    private void OnExtractStartCore(ExtractStartEvent e)
    {
        var telemetry = SelectionOf(e.RecordInputs, e.RecordOutputs);
        var baseSupplemental = Merge(
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.RuntimeContext,
                SupplementalAttributeHelpers.GetRuntimeContextAttributes(e.RuntimeContext)
            ),
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Headers,
                SupplementalAttributeHelpers.GetHeaderAttributes(
                    e.Headers?.Select(pair => new KeyValuePair<string, string?>(
                        pair.Key,
                        pair.Value
                    ))
                )
            )
        );
        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = "extract",
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(e.Provider),
                    ["gen_ai.request.model"] = e.ModelId,
                }
            ),
            baseSupplemental,
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.ExperimentalExtraction,
                new Dictionary<string, object?>
                {
                    ["ai.extraction.state"] = e.State is null
                        ? null
                        : In(() =>
                            StringifyForTelemetry.StringifyDecisionStateForTelemetry(e.State)
                        ),
                    ["ai.extraction.fields"] = In(() =>
                        StringifyForTelemetry.StringifyFields(e.Fields)
                    ),
                    ["ai.extraction.referenceDate"] = In(() => e.ReferenceDate),
                }
            )
        );

        var rootSpan = StartSpan(
            "extract " + e.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Operation,
                e.OperationId,
                e.CallId,
                e.RuntimeContext
            ),
            Activity.Current?.Context ?? default
        );

        if (rootSpan is null)
        {
            return;
        }

        _callStates[e.CallId] = new CallState(e.OperationId, telemetry)
        {
            RootSpan = rootSpan,
            Settings = new Dictionary<string, object?> { ["maxRetries"] = e.MaxRetries },
            Provider = e.Provider,
            ModelId = e.ModelId,
            RuntimeContext = e.RuntimeContext,
            BaseSupplementalAttributes = baseSupplemental,
        };
    }

    private void OnExtractEndCore(ExtractEndEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        SetAttributes(
            state.RootSpan,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.ExperimentalExtraction,
                new Dictionary<string, object?>
                {
                    ["ai.extraction.answers"] = Out(() =>
                        StringifyForTelemetry.StringifyExtractionAnswers(e.Answers)
                    ),
                }
            )
        );
        End(state.RootSpan);
        _callStates.TryRemove(e.CallId, out _);
    }

    private void OnExtractionModelCallStartCore(ExtractionModelCallStartEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                state.Telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = "extract",
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(state.Provider),
                    ["gen_ai.request.model"] = state.ModelId,
                }
            ),
            state.BaseSupplementalAttributes,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.ExperimentalExtraction,
                new Dictionary<string, object?>
                {
                    ["ai.extraction.state"] = In(() =>
                        StringifyForTelemetry.StringifyDecisionModelState(e.State)
                    ),
                    ["ai.extraction.fields"] = In(() =>
                        StringifyForTelemetry.StringifyFields(e.Fields)
                    ),
                    ["ai.extraction.referenceDate"] = In(() => e.ReferenceDate),
                }
            )
        );

        state.ExtractionSpan = StartSpan(
            "extract " + state.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.ExperimentalExtraction,
                e.OperationId,
                e.CallId,
                state.RuntimeContext
            ),
            state.RootSpan.Context
        );
    }

    private void OnExtractionModelCallEndCore(ExtractionModelCallEndEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.ExtractionSpan is null)
        {
            return;
        }

        SetAttributes(
            state.ExtractionSpan,
            Merge(
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    new Dictionary<string, object?>
                    {
                        ["gen_ai.response.id"] = e.Response?.Id,
                        ["gen_ai.response.model"] = e.Response?.ModelId,
                        ["gen_ai.usage.input_tokens"] = e.Usage?.InputTokens,
                        ["gen_ai.usage.output_tokens"] = e.Usage?.OutputTokens,
                    }
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ExperimentalExtraction,
                    new Dictionary<string, object?>
                    {
                        ["ai.extraction.answers"] = Out(() =>
                            StringifyForTelemetry.StringifyExtractionAnswers(e.Answers)
                        ),
                    }
                )
            )
        );
        End(state.ExtractionSpan);
        state.ExtractionSpan = null;
    }

    private void OnAudioOperationStart(
        string callId,
        string operationId,
        string provider,
        string modelId,
        bool? recordInputs,
        bool? recordOutputs,
        string? functionId,
        IReadOnlyDictionary<string, string>? headers,
        TranscriptionAudioInfo? audio,
        string? text
    )
    {
        var telemetry = SelectionOf(recordInputs, recordOutputs);
        var operationName = GenAiConventions.MapOperationName(operationId);
        var baseSupplemental = _supplementalAttributes.Select(
            telemetry,
            SupplementalGroup.Headers,
            SupplementalAttributeHelpers.GetHeaderAttributes(NullableValues(headers))
        );

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = operationName,
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(provider),
                    ["gen_ai.request.model"] = modelId,
                    ["gen_ai.agent.name"] = functionId,
                    ["gen_ai.output.type"] = operationId == "ai.generateSpeech" ? "speech" : "text",
                    ["gen_ai.request.stream"] = operationId == "ai.streamTranscribe",
                    ["gen_ai.input.messages"] = text is null
                        ? null
                        : In(() => TextMessages("user", text)),
                    ["ai.request.text"] = text is null ? null : In(() => text),
                    ["ai.request.audio.size"] = audio?.ByteLength is { } byteLength
                        ? In(() => byteLength)
                        : null,
                    ["ai.request.audio.media_type"] = audio is null
                        ? null
                        : In(() => audio.MediaType),
                }
            ),
            baseSupplemental
        );

        var rootSpan = StartSpan(
            operationName + " " + modelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Operation,
                operationId,
                callId,
                null
            ),
            Activity.Current?.Context ?? default
        );

        if (rootSpan is null)
        {
            return;
        }

        _callStates[callId] = new CallState(operationId, telemetry)
        {
            RootSpan = rootSpan,
            Provider = provider,
            ModelId = modelId,
            BaseSupplementalAttributes = baseSupplemental,
        };
    }

    private void OnAudioOperationEnd(
        string callId,
        TranscriptionAudioInfo? inputAudio,
        GenerateSpeechAudioInfo? outputAudio,
        string? transcript,
        System.Text.Json.JsonElement? usage,
        System.Text.Json.JsonElement? providerMetadata
    )
    {
        if (!TryGetCallState(callId, out var state) || state.RootSpan is null)
        {
            return;
        }

        var attributes = new Dictionary<string, object?>
        {
            ["ai.request.audio.size"] = inputAudio?.ByteLength is { } inputSize
                ? In(() => inputSize)
                : null,
            ["ai.request.audio.media_type"] = inputAudio is null
                ? null
                : In(() => inputAudio.MediaType),
            ["ai.response.text"] = transcript is null ? null : Out(() => transcript),
            ["gen_ai.output.messages"] = transcript is null
                ? null
                : Out(() => TextMessages("assistant", transcript)),
            ["ai.response.audio.size"] = outputAudio is null
                ? null
                : Out(() => outputAudio.ByteLength),
            ["ai.response.audio.media_type"] = outputAudio is null
                ? null
                : Out(() => outputAudio.MediaType),
            ["ai.response.audio.format"] = outputAudio is null
                ? null
                : Out(() => outputAudio.Format),
        };
        foreach (
            var pair in ProviderUsageAttributes.GetProviderUsageAttributes(usage, "gen_ai.usage")
        )
        {
            attributes[pair.Key] = pair.Value;
        }

        SetAttributes(
            state.RootSpan,
            Merge(
                TelemetryAttributes.SelectAttributes(state.Telemetry, attributes),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.Usage,
                    new Dictionary<string, object?>
                    {
                        ["ai.response.usage"] = usage is { } value ? value.GetRawText() : null,
                    }
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.ProviderMetadata,
                    ProviderMetadataAttributes(providerMetadata)
                )
            )
        );

        End(state.RootSpan);
        _callStates.TryRemove(callId, out _);
    }

    private static string TextMessages(string role, string content) =>
        System.Text.Json.JsonSerializer.Serialize(
            new[] { new { role, parts = new[] { new { type = "text", content } } } }
        );

    private static IEnumerable<KeyValuePair<string, string?>>? NullableValues(
        IEnumerable<KeyValuePair<string, string>>? headers
    ) => headers?.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value));

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        // A vector with NaN must not make the span handlers throw and leave spans open.
        NumberHandling = System
            .Text
            .Json
            .Serialization
            .JsonNumberHandling
            .AllowNamedFloatingPointLiterals,
    };

    private static string Json(object? value) =>
        System.Text.Json.JsonSerializer.Serialize(value, JsonOptions);

    private void OnEmbedOperationStart(EmbedStartEvent e)
    {
        var telemetry = SelectionOf(e.RecordInputs, e.RecordOutputs);
        var runtimeContext = e.RuntimeContext;
        var baseSupplemental = Merge(
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.RuntimeContext,
                SupplementalAttributeHelpers.GetRuntimeContextAttributes(runtimeContext)
            ),
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Headers,
                SupplementalAttributeHelpers.GetHeaderAttributes(NullableValues(e.Headers))
            )
        );
        var isMany = e.OperationId == "ai.embedMany";
        var operationName = GenAiConventions.MapOperationName(e.OperationId);

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = operationName,
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(e.Provider),
                    ["gen_ai.request.model"] = e.ModelId,
                }
            ),
            baseSupplemental,
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Embedding,
                isMany
                    ? new Dictionary<string, object?>
                    {
                        ["ai.values"] = In(() => (e.Value.Many ?? []).Select(Json).ToArray()),
                    }
                    : new Dictionary<string, object?> { ["ai.value"] = In(() => Json(e.Value.One)) }
            )
        );

        var rootSpan = StartSpan(
            operationName + " " + e.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Operation,
                e.OperationId,
                e.CallId,
                runtimeContext
            ),
            Activity.Current?.Context ?? default
        );

        if (rootSpan is null)
        {
            return;
        }

        _callStates[e.CallId] = new CallState(e.OperationId, telemetry)
        {
            RootSpan = rootSpan,
            Settings = new Dictionary<string, object?> { ["maxRetries"] = e.MaxRetries },
            Provider = e.Provider,
            ModelId = e.ModelId,
            RuntimeContext = runtimeContext,
            BaseSupplementalAttributes = baseSupplemental,
        };
    }

    private void OnEmbedOperationEnd(EmbedEndEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        var isMany = state.OperationId == "ai.embedMany";
        SetAttributes(
            state.RootSpan,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.Embedding,
                isMany
                    ? new Dictionary<string, object?>
                    {
                        ["ai.embeddings"] = Out(() =>
                            (e.Embedding.Many ?? []).Select(Json).ToArray()
                        ),
                    }
                    : new Dictionary<string, object?>
                    {
                        ["ai.embedding"] = Out(() => Json(e.Embedding.One)),
                    }
            )
        );

        End(state.RootSpan);
        _callStates.TryRemove(e.CallId, out _);
    }

    private void OnEmbedStartCore(EmbeddingModelCallStartEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                state.Telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = "embeddings",
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(state.Provider),
                    ["gen_ai.request.model"] = state.ModelId,
                }
            ),
            state.BaseSupplementalAttributes,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.Embedding,
                new Dictionary<string, object?>
                {
                    ["ai.values"] = In(() => e.Values.Select(Json).ToArray()),
                }
            )
        );

        var embedSpan = StartSpan(
            "embeddings " + state.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Embedding,
                state.OperationId,
                e.CallId,
                state.RuntimeContext
            ),
            state.RootSpan.Context
        );
        if (embedSpan is not null)
        {
            state.EmbedSpans[e.EmbedCallId] = embedSpan;
        }
    }

    private void OnEmbedEndCore(EmbeddingModelCallEndEvent e)
    {
        if (
            !TryGetCallState(e.CallId, out var state)
            || !state.EmbedSpans.TryGetValue(e.EmbedCallId, out var span)
        )
        {
            return;
        }

        SetAttributes(
            span,
            Merge(
                TelemetryAttributes.SelectAttributes(
                    state.Telemetry,
                    new Dictionary<string, object?>
                    {
                        ["gen_ai.usage.input_tokens"] = e.Usage.Tokens,
                    }
                ),
                _supplementalAttributes.Select(
                    state.Telemetry,
                    SupplementalGroup.Embedding,
                    new Dictionary<string, object?>
                    {
                        ["ai.embeddings"] = Out(() => e.Embeddings.Select(Json).ToArray()),
                    }
                )
            )
        );

        End(span);
        state.EmbedSpans.TryRemove(e.EmbedCallId, out _);
    }

    private void OnRerankOperationStart(RerankStartEvent e)
    {
        var telemetry = SelectionOf(e.RecordInputs, e.RecordOutputs);
        var runtimeContext = e.RuntimeContext;
        var baseSupplemental = Merge(
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.RuntimeContext,
                SupplementalAttributeHelpers.GetRuntimeContextAttributes(runtimeContext)
            ),
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Headers,
                SupplementalAttributeHelpers.GetHeaderAttributes(NullableValues(e.Headers))
            )
        );

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = "rerank",
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(e.Provider),
                    ["gen_ai.request.model"] = e.ModelId,
                }
            ),
            baseSupplemental,
            _supplementalAttributes.Select(
                telemetry,
                SupplementalGroup.Reranking,
                new Dictionary<string, object?>
                {
                    ["ai.documents"] = In(() => e.Documents.Select(Json).ToArray()),
                }
            )
        );

        var rootSpan = StartSpan(
            "rerank " + e.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Operation,
                e.OperationId,
                e.CallId,
                runtimeContext
            ),
            Activity.Current?.Context ?? default
        );

        if (rootSpan is null)
        {
            return;
        }

        _callStates[e.CallId] = new CallState(e.OperationId, telemetry)
        {
            RootSpan = rootSpan,
            Settings = new Dictionary<string, object?> { ["maxRetries"] = e.MaxRetries },
            Provider = e.Provider,
            ModelId = e.ModelId,
            RuntimeContext = runtimeContext,
            BaseSupplementalAttributes = baseSupplemental,
        };
    }

    private void OnRerankOperationEnd(RerankEndEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        End(state.RootSpan);
        _callStates.TryRemove(e.CallId, out _);
    }

    private void OnRerankStartCore(RerankingModelCallStartEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        var attributes = Merge(
            TelemetryAttributes.SelectAttributes(
                state.Telemetry,
                new Dictionary<string, object?>
                {
                    ["gen_ai.operation.name"] = "rerank",
                    ["gen_ai.provider.name"] = GenAiConventions.MapProviderName(state.Provider),
                    ["gen_ai.request.model"] = state.ModelId,
                }
            ),
            state.BaseSupplementalAttributes,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.Reranking,
                new Dictionary<string, object?>
                {
                    ["ai.documents"] = In(() => e.Documents.Select(Json).ToArray()),
                }
            )
        );

        state.RerankSpan = StartSpan(
            "rerank " + state.ModelId,
            ActivityKind.Client,
            GetSpanAttributes(
                attributes,
                OpenTelemetrySpanType.Reranking,
                state.OperationId,
                e.CallId,
                state.RuntimeContext
            ),
            state.RootSpan.Context
        );
    }

    private void OnRerankEndCore(RerankingModelCallEndEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RerankSpan is null)
        {
            return;
        }

        SetAttributes(
            state.RerankSpan,
            _supplementalAttributes.Select(
                state.Telemetry,
                SupplementalGroup.Reranking,
                new Dictionary<string, object?>
                {
                    ["ai.ranking.type"] = e.DocumentsType,
                    ["ai.ranking"] = Out(() =>
                        e
                            .Ranking.Select(r =>
                                Json(new { index = r.Index, relevanceScore = r.RelevanceScore })
                            )
                            .ToArray()
                    ),
                }
            )
        );

        End(state.RerankSpan);
        state.RerankSpan = null;
    }

    private void OnAbortCore(StreamAbortContext e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        RecordInferenceDuration(state, errorType: "AbortError");
        EndOpenSpans(state, null);
        End(state.RootSpan);
        _callStates.TryRemove(e.CallId, out _);
    }

    private void OnErrorCore(TelemetryErrorEvent e)
    {
        if (!TryGetCallState(e.CallId, out var state) || state.RootSpan is null)
        {
            return;
        }

        RecordInferenceDuration(
            state,
            errorType: e.Error switch
            {
                OperationCanceledException => "AbortError",
                Exception error => error.GetType().Name,
                _ => "_OTHER",
            }
        );
        EndOpenSpans(state, e.Error);
        TelemetrySpan.RecordError(state.RootSpan, e.Error);
        End(state.RootSpan);
        _callStates.TryRemove(e.CallId!, out _);
    }

    /// <summary>Ends every span below the root, recording <paramref name="error"/> on each when it is set.</summary>
    private static void EndOpenSpans(CallState state, Exception? error)
    {
        void Finish(Activity? span)
        {
            if (span is null)
            {
                return;
            }

            if (error is not null)
            {
                TelemetrySpan.RecordError(span, error);
            }

            End(span);
        }

        foreach (var toolSpan in state.ToolSpans.Values)
        {
            Finish(toolSpan);
        }

        state.ToolSpans.Clear();
        Finish(state.InferenceSpan);
        state.InferenceSpan = null;
        state.InferenceContext = null;
        Finish(state.StepSpan);
        state.StepSpan = null;
        foreach (var embedSpan in state.EmbedSpans.Values)
        {
            Finish(embedSpan);
        }

        state.EmbedSpans.Clear();
        Finish(state.RerankSpan);
        state.RerankSpan = null;
        Finish(state.DecisionSpan);
        state.DecisionSpan = null;
        Finish(state.ExtractionSpan);
        state.ExtractionSpan = null;
    }

    private static void SetAttributes(
        Activity span,
        IReadOnlyDictionary<string, object?> attributes
    )
    {
        foreach (var pair in attributes)
        {
            span.SetTag(pair.Key, pair.Value);
        }
    }

    private static Dictionary<string, object?> ProviderMetadataAttributes(
        System.Text.Json.JsonElement? providerMetadata
    ) =>
        new()
        {
            ["ai.response.providerMetadata"] = providerMetadata
                is { ValueKind: not System.Text.Json.JsonValueKind.Undefined } value
                ? value.GetRawText()
                : null,
        };

    private static Dictionary<string, object?> DetailedUsageAttributes(LanguageModelUsage usage) =>
        new()
        {
            ["ai.usage.inputTokenDetails.noCacheTokens"] = usage.NoCacheInputTokens,
            ["ai.usage.outputTokenDetails.textTokens"] = usage.TextTokens,
            ["ai.usage.outputTokenDetails.reasoningTokens"] = usage.ReasoningTokens,
        };

    private sealed record InferenceMetrics(long StartTimestamp, string Provider, string ModelId);

    private sealed class CallState(string operationId, TelemetrySelection telemetry)
    {
        public string OperationId { get; } = operationId;

        public TelemetrySelection Telemetry { get; } = telemetry;

        public Activity? RootSpan { get; init; }

        public Activity? StepSpan { get; set; }

        public Activity? InferenceSpan { get; set; }

        public ActivityContext? InferenceContext { get; set; }

        public InferenceMetrics? InferenceMetrics { get; set; }
        public ConcurrentDictionary<string, Activity> EmbedSpans { get; } =
            new(StringComparer.Ordinal);

        public Activity? RerankSpan { get; set; }

        public Activity? DecisionSpan { get; set; }

        public Activity? ExtractionSpan { get; set; }

        public Dictionary<string, Activity> ToolSpans { get; } = new(StringComparer.Ordinal);

        public Dictionary<string, object?> Settings { get; init; } = [];

        public string Provider { get; init; } = string.Empty;

        public string ModelId { get; init; } = string.Empty;

        public IReadOnlyDictionary<string, object?>? RuntimeContext { get; init; }

        public Instructions? ObjectSystemInstructions { get; init; }

        public IReadOnlyDictionary<string, object?> BaseSupplementalAttributes { get; init; } =
            new Dictionary<string, object?>();
    }
}
